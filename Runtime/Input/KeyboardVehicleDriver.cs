using UnityEngine;

namespace Adrenak.Tork{
	public class KeyboardVehicleDriver : IVehicleDriver {
		public const string k_SteeringAxisName = "Horizontal";
		public const string k_AccelerateAxisName = "Vertical";
		public const string k_BrakeAxisName = "Jump";

		public VehicleInput GetInput() {
            return new VehicleInput {
			    acceleration = Input.GetAxis(k_AccelerateAxisName),
			    steering = Input.GetAxis(k_SteeringAxisName),
			    brake = Input.GetAxis(k_BrakeAxisName)
            };
		}
	}
}