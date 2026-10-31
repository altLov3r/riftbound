using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Riftbound
{
    [RequireComponent(typeof(CharacterController), typeof(Combatant))]
    public partial class PlayerController : MonoBehaviour
    {
        public Animator animator;
        public float attackDuration = 1.2f;
        public float cleaveDuration = 1.22f;
        public float walkSpeed = 1.5f;
        public float runSpeed = 5.5f;
        public float backwardMultiplier = .65f;
        public float walkStrideSpeed = .987f;
        public float runStrideSpeed = 2.415f;
        public float backRunStrideSpeed = 2.415f;
        public float backWalkStrideSpeed = .987f;
        public float walkPhase, runPhase, backWalkPhase, backRunPhase;
        public bool Walking { get; private set; }
        public OrbitCamera orbit;
        public Combatant Target { get; private set; }
        public float CleaveReady { get; private set; }
        public float HealReady { get; private set; }
        public int Kills { get; private set; }
        public bool QuestAccepted { get; private set; }
        public bool QuestComplete { get; private set; }
        public bool DialogueOpen { get; private set; }
        public QuestGiver ActiveQuestGiver { get; private set; }
        public TownNPC ActiveTownNPC { get; private set; }
        public int Gold { get; private set; }
        public Transform questGiver;
        public string Message { get; set; } = "Welcome to Emberwatch Outpost. Speak to Abe near the village gate.";
        public Combatant Stats { get; private set; }
        CharacterController motor;
        Vector3 spawn;
        float vertical, attackReady, busyUntil;
        string currentAnimation;
        LineRenderer targetRing;
        float sprintUntil;
        Vector3 moveVelocity;
        public float ClassResource { get; private set; } = 100f;
        public float MaxClassResource => CharacterClass == "Paladin" ? 5f : 100f;
        public string ClassResourceName => CharacterClass == "Warrior" ? "Rage" : CharacterClass == "Mage" ? "Mana" : CharacterClass == "Rogue" ? "Energy" : "Holy Power";
        public float EyeHeight
        {
            get
            {
                switch (CharacterRace)
                {
                    case "Dwarf": return 1.25f;
                    case "Goblin": return 1.15f;
                    case "Orc":
                    case "Brute": return 1.85f;
                    default: return 1.6f;
                }
            }
        }

        void Awake()
        {
            motor = GetComponent<CharacterController>();
            Stats = GetComponent<Combatant>();
            Stats.Damaged += OnDamaged;
            spawn = transform.position;
        }
        void Start()
        {
            if (!AudioEffects.Instance && !GetComponent<AudioEffects>()) gameObject.AddComponent<AudioEffects>();
            if (!ChatSystem.Instance && !GetComponent<ChatSystem>()) gameObject.AddComponent<ChatSystem>();
            if (!ZoneManager.Instance && !GetComponent<ZoneManager>()) gameObject.AddComponent<ZoneManager>();
            if (!ZoneBanner.Instance && !GetComponent<ZoneBanner>()) gameObject.AddComponent<ZoneBanner>();
            if (!MinimapRadar.Instance && !GetComponent<MinimapRadar>()) gameObject.AddComponent<MinimapRadar>();
            if (!CombatTextManager.Instance && !GetComponent<CombatTextManager>()) gameObject.AddComponent<CombatTextManager>();
            if (!LootDropSystem.Instance && !GetComponent<LootDropSystem>()) gameObject.AddComponent<LootDropSystem>();
            if (!CastBarSystem.Instance && !GetComponent<CastBarSystem>()) gameObject.AddComponent<CastBarSystem>();
            if (!BuffSystem.Instance && !GetComponent<BuffSystem>()) gameObject.AddComponent<BuffSystem>();
            if (!PartySystem.Instance && !GetComponent<PartySystem>()) gameObject.AddComponent<PartySystem>();
            if (!DungeonInstanceManager.Instance && !GetComponent<DungeonInstanceManager>()) gameObject.AddComponent<DungeonInstanceManager>();
            if (!DuelSystem.Instance && !GetComponent<DuelSystem>()) gameObject.AddComponent<DuelSystem>();
            if (!LootRollSystem.Instance && !GetComponent<LootRollSystem>()) gameObject.AddComponent<LootRollSystem>();
            if (!MailboxSystem.Instance && !GetComponent<MailboxSystem>()) gameObject.AddComponent<MailboxSystem>();
            if (!BattlegroundSystem.Instance && !GetComponent<BattlegroundSystem>()) gameObject.AddComponent<BattlegroundSystem>();
            if (!GuildSystem.Instance && !GetComponent<GuildSystem>()) gameObject.AddComponent<GuildSystem>();
            if (!CharacterSheetSystem.Instance && !GetComponent<CharacterSheetSystem>()) gameObject.AddComponent<CharacterSheetSystem>();
            if (!AchievementSystem.Instance && !GetComponent<AchievementSystem>()) gameObject.AddComponent<AchievementSystem>();
            if (!ReputationSystem.Instance && !GetComponent<ReputationSystem>()) gameObject.AddComponent<ReputationSystem>();
            if (!EncounterJournal.Instance && !GetComponent<EncounterJournal>()) gameObject.AddComponent<EncounterJournal>();
            // Loading a character can publish zone notifications and use these services.
            var frontend = GetComponent<FrontendMenu>() ?? gameObject.AddComponent<FrontendMenu>();
            frontend.Initialize(this);
            targetRing = new GameObject("Selected target ring").AddComponent<LineRenderer>();
            targetRing.useWorldSpace = true; targetRing.loop = true; targetRing.positionCount = 48;
            targetRing.startWidth = .055f; targetRing.endWidth = .055f;
            targetRing.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            targetRing.material.SetColor("_BaseColor", new Color(1, .6f, .15f));
            targetRing.enabled = false;
        }
        void LateUpdate()
        {
            PositionRider();
            if (!targetRing) return;
            targetRing.enabled = Target && Target.Alive;
            if (!targetRing.enabled) return;
            for (int i = 0; i < targetRing.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2 / targetRing.positionCount;
                targetRing.SetPosition(i, Target.transform.position + new Vector3(Mathf.Sin(angle) * .7f, .03f, Mathf.Cos(angle) * .7f));
            }
        }
        void OnDestroy()
        {
            if (Stats) Stats.Damaged -= OnDamaged;
            if (!targetRing) return;
            Destroy(targetRing.material); Destroy(targetRing.gameObject);
        }
        public bool TalentsOpen { get; set; }
        public bool SpellbookOpen { get; set; }
        public bool AchievementOpen { get; set; }
        public bool ReputationOpen { get; set; }
        public bool EncounterJournalOpen { get; set; }
        public bool ChatOpen { get; set; }

        void Update()
        {
            UpdateClassResource();
            if (FlightMaster.IsAnyTaxiInFlight)
            {
                moveVelocity = Vector3.zero;
                vertical = 0f;
                return;
            }
            var keys = Keyboard.current;
            if (keys == null || FrontendMenu.IsOpen || (orbit && orbit.Paused)) return;
            if (UpdateMounts(keys)) return;
            if (UpdateUtilities(keys)) return;
            if (keys.mKey.wasPressedThisFrame && Stats.Alive && !DialogueOpen && !ShopOpen && !ChatOpen && Time.time >= busyUntil) { MapOpen = !MapOpen; InventoryOpen = false; JournalOpen = false; TalentsOpen = false; ProfessionSystem.IsOpen = false; }
            if (keys.jKey.wasPressedThisFrame && Stats.Alive && !DialogueOpen && !ShopOpen && !ChatOpen && Time.time >= busyUntil && !Blocking)
            {
                if (keys.shiftKey.isPressed)
                {
                    EncounterJournalOpen = !EncounterJournalOpen;
                    MapOpen = false; InventoryOpen = false; JournalOpen = false; TalentsOpen = false; ProfessionSystem.IsOpen = false; SpellbookOpen = false; AchievementOpen = false; ReputationOpen = false;
                }
                else
                {
                    JournalOpen = !JournalOpen;
                    MapOpen = false; InventoryOpen = false; EncounterJournalOpen = false; TalentsOpen = false; ProfessionSystem.IsOpen = false; SpellbookOpen = false; AchievementOpen = false; ReputationOpen = false;
                }
            }
            if (keys.nKey.wasPressedThisFrame && Stats.Alive && !DialogueOpen && !ShopOpen && !ChatOpen && Time.time >= busyUntil && !Blocking) { TalentsOpen = !TalentsOpen; MapOpen = false; InventoryOpen = false; JournalOpen = false; ProfessionSystem.IsOpen = false; }
            if (keys.kKey.wasPressedThisFrame && Stats.Alive && !DialogueOpen && !ShopOpen && !ChatOpen && Time.time >= busyUntil && !Blocking) { ProfessionSystem.IsOpen = !ProfessionSystem.IsOpen; MapOpen = false; InventoryOpen = false; JournalOpen = false; TalentsOpen = false; SpellbookOpen = false; }
            if (keys.pKey.wasPressedThisFrame && Stats.Alive && !DialogueOpen && !ShopOpen && !ChatOpen && Time.time >= busyUntil && !Blocking) { SpellbookOpen = !SpellbookOpen; MapOpen = false; InventoryOpen = false; JournalOpen = false; TalentsOpen = false; ProfessionSystem.IsOpen = false; AchievementOpen = false; }
            if (keys.yKey.wasPressedThisFrame && Stats.Alive && !DialogueOpen && !ShopOpen && !ChatOpen && Time.time >= busyUntil && !Blocking) { AchievementOpen = !AchievementOpen; MapOpen = false; InventoryOpen = false; JournalOpen = false; TalentsOpen = false; ProfessionSystem.IsOpen = false; SpellbookOpen = false; ReputationOpen = false; }
            if (keys.uKey.wasPressedThisFrame && Stats.Alive && !DialogueOpen && !ShopOpen && !ChatOpen && Time.time >= busyUntil && !Blocking) { ReputationOpen = !ReputationOpen; MapOpen = false; InventoryOpen = false; JournalOpen = false; TalentsOpen = false; ProfessionSystem.IsOpen = false; SpellbookOpen = false; AchievementOpen = false; }
            if (keys.gKey.wasPressedThisFrame && Stats.Alive && !DialogueOpen && !ShopOpen && !ChatOpen && Time.time >= busyUntil && !Blocking)
            {
                var gSys = GuildSystem.Instance ?? GetComponent<GuildSystem>();
                if (gSys)
                {
                    gSys.IsOpen = !gSys.IsOpen;
                    MapOpen = false; InventoryOpen = false; JournalOpen = false; TalentsOpen = false; ProfessionSystem.IsOpen = false; SpellbookOpen = false; AchievementOpen = false; ReputationOpen = false;
                }
            }
            if (ShopOpen || MapOpen || JournalOpen || EncounterJournalOpen || TalentsOpen || ChatOpen || ProfessionSystem.IsOpen || SpellbookOpen || AchievementOpen || ReputationOpen || FlightMaster.IsAnyTaxiMapOpen || (GuildSystem.Instance && GuildSystem.Instance.IsOpen))
            {
                moveVelocity = Vector3.zero; Play(Mounted ? "Ride" : "Idle"); if (animator) animator.speed = 1;
                GravityOnly();
                if (ShopOpen && (!CanTrade() || keys.eKey.wasPressedThisFrame)) CloseShop();
                return;
            }
            if (DialogueOpen)
            {
                moveVelocity = Vector3.zero;
                Play("Idle"); if (animator) animator.speed = 1;
                GravityOnly();
                bool withinRange = (ActiveQuestGiver != null && CanTalk()) || 
                                   (ActiveTownNPC != null && Vector3.Distance(transform.position, ActiveTownNPC.transform.position) < 4.5f);
                if (!withinRange || Stats.LastHit > dialogueOpenedAt || (Time.time > dialogueOpenedAt + 0.35f && keys.eKey.wasPressedThisFrame)) CloseDialogue();
                return;
            }
            if (!Stats.Alive)
            {
                GravityOnly();
                if (keys.rKey.wasPressedThisFrame) Respawn();
                return;
            }
            if (keys.tabKey.wasPressedThisFrame) SelectTarget();
            if (keys.iKey.wasPressedThisFrame) InventoryOpen = !InventoryOpen;
            if (keys.f5Key.wasPressedThisFrame) SaveAdventure();
            if (keys.digit4Key.wasPressedThisFrame) UsePotion();
            if (Target && (!Target.Alive || Vector3.Distance(transform.position, Target.transform.position) > 25)) Target = null;

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && !DialogueOpen && !ShopOpen && !MapOpen && !JournalOpen && !InventoryOpen && Camera.main)
            {
                var ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
                if (Physics.Raycast(ray, out var hit, 40f, ~(1 << 2), QueryTriggerInteraction.Ignore))
                {
                    var combatant = hit.collider.GetComponentInParent<Combatant>();
                    if (combatant && !combatant.isPlayer && combatant.Alive) Target = combatant;
                }
            }

            if (keys.cKey.wasPressedThisFrame && !DialogueOpen && !ShopOpen && !ChatOpen)
            {
                var csSys = CharacterSheetSystem.Instance ?? GetComponent<CharacterSheetSystem>();
                if (csSys) csSys.ToggleSheet();
            }
            if ((keys.slashKey != null && keys.slashKey.wasPressedThisFrame) || (keys.numpadDivideKey != null && keys.numpadDivideKey.wasPressedThisFrame)) Walking = !Walking;
            bool running = !Walking;

            bool rightMouseSteering = orbit && orbit.IsSteering;
            bool bothMiceDown = orbit && orbit.BothMouseButtonsPressed;

            bool forwardInput = keys.wKey.isPressed || keys.upArrowKey.isPressed || bothMiceDown;
            bool backInput = keys.sKey.isPressed || keys.downArrowKey.isPressed;
            bool leftInput = keys.aKey.isPressed || keys.leftArrowKey.isPressed;
            bool rightInput = keys.dKey.isPressed || keys.rightArrowKey.isPressed;

            bool isFlyingMount = Mounted && SelectedMount && SelectedMount.isFlying;

            // In WoW: Right-mouse turns the character directly to camera heading
            if (rightMouseSteering)
            {
                if (isFlyingMount)
                {
                    // 3D Flight Banking & Pitch Tilt
                    float flightPitch = Mathf.Clamp(-orbit.Pitch * 0.75f, -55f, 55f);
                    transform.rotation = Quaternion.Euler(flightPitch, orbit.Yaw, 0f);
                }
                else
                {
                    var camForward = orbit.transform.forward; camForward.y = 0;
                    if (camForward.sqrMagnitude > .001f) transform.rotation = Quaternion.LookRotation(camForward);
                }
            }
            else
            {
                // Keyboard turning when not holding right-click (responsive 220 deg/s)
                if (leftInput && !rightInput) transform.Rotate(0, -220f * Time.deltaTime, 0);
                else if (rightInput && !leftInput) transform.Rotate(0, 220f * Time.deltaTime, 0);
                if (isFlyingMount && transform.eulerAngles.x != 0)
                {
                    // Level out pitch smoothly when not right-mouse steering
                    var euler = transform.eulerAngles;
                    euler.x = Mathf.LerpAngle(euler.x, 0f, 8f * Time.deltaTime);
                    transform.eulerAngles = euler;
                }
            }

            // Direction calculation:
            Vector3 moveDirection = Vector3.zero;
            if (isFlyingMount && rightMouseSteering)
            {
                // Full 3D Flight Vector in direction of camera gaze
                if (forwardInput) moveDirection += orbit.transform.forward;
                if (backInput) moveDirection -= orbit.transform.forward;
                if (leftInput) moveDirection -= orbit.transform.right;
                if (rightInput) moveDirection += orbit.transform.right;
            }
            else
            {
                if (forwardInput) moveDirection += transform.forward;
                if (backInput) moveDirection -= transform.forward;

                if (rightMouseSteering)
                {
                    // A and D strafe when holding right-click
                    if (leftInput) moveDirection -= transform.right;
                    if (rightInput) moveDirection += transform.right;
                }
            }

            if (moveDirection.sqrMagnitude > 1f) moveDirection.Normalize();

            bool isBackpedaling = backInput && !forwardInput && (!rightMouseSteering || (!leftInput && !rightInput));
            float speed = Mounted && SelectedMount ? (running ? SelectedMount.runSpeed : SelectedMount.walkSpeed) : running ? runSpeed : walkSpeed;
            if (CharacterRace == "Human") speed *= 1.10f;
            if (Time.time < sprintUntil) speed *= 1.65f;
            if (isBackpedaling && !Mounted) speed *= backwardMultiplier;

            if (isFlyingMount)
            {
                if (running) speed = SelectedMount.flySpeed;
                if (keys.leftShiftKey.isPressed) speed *= 1.5f;

                float verticalFly = 0f;
                if (keys.spaceKey.isPressed) verticalFly = 7f;
                else if (keys.xKey.isPressed) verticalFly = -7f;
                else if (!motor.isGrounded) verticalFly = 0f;
                else verticalFly = -2f;

                // When holding right-click and pitching, vertical flight velocity is already in moveDirection
                if (rightMouseSteering && (forwardInput || backInput))
                {
                    vertical = verticalFly; // Space and X still provide additional lift/dive
                }
                else
                {
                    vertical = verticalFly;
                }
            }
            else
            {
                if (motor.isGrounded && vertical < 0) vertical = -2;
                if (keys.spaceKey.wasPressedThisFrame && motor.isGrounded && Time.time >= busyUntil && !Blocking) vertical = 6.4f;
                vertical += Physics.gravity.y * 2.1f * Time.deltaTime;
            }

            bool attacking = Time.time < busyUntil || Blocking;
            var desiredVelocity = attacking ? Vector3.zero : moveDirection * speed;

            // In WoW: Instant acceleration/deceleration response (no sliding inertia)
            moveVelocity = desiredVelocity;
            if (Mounted && !MountPathClear(moveVelocity)) moveVelocity = Vector3.zero;
            if (motor && motor.enabled)
                motor.Move((moveVelocity + Vector3.up * vertical) * Time.deltaTime);
            EnsureAboveTerrain(isFlyingMount);
            if (transform.position.x > -200f && transform.position.y < -10) Respawn();

            var planarVelocity = motor.velocity; planarVelocity.y = 0;
            float actualSpeed = planarVelocity.magnitude;
            if (Mounted)
            {
                Play("Ride"); if (animator) animator.speed = 1;
                mountInstance.Animate(actualSpeed, !motor.isGrounded, Walking);
            }
            else if (!attacking)
            {
                bool airborne = !motor.isGrounded;
                bool movingBack = isBackpedaling || Vector3.Dot(planarVelocity, transform.forward) < -.15f;
                bool animationRun = running;
                string movementState = animationRun ? (movingBack ? "RunBack" : "Run") : (movingBack ? "WalkBack" : "Walk");
                Play(airborne ? "Jump" : actualSpeed > .2f ? movementState : "Idle");
                if (animator)
                {
                    float stride = animationRun ? (movingBack ? backRunStrideSpeed : runStrideSpeed) : (movingBack ? backWalkStrideSpeed : walkStrideSpeed);
                    float playback = !airborne && actualSpeed > .2f ? Mathf.Clamp(actualSpeed / Mathf.Max(.1f, stride), .75f, 1.4f) : 1f;
                    animator.speed = airborne || actualSpeed <= .2f ? 1f : playback;
                }
            }
            bool attack = keys.digit1Key.wasPressedThisFrame;
            if (attack && !Blocking && !InventoryOpen && motor.isGrounded) CastAbility1();
            if (keys.digit2Key.wasPressedThisFrame && !Blocking && !InventoryOpen && motor.isGrounded) CastAbility2();
            if (keys.digit3Key.wasPressedThisFrame) CastAbility3();
            if (keys.fKey.wasPressedThisFrame && !Blocking && !InventoryOpen) CastInterruptPublic();
            if (keys.eKey.wasPressedThisFrame) Interact();
        }
        float interruptReadyTime;
        public float InterruptCooldownRemaining => Mathf.Max(0f, interruptReadyTime - Time.time);
        public string InterruptSpellName => CharacterClass == "Warrior" ? "Pummel" : CharacterClass == "Rogue" ? "Kick" : CharacterClass == "Mage" ? "Counterspell" : "Rebuke";
        public string InterruptIcon => CharacterClass == "Warrior" ? "🛡" : CharacterClass == "Rogue" ? "🦶" : CharacterClass == "Mage" ? "✦" : "🔨";

        public void CastInterruptPublic()
        {
            if (Stats && !Stats.Alive) return;
            if (InterruptCooldownRemaining > 0f)
            {
                Message = $"{InterruptSpellName} is on cooldown ({Mathf.CeilToInt(InterruptCooldownRemaining)}s).";
                return;
            }

            if (!Target || !Target.Alive)
            {
                Message = "No target selected [Tab].";
                return;
            }

            float maxRange = CharacterClass == "Mage" ? 25f : 4.5f;
            if (Vector3.Distance(transform.position, Target.transform.position) > maxRange)
            {
                Message = $"Target out of range for {InterruptSpellName} ({maxRange}m).";
                return;
            }

            if (CastBarSystem.IsTargetCasting(Target))
            {
                bool success = CastBarSystem.InterruptTarget(Target, InterruptSpellName);
                if (success)
                {
                    interruptReadyTime = Time.time + 12f;
                    CombatTextManager.Spawn(Target.transform.position + Vector3.up * 1.5f, $"★ {InterruptSpellName.ToUpper()}! ★", Color.red, true);
                    AudioEffects.PlayBladeHit();
                    ChatSystem.LogCombat($"<color=#ff4040>Your [{InterruptSpellName}] INTERRUPTED {Target.displayName}!</color>");
                    Message = $"Interrupted {Target.displayName}!";
                }
                else
                {
                    Message = $"{Target.displayName}'s cast cannot be interrupted! [Shielded]";
                }
            }
            else
            {
                Message = $"{Target.displayName} is not casting.";
            }
        }
        public void CastAbility1Public() { if (!Blocking && !InventoryOpen && motor && motor.isGrounded) CastAbility1(); }
        public void CastAbility2Public() { if (!Blocking && !InventoryOpen && motor && motor.isGrounded) CastAbility2(); }
        public void CastAbility3Public() { CastAbility3(); }
        public void UsePotionPublic() { UsePotion(); }
        public void ToggleMapPublic()
        {
            if (Stats && Stats.Alive && !DialogueOpen && !ShopOpen && !ChatOpen)
            {
                MapOpen = !MapOpen;
                InventoryOpen = false;
                JournalOpen = false;
                TalentsOpen = false;
            }
        }
        float dialogueOpenedAt;
        public bool CanTalk() => Stats && Stats.Alive && questGiver && questGiver.gameObject.activeInHierarchy && Vector3.Distance(transform.position, questGiver.position) < 4;
        public void CloseDialogue() { DialogueOpen = false; ActiveQuestGiver = null; ActiveTownNPC = null; }
        public void OpenTownNPCDialogue(TownNPC npc)
        {
            if (!npc) return;
            if (Stats && !Stats.Alive) return;
            ActiveTownNPC = npc;
            ActiveQuestGiver = null;
            DialogueOpen = true;
            dialogueOpenedAt = Time.time;
            moveVelocity = Vector3.zero;
        }
        public void AcceptQuest()
        {
            if (!DialogueOpen || !CanTalk() || QuestAccepted || !ActiveQuestGiver) return;
            if (QuestComplete) return;
            QuestAccepted = true; Message = QuestObjective;
            AudioEffects.PlayQuestAccept();
            ChatSystem.LogQuest($"Quest Accepted: {QuestName}");
            CloseDialogue();
            RefreshAdventureWorld(); SaveAdventure(false);
        }
        public void CompleteQuest()
        {
            if (!DialogueOpen || !CanTalk() || !QuestReady || QuestComplete || !ActiveQuestGiver) return;
            int goldGain = QuestGold;
            int questExperience = QuestExperience;
            string completedQuest = QuestName;
            if (CharacterRace == "Human") goldGain = Mathf.RoundToInt(goldGain * 1.25f);
            Gold += goldGain; AddExperience(questExperience); Potions = Mathf.Min(10, Potions + 1);
            QuestIndex++; QuestAccepted = false; QuestComplete = QuestIndex >= 3;
            if (QuestComplete) { WeaponUpgraded = true; GrantItem("emberwatch-axe", true); GrantItem("emberwatch-sigil", true); }
            Stats.Heal(Stats.maxHealth);
            AudioEffects.PlayQuestComplete();
            ChatSystem.LogQuest($"Quest Completed: {completedQuest}! Rewards: {goldGain} Gold, {questExperience} XP.");
            Message = QuestComplete ? "Emberwatch is safe! Rewards: Emberwatch Axe and Sigil. [I]" : "Quest complete! Speak to Abe for your next mission.";
            CloseDialogue();
            RefreshAdventureWorld(); SaveAdventure(false);
        }

        void UpdateClassResource()
        {
            if (CharacterClass == "Warrior")
            {
                if (Time.time - Mathf.Max(Stats.LastHit, attackReady) > 4f)
                    ClassResource = Mathf.Max(0, ClassResource - 4f * Time.deltaTime);
            }
            else if (CharacterClass == "Mage")
            {
                ClassResource = Mathf.Min(MaxClassResource, ClassResource + 8f * Time.deltaTime);
            }
            else if (CharacterClass == "Rogue")
            {
                float regenRate = 20f + TalentSystem.GetEnergyRegenBonus(this);
                ClassResource = Mathf.Min(MaxClassResource, ClassResource + regenRate * Time.deltaTime);
            }
        }

        void CastAbility1()
        {
            if (Blocking || InventoryOpen || Time.time < attackReady) return;
            if (CharacterClass == "Mage")
            {
                if (ClassResource < 15) { Message = "Not enough Mana!"; return; }
                if (!Target) SelectTarget();
                if (!Target || Vector3.Distance(transform.position, Target.transform.position) > 28f)
                {
                    Message = "Target is out of range (28m). TAB selects the nearest target.";
                    return;
                }
                ClassResource -= 15;
                var dir = Target.transform.position - transform.position; dir.y = 0;
                if (dir.sqrMagnitude > .01f) transform.rotation = Quaternion.LookRotation(dir);
                attackReady = Time.time + 1.25f;
                busyUntil = Time.time + 0.6f;
                Play("Attack", true);
                float dmg = 36 + (Level - 1) * 4 + WeaponBonus;
                if (CharacterRace == "Brute") dmg *= 1.15f;
                FireballSpell.Cast(this, Target, dmg);
                Message = "Fireball launched!";
            }
            else if (CharacterClass == "Rogue")
            {
                if (ClassResource < 40) { Message = "Not enough Energy!"; return; }
                ClassResource -= 40;
                StartCoroutine(Attack(false, 0.55f, 20f, "Sinister Strike!"));
            }
            else if (CharacterClass == "Paladin")
            {
                ClassResource = Mathf.Min(MaxClassResource, ClassResource + 1);
                StartCoroutine(Attack(false, 1.1f, 26f, "Crusader Strike! (+1 Holy Power)"));
            }
            else // Warrior
            {
                ClassResource = Mathf.Min(MaxClassResource, ClassResource + 15);
                StartCoroutine(Attack(false, attackDuration, 24f, "Strike! (+15 Rage)"));
            }
        }

        void CastAbility2()
        {
            if (Blocking || InventoryOpen || Time.time < CleaveReady || Time.time < attackReady) return;
            if (CharacterClass == "Mage")
            {
                if (ClassResource < 25) { Message = "Not enough Mana!"; return; }
                ClassResource -= 25;
                CleaveReady = Time.time + 6f;
                busyUntil = Time.time + 0.5f;
                Play("Cleave", true);
                SpellFX.CreateFrostNova(transform.position, 6.5f);
                float dmg = 32 + (Level - 1) * 3 + WeaponBonus;
                if (CharacterRace == "Brute") dmg *= 1.15f;
                foreach (var enemy in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
                {
                    if (Vector3.Distance(transform.position, enemy.transform.position) < 6.5f) {
                        var eCombatant = enemy.GetComponent<Combatant>();
                        Hit(eCombatant, dmg);
                        BuffSystem.ApplyTargetDebuff(eCombatant, "frostbite", "Frostbite", "Movement slowed by 60%.", "❄", new Color(0.3f, 0.8f, 1f), 5f, 0f); }
                }
                Message = "Frost Nova: Freezing blast!";
            }
            else if (CharacterClass == "Rogue")
            {
                if (ClassResource < 35) { Message = "Not enough Energy!"; return; }
                if (!Target || Vector3.Distance(transform.position, Target.transform.position) > 3.5f)
                {
                    Message = "Eviscerate requires a target in melee range.";
                    return;
                }
                ClassResource -= 35;
                CleaveReady = Time.time + 4f;
                attackReady = Time.time + 0.6f;
                busyUntil = Time.time + 0.5f;
                Play("Cleave", true);
                Hit(Target, 52);
                Message = "Eviscerate! Critical finisher!";
            }
            else if (CharacterClass == "Paladin")
            {
                if (ClassResource < 2) { Message = "Requires 2 Holy Power!"; return; }
                ClassResource -= 2;
                CleaveReady = Time.time + 6f;
                busyUntil = Time.time + 0.5f;
                Play("Cleave", true);
                SpellFX.CreateConsecration(transform.position, 5.5f);
                float dmg = 40 + (Level - 1) * 3 + WeaponBonus;
                if (CharacterRace == "Brute") dmg *= 1.15f;
                foreach (var enemy in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
                {
                    if (Vector3.Distance(transform.position, enemy.transform.position) < 5.5f) {
                        var eCombatant = enemy.GetComponent<Combatant>();
                        Hit(eCombatant, dmg);
                        BuffSystem.ApplyTargetDebuff(eCombatant, "frostbite", "Frostbite", "Movement slowed by 60%.", "❄", new Color(0.3f, 0.8f, 1f), 5f, 0f); }
                }
                Message = "Consecration: Holy ground!";
            }
            else // Warrior
            {
                if (ClassResource >= 20) ClassResource -= 20;
                StartCoroutine(Attack(true, cleaveDuration, 38f, "Whirlwind!"));
            }
        }

        void CastAbility3()
        {
            if (Time.time < HealReady) return;
            if (CharacterClass == "Mage")
            {
                if (ClassResource < 20) { Message = "Not enough Mana!"; return; }
                ClassResource -= 20;
                HealReady = Time.time + 10f;
                motor.Move(transform.forward * 7.5f);
                Stats.Heal(40);
                SpellFX.CreateFrostNova(transform.position, 3f);
                Message = "Blink & Arcane Ward: Teleported & shielded 40 HP!";
            }
            else if (CharacterClass == "Rogue")
            {
                if (ClassResource < 20) { Message = "Not enough Energy!"; return; }
                ClassResource -= 20;
                HealReady = Time.time + 10f;
                sprintUntil = Time.time + 5f;
                attackReady = Time.time;
                Message = "Sprint: +65% speed for 5 seconds!";
            }
            else if (CharacterClass == "Paladin")
            {
                if (ClassResource < 3) { Message = "Requires 3 Holy Power!"; return; }
                ClassResource -= 3;
                HealReady = Time.time + 12f;
                Stats.Heal(60);
                SpellFX.CreateConsecration(transform.position, 2f);
                Message = "Holy Light: restored 60 health.";
            }
            else // Warrior
            {
                HealReady = Time.time + 12f;
                Stats.Heal(45);
                Message = "Second Wind: restored 45 health.";
            }
        }

        IEnumerator Attack(bool cleave, float duration = -1f, float damage = 24f, string message = "")
        {
            if (duration < 0) duration = cleave ? cleaveDuration : attackDuration;
            if (!Target) SelectTarget();
            var victim = Target;
            if (!cleave && (!Target || Vector3.Distance(transform.position, Target.transform.position) > 3.2f))
            { Message = "Move closer to an enemy. TAB selects the nearest target."; yield break; }
            if (Target)
            {
                var direction = Target.transform.position - transform.position; direction.y = 0;
                if (direction.sqrMagnitude > .01f) transform.rotation = Quaternion.LookRotation(direction);
            }
            attackReady = Time.time + duration + .08f;
            if (cleave) CleaveReady = Time.time + Mathf.Max(1.5f, 5f - TalentSystem.GetWhirlwindCooldownReduction(this));
            busyUntil = Time.time + duration * .78f;
            if (animator) animator.speed = 1;
            Play(cleave ? "Cleave" : "Attack", true);
            yield return new WaitForSeconds(duration * .45f);
            if (!Stats.Alive) yield break;
            if (cleave)
            {
                foreach (var enemy in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
                    if (Vector3.Distance(transform.position, enemy.transform.position) < 3.8f) Hit(enemy.GetComponent<Combatant>(), damage);
                Message = string.IsNullOrEmpty(message) ? "Whirlwind!" : message;
            }
            else if (victim && Vector3.Distance(transform.position, victim.transform.position) < 3.5f && Vector3.Dot(transform.forward, (victim.transform.position - transform.position).normalized) > .1f)
            {
                Hit(victim, damage);
                if (!string.IsNullOrEmpty(message)) Message = message;
            }
        }
        void Hit(Combatant enemy, float damage)
        {
            if (!enemy || !enemy.Alive) return;
            float finalDmg = damage + (Level - 1) * 3 + WeaponBonus;
            if (CharacterRace == "Brute") finalDmg *= 1.15f;
            finalDmg *= TalentSystem.GetDamageMultiplier(this);

            bool isCrit = UnityEngine.Random.value < (CharacterClass == "Rogue" ? 0.28f : 0.16f);
            if (isCrit) finalDmg *= 1.8f;

            AudioEffects.PlayBladeHit();
            CombatTextManager.SpawnDamage(enemy.transform.position, finalDmg, isCrit);
            string critMsg = isCrit ? " [CRITICAL!]" : "";
            ChatSystem.LogCombat($"Your strike hits {enemy.displayName} for {Mathf.RoundToInt(finalDmg)} Physical damage{critMsg}.");
            enemy.Damage(finalDmg);
            if (CharacterClass == "Warrior")
                BuffSystem.ApplyTargetDebuff(enemy, "rend", "Rend", "Suffering 6 Physical bleed damage every second.", "🩸", new Color(0.85f, 0.2f, 0.2f), 5f, 6f, 1f);
            else if (CharacterClass == "Rogue")
                BuffSystem.ApplyTargetDebuff(enemy, "poison", "Deadly Poison", "Inflicted with 5 Nature poison damage every second.", "🧪", new Color(0.3f, 0.85f, 0.3f), 6f, 5f, 1f);
            else if (CharacterClass == "Paladin")
                BuffSystem.ApplyTargetDebuff(enemy, "judgement", "Judgement", "Weakened against Holy power for 6 seconds.", "✝", new Color(1f, 0.85f, 0.3f), 6f, 4f, 1.5f);
            else if (CharacterClass == "Mage")
                BuffSystem.ApplyTargetDebuff(enemy, "ignite", "Ignite", "Burning for 8 Fire damage every second.", "🔥", new Color(1f, 0.45f, 0.1f), 4f, 8f, 1f);
            if (CharacterRace == "Undead" && Stats && Stats.Alive) Stats.Heal(6);
        }
        public void SelectTarget()
        {
            var targets = FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Select(e => e.GetComponent<Combatant>()).Where(s => s.Alive && Vector3.Distance(transform.position, s.transform.position) < 22).OrderBy(s => (s.transform.position - transform.position).sqrMagnitude).ToArray();
            int index = System.Array.IndexOf(targets, Target);
            Target = targets.Length == 0 ? null : targets[(index + 1) % targets.Length];
        }
        public void SetTarget(Combatant newTarget)
        {
            Target = newTarget;
        }
        public static float SampleSafeGroundY(Vector3 worldPos, float clearance = 0.15f)
        {
            if (worldPos.x < -200f || worldPos.y < -20f || worldPos.y >= 40f)
                return worldPos.y;

            float groundY = worldPos.y;
            bool hasTerrainSample = false;

            var terrain = Terrain.activeTerrain ?? FindFirstObjectByType<Terrain>();
            if (terrain != null && terrain.terrainData != null)
            {
                Vector3 tp = terrain.transform.position;
                Vector3 ts = terrain.terrainData.size;
                if (worldPos.x >= tp.x && worldPos.x <= tp.x + ts.x &&
                    worldPos.z >= tp.z && worldPos.z <= tp.z + ts.z)
                {
                    groundY = terrain.SampleHeight(worldPos) + tp.y;
                    hasTerrainSample = true;
                }
            }

            float rayTop = (hasTerrainSample ? Mathf.Max(worldPos.y, groundY) : worldPos.y) + 12f;
            var hits = Physics.RaycastAll(new Vector3(worldPos.x, rayTop, worldPos.z), Vector3.down, 24f, ~0, QueryTriggerInteraction.Ignore);
            float bestDeckY = float.NegativeInfinity;
            foreach (var h in hits)
            {
                if (h.collider is CharacterController || h.collider is CapsuleCollider) continue;
                var tr = h.collider.transform;
                if (tr.GetComponentInParent<PlayerController>() ||
                    tr.GetComponentInParent<EnemyController>() ||
                    tr.GetComponentInParent<TownNPC>() ||
                    tr.GetComponentInParent<FlightMaster>() ||
                    tr.GetComponentInParent<QuestGiver>() ||
                    tr.GetComponentInParent<Merchant>() ||
                    tr.GetComponentInParent<Wildlife>() ||
                    tr.GetComponentInParent<GatheringNode>())
                    continue;

                if (hasTerrainSample && h.point.y > groundY + 1.8f) continue;
                if (h.point.y > bestDeckY) bestDeckY = h.point.y;
            }

            if (!float.IsNegativeInfinity(bestDeckY))
                groundY = hasTerrainSample ? Mathf.Max(groundY, bestDeckY) : bestDeckY;

            return groundY + clearance;
        }

        public static Vector3 ClampAboveGround(Vector3 worldPos, float clearance = 0.15f)
        {
            if (worldPos.x < -200f || worldPos.y < -20f || worldPos.y >= 40f)
                return worldPos;

            float minSafeY = SampleSafeGroundY(worldPos, clearance);
            if (worldPos.y < minSafeY || worldPos.y > minSafeY + 2.5f)
                worldPos.y = minSafeY;
            return worldPos;
        }

        void EnsureAboveTerrain(bool isFlyingMount)
        {
            if (isFlyingMount || transform.position.x < -200f || transform.position.y >= 40f) return;
            var terrain = Terrain.activeTerrain;
            if (!terrain || !terrain.terrainData) return;

            Vector3 p = transform.position;
            Vector3 tp = terrain.transform.position;
            Vector3 ts = terrain.terrainData.size;
            if (p.x < tp.x || p.x > tp.x + ts.x || p.z < tp.z || p.z > tp.z + ts.z) return;

            float ty = terrain.SampleHeight(p) + tp.y;
            if (p.y < ty - 0.70f)
            {
                float safeY = SampleSafeGroundY(p, 0.25f);
                bool wasEnabled = motor && motor.enabled;
                if (wasEnabled) motor.enabled = false;
                transform.position = new Vector3(p.x, safeY, p.z);
                if (wasEnabled) motor.enabled = true;
                if (vertical < 0f) vertical = 0f;
            }
        }

        void Respawn()
        {
            Dismount();
            CloseDialogue();
            CloseShop(); MapOpen = false; JournalOpen = false; EncounterJournalOpen = false; InventoryOpen = false; TalentsOpen = false; SpellbookOpen = false; AchievementOpen = false; ReputationOpen = false; ChatOpen = false; ProfessionSystem.IsOpen = false; AuctionHouseSystem.AuctionOpen = false; AuctionHouseSystem.MailboxOpen = false; if (GuildSystem.Instance) GuildSystem.Instance.IsOpen = false; if (CharacterSheetSystem.Instance) CharacterSheetSystem.Instance.IsOpen = false;
            StopGuard(); ReturningHome = false;
            Vector3 safeSpawn = ClampAboveGround(spawn, 0.25f);
            StopAllCoroutines(); motor.enabled = false; transform.position = safeSpawn; motor.enabled = true;
            vertical = 0; moveVelocity = Vector3.zero; Stats.Restore(); Target = null; busyUntil = 0;
            if (animator) animator.speed = 1;
            Play("Idle", true);
            Message = "Recovered at the outpost. Your mission progress is kept for this session.";
        }
        public void Play(string state, bool force = false)
        {
            if (!animator || (!force && currentAnimation == state)) return;
            if (!force && Locomotion(currentAnimation) && Locomotion(state))
                animator.CrossFadeInFixedTime(state, .16f, 0, 0);
            else 
                animator.CrossFadeInFixedTime(state, .14f, 0, 0);
            currentAnimation = state;
        }
        static bool Locomotion(string state) => state == "Walk" || state == "WalkBack" || state == "Run" || state == "RunBack";
        float GaitPhase(string state) => state == "Walk" ? walkPhase : state == "Run" ? runPhase : state == "WalkBack" ? backWalkPhase : backRunPhase;
        void GravityOnly()
        {
            if (!motor || !motor.enabled) return;
            if (motor.isGrounded && vertical < 0) vertical = -2;
            vertical += Physics.gravity.y * 1.8f * Time.deltaTime;
            motor.Move(Vector3.up * vertical * Time.deltaTime);
            EnsureAboveTerrain(false);
        }
        void OnDamaged(float amount)
        {
            Dismount();
            CloseDialogue(); CloseShop(); MapOpen = false; JournalOpen = false; ProfessionSystem.IsOpen = false; AuctionHouseSystem.AuctionOpen = false;
            if (Blocking)
                CombatTextManager.SpawnBlock(transform.position);
            else
                CombatTextManager.Spawn(transform.position, $"-{Mathf.RoundToInt(amount)}", new Color(1.0f, 0.25f, 0.25f), false);
            ChatSystem.LogCombat($"You take {Mathf.RoundToInt(amount)} damage.");
            if (!Stats.Alive)
            {
                StopGuard(); ReturningHome = false;
                StopAllCoroutines(); moveVelocity = Vector3.zero; if (animator) animator.speed = 1; Play("Death", true);
                ChatSystem.LogSystem("You have died! Press [R] to respawn at Emberwatch Outpost.");
            }
            else if (!Blocking && motor.isGrounded)
            {
                if (Time.time >= busyUntil)
                {
                    if (animator) animator.speed = 1;
                    Play("Hit", true);
                    attackReady = Mathf.Max(attackReady, Time.time + .22f);
                }
            }
        }
    }
}
