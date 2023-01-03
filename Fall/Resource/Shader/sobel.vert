#version 150

in vec2 pos;

uniform vec2 _screenSize;

out vec2 texCoord;
out vec2 oneTexel;

void main() {
    gl_Position = vec4(pos, 0.2, 1.0);

    oneTexel = 1. / _screenSize;
    texCoord = pos * 0.5 + 0.5;
}
