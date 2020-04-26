using UnityEngine;
using System.Collections;

namespace Adrenak.Tork.Demo {
    public class Demo : MonoBehaviour {
        [Header("Common")]
        public Vehicle vehicle;
        public SmoothFollow smoothFollow;

        IVehicleDriver driver;
        MidAirStabilization midAirStabilization;

        void Start() {
            midAirStabilization = vehicle.GetAddOn<MidAirStabilization>();

            driver = new KeyboardVehicleDriver();
            driver.RegisterVehicle(vehicle);
            smoothFollow.target = vehicle.transform;
        }

        void Update() {
            //driver.DriveVehicles();

            if (Input.GetKeyDown(KeyCode.R))
                midAirStabilization.enabled = !midAirStabilization.enabled;
        }

        void OnGUI() {
            var state = midAirStabilization.enabled;
            var msg = state ? "Switch OFF Mid Air Stabilization" : "Switch ON Mid Air Stabilization";
            if (GUI.Button(new Rect(0, 0, 300, 50), msg))
                midAirStabilization.enabled = !midAirStabilization.enabled;
        }
    }
}
