#version 430 core

in vec2 v_TexCoords;
in vec2 v_Pos;

uniform sampler2D _tex0;
uniform sampler2D _tex1;
uniform float _width;
uniform float _threshold;
uniform float _depthThreshold;
uniform vec4 _outlineColor;
uniform vec4 _depthOutlineColor;
uniform int _diffDepthCol;
uniform int _blackAndWhite;
uniform int _abs;
uniform int _glow;
uniform vec4 _otherColor;
uniform vec2 _screenSize;

out vec4 fragColor;

float linearize_depth(float d, float zNear, float zFar) {
    return zNear * zFar / (zFar + d * (zNear - zFar));
}

float depthAt(vec2 pos) {
    float depth = texture(_tex1, pos).r;
    return linearize_depth(depth, 0.1, 38.4);
}

const float sqrt2 = 1.0 / sqrt(2.);

int shouldOutline(vec2 pos, vec4 center, float depth) {
    vec2 oneTexel = 1. / _screenSize;
    float diag = _width * sqrt2;
    vec2 corners[8] = {
    (pos.xy - diag) * oneTexel,
    (vec2(pos.x + diag, pos.y - diag)) * oneTexel,
    (vec2(pos.x - diag, pos.y + diag)) * oneTexel,
    (pos.xy + diag) * oneTexel,
    (vec2(pos.x - _width, pos.y)) * oneTexel,
    (vec2(pos.x + _width, pos.y)) * oneTexel,
    (vec2(pos.x, pos.y - _width)) * oneTexel,
    (vec2(pos.x, pos.y + _width)) * oneTexel
    };
    float diff;
    for (int i = 0; i < 8; i++) {
        if (corners[i].x < 0 || corners[i].x > 1 || corners[i].y < 0 || corners[i].y > 1) {
            continue;
        }
        if (_abs == 0) {
            diff = -(center.r - texture(_tex0, corners[i]).r)
            -(center.g - texture(_tex0, corners[i]).g)
            -(center.b - texture(_tex0, corners[i]).b);
        } else {
            diff = abs(center.r - texture(_tex0, corners[i]).r)
            + abs(center.g - texture(_tex0, corners[i]).g)
            + abs(center.b - texture(_tex0, corners[i]).b);
        }
        if (abs(depth - depthAt(corners[i])) > _depthThreshold || diff > _threshold) {
            return 1;
        }
    }
    return 0;
}

void main() {
    vec2 oneTexel = 1. / _screenSize;
    vec4 center = texture(_tex0, v_TexCoords);
    int o = shouldOutline(v_Pos, center, depthAt(v_TexCoords));
    if (_blackAndWhite == 1) {
        if (o == 1) {
            fragColor = _outlineColor;
        } else {
            fragColor = _otherColor;
        }
    } else {
        if (o == 1) {
            fragColor = _outlineColor;
        } else {
            fragColor = center;
        }
    }
}