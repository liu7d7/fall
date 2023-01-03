#version 450

layout(lines) in;
layout(triangle_strip, max_vertices = 12) out;

layout(location = 2) uniform vec2 _screenSize;
layout(location = 3) uniform vec2 _radius;

in vec4 v_col[];
in noperspective float v_line_width[];

out vec4 g_col;
out noperspective float g_line_width;
out noperspective float g_line_length;
out noperspective float g_u;
out noperspective float g_v;
out vec2 g_compare;
out vec2 g_pos;

void main()
{
    float u_width        = _screenSize[0];
    float u_height       = _screenSize[1];
    float u_aspect_ratio = u_height / u_width;

    vec2 ndc_a = gl_in[0].gl_Position.xy / gl_in[0].gl_Position.w;
    vec2 ndc_b = gl_in[1].gl_Position.xy / gl_in[1].gl_Position.w;

    vec2 line_vector = ndc_b - ndc_a;
    vec2 viewport_line_vector = line_vector * _screenSize;
    vec2 dir = normalize(vec2(line_vector.x, line_vector.y * u_aspect_ratio));

    float line_width_a     = max(1.0, v_line_width[0]) + _radius[0];
    float line_width_b     = max(1.0, v_line_width[1]) + _radius[0];
    float extension_length = 6;
    float line_length      = length(viewport_line_vector);

    vec2 normal    = vec2(-dir.y, dir.x);
    vec2 normal_a  = vec2(line_width_a/u_width, line_width_a/u_height) * normal;
    vec2 normal_b  = vec2(line_width_b/u_width, line_width_b/u_height) * normal;
    vec2 extension = vec2(extension_length / u_width, extension_length / u_height) * dir;

    g_compare = vec2(-1);

    g_col = vec4(v_col[0].rgb, v_col[0].a * min(v_line_width[0], 1.0f));
    g_u = line_width_a;
    g_v = line_length * 0.5;
    g_line_width = line_width_a;
    g_line_length = line_length * 0.5;
    gl_Position = vec4((ndc_a + normal_a) * gl_in[0].gl_Position.w, gl_in[0].gl_Position.zw);
    //    g_pos = gl_Position.xy;
    g_pos = ndc_a + normal_a;
    EmitVertex();

    g_u = -line_width_a;
    g_v = line_length * 0.5;
    g_line_width = line_width_a;
    g_line_length = line_length * 0.5;
    gl_Position = vec4((ndc_a - normal_a) * gl_in[0].gl_Position.w, gl_in[0].gl_Position.zw);
    //    g_pos = gl_Position.xy;
    g_pos = ndc_a - normal_a;
    EmitVertex();

    g_u = line_width_b;
    g_v = -line_length * 0.5;
    g_line_width = line_width_b;
    g_line_length = line_length * 0.5;
    gl_Position = vec4((ndc_b + normal_b) * gl_in[1].gl_Position.w, gl_in[1].gl_Position.zw);
    //    g_pos = gl_Position.xy;
    g_pos = ndc_b + normal_b;
    EmitVertex();

    g_u = -line_width_b;
    g_v = -line_length * 0.5;
    g_line_width = line_width_b;
    g_line_length = line_length * 0.5;
    gl_Position = vec4((ndc_b - normal_b) * gl_in[1].gl_Position.w, gl_in[1].gl_Position.zw);
    //    g_pos = gl_Position.xy;
    g_pos = ndc_b - normal_b;
    EmitVertex();

    EndPrimitive();

    g_compare = ndc_a;

    g_col = vec4(v_col[0].rgb, v_col[0].a * min(v_line_width[0], 1.0f));
    g_u = line_width_a;
    g_v = line_length * 0.5;
    g_line_width = line_width_a;
    g_line_length = line_length * 0.5;
    gl_Position = vec4((ndc_a + normal_a - extension) * gl_in[0].gl_Position.w, gl_in[0].gl_Position.zw);
    //    g_pos = gl_Position.xy;
    g_pos = ndc_a + normal_a - extension;
    EmitVertex();

    g_u = -line_width_a;
    g_v = line_length * 0.5;
    g_line_width = line_width_a;
    g_line_length = line_length * 0.5;
    gl_Position = vec4((ndc_a - normal_a - extension) * gl_in[0].gl_Position.w, gl_in[0].gl_Position.zw);
    //    g_pos = gl_Position.xy;
    g_pos = ndc_a - normal_a - extension;
    EmitVertex();

    g_u = line_width_b;
    g_v = -line_length * 0.5;
    g_line_width = line_width_b;
    g_line_length = line_length * 0.5;
    gl_Position = vec4((ndc_a + normal_a) * gl_in[0].gl_Position.w, gl_in[0].gl_Position.zw);
    //    g_pos = gl_Position.xy;
    g_pos = ndc_a + normal_a;
    EmitVertex();

    g_u = -line_width_b;
    g_v = -line_length * 0.5;
    g_line_width = line_width_b;
    g_line_length = line_length * 0.5;
    gl_Position = vec4((ndc_a - normal_a) * gl_in[0].gl_Position.w, gl_in[0].gl_Position.zw);
    //    g_pos = gl_Position.xy;
    g_pos = ndc_a - normal_a;
    EmitVertex();
    EndPrimitive();

    g_compare = ndc_b;

    g_col = vec4(v_col[0].rgb, v_col[0].a * min(v_line_width[0], 1.0f));
    g_u = line_width_a;
    g_v = line_length * 0.5;
    g_line_width = line_width_a;
    g_line_length = line_length * 0.5;
    gl_Position = vec4((ndc_b + normal_b + extension) * gl_in[1].gl_Position.w, gl_in[1].gl_Position.zw);
    //    g_pos = gl_Position.xy;
    g_pos = ndc_b + normal_b + extension;
    EmitVertex();

    g_u = -line_width_a;
    g_v = line_length * 0.5;
    g_line_width = line_width_a;
    g_line_length = line_length * 0.5;
    gl_Position = vec4((ndc_b - normal_b + extension) * gl_in[1].gl_Position.w, gl_in[1].gl_Position.zw);
    //    g_pos = gl_Position.xy;
    g_pos = ndc_b - normal_b + extension;
    EmitVertex();

    g_u = line_width_b;
    g_v = -line_length * 0.5;
    g_line_width = line_width_b;
    g_line_length = line_length * 0.5;
    gl_Position = vec4((ndc_b + normal_b) * gl_in[1].gl_Position.w, gl_in[1].gl_Position.zw);
    //    g_pos = gl_Position.xy;
    g_pos = ndc_b + normal_b;
    EmitVertex();

    g_u = -line_width_b;
    g_v = -line_length * 0.5;
    g_line_width = line_width_b;
    g_line_length = line_length * 0.5;
    gl_Position = vec4((ndc_b - normal_b) * gl_in[1].gl_Position.w, gl_in[1].gl_Position.zw);
    //    g_pos = gl_Position.xy;
    g_pos = ndc_b - normal_b;
    EmitVertex();
    EndPrimitive();
}