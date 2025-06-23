using UnityEngine;

namespace Demo {
public class SimpleStock : Shape {


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
int currentHeight = 0;

[SerializeField] 
GameObject cornerPrefab;
[SerializeField] 
GameObject[] neonPrefabs;

public void Initialize(int Width, int Depth, GameObject[] wallPrefabs, GameObject[] windowPrefabs, GameObject[] roofStyle, GameObject cornerPrefab, bool forceFlatRoof, int desiredHeight = 5, int currentHeight = 0) {
    this.Width = Width;
    this.Depth = Depth;
    this.wallPrefabs = wallPrefabs;
    this.windowPrefabs = windowPrefabs;
    this.roofStyle = roofStyle;
    this.desiredHeight = desiredHeight;
    this.currentHeight = currentHeight;
    this.cornerPrefab = cornerPrefab;
    this.forceFlatRoof = forceFlatRoof;
}


protected override void Execute() {
    // Create four walls:
    for (int i = 0; i < 4; i++) {
        Vector3 localPosition = new Vector3();
        switch (i) {
            case 0: localPosition = new Vector3(-(Width - 1) * 0.5f, 0, 0); break; // left
            case 1: localPosition = new Vector3(0, 0, (Depth - 1) * 0.5f); break; // back
            case 2: localPosition = new Vector3((Width - 1) * 0.5f, 0, 0); break; // right
            case 3: localPosition = new Vector3(0, 0, -(Depth - 1) * 0.5f); break; // front
        }
        SimpleRow newRow = CreateSymbol<SimpleRow>("wall", localPosition, Quaternion.Euler(0, i * 90, 0));
        newRow.Initialize(i % 2 == 1 ? Width : Depth, wallPrefabs, windowPrefabs);
        newRow.Generate();

    }

    // Check height and build another stock
    if (currentHeight + 1 < desiredHeight) {
            SimpleStock nextStock = CreateSymbol<SimpleStock>("stock", new Vector3(0, 1, 0));
            nextStock.Initialize(Width, Depth, wallPrefabs, windowPrefabs, roofStyle, cornerPrefab, forceFlatRoof, desiredHeight, currentHeight + 1);
            nextStock.Generate(buildDelay);

                //corner
            Corners corners = CreateSymbol<Corners>("corners", Vector3.zero);
            corners.Initialize(Width, Depth, cornerPrefab); 
            corners.Generate();
    }
    
    else {
       
            SimpleRoof nextRoof = CreateSymbol<SimpleRoof>("roof", new Vector3(0, 1, 0));
            nextRoof.Initialize(Width, Depth, wallPrefabs, windowPrefabs, roofStyle, forceFlatRoof, cornerPrefab);
            nextRoof.Generate(buildDelay);
                //corner
            Corners corners = CreateSymbol<Corners>("corners", Vector3.zero);
            corners.Initialize(Width, Depth, cornerPrefab); 
            corners.Generate();
        
        }

            //neon signs
        if (neonPrefabs != null && neonPrefabs.Length > 0 && currentHeight == 0) {
                GameObject neon = new GameObject("NeonSignGenerator");
                neon.transform.SetParent(this.transform);

                NeonSign1 signGenerator = neon.AddComponent<NeonSign1>();
                signGenerator.Initialize(this.transform, Depth, Width, neonPrefabs, desiredHeight);
                signGenerator.GenerateSigns();
        }
    
		}
	}
}