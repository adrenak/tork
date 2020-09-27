using UnityEngine;

namespace Adrenak.Tork {
    public class SteeringUnity : MonoBehaviour {
        public float range = 35;
        public float value; // 0..1
        public float rate = 45;

        public AckermannUnity ackermann;
        float m_CurrAngle;

        public float Angle {
            get { return range * value; }
            set {
                this.value = value / range;
            }
        }

        void Update() {
            var destination = value * range;
            m_CurrAngle = Mathf.MoveTowards(m_CurrAngle, destination, Time.deltaTime * rate);
            m_CurrAngle = Mathf.Clamp(m_CurrAngle, -range, range);
            ackermann.angle = m_CurrAngle;
        }
    }
}
