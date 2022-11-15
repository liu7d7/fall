#version 330 core

layout(location = 0) in vec3 aPos;

out vec4 vtColor;

uniform mat4 _proj;
uniform mat4 _lookAt;

void main() {
    gl_Position = vec4(aPos, 1.0) * _lookAt * _proj;
    vtColor = vec4(vec3(3), 1.0);
}