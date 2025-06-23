using UnityEngine;

namespace Demo {
    public class NeonSign : MonoBehaviour {

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
            this.neonPrefabs=neonPrefabs;
            this.parent = parent;
            }
        public void GenerateSigns() {
            if (neonPrefabs == null || neonPrefabs.Length == 0)
                return;

            for (int i = 0; i < 4; i++) {
                if (Random.value > 0.5f) continue;
                GameObject prefab = neonPrefabs[Random.Range(0, neonPrefabs.Length)];
                
                Vector3 localPosition = i switch {
                    0 => new Vector3(-(Width - .9f) * 0.5f, 0, 0),
                    1 => new Vector3(0, 0, (Depth - 0.9f) * 0.5f),
                    2 => new Vector3((Width - 0.9f) * 0.5f, 0, 0),
                    3 => new Vector3(0, 0, -(Depth - .9f) * 0.5f),
                    _ => Vector3.zero
                };

                Quaternion rotation = Quaternion.Euler(0, i * 90, 0);
                int maxHorizontal = (i % 2 == 0) ? Depth : Width;

                int signW = UnityEngine.Random.Range(Mathf.Min(Width-2, Depth-2)/2, Mathf.Min(Width-2, Depth-2));
                int signH = UnityEngine.Random.Range(Height/3, Height/2 - 2);

                int startX = Random.Range(1, maxHorizontal - signW);
                int startY = Random.Range(Height / 2, Height - signH + 1);

                for (int y = 0; y < signH; y++) {
                    for (int x = 0; x < signW; x++) {
                        Vector3 localOffset = new Vector3(0, startY + y, x - signW / 2f + 0.5f);
                        Vector3 worldPos = parent.position + localPosition + rotation * localOffset;

                        GameObject tile = Instantiate(prefab, worldPos, rotation, this.transform);

                    }
                }
            }
        }
    }
}