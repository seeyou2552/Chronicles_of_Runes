using Cinemachine;
using UnityEngine;

namespace _02_Scripts.Player
{
    public class PlayerCameraHandler : MonoBehaviour
    {
        [SerializeField] string cameraName;

        public CameraShake cameraShake;

        private void OnEnable()
        {
            EventBus.Subscribe(CameraEventType.Shake, Shake);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(CameraEventType.Shake, Shake);
        }


        public void Start()
        {
            SetPlayerCamera(gameObject);
        }
        
        public void SetPlayerCamera(GameObject playerCamera)
        {
            // CameraManager.Instance.AddCamera(cameraName,this.gameObject);
            CameraManager.Instance.SetCamera(cameraName,this.gameObject);
        }


        public void Shake(object obj)
        {
            if (obj == null)
            {
                cameraShake.Shake();
            }/*
            else
            {
                float duration = (float)obj;
                cameraShake.Shake(duration);
            }
            */
        }
    }
}