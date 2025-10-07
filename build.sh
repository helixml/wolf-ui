#!/bin/bash
set -e

echo "🎮 Building Helix Lab 3D with Godot..."

cd /home/luke/pm/wolf-ui

# Use Godot CI image to build the project
docker run --rm \
  -v "$(pwd)/src:/workspace" \
  -w /workspace \
  barichello/godot-ci:4.3 \
  godot --headless --export-release "Linux/X11" /workspace/helix-lab.x86_64

echo "✅ Build complete: src/helix-lab.x86_64"
