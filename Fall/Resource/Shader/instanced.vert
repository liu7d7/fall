#version 330 core

layout(location = 0) in vec3 pos;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 texCoord;
layout(location = 3) in vec4 color;

out vec4 v_Color;
out vec3 v_Normal;
out vec2 v_TexCoords;
out vec3 v_FragPos;

const int MAX = 128;

uniform mat4 _proj;
uniform mat4 _lookAt;
uniform mat4 _model[MAX];
uniform vec3 _translate[MAX];

void main() {
    mat4 model = _model[gl_InstanceID];
    vec3 translate = _translate[gl_InstanceID];
    vec4 final = vec4(pos + translate, 1.0) * model * _lookAt * _proj;
    gl_Position = final;
    v_TexCoords = texCoord;
    v_Color = color;
    v_Normal = normal;
    v_FragPos = pos + translate;
}