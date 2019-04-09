using UnityEngine;

namespace Adrenak.Tork {
	public class WheelRenderer : MonoBehaviour {
		public Wheel wheel;
		public float offset;
		public float angle;
		
		private void OnValidate() {
			SyncPosition();
		}

		public void Update() {
			SyncPosition();
			SyncRotation();
		}

		void SyncPosition() {
			if (wheel == null) {
				Debug.LogWarning("No Wheel attached to WheelRenderer (" + gameObject.name + ")");
				return;
			}

			transform.position = new Vector3(
				wheel.transform.position.x,
				wheel.transform.position.y,
				wheel.transform.position.z
			);

			transform.localPosition = new Vector3(
				transform.localPosition.x + offset,
				transform.localPosition.y - (wheel.suspensionDistance - wheel.CompressionDistance),
				transform.localPosition.z
			);
		}

		void SyncRotation() {
			//transform.localEulerAngles = wheel.transform.localEulerAngles;

			//var newX = transform.localEulerAngles.x + wheel.Velocity.magnitude;
			//transform.localEulerAngles = new Vector3(
			//	newX,
			//	//transform.localEulerAngles.x + wheel.Velocity.magnitude * 10,
			//	transform.localEulerAngles.y,
			//	transform.localEulerAngles.z
			//);

			transform.localEulerAngles = Vector3.zero;
			angle += (Time.deltaTime * wheel.Velocity.z) / (2 * Mathf.PI * wheel.radius) * 360;
			transform.Rotate(new Vector3(0, 1, 0), wheel.steerAngle - transform.localEulerAngles.y);
			transform.Rotate(new Vector3(1, 0, 0), angle);
		}
	}
}
