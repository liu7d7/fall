#version 150

uniform sampler2D _tex0;

in vec2 texCoord;
in vec2 oneTexel;

uniform vec2 _screenSize;
uniform vec2 _blurDir;
uniform float _radius;

out vec4 fragColor;

void main() {
    vec4 blurred = vec4(0.0);
    float totalStrength = 0.0;
    float totalAlpha = 0.0;
    float totalSamples = 0.0;
    for (float r = -_radius; r <= _radius; r += 1.0) {
        vec4 sampleValue = texture(_tex0, texCoord + oneTexel * r * _blurDir);

        // Accumulate average alpha
        totalAlpha = totalAlpha + sampleValue.a;
        totalSamples = totalSamples + 1.0;

        // Accumulate smoothed blur
        float strength = 1.0 - abs(r / _radius);
        totalStrength = totalStrength + strength;
        blurred = blurred + sampleValue;
    }
    fragColor = vec4(blurred.rgb / (_radius * 2.0 + 1.0), totalAlpha);
}
