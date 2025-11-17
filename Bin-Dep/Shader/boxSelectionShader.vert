#version 450 core
layout (location = 0) in vec3 a_Position;
layout (location = 1) in vec4 a_VertexColor;

out vec4 v_VertexColor;

uniform mat4 projection;
uniform mat4 model;

void main()
{
    gl_Position = projection * model * vec4(a_Position, 1.0); // position x, y, z, 1
    v_VertexColor = vec4(0, 0, 0, 0.4);
}