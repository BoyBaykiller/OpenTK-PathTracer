#version 450 core
layout(location = 0) out vec4 FragColor;

layout(binding = 0) uniform sampler2D SamplerTexture;

in InOutVars
{
    vec2 TexCoord;
} inData;

void main()
{
    FragColor = texture(SamplerTexture, inData.TexCoord);
}