using UnityEngine;

namespace Adrenak.Tork {
    public class Brakes : MonoBehaviour {
        public Ackermann ackermann;

        [Tooltip("The maximum braking torque that can be applied")]
        [SerializeField] float maxTorque = 5000;

        [Tooltip("Multiplier to the maxTorque [0..1]")]
        public float value;

        void FixedUpdate() {
            value = Mathf.Clamp01(value);

            float fs, fp, rs, rp;

            // If we have Ackerman steering, we apply torque based on the steering radius of each wheel
            var radii = AckermannUtils.GetRadii(ackermann.Angle, ackermann.AxleSeparation, ackermann.AxleWidth);
            var total = radii[0] + radii[1] + radii[2] + radii[3];
            fp = radii[0] / total;
            fs = radii[1] / total;
            rp = radii[2] / total;
            rs = radii[3] / total;

            if (ackermann.Angle > 0) {
                ackermann.FrontRightWheel.BrakeTorque = value * maxTorque * fp;
                ackermann.FrontLeftWheel.BrakeTorque = value * maxTorque * fs;
                ackermann.RearRightWheel.BrakeTorque = value * maxTorque * rp;
                ackermann.RearLeftWheel.BrakeTorque = value * maxTorque * rs;
            }
            else {
                ackermann.FrontLeftWheel.BrakeTorque = value * maxTorque * fp;
                ackermann.FrontRightWheel.BrakeTorque = value * maxTorque * fs;
                ackermann.RearLeftWheel.BrakeTorque = value * maxTorque * rp;
                ackermann.RearRightWheel.BrakeTorque = value * maxTorque * rs;
            }
        }
    }
}