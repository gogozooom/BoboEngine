#version 450 core
layout(location = 0) out vec4 f_color;

in vec3 v_Normal;
in vec2 v_TexCoord;

uniform sampler2D mainTexture;
uniform float u_time;

void main()
{
    vec4 color = texture(mainTexture, v_TexCoord); // Texture Sample

    if (color.a == 0.0)
    {
        discard;
    }

    f_color = color;
}