# Color Block Jam — Vertical Slice

A vertical slice of Color Block Jam in Unity 2022.3.62f2 (URP), for 1080×1920 portrait screens. The Android build is [`ColorBlockJam.apk`](ColorBlockJam.apk) in the repository root.

## How to run

1. Open the project with **Unity 2022.3.62f2**. The packages resolve from `Packages/manifest.json`.
2. Open `Assets/ColorBlockJam/Scenes/Splash.unity` and press **Play**, with the Game view at 1080×1920 or 1080×2400 (20:9).
3. To build, use *File › Build Settings*: the scenes (Splash, Main, Gameplay) are already listed, and Android builds with IL2CPP for ARM64.
4. *Window › General › Test Runner* runs 96 EditMode and 3 PlayMode tests.
5. *Edit › Clear All PlayerPrefs* resets progress, coins, boosters and the obstacles seen.

**How to play.** Drag a block into a door of its color; it leaves when its whole shape fits through. Clear the board before the timer runs out. Arrow blocks move only along their arrow, a frozen block cannot move until as many other blocks as its number have left, and holes are cut out of the board. Boosters unlock at levels 2, 4, 6 and 8: Freeze stops the timer for 10 seconds, Hammer breaks a block, Rocket clears a row and Vacuum a color. **AUTO** lets the solver finish the level. In the editor and in development builds, → and ← jump between levels.

## Level editor

Open it from **Color Block Jam › Level Editor**, by double-clicking a level file (`Systems/Level/Data/Levels/LevelNNN.json`), or with **Open Level Editor** on `LevelCatalog.asset`. The levels are on the left in play order, the board is in the middle, and the settings and tools are on the right. The controls have tooltips.

1. Press **New**, or click a level on the left.
2. Set **Width**, **Height**, **Time** and **Difficulty**.
3. Pick a color (keys 1–0) and a tool: **Draw** (drag over cells to make a block of any shape), **Stamp** (a ready shape), **Door** (click or drag along the wall), **Move**, **Erase** or **Hole**. Right-click erases; Ctrl+Z and Ctrl+Y undo and redo.
4. Click a block to make it an arrow block (**Moves**) or to freeze it (**Ice**).
5. **Check** lists mistakes, such as a color without a door or a block that fits no door, and runs the solver in the background. It tells whether the level can be solved, in how many moves and at which difficulty; ◀ ▶ step through the solution.
6. **Generate** makes a new solvable level of the chosen difficulty, with the settings in `GeneratorPresets.asset`.
7. **Save** writes over the level's file; **Save As New Level** adds a file to the end of the catalog. Before saving a level that has problems, cannot be solved or is not yet proven solvable, the editor warns you.
8. **▶ Play** opens the level in the game, on a throwaway copy of the save.

The editor and the game read one format, the level's JSON file (`LevelData`). `LevelCatalog.asset` lists the files in play order.

## Architecture decisions

| Decision | Why |
|---|---|
| **One folder and one assembly per system** under `Assets/ColorBlockJam/Systems`: Core, Boot, UI, Navigation, Settings, Pooling, Economy, Progression, Level, Gameplay, Boosters, Obstacles, Home, LevelEditor, Rendering. | A system is read, changed or removed in one place. References only go one way, so the general systems (Core, UI, Navigation, Settings, Pooling) know nothing of the game. |
| **Data, rules and view are kept apart.** `Level.Data` (the level format), `Gameplay.Logic` (board, movement, solver, timer) and the editor's `LevelEditor.Authoring` (generator, checks, rating) do not reference Unity. | The game, the editor, the tests and worker threads share one implementation of each rule, such as when a block may leave through a door. |
| **VContainer** builds a root scope for the app's services, a scope for each scene and a child scope for each level. | There are no singletons or static state. Restart and Next dispose the level's scope and build a new one, so nothing is left from the previous attempt. |
| **MVP for the UI.** Views only show and report; presenters are plain C#. Each system lists its popups in a window catalog, and `ShowAsync` returns the player's choice. | Presenters are tested without a scene, and a popup does not know the level that opened it. |
| **Content and feel are ScriptableObject assets**: configs, boosters, obstacles, button feedback, transitions and toggle visuals. | Designers tune them without code, and a new booster or obstacle is one class and one asset. |
| **Movement without physics.** A dragged block sweeps its path, slides along what it touches and rolls around bevelled corners. | It is exact and deterministic: blocks never overlap, and the drag allocates nothing per frame (a test checks it). |
| **One solver**, a breadth-first search over block moves, run on a worker thread. | The game spots a stuck board, and the editor proves levels solvable and rates their difficulty, with the same code and without stalling a frame. |
| **Few draw calls.** One mesh per block and one for the board, colors stored in vertices, shared materials, one toon shader, pooled particles and a sprite atlas. | A level draws in a handful of calls with the SRP Batcher, which suits mid-range phones. |
| **Saves go through `IKeyValueStorage`** (PlayerPrefs, written when the app loses focus or quits). | The storage can be swapped without touching the game; an editor test play keeps its writes in memory. |

| Package | Why |
|---|---|
| URP 14 | Mobile rendering with the SRP Batcher. |
| Input System | One pointer path for mouse and touch. |
| VContainer | Fast dependency injection with nested scopes. |
| UniTask | Allocation-free async/await for popups, delays and the background solver. |
| LitMotion (with Burst and Collections) | Allocation-free tweens for the UI and the board. |
| TextMesh Pro | Sharp text with outlines. |
| 2D feature set, Device Simulator devices | The Sprite Editor for 9-slice borders, and phone screens to check the layout. |

## Decisions on open points

- **Levels.** The original game's levels were not supplied, so the levels are new. There are 50, all made with the editor: 1–3 by hand as tutorials, the rest with its generator, each proven solvable and rated by its moves.
- **Timer.** It starts at the first touch on a block. At zero the level stops and **Out of Time!** offers 20 seconds for 100 coins; declining opens the fail popup.
- **Fail.** The fail popup (**Retry**, **Home**) opens when time is up or when the board can no longer be cleared.
- **Pause** opens the settings with a **HOME** button; closing them resumes the level.
- **Restart** builds the level again from its data. Coins and boosters spent during the attempt stay spent.
- **Coins.** The player starts with 100 and earns 10, 20, 30 or 50 for a level by its difficulty. Coins buy extra time and boosters.
- **Placeholders.** Lives show 5, and failing costs nothing. The Shop and Collection tabs open pages with only a title, the locked tabs do nothing, and the profile, plus, language, support, legal and restore buttons only give touch feedback.
- **Optional scope.** All four items are done: the warning before saving an unsolvable level, working boosters, ice and arrow blocks. Holes, AUTO and the generator are extras.

## Known issues and incomplete parts

- The shipped levels never get stuck, because they are proven solvable and solvability does not change during play. To see the stuck fail, make an unsolvable level in the editor and press **▶ Play**.
- On a very large custom level, **Check** may answer "no solution found within the budget" instead of yes or no.
- After level 50 the levels start again from level 1, while the home screen keeps counting up.
- During an editor **▶ Play**, **Continue**, **Retry** and **Home › Play** return to the tested level.
- A booster's description is plain text, so changing its numbers means changing the text too.
- UI sprites are uncompressed on Android for sharpness, so the backgrounds take about 25 MB of memory.

## Feedback on the supplied assets

- There was no "toggle off" sprite; `btn_toggle_off.png` was added.
- `BlockParts.fbx` and `Arrows.fbx` lie on the XY plane facing −Z, while `WallAndDoor.fbx` and `GroundGrid.fbx` are Y-up. The wall, corner and door models are also upside down, and the wall is turned a quarter; *Model turns* on `BoardArt` corrects each.
- `WallAndDoor.fbx` references a missing embedded texture, and `corner_4` has an unused blend shape.
- The booster icons (1024×1024) and the ice texture (2048×2048) are much bigger than their size on screen.
- Soft shadows and glows make automatic 9-slice borders unreliable, so the borders were set by hand.
- There were no obstacle icons, and the Watch Ad button, coin pile and fail offer seen in the reference screens were not supplied, so they are left out.

## LLM tools used

**Claude Code** (Anthropic, Claude Opus 5.5 model) was a pair programmer throughout. It proposed the architecture, wrote most of the C# code, the shaders and the editor tooling, inspected the supplied models, wrote the tests and the commit messages, and drafted this README. I directed and reviewed every step.

## Work time

By the commit history, about 23 hours in seven sessions, from 28 Sep 2026 20:10 to 4 Oct 2026 14:49.
