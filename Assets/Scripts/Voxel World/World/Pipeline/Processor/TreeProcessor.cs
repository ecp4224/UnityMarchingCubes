using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeProcessor : ChunkProcessor
{
    [HideInInspector]
    [BindComponent]
    private World world;

    [Serializable]
    public struct BiomeSpawnSetting
    {
        public BiomeType Type;
        public float spawnRate;
        public float minimumDistance;
        public List<GameObject> Prefabs;
    }

    public List<BiomeSpawnSetting> BiomeSpawnSettings;
    
    private Dictionary<BiomeType, BiomeSpawnSetting> treePrefabs = new Dictionary<BiomeType, BiomeSpawnSetting>();

    void Start()
    {
        foreach (var biomeSetting in BiomeSpawnSettings)
        {
            treePrefabs.Add(biomeSetting.Type, biomeSetting);
        }
    }
    
    public override void ProcessChunk(ref Chunk chunk)
    {
        var spawned = new List<Vector3>();
        var origin = world.ChunkOrigin(chunk);
        var settings = treePrefabs[chunk.BiomeData.type];

        for (var x = 0; x < world.chunkSize; x++)
        {
            for (var y = 0; y < world.chunkSize; y++)
            {
                for (var z = 0; z < world.chunkSize; z++)
                {
                    var point = origin + new Vector3(x, y, z);

                    //TODO Spawn tree at given point
                    RaycastHit hit;
                    var ray = new Ray(point, -transform.up);

                    if (Physics.Raycast(ray, out hit, 5))
                    {
                        var spawnPoint = hit.point;

                        if (spawned.Contains(spawnPoint)) continue;

                        bool canSpawn = true;
                        foreach (var p in spawned)
                        {
                            //Convert to 2D space, so we are sure we don't spawn on top of other trees
                            var a = new Vector2(spawnPoint.x, spawnPoint.z);
                            var b = new Vector2(p.x, p.z);
                            
                            var distance = Vector2.Distance(a, b);

                            if (distance <= settings.minimumDistance)
                            {
                                canSpawn = false;
                                break;
                            }
                        }

                        if (!canSpawn) continue;

                        if (Random.Range(0f, 1f) < settings.spawnRate)
                        {
                            //Get a random prefab from the setting's prefabs
                            var prefab = settings.Prefabs[Random.Range(0, settings.Prefabs.Count)];
                            
                            //Now spawn it!
                            var tree = Instantiate(prefab, spawnPoint, Quaternion.identity);
                            
                            //Make the chunk aware of this tree
                            chunk.AddEntity(tree);
                            
                            Debug.Log("Spawn tree at " + spawnPoint);
                            
                            spawned.Add(spawnPoint);
                        }
                    }
                }
            }
        }
        
        spawned.Clear();
    }
}