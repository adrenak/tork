using Adrenak.Tork;
using UnityEngine;

namespace Adrenak.Tork.Demo {
	public class PhysicsDemo : MonoBehaviour {
		public Vehicle vehicle;
		public SmoothFollow smoothFollow;

		void Start () {
			var player = vehicle.gameObject.AddComponent<KeyboardPlayer>();
			vehicle.SetPlayer(player);

			smoothFollow.target = vehicle.transform;
		}
	}
}
