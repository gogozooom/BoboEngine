#version 330 core
layout (location = 0) in vec3 a_Position;
layout (location = 1) in vec3 a_VertexColor;
layout (location = 2) in vec2 a_TexCoord;

out vec3 v_VertexColor;
out vec2 v_TexCoord;

uniform mat4 projection;
uniform mat4 model;

void main()
{
    gl_Position = projection * model * vec4(a_Position, 1.0); // position x, y, z, 1
    v_VertexColor = a_VertexColor;
    v_TexCoord = vec2(a_TexCoord.x, a_TexCoord.y);
}