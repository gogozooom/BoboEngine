#version 450 core
layout (location = 0) in vec3 a_Position;
layout (location = 1) in vec3 a_NormalizedPos;
layout (location = 2) in vec2 a_CurvePos;

out vec3 v_NormalizedPos;
out vec2 v_CurvePos;

uniform mat4 model;
uniform vec4 u_color;
uniform float u_time;

void main()
{
    gl_Position = model * vec4(a_Position, 1.0); // position x, y, z, 1
    v_NormalizedPos = a_NormalizedPos;
    v_CurvePos = a_CurvePos;
}