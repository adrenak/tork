using UnityEngine;

namespace Adrenak.Tork {
    public class Vehicle : MonoBehaviour {
        readonly IVehicleDriver defaultDriver = new KeyboardVehicleDriver();
        IVehicleDriver driver;
        public IVehicleDriver Driver {
            get { return driver = driver ?? defaultDriver; }
            set { driver = value ?? defaultDriver; }
        }

        [SerializeField] Steering steering;
        public Steering Steer { get { return steering; } }

        [SerializeField] Motor motor;
        public Motor Motor { get { return motor; } }

        [SerializeField] Brakes brake;
        public Brakes Brake { get { return brake; } }

        void Update() {
            var input = Driver.GetInput();

            steering.value = input.steering;
            motor.value = input.acceleration;
            brake.value = input.brake;
        }
    }
}
