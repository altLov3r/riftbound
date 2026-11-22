"""Generate Emberford's original, tileable architectural albedo and normal maps."""

import argparse
from pathlib import Path

import numpy as np
from PIL import Image


SIZE = 1024
STYLES = ("plaster", "timber", "roof", "masonry")


def field(columns, rows, seed, u, v):
    lattice = np.random.default_rng(seed).random((rows, columns), dtype=np.float32)
    x, y = u * columns, v * rows
    ix, iy = np.floor(x).astype(np.int32), np.floor(y).astype(np.int32)
    wx, wy = (x - ix).astype(np.float32), (y - iy).astype(np.float32)
    wx = wx * wx * (3 - 2 * wx)
    wy = wy * wy * (3 - 2 * wy)
    x0, x1 = ix % columns, (ix + 1) % columns
    y0, y1 = iy % rows, (iy + 1) % rows
    lower = lattice[y0, x0] * (1 - wx) + lattice[y0, x1] * wx
    upper = lattice[y1, x0] * (1 - wx) + lattice[y1, x1] * wx
    return lower * (1 - wy) + upper * wy - .5


def finish(kind, u, v):
    broad = field(6, 7, 17, u, v)
    fine = field(184, 178, 21, u, v)
    if kind == "timber":
        warp = field(5, 6, 31, u, v) * .09
        grain = field(42, 6, 13, u + warp, v)
        fibers = field(177, 18, 43, u + warp, v)
        pores = np.minimum(fibers + .25, 0)
        shade = .92 + .10 * broad + .15 * grain + .085 * fibers + .12 * pores
        height = .69 * grain + .40 * fibers + .17 * fine
        return shade, height, (1., .987, .967), 5.7
    if kind == "plaster":
        trowel = field(19, 29, 7, u + broad * .025, v)
        pits = np.minimum(field(116, 122, 61, u, v) + .28, 0)
        shade = .919 + .082 * broad + .08 * trowel + .05 * fine + .16 * pits
        height = .44 * trowel + .32 * fine + .82 * pits
        return shade, height, (1., .996, .987), 5.2
    if kind == "roof":
        strata = field(12, 64, 59, u, v + broad * .018)
        speckle = field(99, 133, 27, u, v)
        shade = .92 + .10 * broad + .081 * strata + .06 * fine + .05 * speckle
        height = .46 * strata + .35 * speckle + .22 * fine
        return shade, height, (1., 1.004, 1.014), 5.2
    grain = field(46, 51, 73, u, v)
    chips = np.minimum(field(122, 111, 41, u + broad * .018, v) + .2, 0)
    shade = .919 + .12 * broad + .11 * grain + .065 * fine + .18 * chips
    height = .54 * grain + .39 * fine + .68 * chips
    return shade, height, (1., 1.002, 1.007), 5.6


def write_maps(destination):
    sample = (np.arange(SIZE, dtype=np.float32) + .5) / SIZE
    u, v = sample[None, :], sample[:, None]
    for kind in STYLES:
        shade, height, tint, strength = finish(kind, u, v)
        rgb = np.stack([shade * channel for channel in tint], axis=-1)
        dx = (np.roll(height, 1, axis=1) - np.roll(height, -1, axis=1)) * strength
        # PNG rows run from texture top to bottom, opposite Unity's V axis.
        dy = (np.roll(height, -1, axis=0) - np.roll(height, 1, axis=0)) * strength
        length = np.sqrt(dx * dx + dy * dy + 1)
        normal = np.stack((dx / length, dy / length, 1 / length), axis=-1) * .5 + .5
        for suffix, values in (("albedo", rgb), ("normal", normal)):
            path = destination / f"{kind}_{suffix}.png"
            pixels = np.rint(np.clip(values, 0, 1) * 255).astype(np.uint8)
            with path.open("xb") as output:
                Image.fromarray(pixels).save(output, format="PNG", optimize=True)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("destination", type=Path)
    args = parser.parse_args()
    args.destination.mkdir(parents=True, exist_ok=True)
    if any((args.destination / f"{kind}_{suffix}.png").exists()
           for kind in STYLES for suffix in ("albedo", "normal")):
        parser.error("choose a destination without existing house maps")
    write_maps(args.destination)
