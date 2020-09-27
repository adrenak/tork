using System;
using System.Collections.Generic;
using UnityEngine;

namespace Adrenak.Tork {
	public class AntiRollUnity : VehicleAddOn {
		[Serializable]
		public class Axle {
			public WheelCollider left;
			public WheelCollider right;
			public float force;
		}

		public new Rigidbody rigidbody;
		public List<Axle> axles;

		void FixedUpdate() {
			foreach(var axle in axles) {
				var wsDown = transform.TransformDirection(Vector3.down);
				wsDown.Normalize();

				float travelL = Mathf.Clamp01(axle.left.GetCompressionRatio()) ;
				float travelR = Mathf.Clamp01(axle.right.GetCompressionRatio());
				float antiRollForce = (travelL - travelR) * axle.force;

				if (axle.left.isGrounded)
					rigidbody.AddForceAtPosition(wsDown * -antiRollForce, axle.left.GetHit().point);
			
				if (axle.right.isGrounded)
					rigidbody.AddForceAtPosition(wsDown * antiRollForce, axle.right.GetHit().point);
			}
		}
	}
}
