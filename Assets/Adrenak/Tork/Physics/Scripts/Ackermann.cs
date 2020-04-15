using UnityEngine;

namespace Adrenak.Tork {
	/// <summary>
	/// An implementation of Ackermann steering mechanism
	/// </summary>
	public class Ackermann : MonoBehaviour {
		public DrawLevel drawLevel;

		[SerializeField] Wheel m_FrontRight;
		[SerializeField] Wheel m_FrontLeft;
		[SerializeField] Wheel m_RearRight;
		[SerializeField] Wheel m_RearLeft;
		public float[,] Radii { get; private set; }

		public float Angle { get; private set; }

		public float AxleSeparation {
			get { return (m_FrontLeft.transform.position - m_RearLeft.transform.position).magnitude; }
		}

		public float AxleWidth {
			get { return (m_FrontLeft.transform.position - m_FrontRight.transform.position).magnitude; }
		}
	
		public float FrontRightRadius {
			get { return AxleSeparation / Mathf.Sin(Mathf.Abs(m_FrontRight.steerAngle)); }
		}

		public float FrontLeftRadius {
			get { return AxleSeparation / Mathf.Sin(Mathf.Abs(m_FrontLeft.steerAngle)); }
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
			m_RearLeft.steerAngle = m_RearRight.steerAngle = 0;

			if (Mathf.Approximately(Angle, 0)) 
				m_FrontRight.steerAngle = m_FrontLeft.steerAngle = 0;
			else if (Angle > 0) {
				m_FrontRight.steerAngle = Angle;
				m_FrontLeft.steerAngle = farAngle;
			}
			else if (Angle < 0) {
				m_FrontLeft.steerAngle = Angle;
				m_FrontRight.steerAngle = -farAngle;
			}
		}

		public Vector3 GetPivot() {
			if(Angle > 0)
				return m_FrontRight.transform.position + Radii[0, 1] * m_FrontRight.transform.right;
			else
				return m_FrontLeft.transform.position - Radii[0, 0] * m_FrontLeft.transform.right;
		}

		public static float GetSecondaryAngle(float angle, float width, float separation) {
			float close = separation / Mathf.Tan(Mathf.Abs(angle) * Mathf.Deg2Rad);
			float far = close + width;

			return Mathf.Atan(separation / far) * Mathf.Rad2Deg;
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


	#if UNITY_EDITOR
		void OnDrawGizmos() {
			if (drawLevel == DrawLevel.Always)
				Draw();
		}

		void OnDrawGizmosSelected() {
			if (drawLevel == DrawLevel.OnSelected)
				Draw();
		}

		void Draw() {
			UnityEditor.Handles.color = Color.cyan;

			if(m_FrontLeft != null) {
				var angle = m_FrontLeft.transform.localEulerAngles.y;
				var origin = m_FrontLeft.transform.position;
				UnityEditor.Handles.DrawLine(origin, origin + Quaternion.AngleAxis(angle, Vector3.up) * transform.forward);
			}

			if (m_FrontRight != null) {
				var angle = m_FrontRight.transform.localEulerAngles.y;
				var origin = m_FrontRight.transform.position;
				UnityEditor.Handles.DrawLine(origin, origin + Quaternion.AngleAxis(angle, Vector3.up) * transform.forward);
			}
		}
	#endif
	}
}
