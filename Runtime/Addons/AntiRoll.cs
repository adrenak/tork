using System;
using System.Collections.Generic;
using UnityEngine;

namespace Adrenak.Tork {
	public class AntiRoll : VehicleAddOn {
		[Serializable]
		public class Axle {
			public TorkWheel left;
			public TorkWheel right;
			public float force;
		}

		public new Rigidbody rigidbody;
		public List<Axle> axles;

		void FixedUpdate() {
			foreach(var axle in axles) {
				var wsDown = transform.TransformDirection(Vector3.down);
				wsDown.Normalize();

				float travelL = Mathf.Clamp01(axle.left.compressionRatio);
				float travelR = Mathf.Clamp01(axle.right.compressionRatio);
				float antiRollForce = (travelL - travelR) * axle.force;

				if (axle.left.isGrounded)
					rigidbody.AddForceAtPosition(wsDown * -antiRollForce, axle.left.hit.point);
			
				if (axle.right.isGrounded)
					rigidbody.AddForceAtPosition(wsDown * antiRollForce, axle.right.hit.point);
			}
		}
	}
}
