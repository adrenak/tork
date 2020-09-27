using UnityEngine;

namespace Adrenak.Tork {
    public class MaxSteerAngleVsSpeed : VehicleAddOn {
        [Tooltip("The steering angle based on the speed (KMPH)")]
        [SerializeField] AnimationCurve curve = AnimationCurve.Linear(0, 35, 250, 5);

        [SerializeField] Steering steering;
        [SerializeField] new Rigidbody rigidbody;

        void Update() {
            steering.range = GetMaxSteerAtSpeed(rigidbody.velocity.magnitude * 3.6f);
        }

        public float GetMaxSteerAtSpeed(float speed) {
            return curve.Evaluate(speed);
        }
    }
}
