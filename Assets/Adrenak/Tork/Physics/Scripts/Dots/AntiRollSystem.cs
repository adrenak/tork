using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Animation;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Pragnesh.Dots
{

	public struct AxleData
	{
		public WheelData left;
		public WheelData right;
		public float force;
	}
	public struct AxleWithEntity : IBufferElementData
	{
		public Entity left;
		public Entity right;
		public float force;
	}
	[UpdateAfter(typeof(BuildPhysicsWorld)), UpdateBefore(typeof(StepPhysicsWorld))]
	public class AntiRollSystem : ComponentSystem
	{
		PhysicsWorld physicsWorld;
		protected override void OnUpdate()
		{
			BufferFromEntity<AxleWithEntity> bufferFromEntity = GetBufferFromEntity<AxleWithEntity>(true);
			physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld;
			World.GetOrCreateSystem<BuildPhysicsWorld>().FinalJobHandle.Complete();

			Entities.WithAll(typeof(AxleWithEntity)).ForEach((Entity entity, ref LocalToWorld tr) =>
			{
				DynamicBuffer<AxleWithEntity> axlesBuffer = bufferFromEntity[entity];
				AxleData[] axles = new AxleData[axlesBuffer.Length];
				for (int i = 0; i < axlesBuffer.Length; i++)
				{
					axles[i] = new AxleData
					{
						left = EntityManager.GetComponentData<WheelData>(axlesBuffer[i].left),
						right = EntityManager.GetComponentData<WheelData>(axlesBuffer[i].right),
						force = (axlesBuffer[i].force),
					};
				}
				int rbId = physicsWorld.GetRigidBodyIndex(entity);
				FixedUpdate(rbId, tr, axles);
			});
		}

		void FixedUpdate(int rbId, LocalToWorld tr, AxleData[] axles)
		{
			foreach (var axle in axles)
			{
				//var wsDown = transform.TransformDirection(Vector3.down);
				var wsDown = ((Quaternion)tr.Rotation) * (Vector3.down);
				wsDown.Normalize();

				float travelL = Mathf.Clamp01(axle.left.CompressionRatio);
				float travelR = Mathf.Clamp01(axle.right.CompressionRatio);
				float antiRollForce = (travelL - travelR) * axle.force;

				if (axle.left.isGrounded)
					//rigidbody.AddForceAtPosition(wsDown * -antiRollForce, axle.left.Hit.point);
					physicsWorld.ApplyImpulse(rbId, wsDown * -antiRollForce, axle.left.Hit.Position);

				if (axle.right.isGrounded)
					//rigidbody.AddForceAtPosition(wsDown * antiRollForce, axle.right.Hit.point);
					physicsWorld.ApplyImpulse(rbId, wsDown * antiRollForce, axle.right.Hit.Position);
			}
		}

	}
}
