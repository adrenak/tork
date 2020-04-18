using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adrenak.Tork { 
    public class DownForceVsSpeed : MonoBehaviour {
        [Tooltip("The down force based on the speed (KMPH)")]
        [SerializeField] AnimationCurve curve = AnimationCurve.Linear(0, 0, 250, 2500);

        Aerodynamics m_Aerodynamics;
        new Rigidbody rigidbody;

        void Update() {
            m_Aerodynamics.downForce = GetDownForceAtSpeed(rigidbody.velocity.magnitude);
        }

        public float GetDownForceAtSpeed(float speed) {
            return curve.Evaluate(speed);
        }
    }
}

