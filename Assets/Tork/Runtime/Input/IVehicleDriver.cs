namespace Adrenak.Tork {
	public interface IVehicleDriver  {
        void RegisterVehicle(Vehicle vehicle);
        void DeregisterVehicle(Vehicle vehicle);
        void DriveVehicles();
	}
}
