using UnityEngine;

namespace Adrenak.Tork {
	public class Steering : MonoBehaviour {
		public float range = 35;
		public float value; // 0..1
		public float rate = 45;

        public Ackermann ackermann;
		float m_CurrAngle;

		void Update() {
			var destination = value * range;
			m_CurrAngle = Mathf.MoveTowards(m_CurrAngle, destination, Time.deltaTime * rate);		
			m_CurrAngle = Mathf.Clamp(m_CurrAngle, -range, range);
			ackermann.SetAngle(m_CurrAngle);
		}
	}
}
