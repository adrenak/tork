using UnityEngine;

namespace Adrenak.Tork {
    /// <summary>
    /// An implementation of Ackermann steering mechanism
    /// </summary>
    public class Ackermann : MonoBehaviour {
        [SerializeField] TorkWheel m_FrontRight;
        public TorkWheel FrontRightWheel { get { return m_FrontRight; } }

        [SerializeField] TorkWheel m_FrontLeft;
        public TorkWheel FrontLeftWheel { get { return m_FrontLeft; } }

        [SerializeField] TorkWheel m_RearRight;
        public TorkWheel RearRightWheel { get { return m_RearRight; } }

        [SerializeField] TorkWheel m_RearLeft;
        public TorkWheel RearLeftWheel { get { return m_RearLeft; } }

        public float Angle { get; set; }

        public float AxleSeparation {
            get { return (m_FrontLeft.transform.position - m_RearLeft.transform.position).magnitude; }
        }

        public float AxleWidth {
            get { return (m_FrontLeft.transform.position - m_FrontRight.transform.position).magnitude; }
        }

        public float FrontRightRadius {
            get { return AxleSeparation / Mathf.Sin(Mathf.Abs(m_FrontRight.SteerAngle)); }
        }

        public float FrontLeftRadius {
            get { return AxleSeparation / Mathf.Sin(Mathf.Abs(m_FrontLeft.SteerAngle)); }
        }

        void Update() {
            var farAngle = AckermannUtils.GetSecondaryAngle(Angle, AxleSeparation, AxleWidth);

            // The rear wheels are always at 0 steer in Ackermann
            m_RearLeft.SteerAngle = m_RearRight.SteerAngle = 0;

            if (Mathf.Approximately((float)Angle, 0))
                m_FrontRight.SteerAngle = m_FrontLeft.SteerAngle = 0;
            else if (Angle > 0) {
                m_FrontRight.SteerAngle = Angle;
                m_FrontLeft.SteerAngle = farAngle;
            }
            else if (Angle < 0) {
                m_FrontLeft.SteerAngle = Angle;
                m_FrontRight.SteerAngle = -farAngle;
            }
        }

        public Vector3 GetPivot() {
            if (Angle > 0)
                return m_RearRight.transform.position + CurrentRadii[0] * m_RearRight.transform.right;
            else
                return m_RearLeft.transform.position - CurrentRadii[0] * m_RearLeft.transform.right;
        }

        public float[] CurrentRadii {
            get { return AckermannUtils.GetRadii(Angle, AxleSeparation, AxleWidth); }
        }
    }
}
