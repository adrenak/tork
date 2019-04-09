using UnityEngine;

namespace Adrenak.Tork {
	[RequireComponent(typeof(Ackermann))]
	public class Steering : MonoBehaviour {
		public float range = 35;
		public float value;
		public float rate = 45;

		public Ackermann m_Ackermann { get; private set; }
		float m_CurrAngle;

		private void Awake() {
			m_Ackermann = GetComponent<Ackermann>();
		}

		private void Update() {
			var destination = value * range;
			m_CurrAngle = Mathf.MoveTowards(m_CurrAngle, destination, Time.deltaTime * rate);		
			m_CurrAngle = Mathf.Clamp(m_CurrAngle, -range, range);
			m_Ackermann.SetAngle(m_CurrAngle);
		}
	}
}
