public struct OctaveNoise : Noise
{
    public int octaves;
    public PerlinNoise[] perlin;
    
    public OctaveNoise(Unity.Mathematics.Random random, int count)
    {
        this.octaves = count;
        this.perlin = new PerlinNoise[count];

        for (int i = 0; i < count; i++)
        {
            perlin[i] = new PerlinNoise(random);
        }
    }
    
    public float Compute(float x, float y)
    {
        float result = 0f;
        float something = 1f;

        for (int i = 0; i < octaves; i++)
        {
            result += perlin[i].Compute(x / something, y / something) * something;

            something *= 2f;
        }

        return result;
    }
}
