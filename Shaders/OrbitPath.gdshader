shader_type spatial;
render_mode unshaded;

uniform float min_width = 1.0;
uniform vec3 line_color = vec3(1.0, 1.0, 0.0);

void fragment() {
    // Ensure precision is set for better handling of small features
    highp float screen_width = min_width / length(INV_PROJECTION_MATRIX[0].xyz);

    // Calculate the current fragment position in screen space
    vec2 screen_pos = FRAGCOORD.xy / VIEWPORT_SIZE;

    // Calculate distance from the center of the line in screen space
    float dist = abs(screen_pos.x - 0.5);

    // Use smoothstep for soft edges and adjust alpha to create the line effect
    float alpha = 1.0 - smoothstep(0.0, screen_width, dist);

    ALBEDO = line_color;
    ALPHA = alpha;
}
