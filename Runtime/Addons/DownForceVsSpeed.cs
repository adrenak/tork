using UnityEngine;

namespace Adrenak.Tork { 
    public class DownForceVsSpeed : VehicleAddOn {
        [Tooltip("The down force based on the speed (KMPH)")]
        [SerializeField] AnimationCurve curve = AnimationCurve.Linear(0, 0, 250, 2500);

        [SerializeField] new Rigidbody rigidbody;

        void Update() {
            var downForce = GetDownForceAtSpeed(rigidbody.velocity.magnitude * 3.6f);
            rigidbody.AddForce(-Vector3.up * downForce);
        }

        public float GetDownForceAtSpeed(float speed) {
            return curve.Evaluate(speed);
        }
    }
}

