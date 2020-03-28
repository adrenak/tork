using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Animation;
using RaycastHit = Unity.Physics.RaycastHit;
using System.Collections.Generic;

namespace Pragnesh.Dots
{

	public struct AerodynamicsData : IComponentData
	{
		//[SerializeField] Rigidbody m_Rigidbody;
		//[SerializeField] Wheel[] m_Wheels;

		public float downForce; //= 1000;
		public float stabilizationTorque; //= 15000;
		public float midAirSteerTorque; //= 1500;
		public float midAirSteerInput;

		public bool doApplyDownForce;
		public bool doStabilize;
		public bool doSteerMidAir;
	}

	[UpdateAfter(typeof(BuildPhysicsWorld)), UpdateAfter(typeof(WheelSystem)), UpdateBefore(typeof(StepPhysicsWorld))]
	public class AerodynamicsSystem : ComponentSystem
	{
		PhysicsWorld physicsWorld;
		protected override void OnUpdate()
		{
			physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld;
			World.GetOrCreateSystem<BuildPhysicsWorld>().FinalJobHandle.Complete();

			Entities.ForEach((Entity entity, ref AerodynamicsData aerodynamics, ref LocalToWorld tr, ref WheelsRef wheelsRef) =>
			{
				WheelData[] wheelDatas = new WheelData[4];
				wheelDatas[0] = EntityManager.GetComponentData<WheelData>(wheelsRef.m_FrontLeft);
				wheelDatas[1] = EntityManager.GetComponentData<WheelData>(wheelsRef.m_FrontRight);
				wheelDatas[2] = EntityManager.GetComponentData<WheelData>(wheelsRef.m_RearLeft);
				wheelDatas[3] = EntityManager.GetComponentData<WheelData>(wheelsRef.m_RearRight);
				int rbId = physicsWorld.GetRigidBodyIndex(entity);
				FixedUpdate(ref aerodynamics, rbId, tr, wheelDatas);

			});
		}

		void FixedUpdate(ref AerodynamicsData aerodynamics, int rbId, LocalToWorld tr, WheelData[] m_Wheels)
		{
			if (aerodynamics.doApplyDownForce)
				DoApplyDownForce(ref aerodynamics, rbId);

			if (aerodynamics.doStabilize)
				Stabilize(ref aerodynamics, tr, m_Wheels, rbId);

			if (aerodynamics.doSteerMidAir)
				SteerMidAir(ref aerodynamics, rbId);
		}

		void DoApplyDownForce(ref AerodynamicsData aerodynamics, int rbId)
		{
			if (aerodynamics.downForce != 0)
				//m_Rigidbody.AddForce(-Vector3.up * aerodynamics.downForce);
				//physicsWorld.ApplyAngularImpulse(rbId, (-Vector3.up * aerodynamics.downForce));
				physicsWorld.ApplyLinearImpulse(rbId, (-Vector3.up * aerodynamics.downForce));
		}

		void Stabilize(ref AerodynamicsData aerodynamics, LocalToWorld tr, WheelData[] m_Wheels, int rbId)
		{
			var inAir = m_Wheels.Where(x => !x.isGrounded);
			if (inAir.Count() != 4) return;

			// Try to keep vehicle parallel to the ground while jumping
			Vector3 locUp = tr.Up;
			Vector3 wsUp = new Vector3(0.0f, 1.0f, 0.0f);
			Vector3 axis = Vector3.Cross(locUp, wsUp);
			float force = aerodynamics.stabilizationTorque;

			//m_Rigidbody.AddTorque(axis * force);
			physicsWorld.ApplyAngularImpulse(rbId, axis * force);
		}

		void SteerMidAir(ref AerodynamicsData aerodynamics, int rbId)
		{
			if (!Mathf.Approximately(aerodynamics.midAirSteerInput, 0))
				//m_Rigidbody.AddTorque(new Vector3(0, aerodynamics.midAirSteerInput * aerodynamics.midAirSteerTorque, 0));
				physicsWorld.ApplyAngularImpulse(rbId, new Vector3(0, aerodynamics.midAirSteerInput * aerodynamics.midAirSteerTorque, 0));
		}
	}
}