using UnityEngine;

namespace Adrenak.Tork {
    public class AckermannDebug : MonoBehaviour {
        [SerializeField] Ackermann target;

        void OnDrawGizmos() {
            if (target.drawLevel == DrawLevel.Always)
                Draw();
        }

        void OnDrawGizmosSelected() {
            if (target.drawLevel == DrawLevel.OnSelected)
                Draw();
        }

        void Draw() {
            UnityEditor.Handles.color = Color.cyan;

            if (target.FrontLeftWheel != null) {
                var angle = target.FrontLeftWheel.transform.localEulerAngles.y;
                var origin = target.FrontLeftWheel.transform.position;
                UnityEditor.Handles.DrawLine(origin, origin + Quaternion.AngleAxis(angle, Vector3.up) * transform.forward);
            }

            if (target.FrontRightWheel != null) {
                var angle = target.FrontRightWheel.transform.localEulerAngles.y;
                var origin = target.FrontRightWheel.transform.position;
                UnityEditor.Handles.DrawLine(origin, origin + Quaternion.AngleAxis(angle, Vector3.up) * transform.forward);
            }
        }
    }
}
