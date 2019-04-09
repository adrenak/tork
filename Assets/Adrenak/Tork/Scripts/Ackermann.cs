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

		public float NearAngle;// { get; private set; }
		public float FarAngle { get; private set; }

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

		private void Start() { }

		public void SetAngle(float angle) {
			NearAngle = angle;
			FarAngle = GetFarAngle(angle, AxleWidth, AxleSeparation);

			// The rear wheels are always at 0 steer in Ackermann
			m_RearLeft.steerAngle = m_RearRight.steerAngle = 0;

			if (Mathf.Approximately(NearAngle, 0)) 
				m_FrontRight.steerAngle = m_FrontLeft.steerAngle = 0;
			else if (NearAngle > 0) {
				m_FrontRight.steerAngle = NearAngle;
				m_FrontLeft.steerAngle = FarAngle;
			}
			else if (NearAngle < 0) {
				m_FrontLeft.steerAngle = NearAngle;
				m_FrontRight.steerAngle = -FarAngle;
			}
		}

		public float[,] GetRadii() {
			float[,] result = new float[2, 2];

			if(Mathf.Abs(NearAngle) < 1) 
				result[0, 0] = result[1, 0] = result[0, 1] = result[1, 1] = 1000;

			if(NearAngle < -1) {
				result[0, 0] = AxleSeparation / Mathf.Sin(Mathf.Abs(NearAngle * Mathf.Deg2Rad));
				result[0, 1] = AxleSeparation / Mathf.Sin(Mathf.Abs(FarAngle * Mathf.Deg2Rad));
				result[1, 0] = AxleSeparation / Mathf.Tan(Mathf.Abs(NearAngle * Mathf.Deg2Rad));
				result[1, 1] = AxleSeparation / Mathf.Tan(Mathf.Abs(FarAngle * Mathf.Deg2Rad));
			}
			else if (NearAngle > 1) {
				result[0, 0] = AxleSeparation / Mathf.Sin(Mathf.Abs(FarAngle * Mathf.Deg2Rad));
				result[0, 1] = AxleSeparation / Mathf.Sin(Mathf.Abs(NearAngle * Mathf.Deg2Rad));
				result[1, 0] = AxleSeparation / Mathf.Tan(Mathf.Abs(FarAngle * Mathf.Deg2Rad));
				result[1, 1] = AxleSeparation / Mathf.Tan(Mathf.Abs(NearAngle * Mathf.Deg2Rad));
			}

			return result;
		}

		public static float GetFarAngle(float angle, float width, float separation) {
			float close = separation / Mathf.Tan(Mathf.Abs(angle) * Mathf.Deg2Rad);
			float far = close + width;

			return Mathf.Atan(separation / far) * Mathf.Rad2Deg;
		}

		void OnDrawGizmos() {
			if (drawLevel == DrawLevel.Always)
				Draw();
		}

		void OnDrawGizmosSelected() {
			if (drawLevel == DrawLevel.OnSelected)
				Draw();
		}

	#if UNITY_EDITOR
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
