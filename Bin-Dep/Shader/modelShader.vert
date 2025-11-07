#version 330 core
layout (location = 0) in vec3 a_Position;
layout (location = 1) in vec3 a_VertexColor;

out vec3 v_VertexColor;

uniform mat4 projection;
uniform mat4 model;

void main()
{
    v_VertexColor = a_VertexColor;
    gl_Position = projection * model * vec4(a_Position, 1.0); // position x, y, z, 1
}