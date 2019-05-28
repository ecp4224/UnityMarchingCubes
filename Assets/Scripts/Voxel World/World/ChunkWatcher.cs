public class ChunkWatcher : BindableMonoBehavior
{
    [BindComponent(fromObject = "Voxel World")]
    private World world;

    public Chunk chunk;

    void Update()
    {
        if (chunk == null) return;
        if (gameObject == null) return;
        if (world.players.Count == 0) return;
        
        foreach (var player in world.players)
        {
            if (world.WithinRange(player.transform.position, chunk))
                return;
        }
        
        //If we get to this point, then this chunk is outside the view distance of all players
        world.UnloadChunk(chunk);
        Destroy(gameObject);
        chunk = null;
    }
}