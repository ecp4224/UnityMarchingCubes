using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class ChunkFileLoader : ChunkProvider
{
    public string chunkDirectoryName = "chunks";
    private string chunkDicrectory;

    [BindComponent]
    private World world;

    public bool enableSaving = false;
    
    public override void Awake()
    {
        base.Awake(); //Ensure we call the base Awake
        
        chunkDicrectory = Application.persistentDataPath + "/" + chunkDirectoryName + "/";

        if (!Directory.Exists(chunkDicrectory))
            Directory.CreateDirectory(chunkDicrectory);
    }
    
    public override Chunk LoadChunkAt(Vector3 worldOrigin)
    { 
        var filename = worldOrigin.ToString().Md5Sum();
        
        var fullPath = chunkDicrectory + filename + ".json";
        if (!File.Exists(fullPath)) return null;
        
        var fileContents = File.ReadAllText(fullPath);

        var chunk = JsonConvert.DeserializeObject<Chunk>(fileContents);

        if (chunk.Verticies.Count == 0)
        {
            File.Delete(fullPath);
            return null; //This chunk is corrupt, ignore it..
        }

        Debug.Log("Loaded from " + fullPath);

        return chunk;
    }

    public void SaveChunk(Chunk chunk)
    {
        if (!enableSaving)
            return;
        
        if (chunk.Verticies.Count == 0)
            return; //Don't save an empty chunk..
        
        var chunkPos = chunk.Position;
        
        var worldOrigin = new Vector3(chunkPos.X * world.voxelSize * world.chunkSize, 0.0f, chunkPos.Z * world.voxelSize * world.chunkSize);
        
        var filename = worldOrigin.ToString().Md5Sum();
        
        var fullPath = chunkDicrectory + filename + ".json";

        var json = JsonConvert.SerializeObject(chunk);
        
        File.WriteAllText(fullPath, json);
    }
}