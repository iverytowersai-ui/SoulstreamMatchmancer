**Purpose & context**

Ivery is building two Unity C# mobile game projects: an existing Match-3 puzzle game engine and a new, more ambitious puzzle-combat RPG called **Soulstream Matchmancer** (also referred to as "Matchmancer"). Matchmancer is a Hero Emblems-style game with 100 levels across 10 stages, a single playable character, local-only save architecture, and deep RPG systems layered on the Match-3 foundation. A collaborator named Geo has contributed structural design notes covering level/stage layout, achievement tracking systems, and UI section requirements.

The primary vehicle for this work is a suite of Claude **skill files** — structured `.skill` archives that encode system knowledge, routing logic, and implementation patterns for use across sessions.

**Current state**

The Match-3 engine skill suite (Skills 01–10 plus a master prompt) has been fully audited and rebuilt. All 11 files were rewritten, packaged, and delivered.

For Matchmancer, a 15-skill extension suite (Skills 11–25) was designed, and Skills 11–19 have been built sequentially and delivered. Systems completed so far:
- **Skill 11**: Tile types and combat formula
- **Skill 12**: Combat resolver
- **Skill 13**: Enemy system with AI turn logic
- **Skill 14**: Character system (HP, ultimate meter)
- **Skill 15**: Gear system (four slots)
- **Skill 16**: Inventory and booster system
- **Skill 17**: Level and stage data for all 100 levels
- **Skill 18**: Stars and results evaluation
- **Skill 19**: Achievement and title system foundation

Skills 20–25 remain to be built.

**On the horizon**

- Completing Skills 20–25 of the Matchmancer extension suite
- UI systems (load screen, login screen, main screen, shop, lore page, stage/puzzle level screens) as outlined in Geo's notes
- Storyline and small animation integration connecting puzzle segments
- Future abstraction layer for potential save system migration beyond local persistence

**Key learnings & principles**

- A **dedicated Matchmancer master prompt** is kept separate from the Match-3 engine skill suite — the two systems are architecturally distinct despite sharing a foundation
- Local-only persistence is the current requirement, but the save abstraction should be designed cleanly to allow future migration without rework
- Each skill follows a consistent internal pattern: ScriptableObject data definitions → runtime MonoBehaviour managers → event-driven wiring → save/load hooks → starter assets → validation checklist
- Ivery communicates tersely and sequentially ("here," "Continue," "next," "Ready") — confirmation of work and forward progression are the primary signals; no revision requests unless explicitly stated

**Approach & patterns**

- Work proceeds **strictly sequentially** through the skill list with no skipping or parallelism
- Ivery delegates prioritization and judgment calls to Claude once direction is set; audits and fixes are applied comprehensively rather than selectively
- Each skill is packaged and delivered as a complete, installable `.skill` file before moving to the next
- Verification steps (line count, section headings, description prefix checks) are run inline before delivery to catch truncation or packaging issues

**Tools & resources**

- **Unity C# / mobile**: Core development platform
- **Claude skill files (`.skill` archives)**: Structured zip archives containing `SKILL.md` and associated assets; read via `zipfile.ZipFile` + `z.read('SKILL.md').decode('utf-8')`
- **Python `zipfile` module**: Packaging with `ZIP_DEFLATED` compression + `os.walk` + `os.path.relpath` is the reliable pattern for producing valid installable archives
- **`bash_tool` with Python `content.replace()`**: Preferred fallback over `str_replace` for multi-line substitutions when exact-match issues arise
- **Output path**: `/mnt/user-data/outputs/` with `present_files` for delivery
- **Geo's design notes**: Source material for Matchmancer's level structure, achievement tracking metrics, and UI section requirements