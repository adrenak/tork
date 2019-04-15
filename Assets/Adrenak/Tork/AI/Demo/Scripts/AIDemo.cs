using Adrenak.Tork;
using UnityEngine;

namespace Adrenak.Tork.Demo {
	public class AIDemo : MonoBehaviour {
		public Vehicle vehicle;
		public SmoothFollow follow;
		public Transform destination;

		void Start() {
			var player = vehicle.gameObject.AddComponent<AIPlayer>();
			vehicle.SetPlayer(player);

			player.destination = destination;

			follow.target = vehicle.transform;
		}
	}
}
