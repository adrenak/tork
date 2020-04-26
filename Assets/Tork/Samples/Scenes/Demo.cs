using UnityEngine;
using System.Collections;

namespace Adrenak.Tork.Demo {
    public class Demo : MonoBehaviour {
        [Header("Common")]
        public Vehicle vehicle;
        public SmoothFollow smoothFollow;
        public Transform destination;

        public bool autoDrive;

        IVehicleDriver driver;
        MidAirStabilization midAirStabilization;

        void Start() {
            midAirStabilization = vehicle.GetAddOn<MidAirStabilization>();

            if (!autoDrive) {
                driver = new KeyboardVehicleDriver();
                driver.RegisterVehicle(vehicle);
            }
            else
                vehicle.GetAddOn<AutoDrive>().enabled = true;

            smoothFollow.target = vehicle.transform;
        }

        void Update() {
            if (!autoDrive)
                driver.DriveVehicles();
            else
                vehicle.GetAddOn<AutoDrive>().destination = destination.position;
        }

        void OnGUI() {
            var state = midAirStabilization.enabled;
            var msg = state ? "Switch OFF Mid Air Stabilization" : "Switch ON Mid Air Stabilization";
            if (GUI.Button(new Rect(0, 0, 300, 50), msg))
                midAirStabilization.enabled = !midAirStabilization.enabled;
        }
    }
}
