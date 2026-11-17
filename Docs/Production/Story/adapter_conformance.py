"""Portable adapter test handoff and receipt comparison, not runtime attestation."""
import argparse
import hashlib
import json
import re
from pathlib import Path

import validate_story_pack as story


HERE = Path(__file__).resolve().parent
DEFAULT_BUNDLE = HERE / "adapter-fixtures.v1.json"
IDENTITY_FILES = {
    "contractSha256": "CATALOG_CONTRACT_V1.md",
    "schemaSha256": "first-five.schema.json",
    "catalogSha256": "first-five.v1.proposed.json",
    "fixturesSha256": "selection-fixtures.json",
}
RESOLUTIONS = {"line", "fallback", "existing", "none", "denied"}


def require(test, message):
    if not test:
        raise ValueError(message)


def exact_keys(value, keys, label):
    require(type(value) is dict and set(value) == set(keys), f"incorrect fields: {label}")


def sha(payload):
    return hashlib.sha256(payload).hexdigest()


def encode(value):
    return (json.dumps(value, indent=2, ensure_ascii=False) + "\n").encode("utf-8")


def frozen_identities():
    manifest = story.load_json(HERE / "schema-proposal-identity.json")
    require(manifest["proposalVersion"] == 1, "unknown proposal version")
    recorded = {row["path"]: row["sha256"] for row in manifest["artifacts"]}
    require(len(recorded) == len(IDENTITY_FILES), "unexpected frozen artifact set")
    result = {}
    for field, name in IDENTITY_FILES.items():
        relative = "Docs/Production/Story/" + name
        payload = (HERE / name).read_bytes()
        digest = sha(payload)
        require(b"\r\n" not in payload, f"artifact must use frozen LF bytes: {name}")
        require(recorded.get(relative) == digest, f"frozen artifact changed: {name}")
        result[field] = digest
    return result


def bind_actor(actor, profiles):
    """Classify first-five component identity; this never grants interaction permission."""
    for profile in profiles.values():
        if actor["actorKind"] != profile["actorKind"]:
            continue
        if actor["actorKind"] == "hostile_enemy":
            if actor["enemyId"] == profile["enemyId"]:
                return profile["profileId"]
        elif actor["enemyId"] == "" and actor["legacyName"].casefold() == profile["legacyName"].casefold():
            return profile["profileId"]
    return ""


def known_values(state, schema):
    known = []
    for key, value in sorted(state.items()):
        field, _, identity = key.partition(":")
        expected_type = int if field == "main.index" else bool
        require(type(value) is expected_type, "scenario state has an incorrect primitive type")
        predicate = {"field": field, "key": identity, "boolValue": value if type(value) is bool else False,
                     "intValue": value if type(value) is int else 0}
        story.check_shape(predicate, schema["$defs"]["condition"], schema)
        known.append(predicate)
    return known


def identity_cases(schema):
    """Extra transport cases; the whole bundle hash covers these, not the frozen62 hash."""
    cases = []

    def add(case_id, kind, name, enemy, channel, state, resolution, profile, line="", topic="", eligible=True):
        cases.append({"caseId": case_id, "actor": {"actorKind": kind, "legacyName": name, "enemyId": enemy},
                      "channel": channel, "topicId": topic, "eligible": eligible, "knownValues": known_values(state, schema),
                      "expected": {"resolution": resolution, "lineId": line, "resolvedProfileId": profile}})

    offer = {"main.index": 0, "main.accepted": False, "main.ready": False, "main.complete": False}
    alive = {"enemy.alive:westbank-reaver-captain": True}
    add("identity-abe-case", "main_quest_giver", "aBE", "", "main_body", offer, "line", "abe", "abe.main0.offer")
    add("identity-rowan-case", "merchant", "QUARTERMASTER ROWAN", "", "greeting", {}, "line", "rowan", "rowan.greeting.public")
    add("identity-mira-wrong-kind", "faction_quest_giver", "Provisioner Mira", "", "greeting", {}, "fallback", "")
    add("identity-rowan-suffix", "merchant", "Quartermaster Rowan Jr.", "", "greeting", {}, "fallback", "")
    add("identity-selvek-renamed", "hostile_enemy", "Renamed encounter object", "westbank-reaver-captain", "encounter_bark", alive, "line", "selvek", "selvek.encounter.toll", "aggro")
    add("identity-selvek-wrong-id", "hostile_enemy", "Captain Selvek", "not-the-reaver-captain", "encounter_bark", alive, "none", "", topic="aggro")
    add("identity-unknown-ineligible", "unknown", "Unknown actor", "", "greeting", {}, "denied", "", eligible=False)
    add("identity-selvek-renamed-no-talk", "hostile_enemy", "Renamed encounter object", "westbank-reaver-captain", "greeting", alive, "denied", "selvek")
    return cases


def make_bundle():
    identities = frozen_identities()
    catalog = story.load_json(story.CATALOG_PATH)
    story.validate_catalog(catalog)
    story.run_fixtures(catalog)
    profiles = {p["profileId"]: p for p in catalog["profiles"]}
    schema = story.load_json(story.SCHEMA_PATH)
    cases = []
    for case in story.load_json(story.FIXTURES_PATH):
        exact_keys(case, {"caseId", "profileId", "channel", "topicId", "state", "eligible", "expected"}, "authoring scenario")
        require(type(case["eligible"]) is bool, "scenario eligibility must be boolean")
        profile = profiles.get(case["profileId"])
        actor = {"actorKind": "unknown", "legacyName": "Unknown actor", "enemyId": ""}
        if profile:
            actor = {key: profile[key] for key in actor}
        known = known_values(case["state"], schema)
        selected = case["expected"]
        resolution = selected[1:] if selected.startswith("$") else "line"
        require(resolution in RESOLUTIONS, "unknown expected resolution")
        cases.append({"caseId": case["caseId"], "actor": actor, "channel": case["channel"],
                      "topicId": case["topicId"], "eligible": case["eligible"], "knownValues": known,
                      "expected": {"resolution": resolution, "lineId": selected if resolution == "line" else "",
                                   "resolvedProfileId": bind_actor(actor, profiles)}})
    cases.extend(identity_cases(schema))
    require(len({c["caseId"] for c in cases}) == len(cases), "duplicate exported case identity")
    return {"formatVersion": 1, "catalogId": catalog["catalogId"], "identities": identities, "cases": cases}


def write_bundle(path):
    bundle = make_bundle()
    payload = encode(bundle)
    if path.exists():
        require(path.read_bytes() == payload, "existing bundle differs; review it or use a new output path")
    else:
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(payload)
    return {"caseCount": len(bundle["cases"]), "bundleSha256": sha(payload),
            "scope": "exported authoring fixtures; no runtime adapter was executed"}


def check_bundle(path):
    actual = story.load_json(path)
    require(actual == make_bundle(), "bundle differs from frozen source scenarios; regenerate/review deliberately")
    require(path.read_bytes() == encode(actual), "bundle bytes must use canonical UTF-8 LF formatting")
    return actual


def compare_results(bundle_path, result_path):
    bundle = check_bundle(bundle_path)
    receipt = story.load_json(result_path)
    exact_keys(receipt, {"formatVersion", "bundleSha256", "identities", "producer", "results"}, "receipt")
    require(type(receipt["formatVersion"]) is int and receipt["formatVersion"] == 1, "unknown result format")
    require(receipt["bundleSha256"] == sha(bundle_path.read_bytes()), "wrong receipt bundle identity")
    require(receipt["identities"] == bundle["identities"], "wrong result catalog/schema/contract/fixture identity")
    producer = receipt["producer"]
    exact_keys(producer, {"kind", "sourceCommit", "entryPoint"}, "producer")
    require(type(producer["kind"]) is str and producer["kind"] in {"unity_adapter", "authoring_reference"}, "unknown producer kind")
    require(type(producer["sourceCommit"]) is str and re.fullmatch(r"[0-9a-fA-F]{40}", producer["sourceCommit"]), "missing exact producer source commit")
    require(type(producer["entryPoint"]) is str and re.fullmatch(r"[A-Za-z_][A-Za-z0-9_.:-]{0,199}", producer["entryPoint"]), "invalid producer entry point")
    require(type(receipt["results"]) is list, "results must be an array")
    expected = {case["caseId"]: case["expected"] for case in bundle["cases"]}
    seen = set()
    for result in receipt["results"]:
        exact_keys(result, {"caseId", "resolution", "lineId", "resolvedProfileId"}, "case result")
        case_id = result["caseId"]
        require(type(case_id) is str and case_id in expected, "unknown result case")
        require(case_id not in seen, "duplicate result case")
        seen.add(case_id)
        require(type(result["resolution"]) is str and result["resolution"] in RESOLUTIONS, f"unknown resolution: {case_id}")
        require(type(result["lineId"]) is str and type(result["resolvedProfileId"]) is str, "result identifiers must be strings")
        require((result["resolution"] == "line") == bool(result["lineId"]), f"line/nonline result mismatch: {case_id}")
        actual = {key: result[key] for key in ("resolution", "lineId", "resolvedProfileId")}
        require(actual == expected[case_id], f"adapter/reference mismatch: {case_id}")
    require(seen == set(expected), "receipt is incomplete")
    return {"receiptComparison": "PASS", "caseCount": len(seen), "producerKind": producer["kind"],
            "producerSourceCommit": producer["sourceCommit"], "independentRuntimeExecutionVerified": False,
            "schemaAcceptanceGranted": False, "runtimePromotionAuthorized": False,
            "scope": "receipt comparison only; execution provenance and owner acceptance require separate evidence"}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="command", required=True)
    export = commands.add_parser("export")
    export.add_argument("--output", type=Path, default=DEFAULT_BUNDLE)
    check = commands.add_parser("check")
    check.add_argument("--bundle", type=Path, default=DEFAULT_BUNDLE)
    compare = commands.add_parser("compare")
    compare.add_argument("--bundle", type=Path, default=DEFAULT_BUNDLE)
    compare.add_argument("--results", type=Path, required=True)
    args = parser.parse_args()
    if args.command == "export":
        report = write_bundle(args.output)
    elif args.command == "check":
        bundle = check_bundle(args.bundle)
        report = {"caseCount": len(bundle["cases"]), "bundleSha256": sha(args.bundle.read_bytes()),
                  "scope": "authoring fixture identity check; NOT runtime execution"}
    else:
        report = compare_results(args.bundle, args.results)
    print(json.dumps(report))


if __name__ == "__main__":
    main()
