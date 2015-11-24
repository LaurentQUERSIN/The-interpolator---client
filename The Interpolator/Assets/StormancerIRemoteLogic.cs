using UnityEngine;
using System.Collections;
using Stormancer;
using Stormancer.Core;

namespace Stormancer
{
    public abstract class StormancerIRemoteLogic : MonoBehaviour
    {
        public RemoteScene RemoteScene;
        private long Clock
        {
            get
            {
                if (RemoteScene != null && RemoteScene.Scene != null)
                {
                    return RemoteScene.Scene.DependencyResolver.GetComponent<IClock>().Clock;
                }
                throw new System.InvalidOperationException("Missing scene.");
            }
        }

        public void OnAwake()
        {
            RemoteScene.LocalLogics.Add(this);
        }

        public abstract void Init(Scene s);
        public abstract void OnConnected();
    }
}
