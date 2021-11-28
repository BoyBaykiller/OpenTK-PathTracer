#version 430 core
layout(location = 0) out vec4 FragColor;

layout(binding = 0) uniform sampler2D SamplerTexture;

in vec2 TexCoord;
void main()
{
    FragColor = texture(SamplerTexture, TexCoord);
}