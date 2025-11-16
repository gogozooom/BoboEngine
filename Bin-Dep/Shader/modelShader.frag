#version 450 core
layout(location = 0) out vec4 f_color;

in vec3 v_VertexColor;
in vec3 v_TexCoord;

uniform sampler2DArray mainTexture;

void main()
{
    f_color = texture(mainTexture, v_TexCoord); // Texture Sample

    //f_color = vec4(v_VertexColor, 1); // Triangle Color Sample
    //f_color = vec4(fract(v_TexCoord), 0, 1); // UV Debug
    //f_color = texture(mainTexture, v_TexCoord) * vec4(v_VertexColor, 1); // Combination
}