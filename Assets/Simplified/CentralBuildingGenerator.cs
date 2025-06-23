using UnityEngine;

namespace Demo {
    public enum BuildingGenerationType {
        Type1_CornerWallsOnly = 1,
        Type2_HeightBasedPrefabs = 2, 
        Type3_CornerWallsWithWindows = 3
    }

    public class CentralBuildingGenerator : Shape {
        [Header("Building Configuration")]
        [SerializeField] public BuildingGenerationType buildingType = BuildingGenerationType.Type1_CornerWallsOnly;
        [SerializeField] public int buildingWidth = 10;
        [SerializeField] public int buildingDepth = 10;
        [SerializeField] public int buildingHeight = 5;
        [SerializeField] public bool forceFlatRoof = false;

        [Header("Expansion Settings")]
        [SerializeField] public bool enableExpansion = false;
        [SerializeField] public int expansionHeight = 3; // Additional height after expansion
        [SerializeField] public int expansionLayers = 1; // How many times to expand (1 = expand once)

        [Header("Prefab Collections")]
        [SerializeField] public GameObject[] wallPrefabs;
        [SerializeField] public GameObject[] windowPrefabs;
        [SerializeField] public GameObject[] roofPrefabs;
        [SerializeField] public GameObject cornerPrefab;

        [Header("Type 3 Settings")]
        [SerializeField] public int windowInterval = 5; // Every 5 height units place walls instead of windows

        [Header("Sign Generation (Universal)")]
        [SerializeField] public bool enableSignGeneration = false;
        [SerializeField] public GameObject[] neonSignPrefabs;
        [SerializeField] [Range(-1f, 1f)] public float signWallOffset = 0.1f; // Positive = outward, Negative = inward
        [SerializeField] [Range(0.1f, 0.9f)] public float signHeightThreshold = 0.4f; // Only generate signs above this % of building height
        [SerializeField] [Range(0.1f, 1f)] public float signSpawnChance = 0.7f; // Chance for each wall to have a sign

        protected override void Execute() {
            switch (buildingType) {
                case BuildingGenerationType.Type1_CornerWallsOnly:
                    GenerateType1Building();
                    break;
                case BuildingGenerationType.Type2_HeightBasedPrefabs:
                    GenerateType2Building();
                    break;
                case BuildingGenerationType.Type3_CornerWallsWithWindows:
                    GenerateType3Building();
                    break;
            }
        }

        // Type 1: Only walls for corners, randomly picking prefabs for walls (uses SimpleStock1 style)
        void GenerateType1Building() {
            CentralStock type1Stock = CreateSymbol<CentralStock>("Type1Stock");
            type1Stock.Initialize(
                buildingWidth, buildingDepth, wallPrefabs, windowPrefabs, 
                roofPrefabs, cornerPrefab, forceFlatRoof, buildingHeight, 0,
                BuildingGenerationType.Type1_CornerWallsOnly, windowInterval, 
                enableSignGeneration ? neonSignPrefabs : null, signWallOffset, signHeightThreshold, signSpawnChance
            );
            type1Stock.Generate(buildDelay);
        }

        // Type 2: Pick one prefab for every height increase (uses SimpleStock2 style)
        void GenerateType2Building() {
            CentralStock type2Stock = CreateSymbol<CentralStock>("Type2Stock");
            type2Stock.Initialize(
                buildingWidth, buildingDepth, wallPrefabs, windowPrefabs, 
                roofPrefabs, cornerPrefab, forceFlatRoof, buildingHeight, 0,
                BuildingGenerationType.Type2_HeightBasedPrefabs, windowInterval, 
                enableSignGeneration ? neonSignPrefabs : null, signWallOffset, signHeightThreshold, signSpawnChance
            );
            type2Stock.Generate(buildDelay);
        }

        // Type 3: Every corner a wall, every 5 height a wall, windows fill the rest (uses SimpleStock3 style)
        void GenerateType3Building() {
            CentralStock type3Stock = CreateSymbol<CentralStock>("Type3Stock");
            type3Stock.Initialize(
                buildingWidth, buildingDepth, wallPrefabs, windowPrefabs, 
                roofPrefabs, cornerPrefab, forceFlatRoof, buildingHeight, 0,
                BuildingGenerationType.Type3_CornerWallsWithWindows, windowInterval, 
                enableSignGeneration ? neonSignPrefabs : null, signWallOffset, signHeightThreshold, signSpawnChance
            );
            type3Stock.Generate(buildDelay);
        }
    }

    public class CentralStock : Shape {
        // Shape parameters
        int Width;
        int Depth;
        GameObject[] wallPrefabs;
        GameObject[] windowPrefabs;
        GameObject[] roofStyle;
        GameObject cornerPrefab;
        bool forceFlatRoof;
        int desiredHeight;
        int currentHeight;
        BuildingGenerationType generationType;
        int windowInterval;
        GameObject[] neonPrefabs;
        float signWallOffset;
        float signHeightThreshold;
        float signSpawnChance;
        bool enableExpansion;
        int expansionHeight;
        int expansionLayers;
        int currentExpansions;

        public void Initialize(int Width, int Depth, GameObject[] wallPrefabs, GameObject[] windowPrefabs, 
                             GameObject[] roofStyle, GameObject cornerPrefab, bool forceFlatRoof, 
                             int desiredHeight = 5, int currentHeight = 0, 
                             BuildingGenerationType generationType = BuildingGenerationType.Type1_CornerWallsOnly,
                             int windowInterval = 5, GameObject[] neonPrefabs = null, float signOffset = 0.1f,
                             float signHeightThreshold = 0.4f, float signSpawnChance = 0.7f,
                             bool enableExpansion = false, int expansionHeight = 3, int expansionLayers = 1, int currentExpansions = 0) {
            this.Width = Width;
            this.Depth = Depth;
            this.wallPrefabs = wallPrefabs;
            this.windowPrefabs = windowPrefabs;
            this.roofStyle = roofStyle;
            this.cornerPrefab = cornerPrefab;
            this.forceFlatRoof = forceFlatRoof;
            this.desiredHeight = desiredHeight;
            this.currentHeight = currentHeight;
            this.generationType = generationType;
            this.windowInterval = windowInterval;
            this.neonPrefabs = neonPrefabs;
            this.signWallOffset = signOffset;
            this.signHeightThreshold = signHeightThreshold;
            this.signSpawnChance = signSpawnChance;
            this.enableExpansion = enableExpansion;
            this.expansionHeight = expansionHeight;
            this.expansionLayers = expansionLayers;
            this.currentExpansions = currentExpansions;
        }

        protected override void Execute() {
            // Create four walls based on building type
            for (int i = 0; i < 4; i++) {
                Vector3 localPosition = new Vector3();
                switch (i) {
                    case 0: localPosition = new Vector3(-(Width - 1) * 0.5f, 0, 0); break; // left
                    case 1: localPosition = new Vector3(0, 0, (Depth - 1) * 0.5f); break; // back
                    case 2: localPosition = new Vector3((Width - 1) * 0.5f, 0, 0); break; // right
                    case 3: localPosition = new Vector3(0, 0, -(Depth - 1) * 0.5f); break; // front
                }

                CreateWallBasedOnType(i, localPosition);
            }

            // Check height and build another stock, expand, or roof
            if (currentHeight + 1 < desiredHeight) {
                BuildNextStock();
                BuildCorners();
            } else if (enableExpansion && currentExpansions < expansionLayers) {
                // Time to expand - create expansion level
                CreateExpansionLevel();
            } else {
                BuildRoof();
                BuildCorners();
            }

            // Generate signs for any type if enabled (only at ground level to avoid duplicates)
            if (neonPrefabs != null && neonPrefabs.Length > 0 && currentHeight == 0) {
                GenerateNeonSigns();
            }
        }

        void CreateWallBasedOnType(int wallIndex, Vector3 localPosition) {
            switch (generationType) {
                case BuildingGenerationType.Type1_CornerWallsOnly:
                    CreateType1Wall(wallIndex, localPosition);
                    break;
                case BuildingGenerationType.Type2_HeightBasedPrefabs:
                    CreateType2Wall(wallIndex, localPosition);
                    break;
                case BuildingGenerationType.Type3_CornerWallsWithWindows:
                    CreateType3Wall(wallIndex, localPosition);
                    break;
            }
        }

        // Type 1: Only walls for corners, randomly picking prefabs for walls
        void CreateType1Wall(int wallIndex, Vector3 localPosition) {
            CentralRow newRow = CreateSymbol<CentralRow>("wall", localPosition, Quaternion.Euler(0, wallIndex * 90, 0));
            newRow.Initialize(wallIndex % 2 == 1 ? Width : Depth, wallPrefabs, windowPrefabs, Vector3.zero, BuildingGenerationType.Type1_CornerWallsOnly);
            newRow.Generate();
        }

        // Type 2: Pick one prefab for every height increase
        void CreateType2Wall(int wallIndex, Vector3 localPosition) {
            CentralRow newRow = CreateSymbol<CentralRow>("wall", localPosition, Quaternion.Euler(0, wallIndex * 90, 0));
            newRow.Initialize(wallIndex % 2 == 1 ? Width : Depth, wallPrefabs, windowPrefabs, Vector3.zero, BuildingGenerationType.Type2_HeightBasedPrefabs);
            newRow.Generate();
        }

        // Type 3: Every corner a wall, every windowInterval height a wall, windows fill the rest
        void CreateType3Wall(int wallIndex, Vector3 localPosition) {
            bool isWallFloor = (currentHeight % windowInterval == 0);
            CentralRow newRow = CreateSymbol<CentralRow>("wall", localPosition, Quaternion.Euler(0, wallIndex * 90, 0));
            newRow.Initialize(wallIndex % 2 == 1 ? Width : Depth, wallPrefabs, windowPrefabs, Vector3.zero, 
                            BuildingGenerationType.Type3_CornerWallsWithWindows, isWallFloor);
            newRow.Generate();
        }

        void BuildNextStock() {
            CentralStock nextStock = CreateSymbol<CentralStock>("stock", new Vector3(0, 1, 0));
            nextStock.Initialize(Width, Depth, wallPrefabs, windowPrefabs, roofStyle, cornerPrefab, 
                               forceFlatRoof, desiredHeight, currentHeight + 1, generationType, windowInterval, neonPrefabs, 
                               signWallOffset, signHeightThreshold, signSpawnChance, enableExpansion, expansionHeight, expansionLayers, currentExpansions);
            nextStock.Generate(buildDelay);
        }

        void CreateExpansionLevel() {
            // Create flat roof at current level first
            CreateFlatExpansionRoof();
            
            // Expand dimensions by 1 unit on each side (2 total per dimension)
            int newWidth = Width + 2;
            int newDepth = Depth + 2;
            
            // Create expanded building section
            CentralStock expandedStock = CreateSymbol<CentralStock>("expandedStock", new Vector3(0, 1, 0));
            expandedStock.Initialize(newWidth, newDepth, wallPrefabs, windowPrefabs, roofStyle, cornerPrefab, 
                                   forceFlatRoof, expansionHeight, 0, generationType, windowInterval, neonPrefabs, 
                                   signWallOffset, signHeightThreshold, signSpawnChance, enableExpansion, expansionHeight, expansionLayers, currentExpansions + 1);
            expandedStock.Generate(buildDelay);
        }

        void CreateFlatExpansionRoof() {
            // Create a flat roof platform at the current level to support the expansion
            for (int x = 0; x < Width; x++) {
                for (int z = 0; z < Depth; z++) {
                    Vector3 roofPosition = new Vector3(
                        x - (Width - 1) * 0.5f, 
                        1, 
                        z - (Depth - 1) * 0.5f
                    );
                    
                    GameObject roofPrefab = roofStyle[RandomInt(roofStyle.Length)];
                    SpawnPrefab(roofPrefab, roofPosition, Quaternion.identity);
                }
            }
        }

        void BuildRoof() {
            CentralRoof nextRoof = CreateSymbol<CentralRoof>("roof", new Vector3(0, 1, 0));
            nextRoof.Initialize(Width, Depth, wallPrefabs, windowPrefabs, roofStyle, forceFlatRoof, cornerPrefab);
            nextRoof.Generate(buildDelay);
        }

        void BuildCorners() {
            Corners corners = CreateSymbol<Corners>("corners", Vector3.zero);
            corners.Initialize(Width, Depth, cornerPrefab);
            corners.Generate();
        }

        void GenerateNeonSigns() {
            if (neonPrefabs == null || neonPrefabs.Length == 0)
                return;

            // Calculate the height where signs should start appearing
            int signStartHeight = Mathf.RoundToInt(desiredHeight * signHeightThreshold);
            
            for (int i = 0; i < 4; i++) {
                // Use the configurable spawn chance
                if (Random.value > signSpawnChance) continue;
                
                GameObject prefab = neonPrefabs[Random.Range(0, neonPrefabs.Length)];

                // Match the EXACT wall positions from CentralStock
                Vector3 wallBasePosition = i switch {
                    0 => new Vector3(-(Width - 1) * 0.5f, 0, 0), // left
                    1 => new Vector3(0, 0, (Depth - 1) * 0.5f), // back
                    2 => new Vector3((Width - 1) * 0.5f, 0, 0), // right
                    3 => new Vector3(0, 0, -(Depth - 1) * 0.5f), // front
                    _ => Vector3.zero
                };

                // Match the rotation from CentralStock (walls rotate by i * 90)
                Quaternion wallRotation = Quaternion.Euler(0, i * 90, 0);
                
                // Determine the wall length for this side (same logic as CentralRow)
                int wallLength = (i % 2 == 0) ? Depth : Width;

                // Sign dimensions - much larger and proportional to building
                // Maximum width: wall length - 2 units (as requested), minimum 3 units
                int maxSignWidth = Mathf.Max(3, wallLength - 2);
                int signW = Random.Range(Mathf.Max(3, maxSignWidth / 2), maxSignWidth + 1);
                
                // Sign height: from threshold height to top of building
                int availableHeight = desiredHeight - signStartHeight;
                int minSignHeight = Mathf.Max(2, availableHeight / 4); // At least 25% of available height
                int maxSignHeight = Mathf.Max(minSignHeight, availableHeight); // Can go to the top
                int signH = Random.Range(minSignHeight, maxSignHeight + 1);

                // Calculate starting position along the wall (center the large sign)
                int startAlongWall = (wallLength - signW) / 2;

                for (int y = 0; y < signH; y++) {
                    for (int x = 0; x < signW; x++) {
                        // Calculate position along the wall (same logic as SimpleRow)
                        float positionAlongWall = (startAlongWall + x) - (wallLength - 1) / 2f;
                        
                        // Calculate the sign position based on wall orientation
                        Vector3 signLocalPosition = Vector3.zero;
                        
                        // Y position starts from the threshold height
                        float yPosition = signStartHeight + y;
                        
                        switch (i) {
                            case 0: // Left wall - signs face right, positioned to the left of wall
                                signLocalPosition = wallBasePosition + new Vector3(-signWallOffset, yPosition, positionAlongWall);
                                break;
                            case 1: // Back wall - signs face forward, positioned behind wall  
                                signLocalPosition = wallBasePosition + new Vector3(positionAlongWall, yPosition, signWallOffset);
                                break;
                            case 2: // Right wall - signs face left, positioned to the right of wall
                                signLocalPosition = wallBasePosition + new Vector3(signWallOffset, yPosition, positionAlongWall);
                                break;
                            case 3: // Front wall - signs face backward, positioned in front of wall
                                signLocalPosition = wallBasePosition + new Vector3(positionAlongWall, yPosition, -signWallOffset);
                                break;
                        }
                        
                        // Create the sign with proper rotation and position
                        SpawnPrefab(prefab, signLocalPosition, wallRotation);
                    }
                }
            }
        }
    }

    public class CentralRow : Shape {
        int Number;
        GameObject[] wallPrefabs;
        GameObject[] windowPrefabs;
        Vector3 direction;
        BuildingGenerationType generationType;
        bool forceWallFloor = false;

        public void Initialize(int Number, GameObject[] wallPrefabs, GameObject[] windowPrefabs, 
                             Vector3 dir = new Vector3(), BuildingGenerationType generationType = BuildingGenerationType.Type1_CornerWallsOnly,
                             bool forceWallFloor = false) {
            this.Number = Number;
            this.wallPrefabs = wallPrefabs;
            this.windowPrefabs = windowPrefabs;
            this.generationType = generationType;
            this.forceWallFloor = forceWallFloor;
            
            if (dir.magnitude != 0) {
                direction = dir;
            } else {
                direction = new Vector3(0, 0, 1);
            }
        }

        protected override void Execute() {
            if (Number <= 0)
                return;

            switch (generationType) {
                case BuildingGenerationType.Type1_CornerWallsOnly:
                    ExecuteType1Row();
                    break;
                case BuildingGenerationType.Type2_HeightBasedPrefabs:
                    ExecuteType2Row();
                    break;
                case BuildingGenerationType.Type3_CornerWallsWithWindows:
                    ExecuteType3Row();
                    break;
            }
        }

        // Type 1: Only walls for corners, randomly picking prefabs for walls
        void ExecuteType1Row() {
            bool corner = Number > 2;

            for (int i = 0; i < Number; i++) {
                bool isCorner = corner && (i == 0 || i == Number - 1);
                GameObject[] prefabSelected;

                if (isCorner || windowPrefabs == null || windowPrefabs.Length == 0) {
                    prefabSelected = wallPrefabs;
                } else {
                    prefabSelected = windowPrefabs;
                }

                if (prefabSelected.Length == 0)
                    continue;

                int index = RandomInt(prefabSelected.Length);
                SpawnPrefab(prefabSelected[index],
                    direction * (i - (Number - 1) / 2f),
                    Quaternion.identity
                );
            }
        }

        // Type 2: Pick one prefab for every height increase
        void ExecuteType2Row() {
            // Choose between wall or window for this entire row
            bool useWindow = Random.value > 0.5f;
            GameObject[] sourcePool = useWindow ? windowPrefabs : wallPrefabs;
            
            if (sourcePool == null || sourcePool.Length == 0) {
                sourcePool = wallPrefabs; // Fallback to walls
            }
            
            GameObject selectedPrefab = sourcePool[RandomInt(sourcePool.Length)];

            for (int i = 0; i < Number; i++) {
                SpawnPrefab(selectedPrefab,
                    direction * (i - (Number - 1) / 2f),
                    Quaternion.identity
                );
            }
        }

        // Type 3: Every corner a wall, every windowInterval height a wall, windows fill the rest
        void ExecuteType3Row() {
            for (int i = 0; i < Number; i++) {
                bool isCorner = (i == 0 || i == Number - 1);
                GameObject prefabToUse;

                if (isCorner || forceWallFloor) {
                    // Use wall prefab for corners or wall floors
                    prefabToUse = wallPrefabs[RandomInt(wallPrefabs.Length)];
                } else {
                    // Use window prefab for non-corners on non-wall floors
                    if (windowPrefabs != null && windowPrefabs.Length > 0) {
                        prefabToUse = windowPrefabs[RandomInt(windowPrefabs.Length)];
                    } else {
                        prefabToUse = wallPrefabs[RandomInt(wallPrefabs.Length)];
                    }
                }

                SpawnPrefab(prefabToUse,
                    direction * (i - (Number - 1) / 2f),
                    Quaternion.identity
                );
            }
        }
    }

    public class CentralRoof : Shape {
        // Grammar rule probabilities
        const float roofContinueChance = 0.5f;

        // Shape parameters
        int Width;
        int Depth;
        GameObject[] roofStyle;
        GameObject[] wallPrefabs;
        GameObject[] windowPrefabs;
        GameObject cornerPrefab;
        bool forceFlatRoof;

        // Offset values for the next layer
        int newWidth;
        int newDepth;

        public void Initialize(int Width, int Depth, GameObject[] wallPrefabs, GameObject[] windowPrefabs,
                             GameObject[] roofStyle, bool forceFlatRoof, GameObject cornerPrefab) {
            this.Width = Width;
            this.Depth = Depth;
            this.wallPrefabs = wallPrefabs;
            this.windowPrefabs = windowPrefabs;
            this.roofStyle = roofStyle;
            this.forceFlatRoof = forceFlatRoof;
            this.cornerPrefab = cornerPrefab;
        }

        protected override void Execute() {
            if (Width == 0 || Depth == 0)
                return;

            newWidth = Width;
            newDepth = Depth;
            
            if (forceFlatRoof) {
                CreateFlatRoof();
            } else {
                CreateRoofPart();
                CreateNextPart();
            }
        }

        void CreateRoofPart() {
            // Randomly create two roof strips in depth direction or width direction
            int side = RandomInt(2);
            CentralRow roofRow;

            switch (side) {
                // Add two roof strips in depth direction
                case 0:
                    for (int i = 0; i < 2; i++) {
                        roofRow = CreateSymbol<CentralRow>("roofStrip", new Vector3((Width - 1) * (i - 0.5f), 0, 0));
                        roofRow.Initialize(Depth, roofStyle, roofStyle, Vector3.zero, BuildingGenerationType.Type1_CornerWallsOnly);
                        roofRow.Generate();
                    }
                    newWidth -= 2;
                    break;
                // Add two roof strips in width direction  
                case 1:
                    for (int i = 0; i < 2; i++) {
                        roofRow = CreateSymbol<CentralRow>("roofStrip", new Vector3(0, 0, (Depth - 1) * (i - 0.5f)));
                        roofRow.Initialize(Width, roofStyle, roofStyle, new Vector3(1, 0, 0), BuildingGenerationType.Type1_CornerWallsOnly);
                        roofRow.Generate();
                    }
                    newDepth -= 2;
                    break;
            }
        }

        void CreateNextPart() {
            // Randomly continue with a roof or a stock
            if (newWidth <= 0 || newDepth <= 0)
                return;

            float randomValue = RandomFloat();
            if (randomValue < roofContinueChance) {
                // Continue with the roof
                CentralRoof nextRoof = CreateSymbol<CentralRoof>("roof");
                nextRoof.Initialize(newWidth, newDepth, wallPrefabs, windowPrefabs, roofStyle, forceFlatRoof, cornerPrefab);
                nextRoof.Generate(buildDelay);
            } else {
                // Continue with a stock
                CentralStock nextStock = CreateSymbol<CentralStock>("stock");
                nextStock.Initialize(newWidth, newDepth, wallPrefabs, windowPrefabs, roofStyle, cornerPrefab, forceFlatRoof);
                nextStock.Generate(buildDelay);
            }
        }

        void CreateFlatRoof() {
            CentralRow flatRoof;
            if (Width % 2 != 0) {
                for (int i = 0; i < Width; i++) {
                    flatRoof = CreateSymbol<CentralRow>("roofStrip", new Vector3((Width / 2 - i), 0, 0));
                    flatRoof.Initialize(Depth, roofStyle, roofStyle, Vector3.zero, BuildingGenerationType.Type1_CornerWallsOnly);
                    flatRoof.Generate();
                }
                newWidth = 0;
            } else {
                for (int i = 0; i < Width; i++) {
                    flatRoof = CreateSymbol<CentralRow>("roofStrip", new Vector3((Width / 2 - 0.5f - i), 0, 0));
                    flatRoof.Initialize(Depth, roofStyle, roofStyle, Vector3.zero, BuildingGenerationType.Type1_CornerWallsOnly);
                    flatRoof.Generate();
                }
                newWidth = 0;
            }
        }
    }
}