#version 430 core

layout(location = 0) in vec3 pos;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 texCoord;
layout(location = 3) in vec4 color;

out vec4 v_Color;
out vec3 v_Normal;
out vec2 v_TexCoords;
out vec3 v_FragPos;

#define MAX 1024

layout(binding = 0, packed) uniform _instanceInfo {
    mat4 _model[MAX];
};
uniform mat4 _proj;
uniform mat4 _lookAt;

void main() {
    mat4 model = _model[gl_InstanceID];
    vec4 final = model * vec4(pos, 1.0);
    gl_Position = final * _lookAt * _proj;
    v_TexCoords = texCoord;
    v_Color = color;
    v_Normal = normal;
    v_FragPos = final.xyz;
}