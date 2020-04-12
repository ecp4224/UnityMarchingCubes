using UnityEngine;

public struct PerlinNoise : Noise
{
    public const float MIN_OFFSET = -10000000f;
    public const float MAX_OFFSET = 10000000f;
    
    private float _xOffset;
    private float _yOffset;

    public PerlinNoise(Unity.Mathematics.Random random)
    {
        _xOffset = random.NextFloat(MIN_OFFSET, MAX_OFFSET);
        _yOffset = random.NextFloat(MIN_OFFSET, MAX_OFFSET);
    }
    
    public PerlinNoise(float xoffset, float yoffset)
    {
        _xOffset = xoffset;
        _yOffset = yoffset;
    }
    
    public float Compute(float x, float y)
    {
        return Mathf.PerlinNoise(x + _xOffset, y + _yOffset);
    }
}
