# YTÜ Game Jam — Comic & Combat

A turn-based RPG prototype built for **Skylab Yıldız Jam 2026** (Yıldız Technical University). The game opens with a short comic-style intro that hands off into a reaction-driven turn battle.

▶️ **Play it on itch.io:** https://hamxxa.itch.io/ytu-gamejam

> ⚠️ This is a game jam prototype and is still unfinished. Expect rough edges. Parts of the code, art, and assets were created with AI assistance.

---

## Overview

The game runs as a single linear experience:

```
Boot ──▶ Comic (intro) ──▶ Fight (turn-based battle) ──▶ Victory
```

- **Comic** — Animator-driven comic pages tell the intro. Click (or press **Space**) to advance panel by panel.
- **Fight** — When the comic ends, the battle scene loads automatically. Defeat every enemy to win.

Scenes are loaded **additively** and orchestrated by a persistent `GameManager` + `SceneLoader` that live in the `Boot` scene.

## Gameplay

Combat is turn-based with a real-time **reaction (QTE)** layer:

- **Turn order** is decided by each character's `speed` (fastest acts first).
- On your turn, pick an **ability**, then pick a **target**.
- When an enemy attacks, a **reaction window** can open. React in time to:
  - **Dodge** — take no damage
  - **Parry** — reflect part of the damage back
  - **Counter** — strike back (has a per-character cooldown)
- Miss the window and you take the full hit.

### Systems under the hood

- **Abilities** carry status effects, cooldowns, target type (enemy/ally), and an animation trigger.
- **Status effects:** Poison, Shield, Attack Up/Down, Defense Up/Down, Heal-over-Time, Instant Heal/Damage, Stun, and **Mark** (a debuff certain characters can "break" for bonus damage).
- **Cooldowns** are tracked per ability, per character, and tick down each turn.
- Data is authored as **ScriptableObjects** under `Assets/ScriptableObjects/` — characters, abilities, and effects can be tuned in the Inspector without touching code.

## Controls

| Context | Input | Action |
|---|---|---|
| Comic | Left click / **Space** | Advance to the next panel |
| Fight | Mouse | Select ability and target via on-screen UI |
| Fight (reactions) | `W A S D`, `J K L U I O`, `C` | Hit the QTE keys shown during a reaction window |

## Running the project

**Requirements:** Unity **6000.3.6f1** (Unity 6).

1. Clone the repo and open the folder in Unity Hub with the matching editor version.
2. Open the **`Boot`** scene (`Assets/Scenes/Boot.unity`) — this is the entry point that loads everything else.
3. Press **Play**.

> Always start from `Boot`. It holds the `GameManager` and `SceneLoader` that drive the Comic → Fight flow. Playing `Comic` or `Fight` in isolation will skip that wiring.

### Build

The build scene order is already configured in **File ▸ Build Settings** (`Boot`, `Fight`, `MainMenu`, `Comic`). Build for **Windows** to match the itch.io release.

## Project structure

```
Assets/
├── Scenes/            Boot, Comic, Fight, MainMenu
├── Scripts/
│   ├── Core/          GameManager, SceneLoader  (flow + additive scene loading)
│   ├── Comic/         ComicManager, ComicPage, input + camera controllers
│   ├── Fight/         TurnManager, BattleEntity, FightManager, QTE / reaction system
│   └── UI/            Health bars, main menu
├── ScriptableObjects/
│   ├── Entities/      Character definitions (Yusef, Alee, Isro, Skeleton, Golem, Necromancer, …)
│   ├── Abilities/     Ability definitions
│   └── Effects/       Status-effect definitions
└── Prefabs/ComicPages/  Comic page prefabs
```

## Credits

- **Created by:** hamxxa, RedSilentStorm ([itch.io](https://hamxxa.itch.io/ytu-gamejam))
- **Event:** Skylab Yıldız Jam 2026 — Yıldız Technical University
- Made with **Unity 6**.
- AI tools were used to assist with development, code, and some graphics.

## Status

Unfinished jam submission, released as-is. Contributions and bug reports are welcome.
