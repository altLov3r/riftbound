# Character art plan — 2026-11-10

Budget:free assets only. Priority:consistent character models before more gameplay features.

## Current mismatch
The project mixes NPCs_WoW humanoids, KayKit Skeletons and Brute-derived assets. The character builder also derives Dwarf/Elf variants from other bodies using scales and added accessories. Their head/body proportions, silhouettes and texture styles differ. Shared materials alone cannot resolve those geometry differences.

## Recommended family:Quaternius Universal
Use these related packs as a candidate pipeline for the existing stylized medieval world:

| Role | Official source | Reason |
|---|---|---|
| Character bodies/heads/hair | https://quaternius.itch.io/universal-base-characters | Humanoid base models; designed for the Universal Animation Library |
| Class clothing and armor | https://quaternius.itch.io/modular-character-outfits-fantasy | Modular parts authored for the Universal bodies |
| Locomotion/combat | https://quaternius.itch.io/universal-animation-library | Retargetable Humanoid animations; a Standard copy already exists in the project |
| Enemy candidates | https://quaternius.com/packs/ultimatemonsters.html | Animated monster pack; evaluate proportions against the chosen characters, retain native rigs where needed |

Choose the free Standard downloads. Source/Pro editions are paid. Promotional totals may include content outside the free tier:inspect the actual Standard archive before promising an outfit, race or animation. Official pages describe CC0 for the first three packs; retain the license supplied with each archive.

These are related, source-verified candidates, not a claim that every model has already passed Unity import. No complete free six-race wardrobe/rig set has been established by this research. Human/Elf styling is a starting point; convincing Orc, Dwarf, Goblin and Undead silhouettes may require additional compatible meshes and Blender work.

## Alternative:KayKit throughout
https://kaylousberg.itch.io/kaykit-adventurers
https://kaylousberg.itch.io/kaykit-character-animations

The free Adventurers and matching Medium/Large rig animation sets provide a coherent, more compact cartoon direction. This fits the existing KayKit skeletons better, but changes the overall look away from the taller WoW-like proportions. Verify versions:the project currently has Skeletons1.0, while current character packs target newer animation sets.

## Local AI option
- Inspected machine:Intel Arc A770 with16GB dedicated VRAM and16GB system RAM. Windows AdapterRAM reports only4GB for this card; the dedicated-memory registry values confirm16GB.
- Local AI is useful for matching reference images, prototype meshes, heads and equipment. Independently generated meshes do not guarantee compatible skeletons, deformation topology or consistent proportions.
- Recommended character workflow:consistent reference design -> generated prototype/sculpt guide -> Blender cleanup/retopology -> shared body/rig and skin weights -> Unity animation validation. Keep armor/weapons separate where they need to be swapped.
- TRELLIS.2 official requirements currently specify Linux, NVIDIA and24GB VRAM:https://github.com/microsoft/TRELLIS.2. No Intel Arc support was established by this check.
- TripoSR is an MIT-licensed image-to-mesh baseline with an explicit CPU fallback:https://github.com/VAST-AI-Research/TripoSR and https://github.com/VAST-AI-Research/TripoSR/blob/main/run.py. CPU speed and16GB system-RAM suitability remain unbenchmarked; this does not establish native Arc GPU acceleration.
- Hunyuan3D2.1 is not the default choice for an EU-based workflow:its published license excludes the EU, UK and South Korea. Source:https://github.com/Tencent-Hunyuan/Hunyuan3D-2.1/blob/main/LICENSE.
- First AI experiment should be one Orc prototype with a measured generation/cleanup/rigging result, before investing in a full roster. No AI environment, model weights or drivers were installed by this research.

## First integration milestone
1. Download candidates outside Assets into an art-review folder; record the archive contents and licenses.
2. Build a separate character test scene with one regular body, one broad body/race candidate, Warrior/Mage outfits, one villager and one enemy. Use the same ground, lighting and camera for all of them.
3. Test Idle, Walk, Run, Backpedal, Attack, Cast, Hit and Death. Check feet against the ground, forward direction, hands/weapons, animation loops and outfit clipping. Use in-place locomotion for the current CharacterController.
4. Show a lineup and motion preview at the gameplay camera distance. Only then migrate the remaining race/class visuals through CharacterVisualFactory.
5. Keep one material treatment/palette and one body-scale convention. Express classes through equipment and silhouettes; do not create a different skeleton for every class. Treat genuinely different creature rigs separately.

Current turn changes documentation only. Existing models and scenes have not been replaced. Unity tests were not rerun for this research/documentation task.
