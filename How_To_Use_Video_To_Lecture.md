# How to Use the Video-to-Lecture Skill

## What It Does

You drop a video file into the chat, and Claude turns it into a study document with notes, lessons, and action steps — all connected to your Matchmancer game.

---

## How to Use It

### Step 1: Open a new Cowork session
Start a fresh conversation with Claude in Cowork.

### Step 2: Drag and drop your video
Take any video file (.mp4, .mov, .mkv, .avi, .webm) and drag it into the chat window.

### Step 3: Tell Claude what you want
Say something like:

- "Turn this into a lecture for Matchmancer"
- "Make notes from this video"
- "What can I learn from this for my game?"
- Or just drop the video with no message — the skill triggers automatically

### Step 4: Wait for the document
Claude will:
1. Extract the audio from your video
2. Transcribe the speech to text (using Whisper)
3. Identify key lessons and topics
4. Connect each lesson to your Matchmancer project
5. Save a Word document (.docx) to your **Match Mancer/Lectures/** folder

### Step 5: Study your lecture
Open the .docx file. Inside you'll find:

- **Overview** — what the video is about + why it matters for Matchmancer
- **Key Takeaways** — the 3-5 most important points
- **Lesson Sections** — each topic broken down with:
  - What you learned (the concept explained simply)
  - How it applies to Matchmancer (with specific references to your characters, tiles, mechanics)
  - Action steps (exactly what to do next in your project)
- **Action Plan** — a numbered checklist of everything to do, in priority order

---

## What Kind of Videos Work?

Anything! The skill adapts based on what the video is about:

| Video Type | What You Get |
|---|---|
| Unity / game dev tutorial | Step-by-step technical lessons mapped to your Matchmancer code |
| Game design talk (GDC, YouTube) | Design principles applied to your 25-level structure, blockers, and balance |
| Art / visual design video | Techniques connected to your dark fantasy art style and AI art pipeline |
| Lore / worldbuilding content | Ideas mapped to Soulstream factions, characters, and narrative |
| Business / marketing video | Strategies scaled for a solo dev launching a match-3 game |
| Anything else | Transferable skills tied to your game-making journey |

---

## First Time Setup

The very first time you use this skill, Whisper (the speech-to-text tool) will need to download a small AI model (~150 MB). This happens automatically — just give it an extra minute the first time. After that, it's instant.

---

## Tips

- **Longer videos = richer lectures.** A 30-minute tutorial gives more material than a 2-minute clip
- **You can also paste transcripts.** If you already have a .txt or .srt subtitle file, just upload that instead of the video
- **Ask for specific formats.** Say "make it a presentation" for slides, or "give me a markdown file" for .md instead of .docx
- **The lectures stack up.** Over time, your Lectures folder becomes a personal Matchmancer knowledge base

---

## Where Your Lectures Are Saved

All lectures go to:

```
Match Mancer/Lectures/
```

Each file is named after the video topic, like:
- `Descent_Into_Madness_Lecture.docx`
- `Unity_Grid_Systems_Lecture.docx`
- `Puzzle_Design_Principles_Lecture.docx`
