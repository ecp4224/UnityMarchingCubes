public struct CombinedNoise : Noise
{
    private Noise noise1;
    private Noise noise2;

    public CombinedNoise(Noise noise1, Noise noise2)
    {
        this.noise1 = noise1;
        this.noise2 = noise2;
    }

    public float Compute(float x, float y)
    {
        return noise1.Compute(x + noise2.Compute(x, y), y);
    }
}
