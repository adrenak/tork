using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Animation;
using Unity.Animation.Hybrid;
using AnimationCurve = UnityEngine.AnimationCurve;
using Pragnesh.Dots;
namespace Pragnesh.Dots
{
	
	public struct VehicleData : IComponentData
	{
		//public Vector3 Velocity;

		[Tooltip("The maximum motor torque available based on the speed (KMPH)")]
		public BlobAssetReference<AnimationCurveBlob> m_MotorTorqueVsSpeed;//= AnimationCurve.Linear(0, 10000, 250, 0);

		[Tooltip("The steering angle based on the speed (KMPH)")]
		public BlobAssetReference<AnimationCurveBlob> m_MaxSteeringAngleVsSpeed;///= AnimationCurve.Linear(0, 35, 250, 5);

		[Tooltip("The down force based on the speed (KMPH)")]
		public BlobAssetReference<AnimationCurveBlob> m_DownForceVsSpeed;//= AnimationCurve.Linear(0, 0, 250, 2500);

		//[SerializeField] PlayerData m_Player;

		//Rigidbody m_Rigidbody;
		//Steering m_Steering;
		//Motor m_Motor;
		//Brakes m_Brake;
		//Aerodynamics m_Aerodynamics;

		public float GetMotorTorqueAtSpeed(float speed)
		{
			return AnimationCurveEvaluator.Evaluate(speed, m_MotorTorqueVsSpeed);
		}

		public float GetMaxSteerAtSpeed(float speed)
		{
			return AnimationCurveEvaluator.Evaluate(speed, m_MaxSteeringAngleVsSpeed);
		}

		public float GetDownForceAtSpeed(float speed)
		{
			return AnimationCurveEvaluator.Evaluate(speed, m_DownForceVsSpeed);
		}

	}

	public class VehicleSystem : ComponentSystem
	{
		//PhysicsWorld physicsWorld;
		protected override void OnUpdate()
		{
			Entities.ForEach((Entity entity, ref VehicleData vehicleData, ref SteeringData m_Steering,
				ref MotorData m_Motor, ref PhysicsVelocity physicsVelocity,

				ref BrakesData m_Brake, ref AerodynamicsData m_Aerodynamics) =>
			{

				VehicleInputData inputData = EntityManager.GetComponentData<VehicleInputData>(entity);

				//var speed = m_Rigidbody.velocity.magnitude * 3.6F;
				var speed = ((Vector3)physicsVelocity.Linear).magnitude * 3.6F;

				m_Steering.range = vehicleData.GetMaxSteerAtSpeed(speed);
				m_Motor.maxTorque = vehicleData.GetMotorTorqueAtSpeed(speed);
				m_Aerodynamics.downForce = vehicleData.GetDownForceAtSpeed(speed);

				//var input = m_Player.GetInput();

				m_Steering.value = inputData.steering;
				m_Motor.value = inputData.acceleration;
				m_Brake.value = inputData.brake;
				m_Aerodynamics.midAirSteerInput = inputData.steering;

			});


		}

		//void Update(VehicleData vehicleData,int rbId,ref)
		//{
		//	//if (m_Player == null) return;

		//	var speed = m_Rigidbody.velocity.magnitude * 3.6F;

		//	m_Steering.range = vehicleData.GetMaxSteerAtSpeed(speed);
		//	m_Motor.maxTorque = vehicleData.GetMotorTorqueAtSpeed(speed);
		//	m_Aerodynamics.downForce = vehicleData.GetDownForceAtSpeed(speed);

		//	var input = m_Player.GetInput();

		//	m_Steering.value = input.steering;
		//	m_Motor.value = input.acceleration;
		//	m_Brake.value = input.brake;
		//	m_Aerodynamics.midAirSteerInput = input.steering;
		//}


	}
}

