using UnityEngine;

namespace Demo {
public class SimpleStock3 : Shape {


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

        public void Initialize(int Width, int Depth, GameObject[] wallPrefabs, GameObject[] windowPrefabs, GameObject[] roofStyle, GameObject cornerPrefab, bool forceFlatRoof, int desiredHeight = 5, int currentHeight = 0){
            this.Width = Width;
            this.Depth = Depth;
            this.wallPrefabs = wallPrefabs;
            this.windowPrefabs = windowPrefabs;
            this.roofStyle = roofStyle;
            this.cornerPrefab = cornerPrefab;
            this.desiredHeight = desiredHeight;
            this.currentHeight = currentHeight;
            this.forceFlatRoof = forceFlatRoof;
            }


        protected override void Execute() {
            bool isWallFloor = currentHeight % 5 == 0;

            for (int i = 0; i < 4; i++) {
                Vector3 localPosition = i switch {
                    0 => new Vector3(-(Width - 1) * 0.5f, 0, 0), // left
                    1 => new Vector3(0, 0, (Depth - 1) * 0.5f),  // back
                    2 => new Vector3((Width - 1) * 0.5f, 0, 0),  // right
                    3 => new Vector3(0, 0, -(Depth - 1) * 0.5f), // front
                    _ => Vector3.zero
                };

                int rowLength = (i % 2 == 1 ? Width : Depth);

                GameObject wallPrefab = wallPrefabs[RandomInt(wallPrefabs.Length)];
                GameObject windowPrefab = isWallFloor ? null : (windowPrefabs.Length > 0 ? windowPrefabs[RandomInt(windowPrefabs.Length)] : null);

                SimpleRow3 row = CreateSymbol<SimpleRow3>("wall", localPosition, Quaternion.Euler(0, i * 90, 0));
                row.Initialize(rowLength, wallPrefab, windowPrefab);
                row.Generate();
}

            // check hieght
            if (currentHeight + 1 < desiredHeight) {
                SimpleStock3 nextStock = CreateSymbol<SimpleStock3>("stock", new Vector3(0, 1, 0));
                nextStock.Initialize(Width, Depth, wallPrefabs, windowPrefabs, roofStyle, cornerPrefab, forceFlatRoof, desiredHeight, currentHeight + 1);
                nextStock.Generate(buildDelay);

                // Add corners
                Corners corners = CreateSymbol<Corners>("corners", Vector3.zero);
                corners.Initialize(Width, Depth, cornerPrefab);
                corners.Generate();

            } else {
                SimpleRoof3 nextRoof = CreateSymbol<SimpleRoof3>("roof", new Vector3(0, 1, 0));
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
