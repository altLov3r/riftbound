"""Read-only authoring checks for the proposed catalog; never runs Unity/gameplay."""
import argparse
import itertools
import json
import re
from pathlib import Path


HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
SCHEMA_PATH = HERE / "first-five.schema.json"
CATALOG_PATH = HERE / "first-five.v1.proposed.json"
FIXTURES_PATH = HERE / "selection-fixtures.json"
BINDINGS = {
    "abe": ("main_quest_giver", "Abe", ""),
    "rowan": ("merchant", "Quartermaster Rowan", ""),
    "mira": ("merchant", "Provisioner Mira", ""),
    "elara": ("faction_quest_giver", "Cartographer Elara", ""),
    "selvek": ("hostile_enemy", "Captain Selvek", "westbank-reaver-captain"),
}
CHANNELS = {
    "abe": {"greeting", "main_title", "main_body", "public_topic"},
    "rowan": {"greeting", "public_topic"},
    "mira": {"greeting", "public_topic"},
    "elara": {"greeting", "quest_offer", "quest_progress", "quest_ready", "quest_after", "public_topic"},
    "selvek": {"encounter_bark"},
}
ELARA_QUESTS = {"frontier_cartography", "river_reaver_bounty"}
NO_PRIVATE_REVEALS = re.compile(r"\b(Mara|Seventh Wagon|Tithe of Names|Perfect Witness|Hollow Choir|Kirin Tor|Forsaken|Bilgewater|Silvermoon)\b", re.I)


def require(value, message):
    if not value:
        raise ValueError(message)


def load_json(path):
    def unique(pairs):
        obj = {}
        for key, value in pairs:
            require(key not in obj, f"duplicate JSON property: {key}")
            obj[key] = value
        return obj

    def bad_constant(value):
        raise ValueError(f"nonfinite JSON number: {value}")

    return json.loads(Path(path).read_text(encoding="utf-8"), object_pairs_hook=unique, parse_constant=bad_constant)


def check_shape(value, schema, root_schema, path="$schema"):
    """Validate the exact documented schema vocabulary, not arbitrary JSON Schema."""
    supported = {"$schema", "$id", "title", "$defs", "$ref", "type", "const", "enum", "properties", "required", "additionalProperties", "items", "minItems", "maxItems", "minimum", "maximum", "minLength", "maxLength", "pattern"}
    require(not set(schema) - supported, f"unsupported schema keyword at {path}")
    if "$ref" in schema:
        ref = schema["$ref"]
        require(ref.startswith("#/$defs/"), f"external schema ref refused: {ref}")
        return check_shape(value, root_schema["$defs"][ref.split("/")[-1]], root_schema, path)
    kind = schema.get("type")
    types = {"object": dict, "array": list, "string": str, "integer": int, "boolean": bool}
    require(kind in types and type(value) is types[kind], f"wrong {kind} type at {path}")
    if "const" in schema:
        require(value == schema["const"], f"wrong constant at {path}")
    if "enum" in schema:
        require(value in schema["enum"], f"unknown enum at {path}: {value}")
    if kind == "object":
        fields = schema.get("properties", {})
        require(not set(schema.get("required", [])) - set(value), f"missing field at {path}")
        if schema.get("additionalProperties") is False:
            require(not set(value) - set(fields), f"unknown field at {path}")
        for key, item in value.items():
            if key in fields:
                check_shape(item, fields[key], root_schema, f"{path}.{key}")
    elif kind == "array":
        require(len(value) >= schema.get("minItems", 0), f"array too short at {path}")
        require(len(value) <= schema.get("maxItems", float("inf")), f"array too long at {path}")
        for i, item in enumerate(value):
            check_shape(item, schema["items"], root_schema, f"{path}[{i}]")
    elif kind == "string":
        require(len(value) >= schema.get("minLength", 0), f"text too short at {path}")
        require(len(value) <= schema.get("maxLength", float("inf")), f"text too long at {path}")
        if "pattern" in schema:
            require(re.search(schema["pattern"], value) is not None, f"pattern mismatch at {path}")
    elif kind == "integer":
        require(schema.get("minimum", value) <= value <= schema.get("maximum", value), f"number outside bounds at {path}")


def predicate_key(condition):
    return condition["field"] + (":" + condition["key"] if condition["key"] else "")


def predicate_value(condition):
    return condition["intValue"] if condition["field"] == "main.index" else condition["boolValue"]


def consistent(state):
    index = state.get("main.index")
    complete = state.get("main.complete")
    accepted = state.get("main.accepted")
    ready = state.get("main.ready")
    if complete is True and (index not in (None, 3) or accepted is True or ready is True):
        return False
    if index == 3 and (complete is False or accepted is True or ready is True):
        return False
    if ready is True and (accepted is False or complete is True or index == 3):
        return False
    active = [key.split(":", 1)[1] for key, value in state.items() if key.startswith("quest.active:") and value is True]
    if len(active) > 1:
        return False
    ids = {key.split(":", 1)[1] for key in state if key.startswith("quest.") and ":" in key}
    for quest in ids:
        get = lambda field: state.get(field + ":" + quest)
        if get("quest.completed") is True and any(get(field) is True for field in ("quest.available", "quest.active", "quest.ready")):
            return False
        if get("quest.ready") is True and get("quest.active") is False:
            return False
        if get("quest.available") is True and (active or get("quest.ready") is True):
            return False
    return True


def constraint_map(conditions):
    result = {}
    for condition in conditions:
        key = predicate_key(condition)
        value = predicate_value(condition)
        require(key not in result, f"duplicate/contradictory predicate {key}")
        result[key] = value
    require(consistent(result), "contradictory condition set")
    return result


def overlap(a, b):
    merged = dict(a)
    for key, value in b.items():
        if key in merged and merged[key] != value:
            return False
        merged[key] = value
    return consistent(merged)


def validate_catalog(catalog, root=ROOT):
    schema = load_json(SCHEMA_PATH)
    check_shape(catalog, schema, schema)
    quest_source = (root / "Assets/RPG/Scripts/FactionQuestSystem.cs").read_text(encoding="utf-8-sig")
    quest_ids = set(re.findall(r'\bid\s*=\s*"([^"]+)"', quest_source))
    require(ELARA_QUESTS <= quest_ids, "required source quest IDs missing")
    camp_source = (root / "Assets/RPG/Scripts/RiverReaverCamp.cs").read_text(encoding="utf-8-sig")
    require('CaptainId = "westbank-reaver-captain"' in camp_source, "captain source identity changed")
    scene = (root / "Assets/RPG/Scenes/Outpost-Expanded.unity").read_text(encoding="utf-8-sig")
    require(all(name in scene for name in ("Quartermaster Rowan", "Provisioner Mira", "Abe")), "saved identity missing; re-audit binding")
    exploration = (root / "Assets/RPG/Scripts/ExplorationSites.cs").read_text(encoding="utf-8-sig")
    require('CartographerName = "Cartographer Elara"' in exploration, "runtime-created Elara identity changed")
    progress = (root / "Assets/RPG/Scripts/PlayerProgress.cs").read_text(encoding="utf-8-sig")
    title_source = re.search(r"public string QuestName =>([^;]+);", progress)
    require(title_source is not None, "main title source changed")
    main_titles = re.findall(r'"([^"]+)"', title_source.group(1))
    require(len(main_titles) == 4, "main title branches changed; re-audit")
    profile_ids, bindings, line_ids = set(), set(), set()
    total = 0
    for profile in catalog["profiles"]:
        pid = profile["profileId"]
        require(pid not in profile_ids, f"duplicate profile {pid}")
        profile_ids.add(pid)
        require((profile["actorKind"], profile["legacyName"], profile["enemyId"]) == BINDINGS[pid], f"wrong source identity/kind for {pid}")
        binding = (profile["actorKind"], profile["legacyName"].casefold(), profile["enemyId"])
        require(binding not in bindings, f"duplicate actor binding {pid}")
        bindings.add(binding)
        require(profile["fallbackText"].strip() and not NO_PRIVATE_REVEALS.search(profile["fallbackText"]) and not re.search(r"[<>{}]", profile["fallbackText"]), f"unsafe fallback {pid}")
        candidates = []
        for entry in profile["entries"]:
            total += 1
            lid = entry["lineId"]
            require(lid.startswith(pid + ".") and lid not in line_ids, f"duplicate/wrong-profile line {lid}")
            line_ids.add(lid)
            require(entry["channel"] in CHANNELS[pid], f"unsupported actor/channel {lid}")
            require(entry["text"].strip() and not re.search(r"[<>{}]", entry["text"]), f"nonliteral text {lid}")
            require(not NO_PRIVATE_REVEALS.search(entry["text"]), f"withheld/copy-universe text {lid}")
            if entry["channel"].startswith("quest_"):
                require(pid == "elara" and entry["topicId"] in ELARA_QUESTS, f"wrong quest surface {lid}")
            elif entry["channel"] in {"main_title", "main_body", "greeting"}:
                require(entry["topicId"] == "", f"unexpected topic {lid}")
            else:
                require(entry["topicId"] != "", f"missing requested topic {lid}")
            require((entry["scope"] == "host_encounter") == (pid == "selvek"), f"wrong encounter scope {lid}")
            for condition in entry["conditions"]:
                field, key = condition["field"], condition["key"]
                if field == "main.index":
                    require(key == "" and condition["boolValue"] is False, f"bad index value {lid}")
                else:
                    require(condition["intValue"] == 0, f"unused integer value {lid}")
                    if field.startswith("main."):
                        require(key == "", f"unexpected main key {lid}")
                if field.startswith("quest."):
                    require(key in quest_ids and key in ELARA_QUESTS, f"unknown/out-of-slice quest {lid}")
                if field.startswith("enemy."):
                    require(key == "westbank-reaver-captain", f"wrong enemy key {lid}")
                if field.startswith(("main.", "quest.")) or field == "enemy.defeated":
                    require(entry["scope"] == "viewer", f"private-state/public-scope leak {lid}")
            require(entry["scope"] != "public" or not entry["conditions"], f"conditional public knowledge {lid}")
            constraints = constraint_map(entry["conditions"])
            if entry["channel"] in {"main_title", "main_body"}:
                require("main.index" in constraints and "main.complete" in constraints, f"unbounded main surface {lid}")
            if entry["channel"] == "main_title":
                require(entry["text"] == main_titles[constraints["main.index"]], f"changed legacy quest title {lid}")
            if entry["channel"] in {"quest_progress", "quest_ready"}:
                quest = entry["topicId"]
                require(constraints.get("quest.active:" + quest) is True, f"missing exact active quest {lid}")
                require(constraints.get("quest.ready:" + quest) is (entry["channel"] == "quest_ready"), f"wrong readiness surface {lid}")
            if entry["channel"] == "quest_offer":
                require(constraints.get("quest.available:" + entry["topicId"]) is True, f"ineligible offer {lid}")
            if entry["channel"] == "quest_after":
                require(constraints.get("quest.completed:" + entry["topicId"]) is True, f"unearned aftermath {lid}")
            if pid == "selvek":
                require(constraints == {"enemy.alive:westbank-reaver-captain": True}, f"unsafe hostile hook {lid}")
            candidates.append((entry, constraints))
        for (left, a), (right, b) in itertools.combinations(candidates, 2):
            if all(left[key] == right[key] for key in ("channel", "topicId", "priority")):
                require(not overlap(a, b), f"ambiguous equal-priority entries: {left['lineId']}/{right['lineId']}")
    require(profile_ids == set(BINDINGS), "incomplete first-five identity set")
    return {"profiles": len(profile_ids), "entries": total, "sourceQuestIds": len(quest_ids)}


def select_line(catalog, profile_id, channel, topic, state, eligible=True):
    """Authoring reference only; not proof of an installed Unity adapter."""
    profile = next((p for p in catalog["profiles"] if p["profileId"] == profile_id), None)
    if not eligible or (profile and profile["actorKind"] == "hostile_enemy" and channel != "encounter_bark"):
        return "$denied"
    fallback = "$none" if channel == "encounter_bark" else "$fallback" if channel in {"greeting", "public_topic"} else "$existing"
    if profile is None or not consistent(state):
        return fallback
    matches = []
    for entry in profile["entries"]:
        if entry["channel"] != channel or entry["topicId"] != topic:
            continue
        # Missing keys stay UNKNOWN, even when the authored predicate requests false.
        if all(predicate_key(c) in state and type(state[predicate_key(c)]) is type(predicate_value(c)) and state[predicate_key(c)] == predicate_value(c) for c in entry["conditions"]):
            matches.append(entry)
    matches.sort(key=lambda e: (-e["priority"], e["lineId"]))
    return matches[0]["lineId"] if matches else fallback


def run_fixtures(catalog):
    cases = load_json(FIXTURES_PATH)
    ids = set()
    for case in cases:
        require(case["caseId"] not in ids, "duplicate scenario ID")
        ids.add(case["caseId"])
        actual = select_line(catalog, case["profileId"], case["channel"], case["topicId"], case["state"], case.get("eligible", True))
        require(actual == case["expected"], f"scenario {case['caseId']}: expected {case['expected']}, got {actual}")
    return len(cases)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--catalog", type=Path, default=CATALOG_PATH)
    parser.add_argument("--fixtures", action="store_true")
    args = parser.parse_args()
    catalog = load_json(args.catalog)
    result = validate_catalog(catalog)
    if args.fixtures:
        result["authoringScenarios"] = run_fixtures(catalog)
    result["scope"] = "read-only authoring/source checks; NOT Unity/gameplay/adapter verification"
    print(json.dumps(result))


if __name__ == "__main__":
    main()
