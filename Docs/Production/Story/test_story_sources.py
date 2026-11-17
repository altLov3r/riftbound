"""In-memory failure-path tests for read-only source evidence; not Unity tests."""
import io
import unittest

import audit_story_sources as audit


GUIDS = {"QuestGiver": "a" * 32, "Merchant": "b" * 32,
         "TownNPC": "c" * 32, "EnemyController": "d" * 32}


def go(identity, name, components, active=1):
    return (f"--- !u!1 &{identity}\nGameObject:\n  m_Component:\n" +
            "".join(f"  - component: {{fileID: {c}}}\n" for c in components) +
            f"  m_Name: {name}\n  m_IsActive: {active}\n")


def transform(identity, owner, parent=0, position=(0, 0, 0), rotation=(0, 0, 0, 1), scale=(1, 1, 1)):
    def vec(values, keys):
        return "{" + ", ".join(f"{key}: {value}" for key, value in zip(keys, values)) + "}"
    return (f"--- !u!4 &{identity}\nTransform:\n  m_GameObject: {{fileID: {owner}}}\n"
            f"  m_LocalPosition: {vec(position, 'xyz')}\n  m_LocalRotation: {vec(rotation, 'xyzw')}\n"
            f"  m_LocalScale: {vec(scale, 'xyz')}\n  m_Father: {{fileID: {parent}}}\n")


def mono(identity, owner, name, kind, key="npcName"):
    return (f"--- !u!114 &{identity}\nMonoBehaviour:\n  m_GameObject: {{fileID: {owner}}}\n"
            f"  m_Enabled: 1\n  m_Script: {{fileID: 11500000, guid: {GUIDS[kind]}, type: 3}}\n"
            f"  {key}: {name}\n")


def actor(owner, name, kind, parent=0, active=1, **kwargs):
    return (go(owner, name, [owner + 1, owner + 2], active) +
            transform(owner + 1, owner, parent, **kwargs) + mono(owner + 2, owner, name, kind))


def scene(abe=None, rowan=None, mira=None, extra=""):
    return ((actor(10, "Abe", "QuestGiver") if abe is None else abe) +
            (actor(20, "Quartermaster Rowan", "Merchant") if rowan is None else rowan) +
            (actor(30, "Provisioner Mira", "Merchant") if mira is None else mira) + extra)


PROGRESS = '''
public string QuestName => QuestIndex == 0 ? "The Forest Road" : QuestIndex == 1 ? "Stolen Supplies" : QuestIndex == 2 ? "The Ashwood Captain" : "Hero of Emberwatch";
public int QuestGold => QuestIndex == 0 ? 25 : QuestIndex == 1 ? 35 : 60;
public int QuestExperience => QuestIndex == 0 ? 80 : QuestIndex == 1 ? 100 : 180;
public bool QuestReady => QuestAccepted && (QuestIndex == 0 ? Kills >= 3 : QuestIndex == 1 ? Supplies >= 3 : QuestIndex == 2 && BossDefeated);
'''
CONTROLLER = '''public void ApplyAuthorizedCompleteQuest(int expectedIndex) {
    QuestIndex++; QuestAccepted = false; QuestComplete = QuestIndex >= 3;
    Potions = Mathf.Min(10, Potions + 1);
    GrantItem("emberwatch-axe", true); GrantItem("emberwatch-sigil", true);
}'''
EXPLORATION = '''
public static readonly string[] SurveyIds = { "westbank-overlook", "bloodrock-vista", "wyrmcrest-lookout" };
public const string CartographerName = "Cartographer Elara";
static void EnsureCartographer(Transform parent) {
    var position = WorldRegions.Greenhollow + new Vector3(-6, 0, -3);
    position.y = WorldRegions.GroundHeight(position);
    var npc = root.AddComponent<TownNPC>(); npc.npcName = CartographerName;
}'''
RIVER = '''
public const string CaptainId = "westbank-reaver-captain";
public const string CacheId = "river-reaver-lockbox";
public static void Ensure() {
    CreateEnemy(root, player, CaptainId, "Captain Selvek", "Undead", "Rogue", Position, true);
}'''
HUD = '''void Start() { ExplorationSites.Ensure(); RiverReaverCamp.Ensure(); }'''
QUESTS = '''
public string prerequisiteQuestId = "";
Add(new QuestDef {
    id = "frontier_cartography", giverNpc = ExplorationSites.CartographerName,
    targetType = "discover_landmark", targetId = "frontier-vistas", faction = "Any",
    requiredCount = 3, rewardGold = 75, rewardXP = 150, rewardItem = "travel-charm",
    nextQuestId = "river_reaver_bounty"
});
Add(new QuestDef {
    id = "river_reaver_bounty", giverNpc = ExplorationSites.CartographerName,
    targetType = "defeat_enemy", targetId = RiverReaverCamp.CaptainId, faction = "Any",
    requiredCount = 1, rewardGold = 60, rewardXP = 140, rewardItem = "riverwarden-signet",
    prerequisiteQuestId = "frontier_cartography"
});
Add(new QuestDef {
    id = "alliance_highspire_dispatch", giverNpc = "Abe", targetType = "talk_npc",
    targetId = "Grand Duke Ronald", faction = "Alliance", requiredCount = 1,
    rewardGold = 45, rewardXP = 110, rewardItem = "alliance-war-banner", prerequisiteQuestId = ""
});'''


class SceneAuditTests(unittest.TestCase):
    def check_scene(self, text, expected="ok", code=None):
        result = audit.audit_scene(io.StringIO(text), GUIDS)
        self.assertEqual(result["status"], expected, result["issues"])
        if code:
            self.assertIn(code, [i["code"] for i in result["issues"]])
        return result

    def test_exact_components_are_mapped_and_counts_are_bounded(self):
        result = self.check_scene(scene())
        self.assertEqual(result["category_counts"], {"QuestGiver": 1, "Merchant": 2, "TownNPC": 0})
        self.assertEqual(result["actors"]["abe"]["component"], 12)
        self.assertEqual(result["runtime_saved"]["elara"]["saved_identity_matches"], 0)

    def test_game_object_name_alone_is_not_an_actor(self):
        self.check_scene(scene(rowan=go(20, "Quartermaster Rowan", [21]) + transform(21, 20)), "fail", "missing_identity")

    def test_duplicate_identity_does_not_select_first(self):
        self.check_scene(scene(extra=actor(40, "Quartermaster Rowan", "Merchant")), "fail", "duplicate_identity")

    def test_wrong_script_kind_rejects_matching_name(self):
        self.check_scene(scene(rowan=actor(20, "Quartermaster Rowan", "TownNPC")), "fail", "wrong_component_kind")

    def test_missing_script_rejects_matching_name(self):
        self.check_scene(scene().replace(f"guid: {GUIDS['QuestGiver']}", "guid: " + "e" * 32), "fail", "wrong_component_kind")

    def test_component_must_belong_to_existing_game_object(self):
        self.check_scene(scene(abe=actor(10, "Abe", "QuestGiver").replace("  m_GameObject: {fileID: 10}", "  m_GameObject: {fileID: 999}")), "fail", "invalid_actor_anchor")

    def test_component_must_be_listed_on_game_object(self):
        self.check_scene(scene(abe=actor(10, "Abe", "QuestGiver").replace("  - component: {fileID: 12}\n", "")), "fail", "invalid_actor_anchor")

    def test_transform_must_belong_to_same_object(self):
        bad = go(10, "Abe", [11, 12]) + transform(11, 999) + mono(12, 10, "Abe", "QuestGiver")
        self.check_scene(scene(abe=bad), "fail", "invalid_actor_anchor")

    def test_missing_ancestor_is_invalid_not_zero_position(self):
        result = self.check_scene(scene(abe=actor(10, "Abe", "QuestGiver", parent=999)), "fail", "invalid_actor_anchor")
        self.assertNotIn("abe", result["actors"])

    def test_invalid_ancestor_game_object_link_fails(self):
        self.check_scene(scene(abe=actor(10, "Abe", "QuestGiver", parent=91), extra=transform(91, 999)), "fail", "invalid_actor_anchor")

    def test_ancestor_cycle_fails(self):
        extra = go(90, "parent", [91]) + transform(91, 90, parent=11)
        self.check_scene(scene(abe=actor(10, "Abe", "QuestGiver", parent=91), extra=extra), "fail", "invalid_actor_anchor")

    def test_own_rotation_does_not_change_origin(self):
        result = self.check_scene(scene(abe=actor(10, "Abe", "QuestGiver", position=(-3, 0, 2), rotation=(0, 1, 0, 0))))
        self.assertEqual(result["actors"]["abe"]["world_position"], [-3, 0, 2])

    def test_own_scale_does_not_change_origin(self):
        result = self.check_scene(scene(abe=actor(10, "Abe", "QuestGiver", position=(-3, 0, 2), scale=(2, 2, 2))))
        self.assertEqual(result["actors"]["abe"]["world_position"], [-3, 0, 2])

    def test_identity_ancestor_translations_are_added(self):
        extra = go(90, "parent", [91]) + transform(91, 90, position=(-17, 7.6496644, 244))
        result = self.check_scene(scene(mira=actor(30, "Provisioner Mira", "Merchant", parent=91, position=(-25, 5.400336, 160)), extra=extra))
        self.assertEqual(result["actors"]["mira"]["world_position"], [-42, 13.0500004, 404])

    def test_nonidentity_ancestor_rotation_omits_world_position(self):
        extra = go(90, "parent", [91]) + transform(91, 90, rotation=(0, 1, 0, 0))
        result = self.check_scene(scene(abe=actor(10, "Abe", "QuestGiver", parent=91), extra=extra), "review_required", "unsafe_world_position")
        self.assertIsNone(result["actors"]["abe"]["world_position"])

    def test_nonunit_ancestor_scale_omits_world_position(self):
        extra = go(90, "parent", [91]) + transform(91, 90, scale=(2, 1, 1))
        result = self.check_scene(scene(abe=actor(10, "Abe", "QuestGiver", parent=91), extra=extra), "review_required", "unsafe_world_position")
        self.assertIsNone(result["actors"]["abe"]["world_position"])

    def test_inactive_saved_actor_is_review(self):
        result = self.check_scene(scene(abe=actor(10, "Abe", "QuestGiver", active=0)), "review_required", "inactive_actor_chain")
        self.assertFalse(result["actors"]["abe"]["active_in_serialized_chain"])

    def test_inactive_ancestor_is_review(self):
        extra = go(90, "parent", [91], active=0) + transform(91, 90)
        result = self.check_scene(scene(abe=actor(10, "Abe", "QuestGiver", parent=91), extra=extra), "review_required", "inactive_actor_chain")
        self.assertFalse(result["actors"]["abe"]["active_in_serialized_chain"])

    def test_duplicate_file_id_fails(self):
        self.check_scene(scene(extra=go(10, "decoy", [])), "fail", "scene_shape")

    def test_nonfinite_position_fails(self):
        self.check_scene(scene(abe=actor(10, "Abe", "QuestGiver", position=(float("nan"), 0, 0))), "fail", "invalid_actor_anchor")

    def test_runtime_identity_saved_now_requires_review(self):
        self.check_scene(scene(extra=actor(40, "Cartographer Elara", "TownNPC")), "review_required", "runtime_identity_now_saved")

    def test_runtime_identity_wrong_kind_fails(self):
        self.check_scene(scene(extra=actor(40, "Cartographer Elara", "Merchant")), "fail", "wrong_component_kind")

    def test_runtime_root_is_not_silently_ignored(self):
        self.check_scene(scene(extra=go(90, "River Reaver camp", [91]) + transform(91, 90)), "review_required", "runtime_roots_now_saved")

    def test_duplicate_guid_metadata_fails(self):
        bad = dict(GUIDS, Merchant=GUIDS["QuestGiver"])
        self.assertEqual(audit.audit_scene(io.StringIO(scene()), bad)["status"], "fail")


class SourceContractTests(unittest.TestCase):
    def test_main_counts_rewards_and_completion_shape_match(self):
        result, issues = audit.audit_main_quests(PROGRESS, CONTROLLER)
        self.assertFalse(issues)
        self.assertTrue(result["expectations_matched"])

    def test_main_kill_count_change_fails(self):
        _, issues = audit.audit_main_quests(PROGRESS.replace("Kills >= 3", "Kills >= 4"), CONTROLLER)
        self.assertIn("main_quest_expectation_changed", [i["code"] for i in issues])

    def test_main_reward_change_fails(self):
        _, issues = audit.audit_main_quests(PROGRESS.replace("? 25", "? 250"), CONTROLLER)
        self.assertTrue(issues)

    def test_final_item_change_fails(self):
        _, issues = audit.audit_main_quests(PROGRESS, CONTROLLER.replace("emberwatch-sigil", "new-reward"))
        self.assertIn("main_completion_contract_changed", [i["code"] for i in issues])

    def test_comment_decoy_does_not_satisfy_missing_property(self):
        _, issues = audit.audit_main_quests("/*" + PROGRESS + "*/", CONTROLLER)
        self.assertIn("main_source_shape", [i["code"] for i in issues])

    def runtime(self, exploration=EXPLORATION, river=RIVER, hud=HUD):
        return audit.audit_runtime({"ExplorationSites": exploration, "RiverReaverCamp": river, "PrototypeHUD": hud})

    def test_runtime_bindings_are_separate_from_saved_scene(self):
        result, issues = self.runtime()
        self.assertFalse(issues)
        self.assertEqual(result["selvek"]["source_call"][0][4:6], ['"Undead"', '"Rogue"'])

    def test_wrong_selvek_race_fails(self):
        result, issues = self.runtime(river=RIVER.replace('"Undead"', '"Human"'))
        self.assertTrue(issues)
        self.assertEqual(result["selvek"]["race"], "Human")

    def test_duplicate_selvek_source_call_fails(self):
        call = 'CreateEnemy(root, player, CaptainId, "Captain Selvek", "Undead", "Rogue", Position, true);'
        _, issues = self.runtime(river=RIVER.replace(call, call + call))
        self.assertTrue(issues)

    def test_elara_name_assignment_must_match_constant(self):
        _, issues = self.runtime(exploration=EXPLORATION.replace("npc.npcName = CartographerName", 'npc.npcName = "Other"'))
        self.assertTrue(issues)

    def test_elara_component_must_be_townnpc(self):
        _, issues = self.runtime(exploration=EXPLORATION.replace("AddComponent<TownNPC>", "AddComponent<Merchant>"))
        self.assertTrue(issues)

    def test_survey_membership_change_fails(self):
        _, issues = self.runtime(exploration=EXPLORATION.replace("bloodrock-vista", "ancient-grove"))
        self.assertTrue(issues)

    def test_hud_ensure_call_in_comment_is_not_execution_binding(self):
        _, issues = self.runtime(hud="void Start() { /*ExplorationSites.Ensure();*/ RiverReaverCamp.Ensure(); }")
        self.assertTrue(issues)

    def test_faction_selected_definitions_match(self):
        result, issues = audit.audit_faction_quests(QUESTS)
        self.assertFalse(issues)
        self.assertEqual(result["definition_count"], 3)

    def test_missing_faction_quest_fails(self):
        _, issues = audit.audit_faction_quests(QUESTS.replace('id = "river_reaver_bounty"', 'id = "other"'))
        self.assertIn("missing_or_duplicate_quest", [i["code"] for i in issues])

    def test_duplicate_faction_identity_fails(self):
        _, issues = audit.audit_faction_quests(QUESTS + QUESTS)
        self.assertIn("missing_or_duplicate_quest", [i["code"] for i in issues])

    def test_bounty_prerequisite_change_fails(self):
        _, issues = audit.audit_faction_quests(QUESTS.replace('prerequisiteQuestId = "frontier_cartography"', 'prerequisiteQuestId = ""'))
        self.assertIn("faction_quest_expectation_changed", [i["code"] for i in issues])

    def test_faction_count_and_reward_changes_fail(self):
        for changed in (QUESTS.replace("requiredCount = 3", "requiredCount = 4"), QUESTS.replace("rewardGold = 60", "rewardGold = 600")):
            with self.subTest(changed=changed):
                self.assertTrue(audit.audit_faction_quests(changed)[1])

    def test_omitted_reward_does_not_silently_take_expected_value(self):
        self.assertTrue(audit.audit_faction_quests(QUESTS.replace("rewardXP = 150,", ""))[1])

    def test_changed_default_prerequisite_requires_review(self):
        self.assertTrue(audit.audit_faction_quests(QUESTS.replace('public string prerequisiteQuestId = ""', 'public string prerequisiteQuestId = "new-gate"'))[1])

    def test_commented_definition_and_string_decoy_not_counted(self):
        result, issues = audit.audit_faction_quests(QUESTS + '\n// Add(new QuestDef { id = "decoy" });\nstring s = "Add(new QuestDef {";')
        self.assertFalse(issues)
        self.assertEqual(result["definition_count"], 3)

    def test_hash_drift_is_review_even_when_binding_unchanged(self):
        baseline = {"snapshot_version": 1, "source_hashes": {"selected.cs": "old"}}
        current = {"snapshot_version": 1, "source_hashes": {"selected.cs": "new"}}
        issues = audit.compare_snapshot(current, baseline)
        self.assertEqual(issues[0]["code"], "source_changed")
        self.assertEqual(audit.status(issues), "review_required")

    def test_unchanged_snapshot_has_no_drift(self):
        report = {"snapshot_version": 1, "source_hashes": {"selected.cs": "same"}}
        self.assertEqual(audit.compare_snapshot(report, report), [])


if __name__ == "__main__":
    unittest.main()
