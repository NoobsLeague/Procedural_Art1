using UnityEngine;

namespace Demo {
    public class Corners : Shape {
        [SerializeField] 
        GameObject cornerPrefab;

        int Width;
        int Depth;

        public void Initialize(int Width, int Depth, GameObject cornerPrefab) {
            this.Width = Width;
            this.Depth = Depth;
            this.cornerPrefab = cornerPrefab;
  

        }

        protected override void Execute() {
            if (cornerPrefab == null || Width < 1 || Depth < 1)
                return;

            Vector3[] positions = new Vector3[4];
            positions[0] = new Vector3(-(Width - 1) * 0.5f, 0, -(Depth - 1) * 0.5f); // Front-left
            positions[1] = new Vector3((Width - 1) * 0.5f, 0, -(Depth - 1) * 0.5f);  // Front-right
            positions[2] = new Vector3(-(Width - 1) * 0.5f, 0, (Depth - 1) * 0.5f);  // Back-left
            positions[3] = new Vector3((Width - 1) * 0.5f, 0, (Depth - 1) * 0.5f);   // Back-right

            Quaternion[] rotations = new Quaternion[4];
            rotations[0] = Quaternion.Euler(0, 180, 0);
            rotations[1] = Quaternion.Euler(0, 90, 0);
            rotations[2] = Quaternion.Euler(0, -90, 0);
            rotations[3] = Quaternion.Euler(0, 0, 0);

            for (int i = 0; i < 4; i++) {
                SpawnPrefab(cornerPrefab, positions[i], rotations[i]);
            }
        }
    }
}
