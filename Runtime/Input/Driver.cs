using UnityEngine;

namespace Adrenak.Tork {
	public abstract class VehicleDriver : MonoBehaviour {
		protected VehicleInput p_Input = new VehicleInput();
		public abstract VehicleInput GetInput();
	}
}
