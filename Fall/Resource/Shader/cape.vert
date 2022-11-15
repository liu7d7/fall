#version 330 core

layout (location = 0) in vec3 pos;
layout (location = 1) in vec2 tier;

out vec4 vtColor;

uniform float _time;
uniform mat4 _proj;
uniform mat4 _lookAt;
uniform vec3 _translation;
uniform vec4 _color;

float curve(float f) {
    return pow(f, 1.6);
}

float func(float x) {
    return (sin(x) + sin(2 * x)) / 2.5;
}

void main() {
    vec3 final = pos;
    float x = (tier.x * 0.25 + _time * (sin(_time) * 0.5 + 4));
    
    
    float mul = (func(x) * 0.25 + 1.25);
    float h = 5.66;
    final.xz *= abs(curve((tier.y - h) / h) * 10) * mul + 0.6;
    
    
    
    float muly = (func(x) * 0.375 + 1);
    final.y += abs(curve((tier.y - h) / h) * 3) * muly;
    
    
    
    final += _translation;
    gl_Position = vec4(final, 1) * _lookAt * _proj;
    vtColor = vec4(_color.rgb, 1);
}