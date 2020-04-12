﻿using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class FlatChunkGenerator: ChunkGenerator<FlatChunkJob>
{
    protected override FlatChunkJob CreateJob(Vector3 origin)
    {
        int size = world.chunkSize;
        
        int buffer = size + 1;
        
        return new FlatChunkJob
        {
            chunk = new NativeArray<float>(buffer * buffer * (world.ChunkHeight + 1), Allocator.Persistent),
            height = world.ChunkHeight,
            origin = origin,
            size = size
        };
    }

    protected override float[] ChunkFromJob(FlatChunkJob job)
    {
        float[] array = job.chunk.ToArray();
        job.chunk.Dispose();
        return array;
    }
}

public struct FlatChunkJob : IJob
{
    [ReadOnly]
    public int size;
    [ReadOnly]
    public int height;
    
    
    public Vector3 origin;
    public NativeArray<float> chunk;

    public void Execute()
    {
        int buffer = size + 1;

        for (int x = 0; x < buffer; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < buffer; z++)
                {
                    int index = x * (height + 1) * buffer + y * buffer + z;
                    //int index = x * (height + 1) * buffer + y * buffer + z;
                    //int index = x + buffer * (y + buffer * z);
                    
                    if (y < height / 2 && y > 0)
                    {
                        chunk[index] = 1;
                    }
                    else
                    {
                        chunk[index] = 0;
                    }
                }
            }
        }
    }
}