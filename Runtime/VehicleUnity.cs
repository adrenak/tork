using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Adrenak.Tork {
    public class VehicleUnity : MonoBehaviour {
        [SerializeField] new Rigidbody rigidbody;
        public Rigidbody Rigidbody { get { return rigidbody; } }

        [Header("Core Components")]
        [SerializeField] AckermannUnity ackermann;
        public AckermannUnity Ackermann { get { return ackermann; } }

        [SerializeField] SteeringUnity steering;
        public SteeringUnity Steering { get { return steering; } }

        [SerializeField] MotorUnity motor;
        public MotorUnity Motor { get { return motor; } }

        [SerializeField] BrakesUnity brake;
        public BrakesUnity Brake { get { return brake; } }

        [Header("Add On Components (Populated on Awake)")]
        [SerializeField] List<VehicleAddOn> addOns;
        public List<VehicleAddOn> AddOns { get { return addOns; } }

        void Awake() {
            addOns = GetComponentsInChildren<VehicleAddOn>().ToList();
        }

        public T GetAddOn<T>() where T : VehicleAddOn {
            foreach (var addOn in addOns)
                if (addOn is T)
                    return addOn as T;
            return null;
        }
    }
}
