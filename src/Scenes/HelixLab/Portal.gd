extends Area3D

var lobby_id = ""
var lobby_name = ""
var pin_required = false
var status_color = Color(0.3, 0.8, 1.0)

@onready var name_label = $LobbyLabel
@onready var status_label = $StatusLabel
@onready var ring = $PortalRing
@onready var surface = $PortalSurface
@onready var light = $PortalLight
@onready var particles = $PortalParticles

func _ready():
	# Start particles
	particles.emitting = true

func set_lobby_data(lobby):
	lobby_id = lobby.id
	lobby_name = lobby.name
	pin_required = lobby.get("pin_required", false)
	update_display()

func update_lobby_data(lobby):
	set_lobby_data(lobby)

func update_display():
	name_label.text = lobby_name
	status_label.text = "🔐 PIN Required" if pin_required else "Open"

	# Color-code portal by lobby type
	if lobby_name.begins_with("External Agent"):
		status_color = Color(0.2, 1.0, 0.3)  # Green for agents
	elif lobby_name.begins_with("PDE:"):
		status_color = Color(0.6, 0.3, 1.0)  # Purple for PDEs
	else:
		status_color = Color(0.3, 0.8, 1.0)  # Cyan for others

	# Apply color to light
	light.light_color = status_color

	# Apply color to ring material
	var ring_mat = ring.get_active_material(0)
	if ring_mat:
		ring_mat.emission_enabled = true
		ring_mat.emission = status_color
		ring_mat.emission_energy_multiplier = 2.0

	# Update shader uniform for portal surface
	var surface_mat = surface.get_active_material(0)
	if surface_mat:
		surface_mat.set_shader_parameter("portal_color", status_color)

func _process(delta):
	# Rotate portal slowly
	rotate_y(delta * 0.3)

	# Bob up and down slightly
	position.y = sin(Time.get_ticks_msec() / 1000.0) * 0.2

func on_interact():
	print("🌀 Interacting with portal: ", lobby_name)

	if pin_required:
		# Show PIN entry
		show_pin_entry()
	else:
		# Join lobby without PIN
		join_lobby(null)

func show_pin_entry():
	print("🔐 Showing PIN entry for ", lobby_name)

	# For now, use simple input dialog
	# TODO: Create 3D holographic keypad
	# Placeholder: Accept PIN via keyboard input
	var pin = [0, 0, 0, 0]  # Would get from UI
	join_lobby(pin)

func join_lobby(pin):
	print("🚀 Joining lobby: ", lobby_name, " with PIN")

	var http = HTTPRequest.new()
	add_child(http)
	http.request_completed.connect(_on_join_completed)

	var moonlight_session_id = OS.get_environment("WOLF_SESSION_ID")
	if not moonlight_session_id:
		moonlight_session_id = "unknown"

	var body = {
		"lobby_id": lobby_id,
		"moonlight_session_id": moonlight_session_id,
		"pin": pin
	}

	var json_body = JSON.stringify(body)
	var headers = ["Content-Type: application/json"]

	var error = http.request("http://localhost/api/v1/lobbies/join",
		headers,
		HTTPClient.METHOD_POST,
		json_body)

	if error != OK:
		print_debug("Failed to send join request: ", error)

func _on_join_completed(result, response_code, headers, body):
	if response_code == 200:
		print("✅ Successfully joined lobby: ", lobby_name)
		# Wolf will switch streams - UI will disappear as we enter lobby
	else:
		var response_text = body.get_string_from_utf8()
		print_debug("❌ Failed to join lobby: HTTP ", response_code, " - ", response_text)
