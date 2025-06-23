using UnityEngine;

namespace Demo {
public class SimpleStock2 : Shape {


	// shape parameters:
[SerializeField]
int Width;
[SerializeField]
int Depth;

[SerializeField]
GameObject[] wallPrefabs;
[SerializeField]
GameObject[] windowPrefabs;

[SerializeField]
GameObject[] roofStyle;

[SerializeField]
public int desiredHeight;

[SerializeField]
public bool forceFlatRoof;

[SerializeField] 
GameObject cornerPrefab;

int currentHeight = 0;

        public void Initialize(int Width, int Depth, GameObject[] wallPrefabs, GameObject[] windowPrefabs, GameObject[] roofStyle, GameObject cornerPrefab, int desiredHeight = 5, int currentHeight = 0){
            this.Width = Width;
            this.Depth = Depth;
            this.wallPrefabs = wallPrefabs;
            this.windowPrefabs = windowPrefabs;
            this.roofStyle = roofStyle;
            this.cornerPrefab = cornerPrefab;
            this.desiredHeight = desiredHeight;
            this.currentHeight = currentHeight;
            }


        protected override void Execute() {

            bool useWindow = Random.value > 0.5f;
            GameObject[] sourcePool = useWindow ? windowPrefabs : wallPrefabs;
            GameObject selectedPrefab = sourcePool[RandomInt(sourcePool.Length)];


            for (int i = 0; i < 4; i++) {
                Vector3 localPosition = i switch {
                    0 => new Vector3(-(Width - 1) * 0.5f, 0, 0), // left
                    1 => new Vector3(0, 0, (Depth - 1) * 0.5f),  // back
                    2 => new Vector3((Width - 1) * 0.5f, 0, 0),  // right
                    3 => new Vector3(0, 0, -(Depth - 1) * 0.5f), // front
                    _ => Vector3.zero
                };

                SimpleRow2 newRow = CreateSymbol<SimpleRow2>("wall", localPosition, Quaternion.Euler(0, i * 90, 0));
                newRow.Initialize(i % 2 == 1 ? Width : Depth, selectedPrefab, selectedPrefab);
                newRow.Generate();

            }

            if (currentHeight + 1 < desiredHeight) {
                SimpleStock2 nextStock = CreateSymbol<SimpleStock2>("stock", new Vector3(0, 1, 0));
                nextStock.Initialize(Width, Depth, wallPrefabs, windowPrefabs, roofStyle, cornerPrefab, desiredHeight, currentHeight + 1);
                nextStock.Generate(buildDelay);

                // Add corners
                Corners corners = CreateSymbol<Corners>("corners", Vector3.zero);
                corners.Initialize(Width, Depth, cornerPrefab); 
                corners.Generate();
            } else {
                SimpleRoof2 nextRoof = CreateSymbol<SimpleRoof2>("roof", new Vector3(0, 1, 0));
                nextRoof.Initialize(Width, Depth, wallPrefabs, windowPrefabs, roofStyle, forceFlatRoof, cornerPrefab);
                nextRoof.Generate(buildDelay);
                 // Add corners
                Corners corners = CreateSymbol<Corners>("corners", Vector3.zero);
                corners.Initialize(Width, Depth, cornerPrefab); 
                corners.Generate();
            }
        }

    
	}
}
