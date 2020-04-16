using UnityEngine;

namespace Adrenak.Tork {
	[RequireComponent(typeof(Ackermann))]
	public class Brakes : MonoBehaviour {
		[Tooltip("The maximum braking torque that can be applied")]
		[SerializeField] float maxTorque = 5000;

		[Tooltip("Multiplier to the maxTorque")]
		public float value;

		public Wheel m_FrontRight;
		public Wheel m_FrontLeft;
		public Wheel m_RearRight;
		public Wheel m_RearLeft;

		public Ackermann m_Ackermann { get; private set; }

		private void Awake() {
			m_Ackermann = GetComponent<Ackermann>();
		}

		private void FixedUpdate() {
			float fr, fl, rr, rl;

			// If we have Ackerman steering, we apply torque based on the steering radius of each wheel
			if (m_Ackermann != null) {
				var radii = Ackermann.GetRadii(m_Ackermann.Angle, m_Ackermann.AxleSeparation, m_Ackermann.AxleWidth);
				var total = radii[0, 0] + radii[1, 0] + radii[0, 1] + radii[1, 1];
				fl = radii[0, 0] / total;
				fr = radii[1, 0] / total;
				rl = radii[0, 1] / total;
				rr = radii[1, 1] / total;
			}
			else
				fr = fl = rr = rl = 1;

			m_FrontLeft.BrakeTorque = value * maxTorque * fl;
			m_FrontRight.BrakeTorque = value * maxTorque * fr;

			m_RearLeft.BrakeTorque = value * maxTorque * rl;
			m_RearRight.BrakeTorque = value * maxTorque * rr;
		}
	}
}