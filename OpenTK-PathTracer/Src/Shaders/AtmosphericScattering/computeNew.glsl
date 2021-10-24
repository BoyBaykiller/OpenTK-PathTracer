#version 430 core
#define FLOAT_MAX 3.4028235e+38
#define FLOAT_MIN -3.4028235e+38
#define EPSILON 0.0001
#define PI 3.1415926535898

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(binding = 0, rgba32f) uniform writeonly restrict imageCube ImgResult;

struct Ray 
{
    vec3 Origin;
    vec3 Direction;
};

layout (std140, binding = 3) uniform AtmosphericDataUBO
{
    mat4 InvProjection;
    mat4[6] InvView;
} atmoDataUBO;

vec3 CalculateScattering(Ray ray, int samples);
float AvgDensityOver(vec3 start, vec3 end, int samples);
float DensityAtPoint(vec3 point);
float RaySphereIntersect(Ray ray, vec3 position, float radius, out float t1, out float t2);
Ray GetWorldSpaceRay(mat4 inverseProj, mat4 inverseView, vec3 viewPos, vec2 normalizedDeviceCoors);
bool IsInside(vec2 pos, vec2 size);


const vec3 PLANET_POSITION = vec3(0, 0, 0);
const float PLANET_RADIUS = 600;
const vec3 LIGHT_DIR = vec3(0, -1, 0);
const float MIE_STRENGTH = 0.76;
const float MAX_RAYLEIGH_HEIGHT = 100;
const float MAX_MIE_HEIGHT = 100;
const float HEIGHT_ABSORBTION = 10000;
uniform float atmosphereRad;

uniform vec3 lightPos;
uniform vec3 viewPos;

uniform float densityFallOff;

uniform int inScatteringSamples;
uniform int densitySamples;
uniform float scatteringStrength;

uniform vec3 waveLengths;
vec3 ScatteringCoefficients;

void main()
{
    ivec2 imgResultSize = imageSize(ImgResult);
    ivec3 imgCoord = ivec3(gl_GlobalInvocationID);
    if (!IsInside(imgCoord.xy, imgResultSize))
        return;
    
    vec2 ndc = vec2(imgCoord.xy) / imgResultSize * 2.0 - 1.0;
    Ray rayEyeToWorld = GetWorldSpaceRay(atmoDataUBO.InvProjection, atmoDataUBO.InvView[imgCoord.z], viewPos, ndc);
    vec3 scattered = vec3(1.0);
    
    imageStore(ImgResult, imgCoord, vec4(scattered, 1.0));
}

vec3 CalculateScattering(Ray ray)
{
    vec3 rayleighScattering = vec3(0.0);
    vec3 mieScattering = vec3(0.0);

    float opticalDepthRayleigh = 0.0;
    float opticalDepthMie = 0.0;

    float cosTheta = dot(ray.Direction, LIGHT_DIR);
    float rayleigh = 3.0 * (1.0 + cosTheta * cosTheta) / (16.0 * PI);
    float mie = 3.0 * (1.0 - MIE_STRENGTH * MIE_STRENGTH) * (1.0 + u * u) / (8.0 * PI * (2.0 + MIE_STRENGTH * MIE_STRENGTH) * pow(1.0 + MIE_STRENGTH * MIE_STRENGTH - 2.0 * MIE_STRENGTH * u, 1.5));

    vec3 endPoint = ray.Origin + ray.Direction * RayAtmossphereIntersect(ray);
    vec3 deltaStep = (endPoint - ray.Origin) / inScatteringSamples;

    for (int i = 0; i < inScatteringSamples; i++)
    {
        // do some shit
        float height = length(ray.Origin) - PLANET_RADIUS;

        vec3 density = vec3(exp(-height / vec2(MAX_RAYLEIGH_HEIGHT, MAX_MIE_HEIGHT)), 0.0);

        float denom = ()

        for (int i = 0; < densitySamples; i++)
        {
            // do more shit
        }

        // combine all shit
        ray.Origin += deltaStep;
    }
}

float RayAtmossphereIntersect(Ray ray)
{
    vec3 sphereToRay = ray.Origin/* - PLANET_POSITION*/;
    float b = dot(ray.Direction, sphereToRay);
    float c = dot(sphereToRay, sphereToRay) - PLANET_RADIUS * PLANET_RADIUS;
    return -b + sqrt(b * b - c);
}

Ray GetWorldSpaceRay(mat4 inverseProj, mat4 inverseView, vec3 viewPos, vec2 normalizedDeviceCoors)
{
    vec4 rayEye = inverseProj * vec4(normalizedDeviceCoors.xy, -1.0, 0.0);
    rayEye.zw = vec2(-1.0, 0.0);
    return Ray(viewPos, normalize((inverseView * rayEye).xyz));
}

bool IsInside(vec2 pos, vec2 size)
{
    return pos.x < size.x && pos.y < size.y;
}