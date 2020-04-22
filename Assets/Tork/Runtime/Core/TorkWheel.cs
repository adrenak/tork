using UnityEngine;

namespace Adrenak.Tork {
    public class TorkWheel : MonoBehaviour {
        // See at the bottom of the file for NOTE (A) to read about this.
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
        public float suspensionDistance = .25f;

        [Tooltip("The k constant in the Hooke's law for the suspension. High values make the spring hard to compress. Make this larger for heavier vehicles. Recommended: 5x car mass.")]
        /// <summary>
        /// The k constact in the Hooke's law for the suspension. High values make the spring hard to compress. Make this higher for heavier vehicles
        /// </summary>
        public float stiffness = 5000;

        [Tooltip("Damping applied to the wheel. Higher values allow the car to negotiate bumps easily. Recommended: 0.25. Values outside [0, 0.5f] are unnatural")]
        /// <summary>
        /// Damping applied to the wheel. Higher values allow the car to negotiate bumps easily. Recommended: 0.25. Values outside [0, 0.5f] are unnatural
        /// </summary>
        public float dampingFactor = .25f;

        [Tooltip("The rate (m/s) at which the spring relaxes to maximum length when the wheel is not on the ground. Recommended: suspension distance/2")]
        /// <summary>
        /// The rate (m/s) at which the spring relaxes to maximum length when the wheel is not on the ground.
        /// </summary>
        public float relaxRate = .125f;

        [Header("Friction")]
        [Tooltip("Determines the friction force that enables the wheel to exert sideways force while turning." +
        "Values below 1 will cause the wheel to drift. Values above 1 will cause sharp turns." +
        "Values outside 0 and 1 are unnatural. Tip: Reduce this for ice, increase for asphalt.")]
        /// <summary>
        /// Determines the friction force that enables the wheel to exert sideways force while turning.
        /// </summary>
        public float sidewaysGrip = 1;

        [Tooltip("Determines the friction force that enables the wheel to exert forward force while experiencing torque. " +
        "Values below 1 will cause the wheel to slip (like ice). Values above 1 will cause the wheel to have high force (and thus higher speeds). " +
        "Values outside 0 and 1 are unnatural. Tip: Reduce this for ice, increase for asphalt.")]
        /// <summary>
        /// Determines the friction force that enables the wheel to exert forward force while experiencing torque.
        /// </summary>
        public float forwardGrip = 1;

        [Tooltip("A constant friction % applied at all times. This allows the car to slow down on its own.")]
        /// <summary>
        /// A constant friction % applied at all times. This allows the car to slow down on its own.
        /// </summary>
        public float rollingFriction = .1f;

        [Header("Raycasting")]
        /// <summary>
        /// The layers used for wheel raycast
        /// </summary>
        public LayerMask m_RaycastLayers;

        /// <summary>
        /// The velocity of the wheel (at the raycast hit point) in world space
        /// </summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>
        /// The angle by which the wheel is turning
        /// </summary>
        public float SteerAngle { get; set; }

        /// <summary>
        /// The torque applied to the wheel for moving in the forward and backward direction
        /// </summary>
        public float MotorTorque { get; set; }

        /// <summary>
        /// The torque the brake is applying on the wheel
        /// </summary>
        public float BrakeTorque { get; set; }

        /// <summary>
        ///Revolutions per minute of the wheel
        /// </summary>
        public float RPM { get; private set; }

        /// <summary>
        /// Whether the wheel is touching the ground
        /// </summary>
        public bool IsGrounded { get; private set; }

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
        public Vector3 SuspensionForce { get; private set; }

        Ray m_Ray;
        new Rigidbody rigidbody;
        public const float k_RayStartHeight = 1;

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
            transform.localEulerAngles = new Vector3(0, SteerAngle, 0);
            CalculateSuspension();
            CalculateFriction();
            CalculateRPM();
        }

        void CalculateRPM() {
            float metersPerMinute = rigidbody.velocity.magnitude * 60;
            float wheelCircumference = 2 * Mathf.PI * radius;
            RPM = metersPerMinute / wheelCircumference;
        }

        void CalculateSuspension() {
            float rayLen = GetRayLen();
            m_Ray.direction = -transform.up.normalized;
            m_Ray.origin = transform.position + transform.up * k_RayStartHeight;

            IsGrounded = WheelRaycast(rayLen, ref m_Hit);
            // If we did not hit, relax the spring and return
            if (!IsGrounded) {
                m_PrevCompressionDist = CompressionDistance;
                CompressionDistance = CompressionDistance - Time.fixedDeltaTime * relaxRate;
                CompressionDistance = Mathf.Clamp(CompressionDistance, 0, suspensionDistance);

                IsGrounded = false;
                return;
            }

            var force = 0.0f;
            CompressionDistance = rayLen - Hit.distance;
            CompressionDistance = Mathf.Clamp(CompressionDistance, 0, suspensionDistance);
            CompressionRatio = CompressionDistance / suspensionDistance;

            // Calculate the force from the springs compression using Hooke's Law
            float compressionForce = stiffness * CompressionRatio;
            force += compressionForce;

            // Calculate the damping force based on compression rate of the spring
            float rate = (m_PrevCompressionDist - CompressionDistance) / Time.fixedDeltaTime;
            m_PrevCompressionDist = CompressionDistance;

            float damperForce = rate * stiffness * (1 - dampingFactor);
            force -= damperForce;

            force *= Vector3.Dot(Hit.normal, transform.up);
            SuspensionForce = transform.up * force;

            // Apply suspension force
            rigidbody.AddForceAtPosition(SuspensionForce, (Hit.point));
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
            Velocity = rigidbody.GetPointVelocity(Hit.point);

            if (!IsGrounded) return;

            // Contact basis (can be different from wheel basis)
            Vector3 normal = Hit.normal;
            Vector3 side = transform.right;
            Vector3 forward = transform.forward;

            // Apply less force if the vehicle is tilted
            var angle = Vector3.Angle(normal, transform.up);
            var multiplier = Mathf.Cos(angle * Mathf.Deg2Rad);

            // Calculate sliding velocity (velocity without normal force)
            Vector3 sideVel = Vector3.Dot(Velocity, side) * side * multiplier;
            Vector3 forwardVel = Vector3.Dot(Velocity, forward) * forward * multiplier;
            Vector3 planarVelocity2D = sideVel + forwardVel;

            Vector3 planarMomentum = planarVelocity2D * rigidbody.mass;

            // NOTE: I have little idea how this code works 
            // It can be drawn on a graph to see what's happening but I haven't
            // had the time to make it more readable. If you improve it, please make a pull request.
            var latForce = Vector3.Dot(-planarMomentum, side) * side * sidewaysGrip;
            var longForce = Vector3.Dot(-planarMomentum, forward) * forward * forwardGrip;
            Vector3 gripResistanceForce = latForce + longForce;

            // Apply rolling resistance friction
            float rollingResistanceForceMag = 1 - rollingFriction;
            longForce *= rollingResistanceForceMag;

            // Apply brake resistance force
            var brakeResistanceForceMag = BrakeTorque / radius;
            brakeResistanceForceMag = Mathf.Clamp(brakeResistanceForceMag, 0, longForce.magnitude);
            Vector3 brakeResistanceForce = longForce.normalized * brakeResistanceForceMag;
            longForce -= brakeResistanceForce;

            gripResistanceForce -= longForce;

            rigidbody.AddForceAtPosition(gripResistanceForce, Hit.point);
            rigidbody.AddForceAtPosition(forward * MotorTorque / radius * forwardGrip * engineShaftToWheelRatio, Hit.point);
        }
        
        public float GetRayLen() {
            return suspensionDistance + radius + k_RayStartHeight;
        }
    }
}

/*
 * NOTE (A)
 * A car with wheels of diameter ~55cm usually has axle of diameter 2.2cm. That's a ratio of 25.
   Could not get values of real cars to verify the 55 and 2.2 cm cliam but hat's what I've read somewhere 
   in a physics problem. This gets me good results. For example: A Civic has 170Nm of torque, at the first 
   gear that's about 530Nm of torque after transmission with a gear ratio of 3.1. If I set the max toque 
   in the motor at 530, I get an acceleration that I'd expect when someone in a Civic floors it.

   I'd imagine heavier vehicles have thicker axles, but their wheels are also large, so may be the 
   ratio would still hold. Or maybe it won't. Please change the value as you prefer.
 */
