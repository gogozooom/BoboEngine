#version 330 core
layout(location = 0) out vec4 f_color;

in vec3 v_VertexColor;

void main()
{
    f_color = vec4(v_VertexColor, 1);
}