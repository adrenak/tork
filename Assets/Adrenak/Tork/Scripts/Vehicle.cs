using UnityEngine;

namespace Adrenak.Tork {
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CenterOfMassAssigner))]
	[RequireComponent(typeof(AntiRoll))]
	[RequireComponent(typeof(Aerodynamics))]
	[RequireComponent(typeof(Motor))]
	[RequireComponent(typeof(Steering))]
	[RequireComponent(typeof(Brakes))]
	public class Vehicle : MonoBehaviour {
		[SerializeField] Rigidbody m_Rigidbody;
		[SerializeField] Steering m_Steering;
		[SerializeField] Motor m_Motor;
		[SerializeField] Brakes m_Brake;
		[SerializeField] Aerodynamics m_Aerodynamics;

		[SerializeField] float m_MaxReverseInput = -.5f;

		[Tooltip("The maximum motor torque available based on the speed (KMPH)")]
		[SerializeField] AnimationCurve m_MotorTorqueVsSpeed = AnimationCurve.Linear(0, 10000, 250, 0);

		[Tooltip("The steering angle based on the speed (KMPH)")]
		[SerializeField] AnimationCurve m_MaxSteeringAngleVsSpeed = AnimationCurve.Linear(0, 35, 250, 5);

		[Tooltip("The down force based on the speed (KMPH)")]
		[SerializeField] AnimationCurve m_DownForceVsSpeed = AnimationCurve.Linear(0, 0, 250, 2500);

		VehicleInput m_Input = VehicleInput.Get(VehicleInputType.Keyboard);

		private void Update() {
			var speed = Vector3.Dot(m_Rigidbody.velocity, transform.forward) * 3.6F;

			m_Steering.range = m_MaxSteeringAngleVsSpeed.Evaluate(speed);
			m_Steering.value = m_Input.GetSteering();

			m_Motor.maxTorque = m_MotorTorqueVsSpeed.Evaluate(speed);
			m_Motor.value = Mathf.Clamp(m_Input.GetAcceleration(), m_MaxReverseInput, 1);

			m_Aerodynamics.downForce = m_DownForceVsSpeed.Evaluate(speed);
			m_Aerodynamics.midAirSteerInput = m_Input.GetSteering();

			m_Brake.value = m_Input.GetBrake();
		}
	}
}
