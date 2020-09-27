// TODO: Add support for All WD, Rear WD, Front WD. Right now it is only All WD
using UnityEngine;

namespace Adrenak.Tork {
    public class MotorUnity : MonoBehaviour {
        [SerializeField] float idleRPM;

        [Tooltip("The maximum torque that the motor generates")]
        public float maxTorque = 10000;

        [Tooltip("Multiplier to the maxTorque")]
        public float value;

        public float m_MaxReverseInput = -.5f;

        [SerializeField] float rpmReadonly;
        public float RPM { get { return rpmReadonly; } }

        public AckermannUnity ackermann;

        void FixedUpdate() {
            ApplyMotorTorque();
            CalculateEngineRPM();
        }

        void Update() {
            value = Mathf.Clamp(value, m_MaxReverseInput, 1);
        }

        void ApplyMotorTorque() {
            float fs, fp, rs, rp;

            // If we have Ackerman steering, we apply torque based on the steering radius of each wheel
            var radii = AckermannUtils.GetRadii(ackermann.angle, ackermann.AxleSeparation, ackermann.AxleWidth);
            var total = radii[0] + radii[1] + radii[2] + radii[3];
            fp = radii[0] / total;
            fs = radii[1] / total;
            rp = radii[2] / total;
            rs = radii[3] / total;

            if (ackermann.angle > 0) {
                ackermann.FrontRightWheel.motorTorque = value * maxTorque * fp;
                ackermann.FrontLeftWheel.motorTorque = value * maxTorque * fs;
                ackermann.RearRightWheel.motorTorque = value * maxTorque * rp;
                ackermann.RearLeftWheel.motorTorque = value * maxTorque * rs;
            }
            else {
                ackermann.FrontLeftWheel.motorTorque = value * maxTorque * fp;
                ackermann.FrontRightWheel.motorTorque = value * maxTorque * fs;
                ackermann.RearLeftWheel.motorTorque = value * maxTorque * rp;
                ackermann.RearRightWheel.motorTorque = value * maxTorque * rs;
            }
        }

        void CalculateEngineRPM() {
            var sum = ackermann.FrontLeftWheel.rpm;
            sum += ackermann.FrontRightWheel.rpm;
            sum += ackermann.RearLeftWheel.rpm;
            sum += ackermann.RearRightWheel.rpm;

            rpmReadonly = idleRPM + sum;
        }
    }
}
