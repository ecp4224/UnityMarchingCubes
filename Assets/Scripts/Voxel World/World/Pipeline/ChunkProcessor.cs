using UnityEngine;

public abstract class ChunkProcessor : BindableMonoBehavior
{
    public int priority;

    public abstract void ProcessChunk(ref Chunk chunk);
}