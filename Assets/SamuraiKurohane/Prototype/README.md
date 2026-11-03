# Imperial District — Gameplay Prototype

This scene is the current **2.5D environment + gameplay blockout** for *Samurai Kurohane*.

## Scene

Open:

`Assets/Scenes/ImperialDistrict_Prototype.unity`

Press **Play**. The `ImperialDistrictPrototype` component builds the environment and prototype combat arena at runtime.

## Current prototype scope

- side-scrolling 2.5D level, 76 units long, in three zones: **Outer Gate** (west), **Canal Bridge** (centre), **Palace Approach** (east, stairs to a gate hall)
- 6 reachable platforms with colliders, a fall-respawn, and level bounds
- 7 parallax depth layers: blood-moon sky, far/mid skyline, imperial palace, canal backdrop, gameplay lane, foreground branches and lanterns
- textured materials (stone brick, wood planks, roof tiles), emissive crimson neon trim, lantern halos, mist, light shafts, rain, splashes, petals and embers
- self-installing URP post-processing (bloom, vignette, grade, ACES)
- **Akio Kurohane** with a procedural character rig (kasa, layered armour, crimson feather emblem, physics cape, katana with energy edge) animated by code: idle, run, jump, land squash, dash, guard, parry, 3-hit combo, heavy, hit, guard break, death
- **Imperial Guard** enemies (3 in the arena) with kabuto, tabard and their own animations
- camera rig with look-ahead, shake, zoom punch and level clamping
- combat feel: hit-stop, sparks, slash arcs, light flashes, dust
- full HUD: health (with ghost bar), posture, dash gauge, combo counter, COUNTER prompt, enemy bars and telegraph warnings

## Prototype controls

### Keyboard / Mouse

| Action | Input |
|---|---|
| Move | A / D or Left / Right |
| Jump | Space |
| Dash | Left Shift |
| Light Attack | J / Left Mouse |
| Heavy Attack | K / Right Mouse |
| Parry | Q |
| Guard | L |

### Gamepad

| Action | Input |
|---|---|
| Move | Left Stick |
| Jump | South button |
| Dash | East button |
| Light Attack | West button |
| Heavy Attack | North button |
| Parry | Left Shoulder |
| Guard | Left Trigger |

## Combat prototype

Akio's combat is built around **precision, timing and posture pressure** rather than button mashing.

- **Light combo (3 hits)** with a small lunge and aim assist toward the nearest enemy; the dash can cancel attack recovery.
- **Heavy attack** deals more posture damage.
- **Guard** absorbs most health damage but costs posture; if posture empties, Akio suffers a guard break.
- **Parry** has a short window. A successful parry negates damage, wrecks the attacker's posture, shortens parry recovery and opens a **counter window**: the next attack deals x1.6 damage (shown as COUNTER).
- **Input buffer** (0.15 s), **coyote time** (0.10 s) and variable jump height keep movement forgiving.

Imperial Guards pace Akio and use three telegraphed attacks:

| Attack | Telegraph | Answer |
|---|---|---|
| Slash | 0.5 s | Parry or guard |
| Overhead | 0.85 s, flagged `!! DODGE !!` | **Unblockable**, must be dodged |
| Thrust | lunge | Parry, guard or dash |

Slash can chain into a quick follow-up. Broken posture leaves a guard open to a punish.

The prototype exists to answer one question:

> Is moving, attacking, guarding and parrying with Akio satisfying before final animation and art are introduced?

## Technical notes

Everything is still generated at runtime (geometry-first by design), but characters now use a procedural rig instead of capsules, so silhouettes and timing can be judged before final models exist.

Press **F1** in Play Mode to toggle the on-screen controls panel.

Scripts (`Assets/SamuraiKurohane/Prototype/`):

- `ImperialDistrictPrototype.cs` + `.Architecture.cs`, `.Atmosphere.cs`, `.Characters.cs`: level generator
- `AkioPrototypeController.cs`: player movement and combat
- `PrototypeCombatTarget.cs`: Imperial Guard AI and combat target
- `SamuraiVisualRig.cs`: procedural character models and animation
- `CombatFeedback.cs`: hit-stop, shake, sparks, slash arcs
- `PrototypeCameraRig.cs`: camera, parallax and drift helpers
- `PrototypeMaterials.cs`: materials and procedural textures
- `PrototypeHUD.cs`: runtime HUD
- `PrototypePostProcessing.cs`: URP volume installer

> Note: this pass was written and syntax-checked outside Unity. Expect to tune rig angles, particle materials and platform heights in Play Mode.

## Windows EXE build

The prototype includes a Windows x64 build command.

With Unity open, use:

`JM Game Studios > Build > Windows Prototype (.exe)`

The build is generated at:

`Builds/Windows/SamuraiKurohanePrototype/SamuraiKurohanePrototype.exe`

Alternatively, close Unity and run from PowerShell at the repository root:

`./Build-Windows.ps1`

The script targets Unity `6000.6.1f1` and writes the Unity batch-build log to:

`Builds/Windows/windows-build.log`

The build uses `ImperialDistrict_Prototype.unity` explicitly, so the old template SampleScene in Build Settings is not used for this prototype executable.

## Next milestone

After the prototype is validated in Play Mode:

1. tune movement and combat timing from hands-on testing;
2. replace Akio's primitive visual blockout with the approved character direction;
3. add animation-driven hit windows;
4. add Kurenai and Tsukiyomi;
5. begin the first vertical-slice encounter.
