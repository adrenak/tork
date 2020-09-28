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
            var radii = AckermannUtils.GetRadii(ackermann.angle, ackermann.AxleSeparation, ackermann.AxleWidth);
            var total = radii[0] + radii[1] + radii[2] + radii[3];
            fp = radii[0] / total;
            fs = radii[1] / total;
            rp = radii[2] / total;
            rs = radii[3] / total;

            if (ackermann.angle > 0) {
                ackermann.FrontRightWheel.brakeTorque = value * maxTorque * fp;
                ackermann.FrontLeftWheel.brakeTorque = value * maxTorque * fs;
                ackermann.RearRightWheel.brakeTorque = value * maxTorque * rp;
                ackermann.RearLeftWheel.brakeTorque = value * maxTorque * rs;
            }
            else {
                ackermann.FrontLeftWheel.brakeTorque = value * maxTorque * fp;
                ackermann.FrontRightWheel.brakeTorque = value * maxTorque * fs;
                ackermann.RearLeftWheel.brakeTorque = value * maxTorque * rp;
                ackermann.RearRightWheel.brakeTorque = value * maxTorque * rs;
            }
        }
    }
}