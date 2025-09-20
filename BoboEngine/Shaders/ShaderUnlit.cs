namespace BoboEngine.Shaders
{
    public class ShaderUnlit : Shader
    {
        Texture texture;

        public ShaderUnlit()
        {
            texture = new();
        }
        public ShaderUnlit(string filePath)
        {
            texture = new(filePath);
        }

        public override Float3 PixelColor(Float2 pixelCoord, Float2 texCoord, Float3 normal, float depth)
        {
            return texture.Sample(texCoord);
        }
    }
}