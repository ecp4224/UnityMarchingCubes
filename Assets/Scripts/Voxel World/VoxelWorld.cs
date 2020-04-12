using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(World))]
public class VoxelWorld : BindableMonoBehavior
{
    [BindComponent]
    [HideInInspector]
    public World world;

    [BindComponent(warnOnly= true)]
    [HideInInspector]
    public ChunkFileLoader fileLoader;

    [BindComponents]
    [HideInInspector]
    public List<ChunkProvider> providers;

    public int startWidth = 10;
    public int startHeight = 10;
	
    // Use this for initialization
    void Start ()
    {
        foreach (var provider in providers.OrderByDescending(c => c.priority))
        {
            world.LoadWith(provider);
        }
        
        Generate();
    }

    public void Generate()
    {
        Generate(new Vector2(0, 0), new Vector2(startWidth, startHeight));
    }
    
    public void Generate(Vector2 min, Vector2 max)
    {
        for (int x = (int) min.x; x < max.x; x++)
        {
            for (int z = (int) min.y; z < max.y; z++)
            {
                var point = new World.ChunkPoint(x, z);
                Chunk c = world.LoadChunkAt(point);

                if (c == null)
                    continue;
                
                world.SpawnChunk(c);
            }
        }
    }
}