# VR Aim Lab - Unity Setup Guide (60 Minutes)

**Goal:** Get from zero to a working VR Aim Lab prototype in Unity
**Time Required:** 60-90 minutes (first time)
**Skill Level:** Intermediate Unity + Basic VR knowledge

---

## 📋 Prerequisites Checklist

Before starting, ensure you have:

```
□ Unity 2021.3 LTS installed (or newer)
□ VR headset (Meta Quest 2 or PC VR)
□ USB cable (for Quest 2) or wireless connection
□ ~5 GB free disk space
□ All project files downloaded/extracted
```

---

## ⏱️ Timeline Overview

| Time | Step | What You're Doing |
|------|------|-------------------|
| 0-10 min | [Step 1](#step-1-create-project) | Create Unity project, install packages |
| 10-20 min | [Step 2](#step-2-import-scripts) | Import C# scripts, verify compilation |
| 20-35 min | [Step 3](#step-3-xr-setup) | Configure XR Origin, controllers |
| 35-50 min | [Step 4](#step-4-create-prefabs) | Create target and weapon prefabs |
| 50-60 min | [Step 5](#step-5-connect-systems) | Connect managers, test in VR |

---

## Step 1: Create Project (10 minutes)

### 1.1 Create New Unity Project (3 min)

1. Open **Unity Hub**
2. Click **New Project**
3. Select template: **3D (URP)** or **3D Core**
4. **Project Name:** `VR_AimLab_Project`
5. **Location:** Choose preferred folder
6. Click **Create Project**
7. Wait for Unity to open (2-5 minutes)

**✓ Checkpoint:** Unity Editor open with empty scene

---

### 1.2 Install Required Packages (7 min)

#### Open Package Manager
```
Window → Package Manager
```

#### Install Packages (one at a time):

**1. XR Interaction Toolkit** (required)
```
□ In Package Manager, change dropdown to "Unity Registry"
□ Search: "XR Interaction Toolkit"
□ Click "Install" (version 2.0 or newer)
□ Wait for installation (~2 min)
□ Accept "Input System" backend change (restart Unity)
```

**2. XR Plugin Management** (usually auto-installed with XR Toolkit)
```
□ Search: "XR Plugin Management"
□ If not installed, click "Install"
```

**3. TextMeshPro** (UI text)
```
□ Search: "TextMeshPro"
□ Should already be installed
□ When prompted, click "Import TMP Essentials"
```

**4. Input System** (should auto-install)
```
□ Search: "Input System"
□ Should be installed automatically
□ If not, install it manually
```

**For Quest 2 ONLY:**
```
□ Search: "Oculus XR Plugin"
□ Click "Install"
```

**For PC VR (SteamVR) ONLY:**
```
□ Search: "OpenXR Plugin"
□ Click "Install"
```

**✓ Checkpoint:** All packages installed, Unity restarted if prompted

---

## Step 2: Import Scripts (10 minutes)

### 2.1 Create Folder Structure (2 min)

In Unity Project window:

```
Right-click Assets → Create → Folder → "Scripts"
Inside Scripts, create folders:
□ Core
□ Gameplay
□ UI
□ Utilities
```

**Final structure:**
```
Assets/
└── Scripts/
    ├── Core/
    ├── Gameplay/
    ├── UI/
    └── Utilities/
```

---

### 2.2 Copy C# Scripts (3 min)

**From your downloaded project:**

Copy **from** `VR_AimLab_Project/Assets/Scripts/Core/` **to** Unity's `Assets/Scripts/Core/`:
- `GameManager.cs`
- `AudioManager.cs`
- `ScoreSystem.cs`

Copy **from** `VR_AimLab_Project/Assets/Scripts/Gameplay/` **to** Unity's `Assets/Scripts/Gameplay/`:
- `Target.cs`
- `MovingTarget.cs`
- `TargetSpawner.cs`
- `VRWeaponController.cs`
- `ObjectPooler.cs`

Copy **from** `VR_AimLab_Project/Assets/Scripts/UI/` **to** Unity's `Assets/Scripts/UI/`:
- `UIManager.cs`
- `MenuController.cs`
- `HUDController.cs`

Copy **from** `VR_AimLab_Project/Assets/Scripts/Utilities/` **to** Unity's `Assets/Scripts/Utilities/`:
- `QuickStartSetup.cs`
- `DebugHelper.cs`

**Total: 13 C# files**

---

### 2.3 Verify Compilation (5 min)

1. Wait for Unity to compile (watch bottom-right progress bar)
2. Check **Console** window (Window → General → Console)
3. **If errors appear:**
   - Most common: "namespace not found"
   - Solution: Ensure all 13 files copied correctly
   - Double-check file names match exactly

**✓ Checkpoint:** Console shows 0 errors, all scripts compiled

---

## Step 3: XR Setup (15 minutes)

### 3.1 Configure XR Plugin Management (3 min)

```
Edit → Project Settings → XR Plug-in Management
```

**For Quest 2:**
```
□ Click "Android" tab
□ Check ☑ "Oculus"
```

**For PC VR:**
```
□ Click "PC" (Windows/Mac) tab
□ Check ☑ "OpenXR"
```

**Close Project Settings**

---

### 3.2 Create XR Origin (4 min)

In **Hierarchy** window:

```
Right-click → XR → XR Origin (VR)
```

This creates:
```
XR Origin (VR)
├── Camera Offset
│   └── Main Camera
├── Left Controller
└── Right Controller
```

**Configure XR Origin:**
1. Select **XR Origin** in Hierarchy
2. Inspector → Position: (0, 0, 0)
3. Camera Offset → Position: (0, 0, 0)

**Delete old camera:**
```
□ Find "Main Camera" (separate from XR Origin)
□ Delete it if exists (XR Origin has its own camera)
```

---

### 3.3 Create XR Interaction Manager (1 min)

```
Hierarchy → Right-click → XR → Interaction Manager
```

This is required for XR Grab Interactables to work.

---

### 3.4 Add Ray Interactors to Controllers (4 min)

**Left Controller:**
```
1. Select: XR Origin → Left Controller
2. Add Component → XR Ray Interactor
3. Configure:
   - Line Type: Straight Line
   - Max Raycast Distance: 10
```

**Right Controller:**
```
1. Select: XR Origin → Right Controller
2. Add Component → XR Ray Interactor
3. Configure:
   - Line Type: Straight Line
   - Max Raycast Distance: 10
```

**✓ Checkpoint:** VR tracking should work (test by pressing Play and putting on headset)

---

### 3.5 Quick VR Test (3 min)

```
1. Press Play ▶
2. Put on VR headset
3. Move head → Camera should follow
4. Look at hands → Controllers should be visible (if models present)
5. Press Stop ■
```

**If tracking doesn't work:**
- Check headset is connected
- Check SteamVR/Oculus app is running
- Verify XR Plugin enabled in Project Settings

---

## Step 4: Create Prefabs (15 minutes)

### 4.1 Create Layers (2 min)

```
Top-right of Inspector → Layers → Edit Layers
Add new User Layer 6: "Target"
```

---

### 4.2 Create Prefabs Folder (1 min)

```
Assets → Right-click → Create → Folder → "Prefabs"
Inside Prefabs, create folder: "Targets"
Inside Prefabs, create folder: "Weapons"
```

---

### 4.3 Create BasicTarget Prefab (5 min)

**Create GameObject:**
```
Hierarchy → Right-click → 3D Object → Sphere
Rename: "BasicTarget"
Position: (0, 1.5, 5) [in front of camera for testing]
Scale: (0.4, 0.4, 0.4)
```

**Configure:**
```
□ Layer: Target (set in Inspector)
□ Add Component → Target (script)
   - Target Type: Basic
   - Base Points: 100
   - Lifetime: 5
   - Use Hit Zones: false
```

**Add Material (optional but recommended):**
```
1. Assets → Create → Material → "RedTarget"
2. Set color: Red
3. Drag material onto BasicTarget sphere
```

**Save as Prefab:**
```
1. Drag "BasicTarget" from Hierarchy to Assets/Prefabs/Targets/
2. Delete "BasicTarget" from Hierarchy (will be spawned later)
```

**✓ Checkpoint:** BasicTarget prefab exists in Prefabs/Targets/

---

### 4.4 Create Weapon Prefab (7 min)

**Create GameObject:**
```
Hierarchy → Right-click → Create Empty
Rename: "VR_Weapon"
Position: (0, 0, 0)
```

**Create Simple Gun Model:**
```
Create child: 3D Object → Cube (gun body)
- Name: "Body"
- Scale: (0.05, 0.05, 0.2)
- Position: (0, 0, 0)

Create child: 3D Object → Cube (gun barrel)
- Name: "Barrel"
- Scale: (0.02, 0.02, 0.15)
- Position: (0, 0, 0.175)

Create child: Empty GameObject
- Name: "ShootPoint"
- Position: (0, 0, 0.25) [at barrel tip]
```

**Add Components to VR_Weapon:**
```
□ Add Component → Rigidbody
   - Use Gravity: ☐ Off
   - Is Kinematic: ☑ On

□ Add Component → Box Collider
   - Size: (0.1, 0.1, 0.3)
   - Is Trigger: ☐ Off

□ Add Component → XR Grab Interactable
   - Attach Transform: VR_Weapon
   - Throw On Detach: ☐ Off

□ Add Component → VR Weapon Controller (script)
   - Max Ammo: 30
   - Fire Rate: 0.2
   - Reload Time: 2.0
   - Range: 100
   - Shoot Point: Drag "ShootPoint" child object here
   - Target Layer: Target (select from dropdown)
   - Show Laser Sight: ☑ On
```

**Position Weapon on Controller:**
```
1. Drag VR_Weapon into: XR Origin → Right Controller
2. Set Position: (0, -0.05, 0.1)
3. Set Rotation: (90, 0, 0)
4. Adjust until gun sits naturally in hand
```

**Save as Prefab:**
```
1. Drag "VR_Weapon" to Assets/Prefabs/Weapons/
2. Keep in scene (attached to Right Controller)
```

**✓ Checkpoint:** Weapon attached to right controller, visible in VR

---

## Step 5: Connect Systems (15 minutes)

### 5.1 Create Manager GameObjects (5 min)

**GameManager:**
```
Hierarchy → Create Empty → "GameManager"
□ Add Component → Game Manager (script)
□ Add Component → Score System (script)
```

**AudioManager:**
```
Hierarchy → Create Empty → "AudioManager"
□ Add Component → Audio Manager (script)
```

**TargetSpawner:**
```
Hierarchy → Create Empty → "TargetSpawner"
□ Add Component → Target Spawner (script)
□ Add Component → Object Pooler (script)
```

**UIManager:**
```
Hierarchy → Create Empty → "UIManager"
□ Add Component → Canvas
   - Render Mode: World Space
   - Position: (0, 2, 2) [in front of player]
   - Rotation: (0, 180, 0) [facing player]
   - Scale: (0.001, 0.001, 0.001)
□ Add Component → Canvas Scaler
□ Add Component → Graphic Raycaster
□ Add Component → UI Manager (script)
```

---

### 5.2 Configure Object Pooler (3 min)

**Select:** TargetSpawner object

**Object Pooler Component:**
```
Pools: Size = 1

Pool 0:
□ Tag: "BasicTarget"
□ Prefab: Drag BasicTarget prefab from Prefabs/Targets/
□ Size: 15

Expand Pool If Needed: ☑ On
```

---

### 5.3 Configure Target Spawner (4 min)

**Still on TargetSpawner object:**

**Target Spawner Component:**
```
Prefabs:
□ Basic Target Prefab: Drag BasicTarget prefab

Spawn Points: (leave empty)
□ Use Random Positions: ☑ On
□ Spawn Area Min: (-5, 1, 5)
□ Spawn Area Max: (5, 3, 10)

Spawn Settings:
□ Pool Size: 20

Game Mode Configs: Size = 1

Config 0:
□ Game Mode: Training
□ Spawn Interval: 2.0
□ Max Active Targets: 3
□ Allowed Target Types: Size = 1
   - Element 0: Basic
□ Target Lifetime: 10.0
□ Base Points: 100
```

---

### 5.4 Connect Manager References (3 min)

**GameManager:**
```
□ Target Spawner: Drag TargetSpawner object
□ UI Manager: Drag UIManager object
□ Game Duration: 0 (infinite for Training mode)
```

**UIManager:**
```
For now, leave empty (UI panels will be created later)
If you want quick test:
□ Main Menu Panel: (skip for now)
□ Gameplay HUD Panel: (skip for now)
```

**Save Scene:**
```
File → Save As
Name: "MainScene"
Location: Assets/Scenes/
```

---

### 5.5 Test Complete System (5 min)

**Add DebugHelper:**
```
Select: Any GameObject (e.g., GameManager)
Add Component → Debug Helper
□ Enable Debug Mode: ☑ On
□ Show Debug UI: ☑ On
□ Allow Keyboard Controls: ☑ On
□ Show FPS: ☑ On
```

**Test Without VR (Keyboard):**
```
1. Press Play ▶
2. Press '1' key → Should start Training mode
3. Check Console → "Started Training mode"
4. Check Hierarchy → Targets should spawn under PoolParent
5. Press 'T' key → Spawns test target
6. Click Mouse → Raycast shoot from camera
7. Should hit target, see points in console
8. Press Stop ■
```

**Test in VR:**
```
1. Put on VR headset
2. Press Play ▶ in Unity
3. Look around → Camera follows head
4. Grab weapon with grip button
5. Point at area where targets spawn
6. Press '1' key on keyboard (or implement UI)
7. Targets should spawn in front of you
8. Pull trigger → Should shoot
9. Hit target → Target disappears, score logged
10. Check FPS overlay (should be 90)
```

---

## ✅ Verification Checklist

After completing all steps, verify:

```
Core Setup:
□ XR Origin in scene with working camera
□ XR Interaction Manager present
□ Ray Interactors on both controllers
□ VR tracking works (head and hands)

Scripts:
□ All 13 C# scripts imported
□ No compilation errors
□ Scripts show in Add Component menu

Prefabs:
□ BasicTarget prefab exists
□ VR_Weapon prefab exists
□ Weapon attached to Right Controller

Managers:
□ GameManager in scene
□ AudioManager in scene
□ TargetSpawner in scene
□ UIManager with Canvas in scene
□ All managers connected

Gameplay:
□ Press '1' → Training mode starts
□ Targets spawn automatically
□ Weapon shoots when trigger pulled
□ Hitting target logs score
□ FPS shows 90 (or close)

Object Pooling:
□ Check PoolParent has spawned targets
□ Targets reuse, don't Instantiate
□ Max 3 active targets (Training mode)
```

---

## 🎮 Next Steps

Your basic VR Aim Lab is now functional! Continue with:

### Immediate (Today):
1. **Create UI Panels** (see SPRINT_PLAN.md Day 5)
   - Main menu for mode selection
   - HUD for score/timer display
   - Game over screen

2. **Add More Game Modes**
   - Speed mode config
   - Precision mode config

### Tomorrow:
3. **Add VFX**
   - Muzzle flash particle system
   - Hit effect particles
   - Bullet tracers

4. **Add Audio**
   - Shoot sounds
   - Hit sounds
   - Background music

5. **Polish**
   - Haptic feedback tuning
   - Performance optimization
   - Build for VR headset

---

## 🐛 Troubleshooting

### Issue: "Type or namespace 'XR' could not be found"
**Solution:**
```
1. Check XR Interaction Toolkit installed
2. Window → Package Manager → XR Interaction Toolkit
3. If not installed, install it
4. Restart Unity
```

---

### Issue: VR controllers not visible/tracking
**Solution:**
```
1. Edit → Project Settings → XR Plug-in Management
2. Ensure Oculus (Quest 2) or OpenXR (PC VR) enabled
3. Check headset connected and recognized by SteamVR/Oculus app
4. Restart Unity
```

---

### Issue: Weapon can't be grabbed
**Solution:**
```
1. Verify weapon has:
   - Collider (not trigger)
   - Rigidbody
   - XR Grab Interactable component
2. Verify controllers have XR Ray Interactor or XR Direct Interactor
3. Check Interaction Manager exists in scene
```

---

### Issue: Targets not spawning
**Solution:**
```
1. Check ObjectPooler has BasicTarget prefab assigned
2. Check TargetSpawner → Basic Target Prefab assigned
3. Press '1' to start Training mode
4. Check Console for spawn messages
5. Check spawn area is in front of camera
```

---

### Issue: Shooting doesn't hit targets
**Solution:**
```
1. Check VRWeaponController → Target Layer = "Target"
2. Check BasicTarget → Layer = "Target"
3. Check Shoot Point assigned in VRWeaponController
4. Enable laser sight to see where shooting
```

---

### Issue: Low FPS / Performance
**Solution:**
```
1. Reduce max active targets
2. Disable VFX temporarily
3. Lower texture quality
4. Edit → Project Settings → Quality → Set to "Medium"
5. Check Profiler (Window → Analysis → Profiler)
```

---

### Issue: UI not clickable in VR
**Solution:**
```
1. Check Canvas → Render Mode = World Space
2. Check Canvas has Graphic Raycaster component
3. Check controllers have XR Ray Interactor
4. Check Canvas positioned in front of player (0, 2, 2)
5. Check Canvas rotation faces player (0, 180, 0)
```

---

## 📚 Additional Resources

**Unity Documentation:**
- [XR Interaction Toolkit Manual](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.0/manual/index.html)
- [Universal Render Pipeline](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.0/manual/index.html)
- [TextMeshPro](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html)

**Project Documentation:**
- [README.md](README.md) - Project overview
- [SDD.md](SDD.md) - Architecture details
- [SPRINT_PLAN.md](SPRINT_PLAN.md) - Development timeline
- [NARRATIVA.md](NARRATIVA.md) - Story and world

---

## 🎯 Quick Commands Reference

**Unity Menu:**
```
VR Aim Lab → Setup Wizard          # Auto-create all managers
VR Aim Lab → Complete Scene Setup  # One-click setup
VR Aim Lab → Open Documentation    # Open docs folder
```

**Context Menu (Right-click QuickStartSetup):**
```
Find All References      # Auto-find managers
Create Missing Managers  # Create any missing GameObjects
Verify Setup            # Check if everything configured
Print Setup Checklist   # Show setup steps
```

**Keyboard Controls (in Play mode with DebugHelper):**
```
1-5: Start game modes
T: Spawn debug target
K: Kill all targets
P: Print stats
F1: Toggle debug UI
ESC: Pause
Mouse Click: Shoot (raycast from camera)
```

---

## ✨ Congratulations!

You now have a working VR Aim Lab prototype! 🎉

**What you've built:**
- ✅ VR-ready scene with XR Origin
- ✅ Grabbable weapon that shoots
- ✅ Spawning target system
- ✅ Object pooling for performance
- ✅ Basic scoring system
- ✅ Training mode playable

**Total setup time:** ~60 minutes

**Next:** Follow [SPRINT_PLAN.md](SPRINT_PLAN.md) to complete all features in 6 days!

---

**Document Version:** 1.0.0
**Last Updated:** November 2025
**Estimated Setup Time:** 60-90 minutes

**Welcome to APEX Academy, Recruit! 🎯**
