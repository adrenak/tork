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
		public Vector3 Velocity { get { return m_Rigidbody.velocity; } }

		[Tooltip("The maximum motor torque available based on the speed (KMPH)")]
		[SerializeField] AnimationCurve m_MotorTorqueVsSpeed = AnimationCurve.Linear(0, 10000, 250, 0);

		[Tooltip("The steering angle based on the speed (KMPH)")]
		[SerializeField] AnimationCurve m_MaxSteeringAngleVsSpeed = AnimationCurve.Linear(0, 35, 250, 5);

		[Tooltip("The down force based on the speed (KMPH)")]
		[SerializeField] AnimationCurve m_DownForceVsSpeed = AnimationCurve.Linear(0, 0, 250, 2500);

		[SerializeField] Driver m_Player;

		Rigidbody m_Rigidbody;
		Steering m_Steering;
		Motor m_Motor;
		Brakes m_Brake;
		Aerodynamics m_Aerodynamics;

		void Start() {
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Steering = GetComponent<Steering>();
			m_Motor = GetComponent<Motor>();
			m_Aerodynamics = GetComponent<Aerodynamics>();
			m_Brake = GetComponent<Brakes>();
		}

		public void SetPlayer(Driver player) {
			m_Player = player;
		}

		void Update() {
			if (m_Player == null) return;

			var speed = m_Rigidbody.velocity.magnitude * 3.6F;

			m_Steering.range = GetMaxSteerAtSpeed(speed);
			m_Motor.maxTorque = GetMotorTorqueAtSpeed(speed);
			m_Aerodynamics.downForce = GetDownForceAtSpeed(speed);

			var input = m_Player.GetInput();

			m_Steering.value = input.steering;
			m_Motor.value = input.acceleration;
			m_Brake.value = input.brake;
			m_Aerodynamics.midAirSteerInput = input.steering;
		}

        public float GetDownForceAtSpeed(float speed) {
            return m_DownForceVsSpeed.Evaluate(speed);
        }

        public float GetMotorTorqueAtSpeed(float speed) {
			return m_MotorTorqueVsSpeed.Evaluate(speed);
		}

		public float GetMaxSteerAtSpeed(float speed) {
			return m_MaxSteeringAngleVsSpeed.Evaluate(speed);
		}
	}
}
