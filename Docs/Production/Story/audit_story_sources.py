"""Bounded, read-only first-five source evidence. No Unity/gameplay acceptance.

Not a general YAML or C# parser: unsupported selected-source shapes require review.
Only explicit allowlisted source paths are opened. No Assets or existing snapshot is
written. Public API: audit_scene(lines, guids), audit_project(root), compare_snapshot.
"""
import argparse
import hashlib
import json
import math
import re
from dataclasses import dataclass, field
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
SCENE = "Assets/RPG/Scenes/Outpost-Expanded.unity"
SCRIPTS = "Assets/RPG/Scripts/"
KINDS = ("QuestGiver", "Merchant", "TownNPC", "EnemyController")
SOURCES = ("QuestGiver", "Merchant", "TownNPC", "EnemyController", "PlayerProgress",
           "PlayerController", "PlayerServices", "PlayerDiscovery", "PlayerExploration",
           "PrototypeHUD", "AdventurePanels", "FactionQuestSystem", "ExplorationSites",
           "RiverReaverCamp", "WorldRegions")
SAVED = {"abe": ("Abe", "QuestGiver"), "rowan": ("Quartermaster Rowan", "Merchant"),
         "mira": ("Provisioner Mira", "Merchant")}
RUNTIME = {"elara": ("Cartographer Elara", "TownNPC", "npcName"),
           "selvek": ("westbank-reaver-captain", "EnemyController", "enemyId")}
HEADER = re.compile(r"^--- !u!(\d+) &(-?\d+)( stripped)?\s*$")
FIELD = re.compile(r"^  ([A-Za-z_]\w*):\s*(.*?)\s*$")
TOKEN = re.compile(r'"(?:\\.|[^"\\])*"|[A-Za-z_]\w*|\d+(?:\.\d*)?[fF]?|==|>=|&&|=>|[^\s]')


def issue(code, message, severity="error", actor=None):
    result = {"code": code, "message": message, "severity": severity}
    if actor is not None:
        result["actor"] = actor
    return result


def status(issues):
    return "fail" if any(i["severity"] == "error" for i in issues) else "review_required" if issues else "ok"


def scalar(value):
    if value.startswith('"'):
        return json.loads(value)
    if value.startswith("'") and value.endswith("'"):
        return value[1:-1].replace("''", "'")
    return value


def reference(value):
    match = re.fullmatch(r"\{fileID: (-?\d+)\}", value)
    if not match:
        raise ValueError("unsupported/missing local fileID reference: " + value)
    return int(match[1])


def vector(value, keys):
    match = re.fullmatch(r"\{(.*)\}", value)
    if not match:
        raise ValueError("unsupported/missing vector: " + value)
    pairs = [item.strip().split(": ") for item in match[1].split(",")]
    data = {key: float(number) for key, number in pairs}
    if len(pairs) != len(data) or set(data) != set(keys) or not all(math.isfinite(v) for v in data.values()):
        raise ValueError("invalid/nonfinite vector: " + value)
    return [data[k] for k in keys]


@dataclass
class Record:
    kind: int
    id: int
    line: int
    stripped: bool = False
    fields: dict = field(default_factory=dict)
    components: list = field(default_factory=list)


def records(lines):
    """Stream minimal GO/Transform/MonoBehaviour fields, ignoring other records."""
    current = None
    allowed = {1: {"m_Name", "m_IsActive"},
               4: {"m_GameObject", "m_Father", "m_LocalPosition", "m_LocalRotation", "m_LocalScale"},
               114: {"m_GameObject", "m_Script", "m_Enabled", "npcName", "enemyId"}}
    for number, line in enumerate(lines, 1):
        if len(line) > 1024 * 1024:
            raise ValueError("scene line exceeds1MiB bounded parser limit")
        header = HEADER.match(line)
        if header:
            if current is not None:
                yield current
            kind = int(header[1])
            current = Record(kind, int(header[2]), number, bool(header[3])) if kind in allowed else None
            continue
        if current is None:
            continue
        component = re.fullmatch(r"  - component: \{fileID: (-?\d+)\}\s*", line)
        if current.kind == 1 and component:
            current.components.append(int(component[1]))
        match = FIELD.match(line)
        if match and match[1] in allowed[current.kind]:
            if match[1] in current.fields:
                raise ValueError(f"duplicate selected YAML field {match[1]} in record {current.id}")
            current.fields[match[1]] = match[2]
    if current is not None:
        yield current


def script_guid(record):
    match = re.fullmatch(r"\{fileID: 11500000, guid: ([0-9a-f]{32}), type: 3\}", record.fields.get("m_Script", ""))
    return match[1] if match else None


def anchor(record, objects, transforms, issues, actor):
    go_id = reference(record.fields.get("m_GameObject", ""))
    go = objects.get(go_id)
    if not go or record.id not in go.components:
        raise ValueError("selected component missing GameObject or absent from its component list")
    own = [transforms[i] for i in go.components if i in transforms]
    if len(own) != 1:
        raise ValueError("selected GameObject must reference exactly one explicit Transform")
    origin = own[0]
    if reference(origin.fields.get("m_GameObject", "")) != go_id:
        raise ValueError("selected Transform belongs to a different GameObject")
    position = vector(origin.fields.get("m_LocalPosition", ""), "xyz")
    result = {"game_object": go_id, "game_object_name": scalar(go.fields.get("m_Name", "")),
              "component": record.id, "component_line": record.line, "transform": origin.id,
              "local_position": position, "ancestors": [], "world_position": None}
    seen = {origin.id}
    active = go.fields.get("m_IsActive") == "1" and record.fields.get("m_Enabled") == "1"
    safe = not (origin.stripped or go.stripped)
    parent_id = reference(origin.fields.get("m_Father", ""))
    while parent_id:
        if parent_id in seen:
            raise ValueError("Transform ancestor cycle")
        if len(seen) >= 128:
            raise ValueError("Transform ancestry exceeds bounded depth128")
        seen.add(parent_id)
        parent = transforms.get(parent_id)
        if parent is None:
            raise ValueError(f"missing/stripped ancestor Transform {parent_id}")
        parent_go_id = reference(parent.fields.get("m_GameObject", ""))
        parent_go = objects.get(parent_go_id)
        if parent_go is None or parent.id not in parent_go.components:
            raise ValueError(f"ancestor Transform {parent_id} has invalid GameObject linkage")
        active = active and parent_go.fields.get("m_IsActive") == "1"
        rotation = vector(parent.fields.get("m_LocalRotation", ""), "xyzw")
        scale = vector(parent.fields.get("m_LocalScale", ""), "xyz")
        delta = vector(parent.fields.get("m_LocalPosition", ""), "xyz")
        identity = all(abs(v) <= 1e-6 for v in rotation[:3]) and abs(abs(rotation[3]) - 1) <= 1e-6
        unit = all(abs(v - 1) <= 1e-6 for v in scale)
        safe = safe and identity and unit and not parent.stripped and not parent_go.stripped
        result["ancestors"].append({"transform": parent_id, "game_object": parent_go_id,
                                    "active_self": parent_go.fields.get("m_IsActive") == "1",
                                    "identity_rotation": identity, "unit_scale": unit})
        position = [a + b for a, b in zip(position, delta)]
        parent_id = reference(parent.fields.get("m_Father", ""))
    result["active_in_serialized_chain"] = active
    if not active:
        issues.append(issue("inactive_actor_chain", "saved component/GameObject/ancestor is inactive or missing an active flag", "review", actor))
    if safe:
        # Own rotation/scale do not affect its origin; only ancestor transforms do.
        result["world_position"] = [round(v, 7) for v in position]
    else:
        issues.append(issue("unsafe_world_position", "ancestor rotation/scale or stripped records require native review; world position omitted", "review", actor))
    return result


def audit_scene(lines, guids):
    issues, objects, transforms, monos, ids = [], {}, {}, [], set()
    try:
        if not all(re.fullmatch(r"[0-9a-f]{32}", guids.get(kind, "")) for kind in KINDS) or len({guids[k] for k in KINDS}) != len(KINDS):
            raise ValueError("missing/invalid/duplicate selected script metadata GUIDs")
        for record in records(lines):
            if len(ids) >= 250000:
                raise ValueError("scene exceeds250000 selected-record audit bound")
            if record.id in ids:
                raise ValueError(f"duplicate selected Unity record fileID {record.id}")
            ids.add(record.id)
            if record.kind == 1:
                objects[record.id] = record
            elif record.kind == 4:
                transforms[record.id] = record
            else:
                monos.append(record)
    except (ValueError, TypeError) as error:
        return {"status": "fail", "issues": [issue("scene_shape", str(error))], "actors": {}, "runtime_saved": {}, "category_counts": {}}
    counts = {kind: sum(script_guid(m) == guids[kind] for m in monos) for kind in ("QuestGiver", "Merchant", "TownNPC")}
    actors = {}
    for actor, (key, kind) in SAVED.items():
        matches = [m for m in monos if scalar(m.fields.get("npcName", "")) == key]
        if len(matches) != 1:
            issues.append(issue("missing_identity" if not matches else "duplicate_identity", f"expected1 saved {key}; found{len(matches)}", actor=actor))
            continue
        record = matches[0]
        if script_guid(record) != guids[kind]:
            issues.append(issue("wrong_component_kind", f"{key} is not the metadata-bound {kind} script", actor=actor))
            continue
        try:
            actors[actor] = {"lookup_key": key, "component_kind": kind, "script_guid": guids[kind],
                             **anchor(record, objects, transforms, issues, actor)}
        except (ValueError, TypeError) as error:
            issues.append(issue("invalid_actor_anchor", str(error), actor=actor))
    runtime = {}
    for actor, (key, kind, key_field) in RUNTIME.items():
        matches = [m for m in monos if scalar(m.fields.get(key_field, "")) == key]
        runtime[actor] = {"expected_creation": "runtime_ensure", "saved_identity_matches": len(matches)}
        if len(matches) > 1:
            issues.append(issue("duplicate_identity", f"multiple saved runtime identities for {key}", actor=actor))
        elif matches:
            if script_guid(matches[0]) != guids[kind]:
                issues.append(issue("wrong_component_kind", f"saved {key} is not {kind}", actor=actor))
            else:
                issues.append(issue("runtime_identity_now_saved", f"{key} is now saved; review Ensure/duplicate assumptions", "review", actor))
    runtime_roots = {"Frontier exploration sites", "River Reaver camp", "Cartographer Elara", "Captain Selvek"}
    roots = sorted(scalar(go.fields.get("m_Name", "")) for go in objects.values() if scalar(go.fields.get("m_Name", "")) in runtime_roots)
    if roots:
        issues.append(issue("runtime_roots_now_saved", "review saved runtime roots: " + ", ".join(roots), "review"))
    if len(roots) != len(set(roots)):
        issues.append(issue("duplicate_runtime_root", "multiple saved roots share a runtime Ensure name"))
    return {"status": status(issues), "issues": issues, "actors": actors, "runtime_saved": runtime,
            "saved_runtime_roots": roots, "category_counts": counts,
            "count_limit": "Explicit saved component records for3 GUID-bound categories only; not total actors or runtime hierarchy."}


def clean_code(text):
    """Remove comments while preserving quoted text; no execution or C# imports."""
    pattern = re.compile(r'@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'|//[^\n]*|/\*[\s\S]*?\*/')
    return pattern.sub(lambda m: "\n" * m[0].count("\n") if m[0].startswith(("//", "/*")) else m[0], text)


def structure(text):
    return re.sub(r'@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'', lambda m: " " * len(m[0]), text)


def balanced(mask, start, opening, closing):
    depth = 0
    for pos in range(start, len(mask)):
        if mask[pos] == opening:
            depth += 1
        elif mask[pos] == closing:
            depth -= 1
            if depth == 0:
                return pos
    raise ValueError("unbalanced bounded source expression")


def split_values(text):
    mask, result, start, depth = structure(text), [], 0, 0
    for pos, char in enumerate(mask):
        if char in "({[":
            depth += 1
        elif char in ")}]":
            depth -= 1
        elif char == "," and depth == 0:
            result.append(text[start:pos].strip())
            start = pos + 1
    result.append(text[start:].strip())
    return result


def canonical(text):
    return TOKEN.findall(clean_code(text))


def method_body(source, name):
    code = clean_code(source)
    mask = structure(code)
    match = re.search(r"\bvoid\s+" + re.escape(name) + r"\s*\([^)]*\)\s*\{", mask)
    if not match:
        raise ValueError("unrecognized/missing method: " + name)
    start = mask.index("{", match.start())
    return code[start + 1:balanced(mask, start, "{", "}")]


def calls(source, name):
    code = clean_code(source)
    mask = structure(code)
    result = []
    for match in re.finditer(r"\b" + re.escape(name) + r"\s*\(", mask):
        start = mask.index("(", match.start())
        result.append(split_values(code[start + 1:balanced(mask, start, "(", ")")]))
    return result


def const_string(source, name):
    code = clean_code(source)
    match = re.search(r"\bconst\s+string\s+" + re.escape(name) + r"\s*=", structure(code))
    if not match:
        raise ValueError("missing constant: " + name)
    end = structure(code).find(";", match.end())
    return json.loads(code[match.end():end].strip())


def expression(source, name):
    code = clean_code(source)
    mask = structure(code)
    match = re.search(r"\bpublic\s+(?:int|bool|string)\s+" + re.escape(name) + r"\s*=>", mask)
    if not match:
        raise ValueError("unrecognized expression property: " + name)
    end = mask.find(";", match.end())
    if end < 0:
        raise ValueError("missing expression terminator: " + name)
    return code[match.end():end].strip()


def string_array(source, name):
    code = clean_code(source)
    mask = structure(code)
    match = re.search(r"\bstring\s*\[\]\s+" + re.escape(name) + r"\s*=\s*\{", mask)
    if not match:
        raise ValueError("unrecognized string array: " + name)
    start = mask.index("{", match.start())
    return [json.loads(value) for value in split_values(code[start + 1:balanced(mask, start, "{", "}")]) if value]


def audit_runtime(sources):
    issues, evidence = [], {}
    try:
        exploration, river = sources["ExplorationSites"], sources["RiverReaverCamp"]
        name = const_string(exploration, "CartographerName")
        captain = const_string(river, "CaptainId")
        cache = const_string(river, "CacheId")
        surveys = string_array(exploration, "SurveyIds")
        elara = method_body(exploration, "EnsureCartographer")
        captain_calls = [args for args in calls(method_body(river, "Ensure"), "CreateEnemy") if len(args) >= 3 and args[2] == "CaptainId"]
        expected_call = ["root", "player", "CaptainId", '"Captain Selvek"', '"Undead"', '"Rogue"', "Position", "true"]
        checks = {
            "elara_name": name == "Cartographer Elara",
            "elara_survey_ids": surveys == ["westbank-overlook", "bloodrock-vista", "wyrmcrest-lookout"],
            "elara_townnpc_binding": re.search(r"var\s+npc\s*=\s*root\.AddComponent<TownNPC>\s*\(\s*\)", structure(elara)) is not None,
            "elara_npc_name_assignment": "npc.npcName=CartographerName;" in "".join(canonical(elara)),
            "elara_grounded_source_anchor": "WorldRegions.Greenhollow+newVector3(-6,0,-3)" in "".join(canonical(elara)) and "position.y=WorldRegions.GroundHeight(position);" in "".join(canonical(elara)),
            "selvek_id": captain == "westbank-reaver-captain",
            "selvek_cache_id": cache == "river-reaver-lockbox",
            "selvek_creation_call": len(captain_calls) == 1 and captain_calls[0] == expected_call,
            "hud_start_ensures": all("".join(canonical(name + ".Ensure();")) in "".join(canonical(method_body(sources["PrototypeHUD"], "Start"))) for name in ("ExplorationSites", "RiverReaverCamp")),
        }
        for key, matched in checks.items():
            if not matched:
                issues.append(issue("runtime_binding_changed", key))
        evidence = {"elara": {"lookup_key": name, "component_kind": "TownNPC", "creation": "ExplorationSites.EnsureCartographer", "survey_ids": surveys,
                               "position_rule": "WorldRegions.Greenhollow + (-6,0,-3); Y=GroundHeight; not native-verified"},
                    "selvek": {"enemy_id": captain, "cache_id": cache, "creation": "RiverReaverCamp.Ensure/CreateEnemy",
                                "source_call": captain_calls,
                                "race": scalar(captain_calls[0][4]) if len(captain_calls) == 1 and len(captain_calls[0]) == 8 else None,
                                "class": scalar(captain_calls[0][5]) if len(captain_calls) == 1 and len(captain_calls[0]) == 8 else None}, "checks": checks}
    except (ValueError, KeyError, TypeError) as error:
        issues.append(issue("runtime_source_shape", str(error), "review"))
    return evidence, issues


def audit_main_quests(progress, controller):
    issues = []
    expected = {
        "QuestName": 'QuestIndex == 0 ? "The Forest Road" : QuestIndex == 1 ? "Stolen Supplies" : QuestIndex == 2 ? "The Ashwood Captain" : "Hero of Emberwatch"',
        "QuestGold": "QuestIndex == 0 ? 25 : QuestIndex == 1 ? 35 : 60",
        "QuestExperience": "QuestIndex == 0 ? 80 : QuestIndex == 1 ? 100 : 180",
        "QuestReady": "QuestAccepted && (QuestIndex == 0 ? Kills >= 3 : QuestIndex == 1 ? Supplies >= 3 : QuestIndex == 2 && BossDefeated)",
    }
    observed = {}
    try:
        for name, wanted in expected.items():
            observed[name] = expression(progress, name)
            if canonical(observed[name]) != canonical(wanted):
                issues.append(issue("main_quest_expectation_changed", name))
        completion = "".join(canonical(method_body(controller, "ApplyAuthorizedCompleteQuest")))
        for required in ("QuestIndex++;QuestAccepted=false;QuestComplete=QuestIndex>=3;", 'GrantItem("emberwatch-axe",true);', 'GrantItem("emberwatch-sigil",true);', "Potions=Mathf.Min(10,Potions+1);"):
            if required not in completion:
                issues.append(issue("main_completion_contract_changed", required))
    except (ValueError, TypeError) as error:
        issues.append(issue("main_source_shape", str(error), "review"))
    return {"observed_expressions": observed, "expected_stages": [
        {"index": 0, "required": "3 road-raider kills", "gold": 25, "xp": 80},
        {"index": 1, "required": "3 supply crates", "gold": 35, "xp": 100},
        {"index": 2, "required": "BossDefeated (Ashwood captain, not Selvek)", "gold": 60, "xp": 180}],
        "expectations_matched": not issues}, issues


def quest_definitions(source):
    code, result = clean_code(source), []
    mask = structure(code)
    for match in re.finditer(r"\bAdd\s*\(\s*new\s+QuestDef\s*\{", mask):
        start = mask.index("{", match.start())
        body = code[start + 1:balanced(mask, start, "{", "}")]
        fields = {}
        for assignment in split_values(body):
            if not assignment:
                continue
            field_match = re.fullmatch(r"([A-Za-z_]\w*)\s*=\s*([\s\S]+)", assignment)
            if not field_match or field_match[1] in fields:
                raise ValueError("unsupported/duplicate QuestDef assignment")
            fields[field_match[1]] = field_match[2].strip()
        result.append(fields)
    return result


def audit_faction_quests(source):
    issues, selected = [], {}
    expected = {
        "frontier_cartography": {"giverNpc": "ExplorationSites.CartographerName", "targetType": '"discover_landmark"', "targetId": '"frontier-vistas"', "requiredCount": "3", "rewardGold": "75", "rewardXP": "150", "rewardItem": '"travel-charm"', "faction": '"Any"', "prerequisiteQuestId": '""', "nextQuestId": '"river_reaver_bounty"'},
        "river_reaver_bounty": {"giverNpc": "ExplorationSites.CartographerName", "targetType": '"defeat_enemy"', "targetId": "RiverReaverCamp.CaptainId", "requiredCount": "1", "rewardGold": "60", "rewardXP": "140", "rewardItem": '"riverwarden-signet"', "faction": '"Any"', "prerequisiteQuestId": '"frontier_cartography"'},
        "alliance_highspire_dispatch": {"giverNpc": '"Abe"', "targetType": '"talk_npc"', "targetId": '"Grand Duke Ronald"', "requiredCount": "1", "rewardGold": "45", "rewardXP": "110", "rewardItem": '"alliance-war-banner"', "faction": '"Alliance"', "prerequisiteQuestId": '""'},
    }
    count = None
    try:
        definitions = quest_definitions(source)
        count = len(definitions)
        default_ok = re.search(r'public\s+string\s+prerequisiteQuestId\s*=\s*""\s*;', clean_code(source)) is not None
        for identity, wanted in expected.items():
            matches = [q for q in definitions if q.get("id") == json.dumps(identity)]
            if len(matches) != 1:
                issues.append(issue("missing_or_duplicate_quest", identity))
                continue
            actual = {key: matches[0].get(key, '""' if key == "prerequisiteQuestId" and default_ok else None) for key in wanted}
            selected[identity] = actual
            for key, value in wanted.items():
                if actual[key] is None or canonical(actual[key]) != canonical(value):
                    issues.append(issue("faction_quest_expectation_changed", identity + "." + key))
    except (ValueError, TypeError) as error:
        issues.append(issue("faction_source_shape", str(error), "review"))
    return {"definition_count": count, "selected": selected, "limit": "3 first-five giver definitions checked, not all quest behavior."}, issues


def audit_project(root=ROOT):
    root = Path(root).resolve()
    issues, hashes, sources, guids = [], {}, {}, {}
    paths = [SCENE] + [SCRIPTS + name + ".cs" for name in SOURCES] + [SCRIPTS + name + ".cs.meta" for name in KINDS]
    try:
        for relative in paths:
            path = root / relative
            if path.resolve().is_relative_to(root) is False:
                raise ValueError("source path escapes root: " + relative)
            bound = 128 * 1024 * 1024 if relative == SCENE else 1024 * 1024
            if path.stat().st_size > bound:
                raise ValueError("source exceeds audit byte bound: " + relative)
            digest = hashlib.sha256()
            with path.open("rb") as stream:
                for chunk in iter(lambda: stream.read(1024 * 1024), b""):
                    digest.update(chunk)
            hashes[relative] = digest.hexdigest()
        for name in SOURCES:
            path = root / (SCRIPTS + name + ".cs")
            if path.stat().st_size > 1024 * 1024:
                raise ValueError("source exceeds1MiB audit bound: " + name)
            sources[name] = path.read_text(encoding="utf-8-sig")
        for name in KINDS:
            meta = (root / (SCRIPTS + name + ".cs.meta")).read_text(encoding="utf-8-sig")
            found = re.findall(r"^guid: ([0-9a-f]{32})\s*$", meta, re.M)
            if len(found) != 1:
                raise ValueError("missing/duplicate script metadata GUID: " + name)
            guids[name] = found[0]
        if len(set(guids.values())) != len(guids):
            raise ValueError("selected script GUIDs are not unique")
        with (root / SCENE).open(encoding="utf-8-sig") as stream:
            scene = audit_scene(stream, guids)
        issues.extend(scene["issues"])
        runtime, found = audit_runtime(sources)
        issues.extend(found)
        main, found = audit_main_quests(sources["PlayerProgress"], sources["PlayerController"])
        issues.extend(found)
        faction, found = audit_faction_quests(sources["FactionQuestSystem"])
        issues.extend(found)
    except (OSError, ValueError, TypeError) as error:
        issues.append(issue("source_read_or_shape", str(error)))
        scene, runtime, main, faction = {}, {}, {}, {}
    return {"snapshot_version": 1, "evidence_kind": "read_only_source_authoring_not_native_or_gameplay",
            "status": status(issues), "issues": issues, "source_hashes": hashes, "script_guids": guids,
            "scene": scene, "runtime_bindings": runtime, "main_quests": main, "faction_quests": faction,
            "limitations": ["Not general Unity YAML/C# parsing, compilation, hierarchy/physics or gameplay proof.",
                            "Saved positions require identity/unit ancestors; no terrain grounding inferred.",
                            "Source hashes include explicit allowlisted files only; changes require review.",
                            "No narrative schema/adapter acceptance or runtime content admission."]}


def compare_snapshot(current, baseline):
    result = []
    if baseline.get("snapshot_version") != current.get("snapshot_version"):
        result.append(issue("snapshot_version_changed", "review snapshot format", "review"))
    old, new = baseline.get("source_hashes", {}), current.get("source_hashes", {})
    for path in sorted(set(old) | set(new)):
        if old.get(path) != new.get(path):
            result.append(issue("source_changed", path, "review"))
    for key in ("script_guids", "scene", "runtime_bindings", "main_quests", "faction_quests"):
        if current.get(key) != baseline.get(key):
            result.append(issue("evidence_changed", key, "review"))
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=ROOT)
    mode = parser.add_mutually_exclusive_group()
    mode.add_argument("--check", type=Path, help="compare read-only against reviewed snapshot")
    mode.add_argument("--write-snapshot", type=Path, help="write NEW snapshot only; refuses overwrite")
    args = parser.parse_args()
    report = audit_project(args.root)
    try:
        if args.check:
            baseline = json.loads(args.check.read_text(encoding="utf-8"))
            report["issues"].extend(compare_snapshot(report, baseline))
            report["status"] = status(report["issues"])
        if args.write_snapshot:
            if report["status"] != "ok":
                raise ValueError("snapshot not written: unresolved audit issues")
            with args.write_snapshot.open("x", encoding="utf-8", newline="\n") as stream:
                json.dump(report, stream, indent=2, sort_keys=True, ensure_ascii=False)
                stream.write("\n")
    except (OSError, ValueError, TypeError) as error:
        report["issues"].append(issue("snapshot_io", str(error)))
        report["status"] = status(report["issues"])
    print(json.dumps({"status": report["status"], "issues": report["issues"], "category_counts": report.get("scene", {}).get("category_counts", {})}, sort_keys=True))
    return 1 if report["status"] == "fail" else 2 if report["status"] == "review_required" else 0


if __name__ == "__main__":
    raise SystemExit(main())
