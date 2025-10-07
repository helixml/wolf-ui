using Godot;

public partial class HelixSign : Node3D
{
    private OmniLight3D _glowLight;
    private AnimationPlayer _animation;

    public override void _Ready()
    {
        _glowLight = GetNode<OmniLight3D>("GlowLight");
        _animation = GetNode<AnimationPlayer>("AnimationPlayer");

        // Start pulsing animation
        _animation.Play("pulse_glow");

        GD.Print("✨ HELIX CODE sign initialized");
    }

    public override void _Process(double delta)
    {
        // Rotate sign slowly for dynamic effect
        RotateY((float)(delta * 0.1));
    }
}
