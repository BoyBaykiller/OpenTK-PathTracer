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
bool RaySphereIntersect(Ray ray, vec3 position, float radius, out float t1, out float t2);
Ray GetWorldSpaceRay(mat4 inverseProj, mat4 inverseView, vec3 viewPos, vec2 normalizedDeviceCoors);
bool IsInside(vec2 pos, vec2 size);


const vec3 PlanetPos = vec3(0, -800, 0);
const float PlanetRad = 600;
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

vec3 CalculateScattering()
{
    vec3 rayleighScattering = vec3(0.0);
    vec3 mieScattering = vec3(0.0);

    
}

bool RaySphereIntersect(Ray ray, vec3 position, float radius, out float t1, out float t2)
{
    // Source: https://antongerdelan.net/opengl/raycasting.html
    t1 = FLOAT_MAX; t2 = FLOAT_MAX;

    vec3 sphereToRay = ray.Origin - position;
    float b = dot(ray.Direction, sphereToRay);
    float c = dot(sphereToRay, sphereToRay) - radius * radius;
    float discriminant = b * b - c;
    if (discriminant < 0)
        return false;

    float squareRoot = sqrt(discriminant);
    t1 = -b - squareRoot;
    t2 = -b + squareRoot;

    return true;
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