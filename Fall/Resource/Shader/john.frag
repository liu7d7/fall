#version 430 core

out vec4 color;

uniform sampler2D _tex0;
uniform int _renderingRed;

in vec4 v_Color;
in vec2 v_TexCoords;

void main() {
    if (_renderingRed == 1) {
        color = v_Color * vec4(1.0, 1.0, 1.0, texture(_tex0, v_TexCoords).r);
        return;
    }
    color = v_Color;
}
