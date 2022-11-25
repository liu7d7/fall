#version 430 core

layout(location = 0) in vec3 pos;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 texCoords;
layout(location = 3) in vec4 color;

uniform mat4 _proj;
uniform mat4 _lookAt;
uniform int _rendering3d;

out vec4 v_Color;
out vec3 v_Normal;
out vec2 v_TexCoords;
out vec3 v_FragPos;

void main() {
    vec4 final = vec4(pos, 1.0) * _lookAt * _proj;
    gl_Position = final;
    v_TexCoords = texCoords;
    v_Color = color;
    v_Normal = normal;
    v_FragPos = pos;
}