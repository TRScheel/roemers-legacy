shader_type spatial;
render_mode unshaded;

uniform float time_speed : hint_range(0.0, 10.0) = 0.01;
uniform sampler2D sun_texture;
uniform float uv_scale = 1.0;
uniform float red_boost = 1;
uniform float noise_strength = 1.4;
uniform int cell_amount = 25;
uniform vec2 period = vec2(10., 15.);

vec2 modulo(vec2 divident, vec2 divisor){
	vec2 positiveDivident = mod(divident, divisor) + divisor;
	return mod(positiveDivident, divisor);
}

vec2 random(vec2 uv) {
	return vec2(fract(sin(dot(uv.xy,
		vec2(12.9898,78.233))) * 43758.5453123));
}

float seamless_noise(vec2 uv, vec2 _period) {
	uv = uv * float(cell_amount);
	vec2 cellsMinimum = floor(uv);
	vec2 cellsMaximum = ceil(uv);
	vec2 uv_fract = fract(uv);
	
	cellsMinimum = modulo(cellsMinimum, _period);
	cellsMaximum = modulo(cellsMaximum, _period);
	
	vec2 blur = smoothstep(0.0, 1.0, uv_fract);
	
	vec2 lowerLeftDirection = random(vec2(cellsMinimum.x, cellsMinimum.y));
	vec2 lowerRightDirection = random(vec2(cellsMaximum.x, cellsMinimum.y));
	vec2 upperLeftDirection = random(vec2(cellsMinimum.x, cellsMaximum.y));
	vec2 upperRightDirection = random(vec2(cellsMaximum.x, cellsMaximum.y));
	
	vec2 fraction = fract(uv);
	
	return mix( mix( dot( lowerLeftDirection, fraction - vec2(0, 0) ),
                     dot( lowerRightDirection, fraction - vec2(1, 0) ), blur.x),
                mix( dot( upperLeftDirection, fraction - vec2(0, 1) ),
                     dot( upperRightDirection, fraction - vec2(1, 1) ), blur.x), blur.y) * 0.8 + 0.5;
}

float fbm(vec2 uv, float time) {
    int octaves = 6;
    float amplitude = 0.5;
    float frequency = 3.0 + mod(time, 1.0) * 2.0;
	float value = 0.0;
	
    for(int i = 0; i < octaves; i++) {
        value += amplitude * seamless_noise(frequency * uv, period);
        amplitude *= 0.5;
        frequency *= 2.0;
    }
    return value;
}

float worley(vec2 uv, float columns, float rows) {
	
	vec2 index_uv = floor(vec2(uv.x * columns, uv.y * rows));
	vec2 fract_uv = fract(vec2(uv.x * columns, uv.y * rows));
	
	float minimum_dist = 1.0;  
	
	for (int y= -1; y <= 1; y++) {
		for (int x= -1; x <= 1; x++) {
			vec2 neighbor = vec2(float(x),float(y));
			vec2 point = random(index_uv + neighbor);
			
			vec2 diff = neighbor + point - fract_uv;
			float dist = length(diff);
			minimum_dist = min(minimum_dist, dist);
		}
	}
	
	return minimum_dist;
}

vec3 round_to_extremes(vec3 color, float midpoint) {
    vec3 result = color;
    
    // Round each component based on the midpoint
    result.r *= color.r < midpoint ? 0.0 : 1.5;
    result.g *= color.g < midpoint ? 0.0 : 1.5;
    result.b *= color.b < midpoint ? 0.0 : 1.5;
    
    return result;
}

void fragment() {
    vec2 uv = UV * uv_scale;
    float time1 = sin(TIME * time_speed);
    float time2 = cos(TIME * time_speed);
    float time3 = sin(TIME * time_speed * 4.0);
    float time4 = cos(TIME * time_speed * 4.0);
    vec2 uv1 = uv + vec2(-time1, time3);
    vec2 uv2 = uv + vec2(time4, -time2);
    vec2 uv3 = uv + vec2(time2 + 0.25, time1 + 0.25);
    vec2 uv4 = uv + vec2(-time3 + 0.25, -time2 + 0.25);

    // Create layered turbulence effect
    // float turbulence1 = fract(fbm(uv, time));
    // float turbulence2 = fract(fbm(uv, time2));
    // float worley = worley(UV, 3.0, 5.0);
    // float turbulence2 = fbm(uv, time * 0.5);

    // vec3 color1 = texture(sun_texture, uv).rgb;

    // vec2 uv_offset = uv;
    // uv_offset += vec2(turbulence1 + turbulence2);
    // vec3 color2 = texture(sun_texture, uv_offset).rgb;

    // vec3 color1 = vec3(1.0, 0.0, 0.0);    
    // vec3 color2 = vec3(0.0, 1.0, 0.0);
    // vec3 color = mix(mix(color1, color2, turbulence1), color1, turbulence2);

    // vec3 color = texture(sun_texture, uv).rgb;
    float fbmresult1 = (fbm(uv1, 0.0) + seamless_noise(uv2, period)) * red_boost;
    vec3 color1 = fract(vec3(fbmresult1, fbmresult1, fbmresult1));
    float fbmresult2 = (fbm(uv2, 3.0) + seamless_noise(uv3 * 3.0, vec2(period.y, period.x))) * red_boost;
    vec3 color2 = fract(vec3(fbmresult2, fbmresult2, fbmresult2));
    float fbmresult3 = (fbm(uv, 0.0) + seamless_noise(uv, period)) * red_boost;
    vec3 color3 = fract(vec3(fbmresult3, fbmresult3, fbmresult3));
    vec3 color4 = texture(sun_texture, UV).rgb;

    vec3 color = mix(color4, (color1 + color2 + color3) / 3.0, 0.25);
    // Apply color modulation
    ALBEDO = color;
    //EMISSION = color;
}
