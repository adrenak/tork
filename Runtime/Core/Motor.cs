// TODO: Add support for All WD, Rear WD, Front WD. Right now it is only All WD
using UnityEngine;

namespace Adrenak.Tork {
    public class Motor : MonoBehaviour {
        [SerializeField] float idleRPM;

        [Tooltip("The maximum torque that the motor generates")]
        public float maxTorque = 10000;

        [Tooltip("Multiplier to the maxTorque")]
        public float value;

        public float m_MaxReverseInput = -.5f;

        [SerializeField] float rpmReadonly;
        public float RPM { get { return rpmReadonly; } }

        public Ackermann ackermann;

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
            var radii = AckermannUtils.GetRadii(ackermann.Angle, ackermann.AxleSeparation, ackermann.AxleWidth);
            var total = radii[0] + radii[1] + radii[2] + radii[3];
            fp = radii[0] / total;
            fs = radii[1] / total;
            rp = radii[2] / total;
            rs = radii[3] / total;

            if (ackermann.Angle > 0) {
                ackermann.FrontRightWheel.MotorTorque = value * maxTorque * fp;
                ackermann.FrontLeftWheel.MotorTorque = value * maxTorque * fs;
                ackermann.RearRightWheel.MotorTorque = value * maxTorque * rp;
                ackermann.RearLeftWheel.MotorTorque = value * maxTorque * rs;
            }
            else {
                ackermann.FrontLeftWheel.MotorTorque = value * maxTorque * fp;
                ackermann.FrontRightWheel.MotorTorque = value * maxTorque * fs;
                ackermann.RearLeftWheel.MotorTorque = value * maxTorque * rp;
                ackermann.RearRightWheel.MotorTorque = value * maxTorque * rs;
            }
        }

        void CalculateEngineRPM() {
            var sum = ackermann.FrontLeftWheel.RPM;
            sum += ackermann.FrontRightWheel.RPM;
            sum += ackermann.RearLeftWheel.RPM;
            sum += ackermann.RearRightWheel.RPM;

            rpmReadonly = idleRPM + sum;
        }
    }
}
