using UnityEngine;

namespace Adrenak.Tork{
	public class KeyboardDriver : VehicleDriver {
		public const string k_SteeringAxisName = "Horizontal";
		public const string k_AccelerateAxisName = "Vertical";
		public const string k_BrakeAxisName = "Jump";

		public override VehicleInput GetInput() {
			p_Input.acceleration = Input.GetAxis(k_AccelerateAxisName);
			p_Input.steering = Input.GetAxis(k_SteeringAxisName);
			p_Input.brake = Input.GetAxis(k_BrakeAxisName);
			return p_Input;
		}
	}
}