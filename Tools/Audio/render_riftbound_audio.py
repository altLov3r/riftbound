"""Render Riftbound's original audio assets to a new directory (Python 3, stdlib)."""
import argparse
from array import array
import math
from pathlib import Path
import random
import sys
import wave

RATE = 32000
TAU = math.tau
FREQUENCIES = {n: 440 * 2 ** ((n - 69) / 12) for n in range(30, 100)}


def stereo(seconds):
    return [[0.0] * round(seconds * RATE) for _ in range(2)]


def add(track, start, seconds, pan, sound, wrap=False):
    origin = round(start * RATE)
    left, right = track
    gain_l = math.cos((pan + 1) * math.pi / 4)
    gain_r = math.sin((pan + 1) * math.pi / 4)
    for index in range(round(seconds * RATE)):
        position = origin + index
        if wrap:
            position %= len(left)
        elif position >= len(left):
            break
        value = sound(index / RATE, index)
        left[position] += value * gain_l
        right[position] += value * gain_r


def pluck(frequency, seconds, gain, bloom=False):
    def voice(t, _):
        if bloom:
            envelope = min(1, t / .19) * min(1, (seconds - t) / .42) * math.exp(-t * .55)
            phase = TAU * frequency * t + .16 * math.sin(TAU * 4.7 * t)
            return gain * envelope * (math.sin(phase) + .17 * math.sin(2 * phase) + .08 * math.sin(3 * phase))
        envelope = min(1, t / .012) * min(1, (seconds - t) / .10)
        phase = TAU * frequency * t
        harmonics = math.sin(phase) * math.exp(-3.8 * t)
        harmonics += .24 * math.sin(2 * phase) * math.exp(-7 * t)
        harmonics += .09 * math.sin(3.003 * phase) * math.exp(-13 * t)
        return gain * envelope * harmonics

    return voice


def bell(frequency, seconds, gain):
    def voice(t, _):
        phase = TAU * frequency * t
        envelope = min(1, t / .006) * min(1, (seconds - t) / .15)
        partials = math.sin(phase) * math.exp(-2.8 * t)
        partials += .38 * math.sin(2.07 * phase) * math.exp(-6 * t)
        partials += .14 * math.sin(3.73 * phase) * math.exp(-12 * t)
        return gain * envelope * partials

    return voice


def note(track, start, midi, seconds, pan, volume, voice=pluck, wrap=False):
    add(track, start, seconds, pan, voice(FREQUENCIES[midi], seconds, volume), wrap)


def bed(name, seed, gust, wildlife):
    seconds = 15
    rng = random.Random(seed)
    track = stereo(seconds)
    for channel, data in enumerate(track):
        slow = quick = 0
        for i in range(-RATE, len(data)):
            t = i / RATE
            white = rng.uniform(-1, 1)
            slow += .008 * (white - slow)
            quick += .085 * (white - quick)
            if i >= 0:
                drift = .70 + .18 * math.sin(TAU * t / 5 + channel) + .12 * math.sin(TAU * t / 15)
                data[i] = gust * drift * (slow * 1.4 + quick * .42)
        # Match the wind samples across the wrap without bringing the bed down to silence.
        start = data[0]
        overlap = RATE // 5
        correction = start - data[-1]
        for i in range(overlap):
            weight = .5 - .5 * math.cos(math.pi * i / (overlap - 1))
            data[len(data) - overlap + i] += correction * weight
    if wildlife:
        for start, pan, pitch in wildlife:
            for offset in (0, .17, .39):
                def bird(t, _, base=pitch, shift=offset):
                    phase = TAU * ((base + shift * 310) * t + 4700 * t * t - 22000 * t * t * t)
                    return .07 * math.sin(math.pi * t / .12) ** 2 * (math.sin(phase) + .17 * math.sin(2 * phase))
                add(track, start + offset, .12, pan, bird, True)
    if name == "deathknell":
        note(track, 4.1, 41, 2.5, -.6, .18, bell, True)
        note(track, 10.8, 41, 2.6, .45, .11, bell, True)
    if name == "crypt":
        for t, pan, midi in [(1.9, .55, 81), (4.8, -.7, 77), (9.2, -.15, 84), (12.5, .65, 82)]:
            note(track, t, midi, .63, pan, .15, bell, True)
            note(track, t + .19, midi, .55, -pan, .05, bell, True)
    return track


def emberwatch_score():
    track = stereo(24)
    chords = [(50, 57, 65, 69), (46, 53, 62, 65), (53, 60, 67, 69), (48, 55, 62, 64)]
    for bar, chord in enumerate(chords * 2):
        start = bar * 3
        for midi, pan, gain in ((chord[0], -.40, .26), (chord[1], .34, .15)):
            add(track, start, 3.6, pan, pluck(FREQUENCIES[midi], 3.6, gain, True), True)
        for tick, member in enumerate((1, 2, 3, 2, 1, 3)):
            note(track, start + tick * .5, chord[member], 1.3, -.46 + member * .22, .13, wrap=True)
    melody = [(1.5, 74), (2.5, 72), (3.5, 69), (5.0, 65), (7.0, 67), (8.5, 69),
              (10.5, 64), (12.5, 74), (14, 77), (15.5, 76), (16.5, 72), (18.5, 69),
              (20.5, 67), (22.5, 65)]
    for t, midi in melody:
        note(track, t, midi, 1.65, .19, .17, wrap=True)
        note(track, t + .24, midi, 1.10, -.6, .036, wrap=True)
    return track


def percussion():
    track = stereo(6)
    rng = random.Random(714)
    for t, accent, pan in [(0, 1, -.1), (.75, .45, -.45), (1.5, .82, .15), (2.625, .5, .48),
                           (3, .95, .02), (4.125, .56, -.55), (4.5, .77, .13), (5.625, .38, .45)]:
        def drum(d, _, gain=accent):
            body = math.sin(TAU * (48 * d + 1.5 * (1 - math.exp(-25 * d)))) * math.exp(-12 * d)
            thud = math.sin(TAU * 116 * d) * math.exp(-24 * d)
            skin = rng.uniform(-1, 1) * math.exp(-65 * d)
            return min(1, d / .004) * min(1, (.33 - d) / .04) * gain * (.65 * body + .14 * thud + .11 * skin)
        add(track, t, .33, pan, drum, True)
    return track


def spell():
    track = stereo(.72)
    rng = random.Random(370)
    low = 0

    def rush(t, _):
        nonlocal low
        low += .06 * (rng.uniform(-1, 1) - low)
        envelope = min(1, t / .09) * math.exp(-6 * t) * min(1, (.72 - t) / .09)
        phase = TAU * (135 * t + 430 * t * t)
        return envelope * (low * .8 + .19 * math.sin(phase) + .08 * math.sin(2.01 * phase))

    add(track, 0, .72, 0, rush)
    note(track, .16, 72, .51, .32, .24, bell)
    note(track, .24, 79, .46, -.39, .19, bell)
    return track


def stinger(melody, duration):
    track = stereo(duration)
    for t, midi, gain in melody:
        length = min(1.18, duration - t)
        note(track, t, midi, length, .25, gain, bell)
        note(track, t, midi - 12, length, -.24, gain * .47)
        if t + .12 < duration:
            note(track, t + .12, midi, min(1.02, duration - t - .12), -.65, gain * .18, bell)
    return track


def write(path, track, level, mono):
    if mono:
        track = [[(left + right) / 2 for left, right in zip(*track)]]
    peak = max(abs(sample) for channel in track for sample in channel)
    scale = level * 32767 / peak
    pcm = array("h", (round(sample * scale) for frame in zip(*track) for sample in frame))
    if sys.byteorder != "little":
        pcm.byteswap()
    with path.open("xb") as handle, wave.open(handle, "wb") as wav:
        wav.setparams((len(track), 2, RATE, 0, "NONE", "not compressed"))
        wav.writeframes(pcm.tobytes())


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", required=True, type=Path)
    root = parser.parse_args().output
    root.mkdir(parents=True, exist_ok=False)
    zones = [
        ("emberwatch", 777, .85, [(2.2, -.6, 1750), (8.9, .55, 1920)]),
        ("greenhollow", 347, .62, [(1.2, -.8, 2050), (5.6, .55, 1670), (10.65, -.25, 2170)]),
        ("bloodrock", 701, 1.28, []),
        ("deathknell", 970, .82, []),
        ("wyrmcrest", 440, 1.9, []),
        ("crypt", 260, .33, []),
    ]
    for name, seed, gust, wildlife in zones:
        write(root / f"bed_{name}.wav", bed(name, seed, gust, wildlife), .65, False)
    write(root / "score_emberwatch.wav", emberwatch_score(), .74, False)
    write(root / "score_combat.wav", percussion(), .75, False)
    write(root / "spell_release.wav", spell(), .79, True)
    cues = {
        "quest_complete": ([(0, 62, .55), (.14, 69, .55), (.25, 74, .52), (.29, 77, .24)], 1.33),
        "level_up": ([(0, 62, .48), (.13, 65, .41), (.26, 69, .44), (.40, 74, .54), (.47, 81, .34)], 1.65),
        "boss_victory": ([(0, 57, .58), (.21, 62, .56), (.40, 69, .56), (.55, 74, .65), (.58, 78, .30)], 1.82),
        "discovery": ([(0, 69, .62), (.24, 76, .51), (.36, 81, .37)], 1.4),
        "heal": ([(0, 67, .51), (.11, 74, .39), (.17, 79, .27)], .78),
        "rune": ([(0, 57, .63), (.18, 69, .45), (.30, 76, .31)], 1.08),
    }
    for name, (melody, duration) in cues.items():
        write(root / f"cue_{name}.wav", stinger(melody, duration), .70, name in ("heal", "rune"))


if __name__ == "__main__":
    main()
