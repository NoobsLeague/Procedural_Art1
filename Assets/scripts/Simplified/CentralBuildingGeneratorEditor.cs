using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CentralBuildingGenerator))]
public class CentralBuildingGeneratorEditor : Editor
{
    private CentralBuildingGenerator generator;
    
    void OnEnable()
    {
        generator = (CentralBuildingGenerator)target;
    }
    
    public override void OnInspectorGUI()
    {
        // Header
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Central Building Generator", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Unified system for all building generation types", EditorStyles.miniLabel);
        
        GUILayout.Space(10);
        
        // Generation buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Building", GUILayout.Height(30)))
        {
            generator.GenerateBuilding();
        }
        if (GUILayout.Button("Clear Building", GUILayout.Height(30)))
        {
            generator.ClearBuilding();
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // Quick type selection buttons
        EditorGUILayout.LabelField("Quick Building Type Selection:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Type 1\n(Corner Walls)", GUILayout.Height(40)))
        {
            generator.settings.buildingType = BuildingType.Type1_CornerWalls;
            generator.GenerateBuilding();
        }
        if (GUILayout.Button("Type 2\n(Height Based)", GUILayout.Height(40)))
        {
            generator.settings.buildingType = BuildingType.Type2_HeightBasedPrefabs;
            generator.GenerateBuilding();
        }
        if (GUILayout.Button("Type 3\n(Windows)", GUILayout.Height(40)))
        {
            generator.settings.buildingType = BuildingType.Type3_CornerWallsWithWindows;
            generator.GenerateBuilding();
        }
        
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // Sign generation toggle
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Sign Generation (Universal):", EditorStyles.boldLabel);
        
        bool previousSignState = generator.settings.enableSignGeneration;
        generator.settings.enableSignGeneration = EditorGUILayout.Toggle("Enable Signs", generator.settings.enableSignGeneration);
        
        if (generator.settings.enableSignGeneration)
        {
            EditorGUILayout.LabelField("Signs can be added to any building type!", EditorStyles.miniLabel);
            if (GUILayout.Button("Regenerate with Current Settings"))
            {
                generator.GenerateBuilding();
            }
        }
        
        // Auto-regenerate if sign state changed
        if (previousSignState != generator.settings.enableSignGeneration)
        {
            generator.GenerateBuilding();
        }
        
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(15);
        
        // Building type info
        ShowBuildingTypeInfo();
        
        GUILayout.Space(10);
        
        // Default inspector
        DrawDefaultInspector();
        
        // Apply changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(generator);
        }
    }
    
    private void ShowBuildingTypeInfo()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Building Type Information:", EditorStyles.boldLabel);
        
        string info = "";
        switch (generator.settings.buildingType)
        {
            case BuildingType.Type1_CornerWalls:
                info = "Type 1: Generates prefabs only at the four corners of the building, randomly selecting from the corner prefab pool. No corners will be generated if the corner prefab pool is empty.";
                break;
            case BuildingType.Type2_HeightBasedPrefabs:
                info = "Type 2: Picks one prefab from the wall pool for every height level and applies it across the entire perimeter of that level.";
                break;
            case BuildingType.Type3_CornerWallsWithWindows:
                info = "Type 3: Places corner prefabs at corners (with wall fallback), walls at specified height intervals, and fills the rest with window prefabs.";
                break;
        }
        
        if (generator.settings.enableSignGeneration)
        {
            info += "\n\n🪧 SIGNS ENABLED: Neon signs will be generated on building walls according to the sign generation settings.";
        }
        
        EditorGUILayout.LabelField(info, EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
    }
}