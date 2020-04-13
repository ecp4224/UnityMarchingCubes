using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu]
public class BlockTextures : ScriptableObject
{
    public List<Texture2D> Textures;

    public Texture2DArray ToArray()
    {
        var textures = new Texture2DArray(Textures[0].width, Textures[0].height, Textures.Count, TextureFormat.RGBA32, true, false);

        for (int i = 0; i < Textures.Count; i++)
        {
            var texture = Textures[i];
            
            textures.SetPixels(texture.GetPixels(0), i, 0);
        }

        textures.Apply();

        return textures;
    }

    [Button()]
    public void SaveAsset()
    {
        
    }
}
