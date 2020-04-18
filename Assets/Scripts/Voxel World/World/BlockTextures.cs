using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu]
public class BlockTextures : ScriptableObject
{
    public Texture2D defaultTexture;
    
    public List<Texture2D> Textures;
    public List<Texture2D> Normals;
    public List<Texture2D> Smoothness;
    public List<Texture2D> AmbientOcclusion;

    private Texture2DArray _cache;

    private Vector2 GetSize()
    {
        foreach (var texture in Textures)
        {
            if (texture == null)
                continue;
            
            return new Vector2(texture.width, texture.height);
        }
        
        foreach (var texture in Normals)
        {
            if (texture == null)
                continue;
            
            return new Vector2(texture.width, texture.height);
        }
        
        foreach (var texture in Smoothness)
        {
            if (texture == null)
                continue;
            
            return new Vector2(texture.width, texture.height);
        }
        
        foreach (var texture in AmbientOcclusion)
        {
            if (texture == null)
                continue;
            
            return new Vector2(texture.width, texture.height);
        }

        return new Vector2();
    }

    public Texture2DArray ToArray()
    {
        if (_cache != null)
            return _cache;

        var size = GetSize();
        
        _cache = new Texture2DArray((int)size.x, (int)size.y, Textures.Count * 4, TextureFormat.RGBA32, true, false);

        int index = 0;
        for (int i = 0; i < Textures.Count; i++)
        {
            var texture = Textures[i];

            if (texture == null)
                texture = defaultTexture;
            
            _cache.SetPixels(texture.GetPixels(0), index, 0);

            index++;
        }
        
        for (int i = 0; i < Normals.Count; i++)
        {
            var texture = Normals[i];

            if (texture == null)
                texture = defaultTexture;
            
            _cache.SetPixels(texture.GetPixels(0), index, 0);

            index++;
        }
        
        for (int i = 0; i < Smoothness.Count; i++)
        {
            var texture = Smoothness[i];

            if (texture == null)
                texture = defaultTexture;
            
            _cache.SetPixels(texture.GetPixels(0), index, 0);

            index++;
        }
        
        for (int i = 0; i < AmbientOcclusion.Count; i++)
        {
            var texture = AmbientOcclusion[i];

            if (texture == null)
                texture = defaultTexture;
            
            _cache.SetPixels(texture.GetPixels(0), index, 0);

            index++;
        }

        _cache.Apply();

        return _cache;
    }

    [Button()]
    public void SaveAsset()
    {
        
    }
}
