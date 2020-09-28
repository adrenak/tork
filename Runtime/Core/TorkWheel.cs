/*
 * NOTE (A)
 * A car with wheels of diameter ~55cm usually has axle of diameter 2.2cm. That's a ratio of 25.
   Could not get values of real cars to verify the 55 and 2.2 cm claim but hat's what I've read somewhere 
   in a physics problem. This gets me good results. For example: A Civic has 170Nm of torque, at the first 
   gear that's about 530Nm of torque after transmission with a gear ratio of 3.1. If I set the max toque 
   in the motor at 530, I get an acceleration that I'd expect when someone in a Civic floors it.

   I'd imagine heavier vehicles have thicker axles, but their wheels are also large, so may be the 
   ratio would still hold. Or maybe it won't. Please change the value as you prefer.
 */

using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Adrenak.Tork {
    public class TorkWheel : MonoBehaviour {
        // See at the top of the file for NOTE (A) to read about this.
        const float engineShaftToWheelRatio = 25;

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
        public float springDamper = .25f;

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
        public float forwardFrictionCoeff = 1;

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
        ///Revolutions per minute of the wheel
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
            CalculateRPM();
        }

        void CalculateRPM() {
            float metersPerMinute = rigidbody.velocity.magnitude * 60;
            float wheelCircumference = 2 * Mathf.PI * radius;
            //RPM = metersPerMinute / wheelCircumference;
        }

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

            float dampingForce = rate * springStrength * springDamper;
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

        void CalculateFriction() {
            if (!isGrounded) return;

            Vector3 right = transform.right;
            Vector3 forward = transform.forward;

            Vector3 lateralVelocity = Vector3.Project(velocity, right);
            Vector3 forwardVelocity = Vector3.Project(velocity, forward);
            Vector3 slip = (forwardVelocity + lateralVelocity) / 2;

            float lateralFriction = Vector3.Project(right, slip).magnitude * suspensionForce.magnitude / 9.8f / Time.fixedDeltaTime * lateralFrictionCoeff;
            rigidbody.AddForceAtPosition(-Vector3.Project(slip, lateralVelocity).normalized * lateralFriction, hit.point);

            float motorForce = motorTorque / radius;
            float maxForwardFriction = motorForce * forwardFrictionCoeff;
            float appliedForwardFriction = 0;
            if (motorForce > 0)
                appliedForwardFriction = Mathf.Clamp(motorForce, 0, maxForwardFriction);
            else
                appliedForwardFriction = Mathf.Clamp(motorForce, maxForwardFriction, 0);

            rigidbody.AddForceAtPosition(forward * appliedForwardFriction * engineShaftToWheelRatio, hit.point);
        }
    }
}