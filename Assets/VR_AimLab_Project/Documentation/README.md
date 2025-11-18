# VR Aim Lab - README

**APEX Neural Precision Academy 2045**

A comprehensive VR aim training simulator for Unity with 5 game modes, advanced scoring, and full XR Toolkit integration.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Project Structure](#project-structure)
- [Game Modes](#game-modes)
- [Controls](#controls)
- [Troubleshooting](#troubleshooting)
- [Performance Tips](#performance-tips)
- [Documentation](#documentation)
- [Support](#support)

---

## 🎯 Overview

VR Aim Lab is a training simulator designed to improve precision, reaction time, and tracking skills in VR. Built with Unity 2021.3 LTS and XR Interaction Toolkit, it features:

- **5 Game Modes**: Training, Precision, Speed, Tracking, Challenge
- **Advanced Scoring**: Accuracy tracking, streak system, combo multipliers, rankings (Bronze→Diamond)
- **VR Optimized**: 90fps target, object pooling, haptic feedback
- **Full XR Integration**: Compatible with Meta Quest 2, PC VR headsets

**Target Platform:** VR (Quest 2, PC VR via Steam VR/Oculus Link)
**Development Time:** 6 days for MVP
**Performance Target:** 90 FPS sustained

---

## ✨ Features

### Core Gameplay
- ✅ **5 Training Modes** with unique difficulty curves
- ✅ **Dynamic Target System** with 6 movement patterns
- ✅ **Precision Zones** (Outer, Inner, Bullseye)
- ✅ **Combo System** (x1 to x5 multipliers)
- ✅ **Streak Tracking** with milestone rewards
- ✅ **Rank Progression** (Bronze → Silver → Gold → Platinum → Diamond)

### VR Features
- ✅ **Full XR Toolkit** integration
- ✅ **Haptic Feedback** on shoot/hit/reload
- ✅ **Laser Sight** with real-time targeting
- ✅ **Raycast Shooting** with VFX
- ✅ **World Space UI** optimized for VR
- ✅ **Spatial 3D Audio** for immersion

### Performance
- ✅ **Object Pooling** for targets and VFX
- ✅ **Material Property Blocks** (no material instances)
- ✅ **90 FPS Target** maintained
- ✅ **Optimized Raycasts** with layer masks
- ✅ **Efficient UI Updates** (event-driven)

---

## 📦 Requirements

### Hardware Requirements

**Minimum:**
- **VR Headset:** Meta Quest 2 or equivalent
- **PC (for PC VR):**
  - CPU: Intel i5-7500 / AMD Ryzen 5 1600
  - GPU: NVIDIA GTX 1060 / AMD RX 580
  - RAM: 8 GB
  - Storage: 2 GB available

**Recommended:**
- **PC:**
  - CPU: Intel i7-9700K / AMD Ryzen 7 3700X
  - GPU: NVIDIA RTX 2070 / AMD RX 5700 XT
  - RAM: 16 GB
  - Storage: 2 GB SSD

### Software Requirements

**Unity:**
- **Version:** Unity 2021.3 LTS or newer
- **Render Pipeline:** Universal Render Pipeline (URP)

**Required Packages:**
- XR Interaction Toolkit 2.0+
- XR Plugin Management
- TextMeshPro
- Input System (new)

**Platform SDKs:**
- Meta XR SDK (for Quest 2)
- OpenXR / SteamVR (for PC VR)

---

## 🚀 Installation

### Step 1: Clone/Download Project

```bash
# Option 1: Git clone (if using version control)
git clone <repository-url>

# Option 2: Download and extract the VR_AimLab_Project folder
```

### Step 2: Open in Unity

1. Open Unity Hub
2. Click **Add** → **Add project from disk**
3. Navigate to `VR_AimLab_Project` folder
4. Select folder and click **Open**
5. Unity will import the project (may take 5-10 minutes)

### Step 3: Install Required Packages

1. Open **Window → Package Manager**
2. Install the following packages:

```
✓ XR Interaction Toolkit (2.0 or newer)
✓ XR Plugin Management
✓ TextMeshPro (import TMP Essentials when prompted)
✓ Input System
```

3. For Quest 2: Also install **Oculus XR Plugin**
4. For PC VR: Also install **OpenXR Plugin** or **SteamVR**

### Step 4: Configure XR Settings

1. Go to **Edit → Project Settings → XR Plug-in Management**
2. Enable your target platform(s):
   - **Android** (for Quest 2): Enable **Oculus**
   - **PC**: Enable **OpenXR** or **SteamVR**

---

## ⚡ Quick Start

### Automated Setup (Recommended)

1. Open the scene: `Scenes/MainScene.unity`
2. In Unity menu: **VR Aim Lab → Setup Wizard**
3. Click **"Yes, Auto Setup"**
4. Follow on-screen instructions
5. Press Play to test!

### Manual Setup

If you prefer manual setup, see [UNITY_SETUP_GUIDE.md](UNITY_SETUP_GUIDE.md) for detailed 60-minute step-by-step instructions.

### Quick Test (No VR)

Use `DebugHelper.cs` for keyboard testing:

```
1-5: Start game modes
Mouse: Shoot (raycast from camera)
T: Spawn debug target
ESC: Pause
P: Print stats
```

---

## 📁 Project Structure

```
VR_AimLab_Project/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs        # Main game loop & state
│   │   │   ├── AudioManager.cs       # Audio system
│   │   │   └── ScoreSystem.cs        # Scoring & ranking
│   │   ├── Gameplay/
│   │   │   ├── Target.cs             # Base target class
│   │   │   ├── MovingTarget.cs       # Moving target behaviors
│   │   │   ├── TargetSpawner.cs      # Spawn management
│   │   │   ├── VRWeaponController.cs # VR shooting mechanics
│   │   │   └── ObjectPooler.cs       # Performance optimization
│   │   ├── UI/
│   │   │   ├── UIManager.cs          # UI panel management
│   │   │   ├── MenuController.cs     # Main menu logic
│   │   │   └── HUDController.cs      # In-game HUD
│   │   └── Utilities/
│   │       ├── QuickStartSetup.cs    # Auto-setup helper
│   │       └── DebugHelper.cs        # Testing utilities
│   ├── Prefabs/       # Target & weapon prefabs (create these)
│   ├── Materials/     # Materials for targets/weapons
│   ├── Audio/         # Sound effects & music
│   └── Scenes/        # Game scenes
├── Documentation/
│   ├── README.md              # This file
│   ├── SDD.md                 # Software design document
│   ├── NARRATIVA.md           # Narrative & world design
│   ├── SPRINT_PLAN.md         # 6-day development plan
│   └── UNITY_SETUP_GUIDE.md   # Step-by-step Unity setup
└── Packages/          # Unity packages config
```

---

## 🎮 Game Modes

### 1. Training Mode
- **Duration:** Infinite (no timer)
- **Purpose:** Practice and warm-up
- **Targets:** Basic stationary
- **Difficulty:** Static

### 2. Precision Mode
- **Duration:** 90 seconds
- **Purpose:** Accuracy training
- **Targets:** Small precision targets + Basic
- **Difficulty:** Progressive

### 3. Speed Mode
- **Duration:** 60 seconds
- **Purpose:** Reaction time
- **Targets:** Fast-despawn targets + Basic
- **Difficulty:** Aggressive progression

### 4. Tracking Mode
- **Duration:** 120 seconds
- **Purpose:** Moving target practice
- **Targets:** Moving targets only (6 patterns)
- **Difficulty:** Progressive

### 5. Challenge Mode
- **Duration:** 180 seconds
- **Purpose:** Ultimate test
- **Targets:** All types (random mix)
- **Difficulty:** Extreme progression

---

## 🕹️ Controls

### VR Controls

**Shooting:**
- **Trigger** (Right/Left Hand): Shoot
- **Grip Button**: Reload

**Weapon:**
- **Grab**: Pick up weapon from holster/table

**UI Interaction:**
- **Ray Pointer**: Point at UI buttons
- **Trigger**: Click buttons

### Keyboard Controls (Debug Mode)

**Game Modes:**
- `1` - Training Mode
- `2` - Precision Mode
- `3` - Speed Mode
- `4` - Tracking Mode
- `5` - Challenge Mode

**Game Control:**
- `ESC` - Pause/Resume
- `R` - Restart
- `M` - Return to Menu

**Debug:**
- `T` - Spawn debug target
- `K` - Kill all targets
- `P` - Print statistics
- `F1` - Toggle debug UI
- `Mouse Click` - Shoot (raycast from camera)

---

## 🔧 Troubleshooting

### Common Issues

#### 1. "XR Interaction Manager not found"

**Solution:**
```
1. Create: GameObject → XR → Interaction Manager
2. Verify it's in the scene
3. Check QuickStartSetup component found it
```

#### 2. "No XR Origin found"

**Solution:**
```
1. Create: GameObject → XR → XR Origin (VR)
2. Ensure it has Camera Offset and Main Camera child
3. Add Left/Right Controller objects
```

#### 3. Targets not spawning

**Solution:**
```
1. Check TargetSpawner has target prefabs assigned
2. Verify ObjectPooler pools are configured
3. Check spawn area bounds are valid
4. Enable Debug Mode and press 'T' to test spawn
```

#### 4. VR controllers not tracking

**Solution:**
```
1. Verify XR Plugin is enabled (Edit → Project Settings → XR)
2. Check headset is connected and recognized
3. Restart Unity
4. Check SteamVR/Oculus app is running
```

#### 5. Low FPS / Performance issues

**Solution:**
```
1. Check object pooling is active
2. Reduce max active targets in TargetSpawner
3. Disable unnecessary visual effects
4. Build → Player Settings → Disable vsync
5. Use URP instead of Built-in pipeline
```

#### 6. UI not visible in VR

**Solution:**
```
1. Check Canvas is set to World Space
2. Verify Canvas is positioned in front of camera
3. Check Canvas scale (0.001 recommended)
4. Add Graphics Raycaster component to Canvas
```

#### 7. Audio not playing

**Solution:**
```
1. Check AudioManager is in scene
2. Verify audio clips are assigned in Inspector
3. Check volume sliders are > 0
4. Test with PlayButtonClick() from menu
```

### Performance Optimization

If experiencing FPS drops:

1. **Reduce Active Targets**: Lower `maxActiveTargets` in TargetSpawner
2. **Increase Spawn Interval**: Increase `spawnInterval` to spawn slower
3. **Disable VFX**: Temporarily disable muzzle flash/tracers
4. **Simplify Materials**: Use unlit materials for targets
5. **Reduce Resolution**: Lower render resolution in XR settings

### Build Issues

**Android (Quest 2):**
```
1. Install Android Build Support in Unity Hub
2. Edit → Project Settings → Player → Android
3. Minimum API Level: Android 10 (API 29)
4. Graphics API: OpenGLES3 or Vulkan
5. IL2CPP Scripting Backend
6. ARM64 Architecture
```

**PC VR:**
```
1. Platform: Windows
2. Architecture: x86_64
3. Graphics API: Direct3D11
4. Scripting Backend: Mono (faster builds) or IL2CPP (better performance)
```

---

## 🚀 Performance Tips

### Achieving 90 FPS

1. **Enable Object Pooling**: Already implemented in `ObjectPooler.cs`
2. **Use Material Property Blocks**: Already used in `Target.cs`
3. **Optimize Raycasts**: Already using layer masks
4. **Reduce Draw Calls**:
   - Use texture atlases
   - Combine meshes where possible
   - Use URP batching
5. **Physics Optimization**:
   - Set Fixed Timestep to 0.0111 (90Hz)
   - Use simple colliders (sphere, box)
6. **VFX Optimization**:
   - Limit particle count
   - Use GPU particles
   - Pool VFX instances

### Monitoring Performance

Use `DebugHelper.cs`:
```csharp
// Enable in Inspector:
- Show FPS: true
- Show Memory Usage: true
- Log Performance Warnings: true
```

### Unity Profiler

1. Open **Window → Analysis → Profiler**
2. Run game in Editor or Development Build
3. Check:
   - CPU Usage (target: < 11ms per frame)
   - GPU Usage
   - GC Allocations (minimize)
   - Physics time

---

## 📚 Documentation

### Available Documents

1. **README.md** (this file) - Overview, setup, troubleshooting
2. **SDD.md** - Technical architecture, systems design
3. **NARRATIVA.md** - Story, characters, world design
4. **SPRINT_PLAN.md** - 6-day development schedule
5. **UNITY_SETUP_GUIDE.md** - 60-minute step-by-step setup

### Reading Order

**For Developers:**
1. README.md (setup & overview)
2. UNITY_SETUP_GUIDE.md (implementation)
3. SDD.md (architecture deep-dive)

**For Designers:**
1. NARRATIVA.md (world & story)
2. SDD.md (game modes & mechanics)
3. README.md (technical context)

**For Project Managers:**
1. SPRINT_PLAN.md (timeline & milestones)
2. README.md (scope & features)
3. SDD.md (technical requirements)

---

## 🎯 Development Checklist

Use this checklist to track MVP implementation:

### Phase 1: Unity Setup (Day 1)
- [ ] Unity 2021.3 LTS installed
- [ ] XR Interaction Toolkit imported
- [ ] TextMeshPro imported (TMP Essentials)
- [ ] XR Plugin configured (Oculus/OpenXR)
- [ ] URP configured
- [ ] All scripts imported to Assets/Scripts/

### Phase 2: Scene Setup (Day 1-2)
- [ ] XR Origin created in scene
- [ ] XR Interaction Manager added
- [ ] GameManager created with ScoreSystem
- [ ] AudioManager created
- [ ] TargetSpawner + ObjectPooler created
- [ ] UIManager with Canvas created
- [ ] QuickStartSetup component added

### Phase 3: Prefabs (Day 2-3)
- [ ] BasicTarget prefab (Sphere + Target.cs + Collider)
- [ ] MovingTarget prefab (extends BasicTarget)
- [ ] PrecisionTarget prefab (smaller scale)
- [ ] Weapon prefab (model + VRWeaponController + XRGrabInteractable)
- [ ] Hit effect prefab (particle system)
- [ ] Muzzle flash prefab

### Phase 4: UI (Day 3-4)
- [ ] Main Menu panel created
- [ ] Gameplay HUD panel created
- [ ] Pause Menu panel created
- [ ] Game Over panel created
- [ ] All TextMeshPro text elements added
- [ ] Buttons connected to MenuController

### Phase 5: Integration (Day 4-5)
- [ ] Managers connected in Inspector
- [ ] Target prefabs assigned to spawner
- [ ] Object pools configured
- [ ] Spawn points/area configured
- [ ] UI references connected
- [ ] Audio clips assigned (if available)

### Phase 6: Testing & Polish (Day 5-6)
- [ ] All 5 game modes tested
- [ ] Scoring system verified
- [ ] VR controls tested (headset)
- [ ] Performance check (90fps achieved)
- [ ] High score save/load tested
- [ ] Build created for target platform

### Optional Enhancements
- [ ] Audio clips added (SFX, music, voice)
- [ ] Additional VFX (tracers, impacts)
- [ ] Multiple weapon models
- [ ] Advanced target visuals
- [ ] Environment meshes/backgrounds

---

## 💬 Support

### Getting Help

**Documentation:**
- Read [UNITY_SETUP_GUIDE.md](UNITY_SETUP_GUIDE.md) for detailed setup
- Check [SDD.md](SDD.md) for architecture questions
- See [Troubleshooting](#troubleshooting) section above

**Debug Tools:**
- Use `QuickStartSetup` → "Verify Setup" context menu
- Enable `DebugHelper` for keyboard testing
- Check Unity Console for error messages

**Common Commands:**
```
// In Unity menu:
VR Aim Lab → Setup Wizard       # Auto-setup tool
VR Aim Lab → Complete Scene Setup   # Create all managers
VR Aim Lab → Open Documentation     # Open docs folder

// QuickStartSetup context menu:
Find All References
Create Missing Managers
Verify Setup
Print Setup Checklist
```

### Reporting Issues

When reporting issues, include:
1. Unity version
2. XR Toolkit version
3. Target platform (Quest 2, PC VR)
4. Console error messages
5. Steps to reproduce

---

## 📝 License

This project is created for educational and training purposes.

---

## 🏆 Credits

**Development:**
- VR Aim Lab Development Team

**Technologies:**
- Unity 2021.3 LTS
- XR Interaction Toolkit
- Universal Render Pipeline
- TextMeshPro

**Inspired by:**
- Aim Lab (PC)
- Aim Trainer VR
- Modern VR training simulators

---

## 🎉 Quick Start Summary

```bash
1. Open project in Unity 2021.3 LTS
2. Install XR Interaction Toolkit from Package Manager
3. Run: VR Aim Lab → Setup Wizard
4. Create target prefabs (Sphere + Target.cs)
5. Create weapon prefab (Model + VRWeaponController)
6. Assign prefabs in Inspector
7. Press Play to test!
```

**Estimated Setup Time:** 60-90 minutes (first time)

**Ready to start?** Open [UNITY_SETUP_GUIDE.md](UNITY_SETUP_GUIDE.md) for step-by-step instructions!

---

**Version:** 1.0.0
**Last Updated:** November 2025
**Deadline:** November 19, 2025 (6 days)

**Good luck, Recruit! Welcome to APEX Academy. 🎯**
