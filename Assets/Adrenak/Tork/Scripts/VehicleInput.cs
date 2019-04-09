using UnityEngine;

namespace Adrenak.Tork {
	// Implement different input types here
	public enum VehicleInputType {
		Keyboard
	}

	public abstract class VehicleInput {
		// Make sure these names match with the input settings of the project
		public const string k_SteeringAxisName = "Horizontal";
		public const string k_AccelerateAxisName = "Vertical";
		public const string k_BrakeAxisName = "Jump";

		protected float steering;
		protected float acceleration;
		protected float brake;
		public abstract float GetSteering();
		public abstract float GetAcceleration();
		public abstract float GetBrake();

		public static VehicleInput Get(VehicleInputType type) {
			switch (type) {
				case VehicleInputType.Keyboard:
					return new KeyboardInput();
				default:
					return new InvalidInput();
			}
		}
	}

	public class KeyboardInput : VehicleInput {
		public override float GetBrake() {
			Update();
			return brake;
		}

		public override float GetAcceleration() {
			Update();
			return acceleration;
		}

		public override float GetSteering() {
			Update();
			return steering;
		}

		void Update() {
			acceleration = Input.GetAxis(k_AccelerateAxisName);
			steering = Input.GetAxis(k_SteeringAxisName);
			brake = Input.GetAxis(k_BrakeAxisName);
		}
	}

	public class InvalidInput : VehicleInput {
		public override float GetAcceleration() {
			throw new System.NotImplementedException();
		}

		public override float GetBrake() {
			throw new System.NotImplementedException();
		}

		public override float GetSteering() {
			throw new System.NotImplementedException();
		}
	}
}