using UnityEngine;

namespace Adrenak.Tork{
    public class MotorTorqueVsSpeedConstraint : MonoBehaviour {
        [Tooltip("The maximum motor torque available based on the speed (KMPH)")]
        [SerializeField] AnimationCurve curve = AnimationCurve.Linear(0, 10000, 250, 0);

        [SerializeField] Motor motor;
        [SerializeField] new Rigidbody rigidbody;

        void Update(){
            motor.maxTorque = GetMotorTorqueAtSpeed(rigidbody.velocity.magnitude);
        }

        public float GetMotorTorqueAtSpeed(float speed) {
            return curve.Evaluate(speed);
        }
    }
}
