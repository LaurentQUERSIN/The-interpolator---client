using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Stormancer
{
    public class StormancerNetworkIdentity : MonoBehaviour
    {
        public bool IsMaster = false;
        public long Id;
        public int PrefabId;

        public List<SynchBehaviourBase> SynchBehaviours { get; set; }
        
        void OnAwake()
        {
            SynchBehaviours = new List<SynchBehaviourBase>(this.GetComponents<SynchBehaviourBase>());
        } 
    }
}