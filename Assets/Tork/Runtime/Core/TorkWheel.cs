using UnityEditor;
using UnityEngine;

namespace Adrenak.Tork {
    public class TorkWheel : MonoBehaviour {
        #region FIELDS
        [Tooltip("Mass of the wheel")]
        /// <summary>
        /// Mass of the wheel
        /// </summary>
        public float mass = 20;

        [Tooltip("The radius of the wheel")]
        /// <summary>
        /// The radius of the wheel
        /// </summary>
        public float radius = 0.25f;

        [Header("Spring")]
        [Tooltip("How far the spring expands when it is in air.")]
        /// <summary>
        /// How far the spring expands when it is in air.
        /// </summary>
        public float springLength = .25f;

        [Tooltip("The k constant in the Hooke's law for the suspension. High values make the spring hard to compress. Make this larger for heavier vehicles. Recommended: 5x car mass.")]
        /// <summary>
        /// The k constact in the Hooke's law for the suspension. High values make the spring hard to compress. Make this higher for heavier vehicles
        /// </summary>
        public float springStrength = 5000;

        [Tooltip("Damping applied to the wheel. Higher values allow the car to negotiate bumps easily. Recommended: 0.25. Values outside [0, 0.5f] are unnatural")]
        /// <summary>
        /// Damping applied to the wheel. Higher values allow the car to negotiate bumps easily. Recommended: 0.25. Values outside [0, 0.5f] are unnatural
        /// </summary>
        public float springDampingFactor = .7f;

        [Tooltip("The rate (m/s) at which the spring relaxes to maximum length when the wheel is not on the ground. Recommended: suspension distance/2")]
        /// <summary>
        /// The rate (m/s) at which the spring relaxes to maximum length when the wheel is not on the ground.
        /// </summary>
        public float springRelaxRate = .125f;

        [Header("Friction")]
        [Tooltip("Determines the friction force that enables the wheel to exert sideways force while turning." +
        "Values below 1 will cause the wheel to drift. Values above 1 will cause sharp turns." +
        "Values outside 0 and 1 are unnatural. Tip: Reduce this for ice, increase for asphalt.")]
        /// <summary>
        /// Determines the friction force that enables the wheel to exert sideways force while turning.
        /// </summary>
        public float lateralFrictionCoeff = 1;

        [Tooltip("Determines the friction force that enables the wheel to exert forward force while experiencing torque. " +
        "Values below 1 will cause the wheel to slip (like ice). Values above 1 will cause the wheel to have high force (and thus higher speeds). " +
        "Values outside 0 and 1 are unnatural. Tip: Reduce this for ice, increase for asphalt.")]
        /// <summary>
        /// Determines the friction force that enables the wheel to exert forward force while experiencing torque.
        /// </summary>
        public float longitudinalFrictionCoeff = 1;

        [Tooltip("A constant friction % applied at all times. This allows the car to slow down on its own.")]
        /// <summary>
        /// A constant friction % applied at all times. This allows the car to slow down on its own.
        /// </summary>
        public float rollingFrictionCoeff = .1f;

        [Header("Raycasting")]
        /// <summary>
        /// The layers used for wheel raycast
        /// </summary>
        public LayerMask m_RaycastLayers;

        /// <summary>
        /// The velocity of the wheel (at the raycast hit point) in world space
        /// </summary>
        public Vector3 velocity { get; private set; }

        [Tooltip("The angle by which the wheel is turning")]
        /// <summary>
        /// The angle by which the wheel is turning
        /// </summary>
        public float steerAngle;

        [Tooltip("The torque applied to the wheel for moving in the forward and backward direction")]
        /// <summary>
        /// The torque applied to the wheel for moving in the forward and backward direction
        /// </summary>
        public float motorTorque;

        [Tooltip("The torque the brake is applying on the wheel")]
        /// <summary>
        /// The torque the brake is applying on the wheel
        /// </summary>
        public float brakeTorque;

        /// <summary>
        /// Revolutions per minute of the wheel
        /// </summary>
        public float rpm { get; private set; }

        /// <summary>
        /// Whether the wheel is touching the ground
        /// </summary>
        public bool isGrounded { get; private set; }

        /// <summary>
        /// The distance to which the spring of the wheel is compressed
        /// </summary>
        public float compressionDistance { get; private set; }
        float m_PrevCompressionDist;

        /// <summary>
        /// The ratio of compression distance and suspension distance
        /// 0 when the wheel is entirely uncompressed, 
        /// 1 when the wheel is entirely compressed
        /// </summary>
        public float compressionRatio { get; private set; }

        /// <summary>
        /// The raycast hit point of the wheel
        /// </summary>
        public RaycastHit hit { get { return m_Hit; } }
        RaycastHit m_Hit;

        public float sprungMass => suspensionForce.magnitude / 9.81f;

        /// <summary>
        /// The force the spring exerts on the rigidbody
        /// </summary>
        public Vector3 suspensionForce { get; private set; }

        Ray m_Ray;
        new Rigidbody rigidbody;
        public const float k_ExtraRayLength = 1;
        public float RayLength => springLength + radius + k_ExtraRayLength;
        #endregion

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
            velocity = rigidbody.GetPointVelocity(transform.position);

            transform.localEulerAngles = new Vector3(
                transform.localEulerAngles.x,
                steerAngle,
                transform.localEulerAngles.z
            );
            CalculateSuspension();
            CalculateFriction();
            //CalculateFriction();
        }

        #region SUSPENSION
        void CalculateSuspension() {
            m_Ray.direction = -transform.up;
            m_Ray.origin = transform.position + transform.up * k_ExtraRayLength;

            isGrounded = WheelRaycast(RayLength, ref m_Hit);
            // If we did not hit, relax the spring and return
            if (!isGrounded) {
                m_PrevCompressionDist = compressionDistance;
                compressionDistance = compressionDistance - Time.fixedDeltaTime * springRelaxRate;
                compressionDistance = Mathf.Clamp(compressionDistance, 0, springLength);
                return;
            }

            var force = 0.0f;
            compressionDistance = RayLength - hit.distance;
            compressionDistance = Mathf.Clamp(compressionDistance, 0, springLength);
            compressionRatio = compressionDistance / springLength;

            // Calculate the force from the springs compression using Hooke's Law
            float compressionForce = springStrength * compressionRatio;
            force += compressionForce;

            // Calculate the damping force based on compression rate of the spring
            float rate = (m_PrevCompressionDist - compressionDistance) / Time.fixedDeltaTime;
            m_PrevCompressionDist = compressionDistance;

            float dampingForce = rate * springStrength * springDampingFactor;
            force -= dampingForce;

            suspensionForce = transform.up * force;

            // Apply suspension force
            rigidbody.AddForceAtPosition(suspensionForce, (hit.point));
        }

        bool WheelRaycast(float maxDistance, ref RaycastHit hit) {
            if (Physics.Raycast(m_Ray.origin, m_Ray.direction, out hit, maxDistance, m_RaycastLayers))
                return true;
            return false;
        }
        #endregion

        #region FRICTION
        public float angularSlip = 0.0f;
        float MomentOfIntertia => .5f * mass * radius * radius;
        float TotalFrictionForce => suspensionForce.magnitude * longitudinalFrictionCoeff;
        float MotorForce => motorTorque / radius;
        float ApplicableMotorForce {
            get {
                //if (Mathf.Abs(MotorForce) == 0f) return 0;
                if (Mathf.Abs(MotorForce) < TotalFrictionForce)
                    return MotorForce;
                else 
                    return TotalFrictionForce * Mathf.Sign(MotorForce);
            }
        }

        void CalculateFriction() {
            float maxFriction = suspensionForce.magnitude * longitudinalFrictionCoeff;
            float motorForce = motorTorque / radius;
            float usedMotorForce;
            if (Mathf.Abs(motorForce) < maxFriction)
                usedMotorForce = motorForce;
            else
                usedMotorForce = Mathf.Sign(motorForce) * maxFriction;

            float unusedMotorForce = Mathf.Sign(motorForce) * (Mathf.Abs(motorForce) - Mathf.Abs(usedMotorForce));
            float unusedFriction = maxFriction - Mathf.Abs(usedMotorForce);

            // Increase slip due to unused motor force
            if (Mathf.Sign(motorForce) == Mathf.Sign(angularSlip))
                Slip(unusedMotorForce);
            // If MotorForce is acting against slip,
            // use the motor slip for reversing the slip
            else if (Mathf.Sign(motorForce) != Mathf.Sign(angularSlip))
                Slip(-Mathf.Sign(motorForce) * motorForce);

            // Decrease slip due to unused normalForce
            if(angularSlip != 0.0f)
                Slip(-Mathf.Sign(angularSlip) * unusedFriction);

            // Apply rolling friction at all times
            float longSpeed = transform.InverseTransformDirection(velocity).y;
            float rollingFrictionForce = suspensionForce.magnitude * rollingFrictionCoeff;
            Longitudinal(Mathf.Sign(longSpeed) * rollingFrictionForce);

            // Apply driving friction
            Longitudinal(usedMotorForce);

            Vector3 right = transform.right;
            Vector3 forward = transform.forward;
            Vector3 latVelocity = Vector3.Project(velocity, right);
            Vector3 longVelocity = Vector3.Project(velocity, forward);
            Vector3 planarVelocity = (longVelocity + latVelocity);
            float lateralFrictionMag = Vector3.Project(right, planarVelocity).magnitude * suspensionForce.magnitude / 9.8f / Time.fixedDeltaTime * lateralFrictionCoeff;
            Vector3 lateralFriction = -latVelocity.normalized * lateralFrictionMag;
            if(isGrounded)
                rigidbody.AddForceAtPosition(lateralFriction, hit.point);
        }
        
        public bool debug;

        void Slip(float torque, bool dontChangeDirection = true) {
            if (torque == 0f) return;
            bool slipWasZero = angularSlip == 0.0f;
            float acc = torque / MomentOfIntertia;
            float delta = acc * Time.fixedDeltaTime;
            angularSlip += delta;
            angularSlip = Mathf.Clamp(angularSlip, -9000, 9000);

            if (dontChangeDirection && !slipWasZero) {
                if (Mathf.Sign(delta) == -1)
                    angularSlip = Mathf.Clamp(angularSlip, 0, 9000);
                else if (Mathf.Sign(delta) == 1)
                    angularSlip = Mathf.Clamp(angularSlip, -9000, 0);
            }
        }

        void Longitudinal(float force) {
            if(isGrounded)
                rigidbody.AddForceAtPosition(transform.forward * force, hit.point);
        }
        #endregion
    }
}