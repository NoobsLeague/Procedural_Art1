using UnityEngine;

namespace Demo {
	public class SimpleRow : Shape {
		int Number;
		GameObject[] wallPrefabs = null;
		GameObject[] windowPrefabs = null;


		Vector3 direction;

		public void Initialize(int Number, GameObject[] wallPrefabs, GameObject[] windowPrefabs, Vector3 dir = new Vector3()) {
			this.Number = Number;
			this.wallPrefabs = wallPrefabs;
			this.windowPrefabs = windowPrefabs;	
			if (dir.magnitude!=0) {
				direction=dir;
			} else {
				direction=new Vector3(0, 0, 1);
			}
			}


		protected override void Execute() {
			if (Number <= 0)
				return;

			bool corner = Number > 2;

			for (int i = 0; i < Number; i++) {
				bool isCorner = corner && (i == 0 || i == Number - 1);
				GameObject[] prefabselected;

				if (isCorner || windowPrefabs == null || windowPrefabs.Length == 0) {
					prefabselected = wallPrefabs;
				} else {
					prefabselected = windowPrefabs;
				}

				if (prefabselected.Length == 0)
					continue;

				int index = RandomInt(prefabselected.Length);
				SpawnPrefab(prefabselected[index],
					direction * (i - (Number - 1) / 2f),
					Quaternion.identity
				);
			}
		}
	}
}
