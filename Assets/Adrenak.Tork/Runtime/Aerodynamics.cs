using System.Linq;
using UnityEngine;

namespace Adrenak.Tork {
	public class Aerodynamics : MonoBehaviour {
		[SerializeField] Rigidbody m_Rigidbody;
		[SerializeField] Wheel[] m_Wheels;

		public float downForce = 1000;
		public float stabilizationTorque = 15000;
		public float midAirSteerTorque = 1500;
		public float midAirSteerInput;

		public bool doApplyDownForce;
		public bool doStabilize;
		public bool doSteerMidAir;

		void FixedUpdate() {
			if (doApplyDownForce)
				DoApplyDownForce();

			if (doStabilize)
				Stabilize();

			if (doSteerMidAir)
				SteerMidAir();
		}

		void DoApplyDownForce() {
			if(downForce != 0)
				m_Rigidbody.AddForce(-Vector3.up * downForce);
		}

		void Stabilize() {
			var inAir = m_Wheels.Where(x => !x.IsGrounded);
			if (inAir.Count() != 4) return;

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
