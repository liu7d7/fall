#version 150

uniform sampler2D _tex0;
uniform sampler2D _tex1;

in vec2 texCoord;
in vec2 oneTexel;

out vec4 fragColor;

void main(){
    vec4 diffuseTexel = texture(_tex0, texCoord);
    vec4 outlineTexel = texture(_tex1, texCoord);
    fragColor = vec4(diffuseTexel.rgb + diffuseTexel.rgb * outlineTexel.rgb * vec3(0.75), 1.0);
}
