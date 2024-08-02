shader_type spatial;
render_mode unshaded, depth_draw_never, cull_disabled, blend_add;

// Uniforms for tweaking the effect
uniform float glow_intensity : hint_range(0.0, 5.0) = 0.15;  // Lowered intensity
uniform float glow_radius : hint_range(0.0, 2.0) = 0.5;
uniform float time_speed : hint_range(0.0, 10.0) = 0.25;

// Simple hash function for pseudorandom values
float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

// Basic noise function
float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash(i + vec2(0.0, 0.0)), hash(i + vec2(1.0, 0.0)), u.x),
               mix(hash(i + vec2(0.0, 1.0)), hash(i + vec2(1.0, 1.0)), u.x), u.y);
}

// Fractal Brownian Motion for richer texture
float fbm(vec2 p) {
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    for (int i = 0; i < 5; i++) {
        value += amplitude * noise(p * frequency);
        frequency *= 2.0;
        amplitude *= 0.5;
    }
    return value;
}

void fragment() {
    vec2 uv = UV * 2.0 - 1.0; // Convert UV to range -1 to 1
    float len = length(uv);

    // Create a radial gradient
    float glow = smoothstep(glow_radius, glow_radius - 0.1, len);

    // Apply time-based noise for dynamic effect
    float time = TIME * time_speed;
    float noise = fbm(uv * 5.0 + vec2(time, time));

    // Combine glow and noise
    glow *= (1.0 + noise * 0.1) * glow_intensity; // Reduced noise impact

    // Output the color with the glow effect
    ALBEDO = vec3(1.0, 0.7, 0.0) * glow; // Adjust color as needed
    EMISSION = vec3(1.0, 0.7, 0.0) * glow * 0.15; // Reduced emission multiplier
}
