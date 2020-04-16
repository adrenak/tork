using UnityEngine;

namespace Adrenak.Tork{
    public class CenterOfMassAssignerDebug : MonoBehaviour {
        [SerializeField] CenterOfMassAssigner target;

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;

            if (target.m_Point != null)
                Gizmos.DrawSphere(target.m_Point.position, .1f);
        }
    }
}
