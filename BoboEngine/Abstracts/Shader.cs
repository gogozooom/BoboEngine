namespace BoboEngine.Shaders;

public abstract class Shader
{
    public abstract Float3 PixelColor(Float2 pixelCoord, Float2 texCoord, Float3 normal, float depth);
}
