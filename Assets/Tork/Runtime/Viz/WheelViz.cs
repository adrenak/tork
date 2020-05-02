using UnityEngine;
using UnityEditor;

namespace Adrenak.Tork {
    public class WheelViz : MonoBehaviour {
        [SerializeField] TorkWheel target;

        void OnDrawGizmos() {
            if (target == null) return;

            var t = target.transform;
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(t.position, t.right, target.radius);

            Handles.color = Color.red;
            var p1 = t.position + t.up * TorkWheel.k_RayStartHeight;
            var p2 = t.position - t.up * (target.GetRayLen() - TorkWheel.k_RayStartHeight);
            Handles.DrawLine(p1, p2);

            var pos = t.position + (-t.up * (target.GetRayLen() - TorkWheel.k_RayStartHeight - target.CompressionDistance - target.radius));
            Handles.DrawWireDisc(pos, t.right, target.radius);
        }
    }
}
