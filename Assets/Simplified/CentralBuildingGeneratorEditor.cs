#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Demo {
    [CustomEditor(typeof(CentralBuildingGenerator))]
    public class CentralBuildingGeneratorEditor : Editor {
        private CentralBuildingGenerator generator;
        
        void OnEnable() {
            generator = (CentralBuildingGenerator)target;
        }
        
        public override void OnInspectorGUI() {
            // Header
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Central Building Generator", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Unified system for all building generation types", EditorStyles.miniLabel);
            
            GUILayout.Space(10);
            
            // Generation buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Building", GUILayout.Height(30))) {
                generator.Generate();
            }
            if (GUILayout.Button("Clear Building", GUILayout.Height(30))) {
                generator.DeleteGenerated();
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Quick type selection buttons
            EditorGUILayout.LabelField("Quick Building Type Selection:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Type 1\nCorner Walls", GUILayout.Height(40))) {
                generator.buildingType = BuildingGenerationType.Type1_CornerWallsOnly;
                generator.Generate();
            }
            if (GUILayout.Button("Type 2\nHeight Based", GUILayout.Height(40))) {
                generator.buildingType = BuildingGenerationType.Type2_HeightBasedPrefabs;
                generator.Generate();
            }
            if (GUILayout.Button("Type 3\nCorners + Windows", GUILayout.Height(40))) {
                generator.buildingType = BuildingGenerationType.Type3_CornerWallsWithWindows;
                generator.Generate();
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Type descriptions
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Building Type Descriptions:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Type 1: Only walls for corners, random wall prefabs", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• Type 2: One consistent prefab per height level", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• Type 3: Corners always walls, windows fill rest", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Universal sign generation
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Sign Generation (Universal):", EditorStyles.boldLabel);
            
            bool previousSignState = generator.enableSignGeneration;
            generator.enableSignGeneration = EditorGUILayout.Toggle("Enable Signs", generator.enableSignGeneration);
            
            if (generator.enableSignGeneration != previousSignState) {
                EditorUtility.SetDirty(generator);
            }
            
            if (generator.enableSignGeneration) {
                EditorGUILayout.LabelField("Signs can be added to any building type!", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("Make sure to assign neon sign prefabs below", EditorStyles.miniLabel);
                
                EditorGUILayout.Space(5);
                
                // Sign configuration controls
                generator.signHeightThreshold = EditorGUILayout.Slider("Sign Height Threshold", generator.signHeightThreshold, 0.1f, 0.9f);
                EditorGUILayout.LabelField($"Signs appear above {generator.signHeightThreshold:P0} of building height", EditorStyles.miniLabel);
                
                generator.signSpawnChance = EditorGUILayout.Slider("Sign Spawn Chance", generator.signSpawnChance, 0.1f, 1f);
                EditorGUILayout.LabelField($"{generator.signSpawnChance:P0} chance per wall to have a sign", EditorStyles.miniLabel);
                
                generator.signWallOffset = EditorGUILayout.Slider("Sign Wall Offset", generator.signWallOffset, -1f, 1f);
                EditorGUILayout.LabelField("Positive = Outside walls, Negative = Inside walls", EditorStyles.miniLabel);
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Sign Size: Up to (Wall Length - 2) width, unlimited height", EditorStyles.miniLabel);
                
                if (GUILayout.Button("Regenerate with Signs")) {
                    generator.Generate();
                }
            }
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Expansion settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Building Expansion:", EditorStyles.boldLabel);
            
            bool previousExpansionState = generator.enableExpansion;
            generator.enableExpansion = EditorGUILayout.Toggle("Enable Expansion", generator.enableExpansion);
            
            if (generator.enableExpansion != previousExpansionState) {
                EditorUtility.SetDirty(generator);
            }
            
            if (generator.enableExpansion) {
                EditorGUILayout.LabelField("Building will expand outward when reaching max height", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("Forces flat roof for proper expansion platforms", EditorStyles.miniLabel);
                
                EditorGUILayout.Space(5);
                generator.expansionHeight = EditorGUILayout.IntField("Expansion Height", generator.expansionHeight);
                EditorGUILayout.LabelField("Height of building after expansion", EditorStyles.miniLabel);
                
                generator.expansionLayers = EditorGUILayout.IntField("Expansion Layers", generator.expansionLayers);
                EditorGUILayout.LabelField("Number of times to expand (1 = one expansion)", EditorStyles.miniLabel);
                
                if (GUILayout.Button("Generate Expanded Building")) {
                    generator.Generate();
                }
            }
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Show type-specific settings
            if (generator.buildingType == BuildingGenerationType.Type3_CornerWallsWithWindows) {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Type 3 Settings:", EditorStyles.boldLabel);
                generator.windowInterval = EditorGUILayout.IntField("Window Interval (Wall every X height)", generator.windowInterval);
                EditorGUILayout.LabelField("Creates walls every " + generator.windowInterval + " height levels", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
            }
            
            // Default inspector for the rest of the properties
            DrawDefaultInspector();
            
            // Validation warnings
            GUILayout.Space(10);
            ValidateSettings();
        }
        
        void ValidateSettings() {
            bool hasWarnings = false;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Validation:", EditorStyles.boldLabel);
            
            if (generator.wallPrefabs == null || generator.wallPrefabs.Length == 0) {
                EditorGUILayout.HelpBox("Wall Prefabs are required for all building types!", MessageType.Error);
                hasWarnings = true;
            }
            
            if (generator.buildingType == BuildingGenerationType.Type3_CornerWallsWithWindows && 
                (generator.windowPrefabs == null || generator.windowPrefabs.Length == 0)) {
                EditorGUILayout.HelpBox("Window Prefabs are required for Type 3 buildings!", MessageType.Warning);
                hasWarnings = true;
            }
            
            if (generator.enableSignGeneration && 
                (generator.neonSignPrefabs == null || generator.neonSignPrefabs.Length == 0)) {
                EditorGUILayout.HelpBox("Neon Sign Prefabs are required when sign generation is enabled!", MessageType.Warning);
                hasWarnings = true;
            }
            
            if (generator.roofPrefabs == null || generator.roofPrefabs.Length == 0) {
                EditorGUILayout.HelpBox("Roof Prefabs are recommended for proper roof generation!", MessageType.Info);
                hasWarnings = true;
            }
            
            if (generator.enableExpansion && generator.expansionHeight <= 0) {
                EditorGUILayout.HelpBox("Expansion Height must be greater than 0!", MessageType.Warning);
                hasWarnings = true;
            }
            
            if (generator.enableExpansion && generator.expansionLayers <= 0) {
                EditorGUILayout.HelpBox("Expansion Layers must be greater than 0!", MessageType.Warning);
                hasWarnings = true;
            }
            
            if (!hasWarnings) {
                EditorGUILayout.LabelField("✓ All settings look good!", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndVertical();
        }
    }
}
#endif