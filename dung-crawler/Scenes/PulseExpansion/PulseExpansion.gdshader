shader_type spatial;
render_mode blend_mix, depth_draw_never, cull_disabled, unshaded;

uniform vec4 shine_color : source_color = vec4(0.0, 1.0, 1.0, 1.0);
uniform float intersection_thickness : hint_range(0.0, 2.0) = 0.5;
uniform float sphere_opacity : hint_range(0.0, 1.0) = 0.0;

uniform sampler2D depth_texture : hint_depth_texture, filter_linear_mipmap;

void fragment() {
    // Base sphere transparency
    ALBEDO = shine_color.rgb;
    ALPHA = sphere_opacity;
    
    // Calculate intersection with environment depth
    float depth = texture(depth_texture, SCREEN_UV).x;
    vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth);
    vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
    view.xyz /= view.w;
    float linear_depth = -view.z;
    
    // Difference between mesh surface and environment depth
    float depth_diff = linear_depth + VERTEX.z;
    
    // If close to geometry, increase brightness and opacity
    if (depth_diff > 0.0 && depth_diff < intersection_thickness) {
        float fade = 1.0 - (depth_diff / intersection_thickness);
        ALPHA = max(ALPHA, fade);
        ALBEDO = shine_color.rgb * (1.0 + fade * 5.0); // Glow effect
    }
}