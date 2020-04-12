using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Random = UnityEngine.Random;

public class ClassicMCGenerator : ChunkGenerator<ClassicChunkJob>
{
    public uint seed;
    public int waterLevel = 32;

    public float noiseScale = 0.13f;

    public float heightLowScaler = 1f / 6f;
    public float heightHighScaler = 1f / 5f;

    public float heightLow = 4;
    public float heightHigh = 6;
    
    protected override ClassicChunkJob CreateJob(Vector3 origin)
    {
        int size = world.chunkSize;

        int buffer = size + 1;

        if (seed == 0)
            seed = (uint)Random.Range(Int32.MinValue, Int32.MaxValue);
        
        return new ClassicChunkJob()
        {
            chunk = new NativeArray<float>(buffer * buffer * (world.ChunkHeight + 1), Allocator.Persistent),
            blocks = new NativeArray<int>(buffer * buffer * (world.ChunkHeight + 1), Allocator.Persistent),
            height = world.ChunkHeight,
            origin = origin,
            scale = noiseScale,
            size = size,
            waterLevel = waterLevel,
            seed = seed,
            heightLowScaler = heightLowScaler,
            heightHighScaler = heightHighScaler,
            heightLow = heightLow,
            heightHigh = heightHigh,
        };
    }

    protected override float[] ChunkFromJob(ClassicChunkJob job)
    {
        float[] array = job.chunk.ToArray();
        job.chunk.Dispose();
        
        //TODO Store bloccks
        job.blocks.Dispose();
        
        return array;
    }
}

public struct ClassicChunkJob : IJob
{
    [ReadOnly] public int size;

    [ReadOnly] public int height;

    [ReadOnly] public int waterLevel;

    [ReadOnly] public float scale;
    
    [ReadOnly] public uint seed;
    [ReadOnly] public Vector3 origin;
    [ReadOnly] public float heightLowScaler;
    [ReadOnly] public float heightHighScaler;
    [ReadOnly] public float heightLow;
    [ReadOnly] public float heightHigh;
    
    public NativeArray<float> chunk;
    public NativeArray<int> blocks;

    public void Execute()
    {
        var rand = new Unity.Mathematics.Random(seed);
            
        Noise noise1 = new CombinedNoise(new OctaveNoise(rand, 8), new OctaveNoise(rand, 8));
        Noise noise2 = new CombinedNoise(new OctaveNoise(rand, 8), new OctaveNoise(rand, 8));
        Noise noise3 = new OctaveNoise(rand, 6);
        
        var heightMap = HeightMap(noise1, noise2, noise3);

        Strate(noise1, heightMap);

        //CreateCaves(rand);

        
        //Do this last
        FillChunkFromBlocks();
    }

    private Vector2 ComputeNoisePosition(int x, int z)
    {
        Vector2 _origin = new Vector2(origin.x + x, origin.z + z);

        return _origin * scale;
    }

    private NativeArray<float> HeightMap(Noise noise1, Noise noise2, Noise noise3)
    {
        //TODO Don't overfill chunks, when building mesh fetch vertex information from surrounding chunks
        int buffer = size + 1;

        var heightMap = new NativeArray<float>(buffer * buffer, Allocator.Temp);

        for (int x = 0; x < buffer; x++)
        {
            for (int z = 0; z < buffer; z++)
            {
                Vector2 pos = ComputeNoisePosition(x, z);
                
                float heightLow = noise1.Compute(pos.x * 1.3f, pos.y * 1.3f) * heightLowScaler - this.heightLow;
                float heightHigh = noise2.Compute(pos.x * 1.3f, pos.y * 1.3f) * heightHighScaler + this.heightHigh;

                float heightResult;
                if (noise3.Compute(pos.x, pos.y) / 8f > 0f)
                {
                    heightResult = heightLow;
                }
                else
                {
                    heightResult = Mathf.Max(heightLow, heightHigh);
                }

                heightResult /= 2f;

                if (heightResult < 0f)
                {
                    heightResult /= (8f / 10f);
                }

                heightMap[buffer * x + z] = heightResult + waterLevel;
            }
        }

        return heightMap;
    }

    private void Strate(Noise noise1, NativeArray<float> heightMap)
    {
        //TODO Don't overfill chunks, when building mesh fetch vertex information from surrounding chunks
        int buffer = size + 1;

        for (int x = 0; x < buffer; x++)
        {
            for (int z = 0; z < buffer; z++)
            {
                Vector2 pos = ComputeNoisePosition(x, z);
                
                var dirtThickness = noise1.Compute(pos.x, pos.y) / 24f - 4f;
                var dirtTransition = heightMap[buffer * x + z];
                var stoneTransition = dirtTransition + dirtThickness;

                for (int y = 0; y < height; y++)
                {
                    var blockType = 0;
                    if (y == 0)
                    {
                        blockType = (int) Block.VOID;
                    }
                    else if (y <= stoneTransition)
                    {
                        blockType = (int) Block.STONE;
                    }
                    else if (y <= dirtTransition)
                    {
                        blockType = (int) Block.DIRT;
                    }

                    int index = x * (height + 1) * buffer + y * buffer + z;

                    blocks[index] = blockType;
                }
            }
        }
    }

    private void Smooth(NativeArray<float> heightMap)
    {
        int buffer = size + 1;
        for (int x = 0; x < buffer; x++)
        {
            for (int z = 0; z < buffer; z++)
            {
                
            }
        }
    }

    private void CreateCaves(Unity.Mathematics.Random random)
    {
        int buffer = size + 1;

        int caveCount = (buffer * buffer * height) / 8192;

        for (int i = 0; i < caveCount; i++)
        {
            float caveX = random.NextInt(0, buffer);
            float caveY = random.NextInt(0, height);
            float caveZ = random.NextInt(0, buffer);

            float caveLength = random.NextFloat() * random.NextFloat() * 200f;

            float theta = random.NextFloat() * Mathf.PI * 2f;
            float deltaTheta = 0f;
            float phi = random.NextFloat() * Mathf.PI * 2f;
            float deltaPhi = 0f;

            float caveRadius = random.NextFloat() * random.NextFloat();

            for (int len = 0; len < caveLength; len++)
            {
                caveX += Mathf.Sin(theta) * Mathf.Cos(phi);
                caveY += Mathf.Cos(theta) * Mathf.Sin(phi);
                caveZ += Mathf.Sin(phi);

                theta += deltaTheta * 0.2f;
                deltaTheta = (deltaTheta * 0.9f) + random.NextFloat() - random.NextFloat();

                phi = phi / 2f + deltaPhi / 4f;
                deltaPhi = (deltaPhi * 0.75f) + random.NextFloat() - random.NextFloat();

                if (random.NextFloat() >= 0.25)
                {
                    float centerX = caveX + (random.NextInt(0, 4) - 2) * 0.2f;
                    float centerY = caveY + (random.NextInt(0, 4) - 2) * 0.2f;
                    float centerZ = caveZ + (random.NextInt(0, 4) - 2) * 0.2f;

                    float radius = (height - centerY) / height;
                    radius = 1.2f + (radius * 3.5f + 1) * caveRadius;
                    radius = radius * Mathf.Sin(len * Mathf.PI / caveLength);
                    
                    for(int sphereX = (int)(centerX - radius); sphereX <= (int)(centerX + radius); ++sphereX) {
                        for(int sphereY = (int)(centerY - radius); sphereY <= (int)(centerY + radius); ++sphereY) {
                            for(int sphereZ = (int)(centerZ - radius); sphereZ <= (int)(centerZ + radius); ++sphereZ) {
                                float var38 = sphereX - centerX;
                                float var39 = sphereY - centerY;
                                float var40 = sphereZ - centerZ;
                                
                                if(var38 * var38 + var39 * var39 * 2.0F + var40 * var40 < radius * radius && sphereX >= 1 && sphereY >= 1 && sphereZ >= 1 && sphereX < buffer - 1 && sphereY < height - 1 && sphereZ < buffer - 1) {
                                    int index = sphereX * (height + 1) * buffer + sphereY * buffer + sphereZ;
                                    blocks[index] = (int) Block.AIR;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void FillChunkFromBlocks()
    {
        int buffer = size + 1;
        for (int x = 0; x < buffer; x++)
        {
            for (int z = 0; z < buffer; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    int index = x * (height + 1) * buffer + y * buffer + z;

                    chunk[index] = blocks[index] == (int) Block.AIR || blocks[index] == (int) Block.VOID ? 0f : 1f;
                }
            }
        }
    }
}