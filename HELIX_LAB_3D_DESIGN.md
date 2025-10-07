# Helix Lab - Immersive 3D Wolf UI

## Vision

Transform Wolf UI from a flat 2D lobby list into an immersive 3D sci-fi laboratory where users navigate through portals to access different agent sessions.

**Inspired by:** CS Lewis Narnia (magical ponds/portals to different worlds)

## Scene Layout

### Main Lab Environment
```
                    ┌─────────────────────────────────────┐
                    │   "HELIX CODE" Neon Sign (Massive)  │
                    │        Visible from anywhere         │
                    └─────────────────────────────────────┘
                                    │
                                    ▼
        ┌───────────────────────────────────────────────┐
        │                                                │
        │  Portal 1        Portal 2        Portal 3     │
        │  (Agent A)       (Agent B)        (PDE 1)     │
        │    🌀              🌀              🌀         │
        │  [Preview]       [Preview]       [Preview]    │
        │  PIN: ****       PIN: ****       PIN: ****   │
        │                                                │
        │              Player Camera                     │
        │                  👁️                            │
        │                                                │
        │  Portal 4        Portal 5        Portal 6     │
        │  (PDE 2)         (Agent C)       (Available)  │
        │    🌀              🌀              💤          │
        │                                                │
        └───────────────────────────────────────────────┘
```

### Scene Hierarchy
```
HelixLab (Node3D)
├── Environment
│   ├── WorldEnvironment (lighting, sky, fog)
│   ├── DirectionalLight3D (main lab lighting)
│   └── AmbientLight3D (soft fill light)
├── LabStructure
│   ├── Floor (MeshInstance3D - reflective surface)
│   ├── Walls (MeshInstance3D - glowing panels)
│   ├── Ceiling (MeshInstance3D - grid with lights)
│   └── LabProps (computers, tables, holographic displays)
├── HelixSign
│   ├── SignMesh (3D text \"HELIX CODE\")
│   ├── NeonGlow (OmniLight3D with blue/purple gradient)
│   ├── ParticleTrail (GPUParticles3D - energy flowing)
│   └── AnimationPlayer (pulsing glow effect)
├── PortalGrid
│   ├── Portal_1 (Area3D, detects player entry)
│   │   ├── PortalMesh (torus/ring shape)
│   │   ├── PortalShader (animated warp effect)
│   │   ├── PreviewViewport (shows lobby content)
│   │   ├── LobbyInfo (3D label with name/status)
│   │   └── PortalParticles (swirling effect)
│   ├── Portal_2
│   └── ... (dynamically created based on lobby count)
├── Player
│   ├── Camera3D (first-person view)
│   ├── RayCast3D (interaction detection)
│   └── MovementController (script for WASD/mouse)
└── UI
    ├── Minimap (shows portal locations)
    ├── PINEntryHologram (3D holographic keypad)
    └── LobbyInfoPanel (selected lobby details)
```

## Features

### 1. HELIX CODE Neon Sign
- **Size:** Massive (visible from across the lab)
- **Material:** Emissive shader with neon glow
- **Color:** Gradient (electric blue → purple → cyan)
- **Effects:**
  - Pulsing glow animation (subtle breathing)
  - Particle trail flowing along letters
  - Volumetric fog around sign for depth
  - Reflection on floor below

**Implementation:**
```gdscript
# HelixSign.gd
extends Node3D

@onready var glow_light = $NeonGlow
@onready var animation = $AnimationPlayer

func _ready():
    animation.play("pulse_glow")
    # Start particle systems
    $ParticleTrail.emitting = true
```

### 2. Portal System

**Portal Mesh:** Torus/ring shape floating above ground

**Shader Effects:**
- Warp/distortion effect at portal surface
- Color-coded by lobby status:
  - 🟢 Green: Agent active, responding
  - 🔵 Blue: Agent waiting for task
  - 🟣 Purple: PDE with user connected
  - 🟡 Yellow: Available lobby
  - ⚪ Gray: Inactive/stopped
- Particle swirl effect around rim
- Portal "event horizon" shader (rippling water-like surface)

**Preview Window:**
- ViewportTexture showing lobby's actual content
- Rendered on flat plane inside portal
- Updates in real-time from Wolf API
- Low FPS (5-10fps) to save resources

**Lobby Info Display:**
- 3D floating label above portal
- Shows: Lobby name, status, user count
- Holographic style (semi-transparent, glowing edges)

### 3. Player Movement

**Camera Control:**
- First-person perspective
- Mouse look (pitch/yaw)
- WASD movement
- Shift = run faster
- Space = jump/fly up
- Ctrl = descend

**Interaction:**
- Raycast from camera to detect portal collision
- Crosshair changes when looking at portal
- E key or click to interact
- Prompt: "Press E to enter [Lobby Name]"

### 4. PIN Entry System

**Holographic Keypad:**
- 3D number buttons (0-9) floating in space
- Appears when entering portal that requires PIN
- Click buttons or type numbers on keyboard
- Glowing feedback on button press
- Display shows: "****" as digits entered
- Submit button or Enter key to confirm

**PIN Validation:**
- Call Wolf API `/api/v1/lobbies/join` with PIN
- If correct: Seamless stream switch to lobby
- If incorrect: Error message, retry prompt
- 3 attempts before timeout

### 5. Lab Environment Details

**Floor:**
- Reflective metallic surface (StandardMaterial3D with metallic=0.8)
- Grid lines with subtle glow
- Reflects portals and neon sign

**Walls:**
- Dark panels with glowing accent lines
- Holographic displays showing:
  - Wolf logo
  - Lobby statistics
  - System status

**Ceiling:**
- Grid structure with embedded lights
- Creates sci-fi industrial feel
- Casts dynamic shadows on portals

**Props:**
- Futuristic computer terminals
- Holographic projectors
- Energy conduits with glowing lines
- Floating data streams (particle effects)

## Technical Implementation

### Scene Files to Create

1. **Scenes/HelixLab/HelixLab.tscn** - Main 3D scene
2. **Scenes/HelixLab/Portal.tscn** - Reusable portal prefab
3. **Scenes/HelixLab/HelixSign.tscn** - HELIX CODE neon sign
4. **Scenes/HelixLab/PINEntry.tscn** - Holographic PIN entry UI

### Scripts to Create

1. **HelixLab.cs** - Main scene controller
   - Fetches lobbies from Wolf API
   - Spawns portals dynamically
   - Manages scene state

2. **Portal.cs** - Portal behavior
   - Lobby data (name, ID, PIN required, status)
   - Preview update from Wolf API
   - Interaction detection
   - Status color changes

3. **PlayerController.cs** - Camera and movement
   - WASD movement with physics
   - Mouse look controls
   - Raycast interaction system
   - Smooth camera interpolation

4. **PINEntry.cs** - PIN entry hologram
   - Number button interactions
   - PIN submission to Wolf API
   - Success/failure feedback
   - Lobby join on success

### Shaders to Create

1. **portal_surface.gdshader** - Portal event horizon
   - Animated distortion/warp effect
   - Color-coded by status
   - Transparency with rim glow

2. **neon_sign.gdshader** - HELIX CODE sign glow
   - Emissive material
   - Pulsing animation
   - Bloom-friendly colors

3. **hologram.gdshader** - Holographic UI elements
   - Scan lines effect
   - Color fade (blue/cyan)
   - Transparency with fresnel

## Wolf API Integration

**Lobby List Endpoint:**
```gdscript
func fetch_lobbies():
    var http = HTTPRequest.new()
    add_child(http)
    http.request_completed.connect(_on_lobbies_received)

    # Unix socket path from environment
    var socket_path = OS.get_environment("WOLF_SOCKET_PATH")
    # Note: May need HTTP-over-Unix-socket library for Godot
    http.request("http://localhost/api/v1/lobbies")
```

**Lobby Join:**
```gdscript
func join_lobby(lobby_id: String, pin: Array):
    var body = JSON.stringify({
        "lobby_id": lobby_id,
        "moonlight_session_id": get_moonlight_session_id(),
        "pin": pin
    })
    http.request("http://localhost/api/v1/lobbies/join", [], HTTPClient.METHOD_POST, body)
```

## Performance Considerations

- **Target:** 30 FPS (enough for UI, saves resources)
- **LOD:** Use low-poly models for lab props
- **Portals:** Max 10-12 visible portals (dynamic culling)
- **Particles:** Moderate particle counts (< 1000 per system)
- **Previews:** Low resolution, low framerate (5fps)

## Development Phases

1. ✅ Create branch and plan
2. Create basic 3D scene with camera
3. Add HELIX CODE neon sign
4. Implement single portal prefab
5. Add portal shader effects
6. Connect to Wolf API for lobby list
7. Spawn portals dynamically
8. Implement movement controls
9. Add portal interaction
10. Create holographic PIN entry
11. Test lobby switching
12. Polish and effects

## Success Criteria

- [ ] User can navigate 3D lab with WASD
- [ ] HELIX CODE sign visible and glowing
- [ ] Portals spawn for each active lobby
- [ ] Portal colors match lobby status
- [ ] Click portal → PIN entry appears
- [ ] Enter correct PIN → Stream switches to lobby
- [ ] Seamless lobby switching without Moonlight reconnect
- [ ] Runs at 30+ FPS in Wolf container

**Next:** Start implementation!
