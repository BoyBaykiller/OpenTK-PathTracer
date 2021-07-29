#version 450 core 

layout (location = 0) in vec3 inPosition;

layout (std140, binding = 0) uniform BasicDataUBO
{
	mat4 projection;
    mat4 invProjection;
    vec2 nearFar;
	mat4 view;
	mat4 invView;
    mat4 projectionView;
	vec4 viewPos;
} basicDataUBO;

layout (location = 0) uniform mat4 modelMatrix;

void main()
{
    gl_Position = basicDataUBO.projectionView * modelMatrix * vec4(inPosition, 1.0);
}