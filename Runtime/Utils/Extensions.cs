using UnityEngine;

namespace Adrenak.Tork {
    public static class Extensions {
        public static float GetCompressionRatio(this WheelCollider WheelL) {
            WheelHit hit;
            bool groundedL = WheelL.GetGroundHit(out hit);
            if(groundedL)
                return 1 - ((-WheelL.transform.InverseTransformPoint(hit.point).y - WheelL.radius) / WheelL.suspensionDistance);
            return 0;
        }

        public static WheelHit GetHit(this WheelCollider WheelL) {
            WheelHit hit;
            bool groundedL = WheelL.GetGroundHit(out hit);
            return hit;
        }

        public static float Evaluate(this WheelFrictionCurve curve, float slip) {
            if (slip < curve.extremumSlip)
                return slip * curve.extremumValue / curve.extremumSlip;
            else
                //return curve.asymptoteValue;
                return ((slip - curve.asymptoteSlip) * (curve.extremumValue - curve.asymptoteValue) / (curve.extremumSlip - curve.asymptoteSlip)) + curve.asymptoteValue;
        }

        public static float Evaluate2(this WheelFrictionCurve curve, float slip) {
            if (slip < curve.extremumSlip)
                return curve.extremumValue;
            else
                return curve.asymptoteValue;
        }
    }
}
