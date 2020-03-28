using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Animation;

namespace Pragnesh.Dots
{
	public class CenterOfMassAssignerDots : MonoBehaviour,IConvertGameObjectToEntity {
		
		public Transform m_Point;




#if UNITY_EDITOR
		private void OnDrawGizmos() {
			Gizmos.color = Color.red;

			if(m_Point != null)
				Gizmos.DrawSphere(m_Point.position, .1f);
		}
#endif

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			if (!dstManager.HasComponent<PhysicsMass>(entity))
			{
				dstManager.AddComponent<PhysicsMass>(entity);
			}
				PhysicsMass mass = dstManager.GetComponentData<PhysicsMass>(entity);
				mass.CenterOfMass = m_Point.localPosition;
				dstManager.SetComponentData(entity, mass);
		}
	}
}
