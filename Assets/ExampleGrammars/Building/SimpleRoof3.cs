using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

namespace Demo {
	public class SimpleRoof3 : Shape {
		// grammar rule probabilities:
		const float roofContinueChance = 0.5f;

		// shape parameters:
		int Width;
		int Depth;

		GameObject[] roofStyle;
		GameObject[] wallPrefabs;
		GameObject[] windowPrefabs;
		GameObject cornerPrefab;

		// (offset) values for the next layer:
		int newWidth;
		int newDepth;

		[SerializeField]
		public bool forceFlatRoof;



		public void Initialize(int Width, int Depth, GameObject[] wallPrefabs, GameObject[] windowPrefabs,GameObject[] roofStyle, bool forceFlatRoof, GameObject cornerPrefab) {
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

	// Pick a consistent roof prefab to use for this entire roof
	GameObject selectedRoofPrefab = roofStyle[RandomInt(roofStyle.Length)];

	newWidth = Width;
	newDepth = Depth;

	if (forceFlatRoof) {
		createFlat(selectedRoofPrefab);
	} else {
		CreateFlatRoofPart(selectedRoofPrefab);
		CreateNextPart();
	}
}


void CreateFlatRoofPart(GameObject roofPrefab) {
	int side = RandomInt(2);
	SimpleRow3 flatRoof;

	switch (side) {
		case 0: // depth direction
			for (int i = 0; i < 2; i++) {
				flatRoof = CreateSymbol<SimpleRow3>("roofStrip", new Vector3((Width - 1) * (i - 0.5f), 0, 0));
				flatRoof.Initialize(Depth, roofPrefab, roofPrefab); // both "wall" and "window" use the same prefab
				flatRoof.Generate();
			}
			newWidth -= 2;
			break;
		case 1: // width direction
			for (int i = 0; i < 2; i++) {
				flatRoof = CreateSymbol<SimpleRow3>("roofStrip", new Vector3(0, 0, (Depth - 1) * (i - 0.5f)));
				flatRoof.Initialize(Width, roofPrefab, roofPrefab, new Vector3(1, 0, 0));
				flatRoof.Generate();
			}
			newDepth -= 2;
			break;
	}
}

void CreateNextPart() {
			// randomly continue with a roof or a stock:
	if (newWidth<=0 || newDepth<=0)
		return;

	float randomValue = RandomFloat();
		if (randomValue<roofContinueChance) { // continue with the roof
				SimpleRoof2 nextRoof = CreateSymbol<SimpleRoof2>("roof");
				nextRoof.Initialize(newWidth, newDepth, wallPrefabs, windowPrefabs, roofStyle, forceFlatRoof, cornerPrefab);
				nextRoof.Generate(buildDelay);
			} else { // continue with a stock
				SimpleStock2 nextStock = CreateSymbol<SimpleStock2>("stock");
				nextStock.Initialize(newWidth, newDepth, wallPrefabs, windowPrefabs, roofStyle, cornerPrefab, 0, 0);
				nextStock.Generate(buildDelay);
	}
}

void createFlat(GameObject roofPrefab) {
	SimpleRow2 flatRoof;

	if (Width % 2 != 0) {
		for (int i = 0; i < Width + 1; i++) {
			flatRoof = CreateSymbol<SimpleRow2>("roofStrip", new Vector3((Width / 2 - i + 0.5f), 0, 0));
			flatRoof.Initialize(Depth+1, roofPrefab, roofPrefab);
			flatRoof.Generate();
		}
	} else {
		for (int i = 0; i < Width + 1; i++) {
			flatRoof = CreateSymbol<SimpleRow2>("roofStrip", new Vector3((Width / 2 - i), 0, 0));
			flatRoof.Initialize(Depth+1, roofPrefab, roofPrefab);
			flatRoof.Generate();
		}
	}
			newWidth = 0;
}

	
	}
}