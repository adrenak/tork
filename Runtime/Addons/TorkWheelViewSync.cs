using UnityEngine;

namespace Adrenak.Tork {
	public class TorkWheelViewSync : VehicleAddOn {
        [System.Serializable]
        public class Entry{
            public TorkWheel wheel;
            public Transform view;
		    public float offset;
		    public float angle;
        }
        public Entry[] entries;
		
		public void Update() {
			if (entries.Length == 0) {
				Debug.LogWarning("No Entries in TorkWheelViewSync (" + gameObject.name + ")");
				return;
			}

            foreach(var entry in entries){
			    SyncPosition(entry);
                SyncRotation(entry);
            }
		}

		void SyncPosition(Entry entry) {
            var wheel = entry.wheel;
            var view = entry.view;
            var offset = entry.offset;

            view.position = new Vector3(
				wheel.transform.position.x,
                wheel.transform.position.y,
                wheel.transform.position.z
			);

            view.localPosition = new Vector3(
				view.localPosition.x + offset,
				view.localPosition.y - (wheel.springLength - wheel.compressionDistance),
				view.localPosition.z
			);
		}

		void SyncRotation(Entry entry) {
            var wheel = entry.wheel;
            var view = entry.view;

            view.localEulerAngles = Vector3.zero;
			entry.angle += (Time.deltaTime * view.InverseTransformDirection(wheel.velocity).z) / (2 * Mathf.PI * wheel.radius) * 360;
			view.Rotate(new Vector3(0, 1, 0), wheel.steerAngle - view.localEulerAngles.y);
			view.Rotate(new Vector3(1, 0, 0), entry.angle);
		}
	}
}
