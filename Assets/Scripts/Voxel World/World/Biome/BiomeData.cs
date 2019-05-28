using System;

/// <summary>
/// The biome data that will be stored inside a Chunk. This data is type agnostic, meaning
/// all Biomes will store this struct, regardless of type.
/// </summary>
[Serializable]
public struct BiomeData
{
    public int currentWidth;
    public int currentHeight;

    public BiomeType type;

    public BiomeData(BiomeType type, int currentWidth = 0, int currentHeight = 0)
    {
        this.type = type;
        this.currentHeight = currentHeight;
        this.currentWidth = currentWidth;
    }
}