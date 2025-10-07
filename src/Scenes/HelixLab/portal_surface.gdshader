shader_type spatial;

uniform vec3 portal_color : source_color = vec3(0.3, 0.8, 1.0);
uniform float time_scale : hint_range(0.0, 5.0) = 1.0;
uniform float distortion_strength : hint_range(0.0, 1.0) = 0.3;

// Animated portal event horizon effect
void fragment() {
    vec2 uv = UV;
    float time = TIME * time_scale;

    // Create swirling distortion
    vec2 center = vec2(0.5, 0.5);
    vec2 toCenter = uv - center;
    float dist = length(toCenter);
    float angle = atan(toCenter.y, toCenter.x);

    // Spiral warp
    float warp = sin(dist * 10.0 - time * 2.0) * distortion_strength;
    angle += warp;

    // Radial waves
    float wave1 = sin(dist * 15.0 - time * 3.0) * 0.5 + 0.5;
    float wave2 = sin(dist * 8.0 + time * 2.0) * 0.5 + 0.5;

    // Combine waves for complex pattern
    float pattern = wave1 * wave2;

    // Edge glow
    float edge = 1.0 - smoothstep(0.4, 0.5, dist);

    // Final color
    vec3 color = portal_color * pattern;
    color += portal_color * edge * 2.0; // Bright edge

    // Transparency increases toward center (portal is more transparent in middle)
    float alpha = smoothstep(0.1, 0.5, dist) * 0.8;

    ALBEDO = color;
    EMISSION = color * 2.0;
    ALPHA = alpha;
    ROUGHNESS = 0.1;
    METALLIC = 0.9;
}

void vertex() {
    // Subtle wave motion on portal surface
    float time = TIME * time_scale;
    VERTEX.z += sin(VERTEX.x * 2.0 + time) * 0.05;
    VERTEX.z += cos(VERTEX.y * 2.0 + time * 1.5) * 0.05;
}
