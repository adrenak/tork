// TODO: Add support for All WD, Rear WD, Front WD. Right now it is only All WD
using UnityEngine;

namespace Adrenak.Tork {
	[RequireComponent(typeof(Ackermann))]
	public class Motor : MonoBehaviour {
		[Tooltip("The maximum torque that the motor generates")]
		public float maxTorque = 20000;

		[Tooltip("Multiplier to the maxTorque")]
		public float value;

		public float m_MaxReverseInput = -.5f;

        public Ackermann ackermann;

		void FixedUpdate() {
			ApplyMotorTorque();
		}

		void ApplyMotorTorque() {
			value = Mathf.Clamp(value, m_MaxReverseInput, 1);

			// If we have Ackerman steering, we apply torque based on the steering radius of each wheel
			var radii = Ackermann.GetRadii(ackermann.Angle, ackermann.AxleSeparation, ackermann.AxleWidth);
			var total = radii[0, 0] + radii[1, 0] + radii[0, 1] + radii[1, 1];
			var fl = radii[0, 0] / total;
            var fr = radii[1, 0] / total;
            var rl = radii[0, 1] / total;
            var rr = radii[1, 1] / total;
			
			ackermann.FrontLeftWheel.MotorTorque = value * maxTorque * fl;
			ackermann.FrontRightWheel.MotorTorque = value * maxTorque * fr;
			ackermann.RearLeftWheel.MotorTorque = value * maxTorque * rl;
			ackermann.RearRightWheel.MotorTorque = value * maxTorque * rr;
		}
	}
}
