extends Node3D

@export var portal_scene: PackedScene

@onready var portal_grid = $PortalGrid
@onready var player = $Player
@onready var camera = $Player/Camera3D

var mouse_sensitivity = 0.003
var move_speed = 5.0
var run_multiplier = 2.0
var velocity = Vector3.ZERO
var rotation_x = 0.0
var rotation_y = 0.0

var lobbies = []
var portal_instances = {}

func _ready():
	# Capture mouse
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

	# Load portal scene
	portal_scene = load("res://Scenes/HelixLab/Portal.tscn")

	# Start fetching lobbies
	fetch_lobbies()

	# Update lobbies every 2 seconds
	var timer = Timer.new()
	add_child(timer)
	timer.wait_time = 2.0
	timer.timeout.connect(fetch_lobbies)
	timer.start()

	print("🎮 Helix Lab initialized - Press ESC to release mouse")

func _input(event):
	# Toggle mouse capture with ESC
	if event.is_action_pressed("ui_cancel"):
		if Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
			Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
		else:
			Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

	# Mouse look
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		rotation_y -= event.relative.x * mouse_sensitivity
		rotation_x -= event.relative.y * mouse_sensitivity
		rotation_x = clamp(rotation_x, -PI/2, PI/2)

func _physics_process(delta):
	# Apply camera rotation
	player.rotation.y = rotation_y
	camera.rotation.x = rotation_x

	# Get movement input
	var input_dir = Vector3.ZERO
	if Input.is_action_pressed("ui_up") or Input.is_key_pressed(KEY_W):
		input_dir.z -= 1
	if Input.is_action_pressed("ui_down") or Input.is_key_pressed(KEY_S):
		input_dir.z += 1
	if Input.is_action_pressed("ui_left") or Input.is_key_pressed(KEY_A):
		input_dir.x -= 1
	if Input.is_action_pressed("ui_right") or Input.is_key_pressed(KEY_D):
		input_dir.x += 1
	if Input.is_key_pressed(KEY_SPACE):
		input_dir.y += 1
	if Input.is_key_pressed(KEY_CTRL):
		input_dir.y -= 1

	# Apply movement
	input_dir = input_dir.normalized()
	var speed = move_speed * run_multiplier if Input.is_key_pressed(KEY_SHIFT) else move_speed

	# Transform to world space
	var move_dir = player.global_transform.basis * input_dir
	velocity = move_dir * speed

	player.global_position += velocity * delta

	# Check portal interaction
	check_portal_interaction()

func check_portal_interaction():
	var raycast = camera.get_node("RayCast3D")
	if raycast.is_colliding():
		var collider = raycast.get_collider()
		if collider and collider.get_parent().has_method("on_interact"):
			if Input.is_key_pressed(KEY_E):
				collider.get_parent().on_interact()

func fetch_lobbies():
	# Use HTTPRequest for Wolf API
	var http = HTTPRequest.new()
	add_child(http)
	http.request_completed.connect(_on_lobbies_received)

	# Note: This uses HTTP to localhost - Wolf API needs to be accessible
	# In production, would use Unix socket via custom HTTP client
	var error = http.request("http://localhost/api/v1/lobbies")
	if error != OK:
		print_debug("Failed to start lobby request: ", error)

func _on_lobbies_received(result, response_code, headers, body):
	if response_code != 200:
		print_debug("Failed to fetch lobbies: HTTP ", response_code)
		return

	var json = JSON.new()
	var parse_result = json.parse(body.get_string_from_utf8())
	if parse_result != OK:
		print_debug("Failed to parse lobbies JSON")
		return

	var data = json.data
	if data.has("success") and data.success and data.has("lobbies"):
		lobbies = data.lobbies
		update_portals()

func update_portals():
	# Remove portals for lobbies that no longer exist
	var lobby_ids = {}
	for lobby in lobbies:
		lobby_ids[lobby.id] = true

	var to_remove = []
	for lobby_id in portal_instances.keys():
		if not lobby_ids.has(lobby_id):
			to_remove.append(lobby_id)

	for lobby_id in to_remove:
		portal_instances[lobby_id].queue_free()
		portal_instances.erase(lobby_id)

	# Create/update portals
	var index = 0
	for lobby in lobbies:
		if not portal_instances.has(lobby.id):
			# Create new portal
			var portal = portal_scene.instantiate()
			portal_grid.add_child(portal)

			# Position in grid (3 columns)
			var row = index / 3
			var col = index % 3
			portal.position = Vector3(col * 6 - 6, 0, row * 6 - 5)

			portal.set_lobby_data(lobby)
			portal_instances[lobby.id] = portal

			index += 1
		else:
			# Update existing portal
			portal_instances[lobby.id].update_lobby_data(lobby)

	print("🎮 Updated portals: ", lobbies.size(), " active lobbies")
