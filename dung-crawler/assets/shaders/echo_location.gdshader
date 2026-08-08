shader_type spatial;
render_mode unshaded, cull_disabled, blend_add, depth_test_disabled;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap, repeat_disable;
uniform float min_step : hint_range(0.0, 1.0, 0.01) = 0.4; // min step for the smoothstep() function
uniform float max_step : hint_range(0.0, 1.0, 0.01) = 0.6; // max step for the smoothstep() function
uniform float fresnel_power : hint_range(0.0, 0.1) = 0.01;
uniform float opacity : hint_range(0.0, 1.0) = 1.0;
uniform vec3 color : source_color = vec3(1.0);

// x direction matrix
const mat2 kx = mat2(
	vec2(1, 0),
	vec2(0, -1)
	
);
// y direction matrix 
const mat2 ky = mat2(
	vec2(0, 1),
	vec2(-1, 0)
);

void fragment() { 
	
	vec4 texture_color = texture(screen_texture, SCREEN_UV);
	
	mat2 I; // Matrix is essentially an array of vectors (in this case vector2s)
	for (int i = -1; i <= 1; i++ ) {
	    for (int j = -1; j <= 1; j++) {
	        vec3 sample_around_pixel = texelFetch(screen_texture, ivec2(FRAGCOORD.xy) + ivec2(i, j), 0).rgb;
	        vec3 sample_grey = vec3(sample_around_pixel.r * 0.3, sample_around_pixel.g * 0.59, sample_around_pixel.b * 0.11);
	        I[i+1][j+1] = sample_grey.r + sample_grey.g + sample_grey.b;
	    }
	}
	
	// apply the operator
	float gx = dot(kx[0], I[0]) + dot(kx[1], I[1]);
	float gy = dot(ky[0], I[0]) + dot(ky[1], I[1]);
	float g = sqrt(pow(gx, 2.0) + pow(gy, 2.0)); // combine x and y of the operator and get rid of negative values
	
	
	// strength of the effect;
	g = smoothstep(min_step, max_step, g);
	
	float fresnel_term = pow(1.0 - max(dot(NORMAL, VIEW), 0.0), fresnel_power);
	vec3 fresnel_outline = mix(ALBEDO, vec3(0.0), fresnel_term);
	
	float fade_factor = dot(NORMAL, normalize(-VERTEX.xyz));
    fade_factor = smoothstep(0.99, 1.0, fade_factor);
    vec3 color_with_fade = mix(ALBEDO, vec3(0.0), fade_factor);
	
	ALPHA = mix(0.0, g + fresnel_outline.x / 10.0, opacity) * color_with_fade.x;
	ALBEDO = color;
}