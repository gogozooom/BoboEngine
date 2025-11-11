#version 330 core
layout(location = 0) out vec4 f_color;

in vec3 v_VertexColor;
in vec2 v_TexCoord;

uniform sampler2D ourTexture;

void main()
{
    //f_color = vec4(fract(v_TexCoord), 0, 1); // UV Debug
    f_color = texture(ourTexture, v_TexCoord)/255; // Texture Sample
    //f_color = vec4(v_VertexColor, 1); // Triangle Color Sample
    //f_color = texture(ourTexture, v_TexCoord) * vec4(v_VertexColor, 1); // Combination
}