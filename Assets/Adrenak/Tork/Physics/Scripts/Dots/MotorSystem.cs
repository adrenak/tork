using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Animation;

namespace Pragnesh.Dots
{

	public struct MotorData : IComponentData
	{
		public float maxTorque;//= 20000;
		public float value;
		public float m_MaxReverseInput;//= -.5f;
	}
	public struct WheelsRef : IComponentData
	{
		public Entity m_FrontRight;
		public Entity m_FrontLeft;
		public Entity m_RearRight;
		public Entity m_RearLeft;
	}

	public struct WheelList
	{
		public WheelData m_FrontRight;
		public WheelData m_FrontLeft;
		public WheelData m_RearRight;
		public WheelData m_RearLeft;
	}


	public class MotorSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((Entity enitity, ref MotorData motor, ref AckermannData m_Ackermann, ref WheelsRef wheelsRefEntity) =>
			{
				WheelList wheelList = new WheelList
				{
					m_FrontLeft = EntityManager.GetComponentData<WheelData>(wheelsRefEntity.m_FrontLeft),
					m_FrontRight = EntityManager.GetComponentData<WheelData>(wheelsRefEntity.m_FrontRight),
					m_RearLeft = EntityManager.GetComponentData<WheelData>(wheelsRefEntity.m_RearLeft),
					m_RearRight = EntityManager.GetComponentData<WheelData>(wheelsRefEntity.m_RearRight)
				};

				ApplyMotorTorque(ref motor, ref m_Ackermann, ref wheelList);

				EntityManager.SetComponentData(wheelsRefEntity.m_FrontLeft, wheelList.m_FrontLeft);
				EntityManager.SetComponentData(wheelsRefEntity.m_FrontRight, wheelList.m_FrontRight);
				EntityManager.SetComponentData(wheelsRefEntity.m_RearLeft, wheelList.m_RearLeft);
				EntityManager.SetComponentData(wheelsRefEntity.m_RearRight, wheelList.m_RearRight);

			});
		}

		void ApplyMotorTorque(ref MotorData motor, ref AckermannData m_Ackermann, ref WheelList wheelList)
		{
			motor.value = Mathf.Clamp(motor.value, motor.m_MaxReverseInput, 1);
			float fr, fl, rr, rl;

			// If we have Ackerman steering, we apply torque based on the steering radius of each wheel
			var radii = AckermannData.GetRadii(m_Ackermann.Angle, m_Ackermann.AxleSeparation, m_Ackermann.AxleWidth);
			//var total = radii[0, 0] + radii[1, 0] + radii[0, 1] + radii[1, 1];
			var total = radii.x + radii.y + radii.z + radii.w;
			fl = radii.x / total;
			fr = radii.y / total;
			rl = radii.z / total;
			rr = radii.w / total;

			wheelList.m_FrontLeft.motorTorque = motor.value * motor.maxTorque * fl;
			wheelList.m_FrontRight.motorTorque = motor.value * motor.maxTorque * fr;

			wheelList.m_RearLeft.motorTorque = motor.value * motor.maxTorque * rl;
			wheelList.m_RearRight.motorTorque = motor.value * motor.maxTorque * rr;
		}

	}
}
