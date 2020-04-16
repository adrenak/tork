using Adrenak.Tork;
using UnityEngine;

namespace Adrenak.Tork.Demo {
	public class AIDemo : MonoBehaviour {
		public Vehicle vehicle;
		public SmoothFollow follow;
		public Transform destination;

		void Start() {
			var ai = vehicle.gameObject.AddComponent<AIDriver>();
			vehicle.SetPlayer(ai);

			ai.destination = destination;
			ai.steerDamping = .5f;

			follow.target = vehicle.transform;
		}
	}
}
