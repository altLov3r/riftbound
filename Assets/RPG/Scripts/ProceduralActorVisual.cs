using System;
using System.Linq;
using UnityEngine;

namespace Riftbound
{
    [DefaultExecutionOrder(850)]
    public sealed class ProceduralActorVisual : MonoBehaviour
    {
        [SerializeField] GameObject model;
        [SerializeField] Animator source;
        [SerializeField] float heightScale = 1f;

        [SerializeField] Transform stagedDisplay;

        Transform display, pelvis;
        Transform rightHandBone, leftHandBone;
        Transform rightWeaponSocket, leftWeaponSocket;
        GameObject rightWeaponInstance, leftWeaponInstance;
        string equippedWeaponId = "";
        string equippedOffhandId = "";
        bool weaponVisible = true;
        Animator displayAnimator;
        Joint[] joints;
        Wrist[] wrists;
        Vector3 previousPosition;
        Vector3 pelvisRest, sourcePelvisRest;
        bool movingParameter;
        Renderer[] originals;

        // Parent joints come first so each child receives the current parent pose.
        static readonly (string bone, string end, HumanBodyBones human, HumanBodyBones humanEnd)[] Body = {
            ("hips", "spine", HumanBodyBones.Hips, HumanBodyBones.Spine),
            ("spine", "head", HumanBodyBones.Spine, HumanBodyBones.Head),
            ("upper_arm.R", "forearm.R", HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm),
            ("forearm.R", "hand.R", HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand),
            ("upper_arm.L", "forearm.L", HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm),
            ("forearm.L", "hand.L", HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand),
            ("thigh.R", "shin.R", HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg),
            ("shin.R", "foot.R", HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot),
            ("thigh.L", "shin.L", HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg),
            ("shin.L", "foot.L", HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot)
        };

        struct Joint
        {
            public Transform from, to, target;
            public Quaternion restRotation;
            public Vector3 axis;
        }

        struct Wrist
        {
            public Joint forearm;
            public Transform hand;
            public Quaternion driverRest, handRest, driverToKit;

            public void Copy()
            {
                if (!forearm.from || !forearm.to || !hand) return;
                var current = Quaternion.Inverse(forearm.from.rotation) * forearm.to.rotation;
                var movement = current * Quaternion.Inverse(driverRest);
                hand.localRotation = driverToKit * movement * Quaternion.Inverse(driverToKit) * handRest;
            }
        }

        public static ProceduralActorVisual Attach(GameObject owner, Animator driver, GameObject prefab, float scale)
        {
            if (!owner || !prefab) return null;
            var visual = owner.AddComponent<ProceduralActorVisual>();
            visual.Configure(driver, prefab, scale);
            visual.CreateDisplay();
            return visual;
        }

        public void Configure(Animator driver, GameObject prefab, float scale)
        {
            source = driver;
            model = prefab;
            heightScale = scale;
        }

        public Animator Driver => source;
        public Transform StagedDisplay => stagedDisplay;
        public string EquippedWeaponId => equippedWeaponId;
        public Transform RightWeaponSocket => rightWeaponSocket;
        public GameObject RightWeaponInstance => rightWeaponInstance;
        public bool UsingUnarmedVariant { get; private set; }

#if UNITY_EDITOR
        public void StageDisplay(Transform preview)
        {
            stagedDisplay = preview;
            var parent = source ? source.transform : transform;
            var originalMeshes = FindOriginalMeshes(parent);
            SetDisplayTransform(preview, parent);
            foreach (var renderer in originalMeshes)
            {
                if (renderer.transform.IsChildOf(preview)) continue;
                renderer.enabled = false;
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            }
        }
#endif

        void Start() => CreateDisplay();

        void CreateDisplay()
        {
            if (!model || display) return;
            // Keep the driver underneath its existing owner. That preserves state, foot planting,
            // strafe orientation and network animation even when its meshes are hidden.
            var parent = source ? source.transform : transform;
            originals = FindOriginalMeshes(parent);
            var oldBones = source ? source.GetComponentsInChildren<Transform>(true) : Array.Empty<Transform>();

            string baseModelName = CleanModelName(model.name);
            var unarmedPrefab = Resources.Load<GameObject>("Actors/Unarmed/" + baseModelName + "_Unarmed");

            GameObject instance;
            if (unarmedPrefab != null)
            {
                UsingUnarmedVariant = true;
                instance = Instantiate(unarmedPrefab, parent, false);
                instance.name = "Emberford Body";
                display = instance.transform;
                if (stagedDisplay)
                {
                    display.localPosition = stagedDisplay.localPosition;
                    display.localRotation = stagedDisplay.localRotation;
                    display.localScale = stagedDisplay.localScale;
                    foreach (var child in display.GetComponentsInChildren<Transform>(true))
                        child.gameObject.layer = stagedDisplay.gameObject.layer;
                    var oldStaged = stagedDisplay.gameObject;
                    stagedDisplay = display;
                    oldStaged.SetActive(false);
                    if (Application.isPlaying) Destroy(oldStaged);
                    else DestroyImmediate(oldStaged);
                }
                else
                {
                    SetDisplayTransform(display, parent);
                }
            }
            else
            {
                UsingUnarmedVariant = false;
                instance = stagedDisplay ? stagedDisplay.gameObject : Instantiate(model, parent, false);
                instance.name = "Emberford Body";
                display = instance.transform;
                if (!stagedDisplay) SetDisplayTransform(display, parent);
            }

            foreach (var npc in instance.GetComponentsInChildren<TownNPC>(true))
            {
                npc.enabled = false;
                if (Application.isPlaying) Destroy(npc);
                else DestroyImmediate(npc);
            }
            var skin = instance.GetComponentInChildren<SkinnedMeshRenderer>();
            if (!skin)
            {
                if (Application.isPlaying) Destroy(instance);
                else DestroyImmediate(instance);
                return;
            }

            rightHandBone = Array.Find(skin.bones, b => b && b.name == "hand.R");
            leftHandBone = Array.Find(skin.bones, b => b && b.name == "hand.L");

            displayAnimator = instance.GetComponentInChildren<Animator>();
            var bindings = Body.Select(pair =>
            {
                var first = FindBone(source, oldBones, pair.bone, pair.human);
                var second = FindBone(source, oldBones, pair.end, pair.humanEnd);
                var target = Array.Find(skin.bones, b => b && b.name == pair.bone);
                var end = Array.Find(skin.bones, b => b && b.name == pair.end);
                return first && second && target && end && first != second
                    ? new Joint { from = first, to = second, target = target, restRotation = target.localRotation,
                        axis = target.InverseTransformDirection(end.position - target.position).normalized }
                    : default;
            }).Where(j => j.target).ToArray();
            if (bindings.Length >= 8 && source.runtimeAnimatorController)
            {
                joints = bindings;
                wrists = bindings.Where(j => j.target.name.StartsWith("forearm.", StringComparison.Ordinal))
                    .Select(j =>
                    {
                        var hand = Array.Find(skin.bones, b => b && b.parent == j.target && b.name.StartsWith("hand.", StringComparison.Ordinal));
                        if (!hand) return default;
                        var align = Quaternion.FromToRotation(j.to.position - j.from.position, hand.position - j.target.position);
                        return new Wrist
                        {
                            forearm = j, hand = hand, handRest = hand.localRotation,
                            driverRest = Quaternion.Inverse(j.from.rotation) * j.to.rotation,
                            driverToKit = Quaternion.Inverse(j.target.rotation) * align * j.from.rotation
                        };
                    }).Where(w => w.hand).ToArray();
                if (displayAnimator) displayAnimator.enabled = false;
                source.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                if (bindings[0].target.name == "hips")
                {
                    pelvis = bindings[0].target;
                    pelvisRest = pelvis.localPosition;
                    sourcePelvisRest = source.transform.InverseTransformPoint(bindings[0].from.position);
                }
            }
            movingParameter = displayAnimator && displayAnimator.parameters.Any(
                p => p.type == AnimatorControllerParameterType.Bool && p.name == "Moving");
            foreach (var renderer in originals)
            {
                if (renderer && (!display || !renderer.transform.IsChildOf(display)))
                    renderer.forceRenderingOff = true;
            }
            previousPosition = transform.position;
            CopyPose();

            if (UsingUnarmedVariant)
            {
                var (mainWeapon, offWeapon, rarity) = ResolveDefaultWeapons(gameObject, baseModelName);
                if (!string.IsNullOrEmpty(mainWeapon))
                    EquipWeaponVisual(mainWeapon, offWeapon, rarity);
            }
        }

        static string CleanModelName(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            string s = raw.Replace("(Clone)", "").Trim();
            if (s.EndsWith("_Unarmed", StringComparison.Ordinal))
                s = s.Substring(0, s.Length - "_Unarmed".Length);
            return s;
        }

        public void EquipWeaponVisual(string weaponId, string offhandWeaponId = null, string rarity = null)
        {
            if (!display) CreateDisplay();
            if (!rightHandBone) return;

            string normMain = weaponId ?? "";
            string normOff = offhandWeaponId ?? "";
            if (equippedWeaponId == normMain && equippedOffhandId == normOff && rightWeaponInstance != null)
            {
                ApplyWeaponRaritySheen(rightWeaponInstance, rarity);
                SetWeaponVisualVisible(weaponVisible);
                return;
            }

            equippedWeaponId = normMain;
            equippedOffhandId = normOff;

            ClearSocketWeapon(ref rightWeaponInstance);
            ClearSocketWeapon(ref leftWeaponInstance);

            if (!string.IsNullOrEmpty(normMain))
            {
                rightWeaponSocket = EnsureSocket(rightHandBone, "WeaponSocket_R");
                rightWeaponInstance = SpawnWeaponInSocket(rightWeaponSocket, normMain, isLeftHand: false, rarity: rarity);
            }

            if (!string.IsNullOrEmpty(normOff) && leftHandBone)
            {
                leftWeaponSocket = EnsureSocket(leftHandBone, "WeaponSocket_L");
                leftWeaponInstance = SpawnWeaponInSocket(leftWeaponSocket, normOff, isLeftHand: true, rarity: rarity);
            }

            SetWeaponVisualVisible(weaponVisible);
        }

        public void SetWeaponVisualVisible(bool visible)
        {
            weaponVisible = visible;
            if (rightWeaponInstance) rightWeaponInstance.SetActive(visible);
            if (leftWeaponInstance) leftWeaponInstance.SetActive(visible);
        }

        Transform EnsureSocket(Transform handBone, string socketName)
        {
            var existing = handBone.Find(socketName);
            if (existing) return existing;
            var go = new GameObject(socketName);
            go.layer = handBone.gameObject.layer;
            go.transform.SetParent(handBone, false);
            return go.transform;
        }

        GameObject SpawnWeaponInSocket(Transform socket, string weaponId, bool isLeftHand, string rarity)
        {
            var prefab = GearArt.LoadWeaponPrefab(weaponId);
            if (!prefab) return null;

            bool isStaff = weaponId.IndexOf("staff", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isDagger = weaponId.IndexOf("dagger", StringComparison.OrdinalIgnoreCase) >= 0;

            // Calibrated palm grip along hand bone (+Y points wrist -> fingertips in Blender FBX rig)
            socket.localPosition = new Vector3(0f, 0.058f, 0f);
            if (isStaff)
            {
                socket.localRotation = Quaternion.Euler(14f, 0f, isLeftHand ? 8f : -8f);
                socket.localScale = Vector3.one * 0.92f;
            }
            else if (isDagger)
            {
                socket.localRotation = Quaternion.Euler(74f, 0f, isLeftHand ? 14f : -14f);
                socket.localScale = Vector3.one * 0.95f;
            }
            else
            {
                socket.localRotation = Quaternion.Euler(78f, 0f, isLeftHand ? 12f : -12f);
                socket.localScale = Vector3.one * 0.92f;
            }

            var inst = Instantiate(prefab, socket, false);
            inst.name = "HeldWeapon_" + weaponId;
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.identity;
            inst.transform.localScale = Vector3.one;

            int layer = socket.gameObject.layer;
            foreach (var t in inst.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = layer;
            foreach (var c in inst.GetComponentsInChildren<Collider>(true)) c.enabled = false;

            ApplyWeaponRaritySheen(inst, rarity);
            return inst;
        }

        static void ApplyWeaponRaritySheen(GameObject weaponGo, string rarity)
        {
            if (!weaponGo) return;
            bool glow = rarity == "Epic" || rarity == "Legendary";
            Color emit = rarity == "Legendary" ? new Color(0.65f, 0.36f, 0.08f) :
                         rarity == "Epic" ? new Color(0.36f, 0.14f, 0.55f) : Color.black;
            var block = new MaterialPropertyBlock();
            foreach (var r in weaponGo.GetComponentsInChildren<Renderer>(true))
            {
                for (int i = 0; i < r.sharedMaterials.Length; i++)
                {
                    r.GetPropertyBlock(block, i);
                    block.SetColor("_EmissionColor", glow ? emit : Color.black);
                    r.SetPropertyBlock(block, i);
                }
            }
        }

        static void ClearSocketWeapon(ref GameObject inst)
        {
            if (!inst) return;
            if (Application.isPlaying) Destroy(inst);
            else DestroyImmediate(inst);
            inst = null;
        }

        public static (string main, string offhand, string rarity) ResolveDefaultWeapons(GameObject owner, string modelName)
        {
            if (!owner) return (null, null, null);

            var player = owner.GetComponentInParent<PlayerController>();
            if (player)
            {
                string id = !string.IsNullOrEmpty(player.EquippedWeaponId) ? player.EquippedWeaponId : "starter-axe";
                var item = ItemCatalog.Get(id);
                string off = player.CharacterClass == "Rogue" && id.IndexOf("dagger", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "copper-dagger" : null;
                return (id, off, item?.rarity);
            }

            var enemy = owner.GetComponentInParent<EnemyController>();
            if (enemy)
            {
                string eid = (enemy.enemyId ?? "").ToLowerInvariant();
                if (eid.Contains("malrik") || eid.Contains("sunstone")) return ("sunstone-reliquary-staff", null, "Epic");
                if (eid.Contains("krag") || eid.Contains("ironmaw-boss")) return ("ironmaw-cleaver", null, "Epic");
                if (eid.Contains("grimgnaw")) return ("bloodrock-waraxe", null, "Rare");
                if (eid.Contains("captain") || eid.Contains("selvek")) return ("captain-axe", null, "Rare");
                if (eid.Contains("warden")) return ("warden-axe", null, "Rare");
                if (eid.Contains("malakar") || eid.Contains("abbot")) return ("scarlet-blade", null, "Epic");
                if (eid.Contains("raider") || eid.Contains("tribe")) return ("raider-cleaver", null, "Uncommon");
            }

            if (modelName.Contains("Mage") || modelName.Contains("Wraith"))
                return ("mage-arcane-staff", null, "Rare");
            if (modelName.Contains("Rogue"))
                return ("duskwood-shadow-dagger", "copper-dagger", "Uncommon");
            if (modelName.Contains("Goblin"))
                return ("copper-dagger", null, "Common");
            if (modelName.Contains("Paladin"))
                return ("truesilver-blade", null, "Rare");
            if (modelName.Contains("Sentinel"))
                return ("worldbreaker-maul", null, "Epic");
            if (modelName.Contains("Brute"))
                return ("bloodrock-waraxe", null, "Uncommon");
            if (modelName.Contains("Briar_Knight"))
                return ("raider-cleaver", null, "Uncommon");
            if (modelName.Contains("Emberwatch_Knight"))
                return ("bronze-broadsword", null, "Uncommon");

            return (null, null, null);
        }

        Renderer[] FindOriginalMeshes(Transform parent) =>
            parent.GetComponentsInChildren<Renderer>(true)
                .Where(r => (!stagedDisplay || !r.transform.IsChildOf(stagedDisplay)) &&
                    (r is SkinnedMeshRenderer || r is MeshRenderer && !r.GetComponent<TextMesh>())).ToArray();

        void SetDisplayTransform(Transform body, Transform parent)
        {
            body.SetParent(parent, false);
            body.localPosition = Vector3.zero;
            body.localRotation = Quaternion.identity;
            var scale = parent.lossyScale;
            float size = Mathf.Max(.35f, heightScale);
            body.localScale = new Vector3(size / Mathf.Max(.01f, Mathf.Abs(scale.x)),
                size / Mathf.Max(.01f, Mathf.Abs(scale.y)), size / Mathf.Max(.01f, Mathf.Abs(scale.z)));
            foreach (var child in body.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = parent.gameObject.layer;

            // The imported skeleton's hips provide a facing axis independent of FBX axis conversion.
            var skin = body.GetComponentInChildren<SkinnedMeshRenderer>();
            if (!skin) return;
            var right = Array.Find(skin.bones, b => b && b.name == "thigh.R");
            var left = Array.Find(skin.bones, b => b && b.name == "thigh.L");
            if (!right || !left) return;
            var forward = Vector3.ProjectOnPlane(Vector3.Cross(right.position - left.position, parent.up), parent.up);
            if (forward.sqrMagnitude > .001f)
                body.localRotation = Quaternion.Euler(0, Vector3.SignedAngle(forward, parent.forward, parent.up), 0);
        }
        static Transform FindBone(Animator animator, Transform[] bones, string kitName, HumanBodyBones body)
        {
            if (!animator) return null;
            if (animator.isHuman) return animator.GetBoneTransform(body);
            string human = body.ToString();
            string alternate = kitName switch
            {
                "hips" => "pelvis", "spine" => "chest",
                "upper_arm.R" => "rightarm", "upper_arm.L" => "leftarm",
                "forearm.R" => "rightforearm", "forearm.L" => "leftforearm",
                "thigh.R" => "rightupleg", "thigh.L" => "leftupleg",
                "shin.R" => "rightleg", "shin.L" => "leftleg",
                _ => kitName.Replace(".", "")
            };
            return Array.Find(bones, t =>
            {
                string clean = t.name.Substring(t.name.LastIndexOf(':') + 1).Replace("_", "").Replace(".", "");
                return string.Equals(clean, human, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(clean, alternate, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(t.name, kitName, StringComparison.OrdinalIgnoreCase);
            });
        }

        void LateUpdate()
        {
            if (!display) return;
            if (joints != null) CopyPose();
            else if (movingParameter)
            {
                var position = transform.position;
                var delta = Vector3.ProjectOnPlane(position - previousPosition, Vector3.up);
                displayAnimator.SetBool("Moving", delta.sqrMagnitude > Mathf.Pow(Time.deltaTime * .12f, 2));
                previousPosition = position;
            }
        }

        void CopyPose()
        {
            if (joints == null || !source) return;
            if (pelvis)
            {
                var current = source.transform.InverseTransformPoint(joints[0].from.position);
                var displacement = source.transform.TransformVector(current - sourcePelvisRest);
                pelvis.localPosition = pelvisRest + pelvis.parent.InverseTransformVector(Vector3.ClampMagnitude(displacement, .65f));
            }
            foreach (var joint in joints)
            {
                if (!joint.from || !joint.to) continue;
                var direction = joint.to.position - joint.from.position;
                joint.target.localRotation = joint.restRotation;
                if (direction.sqrMagnitude > .0001f)
                    joint.target.rotation = Quaternion.FromToRotation(joint.target.rotation * joint.axis, direction) * joint.target.rotation;
            }
            foreach (var wrist in wrists) wrist.Copy();
        }
    }
}
