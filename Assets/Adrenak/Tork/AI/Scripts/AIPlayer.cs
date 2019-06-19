using UnityEngine;

namespace Adrenak.Tork {
	[RequireComponent(typeof(Steering))]
	public class AIPlayer : Player {
		enum Direction {
			Forward,
			Reverse
		}

		public Transform destination;
		Direction m_Direction = Direction.Forward;
		Vehicle m_Vehicle;
		Steering m_Steering;

		bool m_IsInTurningCircle;

		void Start() {
			m_Vehicle = GetComponent<Vehicle>();
			m_Steering = GetComponent<Steering>();
		}

		public override VehicleInput GetInput() {
			var towards = destination.position - transform.position;
			var locTowards = transform.InverseTransformDirection(towards);
			var angle = Vector3.Angle(transform.forward, towards) * Mathf.Sign(locTowards.x);
			bool isTargetOnRight = Mathf.Sign(locTowards.x) > 0;
			bool isBehind = Vector3.Dot(towards, transform.forward) < 0;

			// Get radii at the maximum steering angle
			// This gives is the smallest turning radius the car can have
			var radii = Ackermann.GetRadii(
				m_Vehicle.GetMaxSteerAtSpeed(m_Vehicle.Velocity.magnitude),
				m_Steering.Ackermann.AxleSeparation,
				m_Steering.Ackermann.AxleWidth
			);
			var avgRadius = (radii[0, 0] + radii[0, 1] + radii[1, 0] + radii[1, 1]) / 4;
			avgRadius = Mathf.Abs(avgRadius);

			// Find out if the target is inside the turning circle
			var pivot = m_Steering.Ackermann.GetPivot();
			var localPivot = transform.InverseTransformPoint(pivot);
			var isPivotOnRight = Mathf.Sign(localPivot.x) > 0;
			// If the target and pivot are on opposite sides of the car
			// we move the pivot along the local X axis so we can do a valid comparision
			if (isPivotOnRight != isTargetOnRight)
				localPivot.x *= -1;
			pivot = transform.TransformPoint(localPivot);
			var isInCircle = (destination.position - pivot).magnitude < avgRadius;

			switch (m_Direction) {
				case Direction.Forward:
					Debug.DrawLine(transform.position, pivot, Color.green);
					p_Input.acceleration = 1;
					p_Input.brake = 0;
					p_Input.steering = Mathf.Clamp(angle / m_Steering.range, -1, 1);
					if (isBehind && isInCircle)
						m_Direction = Direction.Reverse;
					break;
				case Direction.Reverse:
					Debug.DrawLine(transform.position, pivot, Color.red);
					p_Input.acceleration = -1;
					p_Input.brake = 0;
					p_Input.steering = Mathf.Clamp(angle / m_Steering.range, -1, 1) * (isTargetOnRight ? -1 : 1);
					if (!isBehind && !isInCircle)
						m_Direction = Direction.Forward;
					break;
			}

			return p_Input;
		}
	}
}
