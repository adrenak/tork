using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adrenak.Tork {
    public class MaxSteerAngleVsSpeedConstraint : MonoBehaviour {
        [Tooltip("The steering angle based on the speed (KMPH)")]
        [SerializeField] AnimationCurve curve = AnimationCurve.Linear(0, 35, 250, 5);

        Steering steering;
        new Rigidbody rigidbody;

        void Update() {
            steering.range = GetMaxSteerAtSpeed(rigidbody.velocity.magnitude);
        }

        public float GetMaxSteerAtSpeed(float speed) {
            return curve.Evaluate(speed);
        }
    }
}
