"""Tests use authoring_reference receipts in temporary files, never Unity evidence."""
import copy
import tempfile
import unittest
from pathlib import Path

import adapter_conformance as conformance


class AdapterHandoffTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix="riftbound-conformance-")
        self.addCleanup(self.temporary.cleanup)
        self.bundle_path = Path(self.temporary.name) / "bundle.json"
        self.receipt_path = Path(self.temporary.name) / "model-receipt.json"
        self.bundle = conformance.make_bundle()
        self.bundle_path.write_bytes(conformance.encode(self.bundle))
        self.receipt = {
            "formatVersion": 1,
            "bundleSha256": conformance.sha(self.bundle_path.read_bytes()),
            "identities": dict(self.bundle["identities"]),
            "producer": {"kind": "authoring_reference", "sourceCommit": "8ad9071820c2fbed32fbb728122ffb2f02980888", "entryPoint": "AuthoringReference.TestOnly"},
            "results": [{"caseId": c["caseId"], **c["expected"]} for c in self.bundle["cases"]],
        }

    def compare(self, receipt=None):
        self.receipt_path.write_bytes(conformance.encode(self.receipt if receipt is None else receipt))
        return conformance.compare_results(self.bundle_path, self.receipt_path)

    def rejects(self, mutate):
        receipt = copy.deepcopy(self.receipt)
        mutate(receipt)
        with self.assertRaises(ValueError):
            self.compare(receipt)

    def test_exported_case_set_and_no_profile_id_input_bypass(self):
        self.assertEqual(len(self.bundle["cases"]), 70)
        self.assertTrue(all("profileId" not in c["actor"] for c in self.bundle["cases"]))
        unknown = next(c for c in self.bundle["cases"] if c["caseId"] == "abe-unknown-not-false")
        self.assertNotIn("main.ready", {v["field"] for v in unknown["knownValues"]})

    def test_model_receipt_never_attests_runtime_or_acceptance(self):
        report = self.compare()
        self.assertEqual(report["caseCount"], 70)
        self.assertFalse(report["independentRuntimeExecutionVerified"])
        self.assertFalse(report["schemaAcceptanceGranted"])
        self.assertFalse(report["runtimePromotionAuthorized"])

    def test_claimed_unity_producer_still_not_independently_attested(self):
        self.receipt["producer"]["kind"] = "unity_adapter"
        self.assertFalse(self.compare()["independentRuntimeExecutionVerified"])

    def test_result_order_does_not_change_case_identity(self):
        self.receipt["results"].reverse()
        self.assertEqual(self.compare()["receiptComparison"], "PASS")

    def test_hostile_identity_does_not_depend_on_display_name(self):
        profiles = {p["profileId"]: p for p in conformance.story.load_json(conformance.story.CATALOG_PATH)["profiles"]}
        renamed = {"actorKind": "hostile_enemy", "legacyName": "Completely different display", "enemyId": "westbank-reaver-captain"}
        self.assertEqual(conformance.bind_actor(renamed, profiles), "selvek")
        renamed["enemyId"] = "another-enemy"
        renamed["legacyName"] = "Captain Selvek"
        self.assertEqual(conformance.bind_actor(renamed, profiles), "")

    def test_extra_identity_expectations_follow_the_actor_not_case_id(self):
        profiles = {p["profileId"]: p for p in conformance.story.load_json(conformance.story.CATALOG_PATH)["profiles"]}
        for case in self.bundle["cases"]:
            self.assertEqual(conformance.bind_actor(case["actor"], profiles), case["expected"]["resolvedProfileId"])

    def test_missing_case_is_not_a_partial_pass(self):
        self.rejects(lambda r: r["results"].pop())

    def test_duplicate_case_refused(self):
        self.rejects(lambda r: r["results"].append(copy.deepcopy(r["results"][0])))

    def test_unknown_case_refused(self):
        self.rejects(lambda r: r["results"][0].update(caseId="invented"))

    def test_wrong_line_is_not_accepted(self):
        self.rejects(lambda r: r["results"][0].update(lineId="abe.main0.ready"))

    def test_wrong_resolved_actor_identity_refused(self):
        self.rejects(lambda r: r["results"][0].update(resolvedProfileId="rowan"))

    def test_nonline_cannot_report_successful_line(self):
        self.rejects(lambda r: r["results"][0].update(resolution="existing"))

    def test_wrong_schema_receipt_refused(self):
        self.rejects(lambda r: r["identities"].update(schemaSha256="0" * 64))

    def test_wrong_bundle_hash_refused(self):
        self.rejects(lambda r: r.update(bundleSha256="0" * 64))

    def test_invented_approval_field_is_not_accepted(self):
        self.rejects(lambda r: r.update(adapterAccepted=True))

    def test_inexact_producer_commit_refused(self):
        self.rejects(lambda r: r["producer"].update(sourceCommit="latest"))

    def test_bad_producer_and_resolution_types_fail_cleanly(self):
        self.rejects(lambda r: r["producer"].update(kind=[]))
        self.rejects(lambda r: r["results"][0].update(resolution=[]))

    def test_changed_context_bundle_is_not_checked_against_old_expectations(self):
        changed = copy.deepcopy(self.bundle)
        changed["cases"][0]["knownValues"].pop()
        self.bundle_path.write_bytes(conformance.encode(changed))
        with self.assertRaises(ValueError):
            conformance.check_bundle(self.bundle_path)

    def test_export_refuses_overwriting_unrelated_bytes(self):
        self.bundle_path.write_bytes(b"do not overwrite")
        with self.assertRaises(ValueError):
            conformance.write_bundle(self.bundle_path)
        self.assertEqual(self.bundle_path.read_bytes(), b"do not overwrite")

    def test_export_is_repeatable_for_identical_bytes(self):
        report = conformance.write_bundle(self.bundle_path)
        self.assertEqual(report["caseCount"], 70)
        self.assertEqual(self.bundle_path.read_bytes(), conformance.encode(self.bundle))


if __name__ == "__main__":
    unittest.main()
