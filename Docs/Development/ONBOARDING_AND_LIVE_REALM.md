# Emberford onboarding, controls, and live realm

Base is `development/emberford` at `3b95c4f`. Keep the existing offline campaign and private invite-code co-op playable. Build the public realm alongside them. First release target is one persistent realm with a closed test capped at four players, then measured capacity increases.

## 1. Movement presets

| Input | Modern (default) | Classic |
| --- | --- | --- |
| W / S on foot | Move relative to the camera, with backpedaling on S | Move forward or back relative to the character |
| A / D on foot | Strafe relative to the camera | Turn the character and camera during keyboard steering |
| Hold RMB | Steer with the camera and strafe with A / D | Steer with the camera and strafe with A / D |
| Hold LMB | Orbit while keeping the travel heading from when free-look began | Orbit without changing movement heading |
| Hold both mouse buttons | Move forward while steering | Move forward while steering |
| Home | Recenter camera yaw with mouse buttons released | Recenter camera yaw with mouse buttons released |
| Mounted | Existing keyboard and mouse steering | Same mounted controls |

Keep Q guard and the existing RMB + Q strafe behavior. Both presets use the existing motor, collision, movement speeds, jumps, and dodge rules. Save the preset locally, share the setting between title and pause menus, and render the current controls in the Controls page and tutorial. Full custom remapping can follow the two presets. Test W/S, A/D, arrows, held mouse buttons, backpedal, diagonal speed, guard, camera orbit, ground and flying mounts, and disabled input under menus.

## 2. First Adventure

Teach one action at a time, in the world the player will keep exploring. The first objective begins in the character's current safe starter area. Emberwatch is the first complete version. The other faction gets the same lessons through its own starter guide and local NPCs once Emberwatch's flow is validated.

1. On first entry, offer **Learn the basics** and **Skip tutorial**. Let players choose Modern or Classic here or change it from Settings at any time.
2. Teach movement with a nearby safe landmark. Confirm actual intentional movement, then ask the player to rotate the camera. Suppress progression from mounting, spawning, teleporting, or moving while a menu is open.
3. Highlight Abe and show **E** inside interaction range. Advance after the player opens the real dialogue and accepts the existing road-raider quest.
4. Teach targeting and the class's first ability during one low-risk encounter. Detect an accepted ability and a valid kill. Provide a short retry hint after death, with the objective retained.
5. Show **J** for the Adventure Guide and **M** for map guidance. Use existing quest tracking and keep a prior manual map pin until the player explicitly replaces it.
6. Return to Abe, turn in the existing quest, inspect the reward in inventory, and hand off to Stolen Supplies. Unlock later hints for mounts, grouping, and trading when the player reaches those features.

Use a compact objective card with one verb, one input hint, and one optional waypoint. Reuse the existing quest, combat, HUD, and save systems. Store versioned per-character lesson checkpoints with a manual replay option. Offline and realm saves remain separate. After a reload, derive steps already satisfied from authoritative quest progress so earlier saves skip completed lessons naturally. Tutorial steps do not mint rewards or advance quest flags. Test fresh characters, skip/replay, resume after quit, pre-existing quest states, different class abilities, both presets, death, and a real first-time player.

## 3. Dedicated realm

### Local mission test

- Add a Unity Linux Dedicated Server build target and a headless startup path using Netcode for GameObjects `StartServer`. Boot the campaign without a camera, UI, or local player. Keep terrain, colliders, spawn anchors, and the same settlement placements in both client and server builds.
- Introduce a realm client mode separate from Relay invitations. Initially bind the test server to loopback with a development-only fixture for two local players. Walk, attack, loot, disconnect, and rejoin while the server continues ticking.
- Move world snapshots, enemy actions, and world events off the host player's avatar channel onto a server-owned channel. Spawn players from explicit world anchors. Accept movement intent and enforce speed and collision server-side before exposing the realm. Keep render-only observer work off the server. Feature availability for solo-only quest chains remains explicit until each shared-world flow is designed and tested.

### Private persistent test realm

- Add a named realm entry with a status indicator, compatible build check, authenticated sign-in, and character selection. Use an established identity service and a realm-issued short-lived, single-use join ticket. Join credentials are carried over a protected transport and validated server-side. Anonymous sessions used for existing private Relay are independent.
- Store realm accounts, characters, inventory, quest progress, positions, moderation and role changes in PostgreSQL. Use stable IDs, versioned migrations, transactional grants, unique reward receipts, and single-writer character admission. Restore a valid position and progress after disconnect and process restart.
- Put the closed realm on one owner-controlled Linux host after local gates pass. Add clean shutdown, service restart, structured logs, health metrics, and off-host backups. Perform a restore drill and keep an independently deployable previous client/server build.

### Public access gates

- Two real PCs on different networks join one named realm without a player host. Exercise admission, movement, combat, loot, logout and rejoin, duplicate login, unauthorized staff requests, version mismatch, outage, backup restore, and restart with online progress intact.
- Measure bandwidth, tick time, CPU, memory, and disconnect rate under load before raising capacity. Start capped at four and expand to 8–16 only when the measurements justify it. Add distance-based replication and partitioning where profiling shows need.
- Add in-game reports, player rules, staff coverage, and a release/rollback procedure before opening registration. Public operation depends on approved hosting and service configuration.

## 4. Staff roles and permissions

| Role | Initial capabilities |
| --- | --- |
| Player | Play, group, chat, and submit reports |
| Guide | Player abilities plus answer newcomer questions and use a guide badge/chat channel |
| Moderator | Review reports, mute chat, and kick with a recorded reason |
| Game Master | Handle tickets, inspect relevant character state, move a stuck character to an approved location, and apply time-limited sanctions |
| Realm Administrator | Assign or revoke staff roles, manage permanent bans, and invoke approved realm operations |

Give each action a separate capability checked by the server at execution. Apply scoped, expiring grants and immediate revocation. Ordinary clients cannot choose a role or become staff by changing their UI. Start with ticket lookup, mute, kick, and stuck recovery. Every staff action records actor, target, reason, time, and result. Keep staff audit access restricted. Defer item grants and destructive moderation until an approval and audit workflow exists. Test role boundaries, revocation during a session, and attempts from an ordinary player.

## Delivery order

1. Add and test the movement presets.
2. Build the first movement/camera tutorial card and test skip, replay, and save/reload. Extend it through Abe's existing quest.
3. In parallel, prove headless boot and two-client world simulation. Keep this development build local until authenticated admission works.
4. Add realm accounts, durable characters, and permission checks. Run the private internet playtest.
5. Finish faction starter parity and the desktop UI, load-test the realm, and open public access after the release gates pass.

## References

- [Blizzard's new-player guide](https://worldofwarcraft.blizzard.com/news/24266319/new-players-starter-guide-welcome-to-world-of-warcraft)
- [Unity 6000.6 Dedicated Server builds](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/dedicated-server/build)
- [Unity Netcode connection approval and payload security](https://mp-docs.dl.it.unity3d.com/netcode/current/basics/connection-approval/)
