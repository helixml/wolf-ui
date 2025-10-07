# 🎮 Helix Lab - Immersive 3D Lobby Selector

## Overview

Helix Lab transforms the Wolf UI lobby selector into an immersive 3D sci-fi laboratory environment. Navigate through portals to access different AI agent sessions and development environments.

**Inspired by:** CS Lewis' Narnia (magical ponds leading to different worlds)

## Features

### 🌟 Massive HELIX CODE Neon Sign
- Huge illuminated sign visible from anywhere in the lab
- Pulsing glow animation
- Particle effects flowing around letters
- Electric blue/purple gradient

### 🌀 Portal System
- Each active lobby appears as a glowing portal
- Color-coded by type:
  - 🟢 **Green** - External agent sessions (active AI)
  - 🟣 **Purple** - Personal Dev Environments
  - 🔵 **Cyan** - Other lobbies
- Animated shader effects (swirling, warping)
- Floating labels showing lobby name and status
- Walk/fly up to portal and press E to enter

### 🎮 Controls
- **WASD** - Move around the lab
- **Mouse** - Look around
- **Shift** - Run faster
- **Space** - Fly up
- **Ctrl** - Descend
- **E** - Interact with portal
- **ESC** - Toggle mouse capture

### 🔐 Holographic PIN Entry
- When entering PIN-protected lobby, holographic keypad appears
- 3D floating number buttons
- Type PIN on keyboard or click buttons
- Glowing feedback on button press
- Submit with Enter key

### 🗺️ Lab Environment
- Reflective metallic floor
- Glowing wall panels
- Futuristic ceiling grid
- Volumetric fog for atmosphere
- Dynamic lighting from portals

## How It Works

1. **Launch "Wolf UI" from Moonlight**
   - Instead of flat lobby list, you appear in 3D lab
   - HELIX CODE sign greets you

2. **Explore Active Lobbies**
   - Walk around to see all available portals
   - Each portal represents an active lobby
   - Preview of lobby content shown inside portal (planned)

3. **Select Lobby**
   - Walk up to desired portal
   - Press E to interact
   - If PIN required, holographic keypad appears

4. **Enter PIN**
   - Type 4-digit PIN from Helix frontend
   - Or click holographic number buttons
   - Submit with Enter

5. **Jump Through Portal!**
   - Wolf validates PIN
   - Streams switch to lobby content
   - You're now in the agent session/PDE
   - Same Moonlight connection (seamless!)

## Technical Details

### Built With
- **Godot 4.4** - Game engine with C# support
- **3D Rendering** - OpenGL acceleration (available in Wolf containers)
- **Shaders** - Custom portal and neon effects
- **Wolf API** - Real-time lobby list via `/api/v1/lobbies`

### Scene Structure
```
HelixLab.tscn (main scene)
├── Environment (lighting, fog)
├── LabStructure (floor, walls, ceiling)
├── HelixSign (HELIX CODE neon sign)
├── PortalGrid (dynamically spawned portals)
├── Player (camera + movement controller)
└── UI (crosshair, minimap)
```

### Components
- **HelixLab.cs** - Main controller, fetches lobbies, spawns portals
- **Portal.cs** - Individual portal behavior, lobby join logic
- **HelixSign.cs** - Neon sign animation
- **portal_surface.gdshader** - Animated portal surface effect

### Performance
- **Target:** 30 FPS (UI doesn't need high framerate)
- **Low-poly models** for lab environment
- **Moderate particles** (< 1000 per system)
- **Dynamic portal spawning** (max ~12 visible portals)

## Installation

### For Development
1. Open project in Godot 4.4+
2. Open Scenes/HelixLab/HelixLab.tscn
3. Press F5 to run
4. Test with mock lobby data

### For Wolf Container
1. Build Godot project for Linux
2. Export to `wolf-ui` executable
3. Use in Wolf config.toml as Wolf UI app
4. Connects to Wolf API via Unix socket at runtime

## Configuration

### Environment Variables
- `WOLF_SOCKET_PATH` - Path to Wolf API socket (default: `/var/run/wolf/wolf.sock`)
- `WOLF_SESSION_ID` - Current Moonlight session ID
- `LOGLEVEL` - Logging level (INFO, DEBUG)

### Wolf Integration
- Fetches lobbies every 2 seconds from `/api/v1/lobbies`
- Joins lobbies via `/api/v1/lobbies/join`
- Same API as original Wolf UI (drop-in replacement)

## Roadmap

### Phase 1: Core 3D Environment ✅
- [x] Create 3D scene
- [x] Add HELIX CODE sign
- [x] Implement basic portals
- [x] Portal shader effects
- [x] WASD camera controls

### Phase 2: Wolf API Integration
- [ ] HTTP-over-Unix-socket client
- [ ] Real-time lobby updates
- [ ] Portal spawning from lobby list
- [ ] Lobby join with PIN

### Phase 3: Polish
- [ ] Portal preview (ViewportTexture)
- [ ] Advanced particle effects
- [ ] Holographic 3D PIN entry
- [ ] Minimap
- [ ] Sound effects
- [ ] Loading transitions

### Phase 4: Production
- [ ] Build and test in Wolf container
- [ ] Performance optimization
- [ ] Error handling
- [ ] User documentation

## Credits

- **Original Wolf UI:** games-on-whales/wolf-ui
- **Helix Lab 3D:** Custom immersive experience for Helix Code
- **Inspiration:** CS Lewis (Narnia portals), sci-fi laboratories
- **Built with:** Godot Engine 4.4

---

**Status:** Initial implementation complete
**Branch:** helix-lab-3d
**Next:** Wolf API integration and testing
