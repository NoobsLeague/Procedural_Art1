using UnityEngine;

namespace Demo {
	public class SimpleRow2 : Shape {
int Number;
Vector3 direction;
GameObject wallPrefab;
GameObject windowPrefab;

		public void Initialize(int Number, GameObject wallPrefab, GameObject windowPrefab, Vector3 dir = new Vector3()) {
			this.Number = Number;
			this.wallPrefab = wallPrefab;
			this.windowPrefab = windowPrefab;
			this.direction = dir.magnitude != 0 ? dir : new Vector3(0, 0, 1);
			}

		protected override void Execute() {
			if (Number <= 0)
				return;

			for (int i = 0; i < Number; i++) {
				bool placeWindow = Random.value > 0.5f;
				GameObject selectedPrefab = placeWindow ? windowPrefab : wallPrefab;

				SpawnPrefab(
					selectedPrefab,
					direction * (i - (Number - 1) / 2f),
					Quaternion.identity
				);
			}
		}


	}
}