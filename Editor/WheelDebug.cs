using UnityEngine;
using UnityEditor;

namespace Adrenak.Tork {
    public class WheelDebug : MonoBehaviour {
        [SerializeField] Wheel target;

        void OnDrawGizmos() {
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(transform.position, transform.right, target.radius);

            Handles.color = Color.red;
            var p1 = transform.position + transform.up * Wheel.k_RayStartHeight;
            var p2 = transform.position - transform.up * (target.GetRayLen() - Wheel.k_RayStartHeight);
            Handles.DrawLine(p1, p2);

            var pos = transform.position + (-transform.up * (target.GetRayLen() - Wheel.k_RayStartHeight - target.CompressionDistance - target.radius));
            Handles.DrawWireDisc(pos, transform.right, target.radius);
        }
    }
}
