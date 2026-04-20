MATCHMANCER
Solo Dev Production Plan
Dual-Track: Unity Learning + AI Art Pipeline
Version 1.0 | March 2026 | Ivery Towers LLC
Art Style: Dark Fantasy | Engine: Unity (C#) | Tools: Midjourney / DALL-E 3 / Leonardo AI

---

Matchmancer — Solo Dev Plan
1. Your Situation & Strategy
You're a total beginner with zero game dev experience, but you have a clean GDD, strong
art direction instincts (from Soulstream), and access to AI image generation tools. This
plan turns those assets into a playable match-3 game.
The strategy is simple: learn Unity and C# fundamentals while simultaneously building
your art asset library with AI tools. By the time you can code the game, your art is ready
to drop in.
Engine: Unity (free Personal license) with C#
AI Art Tools: Midjourney, DALL-E 3, Leonardo AI
Target: Playable MVP in 8–12 weeks (realistic for a learning solo dev)
Art Style: Dark fantasy with glowing sigils, crimson/teal energy, readable at phone scale
Phase Track A: Learn Unity Track B: AI Art
Weeks 1–2 Unity basics + C# fundamentals 6 tile icons + color palette
Weeks 3–4 Match-3 template + 3 character portraits + 3 specials
customization
Weeks 5–6 Blockers + character ultimates 3 blockers + UI backgrounds
Weeks 7–8 25 levels + map screen + polish Map screen + boosters + VFX
sprites
2. Week-by-Week Breakdown
Each week has both tracks running in parallel. Spend mornings on Unity, evenings on art
generation (or vice versa). The key is switching between them so neither feels like a
grind.
Week 1: Foundation
2

---

Matchmancer — Solo Dev Plan
Day Track A: Unity What To Do Track B: Art
Day 1 Install Unity Hub + Download from unity.com. Choose Set up Midjourney/Leo
Unity 2022 LTS 2D template when creating first accounts
project. Follow the setup wizard.
Day 2 Unity Roll-a-Ball Official Unity Learn tutorial. Teaches Generate color palette
tutorial scenes, GameObjects, test images
components, inspector. Do NOT
skip.
Day 3 Continue Roll-a-Ball Finish the tutorial. Experiment: Port Rune tile (3
change speeds, colors, add extra variations)
objects. Break things on purpose.
Day 4 C# basics: variables, Unity Learn C# for beginners series OZONE Mark tile (3
if/else (free). Focus on: int, float, string, variations)
bool, if statements.
Day 5 C# basics: loops, Continue C# series. Understand for Coven Seal tile (3
arrays loops and arrays — match-3 grids variations)
are literally 2D arrays.
Day 6-7 C# basics: functions, Finish C# fundamentals. Write a Remaining 3 tiles +
classes tiny script that spawns colored review all 6
squares in a grid. Your first real
step.
Week 2: Grid Prototype
Day Track A: Unity What To Do Track B: Art
Day 1 Create 9x9 grid with Write a script that instantiates a 9x9 Refine best tile variations
code grid of colored squares. Just cubes
or sprites, no matching yet.
Day 2 Tile selection + swap Detect tap/click on tiles. Swap two Generate Special tile
logic adjacent tiles. No match detection icons (Line Sigil)
yet, just swap positions.
Day 3 Match-3 detection Write CheckForMatches(): scan Star Sigil + Nova Sigil
rows and columns for 3+ same- icons
color tiles. Mark them.
Day 4 Tile destruction + Destroy matched tiles. Make tiles Blue character portrait
gravity above fall down. Spawn new tiles at (main)
top.
Day 5 Cascade detection After gravity fills gaps, check for Blue character (2 more
new matches. Repeat until no more angles)
matches. This is the core loop!
3

---

Matchmancer — Solo Dev Plan
Day 6-7 Polish: animations, Add simple scale/fade animations Crux character portrait
score on match. Add a score counter. (main)
Playtest obsessively.
Week 3: Specials + Template Study
Day Track A: Unity What To Do Track B: Art
Day 1 Match-4 = Line Sigil When 4 tiles match, spawn a Line Crux character
logic Sigil instead of destroying. Line (additional views)
Sigil clears row or column when
matched.
Day 2 Match-5 = Star Sigil Star Sigil clears all tiles of one Kaery character portrait
logic chosen type. Add the choose-a- (main)
color popup.
Day 3 L/T shape = Nova Sigil Detect L and T match shapes. Nova Kaery character
Sigil creates a 3x3 explosion when (additional views)
matched.
Day 4-5 Buy + study a match-3 Unity Asset Store: buy a well-rated Glass blocker overlay
template match-3 kit. Study how they handle: (1-2 layers)
board generation, animation, UI,
level loading.
Day 6-7 Compare your code to See what they did better. Adopt Chain blocker + Stone/
template their patterns for things like level Armor blocker
data, objective tracking, move
counting.
Week 4: Blockers + Characters
Day Track A: Unity What To Do Track B: Art
Day 1-2 Implement Glass Glass sits on tiles. Adjacent Character Ultimate
blocker matches crack it (layer 1 then layer button icons (x3)
2 = destroyed). Track layers per
cell.
Day 3 Implement Chain Chained tile can't be swapped until UI background: dark
blocker adjacent match removes the chain. fantasy board frame
One hit = freed.
Day 4 Implement Stone/ Stone takes 2-3 hits from adjacent Win screen + Lose
Armor blocker matches. Track hit count. Visual screen backgrounds
should crack progressively.
4

---

Matchmancer — Solo Dev Plan
Day 5-6 Character system: Add charge counter (+1 per match). Map screen node path
charge + ultimate At 10 charge, enable Ultimate background
button. Implement Blue's 4x4 clear.
Day 7 Implement Crux + Crux: tap to clear row+column Booster icons (Bomb,
Kaery ultimates cross. Kaery: pick tile A, convert up Line, Swap)
to 8 into tile B.
Week 5-6: Levels + UI
Day Track A: Unity What To Do Track B: Art
Days 1-3 Level data system Create a way to define levels: grid Polish all tile icons for
layout, which blockers, objective final size
type, move limit. JSON or
ScriptableObjects.
Days 4-6 Build levels 1-15 Follow the GDD ramp: 1-5 teach VFX sprites: match
matching, 6-10 teach specials, particles, glow effects
11-15 introduce Glass. Playtest
each one.
Days 7-9 Build levels 16-25 16-20 add Chains, 21-25 mix all Character passive ability
blockers. These should feel icons
challenging but not impossible.
Days 10-14 Map screen + Node-based map (like Candy Final asset review +
character select Crush). Character select before export at correct sizes
each level. Win/lose screens.
Week 7-8: Integration + Polish
Day Track A: Unity What To Do Track B: Art
Days 1-3 Drop in final art assets Replace all placeholder squares Fix any art that doesn't
with your AI-generated tiles, read well in-game
character portraits, UI backgrounds.
Days 4-6 Sound + juice Add match sounds, cascade Generate any missing
sounds, ultimate activation sounds. VFX/particle sprites
Add screen shake, particle effects
on big clears.
Days 7-10 Playtest loop Play all 25 levels start to finish. App icon + loading
Note where levels feel too easy/ screen art
hard. Adjust move counts and
blocker placement.
5

---

Matchmancer — Solo Dev Plan
Days 11-14 Bug fixing + final build Fix all bugs found in playtesting. Store screenshots +
Build for Android (APK) or iOS promotional art
(TestFlight). Share with friends for
feedback.
3. AI Art Pipeline — Complete Prompt Library
Your reference image establishes the visual direction: dark fantasy, crimson/red energy,
glowing runes, dramatic lighting, cracked stone environments. We extract these qualities
but adapt them for match-3 tile readability.
3.1 Style DNA (Extracted from Reference)
Element Matchmancer Application
Color Palette Crimson red (#C41E3A), void black (#0A0A0A), molten gold (#FFD100),
neon teal (#00D4FF), deep violet (#7B2CBF), pale silver (#C0C0C0)
Energy Style Glowing sigils, crackling red/teal particle trails, runic circle ground
effects, volumetric smoke
Materials Cracked obsidian stone, oxidized dark metal, kintsugi gold cracks,
leather with chain accents
Lighting Dramatic chiaroscuro, red/crimson rim lighting, warm gold from rune
glow, cool teal accents
Mood Dark, powerful, supernatural — but stylized enough for mobile (not
horror)
3.2 Tile Icon Prompts (6 Base Tiles)
CRITICAL RULE: Tiles must be simple, high-contrast icons readable at 60x60px on a
phone screen. Generate at 512x512, test by scaling to 60x60. If you can't tell what it is at
tiny size, simplify.
Platform: Use Leonardo AI (SDXL model, guidance 8, 50 steps) for icons. Clean
backgrounds are easier to control.
1. Port Rune (Cyan/Teal #00D4FF)
Game icon, simple glowing cyan portal rune symbol, circular gate design with
inner geometric pattern, neon teal glow (#00D4FF), floating in void, clean
black background, flat icon style with subtle 3D depth, sharp edges, game UI
6

---

Matchmancer — Solo Dev Plan
asset, no text, 512x512. Negative: detailed background, realistic, blurry,
text, watermark
2. OZONE Mark (Electric Gold #FFD100)
Game icon, angular electric gold faction emblem, sharp geometric syndicate
symbol with lightning motif, molten gold glow (#FFD100), metallic sheen,
clean black background, flat icon style with subtle 3D depth, sharp edges,
game UI asset, no text, 512x512. Negative: detailed background, realistic,
blurry, text, watermark
3. Coven Seal (Deep Violet #7B2CBF)
Game icon, mystical purple witch coven seal, circular occult symbol with
pentagram inner design, glowing violet energy (#7B2CBF), ethereal smoke
wisps, clean black background, flat icon style with subtle 3D depth, sharp
edges, game UI asset, no text, 512x512. Negative: detailed background,
realistic, blurry, text, watermark
4. Witchbreed Thorn (Crimson Red #C41E3A)
Game icon, crimson red thorned mark symbol, sharp organic thorn pattern
forming a diamond shape, glowing red energy (#C41E3A), blood-red particle
effects, clean black background, flat icon style with subtle 3D depth, sharp
edges, game UI asset, no text, 512x512. Negative: detailed background,
realistic, blurry, text, watermark
5. Soulstream Shard (White/Silver #C0C0C0)
Game icon, luminous white crystal shard, faceted gemstone with inner light,
silver-white glow (#C0C0C0) with prismatic highlights, floating energy
particles, clean black background, flat icon style with subtle 3D depth,
sharp edges, game UI asset, no text, 512x512. Negative: detailed background,
realistic, blurry, text, watermark
6. Petsha Charm (Warm Amber #D4A017)
Game icon, warm golden charm coin, circular amulet with animal paw motif
center, amber-gold glow (#D4A017), ancient metallic texture, subtle chain
links, clean black background, flat icon style with subtle 3D depth, sharp
edges, game UI asset, no text, 512x512. Negative: detailed background,
realistic, blurry, text, watermark
3.3 Character Portrait Prompts (3 Characters)
Use Midjourney (--ar 2:3 --style raw --v 6.1) for character portraits. These appear as UI
bust portraits during character select and level play. Style follows the reference: dramatic
dark fantasy, strong physique, supernatural energy, but each character has their own
color identity.
Blue — Soulstream Pulse Caster (Teal/Cyan energy identity)
7

---

Matchmancer — Solo Dev Plan
Cinematic bust portrait of a young mystical warrior, sharp jawline, silver-
white hair swept back, glowing teal-cyan eyes, wearing dark obsidian armor
with cracked kintsugi gold veins, teal energy particles orbiting around
shoulders, Soulstream crystal embedded in chest plate glowing white-blue,
dark ruined temple background with volumetric teal mist, dramatic chiaroscuro
lighting, rim light from cyan energy source, shot on 85mm f/1.8, dark fantasy
game character art, hyper-detailed, 8K. Negative: cartoon, anime, bright
colors, modern clothing, blurry, text --ar 2:3 --style raw --v 6.1
Crux — Port Cross Navigator (Gold/Amber energy identity)
Cinematic bust portrait of a tactical rogue navigator, angular features, dark
hair with gold streaks, amber-gold glowing eyes, wearing weathered dark
leather coat with oxidized bronze buckles and gold runic stitching, golden
compass rose pendant glowing at chest, golden crosshair energy forming behind
head, dark stormy sky with portal rifts background, warm gold rim lighting
mixed with cool shadows, dramatic Renaissance-style hard shadow edges, shot
on 85mm f/1.8, dark fantasy game character art, hyper-detailed, 8K. Negative:
cartoon, anime, bright colors, modern clothing, blurry, text --ar 2:3 --style
raw --v 6.1
Kaery — Glamour Shift Enchantress (Violet/Purple energy identity)
Cinematic bust portrait of an enigmatic enchantress, high cheekbones, dark
hair with violet-purple streaks, glowing purple eyes with magical seal iris
pattern, wearing deep crimson velvet cloak over dark armor with violet
crystal fragments embedded in shoulder pieces, floating violet tarot cards
orbiting near hands, purple transformation energy swirling, dark candlelit
chamber background with occult symbols, warm amber candlelight mixed with
violet magical glow, shot on 85mm f/1.8, dark fantasy game character art,
hyper-detailed, 8K. Negative: cartoon, anime, bright colors, modern clothing,
blurry, text --ar 2:3 --style raw --v 6.1
3.4 Special Tiles + Blocker Prompts
Line Sigil (Horizontal beam icon)
Game icon, glowing horizontal energy beam sigil, sleek arrow-like design,
neon cyan and gold energy trail, motion blur streaks, clean black background,
flat icon style with subtle 3D depth, game UI asset, no text, 512x512.
Negative: detailed background, realistic, blurry, text
Star Sigil (Radiant star icon)
Game icon, radiant five-pointed star sigil, glowing white-gold core with
prismatic rainbow edge highlights, celestial energy particles, clean black
background, flat icon style with subtle 3D depth, game UI asset, no text,
512x512. Negative: detailed background, realistic, blurry, text
Nova Sigil (Explosion icon)
8

---

Matchmancer — Solo Dev Plan
Game icon, explosive nova burst sigil, circular shockwave design with
crimson-red and gold energy rings expanding outward, particle debris, clean
black background, flat icon style with subtle 3D depth, game UI asset, no
text, 512x512. Negative: detailed background, realistic, blurry, text
Glass Blocker (Layer 1) (Transparent ice overlay)
Game UI overlay, cracked glass texture, single crack pattern, translucent
ice-blue tint, frost edges, transparent center, game asset overlay, PNG with
transparency feel, 512x512. Negative: opaque, solid color, text
Glass Blocker (Layer 2) (Heavy crack overlay)
Game UI overlay, heavily cracked glass texture, spider-web crack pattern,
translucent ice-blue tint, more frost, nearly breaking apart, game asset
overlay, 512x512. Negative: opaque, solid color, text
Chain Blocker (Iron chain wrap)
Game icon, dark iron chains wrapped in X pattern over a tile space, oxidized
metal texture, faint red glow between chain links, locked padlock at center,
clean dark background, game UI asset, 512x512. Negative: realistic, detailed
background, text
Stone/Armor Blocker (Cracked stone shield)
Game icon, dark stone armor shield tile, cracked obsidian surface with faint
kintsugi gold veins, heavy and solid appearance, 3 hit marks visible as
progressive cracks, clean dark background, game UI asset, 512x512. Negative:
realistic, detailed background, text
4. Complete Asset Checklist
Track your progress. Every asset needed for the MVP:
Asset Count Size Status
Base Tiles (6 types) 6 512x512 ☐ TODO
Special Tiles (Line/Star/Nova) 3 512x512 ☐ TODO
Glass Blocker (2 layers) 2 512x512 ☐ TODO
Chain Blocker 1 512x512 ☐ TODO
Stone/Armor Blocker (3 states) 3 512x512 ☐ TODO
Character Portrait: Blue 1 1024x1536 ☐ TODO
Character Portrait: Crux 1 1024x1536 ☐ TODO
9

---

Matchmancer — Solo Dev Plan
Character Portrait: Kaery 1 1024x1536 ☐ TODO
Ultimate Button Icons (x3) 3 256x256 ☐ TODO
Passive Ability Icons (x3) 3 128x128 ☐ TODO
Booster Icons (Bomb/Line/Swap) 3 256x256 ☐ TODO
Board Background 1 1080x1920 ☐ TODO
Map Screen Background 1 1080x1920 ☐ TODO
Win Screen Background 1 1080x1920 ☐ TODO
Lose Screen Background 1 1080x1920 ☐ TODO
Character Select Background 1 1080x1920 ☐ TODO
App Icon 1 1024x1024 ☐ TODO
Loading Screen 1 1080x1920 ☐ TODO
Match VFX Particles (per color) 6 256x256 ☐ TODO
Ultimate VFX (per character) 3 512x512 ☐ TODO
TOTAL: ~42 unique assets for MVP launch.
5. Essential Resources & Links
5.1 Unity Learning Path (Free)
1. Unity Hub Download: unity.com/download
2. Roll-a-Ball Tutorial: learn.unity.com (search "Roll a Ball")
3. C# for Beginners: Unity Learn > Pathways > Junior Programmer
4. YouTube: "Zigurous Match 3 Unity Tutorial" (full free series)
5. YouTube: "Brackeys" channel (best beginner Unity tutorials)
5.2 Match-3 Specific
• Unity Asset Store: search "Match 3 Kit" or "Match 3 Starter" (budget $30-80)
• Key concepts to Google: "Unity 2D grid system", "Unity ScriptableObject level data", "Unity
match-3 cascade logic"
5.3 AI Art Tools
• Midjourney: Best for character portraits (use --ar 2:3 --style raw --v 6.1)
• Leonardo AI: Best for icons/tiles (SDXL model, guidance 7-9, 50 steps, use negative prompt box)
• DALL-E 3: Best for UI backgrounds and scenes (natural language prompts, strong composition)
5.4 Art Pipeline Rules
10

---

Matchmancer — Solo Dev Plan
6. Generate 4 variations per prompt. Pick the best one.
7. Change only ONE variable per iteration (lighting OR pose OR material, never all three).
8. Test every tile at 60x60px. If it's not readable, simplify the prompt.
9. Remove backgrounds in Photopea (free) or remove.bg before importing to Unity.
10. Export all game assets as PNG with transparent backgrounds.
6. Solo Dev Survival Rules
These aren't motivational fluff. They're hard-earned rules from people who actually
shipped games solo:
Rule 1: Ugly prototype first. Your game will look terrible for weeks. That's correct.
Gameplay feel matters more than visuals at the start. The art comes later and transforms
everything overnight.
Rule 2: Playtest every single day. Even if you only added one feature, play the game. Feel
what's fun and what's broken. Your hands will tell you what your eyes miss.
Rule 3: One feature at a time. Never work on blockers and characters and UI
simultaneously. Finish one, commit, move on. Context-switching is the solo dev killer.
Rule 4: The GDD is your anchor. When you feel lost or tempted to add features, re-read
the GDD. It says "Ignore for now" on relics, events, story, PvP. Trust that. Build the MVP.
Rule 5: Ship ugly, then polish. A working ugly game beats a beautiful broken one. Get all
25 levels playable first. Then make them pretty.
Rule 6: Track B keeps you sane. When coding frustrates you, switch to art generation.
When prompts frustrate you, switch to code. Two tracks = no burnout dead ends.
11

---
