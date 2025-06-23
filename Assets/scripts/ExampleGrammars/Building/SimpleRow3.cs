using UnityEngine;

namespace Demo {
	public class SimpleRow3 : Shape {int Number;
		GameObject wallPrefab = null;
		GameObject windowPrefab = null;
		Vector3 direction;

		public void Initialize(int Number, GameObject wallPrefab, GameObject windowPrefab, Vector3 dir = new Vector3()) {
			this.Number = Number;
			this.wallPrefab = wallPrefab;
			this.windowPrefab = windowPrefab;
			direction = (dir.magnitude != 0) ? dir : new Vector3(0, 0, 1);
		}

		protected override void Execute() {
			if (Number <= 0 || wallPrefab == null)
				return;

			bool makeCorners = Number > 2;

			for (int i = 0; i < Number; i++) {
				bool Corner = makeCorners && (i == 0 || i == Number - 1);

				GameObject prefabToUse = (Corner || windowPrefab == null)
					? wallPrefab
					: windowPrefab;

				SpawnPrefab(prefabToUse,
					direction * (i - (Number - 1) / 2f),
					Quaternion.identity
				);
			}
		}




	}
}