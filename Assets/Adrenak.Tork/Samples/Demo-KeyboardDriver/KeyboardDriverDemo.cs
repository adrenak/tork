using Adrenak.Tork;
using UnityEngine;

namespace Adrenak.Tork.Demo {
	public class KeyboardDriverDemo : MonoBehaviour {
		public Vehicle vehicle;
		public SmoothFollow smoothFollow;

		void Start () {
			var player = vehicle.gameObject.AddComponent<KeyboardDriver>();
			vehicle.SetPlayer(player);

			smoothFollow.target = vehicle.transform;
		}
	}
}
