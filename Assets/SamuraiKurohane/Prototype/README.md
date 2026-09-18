# Imperial District — Gameplay Prototype

This scene is the current **2.5D environment + gameplay blockout** for *Samurai Kurohane*.

## Scene

Open:

`Assets/Scenes/ImperialDistrict_Prototype.unity`

Press **Play**. The `ImperialDistrictPrototype` component builds the environment and prototype combat arena at runtime.

## Current prototype scope

The scene currently includes:

- side-scrolling 2.5D composition
- Japanese imperial / samurai-futurist environment blockout
- blood moon, palace skyline, torii gates and crimson gardens
- wet bridge combat lane and atmospheric rain
- playable **Akio Kurohane prototype**
- keyboard/mouse and gamepad prototype controls
- run, jump and dash
- three-hit light combo
- heavy attack
- guard
- timing-based parry
- health and posture
- respawn after defeat
- Training Samurai combat dummy
- enemy telegraph, strike, posture break and respawn
- simple runtime debug HUD

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

Akio's first combat pass is built around **precision, timing and posture pressure** rather than button mashing.

Light attacks use a three-step combo. Heavy attacks deal higher posture damage. Guard absorbs most health damage but loses posture. A successful parry negates damage and heavily damages the Training Samurai's posture.

The Training Samurai is deliberately simple. It exists to answer one question first:

> Is moving, attacking, guarding and parrying with Akio already satisfying before final animation and character art are introduced?

## Technical notes

This remains a geometry-first prototype. Akio and the Training Samurai use primitive geometry and code-driven combat timing so gameplay can be validated before final models, animations, VFX, sound design and authored hitboxes are introduced.

The runtime gameplay scripts are:

- `AkioPrototypeController.cs`
- `PrototypeCombatTarget.cs`
- `ImperialDistrictPrototype.cs`

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
