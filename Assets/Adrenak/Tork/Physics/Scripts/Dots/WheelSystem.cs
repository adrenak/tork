using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Animation;
using RaycastHit = Unity.Physics.RaycastHit;
using Debug = UnityEngine.Debug;
using Unity.Collections;

namespace Pragnesh.Dots
{
	
	public struct WheelData : IComponentData
{
	public float radius; //= 0.5f;

	[Header("Spring")]
	[Tooltip("How far the spring expands when it is in air.")]
	/// <summary>
	/// How far the spring expands when it is in air.
	/// </summary>
	public float suspensionDistance; //= .2f;

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
	public float stiffness; //= 10000;

	[Tooltip("Damping applied to the wheel. Lower values make the wheel bounce more.")]
	/// <summary>
	/// Damping applied to the wheel. 
	/// Lower values make the wheel bounce more.
	/// </summary>
	public float damping; //= 5000;

	[Tooltip("The rate (m/s) at which the spring relaxes to maximum length when the wheel is not on the ground.")]
	/// <summary>
	/// The rate (m/s) at which the spring relaxes to maximum length when the wheel is not on the ground.
	/// </summary>
	public float relaxRate; //= .5f;

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
	public float sidewaysGrip; //= 1;

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
	public float forwardGrip; //= 1;

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
	public float rollingFriction; //= .1f;

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



	/// <summary>
	/// The velocity of the wheel (at the raycast hit point) in world space
	/// </summary>
	public Vector3 Velocity;

	/// <summary>
	/// The angle by which the wheel is turning
	/// </summary>
	public float steerAngle;

	/// <summary>
	/// The torque applied to the wheel for moving in the forward and backward direction
	/// </summary>
	public float motorTorque;

	/// <summary>
	/// The torque the brake is applying on the wheel
	/// </summary>
	public float brakeTorque;

	/// <summary>
	///Revolutions per minute of the wheel
	/// </summary>
	public float RPM;

	/// <summary>
	/// Whether the wheel is touching the ground
	/// </summary>
	public bool isGrounded;

	/// <summary>
	/// The distance to which the spring of the wheel is compressed
	/// </summary>
	public float CompressionDistance;
	public float m_PrevCompressionDist;

	/// <summary>
	/// The ratio of compression distance and suspension distance
	/// 0 when the wheel is entirely uncompressed, 
	/// 1 when the wheel is entirely compressed
	/// </summary>
	public float CompressionRatio;


	/// <summary>
	/// The force the spring exerts on the rigidbody
	/// </summary>
	public Vector3 SpringForce;

	/// <summary>
	/// The raycast hit point of the wheel
	/// </summary>
	public RaycastHit Hit;
	//RaycastHit m_Hit;


	/// <summary>
	/// The layers used for wheel raycast
	/// </summary>
	public LayerMask m_RaycastLayers;
	
	public UnityEngine.Ray m_Ray;
	//new Rigidbody rigidbody;
	public float k_RayStartHeight;//= 1;
	public Entity root;
	public bool usePIDController;
	public float GetRayLen()
	{
		return suspensionDistance + radius + k_RayStartHeight;
	}
}

	[UpdateAfter(typeof(BuildPhysicsWorld)), UpdateBefore(typeof(StepPhysicsWorld))]
	public class WheelSystem : ComponentSystem
	{
		PhysicsWorld physicsWorld;
		protected override void OnUpdate()
		{
			physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld;
			World.GetOrCreateSystem<BuildPhysicsWorld>().FinalJobHandle.Complete();
			Entities.ForEach((Entity entity, ref WheelData wheel, ref LocalToWorld tr
				) =>
			{
				int rbId = physicsWorld.GetRigidBodyIndex(wheel.root);
				PIDControllerData pIDController = new PIDControllerData();
				if (EntityManager.HasComponent<PIDControllerData>(wheel.root))
				{
					 pIDController = EntityManager.GetComponentData<PIDControllerData>(wheel.root);
				}
				else
				{
					wheel.usePIDController = false;
				}

				FixedUpdate(ref wheel, tr, rbId, ref pIDController);
			});
		}

		void FixedUpdate(ref WheelData wheel, LocalToWorld tr, int rbId, ref PIDControllerData pIDController)
		{
			//transform.localEulerAngles = new Vector3(0, steerAngle, 0);
			CalculateSuspension(ref wheel, tr, rbId, ref pIDController);
			CalculateFriction(rbId, ref wheel, tr);
			CalculateRPM(ref wheel, rbId);
		}

		void CalculateRPM(ref WheelData wheel, int rbId)
		{
			//float metersPerMinute = rigidbody.velocity.magnitude * 60;
			float metersPerMinute = ((Vector3)physicsWorld.GetLinearVelocity(rbId)).magnitude * 60;

			float wheelCircumference = 2 * Mathf.PI * wheel.radius;
			wheel.RPM = metersPerMinute / wheelCircumference;
		}

		void CalculateSuspension(ref WheelData wheel, LocalToWorld transform, int rbId, ref PIDControllerData pIDController)
		{
			float rayLen = wheel.GetRayLen();
			wheel.m_Ray.direction = ((Vector3)(-transform.Up)).normalized;
			//wheel.m_Ray.origin = transform.Position + (transform.Up * wheel.k_RayStartHeight);
			wheel.m_Ray.origin = transform.Position + (transform.Up * wheel.k_RayStartHeight);

			//bool didHit = WheelRaycast(rayLen, ref wheel.Hit,ref wheel);
			bool didHit = WheelRaycast(rayLen, ref wheel, rbId);
			// If we did not hit, relax the spring and return
			if (!didHit)
			{
				wheel.m_PrevCompressionDist = wheel.CompressionDistance;
				wheel.CompressionDistance = wheel.CompressionDistance - Time.fixedDeltaTime * wheel.relaxRate;
				wheel.CompressionDistance = Mathf.Clamp(wheel.CompressionDistance, 0, wheel.suspensionDistance);

				wheel.isGrounded = false;
				return;
			}

			var force = 0.0f;
			wheel.isGrounded = true;
			float distance = Vector3.Distance(wheel.Hit.Position, transform.Position);
			wheel.CompressionDistance = rayLen - distance;
			wheel.CompressionDistance = Mathf.Clamp(wheel.CompressionDistance, 0, wheel.suspensionDistance);

			wheel.CompressionRatio = Mathf.Clamp01(wheel.CompressionDistance / wheel.suspensionDistance);
			

			// Calculate the force from the springs compression using Hooke's Law
			float springForce = wheel.stiffness * wheel.CompressionRatio;
			force += springForce;

			// Calculate the damping force based on compression rate of the spring
			float rate = (wheel.CompressionDistance - wheel.m_PrevCompressionDist) / Time.fixedDeltaTime;
			wheel.m_PrevCompressionDist = wheel.CompressionDistance;

			//stuff  For PIDCOntroller
			if (wheel.usePIDController)
			{
				wheel.CompressionRatio = pIDController.Seek(wheel.CompressionDistance, distance);
				rate = pIDController.Seek(wheel.m_PrevCompressionDist, wheel.CompressionDistance);
			}

			float damperForce = rate * wheel.damping;
			force += damperForce;

			// When normals are faked, the spring force vector is not applied towards the wheel's center,
			// instead it is resolved along the global Y axis and applied
			// This helps maintain velocity over speed bumps, however may be unrealistic
			if (wheel.fakeNormals)
				wheel.SpringForce = Vector3.up * force;
			else
			{
				float fakedScale = Vector3.Dot(wheel.Hit.SurfaceNormal, transform.Up);
				force *= fakedScale;
				wheel.SpringForce = transform.Up * force;
			}

			// Apply suspension force
			//rigidbody.AddForceAtPosition(SpringForce, (Hit.point));
			physicsWorld.ApplyImpulse(rbId, wheel.SpringForce, (wheel.Hit.Position));
		}

		bool WheelRaycast(float maxDistance, ref WheelData wheel, int rbId)
		{
			RaycastInput input = new RaycastInput
			{
				Start = wheel.m_Ray.origin,
				End = wheel.m_Ray.origin + wheel.m_Ray.direction * maxDistance,
				Filter = CollisionFilter.Default
			};
			bool validHit = false;
			Debug.DrawLine(wheel.m_Ray.origin, wheel.m_Ray.origin + wheel.m_Ray.direction * maxDistance);
			NativeList<RaycastHit> wheelhits = new NativeList<RaycastHit>(Allocator.TempJob);
			bool didHt = physicsWorld.CastRay(input, ref wheelhits);

			if (didHt)
			{
				for (int i = 0; i < wheelhits.Length; i++)
				{

					if (wheelhits[i].RigidBodyIndex != rbId)
					{
						wheel.Hit = wheelhits[i];
						validHit = true;
						break;
					}

				}
			}
			wheelhits.Dispose();
			return validHit;
		}

		void CalculateFriction(int rbId, ref WheelData wheel, LocalToWorld transform)
		{
			//wheel.Velocity = rigidbody.GetPointVelocity(Hit.point);
			wheel.Velocity = physicsWorld.GetLinearVelocity(1, wheel.Hit.Position);
			if (!wheel.isGrounded) return;

			// Contact basis (can be different from wheel basis)
			Vector3 normal = wheel.Hit.SurfaceNormal;
			Vector3 side = transform.Right;
			Vector3 forward = transform.Forward;

			// Apply less force if the vehicle is tilted
			var angle = Vector3.Angle(normal, transform.Up);
			var multiplier = Mathf.Cos(angle * Mathf.Deg2Rad);

			// Calculate sliding velocity (velocity without normal force)
			Vector3 sideVel = Vector3.Dot(wheel.Velocity, side) * side * multiplier;
			Vector3 forwardVel = Vector3.Dot(wheel.Velocity, forward) * forward * multiplier;
			Vector3 velocity2D = sideVel + forwardVel;

			float mass = physicsWorld.GetMass(rbId);
			Vector3 momentum = velocity2D * mass;

			var latForce = Vector3.Dot(-momentum, side) * side * wheel.sidewaysGrip;
			var longForce = Vector3.Dot(-momentum, forward) * forward * wheel.forwardGrip;
			Vector3 frictionForce = latForce + longForce;

			// Apply rolling friction
			longForce *= 1 - wheel.rollingFriction;

			// Apply brake force
			var brakeForceMag = wheel.brakeTorque / wheel.radius;
			brakeForceMag = Mathf.Clamp(brakeForceMag, 0, longForce.magnitude);
			longForce -= longForce.normalized * brakeForceMag;

			frictionForce -= longForce;
			//rigidbody.AddForceAtPosition(frictionForce, wheel.Hit.point);
			physicsWorld.ApplyImpulse(rbId, frictionForce, wheel.Hit.Position);
			Debug.DrawLine(wheel.Hit.Position, ((Vector3)wheel.Hit.Position + frictionForce), Color.blue);
			//rigidbody.AddForceAtPosition(forward * motorTorque / radius * forwardGrip, Hit.point);
			physicsWorld.ApplyImpulse(rbId, forward * wheel.motorTorque / wheel.radius * wheel.forwardGrip, wheel.Hit.Position);

		}


	}
}