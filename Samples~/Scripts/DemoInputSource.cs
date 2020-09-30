using UnityEngine;

namespace Adrenak.Tork {
    public class DemoInputSource : MonoBehaviour {
        [SerializeField] Vehicle m_Vehicle;

        public void SetVehicle(Vehicle vehicle){
            m_Vehicle = vehicle;
        }

        void Update(){
            m_Vehicle.Motor.value = Input.GetAxis("Vertical");
            m_Vehicle.Steering.value = Input.GetAxis("Horizontal");
            m_Vehicle.Brake.value = Input.GetKey(KeyCode.Space) ? 1 : 0;
        }
    }
}
