#version 430 core

layout(local_size_x = 32, local_size_y = 1, local_size_z = 1) in;

layout(binding = 0, rgba8) restrict readonly uniform image2D ImgSrc;
layout(binding = 1, rgba8) restrict writeonly uniform image2D ImgDest;

const float WEIGHTS[5] = float[](0.382928, 0.241732, 0.060598, 0.005977, 0.000229);

vec3 GaussianBlur();
vec3 BoxBlur();
bool IsInside(vec2 pos, vec2 size);

layout (location = 0) uniform bool horizontal;

ivec2 imgCoord;

void main()
{
    ivec2 srcSize = imageSize(ImgSrc);
    //imgCoord = ivec2(gl_GlobalInvocationID.xy);
    imgCoord = ivec2(gl_GlobalInvocationID.x / srcSize.y, gl_GlobalInvocationID.x % srcSize.y);

    if (!IsInside(imgCoord, srcSize))
        return;

    imageStore(ImgDest, imgCoord, vec4(GaussianBlur(), 1.0));    
}

vec3 GaussianBlur()
{
    vec3 result = imageLoad(ImgSrc, imgCoord).rgb * WEIGHTS[0];
    if (horizontal)
    {
        result += imageLoad(ImgSrc, ivec2(imgCoord.x + 1, imgCoord.y)).rgb * WEIGHTS[1];
        result += imageLoad(ImgSrc, ivec2(imgCoord.x - 1, imgCoord.y)).rgb * WEIGHTS[1];

        result += imageLoad(ImgSrc, ivec2(imgCoord.x + 2, imgCoord.y)).rgb * WEIGHTS[2];
        result += imageLoad(ImgSrc, ivec2(imgCoord.x - 2, imgCoord.y)).rgb * WEIGHTS[2];

        result += imageLoad(ImgSrc, ivec2(imgCoord.x + 3, imgCoord.y)).rgb * WEIGHTS[3];
        result += imageLoad(ImgSrc, ivec2(imgCoord.x - 3, imgCoord.y)).rgb * WEIGHTS[3];

        result += imageLoad(ImgSrc, ivec2(imgCoord.x + 4, imgCoord.y)).rgb * WEIGHTS[4];
        result += imageLoad(ImgSrc, ivec2(imgCoord.x - 4, imgCoord.y)).rgb * WEIGHTS[4];
    }
    else 
    {
        result += imageLoad(ImgSrc, ivec2(imgCoord.x, imgCoord.y + 1)).rgb * WEIGHTS[1];
        result += imageLoad(ImgSrc, ivec2(imgCoord.x, imgCoord.y - 1)).rgb * WEIGHTS[1];

        result += imageLoad(ImgSrc, ivec2(imgCoord.x, imgCoord.y + 2)).rgb * WEIGHTS[2];
        result += imageLoad(ImgSrc, ivec2(imgCoord.x, imgCoord.y - 2)).rgb * WEIGHTS[2];

        result += imageLoad(ImgSrc, ivec2(imgCoord.x, imgCoord.y + 3)).rgb * WEIGHTS[3];
        result += imageLoad(ImgSrc, ivec2(imgCoord.x, imgCoord.y - 3)).rgb * WEIGHTS[3];

        result += imageLoad(ImgSrc, ivec2(imgCoord.x, imgCoord.y + 4)).rgb * WEIGHTS[4];
        result += imageLoad(ImgSrc, ivec2(imgCoord.x, imgCoord.y - 4)).rgb * WEIGHTS[4];
    }

    return result;
}

vec3 BoxBlur()
{
    vec3 result = 
    imageLoad(ImgSrc, ivec2(imgCoord.x - 1, imgCoord.y + 1)).rgb +
    imageLoad(ImgSrc, ivec2(imgCoord.x, imgCoord.y + 1)).rgb +
    imageLoad(ImgSrc, ivec2(imgCoord.x + 1, imgCoord.y + 1)).rgb +
    
    imageLoad(ImgSrc, ivec2(imgCoord.x - 1, imgCoord.y)).rgb +
    imageLoad(ImgSrc, imgCoord).rgb +
    imageLoad(ImgSrc, ivec2(imgCoord.x + 1, imgCoord.y)).rgb +
    
    imageLoad(ImgSrc, ivec2(imgCoord.x - 1, imgCoord.y - 1)).rgb +
    imageLoad(ImgSrc, ivec2(imgCoord.x, imgCoord.y - 1)).rgb +
    imageLoad(ImgSrc, ivec2(imgCoord.x + 1, imgCoord.y - 1)).rgb;
    
    return result / 9.0;
}

bool IsInside(vec2 pos, vec2 size)
{
    return pos.x < size.x && pos.y < size.y;
}