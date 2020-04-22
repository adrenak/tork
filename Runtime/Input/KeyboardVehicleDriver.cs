using UnityEngine;
using System.Collections.Generic;

namespace Adrenak.Tork {
    public class KeyboardVehicleDriver : IVehicleDriver {
        public List<Vehicle> Vehicles { get; private set; }

        public KeyboardVehicleDriver() {
            Vehicles = new List<Vehicle>();
        }

        public void RegisterVehicle(Vehicle vehicle) {
            Vehicles.Add(vehicle);
        }

        public void DeregisterVehicle(Vehicle vehicle){
            Vehicles.Remove(vehicle);
        }

        public void DriveVehicles() {
            foreach (var vehicle in Vehicles){
                vehicle.Motor.value = Input.GetAxis("Vertical");
                vehicle.Steering.value = Input.GetAxis("Horizontal");
                vehicle.Brake.value = Input.GetAxis("Jump");
            }
        }
    }
}