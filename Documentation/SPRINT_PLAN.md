# SPRINT PLAN - VR Aim Lab APEX
## Cronograma de 6 Dias para MVP

**Deadline:** 19 Novembro 2025
**Objetivo:** Protótipo jogável com 5 modos de jogo funcionais
**Target Performance:** 90fps em Meta Quest 2

---

## Visão Geral do Sprint

| Dia | Foco Principal | Horas | Deliverables | Status |
|-----|----------------|-------|--------------|--------|
| **Dia 1** | Unity Setup & XR Config | 4-5h | Projeto configurado, XR funcionando | ✅ Completo |
| **Dia 2** | Weapon System | 4-5h | Arma dispara, raycast, haptics | ✅ Completo |
| **Dia 3** | Target System & Pooling | 4-5h | Targets spawn/destroy, pooling | ✅ Completo |
| **Dia 4** | Game Logic & Scoring | 5-6h | GameManager, ScoreSystem, UI | ✅ Completo |
| **Dia 5** | Modos de Jogo & Polish | 5-6h | 5 modos, audio, VFX | ✅ Completo |
| **Dia 6** | Testing & Optimization | 6-8h | Build, performance, bugs | 🔄 Em progresso |

**Total Estimado:** 28-35 horas
**Metodologia:** Agile, iterações diárias
**Prioridade:** MVP features first, polish if time allows

---

## DIA 1: Unity Setup & XR Configuration
**Data:** 13 Novembro
**Duração:** 4-5 horas
**Objetivo:** Projeto Unity funcional com VR básico

### ✅ Tarefas Completadas

#### 1.1 Criação do Projeto (30min)
- [x] Criar projeto Unity 2021.3 LTS
- [x] Selecionar template 3D (URP)
- [x] Configurar Project Settings
  - [x] Color Space: Linear
  - [x] Graphics API: Vulkan (Android)
  - [x] Target Platform: Android (Quest 2)

#### 1.2 Importação de Packages (45min)
- [x] XR Plugin Management
- [x] XR Interaction Toolkit (v2.3+)
- [x] Universal Render Pipeline (URP)
- [x] TextMeshPro
- [x] Input System (new)

#### 1.3 XR Configuration (1h)
- [x] XR Plugin Management → OpenXR
- [x] Interaction Layer Mask setup
- [x] Criar XR Origin
  - [x] Main Camera (VR)
  - [x] Left/Right Hand Controllers
  - [x] XR Interaction Manager
- [x] Testar com XR Device Simulator

#### 1.4 Folder Structure (30min)
```
Assets/
├── Scenes/
├── Scripts/
│   ├── Core/
│   ├── Gameplay/
│   ├── UI/
│   ├── Utilities/
│   └── Editor/
├── Prefabs/
├── Materials/
└── Audio/
```

#### 1.5 Verification & Testing (1h)
- [x] XR Origin funcional
- [x] Controllers aparecem em VR
- [x] Input System respondendo
- [x] Build settings configurados (Android)
- [x] Primeiro build teste (PC VR ou Simulator)

**Deliverables:**
- ✅ Projeto Unity configurado
- ✅ XR funcionando (device simulator ou headset)
- ✅ Structure de folders criada
- ✅ Primeiro build teste executável

**Troubleshooting:**
- Se XR não funcionar: Verificar XR Plugin Management settings
- Se build falhar: Verificar Android SDK instalado
- Se controllers não aparecem: Verificar XRRig hierarchy

---

## DIA 2: Weapon System
**Data:** 14 Novembro
**Duração:** 4-5 horas
**Objetivo:** Arma VR funcional com disparo e haptics

### ✅ Tarefas Completadas

#### 2.1 VRWeaponController.cs (2h)
- [x] Criar script VRWeaponController.cs
- [x] Implementar raycast shoot
  ```csharp
  - [x] Physics.Raycast do shootPoint
  - [x] Detect hit (Debug.DrawLine)
  - [x] Range check (100 units)
  ```
- [x] Fire rate com cooldown (Time.time)
- [x] Ammo system (30 rounds)
- [x] Reload functionality (grip button)

#### 2.2 XR Integration (1.5h)
- [x] Adicionar XRGrabInteractable
- [x] Configurar grab settings
  - [x] Movement Type: Velocity Tracking
  - [x] Throw on Detach: true
- [x] Input Actions
  - [x] Trigger → Shoot
  - [x] Grip → Reload
- [x] Haptic feedback
  ```csharp
  - [x] XRBaseController.SendHapticImpulse
  - [x] Intensity: 0.5, Duration: 0.1
  ```

#### 2.3 Laser Sight (45min)
- [x] Adicionar LineRenderer
- [x] Update laser position cada frame
- [x] Cores: vermelho (idle), verde (hit)
- [x] Toggle laser (settings)

#### 2.4 Weapon Prefab Creation (45min)
- [x] Criar prefab VR_Pistol
- [x] Visual básico (cubes/primitives)
- [x] Adicionar ShootPoint (empty GameObject)
- [x] Configurar collider para grabbing
- [x] Adicionar Rigidbody (isKinematic quando grabbed)

#### 2.5 Testing (30min)
- [x] Grab weapon funcional
- [x] Shoot dispara raycast
- [x] Haptics funcionam
- [x] Laser sight visível
- [x] Reload funciona

**Deliverables:**
- ✅ VR_Pistol.prefab funcional
- ✅ Grab + Shoot funcionando
- ✅ Haptic feedback
- ✅ Laser sight

**MVP Features:**
- ✅ Raycast shooting
- ✅ Basic ammo system
- ✅ VR grabbing

**Nice-to-Have (se tempo):**
- ⏳ Muzzle flash (partículas)
- ⏳ Bullet tracers
- ⏳ Recoil animation

---

## DIA 3: Target System & Object Pooling
**Data:** 15 Novembro
**Duração:** 4-5 horas
**Objetivo:** Targets spawnam, detectam hits, e são pooled

### ✅ Tarefas Completadas

#### 3.1 Target.cs (Base Class) (1.5h)
- [x] Criar Target.cs
- [x] Hit detection
  ```csharp
  - [x] OnHit(hitPoint, normal)
  - [x] Calculate hit zone (bullseye, inner, outer)
  - [x] Return points
  ```
- [x] Lifetime timer (5-10s)
- [x] Visual feedback
  - [x] Color change on hit
  - [x] Destroy effect (simple)
- [x] Events: OnTargetHit, OnTargetExpired

#### 3.2 MovingTarget.cs (1h)
- [x] Extend Target.cs
- [x] 6 Movement patterns
  ```csharp
  - [x] Linear
  - [x] Circular
  - [x] Zigzag
  - [x] Random
  - [x] Figure8
  - [x] Bounce (entre pontos)
  ```
- [x] Configurável (speed, pattern, bounds)

#### 3.3 ObjectPooler.cs (1h)
- [x] Criar ObjectPooler.cs
- [x] Pool management
  ```csharp
  - [x] Dictionary<string, Queue<GameObject>>
  - [x] SpawnFromPool(tag, position, rotation)
  - [x] ReturnToPool(tag, GameObject)
  - [x] ExpandPool se necessário
  ```
- [x] Pool initial size: 20 por tipo

#### 3.4 TargetSpawner.cs (1h)
- [x] Spawn logic
  - [x] Random positions (ou fixed spawn points)
  - [x] Spawn interval configurável
  - [x] Max active targets
- [x] Integration com ObjectPooler
- [x] Spawn control (Start/Stop)

#### 3.5 Prefabs Creation (30min)
- [x] BasicTarget.prefab (sphere, vermelho)
- [x] MovingTarget.prefab (sphere, verde)
- [x] PrecisionTarget.prefab (sphere menor, azul)
- [x] Configurar colliders e Target.cs

**Deliverables:**
- ✅ Target.cs + MovingTarget.cs funcionais
- ✅ ObjectPooler implementado
- ✅ TargetSpawner spawnando targets
- ✅ 3 prefabs de targets

**Verification:**
- [x] Targets aparecem na cena
- [x] Weapon pode destruir targets
- [x] Pooling funciona (no Instantiate spam)
- [x] MovingTargets se movem

---

## DIA 4: Game Logic, Scoring & UI
**Data:** 16 Novembro
**Duração:** 5-6 horas
**Objetivo:** Sistema de jogo completo com UI funcional

### ✅ Tarefas Completadas

#### 4.1 GameManager.cs (Singleton) (1.5h)
- [x] Singleton pattern
- [x] GameState enum (Menu, Playing, Paused, GameOver)
- [x] GameMode enum (Training, Precision, Speed, Tracking, Challenge)
- [x] Timer system
  ```csharp
  - [x] Configurável por modo
  - [x] Countdown
  - [x] Event OnTimerUpdated
  ```
- [x] Game flow
  ```csharp
  - [x] StartGame(mode)
  - [x] PauseGame()
  - [x] ResumeGame()
  - [x] EndGame()
  ```
- [x] PlayerPrefs (high scores)

#### 4.2 ScoreSystem.cs (1h)
- [x] Score calculation
- [x] Accuracy tracking (hits / total shots)
- [x] Streak system
  ```csharp
  - [x] Current streak
  - [x] Max streak
  - [x] Breaks on miss
  ```
- [x] Combo multiplier (x2, x3, x4, x5)
- [x] Rank calculation (Bronze → Diamond)
- [x] Events: OnScoreChanged, OnStreakChanged

#### 4.3 AudioManager.cs (Singleton) (1h)
- [x] Singleton pattern
- [x] 4 AudioSources (music, sfx, ui, voice)
- [x] Volume controls
- [x] Public methods:
  ```csharp
  - [x] PlayShootSound(position)
  - [x] PlayHitSound(isPerfect)
  - [x] PlayMenuMusic()
  - [x] PlayGameplayMusic()
  ```
- [x] PlayerPrefs para volumes

#### 4.4 UI System (2h)
- [x] **UIManager.cs**
  - [x] World Space Canvas setup
  - [x] Panel management (Menu, HUD, Pause, GameOver)
  - [x] Event subscriptions
- [x] **HUDController.cs**
  - [x] Score display (TextMeshPro)
  - [x] Timer display (MM:SS)
  - [x] Accuracy %
  - [x] Streak counter
  - [x] Ammo display
  - [x] Rank display
- [x] **MenuController.cs**
  - [x] 5 mode selection buttons
  - [x] High score display
  - [x] Settings panel (volumes)
  - [x] Start/Quit buttons

#### 4.5 UI Prefabs & Canvas (30-45min)
- [x] World Space Canvas (2m na frente da câmera)
- [x] MainMenuPanel
- [x] GameplayHUDPanel
- [x] PauseMenuPanel
- [x] GameOverPanel
- [x] Connect UI elements no Inspector

**Deliverables:**
- ✅ GameManager funcional
- ✅ ScoreSystem calculando corretamente
- ✅ AudioManager tocando sons
- ✅ UI completa (Menu + HUD + GameOver)
- ✅ Game loop funcional

**Verification:**
- [x] Pode iniciar jogo do menu
- [x] HUD atualiza em tempo real
- [x] Score aumenta ao acertar targets
- [x] Timer conta down
- [x] Game Over mostra stats

---

## DIA 5: Modos de Jogo & Polish
**Data:** 17 Novembro
**Duração:** 5-6 horas
**Objetivo:** 5 modos funcionais, audio, VFX básicos

### ✅ Tarefas Completadas

#### 5.1 Game Modes Configuration (2h)
- [x] **Training Mode**
  ```
  - [x] Sem timer (∞)
  - [x] 3 max targets
  - [x] Spawn interval: 2s
  - [x] Apenas BasicTargets
  ```
- [x] **Precision Mode**
  ```
  - [x] Timer: 90s
  - [x] 4 max targets
  - [x] PrecisionTargets + Basic
  - [x] Hit zones ativadas
  ```
- [x] **Speed Mode**
  ```
  - [x] Timer: 60s
  - [x] 6 max targets
  - [x] Spawn interval: 0.8s
  - [x] SpeedTargets + Basic
  ```
- [x] **Tracking Mode**
  ```
  - [x] Timer: 120s
  - [x] 4 max targets
  - [x] Apenas MovingTargets
  - [x] 6 padrões de movimento
  ```
- [x] **Challenge Mode**
  ```
  - [x] Timer: 180s
  - [x] 8 max targets
  - [x] Todos os tipos
  - [x] Dificuldade progressiva (waves)
  ```

#### 5.2 Audio Integration (1.5h)
- [x] Weapon sounds
  - [x] Shoot (pulso energético)
  - [x] Reload (mecânico)
  - [x] Empty gun (beep)
- [x] Target sounds
  - [x] Hit (impact)
  - [x] Destroy (dissolve)
  - [x] Spawn (materialize)
- [x] UI sounds
  - [x] Button click
  - [x] Mode select
- [x] Music (placeholders OK)
  - [x] Menu music (ambient)
  - [x] Gameplay music (focada)

#### 5.3 Visual Effects (1h)
- [x] Hit effect (partículas simples)
- [x] Destroy effect (dissolve)
- [x] Muzzle flash (opcional)
- [x] UI animations (score pop)

#### 5.4 Utilities & Debug (1h)
- [x] **DebugHelper.cs**
  - [x] Keyboard shortcuts (T/Y/U spawn, P stats)
  - [x] FPS counter
  - [x] Toggle debug UI (F1)
- [x] **QuickStartSetup.cs** (Editor)
  - [x] Auto-find managers
  - [x] Verification checklist
  - [x] Create missing managers
- [x] **DesktopWeaponTester.cs**
  - [x] WASD movement
  - [x] Mouse look
  - [x] Click to shoot
  - [x] Permite testar sem VR

#### 5.5 Polish Pass (30min-1h)
- [x] Ajustar fire rate
- [x] Balancear spawn intervals
- [x] Ajustar pontuações
- [x] Testar todos os 5 modos
- [x] Fix bugs óbvios

**Deliverables:**
- ✅ 5 modos de jogo funcionais
- ✅ Audio completo
- ✅ VFX básicos
- ✅ Debug tools
- ✅ Desktop mode funcional

**Verification:**
- [x] Todos os modos iniciam
- [x] Audio toca em eventos corretos
- [x] Effects aparecem
- [x] Desktop mode permite testar sem VR

---

## DIA 6: Testing, Optimization & Build
**Data:** 18-19 Novembro
**Duração:** 6-8 horas
**Objetivo:** Build final, 90fps, sem bugs críticos

### 🔄 Tarefas em Progresso

#### 6.1 Performance Testing (2h)
- [ ] **Profiler Analysis**
  - [ ] CPU usage (<10ms por frame)
  - [ ] GPU usage (<10ms por frame)
  - [ ] Draw calls (<100)
  - [ ] Memory (<500MB)
- [ ] **Optimization**
  - [ ] Verificar pooling funciona (sem GC spikes)
  - [ ] Reduzir draw calls (batching)
  - [ ] Otimizar particle systems
  - [ ] LOD se necessário

#### 6.2 Quest 2 Testing (2-3h)
- [ ] Build para Android
- [ ] Deploy em Quest 2
- [ ] Test all 5 game modes
- [ ] Verificar performance
  - [ ] 90fps estável
  - [ ] No stuttering
  - [ ] No motion sickness
- [ ] Test VR interactions
  - [ ] Grab weapon
  - [ ] Shoot
  - [ ] Reload
  - [ ] UI readable

#### 6.3 Bug Fixing (2-3h)
**Known Issues to Fix:**
- [ ] Weapon positioning quando pickup
- [ ] Targets spawnam fora de vista
- [ ] Score não salva corretamente
- [ ] Audio overlap
- [ ] UI não visível em VR
- [ ] Outros bugs encontrados em testing

#### 6.4 Final Polish (1h)
- [ ] Ajustar weapon hold position
- [ ] Refinar spawn positions
- [ ] Balance final de scores
- [ ] Verificar all UI readable em VR
- [ ] Test pause menu funciona
- [ ] High scores salvam/carregam

#### 6.5 Build & Deploy (30min-1h)
- [ ] Build final Android (Quest 2)
- [ ] Build final PC VR (backup)
- [ ] Test builds em headset
- [ ] Criar README para instalação
- [ ] Package files

**Deliverables:**
- [ ] Build final testado em Quest 2
- [ ] Performance ≥90fps
- [ ] Sem bugs críticos
- [ ] README com instruções

**Success Criteria:**
- [ ] 5 modos jogáveis
- [ ] 90fps em Quest 2
- [ ] Weapon funciona perfeitamente
- [ ] UI legível e funcional
- [ ] Audio sincronizado
- [ ] High scores salvam

---

## MVP vs Nice-to-Have

### ✅ MVP Features (Must Have)
- [x] 5 modos de jogo funcionais
- [x] VR weapon com shoot e reload
- [x] 3 tipos de targets (Basic, Moving, Precision)
- [x] Object pooling
- [x] Score system com accuracy e streaks
- [x] UI completa (Menu, HUD, GameOver)
- [x] Audio básico (shoot, hit sounds)
- [x] Timer funcionando
- [x] High scores salvam (PlayerPrefs)
- [ ] 90fps em Quest 2
- [ ] Desktop mode para testing

### ⏳ Nice-to-Have (Se tempo permitir)
- ⏳ Muzzle flash avançado
- ⏳ Bullet tracers
- ⏳ Particle effects elaborados
- ⏳ ARIA voice lines
- ⏳ Music tracks customizadas
- ⏳ Achievements system
- ⏳ Multiple armas
- ⏳ Multiple ambientes
- ⏳ Recoil animation
- ⏳ Advanced haptics

### ❌ Post-MVP (Futuro)
- Multiplayer
- Leaderboards online
- Campaign mode
- More weapons/targets
- Advanced environments
- Replay system
- Analytics dashboard

---

## Risk Management

### High Risk
**Issue:** Performance <90fps em Quest 2
**Mitigation:**
- Profiler diário
- Object pooling desde Dia 3
- Simplificar VFX se necessário
- Reduzir max active targets

**Issue:** XR setup complexo
**Mitigation:**
- Dia 1 inteiro para setup
- XR Device Simulator para test sem headset
- Desktop mode backup

**Issue:** Bugs de última hora
**Mitigation:**
- Daily testing
- Debug tools (DebugHelper)
- Reserve Dia 6 inteiro para bugs

### Medium Risk
**Issue:** Audio assets não prontos
**Mitigation:**
- Placeholders OK para MVP
- Freesound.org para SFX
- Synthwave loops para música

**Issue:** UI não legível em VR
**Mitigation:**
- World Space desde início
- Test em VR frequentemente
- Aumentar font size se necessário

### Low Risk
**Issue:** Balanceamento de scores
**Mitigation:**
- Valores configuráveis
- Easy to tweak
- Playtest Dia 5-6

---

## Daily Standup Template

### Morning
**Questions:**
1. O que vou fazer hoje?
2. Quais as 3 top tasks?
3. Qual o output esperado?

### Evening
**Questions:**
1. O que completei?
2. O que bloqueou?
3. O que ficou para amanhã?

**Example Log - Dia 4:**
```
Morning:
- Implementar GameManager
- Criar ScoreSystem
- Build UI básica
Output: Game loop funcional

Evening:
✅ GameManager done
✅ ScoreSystem calculando
✅ UI HUD mostrando score
⚠️ Timer bug (fixed)
Tomorrow: Finalizar UI panels
```

---

## Metrics & Success

### Daily Metrics
- [ ] **Dia 1:** XR funciona? ✅
- [ ] **Dia 2:** Weapon dispara? ✅
- [ ] **Dia 3:** Targets spawnam? ✅
- [ ] **Dia 4:** Game loop funcional? ✅
- [ ] **Dia 5:** 5 modos jogáveis? ✅
- [ ] **Dia 6:** 90fps + no bugs? 🔄

### Final Success Criteria
- [ ] ✅ Jogável do início ao fim
- [ ] ✅ Todos os 5 modos funcionam
- [ ] 🔄 Performance target hit (90fps)
- [ ] ✅ UI completa e funcional
- [ ] ✅ Audio integrado
- [ ] 🔄 Build em Quest 2
- [ ] ✅ High scores salvam
- [ ] ✅ Desktop mode funcional

**Current Status:** 90% completo (MVP features done, testing em progresso)

---

## Troubleshooting Guide

### Common Issues

**"XR não funciona"**
→ XR Plugin Management → OpenXR enabled?
→ XR Interaction Manager na cena?

**"Weapon não grab"**
→ XRGrabInteractable component presente?
→ Collider enabled?
→ Layer Mask correto?

**"Targets não aparecem"**
→ ObjectPooler configurado?
→ TargetSpawner StartSpawning chamado?
→ Prefabs assignados?

**"UI não visível em VR"**
→ Canvas em World Space?
→ Canvas distance = 2m?
→ EventSystem presente?

**"Performance baixo"**
→ Profiler → CPU/GPU spike?
→ Draw calls alto?
→ Garbage collection?
→ Too many active targets?

**"Build falha"**
→ Android SDK instalado?
→ Build Settings → Platform = Android?
→ Player Settings → Min API Level 29+?

---

## Próximos Passos (Post-Sprint)

### Immediate (Semana 1-2)
1. User testing com 5-10 pessoas
2. Coletar feedback estruturado
3. Fix bugs reportados
4. Iterar em balanceamento

### Short-term (Mês 1)
1. Adicionar ARIA voice lines
2. Implementar achievements
3. Melhorar VFX
4. Adicionar mais armas (Rifle, SMG)

### Medium-term (Mês 2-3)
1. Novos ambientes (Cyber Range, etc.)
2. Campaign mode básico
3. Steam/Oculus Store submission
4. Marketing & screenshots

### Long-term (Mês 4+)
1. Multiplayer co-op
2. Leaderboards online
3. Mobile VR ports (Pico, etc.)
4. Content updates regulares

---

**Plano criado por:** Development Team
**Data:** 13 Novembro 2025
**Status:** 🔄 Dia 6 em progresso (90% completo)
**Confiança no Deadline:** ✅ Alta (MVP features done)

*"Precision isn't about being perfect. It's about being consistent, adaptive, and never giving up."* - Comandante Cross
