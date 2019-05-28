using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

//TODO Maybe make this a class?
//TODO Since it will exceed 16 bytes with all the lists
public class Chunk
{
    public List<Vector3> Verticies = new List<Vector3>();
    public List<int> Indices = new List<int>();
    public World.ChunkPoint Position;
    public int ChunkSize;
    public int ChunkHeight;
    public float[] Points;
    public BiomeData BiomeData;

    [NonSerialized]
    public List<GameObject> Entity;

    public Chunk(World.ChunkPoint position, float[] points, int chunkSize, int chunkHeight)
    {
        Position = position;
        Points = points;
        ChunkSize = chunkSize;
        ChunkHeight = chunkHeight;
        this.Entity = new List<GameObject>();
    }

    public int PosToIndex(int x, int y, int z)
    {
        
        var index = x * (ChunkHeight + 1) * (ChunkSize + 1) + y * (ChunkSize + 1) + z;
        return index;
        //return x + ChunkSize * (y + ChunkSize * z);
    }

    public void AddEntity(GameObject entity)
    {
        this.Entity.Add(entity);
    }

    public float GetSample(Vector3 position)
    {
        var worldOrigin = new Vector3(Position.X * World.main.voxelSize * World.main.chunkSize, 
                                        0.0f, 
                                        Position.Z * World.main.voxelSize * World.main.chunkSize);

        var truePos = (position - worldOrigin).Abs();
        
        return Points[PosToIndex((int)truePos.x, (int)truePos.y, (int)truePos.z)];
    }

    public Vector3 ConvertPosition(Vector3 position)
    {
        var worldOrigin = new Vector3(Position.X * World.main.voxelSize * World.main.chunkSize, 
            0.0f, 
            Position.Z * World.main.voxelSize * World.main.chunkSize);

        var truePos = (position - worldOrigin).Round();

        if (truePos.x < 0)
            truePos.x = ChunkSize + truePos.x; //Add since the x will always be negative
        if (truePos.z < 0)
            truePos.z = ChunkSize + truePos.z; //Add since the z will always be negative
        
        return truePos;
    }

    internal void Recalculate(int size, float scale, Vector3 origin, bool interpolate)
    {
        int flagIndex;
        int index = 0;
        
        Verticies.Clear();
        Indices.Clear();
        
        float[] afCubes = new float[8];
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    //Offsets are same as cornerOffsets[8]
                    afCubes[0] = Points[PosToIndex(x, y, z)];
                    afCubes[1] = Points[PosToIndex(x+1, y, z)];
                    afCubes[2] = Points[PosToIndex(x+1, y+1, z)];
                    afCubes[3] = Points[PosToIndex(x, y+1, z)];
                    afCubes[4] = Points[PosToIndex(x, y, z+1)];
                    afCubes[5] = Points[PosToIndex(x+1, y, z+1)];
                    afCubes[6] = Points[PosToIndex(x+1, y+1, z+1)];
                    afCubes[7] = Points[PosToIndex(x, y+1, z+1)];
                    
                    
                    //Calculate bitfield
                    flagIndex = 0;
                    for(int vtest = 0; vtest < 8; vtest++)
                    {
                        if(afCubes[vtest] <= 0.0f)
                            flagIndex |= 1 << vtest;
                    }
                    
                    //Skip to next if all corners are the same
                    if(flagIndex == 0x00 || flagIndex == 0xFF)
                        continue;
                    
                    //Get the offset of this current block
                    var offset = new Vector3(x * scale, y * scale, z * scale);

                    for (int triangle = 0; triangle < 5; triangle++)
                    {
                        int edgeIndex = VoxelLookUp.a2iTriangleConnectionTable[flagIndex][3 * triangle];

                        if (edgeIndex < 0)
                            continue; //Skip if the edgeIndex is -1

                        for (int triangleCorner = 0; triangleCorner < 3; triangleCorner++)
                        {
                            edgeIndex =
                                VoxelLookUp.a2iTriangleConnectionTable[flagIndex][3 * triangle + triangleCorner];

                            var edge1 = VoxelLookUp.edgeVertexOffsets[edgeIndex, 0];
                            var edge2 = VoxelLookUp.edgeVertexOffsets[edgeIndex, 1];

                            edge1 *= scale;
                            edge2 *= scale;

                            Vector3 middle;
                            if (interpolate)
                            {
                                float ofst;
                                float s1 = Points[PosToIndex(x + (int)edge1.x, y + (int)edge1.y, z + (int)edge1.z)];
                                float delta = s1 - Points[PosToIndex(x + (int)edge2.x, y + (int)edge2.y, z + (int)edge2.z)];
                                if(delta == 0.0f)
                                    ofst = 0.5f;
                                else
                                    ofst = s1 / delta;
                                middle = edge1 + ofst*(edge2-edge1); 
                            }
                            else
                            {
                                middle = (edge1 + edge2) * 0.5f;
                            }
                            
                            Verticies.Add(offset + middle);
                            Indices.Add(index++);
                        }
                    }
                }
            }
        }
    }
    
    public GameObject GenerateObject(Material material, Vector3 origin)
    {
        if (Verticies.Count == 0)
            return null;
        
        var mesh = new Mesh();
        mesh.vertices = Verticies.ToArray();
        mesh.triangles = Indices.ToArray();
        mesh.uv = new Vector2[mesh.vertices.Length];
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        var go = new GameObject("TerrainChunk");
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        var collider = go.AddComponent<MeshCollider>();
        var watcher = go.AddComponent<ChunkWatcher>();
        mf.sharedMesh = mesh;
        mr.sharedMaterial = material;
        collider.sharedMesh = mesh;
        watcher.chunk = this;
        go.transform.position = origin;
        //go.transform.Rotate(0f, 0f, -180f);
        
        return go;
    }

    public void Dispose()
    {
        foreach (var entity in Entity)
        {
            Object.Destroy(entity);
        }
        
        Entity.Clear();
        Verticies.Clear();
        Indices.Clear();
        Points = null;
    }
}