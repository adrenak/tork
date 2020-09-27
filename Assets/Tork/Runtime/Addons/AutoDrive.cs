using UnityEngine;

namespace Adrenak.Tork {
    public class AutoDrive : VehicleAddOn {
        public Vehicle vehicle;
        public Vector3 destination;
        public float steeringRate = .5f;
        public float brakingDistance;

        public float Direction {
            get {
                var towards = destination - vehicle.transform.position;
                var locTowards = vehicle.transform.InverseTransformDirection(towards);

                // Whether the current destination is behind us
                bool destinationIsBehind = locTowards.z < 0;

                // The minimum turning radius of the vehicle at the given steering angle range
                var minTurningDia = AckermannUtils.GetRadius(
                    vehicle.Steering.range,
                    vehicle.Ackermann.AxleSeparation,
                    vehicle.Ackermann.AxleWidth
                ) * 2;

                // The center of the vehicle if it turns at the minimum turning radius right now
                var minTurningRadiusCenter = vehicle.transform.position + vehicle.transform.right * Mathf.Sign(locTowards.x) * minTurningDia / 2;

                // We have the min turning radius, but we create a "band" of turnign radius
                // with an inner and an outer radii
                float innerDia = minTurningDia - minTurningDia / 4;
                float outerDia = minTurningDia + minTurningDia / 4;

                // x is the distance between the center about which we will move if we decide to move
                // with the smallest turning radius and the destination
                float x = Vector3.Distance(minTurningRadiusCenter, destination);

                // We certainly need to reverse as the inner radius is significantly smaller 
                // than the actual min turnign radius itself and we will certainly not make it
                // at min radius
                if (x < innerDia / 2)
                    return -1;

                // We certainly can go in the forward direction because the outer radius
                // is significantly larger than the actual min turning radius itself and we will
                // certainly make it at the min radius
                else if (x > outerDia / 2)
                    return 1;

                // 
                else if (destinationIsBehind && x < innerDia)
                    return -1;

                else if (destinationIsBehind && x > outerDia)
                    return 1;

                else if (x < minTurningDia)
                    return -(minTurningDia - x) / (minTurningDia - innerDia);
                else
                    return (x - minTurningDia) / (outerDia - minTurningDia);
            }
        }

        void Update() {
            var distance = Vector3.Distance(vehicle.transform.position, destination);

            var towards = destination - transform.position;
            var locTowards = transform.InverseTransformDirection(towards);
            var reqAngle = Vector3.Angle(transform.forward, towards) * Mathf.Sign(locTowards.x);

            var direction = Direction;
            var multiplier = Mathf.Clamp01(distance / brakingDistance);
            vehicle.Brake.value = 1 - multiplier;
            vehicle.Motor.value = direction;
            vehicle.Steering.Angle = Mathf.Lerp(vehicle.Steering.Angle, reqAngle, steeringRate) * direction;
        }
    }
}