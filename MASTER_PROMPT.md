# Reusable master prompt

Copy this prompt into a coding agent with the lab PDF attached and a Unity-capable computer:

```text
Build the complete Unity 6 Universal 2D Coin Runner platformer specified in the attached 72-page Lab02_Unity_Platformer_Coin_Runner PDF. Treat the PDF as the authoritative requirements. Read all pages, including diagrams, code screenshots, Inspector settings, coordinates, troubleshooting, and checkpoints, before implementation. Do not guess omitted values if the PDF shows them; inspect the page and preserve them.

Use Unity 6.3 LTS (6000.3.x) and the Universal 2D template. Keep the project self-contained in a new folder named CoinRunner. Do not edit unrelated files or any synced source/reference files. Use built-in shapes or small original procedural assets; do not download unrelated art.

Implement the whole lab before adding extras:
- Active Input Handling = Both; Left/Right or A/D movement; Space jump only while grounded.
- Use Rigidbody2D.linearVelocity and FixedUpdate for physics, an OverlapCircle at the Player's feet with a Ground LayerMask, and a selected yellow gizmo.
- Reproduce the level's exact tile coordinates: start floor X -10..6 at Y -4 and -5; leave X 7..8 empty; second floor X 9..22 at Y -4 and -5; low platform X 2..5 at Y -1; high platform X 12..15 at Y 1; left wall X -10 from Y -3..2.
- Player at (-6,-2,0), capsule collider, dynamic Rigidbody2D, continuous collision detection, interpolation, frozen Z rotation, and child GroundCheck at local (0,-1,0).
- Smooth camera follow in LateUpdate using SmoothDamp and offset (0,1,-10).
- Coin prefab/tag/trigger, TMP coin counter, GameManager, solid tagged spikes that respawn the player, fall-limit respawn, goal trigger at (21,-2), LEVEL COMPLETE panel, and Play Again restart.
- Preserve the lab's separation between movement and interaction scripts. Ensure all tags, layers, scene references, UI references, build scenes, and prefab references are assigned.

Open the project in the installed Unity Editor, resolve package/version issues, compile, and run the complete manual test checklist: movement and stopping; jump only from ground; reach both platforms and clear gap; camera; each coin updates once; solid spikes respawn; gap fall respawns; goal freezes and shows the panel; Play Again resets the count; no Console errors. Fix failures and repeat validation. Do not claim tests passed unless they were actually run.

Add concise student-facing setup and play instructions, a troubleshooting section, and a test checklist. Then initialize a Git repository in the project folder and make a simple English commit after each reviewable milestone: project setup, movement and level, coins/hazards/goal, validation fixes, and final instructions. Never commit Library, Temp, Logs, build output, secrets, or unrelated files. Create a new private GitHub repository in the user's connected/authenticated account unless the user explicitly requests public visibility. Push each completed milestone. Do not overwrite an existing repository; if the chosen name is taken, use CoinRunner-Lab02 and report the exact name and link. Do not claim a repository, push, commit, build, or test that did not actually succeed.

Final response must include: local project path, GitHub URL, exact commit hashes and simple commit messages that were actually pushed, Unity version, features completed, test results with any limitations, step-by-step open/play/build directions, and the completed local master prompt. If a blocker prevents a GitHub action or Unity validation, finish all independent local work and state exactly which operation remains and what the user needs to enable.
```

## Repository milestone message examples

- `Set up Unity 6 Universal 2D project`
- `Add player movement and level`
- `Add coins hazards and goal`
- `Fix gameplay validation issues`
- `Add setup guide and test checklist`
