# Coin Runner: Unity guide

## 1. Open the project

1. Use Unity `6000.3.26f1` (Unity 6.3 LTS) with the Universal 2D template support.
2. In Unity Hub, choose **Add project from disk** and select the `CoinRunner` folder.
3. Let Unity finish importing packages and compiling scripts. The project uses the Universal Render Pipeline, Tilemap, TextMeshPro, and the Input System package.
4. Confirm **Edit > Project Settings > Player > Other Settings > Active Input Handling** is **Both**. This project uses the lab's classic keyboard calls while keeping Both enabled.
5. Open `Assets/Scenes/CoinRunner.unity`. The level, prefabs, interface, and Build Settings entry are already included. Use **Coin Runner > Rebuild Lab Scene** only if you want to regenerate them.
6. Select the **Game** tab, choose a 16:9 aspect ratio, and press **Play**.

## 2. Scene objects and lab coordinates

The scene builder creates a `Grid` with one `Ground` Tilemap and a `TilemapCollider2D`. Both the object and its Unity layer are named `Ground`. The painted cells follow the lab sheet:

| Piece | Tile coordinates |
| --- | --- |
| Start ground | X `-10` to `6`, Y `-5` and `-4` |
| Gap | X `7` and `8` left empty |
| Second ground | X `9` to `22`, Y `-5` and `-4` |
| Low platform | X `2` to `5`, Y `-1` |
| High platform | X `12` to `15`, Y `1` |
| Left wall | X `-10`, Y `-3` to `2` |

The Player starts at `(-6, -2, 0)` with a capsule collider and dynamic Rigidbody2D. Its Rigidbody2D uses continuous collision detection, interpolation, and frozen Z rotation. `GroundCheck` is a child at local `(0, -1, 0)`.

## 3. Gameplay scripts

- `PlayerMovement` reads Left/Right or A/D in `Update`, applies horizontal velocity in `FixedUpdate`, preserves vertical velocity, and permits Space to jump only when an `OverlapCircle` on the `Ground` layer touches a collider. The jump force is 14 and the selected gizmo draws the yellow ground-check circle.
- `CameraFollow` follows in `LateUpdate` with `SmoothDamp` and offset `(0, 1, -10)`.
- `PlayerInteractions` adds one coin on the `Coin` trigger, wins on the `Goal` trigger, respawns on a solid `Hazard` collision, and respawns below Y `-10`.
- `GameManager` updates `Coins: 0`, shows `LEVEL COMPLETE!`, freezes the game at the goal, and reloads the scene when **Play Again** is pressed.

The Coin prefab has a trigger circle and the `Coin` tag. The Spike prefab has a solid polygon collider and `Hazard` tag. The Goal at `(21, -2, 0)` is a trigger with the `Goal` tag. Six coins and two spikes are placed across the course.

## 4. Play and check

Use:

- **Left/Right** or **A/D** to run.
- **Space** to jump from the ground.
- The **Goal** flag to finish; **Play Again** reloads the level and resets the counter.

### Manual test checklist

- [ ] Player falls onto the starting tiles and stays upright.
- [ ] A/D and Left/Right move the player; releasing input stops horizontal movement.
- [ ] Space jumps from the floor and platforms, but repeated presses in mid-air do not create extra jumps.
- [ ] The player reaches the low platform, high platform, and clears the two-tile gap.
- [ ] Camera follows smoothly and keeps its Z offset at `-10`.
- [ ] Each coin disappears on contact and increments the counter exactly once.
- [ ] Touching either solid spike respawns the player at the start and stops its velocity.
- [ ] Falling below the level respawns the player at the start.
- [ ] Reaching the goal shows the win panel and freezes gameplay.
- [ ] **Play Again** restarts the level with `Coins: 0`.
- [ ] The Unity Console has no errors after import and play.

## 5. Make a Windows build

1. Open **File > Build Profiles** and confirm `Assets/Scenes/CoinRunner.unity` is included.
2. Choose **Windows, Mac, Linux** and select **Windows** as the target.
3. Choose **Build**, select an output folder outside `Assets`, and run the executable to repeat the checklist.
