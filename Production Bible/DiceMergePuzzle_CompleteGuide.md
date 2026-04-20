__DICE MERGE PUZZLE__

Complete Build\-It\-Yourself Guide

From Zero to Published Game in Unity

*Every Single Step Explained So Simply Anyone Can Follow*

Prepared for: Soulstream: Magick Dice Development

Ivery Towers LLC / Finally Free Productions

March 2026

# TABLE OF CONTENTS

__PART 1: __WHAT IS THIS GAME? \(The Big Picture\)

__PART 2: __SETTING UP YOUR WORKSPACE \(Installing Everything\)

__PART 3: __BUILDING THE GAME BOARD \(The Grid\)

__PART 4: __CREATING THE DICE \(Visual \+ Data\)

__PART 5: __THE MERGE MECHANIC \(Core Gameplay\)

__PART 6: __SPAWNING NEW DICE \(After Each Move\)

__PART 7: __SCORING SYSTEM & TARGETS

__PART 8: __BOOSTERS \(Shuffle, Trash, Bomb\)

__PART 9: __DAILY REWARDS \(7\-Day Cycle\)

__PART 10: __TASKS & ACHIEVEMENTS

__PART 11: __LUCKY WHEEL \(Spin to Win\)

__PART 12: __THE COMPLETE UI SYSTEM

__PART 13: __GAME SETTINGS \(The Central Brain\)

__PART 14: __TUTORIAL SYSTEM

__PART 15: __SOUND & VISUAL EFFECTS

__PART 16: __ADS INTEGRATION \(Optional\)

__PART 17: __SAVING & LOADING PLAYER DATA

__PART 18: __BUILDING & PUBLISHING

__PART 19: __COMPLETE FILE & FOLDER STRUCTURE

__PART 20: __FULL CODE LISTINGS \(Every Script\)

# PART 1: WHAT IS THIS GAME?

## 1\.1 — The Simple Explanation

Imagine you have a box of dice\. Each die shows a number \(1 through 6\)\. You place these dice on a board that looks like a checkerboard\. When you DRAG one die ON TOP OF another die that has the SAME number, they MERGE \(combine\) into ONE die with the NEXT number\.

__Example for a 5\-Year\-Old:__

You have two dice both showing '3'\. You pick up one and drop it on the other\. POOF\! They combine into one die showing '4'\. Two 4s make a 5\. Two 5s make a 6\. And so on\!

After every merge, a brand new random die appears on an empty spot on the board\. You keep merging to score points\. The game gives you a TARGET score to reach\. When you reach it, you win that stage and get a harder target\. If the board fills up and you cannot merge anymore, the game is over\.

## 1\.2 — What Makes It Fun?

- It is SIMPLE to learn \(drag and drop\)
- It is SATISFYING when dice merge \(visual and sound effects\)
- It gets HARDER as the board fills up \(strategy required\)
- BOOSTERS add excitement \(Shuffle, Trash, Bomb\)
- DAILY REWARDS make players come back every day
- LUCKY WHEEL gives free prizes
- TASKS give players small goals to chase

## 1\.3 — How a Complete Game Session Works

Let me walk you through exactly what happens from the moment someone opens your game:

1. Player opens the app\. They see the HOME SCREEN with a big 'PLAY' button, their coins at the top, and buttons for Daily Rewards, Tasks, and Lucky Wheel\.
2. Player taps PLAY\. The game board appears\. It is a 5x5 grid \(25 squares\)\. A few dice are already placed randomly on the board\. The HUD \(heads\-up display\) shows the current score, the target score, and the booster buttons\.
3. Player sees a die showing '2' and another die showing '2' on the board\. They put their finger on one of them and drag it onto the other one\.
4. The two dice MERGE with a cool animation\. They become a single die showing '3'\. The score goes up\. A new random die \(maybe a '1' or a '2'\) appears on a random empty square\.
5. Player keeps merging\. Score climbs: 50\.\.\. 150\.\.\. 300\.\.\. getting close to the target of 350\.
6. Player is stuck\! Too many different numbers\. They tap the SHUFFLE booster\. All dice on the board get rearranged to new random positions\. Now they can see new merge opportunities\!
7. Score hits 350\! TARGET REACHED\! A 'WIN' screen appears with confetti\. Player earns coins\. The next target is 700 \(350 \+ 350 x 1\)\.
8. Player goes back to home, checks Tasks \(some completed\!\), claims Daily Reward, spins Lucky Wheel\. Comes back tomorrow for more\!

## 1\.4 — The 10 Systems You Will Build

This game has exactly 10 systems\. Think of each system as a separate LEGO set\. You build each one, then connect them together\. Here they are:

__\#__

__System__

__What It Does__

__1__

__Game Board__

The 5x5 grid where dice live\. Manages positions, empty slots, and grid logic\.

__2__

__Dice System__

Creates dice objects, renders their face \(dots\), handles drag, tracks their value \(1\-6\)\.

__3__

__Merge Engine__

Detects when two same\-value dice overlap, combines them, plays animation, updates score\.

__4__

__Spawner__

After each merge, picks a random empty cell, creates a new random die there\.

__5__

__Scoring & Targets__

Tracks total score, calculates if target is met, advances to next stage\.

__6__

__Boosters__

Shuffle \(rearrange\), Trash \(delete next die\), Bomb \(clear an area\)\. Uses in\-game currency\.

__7__

__Daily Rewards__

7\-day cycle\. Each day gives a reward \(coins, boosters\)\. Resets weekly\.

__8__

__Tasks__

Mini\-goals like 'Merge 10 times' or 'Reach score 500'\. Gives rewards when completed\.

__9__

__Lucky Wheel__

Spinning wheel with prizes\. 5 spins available\. Refills after 2 hours of real time\.

__10__

__UI Manager__

All the screens: Home, Game HUD, Pause, Win, Lose, Wheel, Tasks, Daily, Get More Props\.

# PART 2: SETTING UP YOUR WORKSPACE

## 2\.1 — What You Need to Install

Before writing a single line of code, you need these programs on your computer:

### Step 1: Install Unity Hub

Unity Hub is like a 'launcher' — it manages your Unity installations and projects\.

1. Go to https://unity\.com/download
2. Click 'Download Unity Hub'
3. Run the installer\. Click Next, Next, Install\. Done\!
4. Open Unity Hub\. Create a Unity account \(free\) if you do not have one\.

### Step 2: Install Unity Editor \(Version 2022\.3 LTS\)

LTS means 'Long Term Support' — it is the most stable, tested version\. This is important because bugs in newer versions can break your game\.

1. In Unity Hub, click 'Installs' on the left sidebar\.
2. Click 'Install Editor' button \(top right\)\.
3. Find '2022\.3\.xx LTS' \(the exact number after 3 does not matter, e\.g\., 2022\.3\.62f2\)\.
4. Click Install\. When it asks about 'Modules', CHECK these boxes:

- Android Build Support \(this lets you make Android APK files\)
- iOS Build Support \(if you have a Mac and want iPhone games\)
- WebGL Build Support \(optional, for browser games\)

1. Click Install\. This takes 10\-30 minutes depending on your internet\.

__Important Note:__

Unity is about 5\-10 GB\. Make sure you have enough disk space\. Also, Unity is FREE for personal use \(revenue under $100K/year\)\.

### Step 3: Install a Code Editor

You need a program to write C\# code\. The best free option is Visual Studio Code \(VS Code\)\.

1. Go to https://code\.visualstudio\.com/
2. Download and install it\.
3. Open VS Code\. Go to Extensions \(the square icon on the left\)\.
4. Search and install these extensions:

- 'C\#' by Microsoft \(this gives you code completion and error checking\)
- 'Unity' by Microsoft \(this connects VS Code to Unity\)

## 2\.2 — Creating Your Project

Now let us create the actual game project\. Follow every step exactly:

1. Open Unity Hub\.
2. Click 'Projects' on the left sidebar\.
3. Click 'New project' button \(top right\)\.
4. Under templates, select '2D \(Built\-in Render Pipeline\)'\. This is important — our game is 2D \(flat\), not 3D\.
5. In the 'Project name' field, type: DiceMergePuzzle
6. Choose where to save it \(Desktop is fine for now\)\.
7. Click 'Create project'\. Unity will take 2\-5 minutes to set up\.

## 2\.3 — Setting Up the Folder Structure

A clean folder structure is like a clean desk — it helps you find everything fast\. In Unity's 'Project' window \(at the bottom\), right\-click in the Assets folder and create these folders:

Assets/

  DiceMerge/

    Scenes/           ← Your game levels go here

    Scripts/           ← All your C\# code

      Core/            ← Main game logic

      UI/              ← User interface scripts

      Data/            ← ScriptableObject scripts

      Boosters/        ← Booster logic

      Systems/         ← Daily rewards, tasks, wheel

    Prefabs/           ← Reusable game objects

    Textures/          ← All images and sprites

      Dice/            ← Dice face images

      UI/              ← Button icons, backgrounds

      Effects/         ← Particle effect textures

    Animations/        ← Animation clips and controllers

    Audio/             ← Sound effects and music

      SFX/             ← Sound effects

      Music/           ← Background music

    Resources/         ← Things loaded at runtime

      GameSettings/    ← Central settings file

      Levels/          ← Tutorial level data

    Materials/         ← Visual materials

    Fonts/             ← Custom fonts

__How to Create Folders in Unity:__

Right\-click in the Project window > Create > Folder\. Type the name\. To create a sub\-folder, right\-click on the parent folder first\.

## 2\.4 — Connecting VS Code to Unity

1. In Unity, go to Edit > Preferences \(on Mac: Unity > Settings\)\.
2. Click 'External Tools' on the left\.
3. Under 'External Script Editor', click the dropdown and select 'Visual Studio Code'\.
4. Now whenever you double\-click a script in Unity, it opens in VS Code automatically\!

# PART 3: BUILDING THE GAME BOARD

## 3\.1 — What Is the Game Board?

The game board is a GRID — like a checkerboard or a tic\-tac\-toe board, but bigger\. Ours is 5 columns wide and 5 rows tall, making 25 cells \(squares\)\. Each cell can either be EMPTY or have ONE DIE sitting on it\.

Think of it like a parking lot: each parking space can have one car or be empty\. The dice are the cars\.

## 3\.2 — Understanding the Grid with Numbers

Every cell has an ADDRESS, like a house address\. We use two numbers: ROW and COLUMN\. Row is which horizontal line \(top to bottom\)\. Column is which vertical line \(left to right\)\.

        Col 0   Col 1   Col 2   Col 3   Col 4

       \+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+

Row 0  |  \[0,0\]|  \[0,1\]|  \[0,2\]|  \[0,3\]|  \[0,4\]|

       \+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+

Row 1  |  \[1,0\]|  \[1,1\]|  \[1,2\]|  \[1,3\]|  \[1,4\]|

       \+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+

Row 2  |  \[2,0\]|  \[2,1\]|  \[2,2\]|  \[2,3\]|  \[2,4\]|

       \+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+

Row 3  |  \[3,0\]|  \[3,1\]|  \[3,2\]|  \[3,3\]|  \[3,4\]|

       \+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+

Row 4  |  \[4,0\]|  \[4,1\]|  \[4,2\]|  \[4,3\]|  \[4,4\]|

       \+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+\-\-\-\-\-\-\-\+

So \[2,3\] means 'Row 2, Column 3' — the cell in the middle area, slightly to the right\.

## 3\.3 — Step\-by\-Step: Create the Board in Code

Now let us write the actual code\. I will explain EVERY SINGLE LINE\.

### File: Tile\.cs \(One single square on the board\)

First, we need a script that represents ONE cell/square/tile on the board:

// FILE: Assets/DiceMerge/Scripts/Core/Tile\.cs

using UnityEngine;  // This gives us access to Unity features

// A 'class' is like a blueprint\. Every tile is made from this blueprint\.

public class Tile : MonoBehaviour

\{

    // \-\-\- WHAT THIS TILE KNOWS \-\-\-

    

    // Its position on the grid \(row and column number\)

    public int Row;      // Which horizontal line \(0, 1, 2, 3, or 4\)

    public int Col;      // Which vertical line \(0, 1, 2, 3, or 4\)

    

    // Whether a die is sitting on this tile right now

    public bool IsOccupied = false;

    

    // A reference to the die that is sitting here \(null if empty\)

    public DiceController OccupyingDice = null;

    

    // \-\-\- WHAT THIS TILE CAN DO \-\-\-

    

    // Set up this tile with its position

    public void Initialize\(int row, int col\)

    \{

        Row = row;              // Remember which row

        Col = col;              // Remember which column

        IsOccupied = false;     // Start empty

        OccupyingDice = null;   // No die here yet

    \}

    

    // Place a die on this tile

    public void PlaceDice\(DiceController dice\)

    \{

        IsOccupied = true;       // Mark as occupied

        OccupyingDice = dice;    // Remember which die

        dice\.CurrentTile = this; // Tell the die where it is

    \}

    

    // Remove the die from this tile

    public void RemoveDice\(\)

    \{

        IsOccupied = false;      // Mark as empty

        OccupyingDice = null;    // Forget the die

    \}

\}

__What is MonoBehaviour?__

MonoBehaviour is Unity's base class\. Any script that needs to exist as a component on a GameObject MUST extend MonoBehaviour\. Think of it as the 'connector' that lets your code plug into Unity's engine\.

### File: GameBoard\.cs \(The entire grid\)

Now the big one — the script that creates and manages the entire 5x5 board:

// FILE: Assets/DiceMerge/Scripts/Core/GameBoard\.cs

using UnityEngine;

public class GameBoard : MonoBehaviour

\{

    // \-\-\- SETTINGS \(editable in Unity Inspector\) \-\-\-

    

    \[Header\("Grid Settings"\)\]

    public int MaxRow = 5;        // How many rows \(default 5\)

    public int MaxCol = 5;        // How many columns \(default 5\)

    public float CellSize = 1\.2f; // Space between cells \(in Unity units\)

    

    \[Header\("References"\)\]

    public GameObject TilePrefab;  // The tile template \(we create this later\)

    

    // \-\-\- INTERNAL DATA \-\-\-

    

    // A 2D array = a grid of tiles\. Like a spreadsheet\.

    // tiles\[2\]\[3\] = the tile at Row 2, Column 3

    private Tile\[,\] tiles;

    

    // \-\-\- UNITY LIFECYCLE \-\-\-

    

    // Start\(\) runs ONCE when the game begins

    void Start\(\)

    \{

        CreateBoard\(\);  // Build the grid\!

    \}

    

    // \-\-\- BUILD THE BOARD \-\-\-

    

    void CreateBoard\(\)

    \{

        // Create the 2D array to hold all tiles

        tiles = new Tile\[MaxRow, MaxCol\];

        

        // Calculate where the board's center should be

        // This makes the board centered on screen

        float startX = \-\(MaxCol \- 1\) \* CellSize / 2f;

        float startY = \(MaxRow \- 1\) \* CellSize / 2f;

        

        // Loop through every row

        for \(int row = 0; row < MaxRow; row\+\+\)

        \{

            // Loop through every column in this row

            for \(int col = 0; col < MaxCol; col\+\+\)

            \{

                // Calculate the position for this cell

                float x = startX \+ col \* CellSize;

                float y = startY \- row \* CellSize;

                Vector3 position = new Vector3\(x, y, 0\);

                

                // Create a tile at that position

                GameObject tileObj = Instantiate\(

                    TilePrefab,    // What to create

                    position,      // Where to place it

                    Quaternion\.identity, // No rotation

                    transform      // Make it a child of the board

                \);

                

                // Name it for easy debugging

                tileObj\.name = $"Tile\_\{row\}\_\{col\}";

                

                // Get the Tile script and set it up

                Tile tile = tileObj\.GetComponent<Tile>\(\);

                tile\.Initialize\(row, col\);

                

                // Store in our array

                tiles\[row, col\] = tile;

            \}

        \}

    \}

    

    // \-\-\- HELPER METHODS \-\-\-

    

    // Get the tile at a specific position

    public Tile GetTile\(int row, int col\)

    \{

        // Safety check: make sure row/col are valid

        if \(row < 0 || row >= MaxRow\) return null;

        if \(col < 0 || col >= MaxCol\) return null;

        return tiles\[row, col\];

    \}

    

    // Get a random empty tile \(for spawning new dice\)

    public Tile GetRandomEmptyTile\(\)

    \{

        // Collect all empty tiles into a list

        var emptyTiles = new System\.Collections\.Generic\.List<Tile>\(\);

        

        for \(int row = 0; row < MaxRow; row\+\+\)

        \{

            for \(int col = 0; col < MaxCol; col\+\+\)

            \{

                if \(\!tiles\[row, col\]\.IsOccupied\)

                \{

                    emptyTiles\.Add\(tiles\[row, col\]\);

                \}

            \}

        \}

        

        // If no empty tiles, return null \(board is full\!\)

        if \(emptyTiles\.Count == 0\) return null;

        

        // Pick a random one

        int randomIndex = Random\.Range\(0, emptyTiles\.Count\);

        return emptyTiles\[randomIndex\];

    \}

    

    // Check if the board is completely full

    public bool IsBoardFull\(\)

    \{

        for \(int row = 0; row < MaxRow; row\+\+\)

        \{

            for \(int col = 0; col < MaxCol; col\+\+\)

            \{

                if \(\!tiles\[row, col\]\.IsOccupied\)

                    return false; // Found an empty spot\!

            \}

        \}

        return true; // Every single tile is occupied

    \}

    

    // Check if any merges are possible

    public bool AnyMergesPossible\(\)

    \{

        for \(int row = 0; row < MaxRow; row\+\+\)

        \{

            for \(int col = 0; col < MaxCol; col\+\+\)

            \{

                Tile tile = tiles\[row, col\];

                if \(\!tile\.IsOccupied\) continue;

                

                int val = tile\.OccupyingDice\.FaceValue;

                

                // Check neighbor to the right

                if \(col \+ 1 < MaxCol\)

                \{

                    Tile right = tiles\[row, col \+ 1\];

                    if \(right\.IsOccupied && 

                        right\.OccupyingDice\.FaceValue == val\)

                        return true;

                \}

                

                // Check neighbor below

                if \(row \+ 1 < MaxRow\)

                \{

                    Tile below = tiles\[row \+ 1, col\];

                    if \(below\.IsOccupied && 

                        below\.OccupyingDice\.FaceValue == val\)

                        return true;

                \}

            \}

        \}

        return false; // No merges found anywhere

    \}

\}

## 3\.4 — Setting Up the Board in Unity Editor

Now let us actually BUILD this in Unity step by step:

### Step A: Create the Tile Prefab

1. In Unity, right\-click in the Hierarchy \(left panel\) > 2D Object > Sprites > Square\. A white square appears\.
2. Name it 'TilePrefab' \(click on the name to rename it\)\.
3. In the Inspector \(right panel\), under Transform, set Scale to X: 1, Y: 1, Z: 1\.
4. Under Sprite Renderer, click the color swatch\. Set it to a light beige/cream color \(like R: 240, G: 230, B: 210\)\. This is the empty cell color\.
5. Click 'Add Component' at the bottom of the Inspector\. Search for 'Box Collider 2D' and add it\. This lets the game detect when things touch this tile\.
6. Click 'Add Component' again\. Search for 'Tile' \(your script\)\. Add it\.
7. Now DRAG this object from the Hierarchy INTO the Prefabs folder in the Project window\. It turns blue — it is now a prefab \(template\)\!
8. DELETE the object from the Hierarchy \(right\-click > Delete\)\. We do not need the original; the board script will create copies\.

### Step B: Create the Board Manager

1. Right\-click in Hierarchy > Create Empty\. Name it 'GameBoard'\.
2. With GameBoard selected, click 'Add Component' in the Inspector\. Search for 'GameBoard' \(your script\)\. Add it\.
3. You will see the fields: MaxRow \(set to 5\), MaxCol \(set to 5\), CellSize \(set to 1\.2\), and TilePrefab \(empty\)\.
4. DRAG the TilePrefab from your Prefabs folder into the TilePrefab field\. This tells the script WHAT to create\.
5. Press PLAY\. You should see a 5x5 grid of beige squares appear\!

__Troubleshooting:__

If nothing appears: check the Console window \(Window > General > Console\) for red error messages\. Common issue: the TilePrefab field is empty — make sure you dragged the prefab into it\. If tiles are off\-screen, check that your Camera's Size is set to about 7 \(click Main Camera > adjust Orthographic Size\)\.

# PART 4: CREATING THE DICE

## 4\.1 — What Is a Die in Our Game?

Each die in our game is a small square with dots on it \(just like a real die\)\. It knows three things: what NUMBER it shows \(1\-6\), which TILE it is sitting on, and whether the player is currently DRAGGING it\.

## 4\.2 — The Dice Face Sprites

You need 6 images — one for each face of the die\. Each image shows a die with the correct number of dots:

Dice\_1\.png  →  one dot in the center          ●

Dice\_2\.png  →  two dots diagonally         ●       ●

Dice\_3\.png  →  three dots diagonally     ●    ●    ●

Dice\_4\.png  →  four dots in corners      ●  ●  ●  ●

Dice\_5\.png  →  five dots \(4\+center\)    ●  ●  ●  ●  ●

Dice\_6\.png  →  six dots \(2 columns\)    ●● ●● ●●

You can create these in ANY image editor \(even MS Paint, Canva, or Photoshop\)\. Make them 256x256 pixels, PNG format, with transparent backgrounds\. Or you can use Midjourney/DALL\-E to generate stylized dice art\.

__For Soulstream Theme:__

Since this is for your dark fantasy universe, you could make the dice look like magical rune stones, crystal orbs, or carved bone dice with glowing symbols instead of dots\. Each 'face value' could be a different runic symbol\.

## 4\.3 — Importing Sprites into Unity

1. Save your 6 dice images into the Textures/Dice/ folder on your computer \(in the project folder\)\.
2. Go back to Unity\. It automatically detects the new files\.
3. Click on one of the dice images in the Project window\.
4. In the Inspector, set:

- Texture Type: Sprite \(2D and UI\)
- Sprite Mode: Single
- Pixels Per Unit: 256 \(this means 256 pixels = 1 Unity unit\)

1. Click 'Apply' at the bottom\.
2. Repeat for all 6 images\.

## 4\.4 — The DiceRenderer Script

This script handles showing the correct face image based on the die's value:

// FILE: Assets/DiceMerge/Scripts/Core/DiceRenderer\.cs

using UnityEngine;

public class DiceRenderer : MonoBehaviour

\{

    // Array of sprites: index 0 = Dice\_1, index 1 = Dice\_2, etc\.

    public Sprite\[\] DiceFaces;

    

    // Reference to the SpriteRenderer component

    private SpriteRenderer spriteRenderer;

    

    void Awake\(\)

    \{

        // Get the SpriteRenderer on this object

        spriteRenderer = GetComponent<SpriteRenderer>\(\);

    \}

    

    // Show the correct face for a value

    public void SetFace\(int value\)

    \{

        // value is 1\-6, but array index is 0\-5

        int index = value \- 1;

        

        // Safety check

        if \(index >= 0 && index < DiceFaces\.Length\)

        \{

            spriteRenderer\.sprite = DiceFaces\[index\];

        \}

    \}

\}

## 4\.5 — The DiceController Script \(The Brain of Each Die\)

This is the most important script for each die\. It handles: the die's value, drag\-and\-drop movement, and detecting when it lands on another die:

// FILE: Assets/DiceMerge/Scripts/Core/DiceController\.cs

using UnityEngine;

public class DiceController : MonoBehaviour

\{

    // \-\-\- WHAT THIS DIE KNOWS \-\-\-

    

    \[Header\("Dice Data"\)\]

    public int FaceValue = 1;    // Current number \(1\-6\)

    public Tile CurrentTile;     // Which tile it is on

    

    // \-\-\- INTERNAL STATE \-\-\-

    

    private bool isDragging = false;

    private Vector3 originalPosition;  // Where it was before drag

    private DiceRenderer diceRenderer;

    private Camera mainCamera;

    private int originalSortingOrder;

    

    // \-\-\- REFERENCES \-\-\-

    

    private GameBoard gameBoard;

    private MergeManager mergeManager;

    

    // \-\-\- SETUP \-\-\-

    

    void Awake\(\)

    \{

        diceRenderer = GetComponent<DiceRenderer>\(\);

        mainCamera = Camera\.main;

        

        // Find the managers in the scene

        gameBoard = FindObjectOfType<GameBoard>\(\);

        mergeManager = FindObjectOfType<MergeManager>\(\);

    \}

    

    // Initialize this die with a value and tile

    public void Initialize\(int value, Tile tile\)

    \{

        FaceValue = value;

        CurrentTile = tile;

        diceRenderer\.SetFace\(value\);

        transform\.position = tile\.transform\.position;

        tile\.PlaceDice\(this\);

    \}

    

    // \-\-\- DRAG AND DROP \-\-\-

    

    // Called when player touches/clicks this die

    void OnMouseDown\(\)

    \{

        isDragging = true;

        originalPosition = transform\.position;

        

        // Bring to front while dragging

        var sr = GetComponent<SpriteRenderer>\(\);

        originalSortingOrder = sr\.sortingOrder;

        sr\.sortingOrder = 100;

        

        // Make slightly bigger while dragging

        transform\.localScale = Vector3\.one \* 1\.15f;

    \}

    

    // Called every frame while player holds

    void OnMouseDrag\(\)

    \{

        if \(\!isDragging\) return;

        

        // Move the die to follow finger/mouse

        Vector3 mousePos = mainCamera\.ScreenToWorldPoint\(

            Input\.mousePosition\);

        transform\.position = new Vector3\(

            mousePos\.x, mousePos\.y, 0\);

    \}

    

    // Called when player releases

    void OnMouseUp\(\)

    \{

        isDragging = false;

        

        // Reset visual

        var sr = GetComponent<SpriteRenderer>\(\);

        sr\.sortingOrder = originalSortingOrder;

        transform\.localScale = Vector3\.one;

        

        // Find which tile we dropped onto

        Tile targetTile = FindClosestTile\(\);

        

        if \(targetTile \!= null && targetTile \!= CurrentTile\)

        \{

            if \(targetTile\.IsOccupied\)

            \{

                // There is a die here\! Try to merge

                DiceController other = 

                    targetTile\.OccupyingDice;

                

                if \(other\.FaceValue == this\.FaceValue\)

                \{

                    // SAME VALUE\! MERGE\!

                    mergeManager\.PerformMerge\(

                        this, other\);

                    return;

                \}

            \}

            else

            \{

                // Empty tile — move there

                CurrentTile\.RemoveDice\(\);

                targetTile\.PlaceDice\(this\);

                transform\.position = 

                    targetTile\.transform\.position;

                return;

            \}

        \}

        

        // If we get here, snap back to original position

        transform\.position = originalPosition;

    \}

    

    // Find the closest tile to current position

    Tile FindClosestTile\(\)

    \{

        float closestDist = float\.MaxValue;

        Tile closest = null;

        

        // Check every tile on the board

        Tile\[\] allTiles = FindObjectsOfType<Tile>\(\);

        foreach \(Tile tile in allTiles\)

        \{

            float dist = Vector3\.Distance\(

                transform\.position, 

                tile\.transform\.position\);

            

            // Only consider tiles within snapping range

            if \(dist < 0\.8f && dist < closestDist\)

            \{

                closestDist = dist;

                closest = tile;

            \}

        \}

        

        return closest;

    \}

\}

## 4\.6 — Creating the Dice Prefab in Unity

1. Right\-click Hierarchy > 2D Object > Sprites > Square\.
2. Name it 'DicePrefab'\.
3. Set Scale to X: 0\.9, Y: 0\.9 \(slightly smaller than tile so it fits nicely inside\)\.
4. Add Component > SpriteRenderer \(it already has one from the Square\)\.
5. Add Component > Box Collider 2D\.
6. Add Component > DiceRenderer script\.
7. In the DiceRenderer component, find 'Dice Faces'\. Set Size to 6\.
8. Drag your Dice\_1\.png into Element 0, Dice\_2\.png into Element 1, etc\.
9. Add Component > DiceController script\.
10. Set the Sorting Order on the SpriteRenderer to 1 \(so dice render ON TOP of tiles\)\.
11. Drag the DicePrefab from Hierarchy into Prefabs folder to make it a prefab\.
12. Delete from Hierarchy\.

# PART 5: THE MERGE MECHANIC

## 5\.1 — How Merging Works \(The Rule\)

The merge rule is SIMPLE: when two dice with the SAME face value are placed on top of each other, they COMBINE into ONE die with the NEXT value\. This is the heart of the entire game\.

MERGE RULE:

  1 \+ 1 = 2

  2 \+ 2 = 3

  3 \+ 3 = 4

  4 \+ 4 = 5

  5 \+ 5 = 6

  6 \+ 6 = 6 \(stays at max, but still scores points\!\)

## 5\.2 — The MergeManager Script

This script handles what happens when a merge occurs:

// FILE: Assets/DiceMerge/Scripts/Core/MergeManager\.cs

using UnityEngine;

public class MergeManager : MonoBehaviour

\{

    \[Header\("Settings"\)\]

    public int MaxDiceValue = 6;

    public GameObject MergeEffectPrefab;

    

    \[Header\("References"\)\]

    public GameBoard Board;

    public ScoreManager ScoreManager;

    public DiceSpawner Spawner;

    public AudioSource MergeSFX;

    

    // Perform a merge between two dice

    public void PerformMerge\(

        DiceController draggedDice, 

        DiceController targetDice\)

    \{

        // Step 1: Calculate the new value

        int newValue = targetDice\.FaceValue \+ 1;

        if \(newValue > MaxDiceValue\)

            newValue = MaxDiceValue;

        

        // Step 2: Remember the target tile

        Tile mergeTile = targetDice\.CurrentTile;

        

        // Step 3: Remove both dice from their tiles

        draggedDice\.CurrentTile\.RemoveDice\(\);

        targetDice\.CurrentTile\.RemoveDice\(\);

        

        // Step 4: Destroy the dragged die \(it is consumed\)

        Destroy\(draggedDice\.gameObject\);

        

        // Step 5: Upgrade the target die

        targetDice\.FaceValue = newValue;

        targetDice\.GetComponent<DiceRenderer>\(\)

            \.SetFace\(newValue\);

        mergeTile\.PlaceDice\(targetDice\);

        

        // Step 6: Play merge animation/effects

        PlayMergeEffect\(mergeTile\.transform\.position\);

        

        // Step 7: Add score

        int scoreGained = CalculateScore\(newValue\);

        ScoreManager\.AddScore\(scoreGained\);

        

        // Step 8: Play sound

        if \(MergeSFX \!= null\) MergeSFX\.Play\(\);

        

        // Step 9: Spawn a new die

        Spawner\.SpawnRandomDice\(\);

        

        // Step 10: Check game state

        CheckGameState\(\);

    \}

    

    int CalculateScore\(int value\)

    \{

        // Higher values = more points

        // Value 1 = 10pts, Value 2 = 20pts, etc\.

        return value \* 10;

    \}

    

    void PlayMergeEffect\(Vector3 position\)

    \{

        if \(MergeEffectPrefab \!= null\)

        \{

            GameObject effect = Instantiate\(

                MergeEffectPrefab, position, 

                Quaternion\.identity\);

            Destroy\(effect, 1f\); // Auto\-destroy after 1 sec

        \}

    \}

    

    void CheckGameState\(\)

    \{

        // Check if target score is reached

        if \(ScoreManager\.IsTargetReached\(\)\)

        \{

            // WIN\! Show win screen

            FindObjectOfType<UIPageHandler>\(\)

                \.ShowPage\("Win"\);

            return;

        \}

        

        // Check if board is full AND no merges possible

        if \(Board\.IsBoardFull\(\) && 

            \!Board\.AnyMergesPossible\(\)\)

        \{

            // GAME OVER\! Show lose screen

            FindObjectOfType<UIPageHandler>\(\)

                \.ShowPage\("Lose"\);

        \}

    \}

\}

# PART 6: SPAWNING NEW DICE

## 6\.1 — What Spawning Means

After every merge, the game creates a brand new die on a random empty square\. This keeps the board filling up, which creates tension and strategy\.

## 6\.2 — The DiceSpawner Script

// FILE: Assets/DiceMerge/Scripts/Core/DiceSpawner\.cs

using UnityEngine;

public class DiceSpawner : MonoBehaviour

\{

    \[Header\("References"\)\]

    public GameBoard Board;

    public GameObject DicePrefab;

    

    \[Header\("Spawn Settings"\)\]

    public int MinSpawnValue = 1;

    public int MaxSpawnValue = 3;

    // New dice only spawn as 1, 2, or 3

    // \(you must EARN 4, 5, 6 by merging\)

    

    // Spawn a random die on a random empty tile

    public void SpawnRandomDice\(\)

    \{

        // Find a random empty tile

        Tile emptyTile = Board\.GetRandomEmptyTile\(\);

        

        // If no empty tile, board is full\!

        if \(emptyTile == null\) return;

        

        // Pick a random value

        int value = Random\.Range\(

            MinSpawnValue, MaxSpawnValue \+ 1\);

        // Note: Random\.Range for ints is EXCLUSIVE

        // on the max, so we add 1

        

        // Create the die

        SpawnDice\(value, emptyTile\);

    \}

    

    // Spawn a specific die at a specific tile

    public DiceController SpawnDice\(

        int value, Tile tile\)

    \{

        // Create the die object

        GameObject diceObj = Instantiate\(

            DicePrefab,

            tile\.transform\.position,

            Quaternion\.identity\);

        

        // Get the controller and set it up

        DiceController dice = 

            diceObj\.GetComponent<DiceController>\(\);

        dice\.Initialize\(value, tile\);

        

        // Optional: Add a spawn animation

        StartCoroutine\(SpawnAnimation\(diceObj\)\);

        

        return dice;

    \}

    

    // Simple spawn animation: grow from zero

    System\.Collections\.IEnumerator SpawnAnimation\(

        GameObject obj\)

    \{

        obj\.transform\.localScale = Vector3\.zero;

        float duration = 0\.2f;

        float elapsed = 0f;

        

        while \(elapsed < duration\)

        \{

            elapsed \+= Time\.deltaTime;

            float t = elapsed / duration;

            // Ease\-out: starts fast, slows down

            float scale = 1f \- \(1f \- t\) \* \(1f \- t\);

            obj\.transform\.localScale = 

                Vector3\.one \* scale;

            yield return null;

        \}

        

        obj\.transform\.localScale = Vector3\.one;

    \}

    

    // Called at game start: place initial dice

    public void SpawnInitialDice\(int count\)

    \{

        for \(int i = 0; i < count; i\+\+\)

        \{

            SpawnRandomDice\(\);

        \}

    \}

\}

# PART 7: SCORING SYSTEM & TARGETS

## 7\.1 — How Scoring Works

Every time you merge dice, you earn points\. The formula from the asset description is:

SCORE PER MERGE = newDiceValue × 10

Examples:

  Merge two 1s → get a 2 → earn 20 points

  Merge two 3s → get a 4 → earn 40 points

  Merge two 5s → get a 6 → earn 60 points

TARGET FORMULA:

  target = 350 \+ \(350 × stagesCompleted\)

  Stage 0: target = 350 \+ \(350 × 0\) = 350

  Stage 1: target = 350 \+ \(350 × 1\) = 700

  Stage 2: target = 350 \+ \(350 × 2\) = 1050

  Stage 3: target = 350 \+ \(350 × 3\) = 1400

  \.\.\.and so on forever \("endless"\)

## 7\.2 — The ScoreManager Script

// FILE: Assets/DiceMerge/Scripts/Core/ScoreManager\.cs

using UnityEngine;

using UnityEngine\.UI; // For UI Text elements

public class ScoreManager : MonoBehaviour

\{

    \[Header\("Target Settings"\)\]

    public int BaseTarget = 350;

    public int TargetIncrement = 350;

    

    \[Header\("UI References"\)\]

    public Text ScoreText;   // Shows current score

    public Text TargetText;  // Shows target score

    

    // Private data

    private int currentScore = 0;

    private int stagesCompleted = 0;

    private int currentTarget;

    

    void Start\(\)

    \{

        currentScore = 0;

        stagesCompleted = PlayerPrefs\.GetInt\(

            "StagesCompleted", 0\);

        currentTarget = CalculateTarget\(\);

        UpdateUI\(\);

    \}

    

    int CalculateTarget\(\)

    \{

        return BaseTarget \+ 

            \(TargetIncrement \* stagesCompleted\);

    \}

    

    public void AddScore\(int amount\)

    \{

        currentScore \+= amount;

        UpdateUI\(\);

        

        // Show floating score text \(optional\)

        // ShowScorePopup\(amount\);

    \}

    

    public bool IsTargetReached\(\)

    \{

        return currentScore >= currentTarget;

    \}

    

    public void AdvanceStage\(\)

    \{

        stagesCompleted\+\+;

        PlayerPrefs\.SetInt\(

            "StagesCompleted", stagesCompleted\);

        currentScore = 0;

        currentTarget = CalculateTarget\(\);

        UpdateUI\(\);

    \}

    

    void UpdateUI\(\)

    \{

        if \(ScoreText \!= null\)

            ScoreText\.text = currentScore\.ToString\(\);

        if \(TargetText \!= null\)

            TargetText\.text = 

                $"Target: \{currentTarget\}";

    \}

    

    public int GetCurrentScore\(\) \{ 

        return currentScore; \}

    public int GetCurrentTarget\(\) \{ 

        return currentTarget; \}

    public int GetStagesCompleted\(\) \{ 

        return stagesCompleted; \}

\}

# PART 8: BOOSTERS \(Shuffle, Trash, Bomb\)

## 8\.1 — What Are Boosters?

Boosters are special powers that help the player when they are stuck\. Think of them like power\-ups in Mario\. The player has a limited number of each, and can buy more with coins\.

__Booster__

__What It Does__

__How It Works__

__SHUFFLE__

Rearranges all dice on the board to random new positions

All dice stay the same values, but their positions on the board change randomly\. This can reveal new merge opportunities\.

__TRASH__

Removes/discards the next die that would spawn

Instead of a new die appearing after the next merge, nothing appears\. Gives breathing room on a crowded board\.

__BOMB__

Clears a group of dice in an area

Player taps a spot on the board\. All dice within a configurable radius \(e\.g\., 1 tile around\) are destroyed\. Powerful but limited\.

## 8\.2 — The BoosterManager Script

// FILE: Assets/DiceMerge/Scripts/Boosters/BoosterManager\.cs

using UnityEngine;

using UnityEngine\.UI;

public class BoosterManager : MonoBehaviour

\{

    \[Header\("References"\)\]

    public GameBoard Board;

    public CoinManager CoinManager;

    

    \[Header\("Booster Counts"\)\]

    public int ShuffleCount = 3;

    public int TrashCount = 2;

    public int BombCount = 1;

    

    \[Header\("Booster Costs \(to buy more\)"\)\]

    public int ShuffleCost = 100;

    public int TrashCost = 150;

    public int BombCost = 200;

    

    \[Header\("Bomb Settings"\)\]

    public int BombRadius = 1;

    

    \[Header\("UI"\)\]

    public Text ShuffleCountText;

    public Text TrashCountText;

    public Text BombCountText;

    

    private bool trashActive = false;

    private bool bombActive = false;

    

    void Start\(\)

    \{

        LoadBoosterCounts\(\);

        UpdateUI\(\);

    \}

    

    // ─── SHUFFLE ───

    public void UseShuffle\(\)

    \{

        if \(ShuffleCount <= 0\) return;

        ShuffleCount\-\-;

        SaveBoosterCounts\(\);

        

        // Get all dice currently on the board

        var allDice = new System\.Collections\.Generic

            \.List<DiceController>\(\);

        var allValues = new System\.Collections\.Generic

            \.List<int>\(\);

        

        Tile\[\] allTiles = FindObjectsOfType<Tile>\(\);

        foreach \(Tile t in allTiles\)

        \{

            if \(t\.IsOccupied\)

            \{

                allDice\.Add\(t\.OccupyingDice\);

                allValues\.Add\(

                    t\.OccupyingDice\.FaceValue\);

                t\.RemoveDice\(\);

            \}

        \}

        

        // Shuffle the values list

        for \(int i = allValues\.Count \- 1; 

             i > 0; i\-\-\)

        \{

            int j = Random\.Range\(0, i \+ 1\);

            int temp = allValues\[i\];

            allValues\[i\] = allValues\[j\];

            allValues\[j\] = temp;

        \}

        

        // Reassign dice to positions with

        // shuffled values

        for \(int i = 0; i < allDice\.Count; i\+\+\)

        \{

            allDice\[i\]\.FaceValue = allValues\[i\];

            allDice\[i\]\.GetComponent<DiceRenderer>\(\)

                \.SetFace\(allValues\[i\]\);

        \}

        

        // Put dice back on tiles

        var occupiedTiles = new System\.Collections

            \.Generic\.List<Tile>\(\);

        foreach \(Tile t in allTiles\)

        \{

            // We cleared them, so randomly assign

            occupiedTiles\.Add\(t\);

        \}

        

        // Shuffle tile order

        for \(int i = occupiedTiles\.Count \- 1; 

             i > 0; i\-\-\)

        \{

            int j = Random\.Range\(0, i \+ 1\);

            var temp = occupiedTiles\[i\];

            occupiedTiles\[i\] = occupiedTiles\[j\];

            occupiedTiles\[j\] = temp;

        \}

        

        for \(int i = 0; i < allDice\.Count; i\+\+\)

        \{

            occupiedTiles\[i\]\.PlaceDice\(allDice\[i\]\);

            allDice\[i\]\.transform\.position = 

                occupiedTiles\[i\]\.transform\.position;

        \}

        

        UpdateUI\(\);

    \}

    

    // ─── TRASH ───

    public void ActivateTrash\(\)

    \{

        if \(TrashCount <= 0\) return;

        trashActive = true;

        // Next spawn will be skipped

    \}

    

    public bool ConsumeTrash\(\)

    \{

        if \(\!trashActive\) return false;

        trashActive = false;

        TrashCount\-\-;

        SaveBoosterCounts\(\);

        UpdateUI\(\);

        return true; // Signal: skip this spawn

    \}

    

    // ─── BOMB ───

    public void ActivateBomb\(\)

    \{

        if \(BombCount <= 0\) return;

        bombActive = true;

        // Player must now tap a tile

    \}

    

    public void DetonateBomb\(Tile centerTile\)

    \{

        if \(\!bombActive\) return;

        bombActive = false;

        BombCount\-\-;

        SaveBoosterCounts\(\);

        

        int cRow = centerTile\.Row;

        int cCol = centerTile\.Col;

        

        for \(int r = cRow \- BombRadius; 

             r <= cRow \+ BombRadius; r\+\+\)

        \{

            for \(int c = cCol \- BombRadius; 

                 c <= cCol \+ BombRadius; c\+\+\)

            \{

                Tile tile = Board\.GetTile\(r, c\);

                if \(tile \!= null && tile\.IsOccupied\)

                \{

                    Destroy\(tile\.OccupyingDice

                        \.gameObject\);

                    tile\.RemoveDice\(\);

                \}

            \}

        \}

        

        UpdateUI\(\);

    \}

    

    // ─── SAVE / LOAD ───

    void SaveBoosterCounts\(\)

    \{

        PlayerPrefs\.SetInt\("Shuffle", ShuffleCount\);

        PlayerPrefs\.SetInt\("Trash", TrashCount\);

        PlayerPrefs\.SetInt\("Bomb", BombCount\);

    \}

    

    void LoadBoosterCounts\(\)

    \{

        ShuffleCount = PlayerPrefs\.GetInt\(

            "Shuffle", 3\);

        TrashCount = PlayerPrefs\.GetInt\(

            "Trash", 2\);

        BombCount = PlayerPrefs\.GetInt\(

            "Bomb", 1\);

    \}

    

    void UpdateUI\(\)

    \{

        if \(ShuffleCountText\) ShuffleCountText\.text

            = ShuffleCount\.ToString\(\);

        if \(TrashCountText\) TrashCountText\.text

            = TrashCount\.ToString\(\);

        if \(BombCountText\) BombCountText\.text

            = BombCount\.ToString\(\);

    \}

\}

# PART 9: DAILY REWARDS \(7\-Day Cycle\)

## 9\.1 — How Daily Rewards Work

Every day the player opens the game, they get a reward\. There are 7 rewards in a cycle \(one per day\)\. After day 7, it resets back to day 1\. This is one of the most powerful tools for player retention — it makes people come back every single day\.

DAY 1: 50 coins

DAY 2: 100 coins

DAY 3: 1 Shuffle booster

DAY 4: 200 coins

DAY 5: 1 Trash booster

DAY 6: 300 coins

DAY 7: 1 Bomb booster \+ 500 coins \(JACKPOT\!\)

## 9\.2 — DailyRewardManager Script

// FILE: Assets/DiceMerge/Scripts/Systems/DailyRewardManager\.cs

using UnityEngine;

using System;

public class DailyRewardManager : MonoBehaviour

\{

    public int TotalDays = 7;

    

    // Reward data for each day

    \[System\.Serializable\]

    public class DayReward

    \{

        public string Description;

        public int CoinReward;

        public string BoosterType; // empty, Shuffle, Trash, Bomb

        public int BoosterAmount;

    \}

    

    public DayReward\[\] Rewards = new DayReward\[7\];

    

    // Check if player can claim today's reward

    public bool CanClaimToday\(\)

    \{

        string lastClaim = PlayerPrefs\.GetString\(

            "LastDailyRewardDate", ""\);

        

        if \(string\.IsNullOrEmpty\(lastClaim\)\)

            return true; // Never claimed before

        

        DateTime lastDate = DateTime\.Parse\(lastClaim\);

        DateTime today = DateTime\.Now\.Date;

        

        // Can claim if last claim was before today

        return today > lastDate;

    \}

    

    // Get which day we are on \(0\-6\)

    public int GetCurrentDay\(\)

    \{

        return PlayerPrefs\.GetInt\(

            "DailyRewardDay", 0\);

    \}

    

    // Claim today's reward

    public DayReward ClaimReward\(\)

    \{

        if \(\!CanClaimToday\(\)\) return null;

        

        int day = GetCurrentDay\(\);

        DayReward reward = Rewards\[day\];

        

        // Give rewards

        if \(reward\.CoinReward > 0\)

            FindObjectOfType<CoinManager>\(\)

                \.AddCoins\(reward\.CoinReward\);

        

        if \(\!string\.IsNullOrEmpty\(

            reward\.BoosterType\)\)

        \{

            // Add booster

            var bm = FindObjectOfType

                <BoosterManager>\(\);

            switch \(reward\.BoosterType\)

            \{

                case "Shuffle":

                    bm\.ShuffleCount \+= 

                        reward\.BoosterAmount;

                    break;

                case "Trash":

                    bm\.TrashCount \+= 

                        reward\.BoosterAmount;

                    break;

                case "Bomb":

                    bm\.BombCount \+= 

                        reward\.BoosterAmount;

                    break;

            \}

        \}

        

        // Save claim date

        PlayerPrefs\.SetString\(

            "LastDailyRewardDate", 

            DateTime\.Now\.Date\.ToString\(\)\);

        

        // Advance day \(loop after 7\)

        int nextDay = \(day \+ 1\) % TotalDays;

        PlayerPrefs\.SetInt\(

            "DailyRewardDay", nextDay\);

        

        return reward;

    \}

\}

# PART 10: TASKS & ACHIEVEMENTS

## 10\.1 — What Are Tasks?

Tasks are mini\-goals that give the player something extra to work toward\. They run in the background while the player plays\. When a task is completed, the player gets a reward\.

EXAMPLE TASKS:

  "Merge 10 times"        → Reward: 100 coins

  "Reach score 500"       → Reward: 1 Shuffle

  "Create a 5\-value die"  → Reward: 200 coins

  "Complete 3 stages"     → Reward: 1 Bomb

  "Open 5 gift boxes"     → Reward: 500 coins

  "Use a booster 3 times" → Reward: 150 coins

## 10\.2 — TaskManager Script

// FILE: Assets/DiceMerge/Scripts/Systems/TaskManager\.cs

using UnityEngine;

using System\.Collections\.Generic;

\[System\.Serializable\]

public class GameTask

\{

    public string TaskID;

    public string Description;

    public string TaskType; // merge\_count, score, dice\_value,

                           // stage\_count, booster\_use

    public int RequiredAmount;

    public int CurrentProgress;

    public bool IsCompleted;

    public bool IsClaimed;

    public int CoinReward;

    public string BoosterReward;

    public int BoosterRewardAmount;

\}

public class TaskManager : MonoBehaviour

\{

    public List<GameTask> Tasks = new List<GameTask>\(\);

    

    // Call this whenever a game event happens

    public void ReportEvent\(

        string eventType, int value\)

    \{

        foreach \(GameTask task in Tasks\)

        \{

            if \(task\.IsCompleted\) continue;

            if \(task\.TaskType \!= eventType\) continue;

            

            task\.CurrentProgress \+= value;

            

            if \(task\.CurrentProgress >= 

                task\.RequiredAmount\)

            \{

                task\.IsCompleted = true;

                // Show notification\!

            \}

        \}

    \}

    

    // Player taps 'Claim' on a completed task

    public void ClaimTask\(string taskID\)

    \{

        GameTask task = Tasks\.Find\(

            t => t\.TaskID == taskID\);

        if \(task == null\) return;

        if \(\!task\.IsCompleted\) return;

        if \(task\.IsClaimed\) return;

        

        task\.IsClaimed = true;

        

        // Give rewards

        FindObjectOfType<CoinManager>\(\)

            \.AddCoins\(task\.CoinReward\);

        // Add booster if applicable\.\.\.

    \}

\}

__How to Connect Events:__

In MergeManager after a merge, call: TaskManager\.ReportEvent\("merge\_count", 1\); In ScoreManager when target reached, call: TaskManager\.ReportEvent\("stage\_count", 1\); This way tasks automatically track progress\.

# PART 11: LUCKY WHEEL \(Spin to Win\)

## 11\.1 — How the Wheel Works

The Lucky Wheel is a spinning wheel divided into sections, each with a different prize\. The player gets 5 free spins\. Once all 5 are used, they must wait 2 hours \(120 minutes\) for all 5 spins to refill automatically\.

WHEEL SECTIONS \(example \- configurable\):

  Section 1: 50 coins   \(weight: 30 = most common\)

  Section 2: 100 coins  \(weight: 25\)

  Section 3: 200 coins  \(weight: 15\)

  Section 4: 1 Shuffle  \(weight: 12\)

  Section 5: 1 Trash    \(weight: 8\)

  Section 6: 500 coins  \(weight: 5\)

  Section 7: 1 Bomb     \(weight: 3\)

  Section 8: 1000 coins \(weight: 2 = very rare\!\)

Higher weight = more likely to land on it

Total weight = 100

Chance of 1000 coins = 2/100 = 2%

## 11\.2 — LuckyWheelManager Script

// FILE: Assets/DiceMerge/Scripts/Systems/LuckyWheelManager\.cs

using UnityEngine;

using System;

\[System\.Serializable\]

public class WheelSection

\{

    public string RewardType; // coins, shuffle, trash, bomb

    public int RewardAmount;

    public int Weight; // Higher = more common

\}

public class LuckyWheelManager : MonoBehaviour

\{

    \[Header\("Wheel Settings"\)\]

    public WheelSection\[\] Sections;

    public int MaxSpins = 5;

    public int RefillMinutes = 120;

    

    \[Header\("Wheel Visual"\)\]

    public RectTransform WheelTransform;

    public float SpinDuration = 3f;

    

    private int currentSpins;

    private bool isSpinning = false;

    

    void Start\(\)

    \{

        LoadSpinData\(\);

        CheckRefill\(\);

    \}

    

    void LoadSpinData\(\)

    \{

        currentSpins = PlayerPrefs\.GetInt\(

            "WheelSpins", MaxSpins\);

    \}

    

    void CheckRefill\(\)

    \{

        string lastUsed = PlayerPrefs\.GetString\(

            "WheelLastUsedTime", ""\);

        

        if \(string\.IsNullOrEmpty\(lastUsed\)\) return;

        if \(currentSpins >= MaxSpins\) return;

        

        DateTime lastTime = DateTime\.Parse\(lastUsed\);

        TimeSpan elapsed = DateTime\.Now \- lastTime;

        

        if \(elapsed\.TotalMinutes >= RefillMinutes\)

        \{

            // Full refill\!

            currentSpins = MaxSpins;

            PlayerPrefs\.SetInt\(

                "WheelSpins", currentSpins\);

        \}

    \}

    

    public int GetSpinsRemaining\(\)

    \{

        return currentSpins;

    \}

    

    // Spin the wheel\!

    public void Spin\(\)

    \{

        if \(currentSpins <= 0 || isSpinning\) return;

        

        isSpinning = true;

        currentSpins\-\-;

        PlayerPrefs\.SetInt\(

            "WheelSpins", currentSpins\);

        PlayerPrefs\.SetString\(

            "WheelLastUsedTime", 

            DateTime\.Now\.ToString\(\)\);

        

        // Determine result using weighted random

        int resultIndex = GetWeightedRandom\(\);

        

        // Calculate rotation angle

        float sectionAngle = 

            360f / Sections\.Length;

        float targetAngle = 

            resultIndex \* sectionAngle;

        // Add extra full rotations for drama

        float totalRotation = 

            360f \* 5 \+ targetAngle;

        

        // Start spin animation

        StartCoroutine\(SpinAnimation\(

            totalRotation, resultIndex\)\);

    \}

    

    int GetWeightedRandom\(\)

    \{

        int totalWeight = 0;

        foreach \(var s in Sections\)

            totalWeight \+= s\.Weight;

        

        int random = UnityEngine\.Random\.Range\(

            0, totalWeight\);

        int cumulative = 0;

        

        for \(int i = 0; i < Sections\.Length; i\+\+\)

        \{

            cumulative \+= Sections\[i\]\.Weight;

            if \(random < cumulative\)

                return i;

        \}

        

        return Sections\.Length \- 1;

    \}

    

    System\.Collections\.IEnumerator SpinAnimation\(

        float totalRotation, int resultIndex\)

    \{

        float elapsed = 0;

        float startAngle = 

            WheelTransform\.eulerAngles\.z;

        

        while \(elapsed < SpinDuration\)

        \{

            elapsed \+= Time\.deltaTime;

            float t = elapsed / SpinDuration;

            

            // Ease\-out: spins fast then slows down

            float eased = 1f \- Mathf\.Pow\(

                1f \- t, 3f\);

            float angle = Mathf\.Lerp\(

                0, totalRotation, eased\);

            

            WheelTransform\.eulerAngles = 

                new Vector3\(0, 0, 

                    startAngle \- angle\);

            

            yield return null;

        \}

        

        isSpinning = false;

        

        // Give the reward

        GiveReward\(Sections\[resultIndex\]\);

    \}

    

    void GiveReward\(WheelSection section\)

    \{

        switch \(section\.RewardType\)

        \{

            case "coins":

                FindObjectOfType<CoinManager>\(\)

                    \.AddCoins\(section\.RewardAmount\);

                break;

            case "shuffle":

                FindObjectOfType<BoosterManager>\(\)

                    \.ShuffleCount \+= 

                    section\.RewardAmount;

                break;

            case "trash":

                FindObjectOfType<BoosterManager>\(\)

                    \.TrashCount \+= 

                    section\.RewardAmount;

                break;

            case "bomb":

                FindObjectOfType<BoosterManager>\(\)

                    \.BombCount \+= 

                    section\.RewardAmount;

                break;

        \}

    \}

\}

# PART 12: THE COMPLETE UI SYSTEM

## 12\.1 — All the Screens You Need

Your game needs these screens \(pages\)\. Each one is a Canvas panel in Unity that gets shown or hidden:

__Screen__

__What It Shows__

__When Shown__

__Home__

Play button, coins, settings, daily/task/wheel buttons

When game starts or returns to menu

__HUD__

Score, target, booster buttons, spin key count, coin bar

During active gameplay

__Pause__

Resume and Quit buttons

Player pauses the game

__Win__

Congratulations, score summary, x2 claim option, Next button

Player reaches target score

__Lose__

Game Over text, final score, Continue \(ad\) or Home button

Board full, no moves left

__Wheel__

Lucky Wheel, spin button, spins remaining, timer

Player opens wheel from home

__Tasks__

List of tasks with progress bars and claim buttons

Player opens tasks from home

__Daily__

7\-day calendar showing rewards, today highlighted, claim

Player opens daily rewards

__Get More__

Buy boosters with coins, or watch ads for free ones

Player runs out of boosters

## 12\.2 — UIPageHandler Script

This is the controller that shows and hides all screens:

// FILE: Assets/DiceMerge/Scripts/UI/UIPageHandler\.cs

using UnityEngine;

using System\.Collections\.Generic;

public class UIPageHandler : MonoBehaviour

\{

    // Each page is a Canvas/Panel in the scene

    \[System\.Serializable\]

    public class UIPage

    \{

        public string PageName;

        public GameObject PageObject;

    \}

    

    public List<UIPage> Pages = new List<UIPage>\(\);

    public string StartPage = "Home";

    

    void Start\(\)

    \{

        // Hide everything first

        foreach \(var page in Pages\)

            page\.PageObject\.SetActive\(false\);

        

        // Show the starting page

        ShowPage\(StartPage\);

    \}

    

    public void ShowPage\(string pageName\)

    \{

        foreach \(var page in Pages\)

        \{

            bool shouldShow = 

                page\.PageName == pageName;

            page\.PageObject\.SetActive\(shouldShow\);

        \}

    \}

    

    // Show a page ON TOP of current \(like a popup\)

    public void ShowOverlay\(string pageName\)

    \{

        foreach \(var page in Pages\)

        \{

            if \(page\.PageName == pageName\)

            \{

                page\.PageObject\.SetActive\(true\);

                break;

            \}

        \}

    \}

    

    public void HidePage\(string pageName\)

    \{

        foreach \(var page in Pages\)

        \{

            if \(page\.PageName == pageName\)

            \{

                page\.PageObject\.SetActive\(false\);

                break;

            \}

        \}

    \}

\}

## 12\.3 — How to Build Each Screen in Unity

Here is the step\-by\-step for building the Home Screen\. The same process applies to all other screens:

### Building the Home Screen

1. Right\-click in Hierarchy > UI > Canvas\. Name it 'HomeScreen'\.
2. Select the Canvas\. In Inspector, set Canvas Scaler > UI Scale Mode to 'Scale With Screen Size'\. Set Reference Resolution to 1080 x 1920 \(standard phone\)\. Set Match to 0\.5\.
3. Right\-click on HomeScreen > UI > Image\. This is the background\. Set it to stretch full screen \(hold Alt, click the stretch icon in Rect Transform\)\. Set color to your game's background color\.
4. Right\-click on HomeScreen > UI > Button\. Name it 'PlayButton'\. Position it in the center\. Change the text child to say 'PLAY'\. Make the font big \(size 60\+\)\.
5. Add more buttons: DailyRewardButton, TasksButton, WheelButton, SettingsButton\. Position them around the play button\.
6. Right\-click on HomeScreen > UI > Text\. Name it 'CoinText'\. Position top\-right\. This shows the coin count\.
7. For the PlayButton's OnClick event: drag the UIPageHandler object into the slot, then select UIPageHandler > ShowPage\. Type 'HUD' in the string field\.

__Repeat for All Screens:__

Create a separate Canvas for each screen: HUDScreen, PauseScreen, WinScreen, LoseScreen, WheelScreen, TasksScreen, DailyScreen, GetMoreScreen\. Each one has its own buttons and text\. Then add all of them to the UIPageHandler's Pages list\.

# PART 13: GAME SETTINGS \(The Central Brain\)

## 13\.1 — What Is GameSettings?

GameSettings is a special file called a ScriptableObject\. Think of it as the CONTROL PANEL for your entire game\. Instead of changing numbers in code \(which requires recompiling\), you change numbers in this file and they take effect immediately\.

## 13\.2 — GameSettings Script

// FILE: Assets/DiceMerge/Scripts/Data/GameSettings\.cs

using UnityEngine;

// CreateAssetMenu lets you right\-click in Unity

// to create this as a file

\[CreateAssetMenu\(

    fileName = "GameSettings",

    menuName = "DiceMerge/Game Settings"\)\]

public class GameSettings : ScriptableObject

\{

    \[Header\("=== GRID ==="\)\]

    public int MaxRow = 5;

    public int MaxCol = 5;

    public float CellSize = 1\.2f;

    public int InitialDiceCount = 3;

    

    \[Header\("=== DICE ==="\)\]

    public int MinDiceValue = 1;

    public int MaxDiceValue = 6;

    public int MinSpawnValue = 1;

    public int MaxSpawnValue = 3;

    

    \[Header\("=== TARGETS ==="\)\]

    public int BaseTarget = 350;

    public int TargetIncrement = 350;

    

    \[Header\("=== BOOSTERS ==="\)\]

    public int StartShuffleCount = 3;

    public int StartTrashCount = 2;

    public int StartBombCount = 1;

    public int ShuffleCost = 100;

    public int TrashCost = 150;

    public int BombCost = 200;

    public int BombRadius = 1;

    

    \[Header\("=== LUCKY WHEEL ==="\)\]

    public int MaxSpins = 5;

    public int RefillMinutes = 120;

    

    \[Header\("=== DAILY REWARDS ==="\)\]

    public int\[\] DailyCoinRewards = 

        \{ 50, 100, 0, 200, 0, 300, 500 \};

    public string\[\] DailyBoosterRewards = 

        \{ "", "", "Shuffle", "", 

          "Trash", "", "Bomb" \};

\}

## 13\.3 — Creating the Settings File

1. After saving the script, go back to Unity\.
2. In the Project window, navigate to Resources/GameSettings/\.
3. Right\-click > Create > DiceMerge > Game Settings\.
4. Name the file 'GameSettings'\.
5. Click on it\. In the Inspector, you can now edit ALL game parameters without touching code\!

## 13\.4 — Loading Settings in Code

// Load GameSettings from Resources folder

// Put this in any script that needs settings:

GameSettings settings = Resources\.Load<GameSettings>\(

    "GameSettings/GameSettings"\);

// Now use settings anywhere:

int rows = settings\.MaxRow;         // 5

int target = settings\.BaseTarget;   // 350

int spins = settings\.MaxSpins;      // 5

# PART 14: TUTORIAL SYSTEM

The tutorial teaches new players how to play\. It uses special pre\-defined levels stored as ScriptableObjects\.

## 14\.1 — Tutorial Level Data

// FILE: Assets/DiceMerge/Scripts/Data/TutorialLevel\.cs

using UnityEngine;

\[CreateAssetMenu\(

    fileName = "TutorialLevel",

    menuName = "DiceMerge/Tutorial Level"\)\]

public class TutorialLevel : ScriptableObject

\{

    // Pre\-placed dice for tutorial

    \[System\.Serializable\]

    public class TutorialDice

    \{

        public int Row;

        public int Col;

        public int Value;

    \}

    

    public string LevelName;

    public string InstructionText;

    public TutorialDice\[\] PrePlacedDice;

    public int TargetScore;

\}

Create two tutorial levels in Unity:

- LevelTUT1: Place two dice with value 1 next to each other\. Instruction: 'Drag one die onto the matching die to merge them\!'
- LevelTUT2: Place three pairs\. Instruction: 'Great\! Now try merging multiple pairs to reach the target score\!'

# PART 15: SOUND & VISUAL EFFECTS

## 15\.1 — Sounds You Need

Your game needs these sound effects\. You can find free ones on sites like freesound\.org, mixkit\.co, or create them:

__Sound__

__When It Plays__

__Description__

merge\.wav

Two dice merge

Satisfying pop/chime

spawn\.wav

New die appears

Soft whoosh

pickup\.wav

Player picks up die

Light click

drop\.wav

Player drops die

Soft thud

shuffle\.wav

Shuffle booster

Card shuffle sound

bomb\.wav

Bomb booster

Explosion/boom

win\.wav

Target reached

Triumphant fanfare

lose\.wav

Game over

Sad trombone or buzzer

wheel\_spin\.wav

Wheel spinning

Ratchet/clicking

wheel\_win\.wav

Wheel stops

Prize reveal chime

button\.wav

Any UI button

Click sound

coin\.wav

Receiving coins

Coin jingle

bgm\_menu\.mp3

Home screen

Calm, ambient music

bgm\_game\.mp3

During gameplay

Upbeat, focused music

## 15\.2 — Visual Effects \(VFX\)

For visual effects, Unity has a built\-in Particle System\. Here is how to create a merge effect:

1. Right\-click Hierarchy > Effects > Particle System\.
2. Name it 'MergeEffect'\.
3. In the Particle System settings:

- Duration: 0\.5 seconds
- Start Lifetime: 0\.3\-0\.5
- Start Speed: 2\-4
- Start Size: 0\.1\-0\.3
- Start Color: Gold/Yellow gradient
- Shape: Circle \(small radius\)
- Emission: Burst of 20\-30 particles

1. Make it a prefab \(drag to Prefabs folder\)\.
2. Assign this prefab to MergeManager's MergeEffectPrefab field\.

# PART 16: ADS INTEGRATION \(Optional\)

Ads are DISABLED by default\. When you are ready to monetize, you can add them at these hook points:

- Rewarded Ad — x2 Coin Claim: After winning a stage, show a button 'Watch Ad for x2 Coins'\. If player watches, double their reward\.
- Rewarded Ad — Free Booster: In the 'Get More Props' screen, show 'Watch Ad for Free Shuffle'\.
- Rewarded Ad — Continue: On the Lose screen, show 'Watch Ad to Continue'\. Player gets 3 extra empty cells\.
- Interstitial Ad: Show a full\-screen ad between stages \(after Win screen, before returning to Home\)\.

For implementation, use Unity Ads \(built\-in\) or a mediation platform like AdMob, ironSource, or AppLovin MAX\. Each has its own SDK and setup guide\.

# PART 17: SAVING & LOADING PLAYER DATA

## 17\.1 — What Needs to Be Saved

When the player closes the game and opens it later, these things must be remembered:

- Coins \(how much money they have\)
- Stages completed \(which level they are on\)
- Best score ever achieved
- Booster counts \(Shuffle, Trash, Bomb remaining\)
- Daily reward day \(which day in the 7\-day cycle\)
- Last daily reward claim date
- Lucky Wheel spins remaining
- Last wheel use time \(for refill timer\)
- Task progress \(how far along each task is\)

## 17\.2 — CoinManager \(Simple Currency\)

// FILE: Assets/DiceMerge/Scripts/Core/CoinManager\.cs

using UnityEngine;

using UnityEngine\.UI;

public class CoinManager : MonoBehaviour

\{

    public Text CoinText;

    private int coins;

    

    void Start\(\)

    \{

        coins = PlayerPrefs\.GetInt\("Coins", 500\);

        UpdateUI\(\);

    \}

    

    public void AddCoins\(int amount\)

    \{

        coins \+= amount;

        PlayerPrefs\.SetInt\("Coins", coins\);

        UpdateUI\(\);

    \}

    

    public bool SpendCoins\(int amount\)

    \{

        if \(coins < amount\) return false;

        coins \-= amount;

        PlayerPrefs\.SetInt\("Coins", coins\);

        UpdateUI\(\);

        return true;

    \}

    

    public int GetCoins\(\) \{ return coins; \}

    

    void UpdateUI\(\)

    \{

        if \(CoinText \!= null\)

            CoinText\.text = coins\.ToString\(\);

    \}

\}

__About PlayerPrefs:__

PlayerPrefs is Unity's simple save system\. It saves small amounts of data \(numbers, text\) to the player's device\. It survives app closes\. For this game, PlayerPrefs is perfectly fine\. For bigger games, you would use JSON files or a database\.

# PART 18: BUILDING & PUBLISHING

## 18\.1 — Building for Android

1. Go to File > Build Settings\.
2. Click 'Android' in the platform list\.
3. Click 'Switch Platform' \(this may take a minute\)\.
4. Click 'Player Settings'\. Set:

- Company Name: your company \(e\.g\., 'IveryTowersLLC'\)
- Product Name: 'Dice Merge Puzzle' \(or your game name\)
- Package Name: com\.yourcompany\.dicemergepuzzle
- Minimum API Level: Android 7\.0 \(API 24\)
- Target API Level: Latest installed
- Scripting Backend: IL2CPP \(better performance\)
- Target Architectures: ARM64 \(required for Google Play\)

1. Click 'Build' to generate an APK, or 'Build and Run' if your phone is connected via USB\.

## 18\.2 — Building for iOS

iOS requires a Mac computer with Xcode installed\.

1. In Build Settings, select 'iOS', then Switch Platform\.
2. Click Build\. Unity generates an Xcode project\.
3. Open the Xcode project\.
4. Set your Team \(Apple Developer account\) and Bundle Identifier\.
5. Connect your iPhone and click Build & Run in Xcode\.

## 18\.3 — Publishing to Google Play Store

1. Create a Google Play Developer account \($25 one\-time fee\)\.
2. Go to play\.google\.com/console\.
3. Create a new app\.
4. Fill in: app name, description, screenshots, icon, feature graphic\.
5. Upload your AAB file \(Build Settings > check 'Build App Bundle'\)\.
6. Set content rating, pricing \(free/paid\), and target countries\.
7. Submit for review\. Takes 1\-7 days\.

# PART 19: COMPLETE FILE & FOLDER STRUCTURE

Here is every single file in your completed project:

Assets/DiceMerge/

│

├── Scenes/

│   └── GameScene\.unity

│

├── Scripts/

│   ├── Core/

│   │   ├── Tile\.cs

│   │   ├── GameBoard\.cs

│   │   ├── DiceController\.cs

│   │   ├── DiceRenderer\.cs

│   │   ├── DiceSpawner\.cs

│   │   ├── MergeManager\.cs

│   │   ├── ScoreManager\.cs

│   │   ├── CoinManager\.cs

│   │   └── GameManager\.cs

│   │

│   ├── Boosters/

│   │   └── BoosterManager\.cs

│   │

│   ├── Systems/

│   │   ├── DailyRewardManager\.cs

│   │   ├── TaskManager\.cs

│   │   └── LuckyWheelManager\.cs

│   │

│   ├── UI/

│   │   ├── UIPageHandler\.cs

│   │   ├── HomeUI\.cs

│   │   ├── HUDUI\.cs

│   │   ├── WinUI\.cs

│   │   ├── LoseUI\.cs

│   │   ├── WheelUI\.cs

│   │   ├── TasksUI\.cs

│   │   ├── DailyUI\.cs

│   │   └── GetMoreUI\.cs

│   │

│   └── Data/

│       ├── GameSettings\.cs

│       └── TutorialLevel\.cs

│

├── Prefabs/

│   ├── TilePrefab\.prefab

│   ├── DicePrefab\.prefab

│   └── MergeEffect\.prefab

│

├── Textures/

│   ├── Dice/

│   │   ├── Dice\_1\.png through Dice\_6\.png

│   │   └── Dice\_Universal\.png \(background\)

│   ├── UI/

│   │   ├── btn\_play\.png

│   │   ├── btn\_shuffle\.png

│   │   ├── btn\_trash\.png

│   │   ├── btn\_bomb\.png

│   │   ├── icon\_coin\.png

│   │   ├── icon\_spin\.png

│   │   ├── bg\_home\.png

│   │   ├── bg\_game\.png

│   │   └── wheel\_sections\.png

│   └── Effects/

│       ├── particle\_star\.png

│       └── particle\_glow\.png

│

├── Audio/

│   ├── SFX/

│   │   ├── merge\.wav

│   │   ├── spawn\.wav

│   │   ├── pickup\.wav

│   │   ├── drop\.wav

│   │   ├── shuffle\.wav

│   │   ├── bomb\.wav

│   │   ├── win\.wav

│   │   ├── lose\.wav

│   │   ├── wheel\_spin\.wav

│   │   ├── wheel\_win\.wav

│   │   ├── button\.wav

│   │   └── coin\.wav

│   └── Music/

│       ├── bgm\_menu\.mp3

│       └── bgm\_game\.mp3

│

├── Resources/

│   ├── GameSettings/

│   │   └── GameSettings\.asset

│   └── Levels/

│       ├── LevelTUT1\.asset

│       └── LevelTUT2\.asset

│

├── Animations/

│   ├── DiceMerge\.anim

│   ├── DiceSpawn\.anim

│   └── WheelSpin\.anim

│

├── Materials/

│   └── ParticleMaterial\.mat

│

└── Fonts/

    └── GameFont\.ttf

# PART 20: THE GAME MANAGER \(Connecting Everything\)

The GameManager is the BOSS script that starts the game and coordinates all other systems:

// FILE: Assets/DiceMerge/Scripts/Core/GameManager\.cs

using UnityEngine;

public class GameManager : MonoBehaviour

\{

    \[Header\("System References"\)\]

    public GameBoard Board;

    public DiceSpawner Spawner;

    public MergeManager MergeManager;

    public ScoreManager ScoreManager;

    public BoosterManager BoosterManager;

    public CoinManager CoinManager;

    public UIPageHandler UIHandler;

    public DailyRewardManager DailyRewards;

    public TaskManager Tasks;

    public LuckyWheelManager Wheel;

    

    // Game Settings from Resources

    private GameSettings settings;

    

    void Awake\(\)

    \{

        // Load settings

        settings = Resources\.Load<GameSettings>\(

            "GameSettings/GameSettings"\);

        

        // Apply settings to systems

        Board\.MaxRow = settings\.MaxRow;

        Board\.MaxCol = settings\.MaxCol;

        Board\.CellSize = settings\.CellSize;

        

        MergeManager\.MaxDiceValue = 

            settings\.MaxDiceValue;

        

        ScoreManager\.BaseTarget = 

            settings\.BaseTarget;

        ScoreManager\.TargetIncrement = 

            settings\.TargetIncrement;

        

        Spawner\.MinSpawnValue = 

            settings\.MinSpawnValue;

        Spawner\.MaxSpawnValue = 

            settings\.MaxSpawnValue;

    \}

    

    // Called when player hits PLAY

    public void StartGame\(\)

    \{

        UIHandler\.ShowPage\("HUD"\);

        

        // Check if first time \(tutorial\)

        bool tutDone = PlayerPrefs\.GetInt\(

            "TutorialDone", 0\) == 1;

        

        if \(\!tutDone\)

        \{

            LoadTutorial\(\);

        \}

        else

        \{

            // Normal game start

            Spawner\.SpawnInitialDice\(

                settings\.InitialDiceCount\);

        \}

    \}

    

    void LoadTutorial\(\)

    \{

        TutorialLevel tut = 

            Resources\.Load<TutorialLevel>\(

                "Levels/LevelTUT1"\);

        

        foreach \(var d in tut\.PrePlacedDice\)

        \{

            Tile tile = Board\.GetTile\(d\.Row, d\.Col\);

            if \(tile \!= null\)

                Spawner\.SpawnDice\(d\.Value, tile\);

        \}

    \}

    

    // Called when stage is won

    public void OnStageComplete\(\)

    \{

        CoinManager\.AddCoins\(100\);

        ScoreManager\.AdvanceStage\(\);

        Tasks\.ReportEvent\("stage\_count", 1\);

    \}

    

    // Called when player confirms next stage

    public void ContinueToNextStage\(\)

    \{

        // Clear all dice from board

        ClearBoard\(\);

        

        // Start fresh

        UIHandler\.ShowPage\("HUD"\);

        Spawner\.SpawnInitialDice\(

            settings\.InitialDiceCount\);

    \}

    

    void ClearBoard\(\)

    \{

        DiceController\[\] allDice = 

            FindObjectsOfType<DiceController>\(\);

        foreach \(var d in allDice\)

        \{

            d\.CurrentTile\.RemoveDice\(\);

            Destroy\(d\.gameObject\);

        \}

    \}

\}

## FINAL ASSEMBLY CHECKLIST

Here is the exact order to assemble everything in Unity:

1. Create all scripts first \(copy from this guide\)\.
2. Create TilePrefab and DicePrefab \(as described in Parts 3 and 4\)\.
3. Create MergeEffect particle prefab \(Part 15\)\.
4. Create GameSettings\.asset \(Part 13\)\.
5. Create TutorialLevel assets \(Part 14\)\.
6. Create a new Scene \(GameScene\)\. In the Hierarchy, create these empty GameObjects:

- GameManager
- GameBoard
- DiceSpawner
- MergeManager
- ScoreManager
- CoinManager
- BoosterManager
- DailyRewardManager
- TaskManager
- LuckyWheelManager
- UIPageHandler

1. Add the matching script to each GameObject\.
2. Build all 9 UI screens as Canvases \(Part 12\)\.
3. Wire all references in the Inspector: drag each manager into the appropriate fields on other managers\.
4. Add audio clips to Audio Sources \(Part 15\)\.
5. Press PLAY and test\!

__Testing Tip:__

Test each system individually before connecting them\. First make sure the board displays\. Then make sure dice spawn\. Then test dragging\. Then test merging\. Build piece by piece\!

__Congratulations\! __You now have a complete blueprint to build the Dice Merge Puzzle game from scratch\. Every system, every script, every step is here\. Go build it\!

