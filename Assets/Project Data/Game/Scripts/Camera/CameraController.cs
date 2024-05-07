using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon
{
    [DefaultExecutionOrder(100)]
    public class CameraController : MonoBehaviour
    {
        const int ACTIVE_CAMERA_PRIORITY = 100;
        const int UNACTIVE_CAMERA_PRIORITY = 0;

        static CameraController cameraController;

        [SerializeField] CinemachineBrain cameraBrain;
        [SerializeField] CameraType firstCamera;

        [Space]
        [SerializeField] VirtualCameraCase[] virtualCameras;

        [Header("Forward Shift")]
        [SerializeField] float forwardX = 4f;
        [SerializeField] float forwardZ = 1f;
        [SerializeField] float forwardLerpMultiplier = 4f;

        [Header("Enemy Target Shift")]
        [SerializeField] float enemyShiftX = 4f;
        [SerializeField] float enemyShiftZ = 1f;
        [SerializeField] float enemyShiftLerpMultiplier = 4f;

        static Dictionary<CameraType, int> virtualCamerasLink;

        static Camera mainCamera;
        public static Camera MainCamera => mainCamera;

        static Transform mainTarget;
        public static Transform MainTarget => mainTarget;

        static VirtualCameraCase activeVirtualCamera;
        public static VirtualCameraCase ActiveVirtualCamera => activeVirtualCamera;

        static Transform InternalTarget { get; set; }

        static bool cameraShiftEnabled = true;

        Vector3 forward = Vector3.zero;
        static Vector3 enemyDirection = Vector3.zero;
        static BaseEnemyBehavior targetEnemy;

        void Awake()
        {
            cameraController = this;

            // Get camera component
            mainCamera = GetComponent<Camera>();

            // Initialise cameras link
            virtualCamerasLink = new Dictionary<CameraType, int>();
            for(var i = 0; i < virtualCameras.Length; i++)
            {
                virtualCameras[i].Initialise();
                virtualCamerasLink.Add(virtualCameras[i].CameraType, i);
            }

            // Disable camera brain
            cameraController.cameraBrain.enabled = false;

            EnableCamera(firstCamera);

            InternalTarget = new GameObject("[Internal Camera Target]").transform;

            mainTarget = InternalTarget;
        }

        public static void SetMainTarget(Transform target)
        {
            // Link target
            mainTarget = target;
            InternalTarget.position = mainTarget.position;

            cameraController.cameraBrain.enabled = false;

            for (var i = 0; i < cameraController.virtualCameras.Length; i++)
            {
                cameraController.virtualCameras[i].VirtualCamera.Follow = InternalTarget;
                cameraController.virtualCameras[i].VirtualCamera.LookAt = InternalTarget;
            }

            cameraController.cameraBrain.transform.position = target.position;
            cameraController.cameraBrain.enabled = true;
        }

        public static void SetEnemyTarget(BaseEnemyBehavior enemy)
        {
            targetEnemy = enemy;
        }

        public static void SetCameraShiftState(bool state)
        {
            cameraShiftEnabled = state;
        }

        void LateUpdate()
        {
            if (cameraShiftEnabled)
            {
                var z = mainTarget.forward.z * forwardZ;
                var x = mainTarget.forward.x * forwardX;

                forward = Vector3.Lerp(forward, new Vector3(x, 0, z), Time.deltaTime * forwardLerpMultiplier);

                var currentEnemyDirection = targetEnemy ? (targetEnemy.transform.position - mainTarget.position).normalized : Vector3.zero;

                currentEnemyDirection.x *= enemyShiftX;
                currentEnemyDirection.z *= enemyShiftZ;

                enemyDirection = Vector3.Lerp(enemyDirection, currentEnemyDirection, Time.deltaTime * enemyShiftLerpMultiplier);

                InternalTarget.position = mainTarget.position + forward + enemyDirection;
            }
            else
            {
                InternalTarget.position = mainTarget.position;
            }
        }

        public static VirtualCameraCase GetCamera(CameraType cameraType)
        {
            return cameraController.virtualCameras[virtualCamerasLink[cameraType]];
        }

        public static void EnableCamera(CameraType cameraType)
        {
            if (activeVirtualCamera != null && activeVirtualCamera.CameraType == cameraType)
                return;

            for (var i = 0; i < cameraController.virtualCameras.Length; i++)
            {
                cameraController.virtualCameras[i].VirtualCamera.Priority = UNACTIVE_CAMERA_PRIORITY;
            }

            activeVirtualCamera = cameraController.virtualCameras[virtualCamerasLink[cameraType]];
            activeVirtualCamera.VirtualCamera.Priority = ACTIVE_CAMERA_PRIORITY;
        }
    }
}