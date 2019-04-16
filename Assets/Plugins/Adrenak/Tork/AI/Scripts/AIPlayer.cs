using UnityEngine;

namespace Adrenak.Tork {
	[RequireComponent(typeof(Steering))]
	public class AIPlayer : Player {
		public Transform destination;
		Steering m_Steering;

		bool m_IsInTurningCircle;

		void Start() {
			m_Steering = GetComponent<Steering>();
		}

		public override VehicleInput GetInput() {
			var towards = destination.position - transform.position;
			var locTowards = transform.InverseTransformDirection(towards);

			var angle = Vector3.Angle(transform.forward, towards) 
				* Mathf.Sign(locTowards.x);

			var radii = Ackermann.GetRadii(
				m_Steering.range,
				m_Steering.Ackermann.AxleSeparation,
				m_Steering.Ackermann.AxleWidth
			);
			var avgRadius = (radii[0, 0] + radii[0, 1] + radii[1, 0] + radii[1, 1]) / 4;
			avgRadius = Mathf.Abs(avgRadius);
			var pivot = m_Steering.Ackermann.GetPivot();
			bool isInCircle = (destination.position - pivot).magnitude < avgRadius;

			if (isInCircle) {
				p_Input.acceleration = Mathf.Abs(Mathf.Cos(angle * Mathf.Deg2Rad));
				p_Input.brake = Mathf.Sin(angle * Mathf.Deg2Rad);
			}
			else {
				p_Input.acceleration = 1;
				p_Input.brake = 0;
			}

			p_Input.steering = Mathf.Clamp(angle / m_Steering.range, -1, 1);

			return p_Input;
		}
	}
}
