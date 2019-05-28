#if UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_EDITOR_OSX
using UnityEditor;
using UnityEngine;

/*[CustomEditor(typeof(VoxelWorld))]
public class VoxelWorldEditor : Editor
{
    private Vector2 min;
    private Vector2 max;

    private void OnEnable()
    {
        VoxelWorld vWorld = (VoxelWorld) target;
        min = new Vector2(0, 0);
        max = new Vector2(vWorld.world.Width, vWorld.world.Depth);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        VoxelWorld vWorld = (VoxelWorld) target;

        min = EditorGUILayout.Vector2Field("Generate Min Bounds", min);
        max = EditorGUILayout.Vector2Field("Generate Max Bounds", max);

        if (GUILayout.Button("Delete World"))
        {
            Debug.Log("Unloading all chunks..");
            vWorld.world.UnloadAll();
        }
        
        if (GUILayout.Button("Generate World"))
        {
            Debug.Log("Unloading all chunks..");
            vWorld.world.UnloadAll();
            
            Debug.Log("Generating new chunks..");
            vWorld.Generate(min, max);
        }
    }
}*/
#endif