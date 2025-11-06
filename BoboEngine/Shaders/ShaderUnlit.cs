namespace BoboEngine.Shaders
{
    public class ShaderUnlit
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

        public Float3 PixelColor(Float2 pixelCoord, Float2 texCoord, Float3 normal, float depth, Transform transform = null)
        {
            return texture.Sample(texCoord);
        }
    }
}