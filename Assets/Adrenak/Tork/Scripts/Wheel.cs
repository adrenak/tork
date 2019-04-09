using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Adrenak.Tork {
	public class Wheel : MonoBehaviour {
		[Tooltip("The radius of the wheel")]
		/// <summary>
		/// The radius of the wheel
		/// </summary>
		public float radius = 0.5f;

		[Header("Spring")]
		[Tooltip("How far the spring expands when it is in air.")]
		/// <summary>
		/// How far the spring expands when it is in air.
		/// </summary>
		public float suspensionDistance = .2f;

		[Tooltip(
			"The constant in the Hooke's spring law." +
			" High values make the spring hard to compress" +
			" Make this higher for heavier vehicles"
		)]
		/// <summary>
		/// The k in the Hooke's spring law.
		/// High values make the spring hard to compress
		/// Make this higher for heavier vehicles
		/// </summary>
		public float stiffness = 10000;

		[Tooltip("Damping applied to the wheel. Lower values make the wheel bounce more.")]
		/// <summary>
		/// Damping applied to the wheel. 
		/// Lower values make the wheel bounce more.
		/// </summary>
		public float damping = 5000;

		[Tooltip("The rate (m/s) at which the spring relaxes to maximum length when the wheel is not on the ground.")]
		/// <summary>
		/// The rate (m/s) at which the spring relaxes to maximum length when the wheel is not on the ground.
		/// </summary>
		public float relaxRate = .5f;

		[Header("Friction")]
		[Tooltip(
			"Multiplier for the wheels sideways friction." +
			" Values below 1 will cause the wheel to drift" +
			" Values above 1 will cause sharp turns"
		)]
		/// <summary>
		/// Multiplier for the wheels sideways friction.
		/// Values below 1 will cause the wheel to drift
		/// Values above 1 will cause sharp turns
		/// </summary>
		public float sidewaysGrip = 1;

		[Tooltip(
			"Multiplier for the wheels forward friction." +
			" Values below 1 will cause the wheel to slip (like ice)" +
			" Values above 1 will cause the wheel to have high traction (and this higher speed)"
		)]
		/// <summary>
		/// Multiplier for the wheels forward friction.
		/// Values below 1 will cause the wheel to slip (like ice)
		/// Values above 1 will cause the wheel to have high traction (and this higher speed)
		/// </summary>
		public float forwardGrip = 1;

		[Tooltip(
			"Multiplier for the wheels sideways friction. " +
			" Values below 1 cause the wheel to skid" +
			" Values above 1 will cause the wheel to take sharper turns"
		)]
		/// <summary>
		/// Multiplier for the wheels sideways friction
		/// Values below 1 cause the wheel to skid
		/// Values above 1 will cause the wheel to take sharper turns
		/// </summary>
		public float rollingFriction = .1f;

		[Header("Collision")]
		[Tooltip(
			"Whether the normal force from the wheel collision should be faked. " +
			" True causes the force to be applied only along the wheels upward direction" +
			" causing causing it to not slow down from collisions"
		)]
		/// <summary>
		/// Whether the normal force from the wheel collision should be faked.
		/// True causes the force to be applied only along the wheels upward direction
		/// causing causing it to not slow down from collisions
		/// </summary>
		public bool fakeNormals;


		public LayerMask m_RaycastLayers;

		public Vector3 Velocity { get; private set; }

		/// <summary>
		/// The angle by which the wheel is turning
		/// </summary>
		public float steerAngle { get; set; }

		/// <summary>
		/// The torque applied to the wheel for moving in the forward and backward direction
		/// </summary>
		public float motorTorque { get; set; }

		/// <summary>
		/// The torque the brake is applying on the wheel
		/// </summary>
		public float brakeTorque { get; set; }

		/// <summary>
		/// Whether the wheel is touching the ground
		/// </summary>
		public bool isGrounded { get; private set; }

		/// <summary>
		/// The distance to which the spring of the wheel is compressed
		/// </summary>
		public float CompressionDistance { get; private set; }
		float m_PrevCompressionDist;

		/// <summary>
		/// The ratio of compression distance and suspension distance
		/// 0 when the wheel is entirely uncompressed, 
		/// 1 when the wheel is entirely compressed
		/// </summary>
		public float CompressionRatio { get; private set; }

		/// <summary>
		/// The raycast hit point of the wheel
		/// </summary>
		public RaycastHit Hit { get { return m_Hit; } }
		RaycastHit m_Hit;

		/// <summary>
		/// The force the spring exerts on the rigidbody
		/// </summary>
		public Vector3 SpringForce { get; private set; }

		Ray m_Ray;
		new Rigidbody rigidbody;
		const float k_RayStartHeight = 1;

		void Start() {
			m_Ray = new Ray();

			// Remove rigidbody component from the wheel
			rigidbody = GetComponent<Rigidbody>();
			if (rigidbody)
				Destroy(rigidbody);

			// Get the rigidbody component from the parent
			rigidbody = GetComponentInParent<Rigidbody>();
		}

		void FixedUpdate() {
			transform.localEulerAngles = new Vector3(0, steerAngle, 0);
			CalculateSuspension();
			CalculateFriction();
		}

		void CalculateSuspension() {
			float rayLen = GetRayLen();
			m_Ray.direction = -transform.up.normalized;
			m_Ray.origin = transform.position + transform.up * k_RayStartHeight;

			bool didHit = WheelRaycast(rayLen, ref m_Hit);
			// If we did not hit, relax the spring and return
			if (!didHit) {
				m_PrevCompressionDist = CompressionDistance;
				CompressionDistance = CompressionDistance - Time.fixedDeltaTime * relaxRate;
				CompressionDistance = Mathf.Clamp(CompressionDistance, 0, suspensionDistance);

				isGrounded = false;
				return;
			}

			var force = 0.0f;
			isGrounded = true;
			CompressionDistance = rayLen - Hit.distance;
			CompressionDistance = Mathf.Clamp(CompressionDistance, 0, suspensionDistance);
			CompressionRatio = Mathf.Clamp01(CompressionDistance / suspensionDistance);

			// Calculate the force from the springs compression using Hooke's Law
			float springForce = stiffness * CompressionRatio;
			force += springForce;

			// Calculate the damping force based on compression rate of the spring
			float rate = (CompressionDistance - m_PrevCompressionDist) / Time.fixedDeltaTime;
			m_PrevCompressionDist = CompressionDistance;

			float damperForce = rate * damping;
			force += damperForce;

			// When normals are faked, the spring force vector is not applied towards the wheel's center,
			// instead it is resolved along the global Y axis and applied
			// This helps maintain velocity over speed bumps, however may be unrealistic
			if (fakeNormals)
				SpringForce = Vector3.up * force;
			else {
				float fakedScale = Vector3.Dot(Hit.normal, transform.up);
				force *= fakedScale;
				SpringForce = transform.up * force;
			}

			// Apply suspension force
			rigidbody.AddForceAtPosition(SpringForce, (Hit.point));
		}

		bool WheelRaycast(float maxDistance, ref RaycastHit nearestHit) {
			RaycastHit hit;
			
			if (Physics.Raycast(m_Ray.origin, m_Ray.direction, out hit, maxDistance, m_RaycastLayers)) {
				nearestHit = hit;
				return true;
			}
			return false;
		}

		void CalculateFriction() {
			if (!isGrounded) return;

			// Contact basis (can be different from wheel basis)
			Vector3 normal = Hit.normal;
			Vector3 side = transform.right;
			Vector3 forward = transform.forward;

			// Calculate sliding velocity (velocity without normal force)
			Velocity = rigidbody.GetPointVelocity(Hit.point);
			Vector3 sideVel = Vector3.Dot(Velocity, side) * side;
			Vector3 forwardVel = Vector3.Dot(Velocity, forward) * forward;
			Vector3 velocity2D = sideVel + forwardVel;

			Vector3 momentum = velocity2D * rigidbody.mass;

			var latForce = Vector3.Dot(-momentum, side) * side * sidewaysGrip;
			var longForce = Vector3.Dot(-momentum, forward) * forward * forwardGrip;
			Vector3 frictionForce = latForce + longForce;

			// Apply rolling friction
			longForce *= 1 - rollingFriction;

			// Apply brake force
			var brakeForceMag = brakeTorque / radius;
			brakeForceMag = Mathf.Clamp(brakeForceMag, 0, longForce.magnitude);
			longForce -= longForce.normalized * brakeForceMag;

			frictionForce -= longForce;
			rigidbody.AddForceAtPosition(frictionForce, Hit.point);

			rigidbody.AddForceAtPosition(forward * motorTorque / radius * forwardGrip, Hit.point);
		}

		void AddForce(Vector3 force) {
			if (Mathf.Approximately(force.magnitude, 0)) return;
			rigidbody.AddForceAtPosition(force, Hit.point);
		}

		float GetRayLen() {
			return suspensionDistance + radius + k_RayStartHeight;
		}

	#if UNITY_EDITOR
		void OnDrawGizmos() {
			Handles.color = Color.yellow;
			Handles.DrawWireDisc(transform.position, transform.right, radius);

			Handles.color = Color.red;
			var p1 = transform.position + transform.up * k_RayStartHeight;
			var p2 = transform.position - transform.up * (GetRayLen() - k_RayStartHeight);
			Handles.DrawLine(p1, p2);

			var pos = transform.position + (-transform.up * (GetRayLen() - k_RayStartHeight - CompressionDistance - radius));
			Handles.DrawWireDisc(pos, transform.right, radius);
		}
	#endif
	}
}
