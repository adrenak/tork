using System.Linq;
using UnityEngine;

namespace Adrenak.Tork {
	public class Aerodynamics : MonoBehaviour {
		[SerializeField] Rigidbody m_Rigidbody;
		[SerializeField] Wheel[] m_Wheels;

        [Header("Down Force")]
		public float downForce = 1000;
        
        [Header("Mid Air Stabilization")]
		public float stabilizationTorque = 15000;

        [Header("Mid Air Steer")]
		public float midAirSteerTorque = 1500;
		public float midAirSteerInput;

        [Header("Options")]
		public bool doApplyDownForce;
		public bool doStabilize;
		public bool doSteerMidAir;

		void FixedUpdate() {
			if (doApplyDownForce)
				ApplyDownForce();

			if (doStabilize)
				Stabilize();

			if (doSteerMidAir)
				SteerMidAir();
		}

		void ApplyDownForce() {
			if(downForce != 0)
				m_Rigidbody.AddForce(-Vector3.up * downForce);
		}

		void Stabilize() {
			var inAir = m_Wheels.Where(x => x.IsGrounded);
			if (inAir.Count() == 4) return;

			// Try to keep vehicle parallel to the ground while jumping
			Vector3 locUp = transform.up;
			Vector3 wsUp = new Vector3(0.0f, 1.0f, 0.0f);
			Vector3 axis = Vector3.Cross(locUp, wsUp);
			float force = stabilizationTorque;

			m_Rigidbody.AddTorque(axis * force);
		}

		void SteerMidAir() {
			if(!Mathf.Approximately(midAirSteerInput, 0))
				m_Rigidbody.AddTorque(new Vector3(0, midAirSteerInput * midAirSteerTorque, 0));
		}
	}
}
