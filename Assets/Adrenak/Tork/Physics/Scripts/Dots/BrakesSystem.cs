using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Animation;

namespace Pragnesh.Dots
{
	public struct BrakesData : IComponentData
	{
		[Tooltip("The maximum braking torque that can be applied")]
		public float maxTorque;//= 5000;

		[Tooltip("Multiplier to the maxTorque")]
		public float value;
		public bool isAckermann;
	}

	[UpdateAfter(typeof(BuildPhysicsWorld)), UpdateBefore(typeof(StepPhysicsWorld))]
	public class BrakesSystem : ComponentSystem
	{

		protected override void OnUpdate()
		{
			World.GetOrCreateSystem<BuildPhysicsWorld>().FinalJobHandle.Complete();
			Entities.ForEach((ref BrakesData brake, ref AckermannData ackermann, ref WheelsRef wheelsRefEntity) =>
			{
				WheelList wheelList = new WheelList
				{
					m_FrontLeft = EntityManager.GetComponentData<WheelData>(wheelsRefEntity.m_FrontLeft),
					m_FrontRight = EntityManager.GetComponentData<WheelData>(wheelsRefEntity.m_FrontRight),
					m_RearLeft = EntityManager.GetComponentData<WheelData>(wheelsRefEntity.m_RearLeft),
					m_RearRight = EntityManager.GetComponentData<WheelData>(wheelsRefEntity.m_RearRight)
				};
				FixedUpdate(ref brake, ref ackermann, ref wheelList);

				EntityManager.SetComponentData(wheelsRefEntity.m_FrontLeft, wheelList.m_FrontLeft);
				EntityManager.SetComponentData(wheelsRefEntity.m_FrontRight, wheelList.m_FrontRight);
				EntityManager.SetComponentData(wheelsRefEntity.m_RearLeft, wheelList.m_RearLeft);
				EntityManager.SetComponentData(wheelsRefEntity.m_RearRight, wheelList.m_RearRight);
			});
		}

		private void FixedUpdate(ref BrakesData brakes, ref AckermannData m_Ackermann, ref WheelList wheelList)
		{
			float fr, fl, rr, rl;

			// If we have Ackerman steering, we apply torque based on the steering radius of each wheel
			if (brakes.isAckermann)
			{
				var radii = AckermannData.GetRadii(m_Ackermann.Angle, m_Ackermann.AxleSeparation, m_Ackermann.AxleWidth);
				//var total = radii[0, 0] + radii[1, 0] + radii[0, 1] + radii[1, 1];
				//fl = radii[0, 0] / total;
				//fr = radii[1, 0] / total;
				//rl = radii[0, 1] / total;
				//rr = radii[1, 1] / total;
				var total = radii.x + radii.y + radii.z + radii.w;
				fl = radii.x / total;
				fr = radii.y / total;
				rl = radii.z / total;
				rr = radii.w / total;
			}
			else
				fr = fl = rr = rl = 1;

			wheelList.m_FrontLeft.brakeTorque = brakes.value * brakes.maxTorque * fl;
			wheelList.m_FrontRight.brakeTorque = brakes.value * brakes.maxTorque * fr;

			wheelList.m_RearLeft.brakeTorque = brakes.value * brakes.maxTorque * rl;
			wheelList.m_RearRight.brakeTorque = brakes.value * brakes.maxTorque * rr;
		}

	}
}