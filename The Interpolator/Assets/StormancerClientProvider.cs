using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Stormancer;
using System.Threading.Tasks;

namespace Stormancer
{
    public class StormancerClientProvider : MonoBehaviour
    {
        public string accountId = "";
        public string applicationName = "";

        private StormancerClientProvider _instance;
        public StormancerClientProvider instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StormancerClientProvider();
                return _instance;
            }
        }

        private Client _clientRef = null;
        private Client _client
        {
            get
            {
                if (_clientRef == null)
                {
                    if (accountId == "" || applicationName == "")
                    {
                        Debug.LogWarning("Cannot create client without accoutID and applicationName");
                        throw new System.Exception("Cannot create client without accoutID and applicationName");
                    }
                    UniRx.MainThreadDispatcher.Initialize();
                    var config = ClientConfiguration.ForAccount(accountId, applicationName);
                    _clientRef = new Client(config);
                }
                return _clientRef;
            }
        }
        private List<Scene> _scenes = new List<Scene>();

        public Scene GetPublicScene<T>(string sceneId, T data)
        {
            if (sceneId == "")
            {
                Debug.LogWarning("SceneID can't be empty, cannot connect to remote scene");
                return null;
            }
            if (_client == null)
            {
                Debug.LogError("Client not created. unable to connect to remote scene");
            }
            return _client.GetPublicScene(sceneId, data).ContinueWith(t =>
            {
                if (t.IsFaulted == true)
                {
                    Debug.LogWarning("connection Failed");
                    return null;
                }
                if (_scenes.Contains(t.Result) == true)
                {
                    Debug.LogWarning("the scene " + sceneId + " have already been retrieved");
                    return null;
                }
                Debug.Log("Retreived remote scene");
                _scenes.Add(t.Result);
                return t.Result;
            }).Result;
        }

        public Scene GetPrivateScene<T>(string token)
        {
            //to do;
            return null;
        }

        void OnApplicationQuit()
        {
            foreach(Scene s in _scenes)
            {
                s.Disconnect();
            }
            _client.Disconnect();
        }
    }
}
