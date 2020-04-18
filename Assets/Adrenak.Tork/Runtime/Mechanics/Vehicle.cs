using UnityEngine;

namespace Adrenak.Tork {
	public class Vehicle : MonoBehaviour {
		[SerializeField] VehicleDriver m_Player;

        public Steering m_Steering;
        public Motor m_Motor;
        public Brakes m_Brake;

		public void SetPlayer(VehicleDriver player) {
			m_Player = player;
		}

		void Update() {
			if (m_Player == null) return;

			var input = m_Player.GetInput();

			m_Steering.value = input.steering;
			m_Motor.value = input.acceleration;
			m_Brake.value = input.brake;
		}
	}
}
