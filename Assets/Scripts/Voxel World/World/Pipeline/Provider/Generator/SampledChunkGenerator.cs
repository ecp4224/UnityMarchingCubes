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
    public float heightMultiplier;
    public float resolution = 1f;
    public int seaLevel;

    protected override SampledChunkJob CreateJob(Vector3 origin)
    {
        int size = world.chunkSize;
        float scale = world.voxelSize;

        int buffer = size + 1;

        if (seed == 0)
            seed = Random.Range(Int32.MinValue, Int32.MaxValue);
        
        return new SampledChunkJob()
        {
            chunk = new NativeArray<float>(buffer * buffer * (world.Height + 1), Allocator.Persistent),
            height = world.Height,
            heightMultiplier = heightMultiplier,
            origin = origin,
            scale = scale,
            seed = seed,
            size = size,
            resolution = resolution
        };
    }

    protected override float[] ChunkFromJob(SampledChunkJob job)
    {
        float[] array = job.chunk.ToArray();
        job.chunk.Dispose();
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
    
    
    public Vector3 origin;
    public NativeArray<float> chunk;
    public int seed;

    public float Sample(Vector3 position)
    {   
        float pi = Mathf.PI;
        float sampleSize = 0.1f;
        
        //float height02 = Mathf.PerlinNoise(position.x + (seed * 2), position.z + (seed * 2));
        float height01 = Mathf.PerlinNoise(((position.x+pi)*sampleSize) + seed, ((position.z+pi)*sampleSize) + seed);
        float height = height01 * heightMultiplier;
        float heightSample = height - position.y;

        float volumetricSample = PerlinNoise.PerlinNoise3((position.x+pi)*sampleSize, (position.y+pi)*sampleSize, (position.z+pi)*sampleSize);
        return Mathf.Min(heightSample, -volumetricSample) + Mathf.Clamp01(height01 - position.y + 0.5f);
    }

    public void Execute()
    {
        int buffer = size + 1;

        for (int x = 0; x < buffer; x++)
        {
            for (int z = 0; z < buffer; z++)
            {
                for (int y = 0; y < height; y++)
                {
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
    }
}