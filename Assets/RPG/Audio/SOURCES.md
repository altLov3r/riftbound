# Audio sources and playback

Imported 2026-10-31 from Kenney's official free download pages:

- [RPG Audio](https://kenney.nl/assets/rpg-audio), Kenney Vleugels: knife slices, coin handling, footsteps and book/page foley.
- [Impact Sounds](https://kenney.nl/assets/impact-sounds), Kenney Vleugels: three punch/body impacts and two light metal impacts.
- [UI Audio](https://kenney.nl/assets/ui-audio), Kenney Vleugels: mouse/button clicks.

All three author pages identify the packs as **Creative Commons Zero (CC0)**. Original pack license files are retained beside this note. No paid assets or downloaded preview mixes are included. The 18 original OGG files are unmodified and total 197,310 bytes.

## Runtime mapping

- `AudioEffects`: recorded click, coin and page variations; recorded weapon swishes; movement-distance footsteps while grounded and on foot.
- `GameSound`: recorded body-hit variations and metal blocks, with original authored heal and rune cues. Procedural fallbacks remain available.
- Spell release, quest completion, discovery, level-up and boss victory have distinct original cues. Boss victory has its own playback cooldown.
- UI playback is limited to three voices. World playback is limited to eight spatial voices, four starts per frame, and per-cue cooldowns.
- World playback is suppressed during the character menu, pause and the short initial load guard. UI clicks remain available in menus. Teleports and mount movement do not emit walking footsteps.
- The existing master AudioListener setting remains authoritative. The existing SFX, music and ambience controls now scale their corresponding playback paths.
- Zone audio uses `ZoneManager.GetZoneName` for map boundaries. Emberwatch and Greenhollow share the exploration score, with separate stereo beds. Bloodrock, Deathknell, Wyrmcrest and the crypts have their own beds. Music and ambience crossfade independently after a short boundary hold, and combat brings in percussion while lowering the score and ambience.
- World audio pauses at the title and while the client waits for Emberford. Ambience and music retain separate volume controls.

## Original Riftbound audio

The WAV files in `Resources/RiftboundAudio/Authored` are original synthetic sound design for this project. The source renderer uses seeded noise, filtered transients, resonant bells, plucked strings and layered percussion. It uses no external samples. The existing Kenney pack attributions above apply to the original OGG files.

Render the 32 kHz PCM masters into a new directory with `python Tools/Audio/render_riftbound_audio.py --output <new-directory>`.

## Validation

Check the OGG assets import, then test clicks/quest hand-ins, melee hits, held Q blocks, coin loot, walking/running, mounting, pausing and character selection. Confirm SFX/music/ambience sliders and master mute. Runtime listening and final Unity compilation are handled by the integrating build; no claim of subjective audio acceptance is made by this source note.
