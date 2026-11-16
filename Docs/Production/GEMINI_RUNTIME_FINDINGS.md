# Runtime integration findings for Gemini

Source: Codex production/codex-world-20261116, based on completed world overnight bef0420. These are observations on that world/runtime baseline, not new tests of Gemini's later runtime branch. Reproduce on your current source before changing it. No runtime files edited by Codex.

## GP02 — Reaver arrival / combat dismount

Queued W test through actual PlayerController and horse mount travelled from (-285,6,50) to last trace (-283.25,15.54,74.49), grounded, HP170. The strict scenario then failed its alive/mounted assertion before reaching camp centre (-283,78). Its version did not capture final HP/mount state separately; do not invent a confirmed kill or terrain failure.

PlayerController.OnDamaged unconditionally calls Dismount; this result is consistent with hostile contact/damage-dismount behavior near the camp. Review the intended mounted-arrival/combat behavior, test damage and local/remote mount synchronization, and distinguish a safe arrival point from walking into an occupied captain position. Do not disable AI, grant invulnerability, move the camp or flatten terrain to make this strict scenario green. All saved-scene geometry probes and the traversed part of the slope passed; complete normal-input camp combat acceptance remains pending.

## GP02 — foliage and camera

Reaver upper diagnostic view at camera (-285,18.5,69), looking at (-285,15.8,83), is partly obscured by nearby canopy/branches. A previous higher view was heavily occluded. The geometry survey had zero final sphere-overlap samples, demonstrating that collider clearance does not establish visual sightline clearance.

Reproduce using the real OrbitCamera near the approach/camp, mounted and on foot. If confirmed, propose a bounded local-camera solution (for example existing foliage-occlusion handling) or an exact world-placement request. Do not rotate/clear all trees or mutate shared materials globally. Codex preserved all vegetation.

## GP07 — startup overlay hierarchy

Native fresh test-character world frames show overlapping achievement/weather/player-status overlays during startup, including Craftsman's Calling without a crafting action in that diagnostic. Review whether thresholds/defaults and overlay timing are intended on your newer branch. This is an observed UX candidate, not an authority/persistence failure proven by these frames.

The earlier character-screen session-mode label clipping at 1280x720 remains a separate repro against Codex's older world runtime. Test current Gemini UI rather than assuming it still fails.

## Integration

The two serialized map-route arrays and their owned editor definitions are already synchronized by Codex. AdventurePanels reads WorldMapRoutes data; no duplicate runtime coordinate rewrite is necessary solely to adopt them. Preserve Codex's scene during authorized integration in a third checkout, rerun dependent checks and build the combined state. Separate branch results do not prove combined acceptance.
