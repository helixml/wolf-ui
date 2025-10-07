extends Node3D

@onready var glow_light = $GlowLight
@onready var animation = $AnimationPlayer

func _ready():
	# Start pulsing animation
	animation.play("pulse_glow")
	print("✨ HELIX CODE sign initialized")

func _process(delta):
	# Rotate sign slowly for dynamic effect
	rotate_y(delta * 0.1)
