using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Random = UnityEngine.Random;

public class SampledChunkGenerator : ChunkGenerator<SampledChunkJob>
{
    public int seed;
    public float heightMultiplier = 4;
    public float resolution = 1f;
    public int waterLevel = 32;
    public float noiseScale = 0.13f;

    protected override SampledChunkJob CreateJob(Vector3 origin)
    {
        int size = world.chunkSize;

        int buffer = size + 1;

        if (seed == 0)
            seed = Random.Range(Int32.MinValue, Int32.MaxValue);
        
        return new SampledChunkJob()
        {
            chunk = new NativeArray<float>(buffer * buffer * (world.ChunkHeight + 1), Allocator.Persistent),
            blocks = new NativeArray<int>(buffer * buffer * (world.ChunkHeight + 1), Allocator.Persistent),
            height = world.ChunkHeight,
            heightMultiplier = heightMultiplier,
            origin = origin,
            scale = noiseScale,
            seed = seed,
            size = size,
            resolution = resolution,
            waterLevel = waterLevel,
        };
    }

    protected override float[] VertexFromJob(SampledChunkJob job)
    {
        float[] array = job.chunk.ToArray();
        job.chunk.Dispose();
        return array;
    }
    
    protected override int[] BlocksFromJob(SampledChunkJob job)
    {
        int[] array = job.blocks.ToArray();

        job.blocks.Dispose();

        return array;
    }
}

public struct SampledChunkJob : IJob
{
    
    [ReadOnly]
    public int size;
    
    [ReadOnly]
    public int height;
    
    [ReadOnly]
    public float heightMultiplier;
    
    [ReadOnly]
    public float scale;

    [ReadOnly]
    public float resolution;

    [ReadOnly]
    public float waterLevel;
    
    
    public Vector3 origin;
    public NativeArray<float> chunk;
    public NativeArray<int> blocks;
    public int seed;

    public float Sample(Vector3 position)
    {   
        
        float pi = Mathf.PI;
        float sampleSize = 0.1f;
        
        float height02 = Mathf.PerlinNoise(position.x + (seed * 2), position.z + (seed * 2));
        //float height01 = Mathf.PerlinNoise(((position.x+pi)*sampleSize) + seed, ((position.z+pi)*sampleSize) + seed);
        
        float height = height02 * heightMultiplier;
        float heightSample = height - position.y;

        float volumetricSample = PerlinNoiseThree.PerlinNoise3((position.x+pi)*sampleSize, (position.y+pi)*sampleSize, (position.z+pi)*sampleSize);
        return Mathf.Min(heightSample, -volumetricSample) + Mathf.Clamp01(height02 - position.y + 0.5f);
    }

    public void Execute()
    {
        /*
        int buffer = size + 1;

        for (int x = 0; x < buffer; x++)
        {
            for (int z = 0; z < buffer; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    //int index = (height * z) + y + (x * height * buffer);
                    //int index = x + buffer * (y + buffer * z);
                    int index = x * (height + 1) * buffer + y * buffer + z;

                    var offset = new Vector3(x * scale, y * scale, z * scale);

                    float rawval = Sample(origin + offset);
                    float val = rawval;
                    if (rawval >= resolution)
                        val = 1f;
                    else
                        val = 0f;

                    chunk[index] = val;
                }
            }
        }
        */
        
        var rand = new Unity.Mathematics.Random((uint) seed);
        
        Noise noise1 = new PerlinNoise(0f, 0f);
        //Noise noise1 = new CombinedNoise(new OctaveNoise(rand, 8), new OctaveNoise(rand, 8));
        //Noise noise2 = new CombinedNoise(new OctaveNoise(rand, 8), new OctaveNoise(rand, 8));
        //Noise noise3 = new OctaveNoise(rand, 6);
        
        var heightMap = CreateHeightMap(noise1);

        Strate(heightMap, noise1);

        
        //Do this last
        FillChunkFromBlocks();
    }

    private NativeArray<float> CreateHeightMap(Noise noise)
    {
        int buffer = size + 1;
        
        var heightMap = new NativeArray<float>(buffer * buffer, Allocator.Temp);

        for (int x = 0; x < buffer; x++)
        {
            for (int z = 0; z < buffer; z++)
            {
                var offset = new Vector2(x + origin.x,z + origin.z);
                
                float height = noise.Compute(offset.x * scale , offset.y * scale) * heightMultiplier;
                
                heightMap[buffer * x + z] = height + waterLevel;
            }
        }

        return heightMap;
    }
    
    private void Strate(NativeArray<float> heightMap, Noise noise)
    {
        //TODO Don't overfill chunks, when building mesh fetch vertex information from surrounding chunks
        int buffer = size + 1;

        for (int x = 0; x < buffer; x++)
        {
            for (int z = 0; z < buffer; z++)
            {   
                //var offset = new Vector2((x * scale) + origin.x,(z * scale) + origin.z);

                var dirtThickness = 1f;
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