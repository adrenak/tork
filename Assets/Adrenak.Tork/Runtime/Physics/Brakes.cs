using UnityEngine;

namespace Adrenak.Tork {
	public class Brakes : MonoBehaviour {
        public Ackermann ackermann;

		[Tooltip("The maximum braking torque that can be applied")]
		[SerializeField] float maxTorque = 5000;

		[Tooltip("Multiplier to the maxTorque [0..1]")]
		public float value;

		void FixedUpdate() {
			float fr, fl, rr, rl;

			// If we have Ackerman steering, we apply torque based on the steering radius of each wheel
			if (ackermann != null) {
				var radii = Ackermann.GetRadii(ackermann.Angle, ackermann.AxleSeparation, ackermann.AxleWidth);
				var total = radii[0, 0] + radii[1, 0] + radii[0, 1] + radii[1, 1];
				fl = radii[0, 0] / total;
				fr = radii[1, 0] / total;
				rl = radii[0, 1] / total;
				rr = radii[1, 1] / total;
			}
			else
				fr = fl = rr = rl = 1;

			ackermann.FrontLeftWheel.BrakeTorque = value * maxTorque * fl;
			ackermann.FrontRightWheel.BrakeTorque = value * maxTorque * fr;
			ackermann.RearLeftWheel.BrakeTorque = value * maxTorque * rl;
			ackermann.RearRightWheel.BrakeTorque = value * maxTorque * rr;
		}
	}
}