
namespace BoboEngine.Shaders
{
    public class ShaderLit
    {
        Texture texture;

        public ShaderLit()
        {
            texture = new();
        }
        public ShaderLit(string filePath)
        {
            texture = new(filePath);
        }

        public Float3 PixelColor(Float2 pixelCoord, Float2 texCoord, Float3 normal, float depth, Transform transform = null)
        {
            if (Renderer.debug_renderType == renderType.normals)
                return (normal + Float3.one) / 2;

            float lightIntensity = (Float3.Dot(normal, new(1, 1, 1)) + 2) / 3; // TMP SUN LIGHT SOURCE

            return texture.Sample(texCoord) * lightIntensity;
        }
    }
}