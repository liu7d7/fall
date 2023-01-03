#version 150

uniform sampler2D _tex0;

in vec2 texCoord;
in vec2 oneTexel;

uniform float _radius;

out vec4 fragColor;

void main(){
    vec4 c  = texture(_tex0, texCoord);
    vec4 maxVal = c;
    for (float u = 0.0; u <= _radius; u += 1.0) {
        for (float v = 0.0; v <= _radius; v += 1.0) {
            float weight = (((sqrt(u * u + v * v) / (_radius)) > 1.0) ? 0.0 : 1.0);

            vec4 s0 = texture(_tex0, texCoord + vec2(-u * oneTexel.x, -v * oneTexel.y));
            vec4 s1 = texture(_tex0, texCoord + vec2(u * oneTexel.x, v * oneTexel.y));
            vec4 s2 = texture(_tex0, texCoord + vec2(-u * oneTexel.x, v * oneTexel.y));
            vec4 s3 = texture(_tex0, texCoord + vec2(u * oneTexel.x, -v * oneTexel.y));

            vec4 o0 = min(s0, s1);
            vec4 o1 = min(s2, s3);
            vec4 tempMin = min(o0, o1);
            maxVal = mix(maxVal, min(maxVal, tempMin), weight);
        }
    }

    fragColor = vec4(maxVal.rgb, 1.0);
}
