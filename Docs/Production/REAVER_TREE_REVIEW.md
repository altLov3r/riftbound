# Reaver tree candidate disposition — CW03

Read-only current terrain-data/prototype diagnostics on production/codex-world-20261116. No tree, prefab, material, height, paint or collision changed. Twenty-six interpolated height-change candidates: **23 tree instances and 3 shrubs**. Stored root-to-interpolated-ground offset is zero for all 26. Prototype root up is not a rendered mesh-direction proof.

The table records each instance rather than treating an aggregate zero as individual visual acceptance. Early GPU batch checks stalled at frame1; a later capture-only fixture with editor error-pause handling produced actual rendered frames. Visible sampled groups show upright trunks and supported bases, but individual identification/base/collider acceptance for all 26 remains **NOT RUN**. Upper review views are partly obscured by foliage; camera-shape overlaps alone did not establish visual clearance. There is no demonstrated basis here for mass rotation/removal.

| Terrain instance | World X/Z | Prototype | Root up dot | Root-ground m | Reaver centreline distance m | Prefab colliders | Disposition |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 139 | (-290.63, 67.03) | T_BirchTree_1 | 1.000 | 0.000 | 6.83 | 0 | Grounded data; upright prototype; visual review pending |
| 140 | (-288.68, 68.86) | T_BirchTree_2 | 1.000 | 0.000 | 5.01 | 0 | Grounded data; upright prototype; visual review pending |
| 141 | (-291.65, 75.18) | T_NormalTree_2 | 1.000 | 0.000 | 8.43 | 0 | Grounded data; upright prototype; visual review pending |
| 142 | (-288.83, 77.26) | T_BirchTree_1 | 1.000 | 0.000 | 5.76 | 0 | Grounded data; upright prototype; visual review pending |
| 143 | (-288.04, 81.13) | T_MapleTree_1 | 1.000 | 0.000 | 5.94 | 0 | Grounded data; upright prototype; visual review pending |
| 180 | (-284.92, 70.53) | T_BirchTree_1 | 1.000 | 0.000 | 1.39 | 0 | Grounded data; upright prototype; visual review pending |
| 181 | (-284.47, 75.11) | T_MapleTree_1 | 1.000 | 0.000 | 1.26 | 0 | Grounded data; upright prototype; visual review pending |
| 182 | (-287.31, 77.29) | T_BirchTree_2 | 1.000 | 0.000 | 4.24 | 0 | Grounded data; upright prototype; visual review pending |
| 184 | (-287.68, 87.05) | T_Bush | 0.819 | 0.000 | 10.19 | 0 | Grounded data; leaning shrub prototype; visual review pending |
| 185 | (-287.90, 89.46) | T_MapleTree_2 | 1.000 | 0.000 | 12.47 | 0 | Grounded data; upright prototype; visual review pending |
| 209 | (-280.70, 67.26) | T_BirchTree_2 | 1.000 | 0.000 | 3.06 | 0 | Grounded data; upright prototype; visual review pending |
| 212 | (-281.54, 77.10) | T_Bush | 0.819 | 0.000 | 1.52 | 0 | Grounded data; leaning shrub prototype; visual review pending |
| 213 | (-281.28, 81.35) | T_NormalTree_2 | 1.000 | 0.000 | 3.76 | 0 | Grounded data; upright prototype; visual review pending |
| 214 | (-281.36, 86.30) | T_BirchTree_2 | 1.000 | 0.000 | 8.46 | 0 | Grounded data; upright prototype; visual review pending |
| 215 | (-280.45, 91.50) | T_BirchTree_1 | 1.000 | 0.000 | 13.74 | 0 | Grounded data; upright prototype; visual review pending |
| 235 | (-278.91, 66.18) | T_MapleTree_1 | 1.000 | 0.000 | 4.92 | 0 | Grounded data; upright prototype; visual review pending |
| 236 | (-278.09, 70.33) | T_BirchTree_1 | 1.000 | 0.000 | 5.44 | 0 | Grounded data; upright prototype; visual review pending |
| 237 | (-279.06, 87.05) | T_MapleTree_2 | 1.000 | 0.000 | 9.87 | 0 | Grounded data; upright prototype; visual review pending |
| 238 | (-279.94, 89.57) | T_MapleTree_2 | 1.000 | 0.000 | 11.97 | 0 | Grounded data; upright prototype; visual review pending |
| 254 | (-274.77, 70.05) | T_Bush | 0.819 | 0.000 | 8.77 | 0 | Grounded data; leaning shrub prototype; visual review pending |
| 255 | (-275.20, 73.96) | T_MapleTree_1 | 1.000 | 0.000 | 8.07 | 0 | Grounded data; upright prototype; visual review pending |
| 256 | (-273.43, 78.17) | T_BirchTree_1 | 1.000 | 0.000 | 9.58 | 0 | Grounded data; upright prototype; visual review pending |
| 257 | (-274.70, 86.35) | T_MapleTree_2 | 1.000 | 0.000 | 11.78 | 0 | Grounded data; upright prototype; visual review pending |
| 258 | (-275.44, 88.70) | T_MapleTree_1 | 1.000 | 0.000 | 13.10 | 0 | Grounded data; upright prototype; visual review pending |
| 273 | (-271.69, 78.87) | T_MapleTree_2 | 1.000 | 0.000 | 11.35 | 0 | Grounded data; upright prototype; visual review pending |
| 274 | (-270.47, 84.50) | T_BirchTree_2 | 1.000 | 0.000 | 14.12 | 0 | Grounded data; upright prototype; visual review pending |

Terrain tree rotation is yaw-only. Intentional log/stump meshes and leaning shrub roots must be distinguished from a fallen tree defect. The source terrain/prototype GUIDs, per-tree X/Z/scale/rotation and paint/details remain protected. Route controller probes supplement this table but do not verify every branch or foliage collider.
