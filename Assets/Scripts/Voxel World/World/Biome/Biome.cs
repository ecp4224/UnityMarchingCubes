using UnityEngine;

/// <summary>
/// Represents a Biome Configuration Script. This type can be attached
/// to a World object and the biome will be used by the BiomeProcessor
/// </summary>
[ExecuteInEditMode]
public abstract class Biome : MonoBehaviour
{
    [HideInInspector]
    public BiomeType biomeType;
    
    public int maxWidth = 3;
    public int maxHeight = 3;

    public int minWidth = 1;
    public int minHeight = 1;

    public Material biomeMaterial;
}