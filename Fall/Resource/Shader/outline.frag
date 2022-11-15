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

int shouldOutlineMemo[7][7] = {
    {-1, -1, -1, -1, -1, -1, -1},
    {-1, -1, -1, -1, -1, -1, -1},
    {-1, -1, -1, -1, -1, -1, -1},
    {-1, -1, -1, -1, -1, -1, -1},
    {-1, -1, -1, -1, -1, -1, -1},
    {-1, -1, -1, -1, -1, -1, -1},
    {-1, -1, -1, -1, -1, -1, -1},
};

const float sqrt2 = 1.0 / sqrt(2.0);

int shouldOutline(vec2 pos, vec4 center, float depth) {
    ivec2 pos1 = ivec2(pos - v_Pos);
    pos1 += int(_width * sqrt2);
    if (shouldOutlineMemo[int(pos1.x)][int(pos1.y)] != -1) {
        return shouldOutlineMemo[int(pos1.x)][int(pos1.y)];
    }
    float diag = _width * sqrt2;
    vec2 corners[8] = { (pos.xy - diag) / _screenSize,
                        (vec2(pos.x + diag, pos.y - diag)) / _screenSize,
                        (vec2(pos.x - diag, pos.y + diag)) / _screenSize,
                        (pos.xy + diag) / _screenSize,
                        (vec2(pos.x - _width, pos.y)) / _screenSize,
                        (vec2(pos.x + _width, pos.y)) / _screenSize,
                        (vec2(pos.x, pos.y - _width)) / _screenSize,
                        (vec2(pos.x, pos.y + _width)) / _screenSize };
    float diff;
    float maxDiff = 0;
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
        if (abs(depth - depthAt(corners[i])) > _depthThreshold) {
            shouldOutlineMemo[int(pos1.x)][int(pos1.y)] = 2;
            return 2;
        }
        if (diff > maxDiff) {
            maxDiff = diff;
            if (maxDiff > _threshold / 2.0) {
                break;
            }
        }
    }
    int ret = maxDiff > _threshold / 2.0 ? 1 : 0;
    shouldOutlineMemo[int(pos1.x)][int(pos1.y)] = ret;
    return ret;
}

void main() {
    vec4 center = texture(_tex0, v_TexCoords);
    float depth = depthAt(v_TexCoords);
    int o = shouldOutline(v_Pos, center, depth);
    int o2[4] = { shouldOutline(v_Pos + vec2(-1, -1), center, depth), 
                   shouldOutline(v_Pos + vec2(1, -1), center, depth), 
                   shouldOutline(v_Pos + vec2(-1, 1), center, depth), 
                   shouldOutline(v_Pos + vec2(1, 1), center, depth) };
    float o3 = 0;
    if (_glow == 1) {
        for (int i = 0; i < 4; i++) {
            if (o2[i] == 1 || o2[i] == 2) {
                o3 += 0.05;
            }
        }
    }
    if (_blackAndWhite == 1) {
        if (o == 1 || _diffDepthCol == 0 && o == 2) {
            fragColor = _outlineColor;
        } else if (o == 2) {
            fragColor = _depthOutlineColor;
        } else {
            fragColor = mix(_otherColor, _outlineColor, vec4(o3));
        }
    } else {
        if (o == 1 || _diffDepthCol == 0 && o == 2) {
            fragColor = _outlineColor;
        } else if (o == 2) {
            fragColor = _depthOutlineColor;
        } else {
            fragColor = mix(center, _outlineColor, vec4(o3));
        }
    }
}