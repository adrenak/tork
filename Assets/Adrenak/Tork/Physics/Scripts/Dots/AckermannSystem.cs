using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Animation;
using Adrenak.Tork;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

namespace Pragnesh.Dots
{

	public struct AckermannData : IComponentData
	{

		//[SerializeField] Wheel m_FrontRightTr;
		//[SerializeField] Wheel m_FrontLeftTr;
		//[SerializeField] Wheel m_RearRightTr;
		//[SerializeField] Wheel m_RearLeftTr;
		public float4 Radii { get; set; }

		public float Angle { get; set; }

		public float AxleSeparation;


		public float AxleWidth;

		public float FrontRightRadius;

		public float FrontLeftRadius;



		public static float GetSecondaryAngle(float angle, float width, float separation)
		{
			float close = separation / Mathf.Tan(Mathf.Abs(angle) * Mathf.Deg2Rad);
			float far = close + width;

			return Mathf.Atan(separation / far) * Mathf.Rad2Deg;
		}

		public static float4 GetRadii(float angle, float separation, float width)
		{
			var secAngle = GetSecondaryAngle(angle, width, separation);
			float4 radii = new float4();

			if (Mathf.Abs(angle) < 1)
				//radii[0, 0] = radii[1, 0] = radii[0, 1] = radii[1, 1] = 1000;
				radii.x = radii.y = radii.z = radii.w = 1000;
			if (angle < -1)
			{
				//radii[0, 0] = separation / Mathf.Sin(Mathf.Abs(angle * Mathf.Deg2Rad));
				//radii[0, 1] = separation / Mathf.Sin(Mathf.Abs(secAngle * Mathf.Deg2Rad));
				//radii[1, 0] = separation / Mathf.Tan(Mathf.Abs(angle * Mathf.Deg2Rad));
				//radii[1, 1] = separation / Mathf.Tan(Mathf.Abs(secAngle * Mathf.Deg2Rad));

				radii.x = separation / Mathf.Sin(Mathf.Abs(angle * Mathf.Deg2Rad));
				radii.y = separation / Mathf.Sin(Mathf.Abs(secAngle * Mathf.Deg2Rad));
				radii.z = separation / Mathf.Tan(Mathf.Abs(angle * Mathf.Deg2Rad));
				radii.w = separation / Mathf.Tan(Mathf.Abs(secAngle * Mathf.Deg2Rad));

			}
			else if (angle > 1)
			{
				//radii[0, 0] = separation / Mathf.Sin(Mathf.Abs(secAngle * Mathf.Deg2Rad));
				//radii[0, 1] = separation / Mathf.Sin(Mathf.Abs(angle * Mathf.Deg2Rad));
				//radii[1, 0] = separation / Mathf.Tan(Mathf.Abs(secAngle * Mathf.Deg2Rad));
				//radii[1, 1] = separation / Mathf.Tan(Mathf.Abs(angle * Mathf.Deg2Rad));
				radii.x = separation / Mathf.Sin(Mathf.Abs(secAngle * Mathf.Deg2Rad));
				radii.y = separation / Mathf.Sin(Mathf.Abs(angle * Mathf.Deg2Rad));
				radii.z = separation / Mathf.Tan(Mathf.Abs(secAngle * Mathf.Deg2Rad));
				radii.w = separation / Mathf.Tan(Mathf.Abs(angle * Mathf.Deg2Rad));
			}

			return radii;
		}

	}

	public class AckermannSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{

			Entities.ForEach((Entity enitity, ref AckermannData m_Ackermann, ref WheelsRef wheelsRefEntity) =>
			{

				var m_FrontLeftTr = EntityManager.GetComponentData<LocalToWorld>(wheelsRefEntity.m_FrontLeft);
				var m_FrontRightTr = EntityManager.GetComponentData<LocalToWorld>(wheelsRefEntity.m_FrontRight);

				var m_FrontLeftWheel = EntityManager.GetComponentData<WheelData>(wheelsRefEntity.m_FrontLeft);
				var m_FrontRightWheel = EntityManager.GetComponentData<WheelData>(wheelsRefEntity.m_FrontRight);

				var m_RearLeftTr = EntityManager.GetComponentData<LocalToWorld>(wheelsRefEntity.m_RearLeft);
				var m_RearRightTr = EntityManager.GetComponentData<LocalToWorld>(wheelsRefEntity.m_RearRight);

				ManualUpdate(ref m_Ackermann);

				m_Ackermann.AxleSeparation = ((Vector3)(m_FrontLeftTr.Position - m_RearLeftTr.Position)).magnitude;
				m_Ackermann.AxleWidth = ((Vector3)(m_FrontLeftTr.Position - m_FrontRightTr.Position)).magnitude;
				m_Ackermann.FrontRightRadius = m_Ackermann.AxleSeparation / Mathf.Sin(Mathf.Abs(m_FrontRightWheel.steerAngle));
				m_Ackermann.FrontLeftRadius = m_Ackermann.AxleSeparation / Mathf.Sin(Mathf.Abs(m_FrontLeftWheel.steerAngle));


			});

		}





		private void ManualUpdate(ref AckermannData ackermann)
		{
			ackermann.Radii = AckermannData.GetRadii(ackermann.Angle, ackermann.AxleSeparation, ackermann.AxleWidth);
		}

	}
}



