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
    public int waterLevel;
    
    protected override ClassicChunkJob CreateJob(Vector3 origin)
    {
        int size = world.chunkSize;
        float scale = world.voxelSize;

        int buffer = size + 1;

        if (seed == 0)
            seed = (uint)Random.Range(Int32.MinValue, Int32.MaxValue);
        
        return new ClassicChunkJob()
        {
            chunk = new NativeArray<float>(buffer * buffer * (world.Height + 1), Allocator.Persistent),
            blocks = new NativeArray<int>(buffer * buffer * (world.Height + 1), Allocator.Persistent),
            height = world.Height,
            origin = origin,
            scale = scale,
            size = size,
            waterLevel = waterLevel,
            seed = seed
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

        
        //Do this last
        FillChunkFromBlocks();
    }

    private Vector2 ComputeNoisePosition(int x, int z)
    {
        Vector2 offset = new Vector2(x * scale, z * scale);
        Vector2 _origin = new Vector2(origin.x + offset.x, origin.z + offset.y);

        return _origin;
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
                
                float heightLow = noise1.Compute(pos.x * 1.3f, pos.y * 1.3f) / 6f - 4f;
                float heightHigh = noise2.Compute(pos.x * 1.3f, pos.y * 1.3f) / 5f + 6f;

                float heightResult;
                if (noise3.Compute(x, z) / 8f > 0f)
                {
                    heightResult = heightLow;
                }
                else
                {
                    heightResult = Unity.Mathematics.math.max(heightLow, heightHigh);
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
                    else if (y <= dirtThickness)
                    {
                        blockType = (int) Block.DIRT;
                    }

                    int index = x * (height + 1) * buffer + y * buffer + z;

                    blocks[index] = blockType;
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