using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Animation;
using Adrenak.Tork;

namespace Pragnesh.Dots
{

	public struct SteeringData : IComponentData
	{
		public float range;//= 35;
		public float value;
		public float rate;//= 45;
		public float m_CurrAngle;

	}

	public class SteeringSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((ref SteeringData st, ref AckermannData ackermann, ref WheelsRef wheelsRefEntity) =>
			{
				WheelList wheelList = new WheelList
				{
					m_FrontLeft = EntityManager.GetComponentData<WheelData>(wheelsRefEntity.m_FrontLeft),
					m_FrontRight = EntityManager.GetComponentData<WheelData>(wheelsRefEntity.m_FrontRight),
					m_RearLeft = EntityManager.GetComponentData<WheelData>(wheelsRefEntity.m_RearLeft),
					m_RearRight = EntityManager.GetComponentData<WheelData>(wheelsRefEntity.m_RearRight)
				};

				Update(ref st, ref ackermann, ref wheelList);

				EntityManager.SetComponentData(wheelsRefEntity.m_FrontLeft, wheelList.m_FrontLeft);
				EntityManager.SetComponentData(wheelsRefEntity.m_FrontRight, wheelList.m_FrontRight);
				EntityManager.SetComponentData(wheelsRefEntity.m_RearLeft, wheelList.m_RearLeft);
				EntityManager.SetComponentData(wheelsRefEntity.m_RearRight, wheelList.m_RearRight);
			});
		}

		private void Update(ref SteeringData st, ref AckermannData ackermann, ref WheelList wheelList)
		{
			var destination = st.value * st.range;

			st.m_CurrAngle = Mathf.MoveTowards(st.m_CurrAngle, destination, Time.DeltaTime * st.rate);
			st.m_CurrAngle = Mathf.Clamp(st.m_CurrAngle, -st.range, st.range);
			//Ackermann.SetAngle(st.m_CurrAngle);
			SetAngle(st.m_CurrAngle, ref ackermann, ref wheelList);


		}

		void SetAngle(float angle, ref AckermannData ackermann, ref WheelList wheelList)
		{
			ackermann.Angle = angle;
			var farAngle = AckermannData.GetSecondaryAngle(angle, ackermann.AxleWidth, ackermann.AxleSeparation);

			// The rear wheels are always at 0 steer in Ackermann
			wheelList.m_RearLeft.steerAngle = wheelList.m_RearRight.steerAngle = 0;

			if (Mathf.Approximately(ackermann.Angle, 0))
				wheelList.m_FrontRight.steerAngle = wheelList.m_FrontLeft.steerAngle = 0;
			else if (ackermann.Angle > 0)
			{
				wheelList.m_FrontRight.steerAngle = ackermann.Angle;
				wheelList.m_FrontLeft.steerAngle = farAngle;
			}
			else if (ackermann.Angle < 0)
			{
				wheelList.m_FrontLeft.steerAngle = ackermann.Angle;
				wheelList.m_FrontRight.steerAngle = -farAngle;
			}
		}

	}
}
