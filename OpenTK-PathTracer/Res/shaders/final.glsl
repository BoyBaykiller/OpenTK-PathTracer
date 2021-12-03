#version 430 core
layout(location = 0) out vec4 FragColor;

layout(binding = 0) uniform sampler2D SamplerTexture;

layout(location = 3) in struct
{
    vec2 TexCoord;
} inData;

void main()
{
    FragColor = texture(SamplerTexture, inData.TexCoord);
}