using UnityEngine;
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
        
        public Scene Scene;

        void Start()
        {

            if (IsPublic == true)
            {
                Scene = ClientProvider.GetPublicScene(SceneId, "");
            }
            if (Scene != null)
            {
                foreach (StormancerIRemoteLogic logic in LocalLogics)
                {
                    logic.Init(Scene);
                }
                ConnectScene();
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
                    foreach (StormancerIRemoteLogic remotelogic in LocalLogics)
                    {
                        remotelogic.OnConnected();
                    }
                }
                else
                {
                    Debug.LogWarning("failed to connect to scene: " + SceneId);
                }
            });
        }

        void OnLoad()
        {
            if (DisconnectOnLoad == true)
                Scene.Disconnect();
        }
    }
}
