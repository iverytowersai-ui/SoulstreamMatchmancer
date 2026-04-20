# 🔮 MIDJOURNEY V7 — PRO SECRETS & ADVANCED TECHNIQUES
## The Hidden Arsenal That Separates Amateurs From Pros
### For Soulstream: Magick Dice Editorial & Game Marketing Art

---

> **"The difference between a good AI image and a jaw-dropping one isn't the prompt — it's the 20 secret parameters and techniques that 99% of users never discover."**

---

# 🏆 TIER 1: THE GAME-CHANGERS (Most Impactful Secrets)

## 🔥 Secret #1: The Filename Hack — Instant Photorealism

**What the pros know:** Start your prompt with a realistic camera filename, and Midjourney assumes you want an authentic photograph.

```
IMG_4358.CR2, photo taken so real no camera can compare...
```
or
```
IMG_02202021.HEIC, deleted production still from unreleased HBO series...
```

**Why it works:** Midjourney has been trained on massive datasets of real photos that include filename metadata. When it sees `IMG_4358.CR2` (a Canon RAW file) or `.HEIC` (iPhone format), it shifts its entire generation bias toward genuine photographic output — natural lighting, real camera noise, authentic depth-of-field.

**🎮 Soulstream Application:**
Your prompts already use "photo taken so real no camera can compare" — adding a filename prefix **before** that phrase amplifies the effect:
```
IMG_7291.CR2, photo taken so real no camera can compare, hyper-realistic, 
ultra-detailed, natural skin texture...
```

---

## 🔥 Secret #2: The `--exp` Parameter — Experimental Aesthetics Engine

**What it is:** A brand-new V7 parameter that turbochages detail, texture, and overall aesthetic quality. It's separate from `--stylize` and stacks on top of it.

| `--exp` Value | Effect |
|---|---|
| `--exp 5` | Subtle detail boost. Safe for editorial. |
| `--exp 10` | Noticeable texture enhancement. Sweet spot for portraits. |
| `--exp 25` | Strong aesthetic push. Cinematic lighting gets richer. |
| `--exp 50` | Maximum detail. Can overpower other settings — use carefully. |

**🎮 Soulstream Application:**
Add `--exp 10` to your editorial portraits for richer skin texture and lighting:
```
...photorealistic, hyper-detailed --ar 2:3 --v 7 --style raw --s 250 --exp 10
```

---

## 🔥 Secret #3: `--personalize` — Your AI Learns YOUR Eye

**The secret:** After rating ~200 image pairs on Midjourney's website, you unlock a "Global Profile" that subtly adjusts ALL your generations to match your personal aesthetic preferences.

**How to activate:** 
1. Go to midjourney.com/rank
2. Rate 200+ image pairs (takes ~15 minutes)
3. It auto-activates on V7 thereafter
4. Your generations will subtly shift toward your preferred mood, color tone, and composition style

**Why pros use it:** It creates a consistent "signature look" across all your generations without adding extra prompt words. Your Soulstream characters will all share a subtle visual DNA.

**Override with:** `--p 0` to disable temporarily, or use `--p` explicitly in earlier versions.

---

## 🔥 Secret #4: Prompt Weighting with `::` — Surgical Control

**The hidden grammar most users never learn.** Double colons let you tell Midjourney EXACTLY how important each element is.

**Syntax:**
```
element_A::weight_A element_B::weight_B
```

**🎮 Soulstream Example — Blue's Hero Shot:**
```
piercing bright blue eyes with oceanic depth::2 
storm lightning crackling around his body::1.5 
black leather motorcycle jacket over bare chest::1.3 
moody urban alley background::0.8
dramatic chiaroscuro lighting::1.5
--ar 2:3 --v 7 --style raw --s 250
```

This makes Midjourney prioritize the **eyes** (weight 2) above everything, then the **lightning** and **lighting** (1.5), then the **outfit** (1.3), and treat the **background** as less important (0.8).

**Negative Weights — The Hidden Eraser:**
```
cartoon style::-0.5  plastic skin::-0.5  3D render::-0.7
```
Negative weights actively push Midjourney AWAY from those aesthetics. More powerful than `--no` because you control the intensity.

---

## 🔥 Secret #5: The `/shorten` Command — X-Ray Your Prompts

**What it does:** Paste any prompt into `/shorten` and Midjourney reveals which words actually matter and which are being ignored.

**Why this is gold:** Your Soulstream prompts are beautifully detailed (~150+ words each). But Midjourney doesn't weight every word equally. `/shorten` shows you that maybe "ACEScg color" is being ignored while "piercing blue eyes" is doing all the heavy lifting.

**Pro workflow:**
1. Run `/shorten` on your existing prompt
2. See which tokens Midjourney highlights as impactful
3. Remove dead weight words
4. Double-down on the impactful ones using `::` weights
5. Get BETTER results with SHORTER prompts

---

# 🏆 TIER 2: CONSISTENCY & REFERENCE ENGINE

## 🎯 Secret #6: `--oref` (Omni Reference) — The Consistency God

**This is the single most important parameter for Soulstream.**

`--oref` lets Midjourney "memorize" a character, object, or element and reproduce it consistently across unlimited generations.

**Syntax:**
```
[your prompt] --oref [image_url] --ow 100
```

**The `--ow` (Omni Weight) sweet spot guide:**

| `--ow` Value | Use When... |
|---|---|
| `50-80` | You want the vibe/identity but creative freedom on pose |
| `100` (default) | Standard character consistency |
| `150-250` | Tighter lock on features + outfit |
| `300-400` | Maximum consistency. Above 400 gets unpredictable. |

**🎮 Soulstream Workflow:**
1. Generate Blue's P1 (Hero Editorial Portrait) until you get THE perfect shot
2. Use that image URL as `--oref` for ALL subsequent Blue prompts
3. Now P2, P3, P4, A1, A2, A3, A4 all maintain Blue's exact face, hair, and costume

**⚠️ Limitation:** Uses 2x GPU time. Only 1 image reference at a time.

---

## 🎯 Secret #7: `--cref` + `--cw` — Character Reference (Precision Face Lock)

**Different from `--oref`:** While `--oref` is the all-purpose consistency tool, `--cref` is specifically designed for CHARACTER face consistency.

```
[your prompt] --cref [image_url] --cw 100
```

**The `--cw` (Character Weight) precision guide:**

| `--cw` Value | What It Does |
|---|---|
| `0` | Face only — ignores hair, clothing, everything else |
| `50` | Face + general vibe, but flexible on outfit/hair |
| `100` (default) | Full character lock — face, hair, clothing, everything |

**🎮 Pro Combo for Soulstream:**
```
--cref [blue_hero_url] --cw 0 --oref [blue_outfit_url] --ow 150
```
This locks Blue's **face** via `--cref` but uses a SEPARATE reference for his **outfit** via `--oref`. Maximum control.

---

## 🎯 Secret #8: `--sref` — Style Reference (Brand DNA Lock)

Makes ALL your Soulstream images share a unified visual language — same color grading, same mood, same "show."

```
[your prompt] --sref [your_best_image_url] --sw 100
```

**`--sw` (Style Weight) guide:**

| `--sw` Value | Effect |
|---|---|
| `50` | Light style influence. Your prompt still dominates. |
| `100` (default) | Balanced blend of style reference and prompt. |
| `200-400` | Heavy style lock. The reference image's mood takes over. |
| `--sref random` | Wild card — Midjourney picks a random aesthetic. Great for exploration. |

**🎮 Soulstream Strategy:**
1. Generate one "golden image" that perfectly captures the HBO dark fantasy editorial look
2. Use its URL as `--sref` on every single character prompt
3. Now Blue, Kaery, Chanel, Crux, Kimmy — they all look like they belong in the same show

---

# 🏆 TIER 3: WORKFLOW ACCELERATION

## ⚡ Secret #9: Draft Mode — 10x Faster, Half the Cost

**What it does:** Generates images at reduced resolution, 10x faster, at 50% token cost.

**Pro workflow:**
1. **Draft Mode ON** → Test 20-30 prompt variations rapidly
2. Find the magic combination of words and weights
3. **Draft Mode OFF** → Generate the final high-res version
4. One-click upscale to full quality

**How:** Toggle Draft Mode in Midjourney's web UI settings, or use lower `--q` values:
- `--q 0.25` → Fastest drafts (concept exploration)
- `--q 0.5` → Quick drafts (composition check)
- `--q 1` → Standard quality
- `--q 2` → Enhanced detail
- `--q 4` → Maximum fidelity (final renders only)

---

## ⚡ Secret #10: `--repeat` — Batch Generation

Generate multiple sets from one prompt automatically:

```
[your prompt] --repeat 4
```

Generates 4 separate sets of 4 images (= 16 total). Perfect for:
- Exploring variations of a hero shot
- Finding the perfect expression/pose
- A/B testing prompt tweaks

**Combine with `--chaos 25`** for even more variety in each batch.

---

## ⚡ Secret #11: `--chaos` vs `--weird` — Controlled Creative Explosion

| Parameter | What It Does | Best Value Range |
|---|---|---|
| `--chaos 0` | All 4 grid images look similar | Default |
| `--chaos 25-50` | Moderate variety in poses/compositions | **Sweet spot for editorial** |
| `--chaos 100` | Wildly different interpretations | Brainstorming only |
| `--weird 0` | Standard aesthetic | Default |
| `--weird 250-500` | Slightly unconventional, artistic edge | **Great for dark fantasy** |
| `--weird 1000+` | Truly bizarre, surreal output | Experimental art only |

**🎮 Soulstream Application:**
For Chanel's dark supernatural character, try `--weird 300` to push the nightmare/horror aesthetic into unexpected territory:
```
...dark fantasy seduction, prestige horror-drama --ar 2:3 --v 7 --style raw --s 300 --weird 300
```

---

# 🏆 TIER 4: EDITORIAL PHOTOGRAPHY SECRETS

## 📸 Secret #12: The "Deleted Scene" Technique

**Your prompts already use this — but here's why it works and how to push it further.**

Phrases like "deleted production still from unreleased HBO dark fantasy series" work because they frame the image as a REAL photograph from a REAL production. Midjourney shifts from "generating art" to "recreating a photograph."

**Power variants to rotate:**
- `leaked behind-the-scenes still from [Netflix/HBO/A24] production`
- `editorial_stills_archive` ← acts like a database tag
- `BTS polaroid from movie set, cinematographer's personal collection`
- `unprocessed RAW file recovered from [director's name] hard drive`

---

## 📸 Secret #13: Cinema Camera Bible — Choose Your "Film Stock"

Different cameras and lenses produce DRAMATICALLY different looks in Midjourney:

### For Emotional Portraits (Blue P4, Kaery P4):
```
shot on ARRI Alexa 65 with Cooke S7/i 100mm T2.0
```
Cooke lenses = warm, "Cooke Look" that flatters skin.

### For Epic Wide Shots (Blue P2, power stances):
```
shot on RED V-Raptor 8K with Angénieux Optimo Ultra 24-460mm
```
RED + Angénieux = ultra-sharp, cinematic, wide-range zoom feel.

### For Intimate Close-Ups with Character:
```
shot on Panavision Millennium DXL2 with Panavision C Series Anamorphic
```
Anamorphic = signature oval bokeh, horizontal lens flares, cinematic widescreen feel.

### For Raw, Documentary Feel:
```
shot on Canon C500 Mark II with Canon CN-E 85mm T1.3
```
Canon cinema = slightly warmer, documentary authenticity.

### For Fashion Editorial:
```
shot on Hasselblad H6D-400c with HC 100mm f/2.2
```
Medium format = insane detail, shallow DOF, fashion photography staple.

---

## 📸 Secret #14: The Imperfection Injection

**The single biggest tell of AI images is perfection.** Pro artists deliberately add flaws:

**Skin realism triggers:**
```
natural skin texture with visible pores, micro-imperfections, 
subtle freckles, flyaway hairs catching backlight, 
sweat beads on temple, slight asymmetry in features
```

**Camera realism triggers:**
```
subtle chromatic aberration on edges, natural lens vignette, 
micro film grain ISO 800, slight motion blur on hands, 
lens breathing artifact, dust particles in volumetric light
```

**Environmental realism triggers:**
```
water condensation on nearby surfaces, scuffed and worn materials, 
weathered leather with cracks, fabric wrinkles following body contours,
chipped nail polish, tangled hair strands
```

---

## 📸 Secret #15: Light Layering — The Cinematic Recipe

**Amateur prompts:** "dramatic lighting"
**Pro prompts:** Layer 3-4 specific light sources:

```
Key: warm tungsten 3200K practicals from the left at 45 degrees,
Fill: cool moonlight 6500K ambient from upper right,
Rim: hard backlight creating a sharp silhouette edge,
Accent: motivated neon sign spill in deep crimson from background
```

**🎮 Soulstream — Blue's Signature Lighting Recipe:**
```
warm golden key light catching his left cheekbone at 30 degrees::1.3,
cool electric blue rim light outlining his right shoulder suggesting storm powers::1.5,
volumetric atmospheric haze with dust particles catching cross-light::1.0,
wet pavement creating specular reflections of both light sources below::0.8
```

---

# 🏆 TIER 5: ADVANCED COMPOSITION TECHNIQUES

## 🎨 Secret #16: The "Through Their Eyes" Technique

**You asked for images you can "see through the eyes" — here's how:**

**First-person perspective triggers:**
```
POV shot looking directly into the camera, the subject's gaze 
piercing through the fourth wall, eye contact that pulls the 
viewer INTO the scene, as if the viewer is standing one meter 
away face-to-face, intimate confrontational framing
```

**Emotional connection triggers:**
```
catchlight in eyes reflecting the scene environment,
the subject's expression speaking directly to the viewer,
a gaze that makes the viewer feel SEEN,
uncomfortable intimacy of a close friend's stare
```

**🎮 Soulstream — Making Characters Feel REAL:**
```
...piercing bright blue eyes locked directly onto camera with 
unflinching eye contact::2, the kind of gaze that makes you feel 
he can see right through you::1.5, catchlights reflecting the 
city lights behind the viewer::1.2...
```

---

## 🎨 Secret #17: Magnetic Composition — The "Can't Look Away" Formula

**The advertising industry's secret composition rules applied to Midjourney:**

1. **The Triangle Rule** — Place the character's eyes, their weapon/power source, and one environmental detail in a triangle formation
2. **Leading Lines** — Use environmental elements (lightning bolts, cracks in ground, building edges) pointing toward the character's face
3. **Color Isolation** — Make the character's signature color the ONLY instance of that hue in the frame
4. **Negative Space** — Leave "breathing room" in the composition for the eye to rest

**Prompt implementation:**
```
composition follows the golden ratio, the character positioned 
at the power point of the rule of thirds, leading lines from 
environmental elements directing toward the subject's eyes,
selective color isolation on [character's signature color] 
against a complementary muted background
```

---

# 🏆 TIER 6: NIJI 7 ANIME SECRETS

## 🎌 Secret #18: Niji 7 Exclusive Parameters

**Niji 7 (January 2026) has its own hidden strengths:**

- **Improved Coherency:** V7 Niji renders complex multi-element anime scenes without breaking
- **Cleaner Linework:** Specify `clean linework, precise line weight variation` for professional manga quality
- **Animation Studio Triggers:** Naming specific studios radically changes the output:

| Studio Trigger | Visual Effect |
|---|---|
| `Ufotable` | Hyper-detailed particle effects, epic battle scenes (Demon Slayer) |
| `Studio MAPPA` | Dynamic cinematography, dark tone (Jujutsu Kaisen, Attack on Titan S4) |
| `Studio Bones` | Fluid action, bright heroic energy (My Hero Academia) |
| `WIT Studio` | Cinematic scale, environmental grandeur |
| `Kyoto Animation` | Beautiful lighting, emotional close-ups, slice-of-life beauty |
| `TRIGGER` | Explosive energy, exaggerated poses, punk aesthetic (Kill la Kill) |

---

## 🎌 Secret #19: The `--niji 7 --s 400+` Secret

While standard MJ uses `--s 200-300`, Niji 7 responds exceptionally well to HIGHER stylize values:

```
--niji 7 --s 400   → Rich, polished splash art
--niji 7 --s 500   → Maximum anime "beauty" and detail
```

Combine with your existing prompts for richer game marketing art:
```
...character splash screen quality, mobile game promotional key art 
--ar 2:3 --niji 7 --s 450
```

---

# 🏆 TIER 7: THE ULTIMATE PRO WORKFLOW

## 🔄 The "Triple Lock" — Soulstream Character Pipeline

**Step 1: DISCOVER** (Draft Mode + Chaos)
```
[base prompt] --v 7 --style raw --s 200 --chaos 40 --q 0.5
```
Generate 40+ variations fast and cheap. Find the magic.

**Step 2: REFINE** (Full quality + /shorten)
```
[refined prompt after /shorten analysis] --v 7 --style raw --s 250 --q 2
```
Run 4-8 times until you get THE hero shot.

**Step 3: LOCK** (oref + sref + cref)
```
[prompt] --oref [hero_image_url] --ow 150 --sref [style_image_url] --sw 100 --v 7 --style raw --s 250
```
Now generate ALL 8 prompts for that character with perfect consistency.

**Step 4: POLISH** (Vary Region + Upscale)
Use Midjourney's Vary Region to fix any small issues (wrong hand, off-color detail) without regenerating the whole image.

---

# 📋 QUICK-REFERENCE CHEAT SHEET

## Parameter Stack for Soulstream Editorial
```
--v 7 --style raw --s 250 --exp 10 --q 2 --ar 2:3
```

## Parameter Stack for Soulstream Anime
```
--niji 7 --s 400 --q 2 --ar 2:3
```

## Consistency Stack (after hero shot is locked)
```
--oref [url] --ow 150 --sref [style_url] --sw 100
```

## Exploration Stack (finding the magic)
```
--v 7 --style raw --s 200 --chaos 35 --q 0.5 --repeat 4
```

## Anti-AI Detection Stack (maximum realism)
```
IMG_7291.CR2, [prompt], natural skin texture with visible pores, 
subtle film grain, micro lens imperfections, flyaway hairs 
--v 7 --style raw --s 200 --exp 10 --no cartoon, illustration, 
3d render, plastic skin, smooth skin, airbrushed
```

---

# 💡 GOLDEN RULES FROM THE PROS

1. **Change ONE variable per iteration.** Never change the prompt AND the parameters at the same time.
2. **Front-load the important stuff.** Words at the START of the prompt have more influence.
3. **Use `--no` aggressively.** `--no cartoon, illustration, 3d render, plastic, doll, airbrushed` forces photorealism.
4. **The `/shorten` command is your best friend.** Use it to eliminate dead weight from every prompt.
5. **Collect style reference images obsessively.** The best `--sref` is not a Midjourney image — it's a real movie still or magazine photo.
6. **Never skip Draft Mode.** Spending 50 tokens testing is better than spending 500 on bad generations.
7. **Consistency requires commitment.** Generate the hero shot first, then use `--oref` on everything.
8. **Imperfection = Realism.** Every time you add a "flaw" to the prompt, the image gets more believable.
9. **Light > Subject.** A boring subject with perfect lighting looks professional. A great subject with flat lighting looks amateur.
10. **The prompt is only 30% of the result.** The other 70% is parameters, references, and iteration.

---

*Soulstream: Magick Dice — Ivery Towers LLC*
*Generated: March 4, 2026*
*Classification: Internal Creative Reference*
