#version 430 core

layout(location = 0) in vec3 pos;
layout(location = 1) in vec2 color;

out vec4 v_Color;
out vec2 v_TexCoords;

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
    v_Color = vec4(vec3(color.r), color.g);
}