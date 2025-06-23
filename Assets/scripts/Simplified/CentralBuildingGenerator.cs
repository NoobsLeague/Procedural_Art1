using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType
{
    Type1_CornerWalls = 1,
    Type2_HeightBasedPrefabs = 2,
    Type3_CornerWallsWithWindows = 3
}

[System.Serializable]
public class BuildingGenerationSettings
{
    [Header("General Settings")]
    public BuildingType buildingType = BuildingType.Type1_CornerWalls;
    public int buildingWidth = 10;
    public int buildingDepth = 10;
    public int buildingHeight = 5;
    
    [Header("Central Prefab Pools")]
    public GameObject[] wallPrefabs;
    public GameObject[] windowPrefabs;
    public GameObject[] cornerPrefabs;
    public GameObject[] signPrefabs;
    
    [Header("Type 3 Settings")]
    public int windowInterval = 5; // Every 5 height units place walls instead of windows
    
    [Header("Sign Generation (Any Type)")]
    public bool enableSignGeneration = false;
    [Range(0f, 1f)]
    public float signSpawnChance = 0.7f;
    public int signHeightInterval = 2; // Every X levels
    public int signStartHeight = 1; // Start placing signs from this height
    [Range(0.01f, 1f)]
    public float signWallOffset = 0.1f; // Distance from wall surface
}

public class CentralBuildingGenerator : MonoBehaviour
{
    [Header("Building Configuration")]
    public BuildingGenerationSettings settings;
    
    [Header("Generation Controls")]
    [SerializeField] private bool autoGenerateOnStart = true;
    [SerializeField] private bool clearPreviousBuilding = true;
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    
    private List<GameObject> generatedObjects = new List<GameObject>();
    private Transform buildingParent;
    
    void Start()
    {
        if (autoGenerateOnStart)
        {
            GenerateBuilding();
        }
    }
    
    [ContextMenu("Generate Building")]
    public void GenerateBuilding()
    {
        if (clearPreviousBuilding)
        {
            ClearPreviousBuilding();
        }
        
        CreateBuildingParent();
        
        switch (settings.buildingType)
        {
            case BuildingType.Type1_CornerWalls:
                GenerateType1Building();
                break;
            case BuildingType.Type2_HeightBasedPrefabs:
                GenerateType2Building();
                break;
            case BuildingType.Type3_CornerWallsWithWindows:
                GenerateType3Building();
                break;
        }
        
        // Generate signs if enabled (works with any building type)
        if (settings.enableSignGeneration)
        {
            GenerateSigns();
        }
        
        Debug.Log($"Generated {settings.buildingType} building with {generatedObjects.Count} objects");
    }
    
    private void CreateBuildingParent()
    {
        if (buildingParent == null)
        {
            GameObject parentObj = new GameObject($"Building_{settings.buildingType}");
            parentObj.transform.parent = transform;
            parentObj.transform.localPosition = Vector3.zero;
            buildingParent = parentObj.transform;
        }
    }
    
    private void ClearPreviousBuilding()
    {
        foreach (GameObject obj in generatedObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        generatedObjects.Clear();
        
        if (buildingParent != null)
        {
            DestroyImmediate(buildingParent.gameObject);
            buildingParent = null;
        }
    }
    
    #region Type 1 - Corner Walls Only
    private void GenerateType1Building()
    {
        // Only generate corners if corner prefabs are available
        if (settings.cornerPrefabs == null || settings.cornerPrefabs.Length == 0)
        {
            Debug.LogWarning("Type 1 building: No corner prefabs assigned. No corners will be generated.");
            return;
        }
        
        // Generate corner walls only, randomly picking from central corner pool
        for (int height = 0; height < settings.buildingHeight; height++)
        {
            // Four corners of the building
            Vector3[] cornerPositions = {
                new Vector3(0, height, 0),                                    // Front-left
                new Vector3(settings.buildingWidth, height, 0),               // Front-right
                new Vector3(0, height, settings.buildingDepth),               // Back-left
                new Vector3(settings.buildingWidth, height, settings.buildingDepth) // Back-right
            };
            
            foreach (Vector3 position in cornerPositions)
            {
                GameObject cornerPrefab = GetRandomPrefabFromPool(settings.cornerPrefabs);
                if (cornerPrefab != null)
                    SpawnObject(cornerPrefab, position, Quaternion.identity);
            }
        }
    }
    #endregion
    
    #region Type 2 - Height Based Prefabs
    private void GenerateType2Building()
    {
        // Pick one prefab for every height increase from central wall pool
        for (int height = 0; height < settings.buildingHeight; height++)
        {
            GameObject selectedPrefab = GetRandomPrefabFromPool(settings.wallPrefabs);
            
            if (selectedPrefab != null)
            {
                // Generate around the perimeter using the same prefab for this height level
                for (int x = 0; x <= settings.buildingWidth; x++)
                {
                    for (int z = 0; z <= settings.buildingDepth; z++)
                    {
                        // Only place on perimeter
                        if (x == 0 || x == settings.buildingWidth || z == 0 || z == settings.buildingDepth)
                        {
                            Vector3 position = new Vector3(x, height, z);
                            SpawnObject(selectedPrefab, position, Quaternion.identity);
                        }
                    }
                }
            }
        }
    }
    #endregion
    
    #region Type 3 - Corner Walls with Windows
    private void GenerateType3Building()
    {
        for (int height = 0; height < settings.buildingHeight; height++)
        {
            // Generate perimeter
            for (int x = 0; x <= settings.buildingWidth; x++)
            {
                for (int z = 0; z <= settings.buildingDepth; z++)
                {
                    if (x == 0 || x == settings.buildingWidth || z == 0 || z == settings.buildingDepth)
                    {
                        Vector3 position = new Vector3(x, height, z);
                        bool isCorner = (x == 0 || x == settings.buildingWidth) && (z == 0 || z == settings.buildingDepth);
                        bool isWallHeight = (height % settings.windowInterval == 0 && height > 0);
                        
                        GameObject prefabToUse = null;
                        
                        if (isCorner)
                        {
                            // Only place corners if corner prefabs are available
                            prefabToUse = GetRandomPrefabFromPool(settings.cornerPrefabs);
                            if (prefabToUse == null)
                                prefabToUse = GetRandomPrefabFromPool(settings.wallPrefabs); // Fallback to walls
                        }
                        else if (isWallHeight)
                        {
                            // Place walls at specified intervals
                            prefabToUse = GetRandomPrefabFromPool(settings.wallPrefabs);
                        }
                        else
                        {
                            // Fill with windows
                            prefabToUse = GetRandomPrefabFromPool(settings.windowPrefabs);
                        }
                        
                        if (prefabToUse != null)
                            SpawnObject(prefabToUse, position, Quaternion.identity);
                    }
                }
            }
        }
    }
    #endregion
    
    #region Sign Generation (Universal)
    private void GenerateSigns()
    {
        // Generate signs on walls at specified intervals
        for (int height = settings.signStartHeight; height < settings.buildingHeight; height += settings.signHeightInterval)
        {
            // Front and back walls
            for (int x = 1; x < settings.buildingWidth; x++)
            {
                if (Random.value < settings.signSpawnChance)
                {
                    Vector3 frontPosition = new Vector3(x, height, -settings.signWallOffset);
                    SpawnSign(frontPosition, Quaternion.identity);
                }
                
                if (Random.value < settings.signSpawnChance)
                {
                    Vector3 backPosition = new Vector3(x, height, settings.buildingDepth + settings.signWallOffset);
                    SpawnSign(backPosition, Quaternion.Euler(0, 180, 0));
                }
            }
            
            // Left and right walls
            for (int z = 1; z < settings.buildingDepth; z++)
            {
                if (Random.value < settings.signSpawnChance)
                {
                    Vector3 leftPosition = new Vector3(-settings.signWallOffset, height, z);
                    SpawnSign(leftPosition, Quaternion.Euler(0, 90, 0));
                }
                
                if (Random.value < settings.signSpawnChance)
                {
                    Vector3 rightPosition = new Vector3(settings.buildingWidth + settings.signWallOffset, height, z);
                    SpawnSign(rightPosition, Quaternion.Euler(0, -90, 0));
                }
            }
        }
    }
    
    private void SpawnSign(Vector3 position, Quaternion rotation)
    {
        GameObject signPrefab = GetRandomPrefabFromPool(settings.signPrefabs);
        
        if (signPrefab != null)
        {
            SpawnObject(signPrefab, position, rotation);
        }
    }
    #endregion
    
    #region Utility Methods
    private GameObject GetRandomPrefabFromPool(GameObject[] prefabPool)
    {
        if (prefabPool == null || prefabPool.Length == 0)
            return null;
        
        return prefabPool[Random.Range(0, prefabPool.Length)];
    }
    #endregion
    
    private GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;
        
        GameObject instance = Instantiate(prefab, buildingParent);
        instance.transform.localPosition = position;
        instance.transform.localRotation = rotation;
        
        generatedObjects.Add(instance);
        return instance;
    }
    
    [ContextMenu("Clear Building")]
    public void ClearBuilding()
    {
        ClearPreviousBuilding();
    }
    
    #region Gizmos
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + new Vector3(settings.buildingWidth / 2f, settings.buildingHeight / 2f, settings.buildingDepth / 2f);
        Vector3 size = new Vector3(settings.buildingWidth, settings.buildingHeight, settings.buildingDepth);
        Gizmos.DrawWireCube(center, size);
        
        // Draw base
        Gizmos.color = Color.green;
        Vector3 baseCenter = transform.position + new Vector3(settings.buildingWidth / 2f, 0, settings.buildingDepth / 2f);
        Vector3 baseSize = new Vector3(settings.buildingWidth, 0.1f, settings.buildingDepth);
        Gizmos.DrawWireCube(baseCenter, baseSize);
    }
    #endregion
}