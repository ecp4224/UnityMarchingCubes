using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : BindableMonoBehavior
{
    private static World _main;
    public static World main
    {
        get { return _main; }
    }
    
    [Serializable]
    public struct ChunkPoint
    {
        private readonly int _x;
        private readonly int _z;

        public ChunkPoint(int x, int z)
        {
            this._x = x;
            this._z = z;
        }

        public int X
        {
            get { return _x; }
        }

        public int Z
        {
            get { return _z; }
        }

        public ChunkPoint Move(Vector2 offset)
        {
            return new ChunkPoint(_x + (int)offset.x, _z + (int)offset.y);
        }

        public static Vector2 operator -(ChunkPoint a, ChunkPoint b)
        {
            return new Vector2(a._x - b._x, a._z - b._z);
        }
        
        public static Vector2 operator +(ChunkPoint a, ChunkPoint b)
        {
            return new Vector2(a._x + b._x, a._z + b._z);
        }

        public static bool operator ==(ChunkPoint a, ChunkPoint b)
        {
            return a._x == b._x && a._z == b._z;
        }
        
        public static bool operator !=(ChunkPoint a, ChunkPoint b)
        {
            return a._x != b._x || a._z != b._z;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ChunkPoint)) return false;
            var c = (ChunkPoint) obj;

            return c.X == X && c.Z == Z;

        }

        public override int GetHashCode()
        {
            int hash = 23;
            hash = hash * 31 + _x;
            hash = hash * 31 + _z;
            return hash;
        }

        public override string ToString()
        {
            return "(" + _x + ", " + _z + ")";
        }
    }

    [HideInInspector]
    [BindComponent(warnOnly = true)]
    public ChunkFileLoader loader;
    
    [HideInInspector]
    public List<ChunkProvider> chunkProviderQueue = new List<ChunkProvider>();

    [HideInInspector]
    public List<GameObject> players = new List<GameObject>();

    [HideInInspector]
    [BindComponents]
    public List<Biome> BiomeSettings;

    [HideInInspector]
    [BindComponents]
    public List<ChunkProcessor> ChunkProcessors;

    public int viewDistance = 5;
    private float maxMagnitude = 0f;

    public Material terrianMaterial;
    public int Width = 10;
    public int Height = 10;
    public int Depth = 10;
    private const int PointExists = 1;
    private const int PointEmpty = 0;

    public int chunkSize = 10;
    public float voxelSize = 1.0f;
    
    private Dictionary<ChunkPoint, Chunk> _chunks = new Dictionary<ChunkPoint, Chunk>();
    private Dictionary<ChunkPoint, GameObject> _goCache = new Dictionary<ChunkPoint, GameObject>();

    public override void Awake()
    {
        //Clear this??
        chunkProviderQueue.Clear();
        
        //Then inject
        base.Awake();

        if (_main == null)
            _main = this;
        
    }

    void Start()
    {
        var origin = new ChunkPoint(0, 0);
        var corner = new ChunkPoint(viewDistance, viewDistance);
        var corner2 = new ChunkPoint(-viewDistance, -viewDistance);

        maxMagnitude = (origin - corner2).magnitude;
        Debug.Log((origin - corner).magnitude);
    }
    
    public World LoadWith(ChunkProvider provider)
    {
        if (provider == null)
            return this;

        if (chunkProviderQueue.Contains(provider))
            return this;
        
        chunkProviderQueue.Add(provider);
        return this;
    }

    public Biome BiomeSettingsFor(BiomeType type)
    {
        var settings = BiomeSettings.FirstOrDefault(b => b.biomeType == type);

        return settings;
    }

    public Chunk ChunkAt(ChunkPoint point, bool forceLoad = true)
    {
        if (_chunks.ContainsKey(point))
            return _chunks[point];
        if (!forceLoad) return null;
        
        var c = LoadChunkAt(point);
        return c;
    }

    public void AddPoint(Vector3 position)
    {
        ModifyPoint(position, PointExists);
    }

    public void DeletePoint(Vector3 position)
    {
        ModifyPoint(position, PointEmpty);
    }

    public float GetValue(Chunk chunk, Vector3 insidePoint)
    {
        return chunk.Points[chunk.PosToIndex((int) insidePoint.x, (int) insidePoint.y, (int) insidePoint.z)];
    }

    public void ModifyPoint(Chunk chunk, Vector3 insidePoint, int modifier)
    {
        Debug.Log("Setting " + insidePoint + " to " + modifier);

        chunk.Points[chunk.PosToIndex((int) insidePoint.x, (int) insidePoint.y, (int) insidePoint.z)] = modifier;

        chunk.Recalculate(chunkSize, voxelSize, PointToOrigin(chunk.Position), true);

        var go = ChunkObjectAt(chunk.Position);
        var mesh = go.GetComponent<MeshFilter>().sharedMesh;
        mesh.vertices = chunk.Verticies.ToArray();
        mesh.triangles = chunk.Indices.ToArray();
        mesh.uv = new Vector2[mesh.vertices.Length];
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        go.GetComponent<MeshCollider>().sharedMesh = null;
        go.GetComponent<MeshCollider>().sharedMesh = mesh;
        
        if (insidePoint.x == 10)
        {
            //Get the chunk next to this one and also update
            Chunk newChunk = ChunkAt(chunk.Position.Move(Neighbor.RIGHT));
            var newInsidePoint = new Vector3(0, insidePoint.y, insidePoint.z);
            
            if (GetValue(newChunk, newInsidePoint) != modifier)
                ModifyPoint(newChunk, newInsidePoint, modifier);
        }
        else if (insidePoint.x == 0)
        {
            Chunk newChunk = ChunkAt(chunk.Position.Move(Neighbor.LEFT));
            var newInsidePoint = new Vector3(10, insidePoint.y, insidePoint.z);
            
            if (GetValue(newChunk, newInsidePoint) != modifier)
                ModifyPoint(newChunk, newInsidePoint, modifier);
        }

        if (insidePoint.z == 0)
        {
            //Get the chunk next to this one and also update
            Chunk newChunk = ChunkAt(chunk.Position.Move(Neighbor.BOTTOM));
            var newInsidePoint = new Vector3(insidePoint.x, insidePoint.y, 10);
                
            if (GetValue(newChunk, newInsidePoint) != modifier)
                ModifyPoint(newChunk, newInsidePoint, modifier);
        }
        else if (insidePoint.z == 10)
        {
            //Get the chunk next to this one and also update
            Chunk newChunk = ChunkAt(chunk.Position.Move(Neighbor.TOP));
            var newInsidePoint = new Vector3(insidePoint.x, insidePoint.y, 0);
                
            if (GetValue(newChunk, newInsidePoint) != modifier)
                ModifyPoint(newChunk, newInsidePoint, modifier);
        }
    }

    public void ModifyPoint(Vector3 position, int modifier)
    {
        Chunk chunk = ChunkAt(position, false);
        if (chunk != null)
        {
            var insidePoint = chunk.ConvertPosition(position);

            ModifyPoint(chunk, insidePoint, modifier);
        }
    }

    public Chunk ChunkAt(Vector3 position, bool forceLoad = true)
    {
        var cx = (int)(position.x / chunkSize);
        var cz = (int)(position.z / chunkSize);

        if (position.x < 0)
            cx--;
        if (position.z < 0)
            cz--;
        
        return ChunkAt(new ChunkPoint(cx, cz), forceLoad);
    }
    
    public GameObject ChunkObjectAt(Vector3 position, bool forceLoad = true)
    {
        var cx = (int)(position.x / chunkSize);
        var cz = (int)(position.z / chunkSize);

        return ChunkObjectAt(new ChunkPoint(cx, cz));
    }

    public ChunkPoint VectorToPoint(Vector3 position)
    {
        var cx = (int)(position.x / chunkSize);
        var cz = (int)(position.z / chunkSize);
        
        return new ChunkPoint(cx, cz);
    }

    public GameObject ChunkObjectAt(ChunkPoint point)
    {
        if (_goCache.ContainsKey(point))
            return _goCache[point];

        return null;
    }

    public Chunk LoadChunkAt(ChunkPoint point)
    {
        var x = point.X;
        var z = point.Z;
            
        var origin = new Vector3(x * voxelSize * chunkSize, 0.0f, z * voxelSize * chunkSize);
            
        Chunk c;
        int index = 0;
        do
        {
            c = chunkProviderQueue[index].LoadChunkAt(origin);
            index++;
        } while (c == null && index < chunkProviderQueue.Count);

        if (c != null)
        {
            Debug.Log("Loaded chunk " + point);
        }
        
        return c;
    }

    public void UnloadAll()
    {
        ChunkPoint[] keys = _chunks.Keys.ToArray();

        foreach (var point in keys)
        {
            UnloadChunk(_chunks[point]);
        }
    }

    public bool WithinRange(Vector3 position, Chunk chunk)
    {
        if (chunk == null)
            return false;
        
        ChunkPoint point = chunk.Position;
        
        var cx = (int)(position.x / chunkSize);
        var cz = (int)(position.z / chunkSize);
        
        var playerPoint = new ChunkPoint(cx, cz);

        var difference = playerPoint - point;

        return difference.magnitude <= maxMagnitude;
    }

    public ChunkPoint OriginToPoint(Vector3 origin)
    {
        int x = (int) ((origin.x / voxelSize) / chunkSize);
        int z = (int) ((origin.z / voxelSize) / chunkSize);
        
        return new ChunkPoint(x, z);
    }

    public void UnloadChunk(Chunk chunk)
    {
        var point = chunk.Position;
        
        Debug.Log("Unloading chunk " + point);

        if (_goCache.ContainsKey(point))
        {
            var go = _goCache[point];
            
            Destroy(go);
            
            _goCache.Remove(point);
        }

        if (_chunks.ContainsKey(point))
        {
            if (loader != null)
                loader.SaveChunk(chunk);
            
            chunk.Dispose();

            _chunks.Remove(point);
        }
    }

    public Vector3 PointToOrigin(ChunkPoint point)
    {
        return new Vector3(point.X * voxelSize * chunkSize, 0.0f, point.Z * voxelSize * chunkSize);
    }

    public Vector3 ChunkOrigin(Chunk c)
    {
        return PointToOrigin(c.Position);
    }

    public void SpawnChunk(Chunk nonNullChunk)
    {
        var point = nonNullChunk.Position;
        
        foreach (var processor in ChunkProcessors.OrderByDescending(c => c.priority))
        {
            processor.ProcessChunk(ref nonNullChunk);
        }
                
        //Get material from biome settings
        var biomeSettings = BiomeSettingsFor(nonNullChunk.BiomeData.type);
        
        var origin = new Vector3(nonNullChunk.Position.X * voxelSize * chunkSize, 0.0f, nonNullChunk.Position.Z * voxelSize * chunkSize);
        var gameObject = nonNullChunk.GenerateObject(biomeSettings.biomeMaterial, origin);
        //gameObject.transform.SetParent(gameObject.transform);
                
        _chunks.Add(point, nonNullChunk);
        _goCache.Add(point, gameObject);
    }

    void Update()
    {
        //Ensure the chunks for all players are loaded
        foreach (var player in players)
        {
            var point = VectorToPoint(player.transform.position);

            for (int xadd = (int)-viewDistance; xadd < viewDistance; xadd++)
            {
                for (int zadd = (int) -viewDistance; zadd < viewDistance; zadd++)
                {
                    var pointToCheck = new ChunkPoint(point.X + xadd, point.Z + zadd);

                    //First query to see if this chunk exists
                    var c = ChunkAt(pointToCheck, false);
                    
                    //If this chunk doesn't exist, then load it and spawn the game object
                    if (c == null)
                    {
                        //1. First frame will load it into the job queue, but return null
                        //2. Second frame will check the job status, and if its done, return the chunk, otherwise null
                        //3. Thrid frame will check the job status, and if its done return the chunk, otherwise null
                        //..
                        //..
                        c = LoadChunkAt(pointToCheck);
                        
                        //Ensure the chunk was loaded/generated successfully
                        if (c != null)
                        {
                            //Finally, spawn the chunk
                            SpawnChunk(c);
                        }
                    }
                }
            }
        }
    }
}
