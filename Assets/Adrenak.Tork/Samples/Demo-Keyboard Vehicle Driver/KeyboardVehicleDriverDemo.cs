using UnityEngine;

namespace Adrenak.Tork.Demo {
	public class KeyboardVehicleDriverDemo : MonoBehaviour {
		public Vehicle vehiclePrefab;
        public Transform spawnPoint;
		public SmoothFollow smoothFollow;

		void Start () {
            var vehicle = Instantiate(vehiclePrefab, spawnPoint.position, spawnPoint.rotation);

            vehicle.Driver = new KeyboardVehicleDriver();   // You can comment this out and it'll default to the same driver
			smoothFollow.target = vehicle.transform;
		}
	}
}
