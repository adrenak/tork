using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Pragnesh.Dots
{

    public class SmoothFollowObject : MonoBehaviour, IConvertGameObjectToEntity
    {
        public GameObject _target;
        // The distance in the x-z plane to the target
        public float _distance;//= 10.0f;
                               // the height we want the camera to be above the target
        public float _height;//= 5.0f;

        public float _rotationDamping;
        public float _heightDamping;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new SmoothFollowData
            {
                target = conversionSystem.GetPrimaryEntity(_target),

                distance = _distance,
                height = _height,
                rotationDamping = _rotationDamping,
                heightDamping = _heightDamping,
            });
        }
    }


    public class SmoothFollowSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            float DeltaTime = Time.DeltaTime;
            Entities.ForEach((Transform transform, ref SmoothFollowData smoothFollow) =>
            {

                LocalToWorld target = EntityManager.GetComponentData<LocalToWorld>(smoothFollow.target);
                var wantedRotationAngle = target.Rotation.value.y;
                var wantedHeight = target.Position.y + smoothFollow.height;

                var currentRotationAngle = transform.eulerAngles.y;
                var currentHeight = transform.position.y;

            // Damp the rotation around the y-axis
            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, smoothFollow.rotationDamping * Time.DeltaTime);

            // Damp the height
            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, smoothFollow.heightDamping * Time.DeltaTime);

            // Convert the angle into a rotation
            var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            // Set the position of the camera on the x-z plane to:
            // distance meters behind the target
            transform.position = target.Position;
                transform.position -= currentRotation * Vector3.forward * smoothFollow.distance;

            // Set the height of the camera
            transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

            // Always look at the target
            transform.LookAt(target.Position);
            });
        }


    }


    public struct SmoothFollowData : IComponentData
    {
        // The target we are following
        public Entity target;
        // The distance in the x-z plane to the target
        public float distance;//= 10.0f;
                              // the height we want the camera to be above the target
        public float height;//= 5.0f;

        public float rotationDamping;
        public float heightDamping;
    }
}