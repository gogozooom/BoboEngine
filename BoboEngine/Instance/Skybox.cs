namespace BoboEngine;

public class Skybox : ObjectBehavior
{
    public Float3 skyColor { get => _skyColor; set => SetSkyColor(value); }
    private Float3 _skyColor;

    private void SetSkyColor(Float3 value)
    {
        WindowManager.SetClearColor(value);
        _skyColor = value;
    }
}