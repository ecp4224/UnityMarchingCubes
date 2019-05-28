using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BiomeProcessor : ChunkProcessor
{
    [BindComponent]
    private World world;

    public double randomizeRate = 0.01;

    public BiomeType PickCommonBiome(Vector3 position)
    {
        World.ChunkPoint p = world.VectorToPoint(position);
        return PickCommonBiome(p);
    }

    public BiomeType PickCommonBiome(World.ChunkPoint point)
    {
        //Get all neighboring chunks
        Chunk[] chunks = {
            world.ChunkAt(point.Move(Neighbor.LEFT), false),
            world.ChunkAt(point.Move(Neighbor.RIGHT), false),
            world.ChunkAt(point.Move(Neighbor.TOP), false),
            world.ChunkAt(point.Move(Neighbor.BOTTOM), false),
            world.ChunkAt(point.Move(Neighbor.TOP_LEFT), false),
            world.ChunkAt(point.Move(Neighbor.TOP_RIGHT), false),
            world.ChunkAt(point.Move(Neighbor.BOTTOM_LEFT), false),
            world.ChunkAt(point.Move(Neighbor.BOTTOM_RIGHT), false)
        };

        List<BiomeData> dataPool = (from c in chunks where c != null where c.BiomeData.type != BiomeType.Unknown select c.BiomeData).ToList();

        return dataPool.Count == 0 ? BiomeType.Unknown : dataPool[Random.Range(0, dataPool.Count)].type;
    }
    
    public override void ProcessChunk(ref Chunk chunk)
    {
        if (chunk.BiomeData.type != BiomeType.Unknown)
            return; //Nothing to do if it already has a biome..
        
        //Get all neighboring chunks
        Chunk[] chunks = {
            world.ChunkAt(chunk.Position.Move(Neighbor.LEFT), false),
            world.ChunkAt(chunk.Position.Move(Neighbor.RIGHT), false),
            world.ChunkAt(chunk.Position.Move(Neighbor.TOP), false),
            world.ChunkAt(chunk.Position.Move(Neighbor.BOTTOM), false),
            world.ChunkAt(chunk.Position.Move(Neighbor.TOP_LEFT), false),
            world.ChunkAt(chunk.Position.Move(Neighbor.TOP_RIGHT), false),
            world.ChunkAt(chunk.Position.Move(Neighbor.BOTTOM_LEFT), false),
            world.ChunkAt(chunk.Position.Move(Neighbor.BOTTOM_RIGHT), false)
        };
        
        List<BiomeData> dataPool = new List<BiomeData>();
        List<Chunk> dataPoolChunks = new List<Chunk>();

        foreach (var c in chunks)
        {
            if (c == null)
                continue;

            if (c.BiomeData.type == BiomeType.Unknown)
                continue;
            
            dataPool.Add(c.BiomeData);
            dataPoolChunks.Add(c);
        }

        if (dataPool.Count == 0 || Random.Range(0f, 1f) < randomizeRate)
        {
            //If no neighbors have a biome
            //Pick a random one
            var type = Extensions.RandomEnumValue<BiomeType>(BiomeType.Unknown);
            var biome = new BiomeData(type);

            chunk.BiomeData = biome;
        }
        else
        {
            //Pick a random neightbor from the set
            var index = Random.Range(0, dataPool.Count);
            var biomeData = dataPool[index];
            var neighborChunk = dataPoolChunks[index];
            
            //Get max for the picked type
            var settings = world.BiomeSettingsFor(biomeData.type);
            var maxWidth = settings.maxWidth;
            var maxHeight = settings.maxHeight;
            var minWidth = settings.minWidth;
            var minHeight = settings.minHeight;

            float success = 1f;
            
            //If we are past the minimum, we have a random chance of picking a new biome
            if (biomeData.currentWidth >= minWidth && biomeData.currentHeight >= minHeight)
            {
                //Now calculate the success rate for picking this biomeType
                float successWidth = ((float) maxWidth - biomeData.currentWidth) / (float) maxWidth;
                float successHeight = ((float) maxHeight - biomeData.currentHeight) / (float) maxHeight;

                success = (successHeight + successWidth) / 2f;
            } //Otherwise, always copy the biome


            if (success >= Random.Range(0f, 1f))
            {
                //If we can pick this biomeType, then lets!
                
                //First lets figure out whether to add to the width or height
                var neighborLocation = chunk.Position - neighborChunk.Position;
                
                var newBiomeData = new BiomeData(biomeData.type, biomeData.currentWidth + (int)neighborLocation.x, biomeData.currentHeight + (int)neighborLocation.y);

                chunk.BiomeData = newBiomeData;
            }
            else
            {
                //The gods of chaos have said we shouldn't pick this one
                //Let's pick a random one?
                
                //TODO Maybe a better method
                
                var type = Extensions.RandomEnumValue<BiomeType>(BiomeType.Unknown);
                var biome = new BiomeData(type);

                chunk.BiomeData = biome;
            }
        }
    }
}