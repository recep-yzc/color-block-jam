# Color Block Jam — Vertical Slice

A vertical slice of Color Block Jam made in Unity 2022.3.62f2 (URP). It is a portrait game made for 1080×1920 that also lays out correctly on 20:9 screens.

Drag colored blocks around the board and slide each one out through a door of its own color before the timer runs out.

## How to run

1. Open the project with **Unity 2022.3.62f2**. Packages resolve from `Packages/manifest.json`: URP 14, Input System, UniTask, LitMotion, VContainer and TextMesh Pro.
2. Open `Assets/ColorBlockJam/Scenes/Splash.unity` and press **Play**. Set the Game view to a portrait resolution, such as 1080×1920 or 1080×2400.
3. To build, pick Android in *File › Build Settings*. The scenes are already in order: Splash, Main, Gameplay.
4. To run the tests, open *Window › General › Test Runner › EditMode*. There are 27 tests. Among other things, they cover the board rules, the drag movement (sliding, rolling around corners, never overlapping), the solver, the timer, the generator, and a check that every level in the catalog can be solved.

To reset progress and coins, use *Edit › Clear All PlayerPrefs*. The keys are `progression.currentLevel` and `economy.coins`.

### How to play

- Drag a block with the mouse or a finger. The block slides until it meets something. If you push it past a corner, it rounds the bevel on a curve.
- A block leaves the board when you push it into a door of its own color, but only if its whole shape fits through that door.
- Clear the board before the timer reaches zero. You win coins and move on to the next level.
- The level fails in two cases:
  - **Time's up**: the timer reaches zero.
  - **No moves left**: the board can no longer be cleared.
- The HUD has these buttons:
  - **Pause** opens Resume, Restart and Home.
  - **AUTO** lets the solver play the level from where you are.
  - **Boosters** are placeholders that only give touch feedback.

## Level editor

You can open it in three ways:

- the menu **Color Block Jam › Level Editor**,
- double-click a level file (`Features/Level/Data/Levels/LevelNN.json`),
- the **Open Level Editor** button on `LevelCatalog.asset`.

The window has three columns:

| Left | Center | Right |
|---|---|---|
| The levels of the catalog, in play order. Click one to load it. ▲ ▼ reorder, **Remove** takes a level out of the catalog (the file stays). | The board. The ring around it is the wall, and doors are drawn on it. | Board settings, tools, colors, checks and the generator. |

**Making a level**

1. Press **New**, or pick a level on the left.
2. Set **Width**, **Height**, **Time** and **Difficulty**.
3. Pick a **color**, then a **tool**:
   - **Draw**: drag over empty cells. One drag makes one block of any shape.
   - **Stamp**: pick a shape, then click a cell.
   - **Door**: click or drag along the wall. Clicking a door of the chosen color removes it.
   - **Move**: drag a block somewhere else. It turns red where it does not fit.
   - **Erase**: click a block or a door.

   Right-click erases with every tool. Keys 1–0 pick a color, Delete removes the selected block, and Ctrl+Z / Ctrl+Y undo and redo.
4. **Check** lists mistakes right away: a color without a door, a block too big for every door of its color, overlaps. It then runs the solver in the background. It tells you whether the level is solvable, in how many moves, and which difficulty it plays like. You can step through the solution on the board with ◀ ▶.
5. **Generate** makes a new solvable level of the chosen difficulty. The same seed always gives the same level.
6. **Save** writes over the level's file. **Save As New Level** adds a file at the end of the catalog. Before saving a level that has problems or has not been checked, the editor warns you.
7. **▶ Play** starts the gameplay scene with this level, without touching the player's progress.

**Format.** The editor and the game read the same JSON file (`LevelData`):

```json
{ "width": 6, "height": 7, "timeLimit": 95, "difficulty": 1,
  "blocks": [ { "color": 3, "x": 1, "y": 2, "cells": [ {"x":0,"y":0}, {"x":1,"y":0} ] } ],
  "doors":  [ { "side": 0, "start": 2, "length": 2, "color": 3 } ] }
```

Colors are indexes into `BlockPalette.asset`, which has 10 colors. `LevelCatalog.asset` lists the level files in play order; after the last level, the game starts again from the first. The 10 levels that ship with the game were made with the editor's generator and then checked by the solver:

- levels 1–3: easy,
- levels 4–6 and 8: medium,
- levels 7, 9 and 10: hard.

## Architecture

### Template and consumer

- `Assets/Framework` is a reusable template with no game knowledge. It contains:
  - **Core**: installers, scene loading, key-value storage and startup tasks.
  - **Boot**: the splash screen with its loading bar.
  - **UI**: views, presenters, popups, buttons with feedback, and transitions.
  - **Navigation**: tab pages.
  - **Settings**: toggles and haptics.
  - **Pooling**.
- `Assets/ColorBlockJam` is the game built on that template. It has App, Art, Features (Economy, Progression, Home, Level, Gameplay, LevelEditor), UI and Scenes.
- Every folder is an assembly definition. The framework never references the game, and the asmdefs enforce this. *Why:* the template can go into the next project unchanged, and each feature compiles and is tested on its own.

### Composition with VContainer

- The **App scope** (`App/AppScope.prefab`, the root scope) runs `ScriptableInstaller` assets for app-wide services: storage, scene loading, settings, economy and progression.
- Each **scene scope** runs `MonoInstaller` components for that scene's services.
- Each **popup** has its own child scope. It is created with the popup and disposed with it.
- *Why:* modules are added or removed in the Inspector, there are no singletons or static state, and a scene's objects live exactly as long as the scene.

### UI

- The UI uses **MVP**. A passive `UIView` shows data and raises events; a plain C# `ViewPresenter<TView>` holds the logic.
- How things look is a **strategy asset**:
  - `ViewTransition`: fade, scale, slide,
  - `ButtonFeedback`: scale, punch, jelly, wiggle, spin, tilt, bounce, heartbeat, pop,
  - `ToggleStateVisual`: objects, color, slide.
- Buttons derive from `ButtonBase` and only decide what a click does.
- Popups come from a `PopupCatalog`, are created on first open and are given their presenter by `PopupPresenterInstaller<TPopup, TPresenter>`.
- *Why:* designers can change feel and layout without code, and the presenters can be tested without a scene.

### Gameplay layers

| Layer | Where | What |
|---|---|---|
| Data | `Features/Level` | `LevelData` (JSON), `BlockPalette`, `LevelCatalog`. One format for the editor and the game. |
| Rules | `Features/Gameplay/Logic` (`noEngineReferences`) | `Board`, `BlockDragMover`, `BlockPlacement`, `BoardSolver`, `LevelGenerator`, `LevelDiagnostics`, `LevelTimer` |
| View and input | `Features/Gameplay/Scripts/Board`, `Input` | `BoardView`, `BlockView`, `BlockMeshBuilder`, `BlockDragController` (Input System), `BoardCamera`, `BlockBurstEffects` |
| Flow | `Features/Gameplay/Scripts/Session` | `LevelSession` (build, timer, win and fail), `AutoPlayer`, `LevelFlow`, `LevelProvider` |
| UI | `Features/Gameplay/Scripts/Hud`, `Popups` | HUD and the pause, fail and complete popups |

*Why:* the rules know nothing about Unity. The game, the level editor, the tests and worker threads all use the same code.

### Movement

Movement is fully algorithmic, with no physics engine, but it behaves like pushing a real object across a table:

- **The finger only sets a target.** Every frame the block catches up with it like a weight on a spring: fast when far, gently when close, never faster than a speed cap. It keeps catching up while the finger rests, so a quick flick is never left halfway.
- **Each step is swept** (`BlockDragMover`). The whole path of the frame is tested at once and stops at the first contact, so a fast drag never passes through anything.
- **The rest of the step slides along what it touched.** The part pushing into the surface is dropped and the rest is swept again, several times per frame. That is how the block rubs along walls and other blocks instead of stopping.
- **Corners are round, sides are flat.** The collision works on the block's position: every cell offset where the block would not fit is an obstacle shaped as a 2×2 square with rounded corners. Neighboring obstacles overlap by a whole cell, so flat sides have no seams to snag on, and only real corners are round. Pushed into a corner, the block rolls around it on a curve; pushed at a gap it is slightly out of line with, it is guided in.
- **Blocks never overlap.** The view sits exactly where the mover puts the block, with no smoothing that could cut a corner. Only half a percent of a cell of play is kept, too small to see, so a block always fits a gap exactly its own size.
- On release the block settles on the nearest free cell with an ease that does not overshoot into its neighbor.

### Doors

A block leaves when `Board.CanPassThrough` holds: every cell the whole shape sweeps on the way out is free or beyond a door of its color. An L-shaped block therefore cannot leave through a door that only its foot fits.

### Solver and stuck

- Leaving never hurts, because it only frees space. So the solver first lets every block that can reach its door leave.
- It then searches, breadth first, over *repositions*: moving one block to any cell it can reach. The depth of the result is the number of blocks that must be moved out of the way, which is the level's difficulty.
- Every move can be undone and leaving never hurts, so **a solvable board stays solvable whatever the player does**.
- The session therefore asks the solver once, on a worker thread, and asks again only while the answer is unknown. The stuck fail popup appears after the player's first move on a level that cannot be cleared.

### Generator

The generator places random doors and blocks for a difficulty, and drops any layout where a block can never fit a door of its color. It keeps the first layout the solver proves solvable with a number of repositions in that difficulty's range. Generation is seeded, so it can be repeated.

### Performance

- Each block is merged into **one mesh**. The board (ground tiles, walls and corners) is **one mesh** too, with a submesh for the ground and one for the walls; only the colored doors are meshes of their own. A wall between two doors or corners is a single wall piece stretched to fit, so a level draws in a handful of calls.
- All blocks share one material and all doors share another; each renderer gets its color from a material property block. GPU instancing is on for both, and the toon shader reads the color per instance, so no material is copied at runtime.
- The board's parts never move, so once built they are combined as static geometry (`StaticBatchingUtility`).
- There are no allocations in the update loops. For example, the timer text uses `SetText` with arguments and only changes once a second.
- The block bursts come from a pool and take the block's color.
- The held block's outline is only its outer rim, drawn on top of everything. Two tiny unlit shaders (`Rendering/Shaders`) do it, with no lighting, textures or keywords: one marks the block's silhouette in the stencil buffer, the other draws the mesh pushed out along its normals only outside that mark, ignoring depth. They are added as extra materials only while the block is held, and the push follows normals averaged at build time, so hard edges do not split the rim.
- The solver runs off the main thread.
- UI graphics have raycast target, maskable, rich text, kerning and extra padding turned off wherever they are not needed, and they share one sprite atlas.

### Look

Everything in the gameplay scene uses one toon shader (`Rendering/Shaders/Toon.shader`). Light falls in two soft bands, so shadow sides take a tinted color instead of going dark, and a small glint and a soft rim make surfaces read like candy. It uses only the main light and its shadows, no textures, and one material buffer for all its passes, which keeps it cheap on mobile and friendly to the SRP Batcher. The block colors are sweet tones with no white (Cherry, Tangerine, Lemon, Lime, Mint, Aqua, Sky, Grape, Bubblegum, Cocoa), on lavender walls, a periwinkle ground and background, and a plum outline for the held block.

### Persistence

Coins, the current level and the settings go through `IKeyValueStorage`, which uses PlayerPrefs.

## Known issues and limits

- No APK or video is in the repository. Build one from *Build Settings*.
- Boosters and lives are placeholders, as the case allows.
- On the levels that ship with the game, **stuck cannot happen**, because they are all proven solvable and solvability never changes during play. To see the stuck popup, make a level in the editor that cannot be solved (for example a block with no door of its color), then press ▶ Play.
- While AUTO plays, the timer and the pause button are off.
- The solver has a budget. On a very large custom level, Check may answer "no solution found within the budget" instead of a clear yes or no.
- During an editor ▶ Play, **Next**, **Restart** and **Home › Play** all return to the tested level until play mode ends.
- After level 10 the catalog starts again from level 1, while the home screen keeps counting up.
- The supplied `Arrows.fbx` and `Door_Arrow` meshes are not used, because arrow blocks are not in scope.
- The home screen had a design reference overlay (a screenshot image that is not in the repository). It is turned off in `Main.unity`.

**Feedback on the supplied assets**

- There was no "toggle off" sprite; `btn_toggle_off.png` was added.
- `BlockParts.fbx` and `Arrows.fbx` are modeled on the XY plane facing −Z, while `WallAndDoor.fbx` and `GroundGrid.fbx` are Y-up. The wall, corner and door models in `WallAndDoor.fbx` are also upside down, and the wall is turned a quarter; the board sets each right with its *Model turns* on `BoardArt`.
- A board cell is 2 units, and block modules are quarter cells.
- `WallAndDoor.fbx` references a missing embedded texture, and `corner_4` has an unused blend shape.
- Booster icons are 1024×1024, much bigger than their size on screen.
- Soft shadows and glows make automatic 9-slice borders unreliable, so the borders were set by hand.

## LLM tools used

**Claude Code** (Anthropic, Claude Opus 5.5 model) was used as a pair programmer throughout. It proposed the architecture, wrote most of the C# code and the editor tooling, inspected the supplied models, wrote the tests and the commit messages, and drafted this README. Every step was directed and reviewed by me.

## Work time

Going by the commit history, the work ran from 28 Sep 2026 20:10 to 30 Sep 2026 02:30, in several sessions.
