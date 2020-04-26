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

        public float[,] Radii { get; private set; }

        public float Angle { get; private set; }

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

        private void Start() {
            Radii = new float[2, 2];
        }

        private void Update() {
            Radii = GetRadii(Angle, AxleSeparation, AxleWidth);
        }

        public void SetAngle(float angle) {
            Angle = angle;
            var farAngle = GetSecondaryAngle(angle, AxleWidth, AxleSeparation);

            // The rear wheels are always at 0 steer in Ackermann
            m_RearLeft.SteerAngle = m_RearRight.SteerAngle = 0;

            if (Mathf.Approximately(Angle, 0))
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
                return m_FrontRight.transform.position + Radii[0, 1] * m_FrontRight.transform.right;
            else
                return m_FrontLeft.transform.position - Radii[0, 0] * m_FrontLeft.transform.right;
        }

        public float[,] CurrentRadii {
            get { return GetRadii(Angle, AxleSeparation, AxleWidth); }
        }

        public float CurrentTurningRadius {
            get {
                var radii = CurrentRadii;
                return (radii[0, 0] + radii[0, 1]) / 2;
            }
        }

        public static float GetSecondaryAngle(float angle, float width, float separation) {
            float close = separation / Mathf.Tan(Mathf.Abs(angle) * Mathf.Deg2Rad);
            float far = close + width;

            return Mathf.Atan(separation / far) * Mathf.Rad2Deg;
        }

        public static float GetRadius(float angle, float separation, float width){
            var radii = GetRadii(angle, separation, width);
            return radii[0, 0] + radii[0, 1] + radii[1, 0] + radii[1, 1] / 4;
        }

        public static float[,] GetRadii(float angle, float separation, float width) {
            var secAngle = GetSecondaryAngle(angle, width, separation);
            float[,] radii = new float[2, 2];

            if (Mathf.Abs(angle) < 1)
                radii[0, 0] = radii[1, 0] = radii[0, 1] = radii[1, 1] = 1000;

            if (angle < -1) {
                radii[0, 0] = separation / Mathf.Sin(Mathf.Abs(angle * Mathf.Deg2Rad));
                radii[0, 1] = separation / Mathf.Sin(Mathf.Abs(secAngle * Mathf.Deg2Rad));
                radii[1, 0] = separation / Mathf.Tan(Mathf.Abs(angle * Mathf.Deg2Rad));
                radii[1, 1] = separation / Mathf.Tan(Mathf.Abs(secAngle * Mathf.Deg2Rad));
            }
            else if (angle > 1) {
                radii[0, 0] = separation / Mathf.Sin(Mathf.Abs(secAngle * Mathf.Deg2Rad));
                radii[0, 1] = separation / Mathf.Sin(Mathf.Abs(angle * Mathf.Deg2Rad));
                radii[1, 0] = separation / Mathf.Tan(Mathf.Abs(secAngle * Mathf.Deg2Rad));
                radii[1, 1] = separation / Mathf.Tan(Mathf.Abs(angle * Mathf.Deg2Rad));
            }

            return radii;
        }
    }
}
