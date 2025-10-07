using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public partial class HelixLab : Node3D
{
    [Export] public PackedScene PortalScene;

    private Node3D _portalGrid;
    private Node3D _player;
    private Camera3D _camera;
    private float _mouseSensitivity = 0.003f;
    private float _moveSpeed = 5.0f;
    private float _runMultiplier = 2.0f;
    private Vector3 _velocity = Vector3.Zero;
    private Vector2 _rotation = Vector2.Zero;

    // Wolf API client
    private string _wolfSocketPath = "/var/run/wolf/wolf.sock";
    private List<LobbyData> _lobbies = new();
    private Dictionary<string, Node3D> _portalInstances = new();

    public override void _Ready()
    {
        _portalGrid = GetNode<Node3D>("PortalGrid");
        _player = GetNode<Node3D>("Player");
        _camera = GetNode<Camera3D>("Player/Camera3D");

        // Capture mouse
        Input.MouseMode = Input.MouseModeEnum.Captured;

        // Load portal scene
        PortalScene = GD.Load<PackedScene>("res://Scenes/HelixLab/Portal.tscn");

        // Start fetching lobbies
        FetchLobbies();

        // Update lobbies every 2 seconds
        var timer = new Timer();
        AddChild(timer);
        timer.WaitTime = 2.0;
        timer.Timeout += FetchLobbies;
        timer.Start();

        GD.Print("🎮 Helix Lab initialized - Press ESC to release mouse");
    }

    public override void _Input(InputEvent @event)
    {
        // Toggle mouse capture with ESC
        if (@event.IsActionPressed("ui_cancel"))
        {
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
                Input.MouseMode = Input.MouseModeEnum.Visible;
            else
                Input.MouseMode = Input.MouseModeEnum.Captured;
        }

        // Mouse look
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            _rotation.X -= mouseMotion.Relative.Y * _mouseSensitivity;
            _rotation.Y -= mouseMotion.Relative.X * _mouseSensitivity;
            _rotation.X = Mathf.Clamp(_rotation.X, -Mathf.Pi / 2, Mathf.Pi / 2);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Apply camera rotation
        _player.Rotation = new Vector3(0, _rotation.Y, 0);
        _camera.Rotation = new Vector3(_rotation.X, 0, 0);

        // Get movement input
        var input = Vector3.Zero;
        if (Input.IsActionPressed("ui_up") || Input.IsKeyPressed(Key.W))
            input.Z -= 1;
        if (Input.IsActionPressed("ui_down") || Input.IsKeyPressed(Key.S))
            input.Z += 1;
        if (Input.IsActionPressed("ui_left") || Input.IsKeyPressed(Key.A))
            input.X -= 1;
        if (Input.IsActionPressed("ui_right") || Input.IsKeyPressed(Key.D))
            input.X += 1;
        if (Input.IsKeyPressed(Key.Space))
            input.Y += 1;
        if (Input.IsKeyPressed(Key.Ctrl))
            input.Y -= 1;

        // Apply movement
        input = input.Normalized();
        var speed = Input.IsKeyPressed(Key.Shift) ? _moveSpeed * _runMultiplier : _moveSpeed;

        // Transform input to world space
        var moveDir = _player.GlobalTransform.Basis * input;
        _velocity = moveDir * speed;

        _player.GlobalPosition += _velocity * (float)delta;

        // Check portal interaction
        CheckPortalInteraction();
    }

    private void CheckPortalInteraction()
    {
        var raycast = _camera.GetNode<RayCast3D>("RayCast3D");
        if (raycast.IsColliding())
        {
            var collider = raycast.GetCollider();
            if (collider is Area3D area && area.GetParent() is Portal portal)
            {
                // Show interaction prompt
                if (Input.IsKeyPressed(Key.E))
                {
                    portal.OnInteract();
                }
            }
        }
    }

    private async void FetchLobbies()
    {
        try
        {
            // TODO: Implement HTTP-over-Unix-socket request
            // For now, use HTTP to localhost (requires Wolf API to listen on network)
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost/");

            var response = await client.GetStringAsync("api/v1/lobbies");
            var data = JsonSerializer.Deserialize<LobbiesResponse>(response);

            if (data?.Success == true && data.Lobbies != null)
            {
                _lobbies = data.Lobbies;
                UpdatePortals();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to fetch lobbies: {ex.Message}");
        }
    }

    private void UpdatePortals()
    {
        // Remove portals for lobbies that no longer exist
        var lobbiesById = new HashSet<string>();
        foreach (var lobby in _lobbies)
            lobbiesById.Add(lobby.Id);

        var toRemove = new List<string>();
        foreach (var kvp in _portalInstances)
        {
            if (!lobbiesById.Contains(kvp.Key))
                toRemove.Add(kvp.Key);
        }

        foreach (var id in toRemove)
        {
            _portalInstances[id].QueueFree();
            _portalInstances.Remove(id);
        }

        // Create/update portals for current lobbies
        int index = 0;
        foreach (var lobby in _lobbies)
        {
            if (!_portalInstances.ContainsKey(lobby.Id))
            {
                // Create new portal
                var portal = PortalScene.Instantiate<Portal>();
                _portalGrid.AddChild(portal);

                // Position in grid (3 columns)
                int row = index / 3;
                int col = index % 3;
                portal.Position = new Vector3(col * 6 - 6, 0, row * 6 - 5);

                portal.SetLobbyData(lobby);
                _portalInstances[lobby.Id] = portal;

                index++;
            }
            else
            {
                // Update existing portal
                var portal = _portalInstances[lobby.Id] as Portal;
                portal?.UpdateLobbyData(lobby);
            }
        }

        GD.Print($"🎮 Updated portals: {_lobbies.Count} active lobbies");
    }
}

// Data classes for Wolf API
public class LobbiesResponse
{
    public bool Success { get; set; }
    public List<LobbyData> Lobbies { get; set; }
}

public class LobbyData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool MultiUser { get; set; }
    public string StartedByProfileId { get; set; }
    public bool PinRequired { get; set; }
    public bool StopWhenEveryoneLeaves { get; set; }
}
