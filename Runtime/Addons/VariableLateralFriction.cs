using UnityEngine;

namespace Adrenak.Tork {
    [RequireComponent(typeof(TorkWheel))]
    public class VariableLateralFriction : MonoBehaviour {
        public SlipFrictionCurve curve;
        TorkWheel wheel;

        void Awake() => wheel = GetComponent<TorkWheel>();

        void Update() =>
            wheel.lateralFrictionCoeff = curve.Evaluate(Vector3.Project(wheel.velocity, transform.right).magnitude);
    }
}
