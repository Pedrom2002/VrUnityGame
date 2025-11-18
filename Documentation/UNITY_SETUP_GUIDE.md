# Unity Setup Guide - VR Aim Lab APEX
## Guia Passo-a-Passo de 60 Minutos

**Objetivo:** Ir de projeto vazio até primeiro teste VR funcional em 60 minutos
**Skill Level:** Intermediário (conhecimento básico de Unity)
**Unity Version:** 2021.3 LTS ou superior

---

## Índice
1. [Pré-Requisitos](#1-pré-requisitos) (5min)
2. [Criar Projeto Unity](#2-criar-projeto-unity) (5min)
3. [Importar Packages](#3-importar-packages) (10min)
4. [Configurar XR](#4-configurar-xr) (10min)
5. [Importar Scripts](#5-importar-scripts) (5min)
6. [Criar Prefabs](#6-criar-prefabs) (15min)
7. [Setup de Cena](#7-setup-de-cena) (10min)
8. [Primeiro Teste](#8-primeiro-teste) (5min)

**Tempo Total:** ~60 minutos

---

## 1. Pré-Requisitos
**Duração:** 5 minutos

### 1.1 Software Necessário
- [ ] **Unity Hub** instalado
- [ ] **Unity 2021.3 LTS** instalado via Hub
- [ ] **Android Build Support** (para Quest 2)
  - Android SDK & NDK Tools
  - OpenJDK
- [ ] **Visual Studio** ou **VS Code** (IDE)

### 1.2 Hardware
**Para PC VR:**
- [ ] Headset VR (Oculus Rift, Vive, Index, Quest 2 com Link)
- [ ] Controllers VR

**Para Quest 2 (Standalone):**
- [ ] Meta Quest 2 ou 3
- [ ] Cabo USB-C (para build/debug)
- [ ] Meta Quest Developer Account (free)

### 1.3 Verificação Rápida
```bash
# Verificar Unity instalado
unity-hub --version

# Verificar Android SDK (se Quest 2)
adb version
```

**✅ Checkpoint:** Unity Hub aberto, 2021.3 LTS listado

---

## 2. Criar Projeto Unity
**Duração:** 5 minutos

### 2.1 Criar Novo Projeto
1. Abrir **Unity Hub**
2. Click **New Project**
3. Selecionar template: **3D (URP)** ⚠️ Importante: Universal Render Pipeline
4. Nome do projeto: `VR_AimLab_APEX`
5. Localização: Escolher pasta
6. Click **Create Project**

**⏱️ Aguardar:** Unity abre (1-2min)

### 2.2 Verificar Project Settings
1. `Edit → Project Settings → Player`
2. **Color Space:** Linear (padrão URP)
3. **Graphics API:**
   - Windows: Direct3D11
   - Android: Vulkan
4. Fechar Project Settings

**✅ Checkpoint:** Projeto criado, Unity aberto com cena vazia

---

## 3. Importar Packages
**Duração:** 10 minutos

### 3.1 Package Manager
1. `Window → Package Manager`
2. Filtro: **Unity Registry**

### 3.2 Instalar Packages Essenciais

#### XR Plugin Management
1. Procurar: `XR Plugin Management`
2. Click **Install**
3. ⏱️ Aguardar instalação (1min)

#### XR Interaction Toolkit
1. Procurar: `XR Interaction Toolkit`
2. Click **Install**
3. ⚠️ Popup aparece: "New Input System"
   - Click **Yes** (vai reiniciar Unity)
4. ⏱️ Aguardar reinício (1-2min)

#### TextMeshPro
1. Procurar: `TextMeshPro`
2. Se não estiver instalado, click **Install**
3. Popup: "TMP Essentials"
   - Click **Import TMP Essentials**
   - Fechar janela

#### Input System (Novo)
1. Procurar: `Input System`
2. Verificar se instalado (deve estar após XR Toolkit)
3. Se não, click **Install**

### 3.3 Verificação de Packages
`Window → Package Manager → In Project`

**Lista deve incluir:**
- [x] Universal RP
- [x] XR Plugin Management
- [x] XR Interaction Toolkit
- [x] Input System
- [x] TextMeshPro

**✅ Checkpoint:** Todos os packages instalados

---

## 4. Configurar XR
**Duração:** 10 minutos

### 4.1 XR Plugin Management Settings
1. `Edit → Project Settings → XR Plugin Management`
2. Tab **PC, Mac & Linux Standalone**
   - [x] Check **OpenXR**
   - [x] Check **Windows Mixed Reality** (opcional)
3. Tab **Android** (se Quest 2)
   - [x] Check **Oculus**
   - **⚠️ Importante:** Clicar no ⚙️ ao lado de Oculus
     - **Stereo Rendering Mode:** Multiview
     - **Low Overhead Mode:** Enabled

### 4.2 OpenXR Settings (Se PC VR)
1. Ainda em XR Plugin Management
2. Expandir **OpenXR**
3. Click **+** em Interaction Profiles
4. Adicionar:
   - Oculus Touch Controller Profile
   - HTC Vive Controller Profile (se Vive)
   - Valve Index Controller Profile (se Index)

### 4.3 Input System Configuration
1. `Edit → Project Settings → Player`
2. Scroll até **Active Input Handling**
3. Selecionar: **Both** (Legacy + New)
4. Popup: "API Update Required"
   - Click **Yes** (reinicia Unity)
5. ⏱️ Aguardar reinício (1min)

### 4.4 Android Build Settings (Se Quest 2)
1. `File → Build Settings`
2. Platform: **Android**
3. Click **Switch Platform** (⏱️ 2-3min)
4. `Edit → Project Settings → Player → Android tab`
   - **Minimum API Level:** Android 10.0 (API 29)
   - **Target API Level:** Automatic (highest installed)
   - **Install Location:** Automatic
5. **Package Name:** `com.yourname.vraimlab`
   - ⚠️ Mudar "yourname" para algo único

**✅ Checkpoint:** XR configurado, Input System ativo

---

## 5. Importar Scripts
**Duração:** 5 minutos

### 5.1 Criar Folder Structure
Hierarchy em Project:
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
│   ├── Weapons/
│   ├── Targets/
│   └── Managers/
├── Materials/
└── Audio/
    ├── Music/
    ├── SFX/
    └── UI/
```

**Como criar:**
1. Project window → Right click → Create → Folder
2. Criar estrutura acima

### 5.2 Importar Scripts
**Opção A: Se já tem os scripts**
1. Copiar todos os .cs files para `Assets/Scripts/`
2. Respeitar subfolder structure (Core/, Gameplay/, etc.)
3. Unity vai compilar automaticamente
4. ⏱️ Aguardar compilation (30s-1min)
5. Verificar Console: sem erros

**Opção B: Scripts já estão no projeto**
1. Verificar `Assets/Scripts/` populated
2. Check Console para erros
3. Se erros, resolver (geralmente missing references)

### 5.3 Verificação de Scripts
Verificar que existem:
- [x] `Core/GameManager.cs`
- [x] `Core/AudioManager.cs`
- [x] `Core/ScoreSystem.cs`
- [x] `Gameplay/VRWeaponController.cs`
- [x] `Gameplay/Target.cs`
- [x] `Gameplay/MovingTarget.cs`
- [x] `Gameplay/TargetSpawner.cs`
- [x] `Gameplay/ObjectPooler.cs`
- [x] `UI/UIManager.cs`
- [x] `UI/HUDController.cs`
- [x] `UI/MenuController.cs`
- [x] `Utilities/DebugHelper.cs`
- [x] `Utilities/QuickStartSetup.cs`

**✅ Checkpoint:** Scripts importados sem erros de compilação

---

## 6. Criar Prefabs
**Duração:** 15 minutos

### 6.1 XR Origin Setup
1. Hierarchy → Right click → XR → XR Origin (Action-based)
2. Hierarchy mostra:
   ```
   XR Origin
   ├── Camera Offset
   │   └── Main Camera
   ├── Left Controller
   └── Right Controller
   ```
3. Verificar Main Camera:
   - Tag: **MainCamera**
   - Clear Flags: **Skybox**

**⚠️ Importante:** Deletar "Main Camera" original se existir

### 6.2 XR Interaction Manager
1. Hierarchy → Right click → Create Empty
2. Nome: `XR Interaction Manager`
3. Inspector → Add Component → `XR Interaction Manager`
4. Deixar settings padrão

### 6.3 Basic Target Prefab
1. Hierarchy → 3D Object → **Sphere**
2. Nome: `BasicTarget`
3. Inspector:
   - **Scale:** (0.5, 0.5, 0.5)
   - **Add Component** → `Target` (script)
   - **Add Component** → Sphere Collider (já tem)
4. Create Material:
   - `Assets/Materials` → Create → Material
   - Nome: `TargetMaterial`
   - Albedo Color: Vermelho (#FF4500)
5. Aplicar material ao Sphere
6. Configurar Target script:
   - Target Type: Basic
   - Base Points: 100
   - Lifetime: 10
   - Show Laser Sight: ✓
7. Drag BasicTarget para `Assets/Prefabs/Targets/`
8. Delete BasicTarget da Hierarchy

**✅ Resultado:** BasicTarget.prefab criado

### 6.4 Moving Target Prefab
1. Duplicate `BasicTarget.prefab`
2. Nome: `MovingTarget`
3. Inspector:
   - Remove `Target` component
   - Add `MovingTarget` component
4. Create Material verde (#00FF00)
5. Configure MovingTarget:
   - Movement Pattern: Circular (ou outro)
   - Move Speed: 2
6. Save prefab

### 6.5 Precision Target Prefab
1. Duplicate `BasicTarget.prefab`
2. Nome: `PrecisionTarget`
3. Inspector:
   - Scale: (0.3, 0.3, 0.3) - **menor**
   - Use Hit Zones: ✓
   - Bullseye Radius: 0.05
   - Inner Radius: 0.1
4. Create Material azul (#0080FF)
5. Save prefab

### 6.6 VR Pistol Prefab
1. Hierarchy → Create Empty
2. Nome: `VR_Pistol`
3. Add children (visual básico):
   - 3D Object → Cube (Grip)
     - Scale: (0.05, 0.15, 0.1)
     - Position: (0, 0, 0)
   - 3D Object → Cube (Barrel)
     - Scale: (0.03, 0.03, 0.2)
     - Position: (0, 0.05, 0.1)
   - Create Empty (ShootPoint)
     - Position: (0, 0.05, 0.2)
4. Add Components ao `VR_Pistol` root:
   - `VRWeaponController` (script)
   - `XR Grab Interactable`
   - `Line Renderer` (laser sight)
   - `Box Collider` (para grabbing)
   - `Rigidbody`
     - Use Gravity: ✓
     - Is Kinematic: ✗
5. Configure VRWeaponController:
   - Shoot Point: Arraste ShootPoint aqui
   - Range: 100
   - Fire Rate: 0.2
   - Max Ammo: 30
   - Reload Time: 2
   - Damage: 100
   - Show Laser Sight: ✓
   - Laser Sight: Arraste Line Renderer
6. Configure XR Grab Interactable:
   - Movement Type: Velocity Tracking
   - Throw On Detach: ✓
7. Configure Line Renderer:
   - Positions: 2
   - Width: 0.002
   - Color: Vermelho
8. Drag VR_Pistol para `Assets/Prefabs/Weapons/`
9. Delete da Hierarchy

**✅ Checkpoint:** 3 targets + 1 weapon prefabs criados

---

## 7. Setup de Cena
**Duração:** 10 minutos

### 7.1 Preparar Cena Base
1. `File → New Scene → Basic (URP)`
2. `File → Save As → Assets/Scenes/TrainingRange.unity`
3. Delete "Main Camera" original (XR Origin tem câmera própria)

### 7.2 Adicionar XR Setup
**Se ainda não tem:**
1. Hierarchy → XR → XR Origin (Action-based)
2. Hierarchy → Create Empty → Nome: `XR Interaction Manager`
   - Add Component: XR Interaction Manager

### 7.3 Criar Managers GameObject
1. Hierarchy → Create Empty
2. Nome: `--- MANAGERS ---` (separador visual)
3. Children:
   - Create Empty → `GameManager`
     - Add Component: `GameManager` (script)
   - Create Empty → `AudioManager`
     - Add Component: `AudioManager` (script)
   - Create Empty → `ObjectPooler`
     - Add Component: `ObjectPooler` (script)
   - Create Empty → `TargetSpawner`
     - Add Component: `TargetSpawner` (script)
   - Create Empty → `DebugHelper`
     - Add Component: `DebugHelper` (script)

### 7.4 Configurar Object Pooler
1. Select `ObjectPooler` GameObject
2. Inspector → ObjectPooler script:
   - **Pools** → Size: 3
     - [0]
       - Tag: `BasicTarget`
       - Prefab: Arraste BasicTarget.prefab
       - Size: 10
     - [1]
       - Tag: `MovingTarget`
       - Prefab: Arraste MovingTarget.prefab
       - Size: 10
     - [2]
       - Tag: `PrecisionTarget`
       - Prefab: Arraste PrecisionTarget.prefab
       - Size: 5

### 7.5 Configurar Target Spawner
1. Select `TargetSpawner` GameObject
2. Inspector → TargetSpawner script:
   - **Prefabs:**
     - Basic Target Prefab: BasicTarget
     - Moving Target Prefab: MovingTarget
     - Precision Target Prefab: PrecisionTarget
   - **Spawn Settings:**
     - Use Random Positions: ✓
     - Spawn Area Min: (-5, 1, 5)
     - Spawn Area Max: (5, 3, 10)
   - **Game Mode Configs:** (configurar depois)

### 7.6 Criar UI Canvas
1. Hierarchy → UI → Canvas
2. Nome: `UICanvas`
3. Inspector → Canvas:
   - **Render Mode:** World Space ⚠️ Importante
   - **Position:** (0, 1.5, 2)
   - **Rotation:** (0, 180, 0)
   - **Scale:** (0.001, 0.001, 0.001)
4. Add Component: `UIManager` (script)
5. Add Children (basic structure):
   - Create Empty → `MainMenuPanel`
   - Create Empty → `GameplayHUDPanel`
   - Create Empty → `PauseMenuPanel`
   - Create Empty → `GameOverPanel`

**Para setup completo de UI:** Ver seção opcional abaixo

### 7.7 Adicionar Arma à Cena (Para Testing)
1. Hierarchy → Instanciar `VR_Pistol.prefab`
2. Position: (0, 1, 1) - Em frente ao player
3. ⚠️ Durante gameplay, player vai grab a arma

### 7.8 Adicionar Chão
1. Hierarchy → 3D Object → Plane
2. Nome: `Ground`
3. Position: (0, 0, 0)
4. Scale: (10, 1, 10)

**✅ Checkpoint:** Cena configurada com managers, UI canvas, arma

---

## 8. Primeiro Teste
**Duração:** 5 minutos

### 8.1 Desktop Test (Sem VR)
1. **Play Mode:**
   - Click ▶️ Play button
2. **Verificações:**
   - [x] Console sem erros vermelhos
   - [x] XR Origin visível
   - [x] Arma visível na cena
   - [x] Managers inicializam

3. **Desktop Testing Mode:**
   - Adicionar `DesktopWeaponTester.cs` ao Main Camera:
     - Select: `XR Origin → Camera Offset → Main Camera`
     - Add Component: `DesktopWeaponTester`
   - Configure:
     - Camera Height: 1.6
     - Pickup Range: 5
   - Play Mode → Controles:
     - **WASD:** Movimento
     - **Mouse:** Look
     - **E:** Pickup arma
     - **Left Click:** Shoot
     - **R:** Reload

4. **Test Spawning:**
   - Press **T** (spawn BasicTarget)
   - Press **Y** (spawn MovingTarget)
   - Press **E** (pickup arma se perto)
   - Click esquerdo (shoot)
   - Verificar: Target é destruído ao hit

**✅ Desktop Test passou:** Pode atirar, targets reagem

### 8.2 VR Test (Com Headset)
**Opção A: PC VR (Oculus Link, SteamVR)**
1. Conectar headset ao PC
2. Iniciar Oculus App ou SteamVR
3. Unity Play Mode
4. Headset deve mostrar a cena
5. Test:
   - [x] Grab weapon (trigger + grip)
   - [x] Shoot (trigger)
   - [x] Targets aparecem
   - [x] Hits registram

**Opção B: Quest 2 Build**
1. `File → Build Settings → Android`
2. Conectar Quest 2 via USB
3. Enable Developer Mode no Quest:
   - Meta Quest App → Settings → Developer Mode → On
4. Click **Build And Run**
5. Escolher localização para APK
6. ⏱️ Aguardar build (5-10min primeira vez)
7. App abre automaticamente no Quest
8. Test igual acima

**✅ VR Test passou:** Pode jogar em VR

---

## Troubleshooting

### Problema: "XR Origin não aparece em Hierarchy menu"
**Solução:**
- Verificar XR Interaction Toolkit instalado
- `Window → Package Manager → XR Interaction Toolkit`
- Se não instalado, instalar

### Problema: "Scripts têm erros de compilação"
**Soluções:**
- Verificar Input System instalado
- `Edit → Project Settings → Player → Active Input Handling → Both`
- Reiniciar Unity

### Problema: "Weapon não grab em VR"
**Checklist:**
- [ ] XRGrabInteractable component presente?
- [ ] Collider enabled?
- [ ] Rigidbody presente?
- [ ] XR Interaction Manager na cena?
- [ ] Controllers configurados (Left/Right)?

### Problema: "Targets não spawnam"
**Checklist:**
- [ ] ObjectPooler configurado com pools?
- [ ] TargetSpawner tem prefabs assignados?
- [ ] GameManager.StartGame() foi chamado?
- [ ] Verificar Console para erros

### Problema: "Performance baixo em Quest 2"
**Quick Fixes:**
- Reduzir max active targets (4-5)
- Simplificar particle effects
- `Project Settings → XR → Oculus → Low Overhead Mode → On`
- Stereo Rendering: Multiview

### Problema: "Build para Android falha"
**Checklist:**
- [ ] Android Build Support instalado?
- [ ] Android SDK/NDK configurados?
- [ ] Minimum API Level ≥ 29?
- [ ] Developer Mode enabled no Quest?

---

## Configuração Opcional (UI Completa)

### Se tiver tempo extra (30min+)

#### HUD Setup
1. `GameplayHUDPanel` child de UICanvas:
   - TextMeshPro: `ScoreText`
     - Texto: "0"
     - Font Size: 48
     - Alignment: Center
   - TextMeshPro: `TimerText`
     - Texto: "02:00"
     - Font Size: 36
   - TextMeshPro: `AccuracyText`
     - Texto: "100%"
     - Font Size: 24
   - TextMeshPro: `StreakText`
     - Texto: "Streak: 0"
   - TextMeshPro: `ComboText`
     - Texto: "x2 COMBO!"
2. HUDController component em GameplayHUDPanel
3. Assign TextMeshPro fields no Inspector

#### Menu Setup
Similar para MainMenuPanel, PauseMenuPanel, GameOverPanel

---

## Next Steps

### Agora que o básico funciona:

1. **Configurar Game Modes:**
   - GameManager → Configure each mode
   - TargetSpawner → Create SpawnConfigs

2. **Audio Integration:**
   - Import audio files para `Assets/Audio/`
   - AudioManager → Assign AudioClips

3. **Visual Polish:**
   - Particle systems para hits/destroy
   - Materials melhores
   - Lighting melhor

4. **Testing & Iteration:**
   - Playtest todos os 5 modos
   - Balance spawn rates
   - Ajustar difficulty

5. **Build Final:**
   - Build para Quest 2
   - Performance profiling
   - Bug fixes

---

## Cheat Sheet - Comandos Úteis

### Keyboard Shortcuts (DebugHelper)
```
T - Spawn BasicTarget
Y - Spawn MovingTarget
U - Spawn PrecisionTarget
K - Kill all targets
P - Print stats
F1 - Toggle debug UI
ESC - Pause
R - Restart
M - Return to menu
```

### Unity Shortcuts
```
Ctrl+P - Play Mode
Ctrl+Shift+P - Pause
Ctrl+Shift+F - Frame Selected
F - Focus on Selected
W/E/R - Move/Rotate/Scale tool
```

### Build Commands
```bash
# Quest 2: Ver dispositivos conectados
adb devices

# Quest 2: Install APK manualmente
adb install VR_AimLab.apk

# Quest 2: Ver logs em real-time
adb logcat -s Unity
```

---

## Recursos Adicionais

### Documentation
- [XR Interaction Toolkit Docs](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.3/manual/index.html)
- [Quest Development](https://developer.oculus.com/documentation/unity/)
- [URP Documentation](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)

### Assets (Free)
- **Sounds:** [Freesound.org](https://freesound.org)
- **Music:** [Incompetech](https://incompetech.com)
- **Textures:** [TexturesHub](https://www.textureshub.com)
- **Models:** [Sketchfab](https://sketchfab.com) (filter: Free)

### Communities
- Unity Forums - VR Section
- Reddit: r/Unity3D, r/oculus, r/virtualreality
- Discord: Unity VR Development

---

**Setup Guide criado por:** Development Team
**Última atualização:** Novembro 2025
**Versão:** 1.0

**✅ Parabéns!** Se chegou até aqui, tem um protótipo VR funcionando!

*"Welcome to APEX. Your journey to precision mastery begins now."* - ARIA
