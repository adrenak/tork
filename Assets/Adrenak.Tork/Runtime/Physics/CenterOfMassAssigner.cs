using UnityEngine;

namespace Adrenak.Tork {
    public class CenterOfMassAssigner : MonoBehaviour {
        public Transform m_Point;

        Rigidbody m_Rigidbody;

        void Start() {
            m_Rigidbody = GetComponent<Rigidbody>();
            if (m_Rigidbody == null) return;
            m_Rigidbody.centerOfMass = m_Point.localPosition;
        }
    }
}
