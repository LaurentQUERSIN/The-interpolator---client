﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Stormancer;
using Stormancer.Core;

namespace Stormancer
{
    public class RemoteScene : MonoBehaviour
    {
        public string SceneId = "";
        public bool IsPublic = true;
        public bool DisconnectOnLoad = true;
        public List<StormancerIRemoteLogic> LocalLogics = new List<StormancerIRemoteLogic>();
        public bool Connected = false;


        private StormancerClientProvider _clientProvider;
        public Scene Scene;

        void Awake()
        {
            GameObject temp = GameObject.Find("StormancerClientProvider");
            if (temp != null)
            {
                _clientProvider = temp.GetComponent<StormancerClientProvider>();
                
            }
            else
            {
                Debug.LogError("cannot find stormancerClientProvider");
            }
        }

        void Start()
        {
            if (_clientProvider != null)
            {
                if (IsPublic == true)
                {
                    Scene = _clientProvider.GetPublicScene(SceneId, "");
                }
                if (Scene != null)
                {
                    foreach(StormancerIRemoteLogic logic in LocalLogics)
                    {
                        logic.Init(Scene);
                    }
                }
            }
            else
            {
                Debug.LogError("Cannot find StomrnacerClientProvider");
            }
        }

        public void ConnectScene()
        {
            Scene.Connect().ContinueWith(t =>
            {
                if (Scene.Connected == true)
                {
                    Debug.Log("connected to scene: " + SceneId);
                    Connected = true;
                }
                else
                {
                    Debug.LogWarning("failed to connect to scene: " + SceneId);
                    foreach (StormancerIRemoteLogic remotelogic in LocalLogics)
                    {
                        remotelogic.OnConnected();
                    }
                }
            });
        }
    }
}
