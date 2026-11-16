"""Authoring-validator rejection/regression tests. These are not Unity tests."""
import copy
import tempfile
import unittest
from pathlib import Path

import validate_story_pack as v


class StoryAuthoringTests(unittest.TestCase):
    def setUp(self):
        self.catalog = v.load_json(v.CATALOG_PATH)

    def rejects(self, change):
        candidate = copy.deepcopy(self.catalog)
        change(candidate)
        with self.assertRaises(ValueError):
            v.validate_catalog(candidate)

    def test_valid_and_all_state_fixtures(self):
        self.assertEqual(v.validate_catalog(self.catalog)["profiles"], 5)
        self.assertGreaterEqual(v.run_fixtures(self.catalog), 50)

    def test_missing_required_field(self):
        self.rejects(lambda d: d.pop("schemaVersion"))

    def test_bool_is_not_integer_version(self):
        self.rejects(lambda d: d.update(schemaVersion=True))

    def test_duplicate_profile(self):
        self.rejects(lambda d: d["profiles"].__setitem__(1, copy.deepcopy(d["profiles"][0])))

    def test_identity_cannot_be_cosmetically_renamed(self):
        self.rejects(lambda d: d["profiles"][0].update(legacyName="Old Abe"))

    def test_wrong_actor_kind(self):
        self.rejects(lambda d: d["profiles"][1].update(actorKind="faction_quest_giver"))

    def test_duplicate_line(self):
        self.rejects(lambda d: d["profiles"][0]["entries"].append(copy.deepcopy(d["profiles"][0]["entries"][0])))

    def test_unknown_executable_predicate(self):
        self.rejects(lambda d: d["profiles"][0]["entries"][1]["conditions"][0].update(field="eval"))

    def test_unknown_quest_reference(self):
        self.rejects(lambda d: d["profiles"][3]["entries"][1]["conditions"][0].update(key="frontier_cartograpy"))

    def test_duplicate_predicate(self):
        self.rejects(lambda d: d["profiles"][0]["entries"][1]["conditions"].append(copy.deepcopy(d["profiles"][0]["entries"][1]["conditions"][0])))

    def test_main_completion_cannot_disagree_with_index(self):
        self.rejects(lambda d: d["profiles"][0]["entries"][2]["conditions"][0].update(intValue=3))

    def test_private_condition_cannot_be_public(self):
        self.rejects(lambda d: d["profiles"][0]["entries"][1].update(scope="public"))

    def test_quest_cannot_use_host_encounter_scope(self):
        self.rejects(lambda d: d["profiles"][3]["entries"][1].update(scope="host_encounter"))

    def test_reward_side_effect_refused(self):
        self.rejects(lambda d: d["profiles"][0]["entries"][0].update(rewardGold=100))

    def test_selvek_cannot_gain_friendly_dialogue(self):
        self.rejects(lambda d: d["profiles"][4]["entries"][0].update(channel="greeting", scope="public"))

    def test_withheld_reveal_refused(self):
        self.rejects(lambda d: d["profiles"][0]["entries"][0].update(text="Mara was my sister."))

    def test_rich_text_fallback_refused(self):
        self.rejects(lambda d: d["profiles"][1].update(fallbackText="<b>Trade now</b>"))

    def test_equal_priority_overlap_refused(self):
        def change(d):
            line = copy.deepcopy(d["profiles"][0]["entries"][0])
            line["lineId"] = "abe.greeting.ambiguous"
            d["profiles"][0]["entries"].append(line)
        self.rejects(change)

    def test_main_title_identity_preserved(self):
        self.rejects(lambda d: d["profiles"][0]["entries"][2].update(text="A New Mandatory Story"))

    def test_unused_typed_value_must_be_default(self):
        self.rejects(lambda d: d["profiles"][0]["entries"][1]["conditions"][0].update(intValue=2))

    def test_unknown_false_is_not_known_false(self):
        state = {"main.index": 0, "main.complete": False, "main.accepted": True}
        self.assertEqual(v.select_line(self.catalog, "abe", "main_body", "", state), "$existing")

    def test_host_history_is_not_a_guest_context(self):
        host = {"quest.completed:river_reaver_bounty": True}
        self.assertEqual(v.select_line(self.catalog, "elara", "quest_after", "river_reaver_bounty", host), "elara.reavers.after")
        self.assertEqual(v.select_line(self.catalog, "elara", "quest_after", "river_reaver_bounty", {}), "$existing")

    def test_faction_quest_state_contradiction(self):
        self.assertFalse(v.consistent({"quest.active:frontier_cartography": True, "quest.completed:frontier_cartography": True}))
        self.assertFalse(v.consistent({"quest.active:frontier_cartography": True, "quest.active:river_reaver_bounty": True}))

    def test_duplicate_json_property_and_nonfinite_rejected(self):
        with tempfile.TemporaryDirectory(prefix="riftbound-story-validator-") as folder:
            path = Path(folder) / "invalid.json"
            for text in ('{"a":1,"a":2}', '{"a":NaN}', '{"a":Infinity}'):
                path.write_text(text, encoding="utf-8")
                with self.assertRaises(ValueError):
                    v.load_json(path)


if __name__ == "__main__":
    unittest.main()
