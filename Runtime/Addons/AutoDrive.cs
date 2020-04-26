using UnityEngine;

namespace Adrenak.Tork {
    public class AutoDrive : VehicleAddOn {
        public Vehicle vehicle;
        public Transform destination;
        public float rate = .5f;
        public float minDistance;
        public AnimationCurve accelerationVsDist = AnimationCurve.Linear(0, .5f, 50, 1f);

        public bool IsInReverseArea {
            get {
                var towards = destination.position - transform.position;
                var locTowards = transform.InverseTransformDirection(towards);

                var currMaxSteerAngle = Mathf.Abs(vehicle.Steering.range);
                var separation = vehicle.Ackermann.AxleSeparation;
                var width = vehicle.Ackermann.AxleWidth;
                var currMinRadius = Ackermann.GetRadius(currMaxSteerAngle, separation, width);
                var pivot = vehicle.transform.position + vehicle.transform.right * Mathf.Sign(locTowards.x) * currMinRadius;

                if (Vector3.Distance(pivot, destination.position) < currMinRadius)
                    return true;

                bool isBehind = locTowards.z < 0;
                if (isBehind && Vector3.Distance(pivot, destination.position) < currMinRadius * 2)
                    return true;

                return false;
            }
        }

        void Update() {
            var distance = Vector3.Distance(vehicle.transform.position, destination.position);
            
            var towards = destination.position - transform.position;
            var locTowards = transform.InverseTransformDirection(towards);
            var reqAngle = Vector3.Angle(transform.forward, towards) * Mathf.Sign(locTowards.x);
            
            var isAhead = locTowards.z > 0;

            if (isAhead)
                vehicle.Brake.value = 1 - (distance / minDistance);
            else
                vehicle.Brake.value = 0;

            vehicle.Motor.value = accelerationVsDist.Evaluate(distance) * (IsInReverseArea ? -1 : 1);
            vehicle.Steering.Angle = Mathf.Lerp(vehicle.Steering.Angle, reqAngle, rate) * (IsInReverseArea ? -1 : 1);
        }
    }
}