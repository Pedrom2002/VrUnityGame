# VR Aim Lab - 6-Day Sprint Plan

**Project:** APEX Neural Precision Academy VR Aim Lab
**Timeline:** November 13 → November 19, 2025 (6 days)
**Goal:** Playable MVP prototype with all core features
**Team Size:** Solo developer (adjust times if working in team)

---

## 📅 Overview

| Day | Focus | Deliverables | Hours |
|-----|-------|--------------|-------|
| **Day 1** | Unity Setup & Scripts Import | Project configured, all scripts imported | 4-5h |
| **Day 2** | Weapon System & Shooting | Working VR weapon with raycast shooting | 4-5h |
| **Day 3** | Target System & Spawning | Targets spawn, hit detection, pooling | 4-5h |
| **Day 4** | Game Logic & Scoring | Game modes work, scoring functional | 5-6h |
| **Day 5** | UI & Multiple Modes | All UI panels, all 5 modes playable | 5-6h |
| **Day 6** | Polish, Testing & Build | VFX, audio, optimization, final build | 6-8h |

**Total Estimated Time:** 28-35 hours over 6 days
**Buffer:** ~5-7 hours for unexpected issues

---

## 🎯 Success Criteria (MVP)

**Must Have:**
- ✅ 2+ game modes fully playable (Training + Speed minimum)
- ✅ VR weapon that shoots and hits targets
- ✅ Target spawning with object pooling
- ✅ Score tracking with accuracy display
- ✅ Working UI (menu, HUD, game over)
- ✅ High score save/load
- ✅ Builds and runs on VR headset at 90fps

**Nice to Have:**
- 5 game modes complete
- Moving targets with patterns
- Audio (SFX, music, voice lines)
- VFX (muzzle flash, tracers, hit effects)
- Haptic feedback
- Laser sight

**Can Skip (Post-MVP):**
- Multiple weapon models
- Advanced environments/backgrounds
- Achievements system
- Settings panel
- Tutorial/onboarding

---

## 📆 Day-by-Day Plan

---

## Day 1: Unity Setup & Scripts Import (4-5 hours)

**Date:** November 13, 2025
**Goal:** Project ready for development, all scripts imported and compiled

### Morning Session (2-3 hours)

#### Task 1.1: Unity Project Creation (30 min)
```
□ Install Unity 2021.3 LTS (if not installed)
□ Create new 3D (URP) project
□ Name: "VR_AimLab_Project"
□ Set to Universal Render Pipeline template
```

**Troubleshooting:**
- If URP not available, create 3D project and install URP manually from Package Manager

---

#### Task 1.2: Package Installation (45 min)
```
Window → Package Manager:
□ Install: XR Interaction Toolkit (2.0+)
□ Install: XR Plugin Management
□ Import: TextMeshPro (TMP Essentials when prompted)
□ Install: Input System (accept restart prompt)
□ (Optional) Install: ProBuilder for quick level geometry
```

**For Quest 2:**
```
□ Install: Oculus XR Plugin
□ Edit → Project Settings → XR Plug-in Management → Android → Enable Oculus
```

**For PC VR:**
```
□ Install: OpenXR Plugin (or SteamVR Plugin)
□ Edit → Project Settings → XR Plug-in Management → PC → Enable OpenXR
```

**Verification:**
- Check Package Manager shows all packages installed
- No console errors related to XR

---

#### Task 1.3: Project Settings Configuration (30 min)
```
Edit → Project Settings:

Player:
□ Company Name: "Your Name"
□ Product Name: "VR Aim Lab"
□ Default Orientation: Landscape

XR Plug-in Management:
□ Android: Oculus enabled (Quest 2)
□ PC: OpenXR enabled (PC VR)

Quality:
□ Set active quality level to "Medium" or "High"
□ Anti-Aliasing: MSAA 4x (for VR)

Graphics:
□ Scriptable Render Pipeline: URP asset
□ Render Scale: 1.0
```

---

### Afternoon Session (2 hours)

#### Task 1.4: Scripts Import (30 min)
```
1. Create folder structure:
   Assets/Scripts/Core/
   Assets/Scripts/Gameplay/
   Assets/Scripts/UI/
   Assets/Scripts/Utilities/

2. Copy all .cs files from VR_AimLab_Project/Assets/Scripts/ to Unity project

3. Wait for Unity to compile (watch Console for errors)

4. Fix any namespace or compilation errors
```

**Expected Files:**
- Core/: GameManager.cs, AudioManager.cs, ScoreSystem.cs (3 files)
- Gameplay/: Target.cs, MovingTarget.cs, TargetSpawner.cs, VRWeaponController.cs, ObjectPooler.cs (5 files)
- UI/: UIManager.cs, MenuController.cs, HUDController.cs (3 files)
- Utilities/: QuickStartSetup.cs, DebugHelper.cs (2 files)

**Total: 13 C# scripts**

---

#### Task 1.5: Basic Scene Setup (45 min)
```
1. Create Main Scene:
   □ File → New Scene
   □ Save as: "Assets/Scenes/MainScene.unity"

2. Create XR Origin:
   □ GameObject → XR → XR Origin (VR)
   □ Should create: XR Origin, Camera Offset, Main Camera, Left/Right Controllers

3. Create XR Interaction Manager:
   □ GameObject → XR → Interaction Manager

4. Test VR tracking:
   □ Press Play
   □ Put on headset
   □ Verify camera tracks head movement
```

---

#### Task 1.6: Manager GameObjects (45 min)
```
Method 1 - Auto Setup:
□ Create empty GameObject: "QuickStartSetup"
□ Add component: QuickStartSetup.cs
□ In Unity menu: VR Aim Lab → Setup Wizard
□ Click "Yes, Auto Setup"

Method 2 - Manual:
□ Create empty: "GameManager" (add GameManager.cs + ScoreSystem.cs)
□ Create empty: "AudioManager" (add AudioManager.cs)
□ Create empty: "TargetSpawner" (add TargetSpawner.cs + ObjectPooler.cs)
□ Create empty: "UIManager" (add Canvas + UIManager.cs)
```

**Verify:**
- Run: QuickStartSetup → "Verify Setup" (context menu)
- All managers should show checkmarks in console

---

### End of Day 1 Checklist
```
✓ Unity project created with URP
✓ All packages installed (XR Toolkit, TMP, Input System)
✓ XR configured for target platform
✓ All 13 scripts imported and compiling
✓ XR Origin + XR Interaction Manager in scene
✓ Manager GameObjects created
✓ VR tracking tested and working
```

**Commit/Save:** Back up project folder

---

## Day 2: Weapon System & Shooting (4-5 hours)

**Date:** November 14, 2025
**Goal:** Working VR weapon with raycast shooting, haptic feedback, and visual feedback

### Morning Session (2.5 hours)

#### Task 2.1: Weapon Prefab Creation (1 hour)
```
1. Create basic weapon model:
   □ GameObject → 3D Object → Cube (gun body)
   □ Scale: (0.05, 0.05, 0.2)
   □ Add child Cube (gun barrel): Scale (0.02, 0.02, 0.15), Position (0, 0, 0.175)
   □ Add child Empty: "ShootPoint" at barrel tip

2. Add components:
   □ Add: Rigidbody (Use Gravity: off, Is Kinematic: on)
   □ Add: Box Collider (adjust to fit model)
   □ Add: XR Grab Interactable component
   □ Add: VRWeaponController script

3. Configure XR Grab Interactable:
   □ Attach Transform: leave as weapon transform
   □ Throw on Detach: off (optional)
```

**Troubleshooting:**
- If XR Grab Interactable missing → ensure XR Toolkit installed
- If script errors → check VRWeaponController.cs compiled successfully

---

#### Task 2.2: Weapon Configuration (30 min)
```
VRWeaponController Inspector:
□ Max Ammo: 30
□ Fire Rate: 0.2
□ Reload Time: 2.0
□ Range: 100
□ Shoot Point: Assign the ShootPoint child object
□ Target Layer: "Target" (create layer if needed)

Visual Feedback:
□ Show Laser Sight: true
```

**Create Target Layer:**
1. Top right of Inspector → Layers → Edit Layers
2. Add new layer: "Target"

---

#### Task 2.3: Laser Sight Setup (30 min)
```
Automatic:
□ VRWeaponController will auto-create LineRenderer
□ Press Play to test
□ Laser should appear from weapon when grabbed

Manual (if needed):
□ Add LineRenderer component to weapon
□ Positions: 2 (start, end)
□ Width: 0.002
□ Material: Create new material (Shader: Sprites/Default)
□ Color: Red
```

---

#### Task 2.4: Attach Weapon to Controller (30 min)
```
Option 1 - Socket Interactor (advanced):
□ On Right Controller: Add XR Socket Interactor
□ Weapon snaps to hand automatically

Option 2 - Manual placement (simple):
□ Drag weapon prefab into Right Controller hierarchy
□ Position in hand: (0, -0.05, 0.1) - adjust to fit
□ Rotation: (90, 0, 0) - adjust for natural grip

Option 3 - Grabbable in Scene:
□ Place weapon prefab in scene on a table
□ Player grabs it at runtime
```

**Recommended for MVP:** Option 2 (quick and works immediately)

---

### Afternoon Session (1.5-2 hours)

#### Task 2.5: Test Shooting (No Targets) (30 min)
```
1. Enable DebugHelper:
   □ Add DebugHelper component to QuickStartSetup object
   □ Enable Debug Mode: true
   □ Show FPS: true

2. Test in VR:
   □ Press Play
   □ Put on headset
   □ Grab weapon (grip button)
   □ Pull trigger → Check console for "Shot fired!" message
   □ Check ammo decreases
   □ Hold grip → Weapon should reload after 2 seconds

3. Test laser sight:
   □ Laser should point from weapon when held
   □ Laser should change color if hitting something
```

**Troubleshooting:**
- Trigger not working → Check XR Controller input bindings
- Laser not visible → Check LineRenderer enabled and material assigned
- No haptics → Normal if no haptic feedback assigned yet

---

#### Task 2.6: Simple Test Target (1 hour)
```
1. Create dummy target:
   □ GameObject → 3D Object → Sphere
   □ Name: "TestTarget"
   □ Scale: (0.5, 0.5, 0.5)
   □ Position: In front of XR Origin (0, 1.5, 5)
   □ Layer: "Target"

2. Add Target component:
   □ Add: Target.cs script
   □ Base Points: 100
   □ Lifetime: 10 (won't despawn)

3. Test hit detection:
   □ Press Play
   □ Shoot target with weapon
   □ Should see "Hit!" message in console
   □ Target should flash yellow briefly
```

**Success Criteria:**
- Shooting target logs hit message
- Target changes color on hit
- Console shows points earned

---

### End of Day 2 Checklist
```
✓ Weapon prefab created with basic model
✓ VRWeaponController configured
✓ Laser sight visible and working
✓ Weapon can be grabbed in VR
✓ Trigger shoots raycast
✓ Ammo system works (shoot + reload)
✓ Test target can be hit and responds
```

**Commit/Save:** Back up project

---

## Day 3: Target System & Spawning (4-5 hours)

**Date:** November 15, 2025
**Goal:** Targets spawn automatically, object pooling works, targets despawn after lifetime

### Morning Session (2.5 hours)

#### Task 3.1: Target Prefab Creation (1 hour)
```
1. Create BasicTarget prefab:
   □ Duplicate TestTarget from yesterday
   □ Rename: "BasicTarget"
   □ Materials: Create red material, assign to sphere
   □ Add: SphereCollider (if not present)
   □ Configure Target.cs:
      - Target Type: Basic
      - Base Points: 100
      - Lifetime: 5
      - Use Hit Zones: false (for now)

2. Save as prefab:
   □ Drag "BasicTarget" into Assets/Prefabs/ folder
   □ Delete from scene (will be spawned by TargetSpawner)

3. Create MovingTarget prefab:
   □ Duplicate BasicTarget prefab
   □ Rename: "MovingTarget"
   □ Remove Target.cs, add MovingTarget.cs
   □ Movement Pattern: Circular
   □ Move Speed: 2
   □ Pattern Scale: 3
```

**Optional (Nice to Have):**
- Create PrecisionTarget (smaller, cyan material)
- Add visual variations (different shapes/colors per type)

---

#### Task 3.2: Object Pooler Configuration (45 min)
```
1. Find TargetSpawner object in scene
   □ Should have: TargetSpawner.cs + ObjectPooler.cs components

2. Configure ObjectPooler pools:
   □ Pools: Size = 2 (BasicTarget, MovingTarget)

   Pool 0:
   - Tag: "BasicTarget"
   - Prefab: BasicTarget prefab
   - Size: 15

   Pool 1:
   - Tag: "MovingTarget"
   - Prefab: MovingTarget prefab
   - Size: 10

3. Create pool parent:
   □ Create empty GameObject: "PoolParent"
   □ Assign to ObjectPooler → Pool Parent field
```

---

#### Task 3.3: Target Spawner Configuration (45 min)
```
TargetSpawner Inspector:

Prefabs:
□ Basic Target Prefab: Assign BasicTarget
□ Moving Target Prefab: Assign MovingTarget

Spawn Points:
Option A - Random Spawn Area:
□ Use Random Positions: true
□ Spawn Area Min: (-5, 1, 5)
□ Spawn Area Max: (5, 3, 10)

Option B - Fixed Spawn Points:
□ Use Random Positions: false
□ Create empty GameObjects as spawn points
□ Position them around play area
□ Assign to Spawn Points array

Game Mode Configs:
□ Size: 2 (configure Training and Speed for MVP)

Config 0 (Training):
- Game Mode: Training
- Spawn Interval: 2.0
- Max Active Targets: 3
- Allowed Target Types: Basic
- Target Lifetime: 10.0
- Base Points: 100

Config 1 (Speed):
- Game Mode: Speed
- Spawn Interval: 0.8
- Max Active Targets: 6
- Allowed Target Types: Basic, Speed
- Target Lifetime: 3.0
- Base Points: 100
```

---

### Afternoon Session (1.5-2 hours)

#### Task 3.4: Connect Systems (30 min)
```
GameManager Inspector:
□ Target Spawner: Assign TargetSpawner object
□ UI Manager: Assign UIManager object
□ Game Duration: 60

TargetSpawner Inspector:
□ Pool Size: 20
□ Enable Progression: true
```

---

#### Task 3.5: Test Spawning (1 hour)
```
1. Start game via code:
   □ Add DebugHelper component (if not present)
   □ Press Play
   □ Press '1' key → Should start Training mode

2. Verify:
   □ Targets spawn every 2 seconds
   □ Max 3 targets on screen
   □ Targets despawn after 10 seconds
   □ Shooting targets destroys them
   □ New targets spawn to replace destroyed ones

3. Test pooling:
   □ Select ObjectPooler in Hierarchy while playing
   □ Watch PoolParent children activate/deactivate
   □ Should reuse targets, not create new ones

4. Test Speed mode:
   □ Press 'R' to restart, then '3' for Speed mode
   □ Targets spawn faster
   □ More targets on screen
```

**Troubleshooting:**
- Targets not spawning → Check ObjectPooler has prefabs assigned
- Too many targets → Check TargetSpawner maxActiveTargets
- Targets not despawning → Check Target lifetime > 0

---

#### Task 3.6: Moving Targets (30 min - Optional)
```
If time permits:
1. Test moving targets:
   □ Manually spawn a MovingTarget in scene
   □ Press Play
   □ Should move in circle pattern
   □ Try shooting while moving

2. Add to spawn config:
   □ Update Speed mode config
   □ Allowed Target Types: Add "Moving"
   □ Test spawning mix of basic + moving
```

---

### End of Day 3 Checklist
```
✓ BasicTarget prefab complete with materials
✓ MovingTarget prefab with movement (optional)
✓ Object pooling configured (20 targets)
✓ TargetSpawner configured with spawn area
✓ Training mode config complete
✓ Speed mode config complete
✓ Targets spawn and despawn automatically
✓ Shooting targets works end-to-end
✓ Object pooling verified (no Instantiate during play)
```

**Commit/Save:** Back up project

---

## Day 4: Game Logic & Scoring (5-6 hours)

**Date:** November 16, 2025
**Goal:** Complete game loop, scoring system, game states, high score saving

### Morning Session (3 hours)

#### Task 4.1: Complete GameManager Integration (1 hour)
```
1. Verify event connections:
   □ GameManager.OnScoreChanged fires on target hit
   □ GameManager.OnGameStateChanged fires on state changes
   □ ScoreSystem calculates accuracy correctly

2. Test game flow:
   □ Press '1' → Training mode starts
   □ State: Menu → Playing
   □ Shoot targets → Score increases
   □ Press ESC → Pauses (State: Playing → Paused)
   □ Press ESC again → Resumes
   □ (Speed mode) Timer runs out → Game Over (State: Playing → GameOver)

3. Debug score system:
   □ Shoot 10 targets
   □ Check score = 1000 (base) + any combos
   □ Miss a target (let it despawn) → Streak breaks
   □ Press 'P' to print stats
```

---

#### Task 4.2: Scoring Mechanics (1 hour)
```
1. Test base scoring:
   □ Hit basic target → 100 points
   □ Check console for point calculation
   □ Verify ScoreSystem.CurrentScore updates

2. Test streak system:
   □ Hit 5 targets in a row → Combo multiplier x2
   □ Hit 10 in a row → x3
   □ Let a target expire → Streak resets to 0
   □ Check console logs

3. Test accuracy:
   □ Shoot 10 times, hit 7 targets
   □ Accuracy should be 70%
   □ Accuracy formula: (hits / totalShots) * 100
   □ Check HUD displays accuracy (if UI ready)

4. Verify rank system:
   □ Score 500 pts with 60% accuracy → Silver
   □ Score 1500 pts with 70% accuracy → Gold
   □ Check console for rank-up messages
```

---

#### Task 4.3: High Score System (1 hour)
```
1. Test PlayerPrefs saving:
   □ Play Training mode, score 1000 points
   □ End game
   □ Check: GameManager.SaveHighScore() called
   □ Print PlayerPrefs.GetInt("HighScore_Training") in console

2. Test high score loading:
   □ Return to menu
   □ Check high score displays (if UI ready)
   □ Play again, score lower → High score unchanged
   □ Play again, score higher → High score updates

3. Test per-mode saving:
   □ Play Training → Save score
   □ Play Speed → Save different score
   □ Verify each mode has separate high scores
```

---

### Afternoon Session (2-3 hours)

#### Task 4.4: Timer System (1 hour)
```
1. Configure mode timers:
   □ Training: 0 (infinite)
   □ Speed: 60 seconds
   □ Precision: 90 seconds (if implemented)
   □ Tracking: 120 seconds (if implemented)
   □ Challenge: 180 seconds (if implemented)

2. Test countdown:
   □ Start Speed mode
   □ Watch timer count down: 60, 59, 58...
   □ At 0 → Game should end automatically
   □ State should change to GameOver
   □ Press 'P' every 10 seconds to verify timer

3. Test training mode (no timer):
   □ Start Training mode
   □ Timer should show "∞"
   □ Game should not end automatically
```

---

#### Task 4.5: Difficulty Progression (1 hour)
```
1. Enable adaptive difficulty:
   □ GameManager → Adaptive Difficulty: true
   □ Difficulty Increase Interval: 30 seconds

2. Test progression:
   □ Start Speed mode
   □ Every 30 seconds, check:
      - Spawn interval decreases
      - Max active targets increases
   □ Press 'P' to see current difficulty level

3. Verify per-mode progression:
   □ Training mode: No progression (static)
   □ Speed mode: Aggressive progression
   □ Verify values change over time
```

---

#### Task 4.6: End-to-End Testing (1 hour)
```
Complete game session test:

Training Mode:
□ Start → Targets spawn
□ Shoot 20 targets
□ Check score, accuracy, rank
□ Press M → Return to menu
□ Verify high score saved

Speed Mode:
□ Start → Faster spawning
□ Play full 60 seconds
□ Check timer counts down
□ Game ends at 0
□ Check game over screen (if UI ready)
□ Score saved if higher than previous

Verify All Systems:
□ Shooting works
□ Targets spawn correctly
□ Scoring accurate
□ Streaks tracked
□ Accuracy calculated
□ Timer works (Speed mode)
□ Game state transitions work
□ High scores save/load
```

---

### End of Day 4 Checklist
```
✓ Game state machine working (Menu → Playing → Paused → GameOver)
✓ Scoring system complete (points, streaks, combos)
✓ Accuracy tracking functional
✓ Rank system calculating correctly
✓ Timer system working (countdown + infinite)
✓ Difficulty progression increasing over time
✓ High score save/load working per mode
✓ 2 game modes fully playable (Training + Speed)
```

**Commit/Save:** Back up project

---

## Day 5: UI & Multiple Modes (5-6 hours)

**Date:** November 17, 2025
**Goal:** Complete UI for all panels, all 5 game modes playable (if time permits), VR-ready menus

### Morning Session (3 hours)

#### Task 5.1: Main Menu UI (1.5 hours)
```
1. Create Canvas structure:
   □ UIManager should have Canvas (World Space)
   □ Canvas scale: (0.001, 0.001, 0.001)
   □ Position: 2m in front of XR Origin camera

2. Main Menu Panel:
   Create UI hierarchy:

   MainMenuPanel
   ├── Title (TextMeshPro): "APEX ACADEMY"
   ├── ModeButtons (Vertical Layout Group)
   │   ├── TrainingButton (Button + TMP)
   │   ├── PrecisionButton
   │   ├── SpeedButton
   │   ├── TrackingButton
   │   └── ChallengeButton
   ├── ModeInfoPanel
   │   ├── ModeNameText (TMP)
   │   ├── ModeDescriptionText (TMP)
   │   ├── HighScoreText (TMP)
   │   └── HighScoreAccuracyText (TMP)
   └── StartButton (Button)

3. Configure MenuController:
   □ Assign all button references
   □ Assign all text references
   □ Connect button onClick events:
      - TrainingButton → MenuController.SelectGameMode(Training)
      - SpeedButton → MenuController.SelectGameMode(Speed)
      - StartButton → MenuController.OnStartButtonPressed()
```

**Quick Tip:** Use Unity UI → Button, then replace Text with TextMeshPro - Text

---

#### Task 5.2: Gameplay HUD (1 hour)
```
GameplayHUDPanel (initially inactive)
├── TopBar
│   ├── ScoreText (TMP): "0"
│   ├── TimerText (TMP): "01:00"
│   └── AccuracyText (TMP): "0.0%"
├── StreakText (TMP): "Streak: 0"
├── ComboText (TMP): "x1 COMBO" (hidden initially)
├── AmmoText (TMP): "30/30"
└── RankText (TMP): "BRONZE"

Configure HUDController:
□ Assign all text references
□ Enable animations: true
□ Score Pop Duration: 0.3
□ Streak Banner Duration: 2.0

Connect to events:
□ HUDController should auto-subscribe to GameManager events
□ Test: Score changes should update HUD in real-time
```

---

#### Task 5.3: Pause & Game Over Panels (30 min)
```
PauseMenuPanel (initially inactive)
├── Title (TMP): "PAUSED"
├── ResumeButton
├── RestartButton
└── MenuButton

GameOverPanel (initially inactive)
├── Title (TMP): "MISSION COMPLETE"
├── FinalScoreText (TMP)
├── AccuracyText (TMP)
├── MaxStreakText (TMP)
├── RankText (TMP)
├── HighScoreText (TMP): "NEW HIGH SCORE!" or "High Score: X"
├── RestartButton
└── MenuButton

Connect buttons to UIManager:
□ ResumeButton → UIManager.OnResumeButtonPressed()
□ RestartButton → UIManager.OnRestartButtonPressed()
□ MenuButton → UIManager.OnMenuButtonPressed()
```

---

### Afternoon Session (2-3 hours)

#### Task 5.4: UI Integration & Testing (1.5 hours)
```
1. Connect UIManager references:
   □ Main Menu Panel: Assign MainMenuPanel object
   □ Gameplay HUD Panel: Assign GameplayHUDPanel
   □ Pause Menu Panel: Assign PauseMenuPanel
   □ Game Over Panel: Assign GameOverPanel
   □ Menu Controller: Assign MenuController component
   □ HUD Controller: Assign HUDController component

2. Test UI flow:
   Menu → Playing:
   □ Start game → Main menu hides, HUD shows
   □ Score updates on HUD when hitting targets
   □ Timer counts down
   □ Accuracy updates

   Playing → Paused:
   □ Press ESC → Pause menu shows
   □ Click Resume → Returns to game

   Playing → Game Over:
   □ Timer ends → Game over screen shows
   □ Shows final stats
   □ Click Restart → Restarts mode
   □ Click Menu → Returns to main menu

3. Test VR interaction:
   □ Enable XR Ray Interactor on controllers
   □ Point at UI buttons
   □ Pull trigger to click
   □ All buttons should respond
```

---

#### Task 5.5: Additional Game Modes (1-1.5 hours - Optional)
```
If time permits, add remaining modes:

Precision Mode:
□ Add config to TargetSpawner
□ Duration: 90s
□ Targets: Precision (small) + Basic
□ Test mode via menu

Tracking Mode:
□ Add config
□ Duration: 120s
□ Targets: Moving only
□ Test mode

Challenge Mode:
□ Add config
□ Duration: 180s
□ Targets: All types
□ Aggressive progression
□ Test mode

For each mode:
□ Add button to menu
□ Update mode info descriptions
□ Test full game session
□ Verify high score saves separately
```

**MVP Priority:** Training + Speed are essential. Others are nice-to-have.

---

#### Task 5.6: UI Polish (30 min - Optional)
```
If time permits:
□ Add button hover effects (scale up, color change)
□ Add button click sounds (AudioManager.PlayButtonClick)
□ Add mode select sound
□ Color-code accuracy (green/yellow/red)
□ Add rank color to rank text
□ Format numbers (1000 → 1,000)
□ Animate score counting up
```

---

### End of Day 5 Checklist
```
✓ Main menu complete with mode selection
✓ Gameplay HUD showing score, timer, accuracy
✓ Pause menu functional
✓ Game over screen with stats
✓ UI connected to game systems via events
✓ VR ray interaction working with UI
✓ Training + Speed modes fully playable with UI
✓ (Optional) Precision, Tracking, Challenge modes added
✓ All UI transitions smooth (Menu ↔ Playing ↔ Paused ↔ GameOver)
```

**Commit/Save:** Back up project

---

## Day 6: Polish, Testing & Build (6-8 hours)

**Date:** November 18, 2025 (deadline: Nov 19)
**Goal:** Final polish, VFX, audio, testing, optimization, build for VR

### Morning Session (3-4 hours)

#### Task 6.1: Visual Effects (1.5 hours)
```
Create VFX prefabs:

1. Muzzle Flash:
   □ GameObject → Effects → Particle System
   □ Configure:
      - Duration: 0.1
      - Start Lifetime: 0.05
      - Start Size: 0.1-0.2
      - Emission: Burst (10 particles)
      - Shape: Cone
      - Color: Yellow/Orange gradient
   □ Save as Prefab: "MuzzleFlash"
   □ Assign to VRWeaponController → Muzzle Flash Prefab

2. Hit Effect:
   □ Particle System
   □ Configure:
      - Duration: 0.5
      - Start Lifetime: 0.3
      - Start Size: 0.05-0.1
      - Emission: Burst (20 particles)
      - Color: Based on target color
   □ Save as Prefab: "HitEffect"
   □ Assign to Target → Hit Effect Prefab

3. Target Destroy Effect:
   □ Larger particle burst
   □ Save as Prefab: "DestroyEffect"
   □ Assign to Target → Destroy Effect Prefab

4. Bullet Tracer:
   □ Capsule stretched thin
   □ Emissive material (cyan glow)
   □ VRWeaponController handles movement
   □ Save as Prefab: "BulletTracer"
```

**Performance Note:** Keep particle counts low (<50 per effect) for VR performance

---

#### Task 6.2: Audio Integration (1 hour)
```
Audio setup (if audio files available):

1. Import audio files to Assets/Audio/:
   SFX:
   - shoot.wav
   - reload.wav
   - hit.wav
   - perfect_hit.wav
   - target_spawn.wav

   Music:
   - menu_music.ogg
   - gameplay_music.ogg

2. Assign to AudioManager:
   □ Find AudioManager in scene
   □ Assign all AudioClip fields
   □ Test volume sliders

3. Test audio:
   □ Menu → Music plays
   □ Start game → Music changes
   □ Shoot → Shoot sound plays
   □ Hit target → Hit sound plays (spatial 3D)
   □ Reload → Reload sound plays
```

**If no audio files available:** Skip this section, AudioManager will work silently

---

#### Task 6.3: Haptic Feedback (30 min)
```
Verify VRWeaponController haptics:

1. Check configuration:
   □ Shoot Haptic Intensity: 0.5
   □ Shoot Haptic Duration: 0.1
   □ Reload Haptic Intensity: 0.3
   □ Reload Haptic Duration: 0.2

2. Test in VR:
   □ Shoot → Feel vibration in controller
   □ Reload → Feel reload pulse
   □ Hit bullseye → Stronger vibration

3. Adjust if too strong/weak:
   □ Lower intensity if uncomfortable
   □ Increase duration for more noticeable feedback
```

---

#### Task 6.4: Optimization Pass (1 hour)
```
Performance optimization:

1. Check FPS:
   □ Enable DebugHelper → Show FPS
   □ Target: 90 fps sustained
   □ If below 90:
      - Reduce max active targets
      - Simplify VFX
      - Lower texture resolution
      - Disable shadows on small objects

2. Verify object pooling:
   □ Open Profiler (Window → Analysis → Profiler)
   □ Play game for 60 seconds
   □ Check GC Allocations
   □ Should see no Instantiate() calls during gameplay
   □ Check memory doesn't grow continuously

3. Optimize materials:
   □ Use URP/Lit or URP/Unlit shaders
   □ Reduce texture sizes to 512x512 or lower
   □ Enable GPU Instancing on materials

4. Physics optimization:
   □ Edit → Project Settings → Physics
   □ Fixed Timestep: 0.0111 (90Hz for VR)
   □ Solver Iterations: 4-6 (lower = faster)
```

---

### Afternoon Session (3-4 hours)

#### Task 6.5: Comprehensive Testing (2 hours)
```
Full playthrough testing:

Test 1: Training Mode
□ Start from menu
□ Play for 5 minutes
□ Hit 30 targets
□ Check: Score correct, accuracy calculates, no errors
□ Pause and resume
□ Return to menu
□ Verify high score saved

Test 2: Speed Mode
□ Start from menu
□ Play full 60 seconds
□ Score 2000+ points
□ Check: Timer counts down, difficulty increases, combo works
□ Game ends at 0 seconds
□ Check game over stats
□ Restart immediately
□ Play again, verify new session

Test 3: All UI Flows
□ Main Menu → Training → Pause → Resume → GameOver → Menu
□ Main Menu → Speed → Quit (ESC) → Menu
□ Test all buttons respond

Test 4: VR Comfort
□ Play for 15 minutes continuously
□ Check for motion sickness symptoms
□ Verify 90fps maintained
□ No hitching or stuttering
□ Haptics comfortable

Test 5: Edge Cases
□ Shoot with 0 ammo → Empty sound plays
□ Let 10 targets despawn → Streak resets
□ Score exactly 500 pts → Rank up to Silver
□ Reload mid-reload → Should not break
□ Pause while shooting → Should handle gracefully
```

---

#### Task 6.6: Bug Fixing (1 hour)
```
Common issues to check:

□ Targets spawn inside floor/walls → Adjust spawn area
□ Weapon falls through hand → Adjust Rigidbody settings
□ UI not clickable in VR → Check Ray Interactor and Canvas
□ Score not saving → Check PlayerPrefs.Save() called
□ FPS drops → Reduce max targets, disable complex VFX
□ Targets don't pool → Check ObjectPooler configuration
□ Streak doesn't reset → Check Target.OnLifetimeExpired() calls GameManager
□ Timer doesn't start → Check GameMode configuration

Fix priority: Game-breaking bugs first, polish issues later
```

---

#### Task 6.7: Build for VR (1-1.5 hours)
```
Quest 2 Build:

1. Configure build settings:
   □ File → Build Settings
   □ Platform: Android
   □ Switch Platform (wait 5-10 min)
   □ Add Open Scenes
   □ Texture Compression: ASTC

2. Player Settings:
   □ Company Name, Product Name
   □ Minimum API Level: Android 10 (API 29)
   □ Graphics APIs: Remove Vulkan if issues (use OpenGLES3)
   □ Scripting Backend: IL2CPP
   □ Target Architectures: ARM64 only
   □ XR Plug-in Management → Android → Oculus enabled

3. Build:
   □ Connect Quest 2 via USB
   □ Enable Developer Mode on Quest 2
   □ Build and Run
   □ Wait 10-20 minutes
   □ Test on headset

PC VR Build:

1. Configure build settings:
   □ Platform: Windows
   □ Architecture: x86_64

2. Player Settings:
   □ Graphics APIs: Direct3D11
   □ XR Plug-in Management → PC → OpenXR enabled

3. Build:
   □ Build (not Build and Run)
   □ Create folder: Builds/PC_VR/
   □ Test with SteamVR/Oculus app running
```

**Troubleshooting:**
- Build fails → Check console errors, likely missing SDK
- App crashes on Quest → Check logs via adb logcat
- Performance issues → Lower quality settings

---

### End of Day 6 Checklist
```
✓ VFX added (muzzle flash, hit effects, tracers)
✓ Audio integrated (if files available)
✓ Haptic feedback tested and comfortable
✓ Performance optimized (90 fps achieved)
✓ Object pooling verified (no GC spikes)
✓ Comprehensive testing complete (all modes work)
✓ Known bugs fixed or documented
✓ Build created for target platform (Quest 2 or PC VR)
✓ Final build tested on actual VR headset
✓ Project backed up
```

---

## 📊 Final Deliverables Checklist

### Code
- [x] All 13 scripts implemented and functional
- [x] Clean, commented code with XML docs
- [x] No compilation errors or warnings
- [x] Event-driven architecture
- [x] Singleton pattern for managers

### Prefabs
- [ ] BasicTarget prefab (sphere + Target.cs)
- [ ] MovingTarget prefab (extends BasicTarget)
- [ ] Weapon prefab (VRWeaponController + XR Grab)
- [ ] VFX prefabs (muzzle, hit, tracer)

### Scene
- [ ] MainScene.unity with all managers
- [ ] XR Origin configured
- [ ] UI Canvas with all panels
- [ ] Spawn area configured

### Game Modes
- [ ] Training mode (essential)
- [ ] Speed mode (essential)
- [ ] Precision mode (optional)
- [ ] Tracking mode (optional)
- [ ] Challenge mode (optional)

### Systems
- [ ] VR weapon shooting works
- [ ] Target spawning with pooling
- [ ] Scoring system functional
- [ ] High score save/load
- [ ] UI completely integrated
- [ ] Game state machine working

### Performance
- [ ] 90 fps on target hardware
- [ ] Object pooling active
- [ ] No GC allocations during gameplay
- [ ] VR comfort verified (no motion sickness)

### Build
- [ ] Build created for Quest 2 or PC VR
- [ ] Tested on actual VR headset
- [ ] No critical bugs in final build

---

## 🚨 Risk Mitigation

### High Risk Areas

**Day 2: VR Controller Integration**
- **Risk:** XR Toolkit input not working
- **Mitigation:** Test early, use DebugHelper as fallback

**Day 3: Object Pooling**
- **Risk:** Targets not spawning from pool
- **Mitigation:** Test pooling immediately, can skip pooling for MVP

**Day 5: UI in VR**
- **Risk:** Buttons not clickable in VR
- **Mitigation:** Test ray interaction early, have backup keyboard controls

**Day 6: Performance**
- **Risk:** FPS drops below 90
- **Mitigation:** Reduce max targets, simplify VFX, test throughout week

### Fallback Plan

If running out of time, cut features in this order:
1. ✂️ Challenge mode (keep 4 modes)
2. ✂️ Tracking mode (keep 3 modes)
3. ✂️ Precision mode (keep 2 modes: Training + Speed)
4. ✂️ VFX (keep core gameplay)
5. ✂️ Audio (silent gameplay is okay)
6. ✂️ Moving targets (static only)

**Minimum Viable Product:**
- Training mode + Speed mode
- VR weapon shoots targets
- Basic scoring + high scores
- Simple UI (menu + HUD)
- 90fps performance

---

## 📞 Daily Check-In Template

Use this at end of each day:

```
Day X Complete ✅

Completed:
- [List what you finished]

Blockers:
- [Any issues encountered]

Tomorrow's Priority:
- [Top 3 tasks for next day]

Time Spent: X hours
Remaining: Y hours
On Schedule: Yes/No
```

---

## 🎯 Success Metrics

**MVP Success:**
- ✅ Game runs on VR headset
- ✅ 2+ game modes fully playable
- ✅ Score tracks and saves
- ✅ 90fps maintained
- ✅ Build completed by Nov 19

**Stretch Goals:**
- ✨ 5 game modes complete
- ✨ Full VFX and audio
- ✨ Moving targets with patterns
- ✨ All UI panels polished

---

**Good luck! You've got this! 🚀**

**Remember:** Focus on making 2 modes perfect rather than 5 modes mediocre. Playable MVP > Feature-complete but broken.

---

**Document Version:** 1.0.0
**Last Updated:** November 2025
**Sprint Start:** November 13, 2025
**Deadline:** November 19, 2025
