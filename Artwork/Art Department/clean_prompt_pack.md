# MATCHMANCER - Clean Prompt Pack

A simplified, production-friendly version of the Matchmancer prompt doc.

This version keeps the useful art direction and removes most of the repetition.
Use it as a working prompt pack, not a museum plaque.

---

## 1) Global Style Lock

**Palette**
- Void black: `#000000` to `#1A1A1A`
- Cyan glow: `#00D4FF`
- Violet glow: `#7B2CBF`
- Silver-white: `#D9D9D9`
- Aged gold: muted, patina, never shiny
- Bone ivory: `#E8E8E0`

**Materials**
- Obsidian
- Carved stone
- Forged metal
- Ritual alloy
- Patina, abrasion, soot, weathering

**Core rules**
- Glow means **contained power**, not decoration
- Futurism means **geometry + containment**, not chrome
- Assets must read clearly at small size
- Silhouette first, surface detail second

---

## 2) Global Negative Prompt

Use this in the negative box or append only when needed:

```text
watermark, text, logo, cartoon, anime, chibi, cute style, bright neon casino glow, sparkles, glitter, chrome, plastic, smooth gradients, soft pastel, lens flare, bloom overload, literal faces, literal animals, modern consumer objects, low quality, blurry, compression artifacts, noisy grain, white background, gradient background
```

---

## 3) Global Prompt Tail

Append this to most assets:

```text
dark fantasy tech-arcana game asset, obsidian and forged metal aesthetic, contained energy glow, matte weathered surfaces, clean silhouette, readable at small size, AAA mobile game quality, hyper-detailed, sharp focus, isolated asset, transparent background
```

---

## 4) Recommended Prompt Formula

Use this structure for every asset:

```text
[asset type and viewpoint], [primary silhouette], [materials], [contained glow behavior], [lighting], [readability rule], [style tail]
```

For Midjourney, keep parameters at the end.

Suggested defaults:
- Icons / tiles: `--ar 1:1 --v 7 --style raw --s 150`
- Portrait backgrounds: `--ar 9:16 --v 7 --style raw --s 150`
- Portraits: `--ar 1:1 --v 7 --style raw --s 175`

---

# WEEK 1 - CORE PLAYABLE LOOK

## D1) Tiles

### T01 - Dice Tile

```text
Abstract cube-fragment game tile icon, top-down centered, beveled six-sided polyhedron, matte obsidian faces, thin cyan containment seams along each edge like arcane circuitry, faint cyan light trapped inside hairline cracks, dark steel edge seams, subtle ritual geometry etched into one face, architectural and shard-like, no pips, no casino dice look, isolated on black, clean silhouette, readable at 32px, dark fantasy tech-arcana game asset, matte weathered surfaces, transparent background --ar 1:1 --v 7 --style raw --s 150
```

Negative add-on:

```text
literal dice dots, pips, casino dice, rounded toy look, bright neon, bloom, cute 3D render
```

### T02 - Rune Tile

```text
Flat ritual rune plate game tile icon, top-down centered, thin hexagonal slab of carved dark slate, one central abstract sigil engraved into the surface, engraved channels filled with contained violet glow, tarnished silver edge band, weathered stone texture with fine chisel marks, restrained internal light only, isolated on black, simple readable silhouette, readable at 32px, dark fantasy tech-arcana game asset, matte weathered surfaces, transparent background --ar 1:1 --v 7 --style raw --s 150
```

Negative add-on:

```text
letters, alphabet glyphs, complex rune clutter, too many symbols, bright neon, bloom
```

### T03 - Chalice Tile

```text
Abstract chalice silhouette game tile icon, top-down centered, reduced geometric goblet form with wide bowl, narrow stem, flat base, aged muted gold with heavy patina and oxidation, darker bronze-brown in crevices, matte bone-ivory accent near rim, ancient worn metal texture, no shine, isolated on black, bold readable silhouette, readable at 32px, dark fantasy tech-arcana game asset, transparent background --ar 1:1 --v 7 --style raw --s 150
```

Negative add-on:

```text
shiny gold, polished gold, holy grail, liquid in cup, wine glass, glitter, treasure icon
```

### T04 - Blade Tile

```text
Abstract forged shard tile icon, top-down centered, angular triangular-rhombus wedge, dark hammered steel with carbon-black forge patina, one thin silver-white containment line along the sharpest edge, fractured industrial silhouette, not a weapon, no handle, no hilt, isolated on black, readable at 32px, dark fantasy tech-arcana game asset, transparent background --ar 1:1 --v 7 --style raw --s 150
```

Negative add-on:

```text
sword, dagger, knife, handle, hilt, weapon icon, fantasy blade
```

### T05 - Mask Tile

```text
Abstract ritual mask curve tile icon, top-down centered, crescent-arch faceplate silhouette with two asymmetrical hollow eye voids, carved bone-ivory body, matte indigo inlay along lower curve, worn edges, subtle violet glow buried deep inside the eye voids, ceremonial but non-human, isolated on black, clean readable silhouette, readable at 32px, dark fantasy tech-arcana game asset, transparent background --ar 1:1 --v 7 --style raw --s 150
```

Negative add-on:

```text
human face, realistic nose, realistic mouth, theater mask, venetian mask, facial realism
```

### T06 - Crystal Tile

```text
Prism core crystal tile icon, top-down centered, elongated faceted hexagonal crystal, deep indigo body, frosted matte facet surfaces, thin silver-white edges where facets meet, cyan-white inner light trapped inside the center lattice, no outward glow bleed, isolated on black, geometric readable silhouette, readable at 32px, dark fantasy tech-arcana game asset, transparent background --ar 1:1 --v 7 --style raw --s 150
```

Negative add-on:

```text
diamond jewelry, glass marble, rainbow refraction, sparkles, gemstone ring, bloom overload
```

---

## D3) Special Tiles

### S01 - Horizontal Line Special

```text
Top-down special tile icon, horizontal energy bar embedded in dark obsidian housing, elongated rectangular silhouette wider than tall, central cyan plasma channel running left to right, dark steel containment brackets on both ends, brushed metal and obsidian surfaces, clear directional read as horizontal sweep, isolated on black, readable at 32px, premium not cute, dark fantasy tech-arcana game asset, transparent background --ar 1:1 --v 7 --style raw --s 150
```

### S02 - Vertical Line Special

```text
Top-down special tile icon, vertical energy column embedded in dark obsidian housing, tall rectangular silhouette, central cyan plasma channel running top to bottom, dark steel containment brackets on top and bottom, brushed metal and obsidian surfaces, clear directional read as vertical sweep, isolated on black, readable at 32px, premium not cute, dark fantasy tech-arcana game asset, transparent background --ar 1:1 --v 7 --style raw --s 150
```

### S03 - Star Special

```text
Top-down special tile icon, radial purge star forged from dark ritual alloy, five- or six-point silhouette, faint violet energy at each point, dense cyan-violet core trapped inside silver-white containment rings, carved obsidian texture, powerful radial identity, isolated on black, readable at 32px, premium not cute, dark fantasy tech-arcana game asset, transparent background --ar 1:1 --v 7 --style raw --s 160
```

### S04 - Nova Special

```text
Top-down special tile icon, circular nova core with heavy dark forged shell, concentric ritual rings etched into obsidian plating, compressed cyan core glow with violet outer resonance, looks like contained area-of-effect power ready to detonate, isolated on black, readable at 32px, premium not cute, dark fantasy tech-arcana game asset, transparent background --ar 1:1 --v 7 --style raw --s 160
```

---

## D4) Blockers

### B01 - Glass Layer 1

```text
Game blocker overlay, thin semi-transparent arcane glass panel with faint cyan tint, subtle internal stress lines and micro-bubble imperfections, thin dark iron frame, tile beneath should remain partially visible, one-hit break readability, isolated overlay, transparent background preserved, dark fantasy tech-arcana game asset, sharp detail --ar 1:1 --v 7 --style raw --s 125
```

### B02 - Glass Layer 2

```text
Game blocker overlay, thicker double-pane arcane glass with darker cyan-grey tint, overlapping glass layers separated by a visible air gap, reinforced dark iron frame with corner brackets, clearly stronger than layer 1, tile beneath still somewhat visible, isolated overlay, transparent background preserved, dark fantasy tech-arcana game asset, sharp detail --ar 1:1 --v 7 --style raw --s 125
```

### B03 - Chain Blocker

```text
Game blocker overlay, two crossing hand-forged dark iron chains forming an X over a tile, thick hammered links with rust patina, central arcane padlock with faint violet glow in keyhole, subtle shadow over tile plane, bold readable silhouette at 64px, isolated overlay, transparent background, dark fantasy tech-arcana game asset --ar 1:1 --v 7 --style raw --s 140
```

### B04 - Stone Blocker HP3

```text
Top-down ritual stone blocker, full tile cover, thick obsidian-granite slab with simple carved geometric containment marks, pristine undamaged surface, sharp-cut edges, heavy mass, subtle silver-white edge highlights, isolated on black, reads as full-health blocker, dark fantasy tech-arcana game asset, transparent background --ar 1:1 --v 7 --style raw --s 140
```

### Stone HP2

```text
Same ritual stone blocker, visible diagonal crack with smaller branch fractures, slight corner chipping, faint cyan light deep inside the main crack, clearly medium damage, isolated on black, dark fantasy tech-arcana game asset, transparent background --ar 1:1 --v 7 --style raw --s 140
```

### Stone HP1

```text
Same ritual stone blocker, heavily fractured surface with multiple deep cracks, chunks missing from corners, bright cyan crack network visible inside, looks one hit from destruction, isolated on black, dark fantasy tech-arcana game asset, transparent background --ar 1:1 --v 7 --style raw --s 145
```

---

## D5) HUD Kit

### UI01 - Objectives Panel

```text
Wide horizontal game objectives panel, dark obsidian-black surface, thin silver-white inset border, subtle brushed metal texture, very thin cyan accent line along top edge, sharp 45-degree beveled corners, mostly empty center for UI content, premium and minimal, designed as scalable game HUD element, isolated on transparent background, dark fantasy tech-arcana UI --ar 16:9 --v 7 --style raw --s 100
```

### UI02 - Moves Counter Panel

```text
Compact square moves counter panel, dark obsidian-black surface, thin silver-white border, large open center for number display, subtle geometric containment ring etched around the number area, thin violet accent along bottom edge, premium minimal game HUD element, isolated on transparent background, dark fantasy tech-arcana UI --ar 1:1 --v 7 --style raw --s 100
```

### UI03 - Pause Button

```text
Circular pause button icon, dark obsidian button body, thin silver-white ring border, two central pause bars in silver-white, faint cyan inner edge glow, slight bevel to feel pressable, clean and instantly readable, isolated on transparent background, dark fantasy tech-arcana UI --ar 1:1 --v 7 --style raw --s 100
```

### UI04 - Booster Slot

```text
Game booster slot UI element, rounded-rectangle obsidian panel, thin dashed silver-white border, slightly lighter interior, small faint plus sign at center for empty state, clean readable utility slot, isolated on transparent background, dark fantasy tech-arcana UI --ar 1:1 --v 7 --style raw --s 100
```

### UI05 - Coin Icon

```text
Currency icon, simple circular coin of aged muted gold with heavy patina, abstract geometric stamp on the face, beveled oxidized rim, hammered matte surface, no bright shine, bold readable silhouette, isolated on transparent background, dark fantasy tech-arcana UI icon --ar 1:1 --v 7 --style raw --s 120
```

### UI06 - Gem Icon

```text
Premium currency icon, faceted violet crystal with contained cyan inner core, clean front-facing diamond or shield silhouette, thin silver-white facet edges, restrained internal light only, isolated on transparent background, bold readable silhouette, dark fantasy tech-arcana UI icon --ar 1:1 --v 7 --style raw --s 120
```

---

## D6) Chapter 1 Backgrounds

### BG01 - Default Battle Background

```text
Full-screen mobile game background, vertical 9:16, vast dark ritual chamber viewed slightly from above, polished black obsidian floor with faint geometric grid lines where the board sits, deep shadowed walls, distant carved pillars at side edges only, faint cyan runes along distant architectural seams, upper area recedes into darkness, lower gameplay zone kept quiet and mostly negative space, solemn cavernous mood, dark fantasy tech-arcana environment --ar 9:16 --v 7 --style raw --s 125
```

### BG02 - Boss Battle Background

```text
Full-screen mobile game background, vertical 9:16, same ritual chamber but darker and more charged, cracked obsidian floor with a faint violet containment seal under the board area, subtle violet-black haze above, damaged pillars leaking cyan energy through cracks, ominous awakening mood, gameplay zone still clean and mostly negative space, dark fantasy tech-arcana boss environment --ar 9:16 --v 7 --style raw --s 130
```

---

# WEEK 2 - RETENTION / MAP / SHOP / CHARACTERS

## Simplified direction for the next batch

Use the same structure and keep each prompt focused on:
1. **What the asset is**
2. **What silhouette it must have**
3. **What material language it uses**
4. **What the contained glow does**
5. **What gameplay readability rule matters**

Do **not** repeat generic rendering phrases three times.

### Example: Map Node Normal

```text
Small circular level node icon, dark obsidian disc, thin silver-white ring, tiny cyan center dot, brushed metal texture, clean unlocked-state readability, isolated on transparent background, dark fantasy tech-arcana mobile game UI --ar 1:1 --v 7 --style raw --s 100
```

### Example: Shop Offer Card

```text
Game shop offer card container, dark obsidian card surface, thin silver-white border, subtle inner bevel, structured upper image area and lower price area separated by a thin line, clean premium monetization UI, isolated on transparent background, dark fantasy tech-arcana mobile game UI --ar 4:5 --v 7 --style raw --s 90
```

### Example: Character Portrait Formula

```text
Upper-body character portrait, dark void background, strong readable silhouette, one defining color energy, controlled expression, premium material contrast between armor and cloth, shallow depth mood, dark fantasy tech-arcana AAA mobile game art --ar 1:1 --v 7 --style raw --s 175
```

---

## 5) Practical Improvement Notes

### What was improved from the original doc
- Removed redundant phrases like repeating "hyper-detailed, sharp focus, 8K render" in every line unless useful
- Reduced over-explaining of lighting when silhouette and material mattered more
- Kept the parts that actually steer the model:
  - silhouette
  - material
  - containment glow
  - scale readability
  - exclusions

### What still matters most in testing
Run first-pass tests on:
1. Dice tile
2. Rune tile
3. Glass blocker layer 1
4. Horizontal line special
5. Default battle background

These will tell you quickly whether the style lock is actually holding.

### Best workflow
1. Generate 4 variants
2. Pick best silhouette
3. Refine material language
4. Refine glow containment
5. Downscale and test readability

---

## 6) One-Line Master Direction

```text
Dark fantasy puzzle UI with obsidian, carved stone, forged metal, restrained cyan-violet containment glow, readable silhouettes, premium mobile game polish, no cute, no chrome, no casino energy
```

