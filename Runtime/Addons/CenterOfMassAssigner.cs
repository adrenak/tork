using UnityEngine;

namespace Adrenak.Tork {
    public class CenterOfMassAssigner : VehicleAddOn {
        [SerializeField] Rigidbody m_Rigidbody;

        void Start() {
            if (m_Rigidbody == null) 
                return;
            m_Rigidbody.centerOfMass = m_Rigidbody.transform.InverseTransformPoint(transform.position);
        }
    }
}
