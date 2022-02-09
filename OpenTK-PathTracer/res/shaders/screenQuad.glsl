#version 450 core

const vec4 data[6] = vec4[]
(
    vec4( -1.0,  1.0,  0.0, 1.0 ),
    vec4( -1.0, -1.0,  0.0, 0.0 ),
    vec4(  1.0, -1.0,  1.0, 0.0 ),
    vec4( -1.0,  1.0,  0.0, 1.0 ),
    vec4(  1.0, -1.0,  1.0, 0.0 ),
    vec4(  1.0,  1.0,  1.0, 1.0 )
);

out InOutVars
{
    vec2 TexCoord;
} outData;

void main()
{
    vec4 vertex = data[gl_VertexID];

    gl_Position = vec4(vertex.xy, 0.0, 1.0);
    outData.TexCoord = vertex.zw;
}