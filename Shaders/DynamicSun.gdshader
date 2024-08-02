shader_type spatial;
render_mode unshaded;

uniform float time_speed : hint_range(0.0, 10.0) = 0.25;
uniform sampler2D sun_texture;
uniform float uv_scale = 1.0;
uniform float red_boost = 6;
uniform float noise_strength = 0.1;
uniform int cell_amount = 25;
uniform vec2 period = vec2(10., 15.);

float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash(i + vec2(0.0, 0.0)), hash(i + vec2(1.0, 0.0)), u.x),
               mix(hash(i + vec2(0.0, 1.0)), hash(i + vec2(1.0, 1.0)), u.x), u.y);
}

float smooth_noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);

    return mix(mix(noise(i + vec2(0.0, 0.0)), noise(i + vec2(1.0, 0.0)), u.x),
               mix(noise(i + vec2(0.0, 1.0)), noise(i + vec2(1.0, 1.0)), u.x), u.y);
}

float fbm(vec2 p) {
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    for (int i = 0; i < 5; i++) { // Increase number of octaves for more detail
        value += amplitude * smooth_noise(p * frequency);
        frequency *= 2.0;
        amplitude *= 0.5;
    }
    return value;
}

void fragment() {
    vec2 uv = UV * uv_scale;
    float time = TIME * time_speed;

    // Create layered turbulence effect
    float turbulence1 = fbm(uv + vec2(time, time) * noise_strength);
    float turbulence2 = fbm(uv * 1.5 + vec2(time, time) * noise_strength * 0.5); // Different scale
    float turbulence3 = fbm(uv * 0.5 + vec2(time * 0.5, time * 0.5) * noise_strength); // Slower movement

    // Combine multiple turbulence layers
    float combined_turbulence = turbulence1 + turbulence2 * 0.5 + turbulence3 * 0.25;

    // Modify UVs with combined turbulence
    uv += vec2(combined_turbulence * 0.05, combined_turbulence * 0.05);

    // Sample the texture with the modified UVs
    vec3 color = texture(sun_texture, uv).rgb;

    // Boost red component
    color.r *= red_boost;

    // Apply color modulation
    ALBEDO = color;
    EMISSION = color * 2.0;
}
