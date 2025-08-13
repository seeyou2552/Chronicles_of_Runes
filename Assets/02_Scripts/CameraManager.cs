using Cinemachine;
using Pathfinding.Ionic.Zip;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _02_Scripts
{
    public class CameraManager : Singleton<CameraManager>
    {
        private Dictionary<string, GameObject> CameraList;
        public Camera mainCamera;
        public void Awake()
        {
            if (Instance != this)
            {
                Destroy(gameObject); // 중복된 인스턴스 파괴
                return;
            }
            DontDestroyOnLoad(gameObject);
            mainCamera = gameObject.GetComponent<Camera>();
        }

        public void SetCamera(string cameraName, GameObject camera)
        {
            camera.GetComponent<CinemachineVirtualCamera>().Follow =
                PlayerController.Instance.gameObject.transform;
        }

        public void ChangeBackground(StageTheme theme)
        {
            switch (theme)
            {
                case StageTheme.Castle:
                case StageTheme.Catacomb:
                case StageTheme.Dungeon:
                    Camera.main.backgroundColor = Color.black;
                    break;
                case StageTheme.Volcano:
                    Camera.main.backgroundColor = new Color(0.7098f, 0.2784f, 0.1568f);
                    break;
            }
        }
    }
}