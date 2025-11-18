# VR Aim Lab - Software Design Document (SDD)

**Version:** 1.0.0
**Date:** November 2025
**Project:** APEX Neural Precision Academy 2045
**Target Platform:** VR (Quest 2, PC VR)
**Engine:** Unity 2021.3 LTS
**Performance Target:** 90 FPS sustained

---

## Table of Contents

1. [Overview](#overview)
2. [Technical Architecture](#technical-architecture)
3. [Core Systems](#core-systems)
4. [Game Modes](#game-modes)
5. [Gameplay Mechanics](#gameplay-mechanics)
6. [User Interface](#user-interface)
7. [Performance Optimization](#performance-optimization)
8. [Data Management](#data-management)
9. [VR Implementation](#vr-implementation)
10. [Future Enhancements](#future-enhancements)

---

## Overview

### Vision Statement
VR Aim Lab is a comprehensive VR aim training simulator set in 2045's APEX Neural Precision Academy. The system provides scientifically-designed training scenarios to improve player accuracy, reaction time, and tracking skills through immersive VR gameplay.

### Key Features
- **5 Specialized Game Modes**: Training, Precision, Speed, Tracking, Challenge
- **VR-Native Design**: Full XR Toolkit integration with haptic feedback
- **Progressive Difficulty**: Adaptive difficulty system that scales with performance
- **Performance Optimized**: Object pooling, 90fps target for VR comfort
- **Comprehensive Scoring**: Accuracy tracking, streak system, combo multipliers, ranking
- **Immersive Audio**: Spatial 3D audio with dynamic music system

### Target Specifications
- **Platform**: Unity 2021.3 LTS
- **Render Pipeline**: Universal Render Pipeline (URP)
- **VR SDK**: XR Interaction Toolkit 2.0+
- **UI Framework**: TextMeshPro + World Space Canvas
- **Target FPS**: 90 FPS (VR requirement)
- **Supported Headsets**: Meta Quest 2, PC VR (Steam VR, Oculus Link)

---

## Technical Architecture

### Architecture Diagram
```
┌─────────────────────────────────────────────────┐
│                  Game Layer                      │
│  ┌─────────────┐  ┌─────────────┐              │
│  │ GameManager │  │ AudioManager│              │
│  │ (Singleton) │  │ (Singleton) │              │
│  └─────────────┘  └─────────────┘              │
└─────────────────────────────────────────────────┘
                      │
┌─────────────────────────────────────────────────┐
│                Gameplay Layer                    │
│  ┌──────────────┐  ┌─────────────┐             │
│  │VRWeapon      │  │TargetSpawner│             │
│  │Controller    │  │             │             │
│  └──────────────┘  └─────────────┘             │
│  ┌──────────────┐  ┌─────────────┐             │
│  │ Target       │  │ObjectPooler │             │
│  │ System       │  │             │             │
│  └──────────────┘  └─────────────┘             │
└─────────────────────────────────────────────────┘
                      │
┌─────────────────────────────────────────────────┐
│                   UI Layer                       │
│  ┌──────────────┐  ┌─────────────┐             │
│  │ UIManager    │  │MenuController│             │
│  │              │  │             │             │
│  └──────────────┘  └─────────────┘             │
│  ┌──────────────┐                               │
│  │HUDController │                               │
│  │              │                               │
│  └──────────────┘                               │
└─────────────────────────────────────────────────┘
```

### Design Patterns

#### Singleton Pattern
Used for managers that need global access:
- **GameManager**: Central game state and flow control
- **AudioManager**: Centralized audio playback
- **ObjectPooler**: Global object pool access

#### Observer Pattern
Event-driven communication between systems:
- Score changes trigger UI updates
- Game state changes trigger panel switches
- Streak milestones trigger audio/visual feedback

#### Object Pool Pattern
Performance optimization for frequently instantiated objects:
- Target pooling (20+ objects pre-instantiated)
- VFX pooling for hit effects
- Reduces garbage collection during gameplay

#### Component Pattern
Modular, reusable components:
- `Target.cs` base class extended by `MovingTarget.cs`
- Clean separation of concerns (gameplay, UI, audio)

---

## Core Systems

### 1. Game Manager System

**Class:** `GameManager.cs`
**Responsibility:** Central game flow control

**Features:**
- Singleton instance management
- Game state machine (Menu → Playing → Paused → GameOver)
- 5 game mode configurations
- Timer system with mode-specific durations
- Adaptive difficulty progression
- High score persistence (PlayerPrefs)
- Event system for state changes

**State Machine:**
```
Menu ──StartGame()──> Playing ──PauseGame()──> Paused
                         │                         │
                         │                  ResumeGame()
                         │                         │
                         └───────EndGame()─────────┴──> GameOver
                                                           │
                                                   ReturnToMenu()
                                                           │
                                                          Menu
```

**Key Methods:**
- `StartGame(GameMode)`: Initialize and start game session
- `PauseGame()` / `ResumeGame()`: Toggle pause state
- `EndGame()`: Finalize session, save scores
- `OnTargetHit()`: Process target hit events
- `OnTargetExpired()`: Handle missed targets

### 2. Score System

**Class:** `ScoreSystem.cs`
**Responsibility:** Score calculation and progression

**Scoring Components:**
- **Base Points**: 100 per standard hit
- **Perfect Hit Bonus**: +50 for bullseye hits
- **Combo Multiplier**: x1 to x5 based on streak
- **Streak System**: Consecutive hits without misses
- **Accuracy Tracking**: Hits/Total Shots ratio

**Ranking System:**
| Rank | Score Required | Accuracy Required |
|------|----------------|-------------------|
| Bronze | 0+ | 0%+ |
| Silver | 500+ | 60%+ |
| Gold | 1500+ | 70%+ |
| Platinum | 3000+ | 80%+ |
| Diamond | 5000+ | 90%+ |

**Combo System:**
- Every 5 consecutive hits = +1 multiplier
- Max multiplier: x5
- Streak breaks on target expiry (not on miss)
- Combo milestone sound effects at 10, 20, 30+ streaks

### 3. Audio Manager

**Class:** `AudioManager.cs`
**Responsibility:** Centralized audio management

**Audio Channels:**
- **Music**: Background music (2D, looping)
- **SFX**: Spatial 3D sound effects
- **UI**: 2D interface sounds
- **Voice**: ARIA voice lines (highest priority)

**Features:**
- Volume control per channel
- Fade in/out for music transitions
- Spatial 3D audio for immersion
- Haptic-audio sync for weapon feedback
- PlayerPrefs persistence for volume settings

### 4. Target System

**Classes:** `Target.cs`, `MovingTarget.cs`
**Responsibility:** Target behavior and hit detection

**Target Types:**
| Type | Behavior | Lifetime | Points |
|------|----------|----------|--------|
| Basic | Stationary | 5s | 100 |
| Speed | Fast spawn/despawn | 3s | 150 |
| Precision | Small size | 6s | 200 |
| Moving | 6 movement patterns | 8s | 150 |
| Combo | Requires multiple hits | 10s | 300 |

**Movement Patterns:**
1. **Linear**: Straight line movement
2. **Circular**: Orbit around center point
3. **Zigzag**: Side-to-side with forward motion
4. **Random**: Random direction changes
5. **Figure-8**: Figure-eight pattern
6. **Bounce**: Bounce between waypoints

**Hit Zones:**
- **Outer** (full radius): 1x base points
- **Inner** (50% radius): 1.5x base points
- **Bullseye** (20% radius): 2x base points + perfect bonus

### 5. Weapon System

**Class:** `VRWeaponController.cs`
**Responsibility:** VR weapon mechanics

**Features:**
- XR Grab Interactable integration
- Raycast-based shooting (Physics.Raycast)
- Configurable fire rate and ammo
- Automatic reload via grip button
- Haptic feedback on shoot/reload
- Visual laser sight with LineRenderer
- Muzzle flash and bullet tracer VFX
- Recoil simulation with recovery

**Shooting Pipeline:**
```
Trigger Press → Check Ammo → Raycast → Hit Detection →
→ Target.OnHit() → GameManager.OnTargetHit() →
→ Score Update → VFX + Audio + Haptics
```

### 6. Spawning System

**Class:** `TargetSpawner.cs`
**Responsibility:** Target spawning and difficulty scaling

**Features:**
- Object pool integration
- Wave-based spawning
- Mode-specific configurations
- Adaptive difficulty (decrease interval, increase count)
- Random or fixed spawn points
- Configurable spawn area bounds

**Difficulty Progression:**
- Level increases every 30 seconds
- Spawn interval decreases by 0.1s per level (min 0.3s)
- Max active targets +1 per level
- Different progression curves per game mode

---

## Game Modes

### 1. Training Mode
**Purpose:** Practice and warm-up
**Duration:** Infinite (no timer)
**Targets:** Basic stationary targets
**Spawn Rate:** 2s interval
**Max Active:** 3 targets
**Difficulty:** Static

**Use Case:** New players, warm-up routine, accuracy practice

---

### 2. Precision Mode
**Purpose:** Accuracy and precision training
**Duration:** 90 seconds
**Targets:** Precision (small) + Basic
**Spawn Rate:** 2s initial → 1.5s (progresses)
**Max Active:** 4 → 6 targets
**Difficulty:** Progressive

**Scoring Focus:**
- Bullseye hits worth significantly more
- Accuracy heavily weighted
- Tight grouping rewarded

---

### 3. Speed Mode
**Purpose:** Reaction time and speed
**Duration:** 60 seconds
**Targets:** Speed (fast despawn) + Basic
**Spawn Rate:** 0.8s initial → 0.5s (progresses)
**Max Active:** 6 → 10 targets
**Difficulty:** Aggressive progression

**Scoring Focus:**
- High volume of targets
- Quick reflexes rewarded
- Combo multipliers crucial

---

### 4. Tracking Mode
**Purpose:** Moving target tracking
**Duration:** 120 seconds
**Targets:** Moving targets only
**Spawn Rate:** 1.5s interval
**Max Active:** 4 → 6 targets
**Difficulty:** Progressive

**Scoring Focus:**
- Lead shots required
- Tracking skill development
- Moving target hit bonus

---

### 5. Challenge Mode
**Purpose:** Ultimate skill test
**Duration:** 180 seconds
**Targets:** All types (random mix)
**Spawn Rate:** 1s initial → 0.4s (progresses)
**Max Active:** 8 → 12 targets
**Difficulty:** Extreme progression

**Scoring Focus:**
- Adaptability to all target types
- Sustained high performance
- Diamond rank achievement

---

## Gameplay Mechanics

### Hit Detection
```csharp
// Raycast from weapon
Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, range, targetLayer)

// Determine hit zone
float distanceFromCenter = Vector3.Distance(hitPoint, targetCenter);
if (distance <= bullseyeRadius) → Bullseye (2x points)
else if (distance <= innerRadius) → Inner (1.5x points)
else → Outer (1x points)

// Apply combo multiplier
finalPoints = basePoints * zoneMultiplier * comboMultiplier
```

### Streak System
- **Build:** Consecutive hits without target expiry
- **Break:** When target despawns without being hit
- **Miss:** Does NOT break streak (encourages shooting)
- **Milestones:** Audio/visual feedback at 10, 20, 30+

### Combo Multiplier
```
Streak:  0-4  = x1
Streak:  5-9  = x2
Streak: 10-14 = x3
Streak: 15-19 = x4
Streak: 20+   = x5 (max)
```

### Accuracy Calculation
```
Accuracy = (SuccessfulHits / TotalShots) * 100%
```
- Displayed in real-time on HUD
- Color-coded: Green (90%+), Yellow (70%+), Orange (50%+), Red (<50%)

---

## User Interface

### UI Architecture
**Canvas Type:** World Space (VR compatible)
**Text Rendering:** TextMeshPro
**Interaction:** XR Ray Interactor

### UI Panels

#### 1. Main Menu
**Components:**
- Game mode selection buttons (5 modes)
- Mode description panel
- High score display per mode
- Settings panel (volume, laser sight)
- Start button
- Quit button

#### 2. Gameplay HUD
**Elements:**
- **Top Center:** Score (large, animated)
- **Top Right:** Timer (countdown with color warnings)
- **Top Left:** Accuracy percentage
- **Middle Left:** Current streak
- **Middle Right:** Combo multiplier (if active)
- **Bottom Center:** Ammo counter with fill bar
- **Bottom Right:** Current rank badge

#### 3. Pause Menu
**Options:**
- Resume button
- Restart button
- Settings (volume)
- Return to Menu

#### 4. Game Over Screen
**Display:**
- Final score (large, highlighted)
- Accuracy percentage
- Max streak achieved
- Final rank (colored badge)
- High score indicator (NEW HIGH SCORE!)
- Buttons: Restart, Menu

---

## Performance Optimization

### Target: 90 FPS Sustained

### Optimization Techniques

#### 1. Object Pooling
**Implementation:** `ObjectPooler.cs`
- Pre-instantiate 20 targets per type
- Reuse targets instead of Instantiate/Destroy
- Reduces GC pressure during gameplay
- Queue-based pool management

```csharp
// Instead of:
Instantiate(targetPrefab, position, rotation);
Destroy(target, 5f);

// Use:
ObjectPooler.Instance.SpawnFromPool("BasicTarget", position, rotation);
target.SetActive(false); // Returns to pool
```

#### 2. Material Property Blocks
**Usage:** Target color changes
- Avoids creating material instances
- Reduces draw calls
- Used in `Target.SetTargetColor()`

#### 3. Occlusion Culling
**Recommendation:** Enable in Unity
- Automatically cull objects not visible to camera
- Reduces rendering overhead

#### 4. Physics Optimization
- **Layer Masks:** Raycast only against target layer
- **Fixed Update:** Physics at 90Hz for VR
- **Collider Optimization:** Simple colliders (sphere, box)

#### 5. UI Optimization
- **World Space Canvas:** One canvas for all UI
- **Canvas Groups:** For fade effects without rebuilding
- **TextMeshPro:** GPU-accelerated text rendering

#### 6. Audio Optimization
- **2D Audio:** For non-spatial sounds (music, UI)
- **3D Audio:** Limited range for spatial sounds
- **Audio Source Pooling:** Reuse audio sources

### Performance Monitoring
Use `DebugHelper.cs`:
- Real-time FPS display
- Memory usage tracking
- Performance warnings logged

---

## Data Management

### Save System
**Technology:** PlayerPrefs (simple persistence)

**Saved Data:**
```csharp
// Per Game Mode:
PlayerPrefs.SetInt($"HighScore_{gameMode}", score);
PlayerPrefs.SetFloat($"HighScoreAccuracy_{gameMode}", accuracy);
PlayerPrefs.SetInt($"HighScoreStreak_{gameMode}", maxStreak);

// Settings:
PlayerPrefs.SetFloat("MasterVolume", volume);
PlayerPrefs.SetFloat("MusicVolume", volume);
PlayerPrefs.SetFloat("SFXVolume", volume);
PlayerPrefs.SetInt("LaserSight", enabled ? 1 : 0);
```

### Data Flow
```
Gameplay → ScoreSystem → GameManager.EndGame() →
→ SaveHighScore() → PlayerPrefs.Save() → Disk
```

**Future Enhancement:** JSON serialization for more complex data structures

---

## VR Implementation

### XR Toolkit Integration

#### XR Origin Setup
```
XR Origin (VR)
├── Camera Offset
│   └── Main Camera
├── Left Controller
│   ├── XR Controller (Left)
│   └── XR Ray Interactor
└── Right Controller
    ├── XR Controller (Right)
    ├── XR Ray Interactor
    └── Weapon (attached)
```

#### Weapon Setup
```csharp
// VRWeaponController components:
- XRGrabInteractable (for picking up weapon)
- Collider (for grab detection)
- Rigidbody (physics)
- VRWeaponController script

// Events:
grabInteractable.selectEntered += OnGrab;
grabInteractable.selectExited += OnRelease;
grabInteractable.activated += OnTriggerPressed;
```

#### Input Mapping
- **Trigger:** Shoot
- **Grip:** Reload
- **Grab:** Pick up weapon
- **UI Interaction:** Ray interactor with canvas

### Haptic Feedback
```csharp
// Shoot feedback
controller.SendHapticImpulse(0.5f, 0.1f);

// Reload feedback
controller.SendHapticImpulse(0.3f, 0.2f);

// Perfect hit feedback
controller.SendHapticImpulse(0.7f, 0.15f);
```

### Comfort & Accessibility
- **Stationary Gameplay:** No locomotion (reduces motion sickness)
- **Seated or Standing:** Flexible play styles
- **Adjustable Spawn Area:** Accommodate different play spaces
- **Laser Sight:** Optional for aiming assistance
- **Audio Cues:** Complement visual feedback

---

## Future Enhancements

### Phase 2 Features
1. **Multiplayer Support**
   - Competitive leaderboards
   - Co-op training missions
   - Ghost replay system

2. **Advanced Analytics**
   - Heat maps of hit distribution
   - Performance graphs over time
   - AI-driven coaching tips

3. **Customization**
   - Custom weapon skins
   - Adjustable target colors/sizes
   - Create custom scenarios

4. **Progression System**
   - Unlock system for weapons/modes
   - Achievement badges
   - Seasonal challenges

5. **Additional Game Modes**
   - Reflex mode (instant hit registration)
   - Flick mode (quick target switching)
   - Strafe mode (moving player)

### Technical Improvements
- Save system migration to JSON/binary
- Cloud save integration
- Replay recording system
- Advanced VFX (particle systems, shaders)
- Mod support

---

## Conclusion

VR Aim Lab provides a solid foundation for VR aim training with:
- ✅ Clean, modular architecture
- ✅ 5 diverse game modes
- ✅ Performance-optimized (90fps target)
- ✅ Full VR integration with haptics
- ✅ Comprehensive scoring and progression
- ✅ Extensible design for future features

**Development Timeline:** 6 days for MVP
**Code Quality:** Production-ready, well-documented
**Scalability:** Designed for easy feature additions

---

**Document Version:** 1.0.0
**Last Updated:** November 2025
**Authors:** VR Aim Lab Development Team
