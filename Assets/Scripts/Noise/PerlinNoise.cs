public struct PerlinNoise : Noise
{
    private float _xOffset;
    private float _yOffset;

    public PerlinNoise(Unity.Mathematics.Random random)
    {
        _xOffset = random.NextFloat(float.MinValue, float.MaxValue);
        _yOffset = random.NextFloat(float.MinValue, float.MaxValue);
    }
    
    public float Compute(float x, float y)
    {
        return Unity.Mathematics.noise.cnoise(new Unity.Mathematics.float2(x + _xOffset, y + _yOffset));
    }
}
