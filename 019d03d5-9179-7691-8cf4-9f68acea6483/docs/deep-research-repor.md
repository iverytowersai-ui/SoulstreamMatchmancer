# Leonardo AI – Deep Research Report
## Comprehensive Guide to Models, Presets & Prompt Engineering

---

# Executive Summary

Leonardo AI offers a **suite of cutting-edge generative models** for images and video. This report provides an exhaustive breakdown of every available model — including **Featured Models** (Lucid Origin, GPT Image-1.5, Seedream 4.5, Nano Banana Pro, Seedream 4.0, Nano Banana, Lucid Realism, FLUX.2 Pro) and **Other Models** (Ideogram 3.0, GPT-Image-1, FLUX.1 Kontext Max, FLUX.1 Kontext, FLUX Dev, FLUX Schnell, Phoenix 1.0, Phoenix 0.9) — along with all **Style Presets** (Anime, Cinematic Kino, Concept Art, Graphic Design, Illustrative Albedo, Leonardo Lightning, Lifelike Vision).

Each entry covers capabilities, strengths, limitations, best use-cases, and prompt techniques. The guide also includes cross-model prompt-engineering principles, iteration workflows, troubleshooting tips, evaluation metrics, and a comprehensive cheat-sheet.

---

# 1. Featured Models

These are Leonardo AI's top-tier, most capable image generation models.

## 1.1 Lucid Origin
| Attribute | Detail |
|---|---|
| **Developer** | Leonardo AI (proprietary) |
| **Type** | Text-to-Image |
| **Resolution** | Full HD |
| **Availability** | All plans (including free tier) |

**Overview:** Lucid Origin is Leonardo AI's flagship aesthetic generalist model. It is designed to be the ultimate all-rounder — excelling at photorealistic, digital art, hand-drawn, and stylized 3D outputs with equal competence.

**Strengths:**
- Robust prompt responsiveness with excellent adherence to instructions
- Rich, vibrant color saturation and sharp details
- Strong text rendering and legibility within generated images
- Versatile across styles: realism, illustration, concept art, graphic design
- Clean, polished outputs ideal for marketing visuals and portfolio work

**Limitations:**
- Slightly neutral by default — may need explicit style guidance for highly specific aesthetics
- Capped at Full HD resolution (no native 4K)

**Best Use Cases:** Marketing visuals, illustrations, portraits, product mockups, social media content, general-purpose creative work.

**Prompt Tips:**
- Emphasize aesthetic terms: *"surreal," "vibrant," "cinematic"*
- Add camera/photography details: *"DSLR, 50mm lens, high contrast, bokeh"*
- Example: *"An elegant vintage car parked under cherry blossoms, film grain, sunset lighting, dramatic angle, ultra HD"*

---

## 1.2 GPT Image-1.5
| Attribute | Detail |
|---|---|
| **Developer** | OpenAI |
| **Type** | Text-to-Image, Image Editing |
| **Architecture** | Integrated into GPT-5 |
| **Speed** | Up to 4x faster than GPT Image-1 |

**Overview:** OpenAI's most advanced image generation model, released December 2025. Integrated directly into GPT-5's architecture, it brings unmatched language understanding to image generation.

**Strengths:**
- Extremely fast processing (up to 4x faster than its predecessor)
- Deep contextual layout precision — follows complex multi-element instructions accurately
- Region-aware editing: modify specific parts of an image while preserving the rest
- Excellent readable text rendering within images
- High structural integrity for complex UI mockups and design layouts
- Superior lighting consistency and style uniformity across outputs

**Limitations:**
- May not match the extreme hyper-realism of dedicated photorealistic models (e.g., Seedream 4.5)
- Cloud-only via API

**Best Use Cases:** Complex layouts, UI/UX mockups, detailed infographics, text-heavy designs, multi-element compositions, rapid iteration workflows.

**Prompt Tips:**
- Leverage its layout understanding: specify spatial relationships explicitly
- Example: *"A product landing page mockup: hero image of a smartwatch on the left, with the heading 'TimeX Pro' on the right, clean white background, modern sans-serif typography, high detail"*

---

## 1.3 Seedream 4.5
| Attribute | Detail |
|---|---|
| **Developer** | ByteDance |
| **Type** | Text-to-Image, Image Editing |
| **Resolution** | Native 4K+ |
| **Key Feature** | Character consistency across generations |

**Overview:** The latest in ByteDance's Seedream series, Seedream 4.5 is a powerhouse for photorealistic image generation with industry-leading character consistency.

**Strengths:**
- Exceptional photorealistic output with cinematic lighting and realistic shadows
- **Character consistency**: maintains character identity across multiple images using up to 10 reference images, preventing "character drift"
- Professional-grade inpainting and outpainting capabilities
- Preserves facial features, lighting, and color tones from references
- Marketing-ready outputs with crisp text and clean visuals
- Stable characters, objects, and environments across multiple generations

**Limitations:**
- Computationally heavy for highest quality outputs
- Best results require multiple reference images for character consistency

**Best Use Cases:** Commercial advertising, product photography, character-driven series, e-commerce imagery, brand consistency projects, film concept art.

**Prompt Tips:**
- Use its character reference system: upload multiple angles of a character
- Example: *"A confident young woman in a red leather jacket walking through a neon-lit Tokyo street at night, cinematic lighting, Seedream photorealistic, 4K"*

---

## 1.4 Nano Banana Pro (Google Gemini 3 Pro)
| Attribute | Detail |
|---|---|
| **Developer** | Google DeepMind |
| **Type** | Text-to-Image, Image Editing |
| **Architecture** | Gemini 3 Pro |
| **Resolution** | Native 4K+ |

**Overview:** The premium tier of Leonardo's Nano Banana line, built on Google's Gemini 3 Pro architecture. Designed for the most demanding creative workflows.

**Strengths:**
- Excellence in photorealism with native 4K+ resolution
- High-fidelity text rendering in multiple languages
- Complex reasoning for precise layouts and intricate compositions
- Superior handling of multi-element scenes with correct spatial logic
- Advanced editing capabilities via natural language instructions

**Limitations:**
- Higher token/credit cost due to model complexity
- Cloud-only processing (no local inference)
- Slower generation times compared to lighter models

**Best Use Cases:** Professional photography-tier outputs, complex multi-element compositions, detailed text-in-image work, multilingual designs, high-resolution print-ready assets.

**Prompt Tips:**
- Take advantage of its reasoning: describe spatial relationships and logical elements
- Example: *"A blueprint-style technical diagram of a spaceship with labeled components in English and Japanese, clean white background, engineering precision, 4K resolution"*

---

## 1.5 Seedream 4.0
| Attribute | Detail |
|---|---|
| **Developer** | ByteDance |
| **Type** | Text-to-Image, Image Editing, Multi-image Composition |
| **Resolution** | Native 4K (2K in 1.8 seconds) |
| **Architecture** | Mixture-of-Experts (MoE) |

**Overview:** The foundational release in ByteDance's Seedream line. A unified multimodal framework integrating text-to-image synthesis, image editing, and multi-image composition.

**Strengths:**
- Ultra-fast generation: 2K resolution in approximately 1.8 seconds
- Unified architecture handles generation, editing, and composition in one model
- Strong text rendering (headlines, captions, labels in multiple languages)
- Multi-reference support (up to 6 input images for richer compositions)
- Natural language editing for background replacement, object manipulation, style changes
- Excellent for photorealistic photography, illustrations, anime, and painterly styles

**Limitations:**
- Character consistency slightly less refined than Seedream 4.5
- Some complex multi-character scenes may show inconsistencies

**Best Use Cases:** Commercial design, advertising, product mockups, e-commerce imagery, rapid content creation, social media assets.

**Prompt Tips:**
- Leverage its speed for rapid iteration cycles
- Example: *"A luxury perfume bottle on a marble surface with soft pink rose petals scattered around, studio lighting, product photography, 4K"*

---

## 1.6 Nano Banana (Google Gemini 2.5 Flash)
| Attribute | Detail |
|---|---|
| **Developer** | Google DeepMind |
| **Type** | Text-to-Image, Image Editing |
| **Architecture** | Gemini 2.5 Flash |
| **Optimization** | Speed and pattern-matching |

**Overview:** The standard-tier Nano Banana model, optimized for speed and practical image editing tasks. Built on Google's Gemini 2.5 Flash architecture.

**Strengths:**
- Superb logical consistency (objects placed correctly in scenes)
- Excellent handling of text/rendered letters in images
- Quick style transfers and edits on existing images
- Strong instruction-following for editing commands
- Good balance of quality and speed for everyday workflows

**Limitations:**
- Lower resolution ceiling than Nano Banana Pro
- Less suited for ultra-complex reasoning tasks
- Cloud-only model

**Best Use Cases:** Concept art, product mockups, narrative scenes, fashion visuals, quick edits and style transfers, daily creative workflows.

**Prompt Tips:**
- Excel at instruction-style editing: *"Remove the background," "Enhance the clouds," "Make the sky stormy"*
- Example: *"A portrait of a wise owl wearing steampunk glasses and a leather jacket, photorealistic, shallow depth-of-field"*

---

## 1.7 Lucid Realism
| Attribute | Detail |
|---|---|
| **Developer** | Leonardo AI (proprietary) |
| **Type** | Text-to-Image |
| **Focus** | Hyper-photorealistic, cinematic analog |

**Overview:** A specialized Leonardo AI model engineered for cinematic, analog-style realism. It simulates light physics and film artifacts to produce images resembling high-end analog camera photography.

**Strengths:**
- Hyper-photorealistic visual clarity
- Simulates real-world light physics accurately
- Film-like artifacts (grain, bokeh, lens distortion) for authentic analog feel
- Exceptional skin textures, micro-detail rendering, and natural expressions
- Story-driven imagery with emotional depth
- Ideal bridge from still images to video generation

**Limitations:**
- Less versatile for stylized or illustration work
- Focused scope means it's not suitable for anime, vector, or abstract styles

**Best Use Cases:** Cinematic portraiture, film stills, emotion-driven narrative scenes, fashion editorial, analog photography simulation, video pre-visualization.

**Prompt Tips:**
- Use film/camera terminology: *"shot on Kodak Portra 400," "35mm film grain," "natural window light"*
- Example: *"A weathered fisherman mending nets at dawn, golden hour backlight, shot on medium format film, shallow depth-of-field, Lucid Realism"*

---

## 1.8 FLUX.2 Pro
| Attribute | Detail |
|---|---|
| **Developer** | Black Forest Labs |
| **Type** | Text-to-Image, Multi-reference Editing |
| **Resolution** | 4 megapixel output |
| **Key Feature** | Up to 9 reference images |

**Overview:** The production-grade model from Black Forest Labs' FLUX line, optimized for cinematic realism and professional workflows.

**Strengths:**
- 4-megapixel photorealistic output at production quality
- Multi-reference image editing: combine up to 9 reference images for complex compositions
- Enhanced text rendering capabilities
- Structural stability with natural proportions even in complex scenes
- Refined textures and precise color matching (supports hex code inputs)
- Superior lighting realization in multi-element compositions

**Limitations:**
- Higher resource consumption for multi-reference workflows
- May require more prompt tuning for abstract or heavily stylized outputs

**Best Use Cases:** High-end advertising, movie-like scene generation, product photography, brand assets requiring exact color matching, professional editorial work.

**Prompt Tips:**
- Specify exact colors using hex codes when precision matters
- Example: *"A luxury car showroom with vehicles in #C41E3A red, soft ambient lighting reflecting off polished marble floors, cinematic wide-angle, FLUX.2 Pro quality"*

---

# 2. Other Models

These models complement the featured lineup with specialized capabilities.

## 2.1 Ideogram 3.0
| Attribute | Detail |
|---|---|
| **Developer** | Ideogram AI |
| **Type** | Text-to-Image |
| **Focus** | Typography, graphic design, conceptual clarity |

**Overview:** A concept-art-focused model exceptional for typographical outputs, logos, and crisp poster-style visuals.

**Strengths:**
- Industry-leading legible typography within generated images
- Clear conceptual visuals with expressive outlines
- Strong graphic design capabilities (posters, infographics, logos)
- Supports negative prompts for fine control
- Step-by-step prompt patterns for layered compositions

**Best Use Cases:** Logo design, poster creation, infographics, storyboards, concept art with text overlays, educational diagrams.

**Prompt Tips:**
- Break complex scenes into numbered steps
- Example: *"A motivational poster with the text 'RISE ABOVE' in bold gothic font, a mountain silhouette at sunrise, clean vector style, vibrant orange gradient background"*

---

## 2.2 GPT-Image-1
| Attribute | Detail |
|---|---|
| **Developer** | OpenAI |
| **Type** | Text-to-Image |

**Overview:** The predecessor to GPT Image-1.5. Still capable for general-purpose image generation with good prompt adherence.

**Strengths:**
- Solid general-purpose image generation
- Good text understanding and prompt adherence
- Reliable for standard creative workflows

**Limitations:**
- Slower than GPT Image-1.5 (up to 4x)
- Less precise region-aware editing
- Lower structural integrity for complex layouts

**Best Use Cases:** General creative work where speed is not critical, simple compositions, standard marketing assets.

---

## 2.3 FLUX.1 Kontext Max
| Attribute | Detail |
|---|---|
| **Developer** | Black Forest Labs |
| **Type** | Image-to-Image Editing |
| **Tier** | Premium |

**Overview:** The premium version of FLUX.1 Kontext, offering higher resolution, more cinematic detail, and maximum prompt accuracy.

**Strengths:**
- Higher resolution output than standard Kontext
- Maximum prompt accuracy without sacrificing speed
- Advanced character consistency across edits
- Richer, more cinematic detail in output
- Precise typography handling in edited images
- Complex visual logic for professional-grade editing

**Best Use Cases:** Professional image editing, cinematic visual editing, brand asset refinement, high-resolution character work.

---

## 2.4 FLUX.1 Kontext
| Attribute | Detail |
|---|---|
| **Developer** | Black Forest Labs |
| **Type** | Image-to-Image Editing |

**Overview:** A context-aware image editing model that uses natural language instructions to modify existing images intelligently.

**Strengths:**
- Natural language-instructed image editing ("change the apple to a pear")
- Multi-character and scene consistency across edits
- Precision editing of specific elements without affecting surroundings
- Iterative editing through multiple conversation turns
- Maintains logical consistency across characters, objects, and environments

**Best Use Cases:** Product image variations, character outfit/expression changes, scene modifications, iterative design refinement.

**Prompt Tips:**
- Be specific about what to change and what to preserve
- Example: *"Change the woman's dress from blue to emerald green, keep the background and lighting exactly the same"*

---

## 2.5 FLUX Dev
| Attribute | Detail |
|---|---|
| **Developer** | Black Forest Labs |
| **Type** | Text-to-Image |
| **Focus** | Quality-focused generation |

**Overview:** The development-oriented model in the FLUX family, prioritizing visual quality and custom training capabilities.

**Strengths:**
- High-quality detailed outputs
- Supports custom training and fine-tuning
- Consistent aesthetic quality across generations
- Good balance of speed and detail

**Best Use Cases:** Custom model training, quality-focused workflows, aesthetic exploration, fine-tuned brand-specific generation.

---

## 2.6 FLUX Schnell
| Attribute | Detail |
|---|---|
| **Developer** | Black Forest Labs |
| **Type** | Text-to-Image |
| **Focus** | Speed-optimized generation |

**Overview:** The speed-optimized model in the FLUX family. "Schnell" (German for "fast") prioritizes rapid generation for high-throughput workflows.

**Strengths:**
- Fastest generation in the FLUX family
- Good quality-to-speed ratio
- Ideal for rapid prototyping and batch generation
- Low token/credit cost per generation

**Best Use Cases:** Rapid prototyping, concept exploration, batch generation, thumbnail creation, iterative brainstorming.

---

## 2.7 Phoenix 1.0
| Attribute | Detail |
|---|---|
| **Developer** | Leonardo AI (proprietary) |
| **Type** | Text-to-Image |
| **Tier** | Flagship foundational model |

**Overview:** Leonardo AI's flagship proprietary foundational model — the backbone of their image generation platform.

**Strengths:**
- Best-in-class prompt adherence across all Leonardo models
- Exceptional in-image text rendering (coherent, legible text)
- "Edit with AI" iterative editing capabilities
- Strong photorealistic baseline
- Supports various styles from cinematic realism to stylized art
- High-resolution output

**Best Use Cases:** General-purpose high-quality generation, text-in-image work, iterative editing workflows, professional creative production.

**Prompt Tips:**
- Leverage its prompt adherence: be as detailed as desired without fear of "confusing" the model
- Example: *"A cozy coffee shop interior, warm lighting, wooden tables, a chalkboard menu reading 'Today's Special: Lavender Latte,' steam rising from a ceramic cup, photorealistic"*

---

## 2.8 Phoenix 0.9
| Attribute | Detail |
|---|---|
| **Developer** | Leonardo AI (proprietary) |
| **Type** | Text-to-Image |

**Overview:** The earlier iteration of Phoenix, still available for users who prefer its specific aesthetic characteristics.

**Strengths:**
- Solid prompt adherence (slightly less refined than 1.0)
- Good text rendering capabilities
- Established aesthetic that some users may prefer
- Lower resource consumption than Phoenix 1.0

**Best Use Cases:** Legacy workflows, users who prefer Phoenix 0.9's specific aesthetic, lower-cost generation runs.

---

# 3. Style Presets & Elements

Leonardo AI offers powerful style presets that act as fine-tuning filters, dramatically altering the aesthetic of outputs.

## 3.1 Anime
| Attribute | Detail |
|---|---|
| **Underlying Model** | Leonardo Anime XL, Anime Pastel Dream |
| **Style** | Japanese animation / manga |
| **Sub-styles** | Cell-shaded Anime, Soft Cell Anime |

**Description:** Produces images with characteristics common to Japanese animation — bold colors, distinct line art, expressive eyes, dynamic poses, and anime-specific stylizations. Offers sub-style variations from hard cell-shading to soft pastel illustrations.

**Best For:** Character portraits, fan art, avatars, manga panels, anime-style game assets.

**Prompt Tips:**
- Specify sub-styles: *"cell-shaded anime"* vs *"soft pastel anime"*
- Example: *"A magical girl warrior with flowing pink hair, dynamic action pose, cherry blossom particles, cell-shaded anime style, vibrant colors"*

---

## 3.2 Cinematic Kino
| Attribute | Detail |
|---|---|
| **Underlying Model** | Leonardo Kino XL |
| **Style** | Film / movie stills |
| **Aesthetic** | Dramatic lighting, bokeh, depth-of-field |

**Description:** Engineered for cinematic-style imagery with a focus on producing film-like scenes. Creates hyperrealistic or photorealistic results with dramatic lighting, natural bokeh, and depth-of-field effects.

**Best For:** Movie poster concepts, film stills, dramatic portraits, scene visualization, storyboarding.

**Prompt Tips:**
- Works exceptionally well with concise, keyword-based prompts
- Add HDR or Moody modifiers for distinct results
- Example: *"A lone detective standing under a flickering streetlight, rain-soaked alley, film noir, dramatic shadows, Cinematic Kino"*

---

## 3.3 Concept Art
| Attribute | Detail |
|---|---|
| **Style** | Painterly, conceptual |
| **Focus** | Shape language, atmosphere, mood |

**Description:** Produces stylized, painterly outputs emphasizing mood, shape language, and narrative hints over strict photorealistic detail. Ideal for the ideation phase of creative projects.

**Best For:** Game concept art, character design exploration, environment design, world-building visualization, pre-production art.

**Prompt Tips:**
- Focus on mood and atmosphere rather than precise detail
- Example: *"A forgotten underwater temple, bioluminescent coral and ancient stone pillars, mysterious blue-green atmosphere, concept art style"*

---

## 3.4 Graphic Design
| Attribute | Detail |
|---|---|
| **Style** | Clean vectors, bold palettes |
| **Sub-styles** | Pop Art, Vector |
| **Focus** | Typography, layout, composition |

**Description:** Optimized for structured, clean, and visually striking imagery with high contrast and bold palettes. Offers sub-style variations including Graphic Design Pop Art and Graphic Design Vector.

**Best For:** Poster design, logo creation, layout visualization, UI icon generation, branding assets, social media graphics.

**Prompt Tips:**
- Specify design constraints: *"flat design," "vector style," "limited color palette"*
- Example: *"A minimalist tech company logo, geometric shapes forming the letter 'A,' electric blue on white background, clean vector style, Graphic Design preset"*

---

## 3.5 Illustrative Albedo
| Attribute | Detail |
|---|---|
| **Underlying Model** | AlbedoBase XL |
| **Style** | Vivid digital illustration |
| **Sub-styles** | 16 distinct illustrative styles |

**Description:** "Albedo" refers to color value without lighting effects. This preset produces images with pure, vivid, highly saturated colors and distinct CG texturing — leaning towards digital illustration techniques without heavy photorealistic ambient lighting or shadow passes.

**Best For:** Colorful digital illustrations, character art, game asset design, vivid concept pieces, stylized portraits.

**Prompt Tips:**
- Emphasize color and form over lighting realism
- Example: *"A forest spirit with antlers made of crystal, surrounded by glowing fireflies, vivid saturated colors, illustrative style, no photorealistic shadows"*

---

## 3.6 Leonardo Lightning
| Attribute | Detail |
|---|---|
| **Underlying Model** | Leonardo Lightning XL |
| **Speed** | 2–3x faster generation |
| **Variants** | Generalist (Lightning XL), Anime-specific (Anime XL) |

**Description:** Focused entirely on maximizing generation speed. These fine-tuned models increase image generation speed by 2–3x while maintaining quality and reducing token costs. Available in both generalist photorealistic and anime-specific variants.

**Best For:** Rapid prototyping, concept exploration, batch generation, iterative brainstorming, thumbnail creation.

**Prompt Tips:**
- Great for quickly exploring multiple prompt variations
- Use for initial ideation, then switch to a higher-quality model for final outputs
- Example: *"Quick concept: a cyberpunk samurai in neon rain, dynamic pose, Leonardo Lightning"*

---

## 3.7 Lifelike Vision
| Attribute | Detail |
|---|---|
| **Underlying Model** | Vision XL, Lucid Realism configurations |
| **Style** | Hyper-photorealistic |
| **Sub-styles** | 20 different photorealistic styles |

**Description:** Pushes for absolute hyper-realism. Focuses on rendering accurate textures, skin details, micro-details, and realistic lighting to produce images virtually indistinguishable from high-quality photographs. Includes 20 sub-styles for different photorealistic aesthetics.

**Best For:** Portrait photography simulation, product photography, architectural visualization, fashion editorial, stock photography creation.

**Prompt Tips:**
- Include micro-detail keywords: *"skin pores visible," "individual hair strands," "light refractions"*
- Example: *"Close-up portrait of an elderly man with deep wrinkles, kind eyes, natural window light, skin pores visible, shot on Phase One medium format, Lifelike Vision"*

---

# 4. General Prompt-Engineering Principles

Across all models, the same **foundations** apply:

- **Be Specific & Descriptive:** Give clear details on *subject, action, and context*. E.g. instead of "dog on couch," try *"a golden retriever puppy sitting on a navy velvet couch in a sunlit living room."*
- **Use Style Modifiers:** Add genre/style tags: "photorealistic," "digital art," "manga style," "cinematic." Leonardo responds well to artistic cues (e.g. *"oil painting," "3D render," "film noir"*).
- **Iterative Refinement:** Rarely is the first prompt perfect. Generate, review, tweak the prompt and rerun. Use step-by-step breakdowns for complex scenes.
- **Balance Detail vs. Creativity:** Overloading the prompt can confuse the model; too little detail makes it guess. Aim for concise but vivid descriptions.
- **Vocabulary Matters:** Use concrete nouns and vivid adjectives. Swap vague words ("beautiful") for specifics ("golden-orange sunset over cliffs").
- **Leverage Negative Prompts:** Use them to *exclude* unwanted elements. Common tokens: *"blurry, lowres, artifact, watermark, extra limbs."*
- **Adjust Hyperparameters:** Tweak *CFG/Guidance Scale* (12–15 for strict adherence, 5–8 for creativity), *samplers* (Euler, DDIM), and *steps* (50–100).
- **Seeding for Consistency:** Use a fixed random seed to reproduce results. Different seeds for variations.
- **Frame-by-Frame Prompts (Video):** For video models, treat each shot as a mini-prompt with consistent style/character descriptions.

---

# 5. Model-Specific Prompt Patterns & Examples

## 5.1 Nano Banana / Nano Banana Pro
- **Structure:** *Subject + Action + Context + Style*
- **Editing:** Prepend instructions: *"Remove the background," "Make the sky stormy"*
- **Inpainting:** Upload reference + transformation prompt: *"Turn this daytime scene into a night street with rain"*

## 5.2 Lucid Origin / Lucid Realism
- **Structure:** Emphasize aesthetic terms and camera details
- **Modifiers:** *"surreal," "vibrant," "DSLR," "50mm lens," "high contrast"*

## 5.3 Seedream 4.0 / 4.5
- **Structure:** Subject + Style + Quality markers
- **Character Consistency (4.5):** Upload multiple reference angles, describe consistent attributes

## 5.4 FLUX Models
- **FLUX.2 Pro:** Specify exact colors (hex codes), multi-reference compositions
- **FLUX.1 Kontext:** Natural language editing instructions with clear change/preserve boundaries

## 5.5 Video Models (Sora/Veo/Kling/O3)
- **Shot-by-Shot:** *"Shot 1: [description]. Shot 2: [description]. [Global style/music]."*
- **Scene Description (Veo):** Single-shot continuous scene prompts
- **Camera Directions:** *"pan right," "slow zoom," "wide angle"*

---

# 6. Iteration Workflow

A step-by-step cycle ensures the best results:

1. **Initial Prompt:** Start with a basic descriptive prompt (subject, setting, mood).
2. **Model/Settings:** Pick the appropriate model. Adjust CFG scale (7–12), sampling steps (50–100), and resolution.
3. **Generate & Review:** Check if subject is present, style is correct, artifacts are absent.
4. **Refine Prompt:** Add missing descriptors, use negative prompts, change style terms.
5. **Adjust Hyperparameters:** Increase steps for detail, change sampler if noisy, raise/lower CFG.
6. **Seed Control:** Same seed for comparison, new seed for variety.
7. **Repeat:** Regenerate until the output matches your intent.

---

# 7. Troubleshooting & Common Failures

| Issue | Fix |
|---|---|
| **Subject Missing/Deformed** | Make subject more prominent in prompt. Add "clearly visible," "as the focal point." |
| **Weird Text or Symbols** | Use negative: "no text, no lettering." For wanted text, specify style clearly. |
| **Blurry or Low-detail** | Increase resolution/steps. Add "ultra-detailed, sharp focus, 8k." |
| **Odd Colors/Lighting** | Describe lighting explicitly. Use exact color terms. Negative: "no oversaturation." |
| **Repetitive Objects** | Negative prompt: "no extra person, no repeated buildings." Split prompt into phases. |
| **Bad Proportions** | Add perspective cues: "aerial view, low angle." Say "realistic human anatomy." |
| **Video Jitter** | Mention "consistent characters/lighting." Use same seed across shots. Simplify scene. |
| **Slow Generation** | Lower resolution/steps first, refine best result. Use Lightning preset for speed. |

---

# 8. Evaluation Metrics

- **Prompt Alignment:** Does the output match the prompt? Use CLIP-based scoring or manual checks.
- **Visual Quality:** Sharpness, absence of artifacts, texture detail.
- **Style Fidelity:** Compare to style references and known outputs.
- **Video Coherence:** Temporal consistency, smooth transitions, audio sync.
- **Human Evaluation:** A/B testing, preference scores, user feedback.
- **Technical Checks:** Resolution, aspect ratio, metadata verification.

---

# 9. Cheat-Sheet: Prompt Tokens & Modifiers

| Category | Tokens |
|---|---|
| **General Enrichers** | `ultra-detailed`, `hyper-realistic`, `8k`, `photorealistic`, `trending on artstation`, `award-winning photo`, `sharp focus` |
| **Art Styles** | `oil painting`, `watercolor`, `pencil sketch`, `digital art`, `3D render`, `anime style`, `cyberpunk`, `noir`, `vaporwave` |
| **Lighting & Camera** | `backlit`, `soft light`, `cinematic lighting`, `bokeh`, `macro`, `wide-angle lens`, `golden hour`, `studio portrait` |
| **Composition** | `wide shot`, `close-up`, `top-down view`, `over-the-shoulder`, `symmetrical`, `dynamic angle`, `rule of thirds` |
| **Scene Modifiers** | `futuristic city`, `ancient ruins`, `magical forest`, `rainy night`, `sunset`, `minimalist` |
| **Color & Mood** | `vivid colors`, `pastel palette`, `monochrome`, `neon-lit`, `warm hues`, `cool tone`, `moody atmosphere` |
| **Quality Enhancers** | `high resolution`, `ray-traced reflections`, `cinematic color grading`, `film grain`, `sharp edges` |
| **Negative Tokens** | `blurry`, `lowres`, `distorted`, `deformed`, `mutated`, `watermark`, `signature`, `extra limbs`, `ugly`, `grainy` |

**Template:**
> *"[Main Subject], [vivid adjective] [setting], [style/medium], [lighting detail], [composition], ultra high definition."*

**Negative Template:**
> *"Negative: blurry, lowres, distorted, watermark, extra limbs, bad anatomy"*

---

# 10. Model Comparison Table

| Model | Type | Developer | Key Feature | Best For | Speed |
|---|---|---|---|---|---|
| **Lucid Origin** | Image | Leonardo AI | Aesthetic generalist | All-purpose creative | ●●●○ |
| **GPT Image-1.5** | Image/Edit | OpenAI | Layout precision | Complex compositions | ●●●● |
| **Seedream 4.5** | Image/Edit | ByteDance | Character consistency | Commercial, characters | ●●●○ |
| **Nano Banana Pro** | Image/Edit | Google | 4K+ reasoning | Professional detail | ●●○○ |
| **Seedream 4.0** | Image/Edit | ByteDance | Ultra-fast 4K | Rapid commercial | ●●●● |
| **Nano Banana** | Image/Edit | Google | Logical consistency | Daily creative | ●●●○ |
| **Lucid Realism** | Image | Leonardo AI | Analog cinematic | Film-like portraits | ●●●○ |
| **FLUX.2 Pro** | Image/Edit | Black Forest Labs | 9-image references | Cinematic production | ●●●○ |
| **Ideogram 3.0** | Image | Ideogram AI | Typography | Graphic design | ●●●○ |
| **GPT-Image-1** | Image | OpenAI | Text understanding | General creative | ●●○○ |
| **FLUX.1 Kontext Max** | Edit | Black Forest Labs | Premium editing | Professional edits | ●●●○ |
| **FLUX.1 Kontext** | Edit | Black Forest Labs | NL image editing | Quick edits | ●●●○ |
| **FLUX Dev** | Image | Black Forest Labs | Custom training | Fine-tuning | ●●●○ |
| **FLUX Schnell** | Image | Black Forest Labs | Maximum speed | Rapid prototyping | ●●●● |
| **Phoenix 1.0** | Image | Leonardo AI | Prompt adherence | Text-in-image | ●●●○ |
| **Phoenix 0.9** | Image | Leonardo AI | Legacy aesthetic | Budget workflows | ●●●○ |

*Speed scale: ●○○○ = Slow → ●●●● = Very Fast*

---

# 11. Example Prompts & Expected Outputs

**Short Prompt:**
> *"A cat astronaut floating in space, photorealistic, 8k"*
> **Expected:** Detailed cat in spacesuit against starry background. Visible fur texture, spacesuit reflections.

**Medium Prompt:**
> *"A bustling cyberpunk city at night, neon signs, rain-soaked streets reflecting lights, in the style of Blade Runner, ultra-detailed"*
> **Expected:** Moody cityscape with skyscrapers, neon ads, wet pavement reflections, foggy/rainy atmosphere.

**Long Prompt:**
> *"A medieval village market square at dawn. Foreground: a wooden stall with fruits and vegetables, morning light casting long shadows. Middle: peasants and traders haggling, a cobblestone path leading to a stone fountain. Background: misty forest and castle on a hill. Film still, golden hour lighting, ultra high resolution."*
> **Expected:** Layered scene with crisp textures, period clothing, glistening cobblestones, softly lit castle — like a fantasy movie still.

---

# 12. Tools, UIs, and Integrations

- **Leonardo.AI Platform:** Prompt fields, advanced settings (negative prompts, CFG, sampler), image/video upload, "Improve Prompt" feature.
- **Leonardo API:** REST API for programmatic access (`/generate/image`, `/generate/video`).
- **Community:** Replicate, Hugging Face mirrors, community galleries, Discord channels.
- **Integrations:** Zapier/Make automation, Photoshop/Figma plugins.

**Recommended Flow:** Draft prompts in a text editor → Test in Leonardo's UI → Use API for batch automation → Save seeds and prompts for reproducibility.

---

**Key Takeaway:** By combining clear, detailed prompts with model-specific strategies and iterative refinement, you can harness Leonardo AI's full suite of models to produce stunning images and videos. Use the cheat-sheets and workflows above, and don't hesitate to experiment — prompt engineering is as much creativity as it is technique. Enjoy the process, learn from each generation, and you'll level up fast!