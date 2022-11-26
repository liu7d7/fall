#version 450

layout(location = 0) in vec3 pos_width;
layout(location = 1) in vec4 col;

layout(location = 0) uniform mat4 _lookAt;
layout(location = 1) uniform mat4 _proj;

out vec4 v_col;
out noperspective float v_line_width;

void main()
{
    v_col = col;
    v_line_width = 1.75;
    gl_Position = vec4(pos_width.xyz, 1.0) * _lookAt * _proj;
}