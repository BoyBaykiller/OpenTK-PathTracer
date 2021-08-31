#version 430 core 

layout(location = 0) in vec3 inPosition;

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

layout(location = 0) uniform mat4 modelMatrix;

void main()
{
    gl_Position = basicDataUBO.ProjectionView * modelMatrix * vec4(inPosition, 1.0);
}