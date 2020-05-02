using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Adrenak.Tork {
    public class Vehicle : MonoBehaviour {
        [SerializeField] Rigidbody rigidbody;
        public Rigidbody Rigidbody { get { return rigidbody; } }

        [Header("Core Components")]
        [SerializeField] Ackermann ackermann;
        public Ackermann Ackermann { get { return ackermann; } }

        [SerializeField] Steering steering;
        public Steering Steering { get { return steering; } }

        [SerializeField] Motor motor;
        public Motor Motor { get { return motor; } }

        [SerializeField] Brakes brake;
        public Brakes Brake { get { return brake; } }

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
