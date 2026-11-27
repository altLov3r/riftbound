using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Riftbound.Editor
{
    public static class RiftIncursionChecks
    {
        public static void Run()
        {
            int pass = 0, fail = 0;
            void Check(string name, bool ok, string detail)
            {
                if (ok) { pass++; Debug.Log($"[RiftIncursionChecks] PASS {name}: {detail}"); }
                else { fail++; Debug.LogError($"[RiftIncursionChecks] FAIL {name}: {detail}"); }
            }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Safe site separation from major towns (>= 50m)
            var towns = new (string name, Vector3 pos)[]
            {
                ("Emberford", WorldRegions.Emberford),
                ("Highspire", WorldRegions.Highspire),
                ("Frostpeak", WorldRegions.Frostpeak),
                ("Silverglen", WorldRegions.Silverglen),
                ("Deathknell", WorldRegions.Deathknell),
                ("Rustwater", WorldRegions.Rustwater),
                ("Bloodrock", WorldRegions.Bloodrock),
                ("Stronghold", WorldRegions.Stronghold),
                ("Greenhollow", WorldRegions.Greenhollow),
            };
            float minDist = float.MaxValue;
            string closestPair = "";
            foreach (var site in RiftIncursionSystem.Sites)
            {
                foreach (var town in towns)
                {
                    float d = Vector2.Distance(new Vector2(site.Position.x, site.Position.z), new Vector2(town.pos.x, town.pos.z));
                    if (d < minDist) { minDist = d; closestPair = $"{site.Id}<->{town.name}"; }
                }
            }
            Check("sites_safe_from_towns", RiftIncursionSystem.Sites.Length >= 4 && minDist >= 50f,
                $"sites={RiftIncursionSystem.Sites.Length}, minDist={minDist:F1}m ({closestPair})");

            // Create minimal test harness (Player + Combatant + ProfessionSystem + RiftIncursionSystem)
            var playerGo = new GameObject("TestRiftPlayer");
            var cc = playerGo.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = 0.35f;
            var stats = playerGo.AddComponent<Combatant>();
            stats.ConfigureVitals(200, 0);
            stats.Restore();
            var player = playerGo.AddComponent<PlayerController>();
            var prof = playerGo.AddComponent<ProfessionSystem>();
            var rift = playerGo.AddComponent<RiftIncursionSystem>();

            // 2. Start Incursion -> Wave 1 Vanguard + SkyBeam visuals
            bool started = rift.StartIncursion(0);
            var beam = GameObject.Find("RiftSkyBeam");
            bool wave1Ok = started &&
                           rift.Phase == RiftIncursionSystem.IncursionPhase.Wave1Vanguard &&
                           rift.VanguardTotal == 3 &&
                           rift.VanguardDefeated == 0 &&
                           beam != null;
            Check("wave1_portal_and_vanguard_spawn", wave1Ok,
                $"started={started}, phase={rift.Phase}, vanguard={rift.VanguardTotal}, skyBeam={beam != null}");

            // 3. Defeat all 3 Vanguard -> Escalate to Wave 2 Rift Herald
            foreach (var v in rift.ActiveVanguard)
            {
                var c = v.GetComponent<Combatant>();
                if (c) c.Damage(9999f);
            }
            // Invoke Update via SendMessage to advance state machine
            rift.SendMessage("Update");
            bool wave2Ok = rift.Phase == RiftIncursionSystem.IncursionPhase.Wave2Herald &&
                           rift.ActiveHerald != null &&
                           rift.ActiveHerald.GetComponent<Combatant>().Alive;
            Check("wave2_herald_escalation", wave2Ok,
                $"phase={rift.Phase}, herald={(rift.ActiveHerald ? rift.ActiveHerald.enemyId : "null")}");

            // 4. Void Nova Telegraph Hit (inside 8m) vs Dodge (outside 8m)
            float hpBeforeHit = stats.Health;
            playerGo.transform.position = rift.ActiveHerald.transform.position + new Vector3(2f, 0f, 0f);
            rift.TriggerVoidNovaNowForTest();
            bool charging = rift.IsVoidNovaCharging;
            rift.ResolveVoidNovaNowForTest();
            float hpAfterHit = stats.Health;

            playerGo.transform.position = rift.ActiveHerald.transform.position + new Vector3(12f, 0f, 0f);
            rift.TriggerVoidNovaNowForTest();
            rift.ResolveVoidNovaNowForTest();
            float hpAfterDodge = stats.Health;

            bool novaOk = charging &&
                          rift.VoidNovasHitForTest == 1 &&
                          rift.VoidNovasDodgedForTest == 1 &&
                          hpAfterHit < hpBeforeHit &&
                          Mathf.Approximately(hpAfterDodge, hpAfterHit);
            Check("void_nova_hit_and_dodge", novaOk,
                $"charging={charging}, hits={rift.VoidNovasHitForTest}, dodges={rift.VoidNovasDodgedForTest}, hp={hpBeforeHit:F0}->{hpAfterHit:F0}->{hpAfterDodge:F0}");

            // 5. Defeat Herald -> Stabilized, rewards granted, reagents added, visuals cleaned up
            int goldBefore = player.Gold;
            int xpBefore = player.Experience;
            int ironBefore = prof.reagents.TryGetValue("Iron Ore", out int ib) ? ib : 0;
            int herbBefore = prof.reagents.TryGetValue("Kingsblood", out int hb) ? hb : 0;

            rift.ActiveHerald.GetComponent<Combatant>().Damage(99999f);
            rift.SendMessage("Update");

            int ironAfter = prof.reagents.TryGetValue("Iron Ore", out int ia) ? ia : 0;
            int herbAfter = prof.reagents.TryGetValue("Kingsblood", out int ha) ? ha : 0;
            bool portalCleaned = GameObject.Find("RiftSkyBeam") == null;

            bool completeOk = rift.Phase == RiftIncursionSystem.IncursionPhase.StabilizedCooldown &&
                              rift.TotalRiftsClosed == 1 &&
                              rift.HasRiftSurge &&
                              player.Gold == goldBefore + RiftIncursionSystem.RewardGold &&
                              (player.Experience >= xpBefore + RiftIncursionSystem.RewardXp || player.Level > 1) &&
                              ironAfter == ironBefore + 2 &&
                              herbAfter == herbBefore + 2 &&
                              portalCleaned;
            Check("rift_stabilized_rewards_and_cleanup", completeOk,
                $"phase={rift.Phase}, closed={rift.TotalRiftsClosed}, surge={rift.HasRiftSurge}, gold={goldBefore}->{player.Gold}, iron={ironBefore}->{ironAfter}, herb={herbBefore}->{herbAfter}, cleaned={portalCleaned}");

            Debug.Log($"[RiftIncursionChecks] PASS={pass} FAIL={fail}");
            if (fail > 0) EditorApplication.Exit(1);
        }
    }
}
