#version 450 core
layout(location = 0) out vec4 f_color;

in vec3 v_Normal;
in vec3 v_TexCoord;

uniform sampler2DArray mainTexture;

void main()
{
    vec4 color = texture(mainTexture, v_TexCoord);

    // Alpha Discard
    if (color.w < 0.1)
    {
        discard;
    }

    f_color = color; // Texture Sample

    //f_color = vec4(v_VertexColor, 1); // Triangle Color Sample
    //f_color = vec4(fract(v_TexCoord), 0, 1); // UV Debug
    //f_color = texture(mainTexture, v_TexCoord) * vec4(v_VertexColor, 1); // Combination
}