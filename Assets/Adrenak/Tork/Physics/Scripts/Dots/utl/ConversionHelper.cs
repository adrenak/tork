using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Adrenak.Tork;
using Unity.Animation.Hybrid;

namespace Pragnesh.Dots
{
	[DisallowMultipleComponent]
	[RequiresEntityConversion]
	public class ConversionHelper : MonoBehaviour, IConvertGameObjectToEntity
	{
		public Wheel[] wheels;
		public WheelRenderer[] wheelRenderers;
		public bool usePIDController;
		public PIDController pidController;

		Ackermann ackermann;
		 Vehicle vehicle;
		 Brakes brakes;
		 AntiRoll antiRoll;
		 Aerodynamics aerodynamics;
		 Motor motor;
		 Steering steering;

		void GetComponents()
		{
			TryGetComponent(out ackermann);
			TryGetComponent(out vehicle);
			TryGetComponent(out brakes);
			TryGetComponent(out antiRoll);
			TryGetComponent(out aerodynamics);
			TryGetComponent(out motor);
			TryGetComponent(out steering);
			if (pidController == null)
			{
				TryGetComponent(out pidController);
			}
		}

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			GetComponents();
			#region Wheel & Wheel Rendere Conversion
			Entity[] wheelsEntitys = new Entity[wheels.Length];
			for (int i = 0; i < wheels.Length; i++)
			{
				Entity wheel = conversionSystem.GetPrimaryEntity(wheels[i].transform);
				wheelsEntitys[i] = wheel;
				dstManager.AddComponentData(wheel, new WheelData
				{

					radius = wheels[i].radius,
					suspensionDistance = wheels[i].suspensionDistance,
					stiffness = wheels[i].stiffness,
					damping = wheels[i].damping,
					relaxRate = wheels[i].relaxRate,
					sidewaysGrip = wheels[i].sidewaysGrip,
					forwardGrip = wheels[i].forwardGrip,
					rollingFriction = wheels[i].rollingFriction,
					fakeNormals = wheels[i].fakeNormals,
					m_RaycastLayers = wheels[i].m_RaycastLayers,
					Velocity = wheels[i].Velocity,
					steerAngle = wheels[i].steerAngle,
					motorTorque = wheels[i].motorTorque,
					brakeTorque = wheels[i].brakeTorque,
					RPM = wheels[i].RPM,
					isGrounded = wheels[i].isGrounded,
					CompressionDistance = wheels[i].CompressionDistance,
					//m_PrevCompressionDist = wheels[i].m_PrevCompressionDist,
					CompressionRatio = wheels[i].CompressionRatio,
					//Hit = wheels[i].hit,
					SpringForce = wheels[i].SpringForce,
					//m_Ray = wheels[i].m_Ray,
					k_RayStartHeight = wheels[i].k_RayStartHeight,
					root = entity,
					usePIDController=usePIDController,
				});

			}

			for (int i = 0; i < wheelRenderers.Length; i++)
			{
				Entity wheelRenderer = conversionSystem.GetPrimaryEntity(wheelRenderers[i].transform);
				Entity wheel = conversionSystem.GetPrimaryEntity(wheelRenderers[i].wheel.transform);

				dstManager.AddComponentData(wheelRenderer, new WheelRendereData
				{
					wheel = wheel,
					offset = wheelRenderers[i].offset,
					angle = wheelRenderers[i].angle,
				});

			}

			#endregion

			#region Vehcle MainScripts Conversion

			if (ackermann != null)
			{
				dstManager.AddComponentData(entity, new AckermannData
				{
					Angle = ackermann.Angle,
					AxleSeparation = ackermann.AxleSeparation,
					AxleWidth = ackermann.AxleWidth,
					FrontRightRadius = ackermann.FrontRightRadius,
					FrontLeftRadius = ackermann.FrontLeftRadius,
				});
			}

			//aerodynamicsscript conversion
			if (aerodynamics != null)
			{
				dstManager.AddComponentData(entity, new AerodynamicsData
				{
					downForce = aerodynamics.downForce,
					stabilizationTorque = aerodynamics.stabilizationTorque,
					midAirSteerTorque = aerodynamics.midAirSteerTorque,
					midAirSteerInput = aerodynamics.midAirSteerInput,
					doApplyDownForce = aerodynamics.doApplyDownForce,
					doStabilize = aerodynamics.doStabilize,
					doSteerMidAir = aerodynamics.doSteerMidAir,
				});
			}
			
			//AntiRoll ConversionSystem
			if (antiRoll != null)
			{
				DynamicBuffer<AxleWithEntity> vehicleParentArrays = dstManager.AddBuffer<AxleWithEntity>(entity);
				foreach (AntiRoll.Axle axle in antiRoll.axles)
				{
					Entity leftEntity = conversionSystem.GetPrimaryEntity(axle.left.transform);
					Entity rightEntity = conversionSystem.GetPrimaryEntity(axle.right.transform);
					vehicleParentArrays.Add(new AxleWithEntity
					{
						force = axle.force,
						left = leftEntity,
						right = rightEntity,
					});
				}
			}
			//Brakes conversionSystem
			if (brakes != null)
			{
				dstManager.AddComponentData(entity, new BrakesData
				{

					maxTorque = brakes.maxTorque,
					value = brakes.value,
					isAckermann = true,

				});
			}

			//Motor conversion
			if (motor != null)
			{
				dstManager.AddComponentData(entity, new MotorData
				{
					maxTorque = motor.maxTorque,
					value = motor.value,
					m_MaxReverseInput = motor.m_MaxReverseInput,
				});

				Entity FL = conversionSystem.GetPrimaryEntity(motor.m_FrontLeft.transform);
				Entity FR = conversionSystem.GetPrimaryEntity(motor.m_FrontRight.transform);
				Entity RL = conversionSystem.GetPrimaryEntity(motor.m_RearLeft.transform);
				Entity RR = conversionSystem.GetPrimaryEntity(motor.m_RearRight.transform);
				dstManager.AddComponentData(entity, new WheelsRef
				{
					m_FrontLeft = FL,
					m_FrontRight = FR,
					m_RearRight = RR,
					m_RearLeft = RL,
				});
			}

			//Steering conversion
			if (steering != null)
			{
				dstManager.AddComponentData(entity, new SteeringData
				{

					range = steering.range,
					value = steering.value,
					rate = steering.rate,
				});
			}
			//Vehicle conversion 
			if (vehicle != null)
			{
				dstManager.AddComponentData(entity, new VehicleData
				{
					m_MotorTorqueVsSpeed = vehicle.m_MotorTorqueVsSpeed.ToAnimationCurveBlobAssetRef(),
					m_MaxSteeringAngleVsSpeed = vehicle.m_MaxSteeringAngleVsSpeed.ToAnimationCurveBlobAssetRef(),
					m_DownForceVsSpeed = vehicle.m_DownForceVsSpeed.ToAnimationCurveBlobAssetRef(),

				});
				dstManager.AddComponent<VehicleInputData>(entity);

				
			}
			if (pidController != null)
			{
				//PIDCOntroller for Smooth Suspension
				dstManager.AddComponentData(entity, new PIDControllerData
				{
					pCoeff = pidController.pCoeff,
					iCoeff = pidController.iCoeff,
					dCoeff = pidController.dCoeff,
					minimum = pidController.minimum,
					maximum = pidController.maximum,
				});
			}
			#endregion
		}
	}

	public struct WheelRendereData : IComponentData
	{
		public Entity wheel;
		public float offset;
		public float angle;
	}
}