# Software Design Document (SDD)
## VR Aim Lab - APEX Training System

**Version:** 1.0.0
**Date:** November 2025
**Platform:** Unity 2021.3 LTS
**Target:** Meta Quest 2, PC VR (SteamVR, Oculus Rift)

---

## 1. Visão Geral

### 1.1 Conceito
O **APEX Training System** é um simulador VR de treino de mira desenvolvido para a Academia de Precisão Neural (2045). O sistema utiliza tecnologia de realidade virtual para proporcionar uma experiência imersiva de treino de tiro, com 5 modos de jogo distintos e um sistema de progressão baseado em performance.

### 1.2 Objetivo
Criar um ambiente de treino virtual que:
- Melhore a precisão e velocidade de reação dos jogadores
- Forneça feedback em tempo real sobre performance
- Ofereça progressão clara através de ranks (Bronze → Diamond)
- Mantenha 90fps estáveis em VR para conforto máximo
- Seja acessível tanto em VR headsets standalone quanto PC VR

### 1.3 Pilares de Design
1. **Precisão Acima de Tudo** - Sistema de hit detection preciso com zonas (bullseye, inner, outer)
2. **Feedback Imediato** - Visual, áudio e haptic feedback a cada ação
3. **Progressão Clara** - Sistema de ranks e achievements visíveis
4. **Performance** - 90fps mínimo em Quest 2
5. **Acessibilidade VR** - UI em World Space, controles intuitivos

---

## 2. Arquitetura do Sistema

### 2.1 Diagrama de Componentes

```
┌─────────────────────────────────────────────────────────┐
│                    APEX TRAINING SYSTEM                  │
├─────────────────────────────────────────────────────────┤
│                                                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ GameManager  │  │ AudioManager │  │ ScoreSystem  │  │
│  │  (Singleton) │  │  (Singleton) │  │  (Core)      │  │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘  │
│         │                  │                  │          │
│         └──────────────────┴──────────────────┘          │
│                            │                             │
│         ┌──────────────────┴──────────────────┐          │
│         │                                      │          │
│  ┌──────▼────────┐              ┌─────────▼──────────┐  │
│  │  UIManager    │              │  TargetSpawner    │  │
│  │  - HUD        │              │  - ObjectPooler   │  │
│  │  - Menus      │              │  - Wave System    │  │
│  └───────────────┘              └───────────────────┘  │
│                                                         │
│  ┌──────────────────┐           ┌─────────────────────┐│
│  │ VRWeaponController│          │  Target Classes     ││
│  │  - Raycast       │           │  - Target.cs        ││
│  │  - Haptics       │           │  - MovingTarget.cs  ││
│  └──────────────────┘           └─────────────────────┘│
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### 2.2 Managers (Singleton Pattern)

#### GameManager.cs
**Responsabilidades:**
- Controle de fluxo de jogo (Menu → Playing → GameOver)
- Gerenciamento de 5 modos de jogo
- Timer configurável por modo
- Sistema de save/load (PlayerPrefs)
- Eventos globais (OnScoreChanged, OnGameStateChanged, OnTimerUpdated)

**Estados (GameState enum):**
- `Menu` - Menu principal, seleção de modo
- `Playing` - Jogo ativo
- `Paused` - Jogo pausado
- `GameOver` - Fim de jogo, exibindo stats

**Modos de Jogo (GameMode enum):**
- `Training` - Sem limite de tempo, foco em precisão
- `Precision` - Alvos pequenos, alta pontuação por bullseye
- `Speed` - 60 segundos, máximo de alvos
- `Tracking` - Alvos em movimento
- `Challenge` - Mix de todos os tipos, dificuldade progressiva

#### AudioManager.cs
**Responsabilidades:**
- 4 AudioSources separados (music, sfx, ui, voice)
- Spatial 3D audio para sons de disparo e hits
- Controles de volume (master, music, sfx, ui, voice)
- Music crossfade e fade in/out
- PlayerPrefs para persistir configurações

**Métodos Públicos:**
```csharp
PlayShootSound(Vector3 position)
PlayHitSound(bool isPerfect)
PlayComboSound()
PlayMenuMusic()
PlayGameplayMusic()
```

#### ScoreSystem.cs
**Responsabilidades:**
- Cálculo de pontuação com multiplicadores
- Sistema de streak (hits consecutivos)
- Sistema de combo (multiplicadores x2, x3, x4, x5)
- Cálculo de accuracy (hits / total shots)
- Sistema de ranks baseado em score

**Ranks:**
- Bronze: 0-999 pontos
- Silver: 1000-2999 pontos
- Gold: 3000-5999 pontos
- Platinum: 6000-9999 pontos
- Diamond: 10000+ pontos

### 2.3 Gameplay Loop

```
1. Player seleciona modo (MenuController)
        ↓
2. GameManager.StartGame(mode)
        ↓
3. UIManager mostra HUD
        ↓
4. TargetSpawner inicia spawn
        ↓
5. Player atira → VRWeaponController.TryShoot()
        ↓
6. Raycast hit → Target.OnHit()
        ↓
7. GameManager.OnTargetHit(points, isPerfect)
        ↓
8. ScoreSystem atualiza (score, streak, combo)
        ↓
9. UIManager atualiza display
        ↓
10. Loop até timer = 0 ou player pausa
        ↓
11. GameManager.EndGame()
        ↓
12. UIManager mostra GameOver com stats
```

---

## 3. Modos de Jogo Detalhados

### 3.1 Training Mode
**Objetivo:** Prática livre sem pressão de tempo
**Características:**
- Sem limite de tempo (∞ no timer)
- 3 alvos simultâneos máximo
- Spawn interval: 2 segundos
- Target lifetime: 10 segundos
- Tipos de alvo: Apenas Basic
- Ideal para: Warm-up, ajustar sensibilidade

**Configuração:**
```csharp
spawnInterval = 2f
maxActiveTargets = 3
allowedTargetTypes = [Basic]
targetLifetime = 10f
```

### 3.2 Precision Mode
**Objetivo:** Testar precisão com alvos menores
**Características:**
- Tempo: 90 segundos
- Alvos menores (50% do tamanho normal)
- Hit zones ativadas (bullseye, inner, outer)
- Bullseye = 2x pontos
- 4 alvos simultâneos
- Tipos: Precision + Basic

**Pontuação:**
- Outer zone: 100 pontos (base)
- Inner zone: 150 pontos (1.5x)
- Bullseye: 200 pontos (2x) + perfect hit bonus

### 3.3 Speed Mode
**Objetivo:** Máximo de alvos em 60 segundos
**Características:**
- Tempo: 60 segundos
- Spawn rápido: 0.8 segundos
- 6 alvos simultâneos
- Target lifetime curto: 3 segundos
- Tipos: Speed + Basic
- Combo multipliers são cruciais

**Estratégia:**
- Foco em velocidade, não em precisão
- Manter streak para multiplicadores
- Ignorar alvos distantes

### 3.4 Tracking Mode
**Objetivo:** Treino de tracking com alvos em movimento
**Características:**
- Tempo: 120 segundos
- Apenas MovingTargets
- 4 alvos simultâneos
- 6 padrões de movimento (Linear, Circular, Zigzag, Random, Figure8, Bounce)
- Target lifetime: 8 segundos
- Spawn interval: 1.5 segundos

**Padrões de Movimento:**
```csharp
Linear    - Movimento em linha reta
Circular  - Círculo ao redor de um ponto
Zigzag    - Padrão em zigue-zague
Random    - Mudanças aleatórias de direção
Figure8   - Padrão em forma de 8
Bounce    - Saltar entre pontos predefinidos
```

### 3.5 Challenge Mode
**Objetivo:** Teste definitivo com dificuldade progressiva
**Características:**
- Tempo: 180 segundos (3 minutos)
- Todos os tipos de alvo
- 8 alvos simultâneos (máximo)
- Dificuldade aumenta a cada 30 segundos
- Wave system ativo
- Progressão: spawn interval diminui, max targets aumenta

**Sistema de Waves:**
```csharp
Wave 1 (0-30s):   interval=1.0s, maxTargets=4
Wave 2 (30-60s):  interval=0.8s, maxTargets=5
Wave 3 (60-90s):  interval=0.6s, maxTargets=6
Wave 4 (90-120s): interval=0.5s, maxTargets=7
Wave 5 (120-180s): interval=0.4s, maxTargets=8
```

---

## 4. Sistema de Progressão

### 4.1 Ranks
**Critério:** Baseado em pontuação final

| Rank     | Score Mínimo | Cor       | Accuracy Mínima |
|----------|--------------|-----------|-----------------|
| Bronze   | 0            | #CD7F32   | 0%              |
| Silver   | 1000         | #C0C0C0   | 50%             |
| Gold     | 3000         | #FFD700   | 65%             |
| Platinum | 6000         | #E5E4E2   | 80%             |
| Diamond  | 10000        | #B9F2FF   | 90%             |

### 4.2 Streak System
**Mecânica:**
- Cada hit consecutivo aumenta streak
- Miss ou target expired quebra streak
- Milestones: 10, 20, 30, 50, 100 hits
- Banner visual aparece em milestones

**Bonuses:**
```
Streak 10+:  +50 pontos bonus
Streak 20+:  +100 pontos bonus
Streak 50+:  +500 pontos bonus
Streak 100+: +1000 pontos bonus
```

### 4.3 Combo Multiplier
**Mecânica:**
- Hits rápidos (< 1 segundo entre hits) aumentam combo
- Combo multiplica score do próximo hit
- Máximo: x5

**Tabela de Multiplicadores:**
```
2 hits rápidos:  x2 (Combo duplo)
3 hits rápidos:  x3 (Combo triplo)
4 hits rápidos:  x4 (Combo quádruplo)
5+ hits rápidos: x5 (MEGA COMBO)
```

### 4.4 Achievements (Futuro)
- **Primeiro Passo** - Completar Training Mode
- **Recruta APEX** - Atingir Silver rank
- **Olho de Águia** - 95% accuracy em Precision Mode
- **Reflexos Relâmpago** - 100+ alvos em Speed Mode
- **Mestre do Tracking** - 90% accuracy em Tracking Mode
- **Lenda APEX** - Atingir Diamond rank em Challenge Mode

---

## 5. Sistemas Técnicos

### 5.1 VR Weapon System

#### VRWeaponController.cs
**Componentes Requeridos:**
- XRGrabInteractable (Unity XR Toolkit)
- LineRenderer (laser sight)
- Collider (para grabbing)
- Rigidbody (física VR)

**Features:**
- **Raycast Shooting:** Physics.Raycast do shootPoint
- **Fire Rate:** Cooldown entre disparos (0.2s default)
- **Ammo System:** Munição limitada (30 default) com reload
- **Reload:** Grip button (ou R no desktop mode)
- **Haptic Feedback:** XRBaseController.SendHapticImpulse
- **Laser Sight:** LineRenderer de shootPoint ao hit point
- **Audio:** Integração com AudioManager (shoot, reload, empty)
- **Recoil:** Visual feedback com rotação do weapon

**Desktop Mode Support:**
- Atira da posição da câmera (Camera.main)
- Direção = Camera.main.transform.forward
- Permite testar sem VR headset
- DesktopWeaponTester.cs para controle WASD + Mouse

### 5.2 Target System

#### Target.cs (Base Class)
**Características:**
- Hit detection via collider
- Lifetime timer (auto-destroy após X segundos)
- Hit zones (bullseye, inner, outer)
- Visual feedback (cor muda ao hit)
- Spawn/destroy particle effects
- Events: OnTargetHit, OnTargetExpired

**Target Types:**
```csharp
Basic      - Alvo padrão, estático
Speed      - Spawn/despawn rápido
Precision  - Menor tamanho, mais pontos
Moving     - Em movimento (MovingTarget.cs)
Combo      - Requer múltiplos hits
```

#### MovingTarget.cs (Extends Target)
**6 Padrões de Movimento:**

1. **Linear:** Movimento em linha reta
2. **Circular:** Rotação ao redor de um ponto central
3. **Zigzag:** Movimento em ziguezague
4. **Random:** Mudanças aleatórias de direção dentro de bounds
5. **Figure8:** Padrão em forma de 8
6. **Bounce:** Bounce entre pontos predefinidos

**Configuração:**
```csharp
moveSpeed = 2f           // Velocidade de movimento
patternScale = 3f        // Escala do padrão
randomizePattern = false // Padrão aleatório ao spawn
```

### 5.3 Spawn System

#### TargetSpawner.cs
**Features:**
- **Object Pooling:** Pool de 20 targets pré-instanciados
- **Wave System:** Dificuldade progressiva
- **Spawn Positions:** Aleatórias ou fixed spawn points
- **Spawn Control:** Start/Stop/Pause/Resume
- **Configuração por Modo:** SpawnConfig para cada GameMode

**Object Pooling Benefits:**
- Elimina garbage collection spikes
- Mantém performance constante
- Reusa targets em vez de Instantiate/Destroy

**Difficulty Progression:**
```csharp
IncreaseDifficulty(level):
  - Diminui spawn interval
  - Aumenta max targets simultâneos
  - Mantém performance em 90fps
```

### 5.4 UI System

#### UIManager.cs
**World Space Canvas para VR:**
- RenderMode: WorldSpace
- Posicionado 2m na frente do player
- Sempre olha para câmera (LookAt)
- Scale: 0.001 (ajustado para VR)

**Painéis:**
- **MainMenu:** Seleção de modo, settings, high scores
- **GameplayHUD:** Score, timer, accuracy, streak, ammo, rank
- **PauseMenu:** Resume, settings, return to menu
- **GameOver:** Stats finais, rank atingido, novo high score

#### HUDController.cs
**Display Elements:**
- Score (animado, count-up effect)
- Timer (MM:SS format, color-coded)
- Accuracy (%, color-coded)
- Streak (com banner em milestones)
- Combo (x2, x3, x4, x5 com cores)
- Ammo (current/max)
- Rank (Bronze, Silver, Gold, Platinum, Diamond)

**Color Coding:**
```
Accuracy:
  ≥90% = Green
  ≥70% = Yellow
  ≥50% = Orange
  <50% = Red

Timer:
  >30s = White
  ≤30s = Yellow
  ≤10s = Red (warning)
```

---

## 6. Performance & Optimization

### 6.1 Target: 90fps VR

**Estratégias:**
1. **Object Pooling:** Elimina Instantiate/Destroy
2. **Occlusion Culling:** Desativa rendering de objetos não visíveis
3. **LOD System:** (Futuro) Simplifica meshes distantes
4. **Batching:** Combina draw calls de materiais iguais
5. **MaterialPropertyBlock:** Evita material instances

### 6.2 Profiling Checklist
- CPU: ≤10ms por frame (90fps = 11.1ms)
- GPU: ≤10ms por frame
- Draw Calls: ≤100
- SetPass Calls: ≤50
- Triangles: ≤100k total
- Memory: ≤500MB

### 6.3 Unity Optimization Settings
```
Quality Settings:
  - Texture Quality: Medium
  - Shadow Resolution: Low
  - Anti-Aliasing: MSAA x2
  - VSync: Off (VR runtime controla)

Player Settings:
  - Color Space: Linear
  - Graphics API: Vulkan (Quest 2) / DX11 (PC)
  - Stereo Rendering Mode: Multi-view
  - Target Frame Rate: 90fps
```

### 6.4 XR Optimization
- **Fixed Foveated Rendering:** Ativo no Quest 2
- **Dynamic Resolution:** Ajusta resolução para manter 90fps
- **Oculus Dash Compatibility:** Pause game quando dashboard aberto
- **Hand Tracking:** Desativado (não usado, economiza performance)

---

## 7. Tech Stack

### 7.1 Unity Packages
- **Unity 2021.3 LTS** (Long Term Support)
- **XR Interaction Toolkit** (v2.3+) - VR interactions
- **Universal Render Pipeline (URP)** - Render moderno e eficiente
- **TextMeshPro** - UI text de alta qualidade
- **XR Plugin Management** - Multi-platform VR

### 7.2 Platform Support
**Primary:**
- Meta Quest 2 (Android, Snapdragon XR2)
- Meta Quest 3 (futuro)

**Secondary:**
- PC VR (SteamVR)
- Oculus Rift S
- Valve Index

### 7.3 Input System
- **XR Interaction Toolkit:** Trigger para shoot, Grip para reload
- **Legacy Input (fallback):** Desktop mode testing
- **Input System Package:** Keyboard shortcuts (DebugHelper.cs)

### 7.4 Audio System
- **Unity Audio Mixer:** Separação music/sfx/ui/voice
- **Spatial Audio:** 3D sound com rolloff linear
- **Audio Sources:** 4 separados (evita conflicts)

---

## 8. Requisitos do Sistema

### 8.1 Hardware - Quest 2
- **Headset:** Meta Quest 2 (ou superior)
- **Storage:** 500MB mínimo
- **RAM:** 6GB (Quest 2 tem 6GB)
- **Refresh Rate:** 72Hz, 90Hz, 120Hz (target 90Hz)

### 8.2 Hardware - PC VR
- **CPU:** Intel i5-9600K ou AMD Ryzen 5 3600
- **GPU:** NVIDIA GTX 1660 ou AMD RX 5500 XT
- **RAM:** 8GB mínimo, 16GB recomendado
- **Storage:** 1GB
- **VR Headset:** Oculus Rift S, HTC Vive, Valve Index, Quest 2 (Link)

### 8.3 Software
- **Unity:** 2021.3 LTS ou superior
- **Android Build Support:** Para Quest 2
- **XR Plugin Management:** OpenXR ou Oculus
- **Build Tools:** Android SDK, NDK

---

## 9. Estrutura de Ficheiros

```
Assets/
├── Scenes/
│   ├── MainMenu.unity
│   └── TrainingRange.unity
│
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs
│   │   ├── AudioManager.cs
│   │   └── ScoreSystem.cs
│   │
│   ├── Gameplay/
│   │   ├── Target.cs
│   │   ├── MovingTarget.cs
│   │   ├── TargetSpawner.cs
│   │   ├── VRWeaponController.cs
│   │   └── ObjectPooler.cs
│   │
│   ├── UI/
│   │   ├── UIManager.cs
│   │   ├── MenuController.cs
│   │   └── HUDController.cs
│   │
│   ├── Utilities/
│   │   ├── QuickStartSetup.cs
│   │   └── DebugHelper.cs
│   │
│   └── Editor/
│       ├── WeaponSetup.cs
│       └── XRSetupDiagnostics.cs
│
├── Prefabs/
│   ├── Weapons/
│   │   └── VR_Pistol.prefab
│   │
│   ├── Targets/
│   │   ├── BasicTarget.prefab
│   │   ├── MovingTarget.prefab
│   │   └── PrecisionTarget.prefab
│   │
│   └── Managers/
│       ├── GameManager.prefab
│       ├── AudioManager.prefab
│       └── UIManager.prefab
│
├── Materials/
│   ├── TargetMaterial.mat
│   └── WeaponMaterial.mat
│
└── Audio/
    ├── Music/
    ├── SFX/
    └── UI/
```

---

## 10. Código - Best Practices

### 10.1 C# Style Guide
```csharp
// ✅ BOM
public class ExampleClass : MonoBehaviour
{
    #region Inspector Fields
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    #endregion

    #region Private Fields
    private bool isActive = true;
    #endregion

    #region Unity Lifecycle
    private void Awake() { }
    private void Start() { }
    private void Update() { }
    #endregion

    #region Public Methods
    /// <summary>
    /// Descrição do método
    /// </summary>
    public void DoSomething() { }
    #endregion
}
```

### 10.2 Performance Guidelines
- **Avoid GetComponent in Update:** Cache referências
- **Use Object Pooling:** Para objetos frequentemente criados/destruídos
- **Minimize Garbage:** Reuse containers, avoid string concatenation
- **Coroutines vs Update:** Use coroutines para timers
- **Events vs SendMessage:** Use events/delegates

### 10.3 VR Specific
- **Fixed Update for Physics:** Rigidbody movement
- **Haptics Responsivo:** Intensidade baseada em ação
- **UI World Space:** Sempre, nunca Screen Space
- **Comfort:** Evitar movimentos bruscos de câmera

---

## 11. Testing Checklist

### 11.1 Functional Tests
- [ ] Todos os 5 modos iniciam corretamente
- [ ] Weapon atira e registra hits
- [ ] Targets spawnam e destroem corretamente
- [ ] Score system calcula corretamente
- [ ] Timer funciona em todos os modos
- [ ] UI atualiza em tempo real
- [ ] Audio toca nos eventos corretos
- [ ] High scores salvam/carregam

### 11.2 VR Tests
- [ ] Grab weapon funciona (trigger + grip)
- [ ] Haptic feedback funciona
- [ ] Laser sight aponta corretamente
- [ ] UI legível em VR (World Space)
- [ ] Performance ≥90fps
- [ ] Sem motion sickness
- [ ] Conforto após 15min

### 11.3 Performance Tests
- [ ] FPS ≥90 em Quest 2
- [ ] Memory usage ≤500MB
- [ ] Draw calls ≤100
- [ ] No GC spikes durante gameplay
- [ ] Load time ≤5 segundos

---

## 12. Known Limitations & Future Work

### 12.1 Limitações Atuais
- Apenas 1 arma disponível (pistola)
- Sem multiplayer
- Sem sistema de achievements persistente
- Sem leaderboards online
- Ambientes limitados (1 training hall)

### 12.2 Future Features (Post-MVP)
- **Armas Adicionais:** Rifle de precisão, SMG rápida
- **Ambientes:** Cyber Range, Kinetic Arena, Apex Nexus
- **Multiplayer:** Co-op e competitivo
- **Achievements System:** Com unlock de skins
- **Leaderboards:** Online com Steam/Oculus integration
- **Replay System:** Gravar e rever sessões
- **Voice AI (ARIA):** Tutora virtual com voice lines
- **Analytics:** Tracking de progressão detalhada

---

## 13. Troubleshooting

### 13.1 Common Issues

**Issue:** Weapon não atira
**Fix:** Verificar se shootPoint está assignado no Inspector

**Issue:** Targets não spawnam
**Fix:** Verificar ObjectPooler configurado com pools "BasicTarget", "MovingTarget", "PrecisionTarget"

**Issue:** UI não aparece em VR
**Fix:** Canvas deve estar em World Space, não Screen Space

**Issue:** Performance baixo (<90fps)
**Fix:** Ativar Fixed Foveated Rendering, reduzir max active targets

**Issue:** Audio não toca
**Fix:** Verificar AudioManager existe na cena, audio clips assignados

### 13.2 Debug Tools
- **DebugHelper.cs:** Keyboard shortcuts (T, Y, U para spawn, P para stats)
- **QuickStartSetup.cs:** Verifica setup VR, managers presentes
- **Unity Profiler:** CPU/GPU/Memory monitoring
- **XR Plugin Management:** Verifica XR providers ativos

---

## 14. Conclusão

O **APEX Training System** representa um simulador VR de treino de mira completo e polished, projetado para proporcionar uma experiência de alta qualidade tanto em standalone VR (Quest 2) quanto em PC VR.

**Objetivos Atingidos:**
✅ 5 modos de jogo distintos e balanceados
✅ Sistema de progressão com 5 ranks
✅ Performance otimizada (90fps target)
✅ VR-first design com full XR Toolkit integration
✅ UI completa (Menu, HUD, Pause, GameOver)
✅ Audio spatial 3D com AudioManager
✅ Object pooling para targets
✅ Desktop mode para testing sem VR

**MVP Completo:** Ready for deployment e testing em Quest 2.

**Next Steps:**
1. Build e deploy em Quest 2
2. User testing e feedback
3. Iteração baseada em dados
4. Expansão com features pós-MVP

---

**Desenvolvido por:** [Your Name]
**Deadline:** 19 Novembro 2025
**Status:** ✅ Completo
