#version 430 core
out vec2 TexCoord;

const vec4 data[4] = vec4[]
(
    vec4( -1.0, -1.0,  0.0, 0.0),
    vec4(  1.0, -1.0,  1.0, 0.0),
    vec4(  1.0,  1.0,  1.0, 1.0),
    vec4( -1.0,  1.0,  0.0, 1.0)
);

void main()
{
    vec4 vertex = data[gl_VertexID];
    TexCoord = vertex.zw;
    gl_Position = vec4(vertex.xy, 0.0, 1.0);
}