#version 430 core
#extension GL_ARB_bindless_texture : require
#define FLOAT_MAX 3.4028235e+38
#define FLOAT_MIN -3.4028235e+38
#define EPSILON 0.001
#define PI 3.1415926535898

layout(local_size_x = 32, local_size_y = 1, local_size_z = 1) in;

layout(binding = 0, rgba32f) restrict uniform image2D ImgResult;
layout(binding = 1) uniform samplerCube SamplerEnvironment;

struct Material {
    vec3 Albedo; // Base color
    float SpecularChance; // How reflective
    
    vec3 Emissiv; // How much light is emitted
    float SpecularRoughness; // How rough reflections are

    vec3 Absorbance; // How strongly light is absorbed
    float RefractionChance; // How transparent

    float RefractionRoughness; // How rough refractions are
    float IOR; // How strongly light gets refracted and the amout of light that is reflected
};

struct Cuboid {
    vec3 Min;
    vec3 Max;
    
    Material Material;
};

struct Sphere {
    vec3 Position;
    float Radius;

    Material Material;
};

struct HitInfo {
    float T;
    bool FromInside;
    vec3 NearHitPos;
    vec3 Normal;
    Material Material;
};

struct Ray {
    vec3 Origin;
    vec3 Direction;
};

layout(std140, binding = 0) uniform BasicDataUBO
{
	mat4 Projection;
    mat4 InvProjection;
    vec2 NearFar;
	mat4 View;
	mat4 InvView;
    mat4 ProjectionView;
	vec3 ViewPos;
    vec3 ViewDir;
} basicDataUBO;

layout(std140, binding = 1) uniform GameObjectsUBO
{
    Sphere Spheres[256];
	Cuboid Cuboids[64];
} gameObjectsUBO;

vec3 PathTrace(Ray ray);
bool GetClosestIntersectingRayObject(Ray ray, out HitInfo hitInfo);
bool GetClosestIntersectingLight(Ray ray, out HitInfo hitInfo);
bool RaySphereIntersect(Ray ray, vec3 position, float radius, out float t1, out float t2);
bool RayCuboidIntersect(Ray ray, vec3 aabbMin, vec3 aabbMax, out float t1, out float t2);
vec3 GetNormal(vec3 spherePos, vec3 surfacePosition);
vec3 GetNormal(vec3 aabbMin, vec3 aabbMax, vec3 surfacePosition);
vec3 GetCosWeightedHemissphereDir(inout uint rndSeed, vec3 normal);
vec2 GetPointOnCircle(inout uint rndSeed);
uint GetWangHash(inout uint seed);
float GetRandomFloat01(inout uint state);
float GetSmallestPositive(float t1, float t2);
float FresnelReflectAmount(float n1, float n2, vec3 normal, vec3 incident, float f0);
float Fresnel(float n1, float n2, float cosTheta, float minReflect);
vec3 LessThan(vec3 f, float value);
vec3 InverseGammaToLinear(vec3 rgb);
Ray GetWorldSpaceRay(mat4 inverseProj, mat4 inverseView, vec3 viewPos, vec2 normalizedDeviceCoords);

uniform vec2 uboGameObjectsSize;

uniform int rayDepth;
uniform int SSP;

uniform float focalLength;
uniform float apertureDiameter;

layout(location = 0) uniform int thisRendererFrame;

uint rndSeed;
void main()
{
    ivec2 imgResultSize = imageSize(ImgResult);
    ivec2 imgCoord = ivec2(gl_GlobalInvocationID.x / imgResultSize.y, gl_GlobalInvocationID.x % imgResultSize.y);

    rndSeed = gl_GlobalInvocationID.x * 1973 + gl_GlobalInvocationID.y * 9277 + thisRendererFrame * 2699 | 1;
    vec3 color = vec3(0);
    for (int i = 0; i < SSP; i++)
    {   
        vec2 subPixelOffset = vec2(GetRandomFloat01(rndSeed), GetRandomFloat01(rndSeed)) - 0.5; // integrating over whole pixel eliminates aliasing
        vec2 ndc = (imgCoord + subPixelOffset) / imgResultSize * 2.0 - 1.0;
        Ray rayEyeToWorld = GetWorldSpaceRay(basicDataUBO.InvProjection, basicDataUBO.InvView, basicDataUBO.ViewPos, ndc);

        vec3 focalPoint = rayEyeToWorld.Origin + rayEyeToWorld.Direction * focalLength;
        vec2 offset = apertureDiameter * 0.5 * GetPointOnCircle(rndSeed);
        
        rayEyeToWorld.Origin = (basicDataUBO.InvView * vec4(offset, 0.0, 1.0)).xyz;
        rayEyeToWorld.Direction = normalize(focalPoint - rayEyeToWorld.Origin);

        color += PathTrace(rayEyeToWorld);
    }
    color /= SSP;
    vec3 lastFrameColor = imageLoad(ImgResult, imgCoord).rgb;

    color = mix(lastFrameColor, color, 1.0 / (thisRendererFrame + 1));
    imageStore(ImgResult, imgCoord, vec4(color, 1.0));
}

vec3 PathTrace(Ray ray)
{
    vec3 throughPut = vec3(1);
    vec3 ret = vec3(0);
    HitInfo hitInfo;
    for (int i = 0; i < rayDepth; i++)
    {
        if (GetClosestIntersectingRayObject(ray, hitInfo))
        {
            if (hitInfo.FromInside)
            {
                hitInfo.Normal *= -1.0;
                throughPut *= exp(-hitInfo.Material.Absorbance * hitInfo.T);
            }

            float specularChance = hitInfo.Material.SpecularChance;
            float refractionChance = hitInfo.Material.RefractionChance;
            if (specularChance > 0.0)
            {
                specularChance = FresnelReflectAmount(
                hitInfo.FromInside ? hitInfo.Material.IOR : 1.0, 
                !hitInfo.FromInside ? hitInfo.Material.IOR : 1.0,
                ray.Direction, 
                hitInfo.Normal, 
                specularChance);

                float chanceMultiplier = (1.0 - specularChance) / (1.0 - hitInfo.Material.SpecularChance);
                refractionChance *= chanceMultiplier;
            }
            

            float rayProbability = 1.0;
            float doSpecular = 0.0;
            float doRefraction = 0.0;
            float raySelectRoll = GetRandomFloat01(rndSeed);
            if (specularChance > raySelectRoll)
            {
                doSpecular = 1.0;
                rayProbability = specularChance;
            }
            else if (specularChance + refractionChance > raySelectRoll)
            {
                doRefraction = 1.0;
                rayProbability = refractionChance;
            }
            else
            {
                rayProbability = 1.0 - (specularChance + refractionChance);
            }
            

            vec3 diffuseRayDir = GetCosWeightedHemissphereDir(rndSeed, hitInfo.Normal);
            vec3 specularRayDir = normalize(mix(reflect(ray.Direction, hitInfo.Normal), diffuseRayDir, hitInfo.Material.SpecularRoughness * hitInfo.Material.SpecularRoughness));
            vec3 refractionRayDir = refract(ray.Direction, hitInfo.Normal, hitInfo.FromInside ? hitInfo.Material.IOR / 1.0 : 1.0 / hitInfo.Material.IOR);
            refractionRayDir = normalize(mix(refractionRayDir, GetCosWeightedHemissphereDir(rndSeed, -hitInfo.Normal), hitInfo.Material.RefractionRoughness * hitInfo.Material.RefractionRoughness));
            

            ray.Origin = hitInfo.NearHitPos + hitInfo.Normal * EPSILON * (doRefraction == 1.0 ? -1 : 1);
            ray.Direction = mix(diffuseRayDir, specularRayDir, doSpecular);
            ray.Direction = mix(ray.Direction, refractionRayDir, doRefraction);

            ret += hitInfo.Material.Emissiv * throughPut;
            if (doRefraction == 0.0)
                throughPut *= hitInfo.Material.Albedo;

            rayProbability = max(rayProbability, EPSILON);
            throughPut /= rayProbability;
            
            
            float p = max(throughPut.x, max(throughPut.y, throughPut.z));
            if (GetRandomFloat01(rndSeed) > p)
                break;

            throughPut *= 1.0 / p;
        }
        else
        {
            ret += texture(SamplerEnvironment, ray.Direction).rgb * throughPut;
            break;
        }  
    }
    return ret;
}

bool GetClosestIntersectingRayObject(Ray ray, out HitInfo hitInfo)
{
    hitInfo.T = FLOAT_MAX;
    float t1, t2;

    for (int i = 0; i < uboGameObjectsSize.x; i++)
    {
        vec3 pos = gameObjectsUBO.Spheres[i].Position;
        float radius = gameObjectsUBO.Spheres[i].Radius;
        if (RaySphereIntersect(ray, pos, radius, t1, t2) && t2 > 0 && t1 < hitInfo.T)
        {
            hitInfo.T = GetSmallestPositive(t1, t2);
            hitInfo.FromInside = hitInfo.T == t2;
            hitInfo.Material = gameObjectsUBO.Spheres[i].Material;
            hitInfo.NearHitPos = ray.Origin + ray.Direction * hitInfo.T;
            hitInfo.Normal = GetNormal(pos, hitInfo.NearHitPos);
        }
    }
    
    for (int i = 0; i < uboGameObjectsSize.y; i++)
    {
        vec3 aabbMin = gameObjectsUBO.Cuboids[i].Min;
        vec3 aabbMax = gameObjectsUBO.Cuboids[i].Max;
        if (RayCuboidIntersect(ray, aabbMin, aabbMax, t1, t2) && t2 > 0 && t1 < hitInfo.T)
        {
            hitInfo.T = GetSmallestPositive(t1, t2);
            hitInfo.FromInside = hitInfo.T == t2;
            hitInfo.Material = gameObjectsUBO.Cuboids[i].Material;
            hitInfo.NearHitPos = ray.Origin + ray.Direction * hitInfo.T;
            hitInfo.Normal = GetNormal(aabbMin, aabbMax, hitInfo.NearHitPos);
        }
    }

    return hitInfo.T != FLOAT_MAX;
}

bool RaySphereIntersect(Ray ray, vec3 position, float radius, out float t1, out float t2)
{
    // Source: https://antongerdelan.net/opengl/raycasting.html
    t1 = t2 = FLOAT_MAX;

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

bool RayCuboidIntersect(Ray ray, vec3 aabbMin, vec3 aabbMax, out float t1, out float t2)
{
    // Source: https://medium.com/@bromanz/another-view-on-the-classic-ray-aabb-intersection-algorithm-for-bvh-traversal-41125138b525
    t1 = FLOAT_MIN;
    t2 = FLOAT_MAX;

    vec3 t0s = (aabbMin - ray.Origin) * (1.0 / ray.Direction);
    vec3 t1s = (aabbMax - ray.Origin) * (1.0 / ray.Direction);

    vec3 tsmaller = min(t0s, t1s);
    vec3 tbigger = max(t0s, t1s);

    t1 = max(t1, max(tsmaller.x, max(tsmaller.y, tsmaller.z)));
    t2 = min(t2, min(tbigger.x, min(tbigger.y, tbigger.z)));
    return t1 <= t2;
}

vec3 GetNormal(vec3 spherePos, vec3 surfacePosition)
{
    return normalize(surfacePosition - spherePos);
}

vec3 GetNormal(vec3 aabbMin, vec3 aabbMax, vec3 surfacePosition)
{
    // Source: https://gist.github.com/Shtille/1f98c649abeeb7a18c5a56696546d3cf
    // step(edge,x) : x < edge ? 0 : 1

    vec3 halfSize = (aabbMax - aabbMin) * 0.5;
    vec3 centerSurface = surfacePosition - (aabbMax + aabbMin) * 0.5;
    
    vec3 normal = vec3(0.0);
    normal += vec3(sign(centerSurface.x), 0.0, 0.0) * step(abs(abs(centerSurface.x) - halfSize.x), EPSILON);
    normal += vec3(0.0, sign(centerSurface.y), 0.0) * step(abs(abs(centerSurface.y) - halfSize.y), EPSILON);
    normal += vec3(0.0, 0.0, sign(centerSurface.z)) * step(abs(abs(centerSurface.z) - halfSize.z), EPSILON);
    return normalize(normal);
}

vec3 GetCosWeightedHemissphereDir(inout uint rndSeed, vec3 normal)
{
    float z = GetRandomFloat01(rndSeed) * 2.0 - 1.0; // ZPosition: Map from [0, 1] to [-1, 1]
    float a = GetRandomFloat01(rndSeed) * 2.0 * PI; // Angle (radians): Map from [0, 1] to [0, 2Pi]
    float r = (1.0 - z * z); // Radius at ZPosition. Mathematically correct would be: sqrt(1 - z * z)
    float x = r * cos(a);
    float y = r * sin(a);

    // Convert unit vector in sphere to a cosine weighted vector in hemissphere
    return normalize(normal + vec3(x, y, z));
}

vec2 GetPointOnCircle(inout uint rndSeed)
{
    float angle = GetRandomFloat01(rndSeed) * 2.0 * PI;
    float r = sqrt(GetRandomFloat01(rndSeed));
    return vec2(cos(angle), sin(angle)) * r;
}

uint GetPCGHash(inout uint seed)
{
    seed = seed * 747796405u + 2891336453u;
    uint word = ((seed >> ((seed >> 28u) + 4u)) ^ seed) * 277803737u;
    return (word >> 22u) ^ word;
}
 
float GetRandomFloat01(inout uint state)
{
    return float(GetPCGHash(state)) / 4294967296.0;
}

float GetSmallestPositive(float t1, float t2)
{
    // Assumes at least one float > 0
    return t1 < 0 ? t2 : t1;
}

float FresnelReflectAmount(float n1, float n2, vec3 normal, vec3 incident, float f0)
{
    // Schlick aproximation
    float r0 = (n1 - n2) / (n1 + n2);
    r0 *= r0;
    float cosX = -dot(normal, incident);
    if (n1 > n2)
    {
        float n = n1 / n2;
        float sinT2 = n * n * (1.0 - cosX * cosX);
        // Total internal reflection
        if (sinT2 > 1.0)
            return 1.0;
        cosX = sqrt(1.0 - sinT2);
    }
    float x = 1.0 - cosX;
    float ret = r0 + (1.0 - r0) * x * x * x * x * x;

    return mix(f0, 1.0, ret);
}

vec3 InverseGammaToLinear(vec3 rgb)
{
    return mix(pow(((rgb + 0.055) / 1.055), vec3(2.4)), rgb / 12.92, vec3(lessThan(rgb, vec3(0.04045))));
}

Ray GetWorldSpaceRay(mat4 inverseProj, mat4 inverseView, vec3 viewPos, vec2 normalizedDeviceCoords)
{
    vec4 rayEye = inverseProj * vec4(normalizedDeviceCoords.xy, -1.0, 0.0);
    rayEye.zw = vec2(-1.0, 0.0);
    return Ray(viewPos, normalize((inverseView * rayEye).xyz));
}
