using Unity.Entities;
using UnityEngine;

namespace Pragnesh.Dots
{
	public struct VehicleInputData:IComponentData {
		public float steering;
		public float acceleration;
		public float brake;
	}

	public class KeyboardInputSystem : ComponentSystem
	{
		 const string k_SteeringAxisName = "Horizontal";
		 const string k_AccelerateAxisName = "Vertical";
		 const string k_BrakeAxisName = "Jump";

		protected override void OnUpdate()
		{
			Entities.ForEach((ref VehicleInputData vehicleInput) => {

				vehicleInput.acceleration = Input.GetAxis(k_AccelerateAxisName);
				vehicleInput.steering = Input.GetAxis(k_SteeringAxisName);
				vehicleInput.brake = Input.GetAxis(k_BrakeAxisName);
			});
		}
	}
}