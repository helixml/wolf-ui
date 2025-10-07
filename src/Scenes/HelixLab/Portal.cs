using Godot;
using System;
using System.Net.Http;
using System.Text.Json;

public partial class Portal : Area3D
{
    private string _lobbyId;
    private string _lobbyName;
    private bool _pinRequired;
    private Color _statusColor;

    private Label3D _nameLabel;
    private Label3D _statusLabel;
    private MeshInstance3D _ring;
    private MeshInstance3D _surface;
    private OmniLight3D _light;
    private GPUParticles3D _particles;

    public override void _Ready()
    {
        _nameLabel = GetNode<Label3D>("LobbyLabel");
        _statusLabel = GetNode<Label3D>("StatusLabel");
        _ring = GetNode<MeshInstance3D>("PortalRing");
        _surface = GetNode<MeshInstance3D>("PortalSurface");
        _light = GetNode<OmniLight3D>("PortalLight");
        _particles = GetNode<GPUParticles3D>("PortalParticles");

        // Start particles
        _particles.Emitting = true;
    }

    public void SetLobbyData(LobbyData lobby)
    {
        _lobbyId = lobby.Id;
        _lobbyName = lobby.Name;
        _pinRequired = lobby.PinRequired;

        UpdateDisplay();
    }

    public void UpdateLobbyData(LobbyData lobby)
    {
        SetLobbyData(lobby);
    }

    private void UpdateDisplay()
    {
        _nameLabel.Text = _lobbyName;
        _statusLabel.Text = _pinRequired ? "🔐 PIN Required" : "Open";

        // Color-code portal by lobby name prefix
        if (_lobbyName.StartsWith("External Agent"))
            _statusColor = new Color(0.2f, 1.0f, 0.3f); // Green for agents
        else if (_lobbyName.StartsWith("PDE:"))
            _statusColor = new Color(0.6f, 0.3f, 1.0f); // Purple for PDEs
        else
            _statusColor = new Color(0.3f, 0.8f, 1.0f); // Cyan for others

        // Apply color to light and ring
        _light.LightColor = _statusColor;

        var ringMat = _ring.GetActiveMaterial(0) as StandardMaterial3D;
        if (ringMat != null)
        {
            ringMat.EmissionEnabled = true;
            ringMat.Emission = _statusColor;
            ringMat.EmissionEnergyMultiplier = 2.0f;
        }
    }

    public override void _Process(double delta)
    {
        // Rotate portal slowly
        RotateY((float)(delta * 0.3));

        // Bob up and down slightly
        Position = new Vector3(Position.X, Mathf.Sin((float)Time.GetTicksMsec() / 1000.0f) * 0.2f, Position.Z);
    }

    public void OnInteract()
    {
        GD.Print($"🌀 Interacting with portal: {_lobbyName}");

        if (_pinRequired)
        {
            // Show PIN entry dialog
            ShowPINEntry();
        }
        else
        {
            // Join lobby without PIN
            JoinLobby(null);
        }
    }

    private void ShowPINEntry()
    {
        // TODO: Instantiate holographic PIN entry UI
        GD.Print($"🔐 Showing PIN entry for {_lobbyName}");

        // For now, use simple prompt
        // In full implementation, this would show 3D holographic keypad
        var pin = new int[] { 0, 0, 0, 0 }; // Placeholder
        JoinLobby(pin);
    }

    private async void JoinLobby(int[] pin)
    {
        try
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost/");

            var requestData = new
            {
                lobby_id = _lobbyId,
                moonlight_session_id = GetMoonlightSessionId(),
                pin = pin
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/v1/lobbies/join", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                GD.Print($"✅ Successfully joined lobby: {_lobbyName}");
                // Wolf will switch streams automatically - UI will disappear as we enter the lobby
            }
            else
            {
                GD.PrintErr($"❌ Failed to join lobby: {result}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error joining lobby: {ex.Message}");
        }
    }

    private string GetMoonlightSessionId()
    {
        // TODO: Get actual Moonlight session ID from environment or Wolf API
        // For now, return placeholder
        return Environment.GetEnvironmentVariable("WOLF_SESSION_ID") ?? "unknown";
    }
}
