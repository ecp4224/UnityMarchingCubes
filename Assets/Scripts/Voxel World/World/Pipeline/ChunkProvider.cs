using UnityEngine;

public abstract class ChunkProvider : BindableMonoBehavior
{
    public int priority;
    
    public abstract Chunk LoadChunkAt(Vector3 worldOrigin);
}