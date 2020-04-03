using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// This Scripts Need Lot improvement
/// </summary>
namespace Pragnesh.Dots
{
	public class WheelRendererSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((ref WheelRendereData rendereData, ref LocalToWorld tr,
				ref Translation translation, ref Rotation rt) =>
			{
				var wheelData = EntityManager.GetComponentData<WheelData>(rendereData.wheel);
				var wheelTr = EntityManager.GetComponentData<LocalToWorld>(rendereData.wheel);
				SyncPosition(ref rendereData, wheelData, wheelTr, ref translation, ref tr);
				SyncRotation(ref rt, ref rendereData, tr, wheelData);
			});
		}

		void SyncPosition(ref WheelRendereData rendereData, WheelData wheel, LocalToWorld wheelTr, ref Translation translation, ref LocalToWorld tr)
		{

			//transform.position = new Vector3(
			//	wheel.transform.position.x,
			//	wheel.transform.position.y,
			//	wheel.transform.position.z
			//);
			//tr.Value = wheelTr.Position;		
			tr.Value = wheelTr.Value;

			//transform.localPosition = new Vector3(
			//	transform.localPosition.x + offset,
			//	transform.localPosition.y - (wheel.suspensionDistance - wheel.CompressionDistance),
			//	transform.localPosition.z
			//);
			//translation.Value = new Vector3(
			//	translation.Value.x + rendereData.offset,
			//	translation.Value.y - (wheel.suspensionDistance - wheel.CompressionDistance),
			//	translation.Value.z
			//);

		}

		void SyncRotation(ref Rotation rt, ref WheelRendereData rendereData, LocalToWorld tr, WheelData wheel)
		{
			//transform.localEulerAngles = Vector3.zero;
			rt.Value = quaternion.Euler(0, 0, 0);
			//rendereData.angle += (Time.deltaTime * transform.InverseTransformDirection(wheel.Velocity).z) / (2 * Mathf.PI * wheel.radius) * 360;
			rendereData.angle += (Time.DeltaTime * (Quaternion.Inverse(tr.Rotation) * (wheel.Velocity)).z) / (2 * Mathf.PI * wheel.radius) * 360;
			//transform.Rotate(new Vector3(0, 1, 0), wheel.steerAngle - transform.localEulerAngles.y);
			rt.Value = quaternion.EulerXYZ(rendereData.angle, (wheel.steerAngle - rt.Value.value.y) * Time.DeltaTime, 0);
			//rt.Value = quaternion.AxisAngle(new Vector3(0, 1, 0), wheel.steerAngle - rt.Value.value.y);
			//transform.Rotate(new Vector3(1, 0, 0), angle);
			//rt.Value = quaternion.RotateX(rendereData.angle);
		}
	}
}

