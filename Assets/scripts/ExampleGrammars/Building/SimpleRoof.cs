using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

namespace Demo {
	public class SimpleRoof : Shape {
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

		newWidth = Width;
		newDepth = Depth;
		if (forceFlatRoof) {
			createFlat();
		}else{
		CreateFlatRoofPart();
		CreateNextPart();
		}
}

		void CreateFlatRoofPart() {
			// Randomly create two roof strips in depth direction or in width direction:
			int side = RandomInt(2);
			SimpleRow flatRoof;

			switch (side) {
				// Add two roof strips in depth direction
				case 0:
					for (int i = 0; i<2; i++) {
						flatRoof = CreateSymbol<SimpleRow>("roofStrip",new Vector3((Width-1)*(i-0.5f), 0, 0));
						flatRoof.Initialize(Depth, roofStyle, roofStyle);
						flatRoof.Generate();
					}
					newWidth-=2;
					break;
				// Add two roof strips in width direction
				case 1:
					for (int i = 0; i<2; i++) {
						flatRoof = CreateSymbol<SimpleRow>("roofStrip",new Vector3(0,0,(Depth-1)*(i-0.5f)));
						flatRoof.Initialize(Width, roofStyle, roofStyle,new Vector3(1,0,0));
						flatRoof.Generate();
					}
					newDepth-=2;
					break;
			}
		}

void CreateNextPart() {
			// randomly continue with a roof or a stock:
	if (newWidth<=0 || newDepth<=0)
		return;

	float randomValue = RandomFloat();
		if (randomValue<roofContinueChance) { // continue with the roof
				SimpleRoof nextRoof = CreateSymbol<SimpleRoof>("roof");
				nextRoof.Initialize(newWidth, newDepth, wallPrefabs, windowPrefabs, roofStyle, forceFlatRoof, cornerPrefab);
				nextRoof.Generate(buildDelay);
			} else { // continue with a stock
				SimpleStock nextStock = CreateSymbol<SimpleStock>("stock");
				nextStock.Initialize(newWidth, newDepth, wallPrefabs, windowPrefabs, roofStyle, cornerPrefab, forceFlatRoof);
				nextStock.Generate(buildDelay);
	}
}

void createFlat(){
	SimpleRow flatRoof;
	if(Width%2 != 0){
	for (int i = 0;i<Width ;i++){
		flatRoof = CreateSymbol<SimpleRow>("roofStrip",new Vector3((Width/2-i), 0, 0));
		flatRoof.Initialize(Depth, roofStyle, roofStyle);
		flatRoof.Generate();
		}
		newWidth=0;
	}else{
	for (int i = 0;i<Width ;i++){
		flatRoof = CreateSymbol<SimpleRow>("roofStrip",new Vector3((Width/2-0.5f-i), 0, 0));
		flatRoof.Initialize(Depth, roofStyle, roofStyle);
		flatRoof.Generate();
		}
		newWidth=0;
	}
}

	
	}
}