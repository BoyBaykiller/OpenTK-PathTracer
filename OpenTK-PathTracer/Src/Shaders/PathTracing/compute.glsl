#version 430 core
#define FLOAT_MAX 3.4028235e+38
#define FLOAT_MIN -3.4028235e+38
#define EPSILON 0.001
#define PI 3.1415926586            
// Example shader include: #include PathTracing/fragCompute

layout(local_size_x = 32, local_size_y = 1, local_size_z = 1) in;

layout(binding = 0, rgba32f) restrict uniform image2D ImgResult;
layout(binding = 1) uniform samplerCube SamplerEnvironment;

struct Material 
{
    vec3 Albedo; // Base color
    float SpecularChance; // How reflective
    
    vec3 Emissiv; // How much light is emitted
    float SpecularRoughness; // How rough reflections are

    vec3 Absorbance; // How strongly light is absorbed
    float RefractionChance; // How transparent

    float RefractionRoughness; // How rough refractions are
    float IOR; // How strongly light gets refracted and the amout of light that is reflected
};

struct Cuboid 
{
    vec3 Min;
    vec3 Max;
    
    Material Material;
};

struct Sphere 
{
    vec3 Position;
    float Radius;

    Material Material;
};

struct HitInfo 
{
    float T;
    bool FromInside;
    vec3 NearHitPos;
    vec3 Normal;
    Material Material;
};

struct Ray 
{
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

vec3 Radiance(Ray ray);
float BSDF(inout Ray ray, HitInfo hitInfo, out bool isRefractive);
bool RayTrace(Ray ray, out HitInfo hitInfo);
bool RaySphereIntersect(Ray ray, vec3 position, float radius, out float t1, out float t2);
bool RayCuboidIntersect(Ray ray, vec3 aabbMin, vec3 aabbMax, out float t1, out float t2);
vec3 CosineSampleHemisphere(vec3 normal);
vec2 UniformSampleUnitCircle();
vec3 GetNormal(vec3 spherePos, float radius, vec3 surfacePosition);
vec3 GetNormal(vec3 aabbMin, vec3 aabbMax, vec3 surfacePosition);
uint GetPCGHash(inout uint seed);
float GetRandomFloat01();
float GetSmallestPositive(float t1, float t2);
Ray GetWorldSpaceRay(mat4 inverseProj, mat4 inverseView, vec3 viewPos, vec2 normalizedDeviceCoords);
float FresnelSchlick(float cosTheta, float n1, float n2);
vec3 InverseGammaToLinear(vec3 rgb);

uniform vec2 uboGameObjectsSize;

uniform int rayDepth;
uniform int SPP;

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
    for (int i = 0; i < SPP; i++)
    {   
        vec2 subPixelOffset = vec2(GetRandomFloat01(), GetRandomFloat01()) - 0.5; // integrating over whole pixel eliminates aliasing
        vec2 ndc = (imgCoord + subPixelOffset) / imgResultSize * 2.0 - 1.0;
        Ray rayEyeToWorld = GetWorldSpaceRay(basicDataUBO.InvProjection, basicDataUBO.InvView, basicDataUBO.ViewPos, ndc);

        vec3 focalPoint = rayEyeToWorld.Origin + rayEyeToWorld.Direction * focalLength;
        vec2 offset = apertureDiameter * 0.5 * UniformSampleUnitCircle();
        
        rayEyeToWorld.Origin = (basicDataUBO.InvView * vec4(offset, 0.0, 1.0)).xyz;
        rayEyeToWorld.Direction = normalize(focalPoint - rayEyeToWorld.Origin);

        color += Radiance(rayEyeToWorld);
    }
    color /= SPP;
    vec3 lastFrameColor = imageLoad(ImgResult, imgCoord).rgb;

    color = mix(lastFrameColor, color, 1.0 / (thisRendererFrame + 1));
    imageStore(ImgResult, imgCoord, vec4(color, 1.0));
}

vec3 Radiance(Ray ray)
{
    vec3 throughput = vec3(1.0);
    vec3 resultColor = vec3(0.0);

    HitInfo hitInfo;
    bool isRefractive;
    float rayProbability;
    for (int i = 0; i < rayDepth; i++)
    {
        if (RayTrace(ray, hitInfo))
        {
            // If ray did just pass through medium apply Beer's law
            if (hitInfo.FromInside)
            {
                hitInfo.Normal *= -1.0;
                throughput *= exp(-hitInfo.Material.Absorbance * hitInfo.T);
            }

            // Evaluating BSDF gives a new ray based on the hitPoints properties and the incomming ray,
            // the probability this ray would take its path 
            // and a bool indicating wheter the ray penetrates into the medium
            rayProbability = BSDF(ray, hitInfo, isRefractive);

            resultColor += hitInfo.Material.Emissiv * throughput;
            if (!isRefractive)
            {
                // The cosine term is already taken into account by the CosineSampleHemisphere function. Its weighting the random rays to a cosine distibution
                // throughput *= hitInfo.Material.Albedo * dot(ray.Direction, hitInfo.Normal);
                
                throughput *= hitInfo.Material.Albedo;
            }

            throughput /= rayProbability;
            
            // Russian Roulette, unbiased method to terminate rays and therefore lower render times (also reduces fireflies)
            {
                float p = max(throughput.x, max(throughput.y, throughput.z));
                if (GetRandomFloat01() > p)
                    break;

                throughput /= p;
            }
        }
        else
        {
            resultColor += texture(SamplerEnvironment, ray.Direction).rgb * throughput;
            break;
        }
    }
    return resultColor;
}

float BSDF(inout Ray ray, HitInfo hitInfo, out bool isRefractive)
{
    isRefractive = false;

    float specularChance = hitInfo.Material.SpecularChance;
    float refractionChance = hitInfo.Material.RefractionChance;
    if (specularChance > 0.0)
    {
        specularChance = mix(specularChance, 1.0, FresnelSchlick(dot(-ray.Direction, hitInfo.Normal), hitInfo.FromInside ? hitInfo.Material.IOR : 1.0, !hitInfo.FromInside ? hitInfo.Material.IOR : 1.0));
        float diffuseChance = 1.0 - specularChance - refractionChance;
        refractionChance = 1.0 - specularChance - diffuseChance;
    }

    vec3 diffuseRay = CosineSampleHemisphere(hitInfo.Normal);
    float rayProbability = 1.0;
    //float isDiffuse = 1.0 - isSpecular - isRefractive;
    
    float raySelectRoll = GetRandomFloat01();
    if (specularChance > raySelectRoll)
    {
        ray.Direction = normalize(mix(reflect(ray.Direction, hitInfo.Normal), diffuseRay, hitInfo.Material.SpecularRoughness * hitInfo.Material.SpecularRoughness));
        rayProbability = specularChance;
    }
    else if (specularChance + refractionChance > raySelectRoll)
    {
        vec3 refractionRayDir = refract(ray.Direction, hitInfo.Normal, hitInfo.FromInside ? hitInfo.Material.IOR / 1.0 : 1.0 / hitInfo.Material.IOR);
        refractionRayDir = normalize(mix(refractionRayDir, CosineSampleHemisphere(-hitInfo.Normal), hitInfo.Material.RefractionRoughness * hitInfo.Material.RefractionRoughness));
        ray.Direction = refractionRayDir;
        rayProbability = refractionChance;
        isRefractive = true;
    }
    else
    {
        ray.Direction = diffuseRay;
        rayProbability = 1.0 - specularChance - refractionChance;
    }
    
    ray.Origin = hitInfo.NearHitPos + hitInfo.Normal * EPSILON * (isRefractive ? -1 : 1);
    return max(rayProbability, EPSILON);
}

bool RayTrace(Ray ray, out HitInfo hitInfo)
{
    hitInfo.T = FLOAT_MAX;
    float t1, t2;

    for (int i = 0; i < uboGameObjectsSize.x; i++)
    {
        vec3 pos = gameObjectsUBO.Spheres[i].Position;
        float radius = gameObjectsUBO.Spheres[i].Radius;
        if (RaySphereIntersect(ray, pos, radius, t1, t2) && t2 > 0.0 && t1 < hitInfo.T)
        {
            hitInfo.T = GetSmallestPositive(t1, t2);
            hitInfo.FromInside = hitInfo.T == t2;
            hitInfo.Material = gameObjectsUBO.Spheres[i].Material;
            hitInfo.NearHitPos = ray.Origin + ray.Direction * hitInfo.T;
            hitInfo.Normal = GetNormal(pos, radius, hitInfo.NearHitPos);
        }
    }
    
    for (int i = 0; i < uboGameObjectsSize.y; i++)
    {
        vec3 aabbMin = gameObjectsUBO.Cuboids[i].Min;
        vec3 aabbMax = gameObjectsUBO.Cuboids[i].Max;
        if (RayCuboidIntersect(ray, aabbMin, aabbMax, t1, t2) && t2 > 0.0 && t1 < hitInfo.T)
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
    if (discriminant < 0.0)
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

vec3 CosineSampleHemisphere(vec3 normal)
{
    // Source: https://blog.demofox.org/2020/05/25/casual-shadertoy-path-tracing-1-basic-camera-diffuse-emissive/

    float z = GetRandomFloat01() * 2.0 - 1.0;
    float a = GetRandomFloat01() * 2.0 * PI;
    float r = sqrt(1.0 - z * z);
    float x = r * cos(a);
    float y = r * sin(a);

    // Convert unit vector in sphere to a cosine weighted vector in hemisphere
    return normalize(normal + vec3(x, y, z));
}

vec2 UniformSampleUnitCircle()
{
    float angle = GetRandomFloat01() * 2.0 * PI;
    float r = sqrt(GetRandomFloat01());
    return vec2(cos(angle), sin(angle)) * r;
}

vec3 GetNormal(vec3 spherePos, float radius, vec3 surfacePosition)
{
    return (surfacePosition - spherePos) / radius;
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

uint GetPCGHash(inout uint seed)
{
    seed = seed * 747796405u + 2891336453u;
    uint word = ((seed >> ((seed >> 28u) + 4u)) ^ seed) * 277803737u;
    return (word >> 22u) ^ word;
}
 
float GetRandomFloat01()
{
    return float(GetPCGHash(rndSeed)) / 4294967296.0;
}

// Assumes t2 > t1
float GetSmallestPositive(float t1, float t2)
{
    return t1 < 0 ? t2 : t1;
}

Ray GetWorldSpaceRay(mat4 inverseProj, mat4 inverseView, vec3 viewPos, vec2 normalizedDeviceCoords)
{
    vec4 rayEye = inverseProj * vec4(normalizedDeviceCoords.xy, -1.0, 0.0);
    rayEye.zw = vec2(-1.0, 0.0);
    return Ray(viewPos, normalize((inverseView * rayEye).xyz));
}

float FresnelSchlick(float cosTheta, float n1, float n2)
{
    float r0 = (n1 - n2) / (n1 + n2);
    r0 *= r0;
    return r0 + (1.0 - r0) * pow(1.0 - cosTheta, 5.0);
}

vec3 InverseGammaToLinear(vec3 rgb)
{
    return mix(pow(((rgb + 0.055) / 1.055), vec3(2.4)), rgb / 12.92, vec3(lessThan(rgb, vec3(0.04045))));
}
