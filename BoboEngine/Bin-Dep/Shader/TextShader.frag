#version 450 core
layout(location = 0) out vec4 f_color;

in vec3 v_NormalizedPos;
in vec2 v_CurvePos;
in float v_IsClockwise;

uniform sampler2D mainTexture;
uniform vec4 u_color;
uniform float u_time;

void main()
{
    if (v_CurvePos.x * v_CurvePos.x > v_CurvePos.y == (v_IsClockwise == 0))
    {
        discard;
    }

    f_color = u_color;
}