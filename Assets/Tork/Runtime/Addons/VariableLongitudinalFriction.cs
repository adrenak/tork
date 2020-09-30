using UnityEngine;

namespace Adrenak.Tork {
    [RequireComponent(typeof(TorkWheel))]
    public class VariableLongitudinalFriction : MonoBehaviour {
        public SlipFrictionCurve curve;
        TorkWheel wheel;

        void Awake() => wheel = GetComponent<TorkWheel>();

        void Update() {
            wheel.longitudinalFrictionCoeff = curve.Evaluate(wheel.angularSlip);
        }
    }
}
