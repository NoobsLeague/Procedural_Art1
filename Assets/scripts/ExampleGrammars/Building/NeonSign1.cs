using UnityEngine;

namespace Demo {
    public class NeonSign1 : MonoBehaviour {

        [SerializeField]
        int Width;
        [SerializeField]
        int Depth;
        [SerializeField]
        GameObject[] neonPrefabs;
        [SerializeField]
        int Height;

        [SerializeField]
        Transform parent;

        public void Initialize(Transform parent, int Width, int Depth, GameObject[] neonPrefabs, int Height){
            this.Width = Width;
            this.Depth = Depth;
            this.Height = Height;
            this.neonPrefabs = neonPrefabs;
            this.parent = parent;
        }

        public void GenerateSigns() {
            if (neonPrefabs == null || neonPrefabs.Length == 0)
                return;

            for (int i = 0; i < 4; i++) {
                if (Random.value > 0.5f) continue;
                
                GameObject prefab = neonPrefabs[Random.Range(0, neonPrefabs.Length)];

                // Match the wall positions from SimpleStock
                Vector3 wallBasePosition = i switch {
                    0 => new Vector3(-(Width - 1) * 0.5f, 0, 0), // left
                    1 => new Vector3(0, 0, (Depth - 1) * 0.5f), // back
                    2 => new Vector3((Width - 1) * 0.5f, 0, 0), // right
                    3 => new Vector3(0, 0, -(Depth - 1) * 0.5f), // front
                    _ => Vector3.zero
                };

                // Match the rotation from SimpleStock (walls rotate by i * 90)
                Quaternion wallRotation = Quaternion.Euler(0, i * 90, 0);
                
                // Determine the wall length for this side
                int wallLength = (i % 2 == 0) ? Depth : Width;

                // Sign dimensions
                int signW = UnityEngine.Random.Range(2, Mathf.Min(wallLength - 2, 5));
                int signH = UnityEngine.Random.Range(1, Mathf.Min(Height / 3, 3));

                // Calculate starting position along the wall
                int maxStart = wallLength - signW - 1;
                int startAlongWall = Random.Range(1, Mathf.Max(2, maxStart));
                int startY = Random.Range(1, Mathf.Max(2, Height - signH));

                for (int y = 0; y < signH; y++) {
                    for (int x = 0; x < signW; x++) {
                        // Position along the wall
                        float positionAlongWall = startAlongWall + x - (wallLength * 0.5f) + 0.5f;
                        
                        // Calculate the outward offset based on building dimensions
                        // The walls are positioned at (Width-1)/2 and (Depth-1)/2
                        // So we need to push out by an additional 0.5 + small offset
                        float outwardOffset = 0.9f; // This accounts for wall thickness
                        float outwardOffsetBack = 1.1f;
                        
                        // Create position directly based on which wall
                        Vector3 signPosition = wallBasePosition;
                        
                        switch (i) {
                            case 0: // Left wall - signs go along Z axis, push left
                                signPosition += new Vector3(outwardOffset, startY + y, positionAlongWall);
                                break;
                            case 1: // Back wall - signs go along X axis, push back
                                signPosition += new Vector3(positionAlongWall, startY + y, outwardOffsetBack);
                                break;
                            case 2: // Right wall - signs go along Z axis, push right
                                signPosition += new Vector3(-outwardOffset, startY + y, positionAlongWall);
                                break;
                            case 3: // Front wall - signs go along X axis, push front
                                signPosition += new Vector3(positionAlongWall, startY + y, -outwardOffsetBack);
                                break;
                        }
                        
                        // Final world position
                        Vector3 worldPos = parent.position + signPosition;
                        
                        // Signs should face the same direction as the wall
                        GameObject tile = Instantiate(prefab, worldPos, wallRotation, this.transform);
                    }
                }
            }
        }
    }
}