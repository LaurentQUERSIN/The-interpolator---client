using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Stormancer
{
    public class StormancerNetworkIdentity : MonoBehaviour
    {
        public bool IsMaster = false;
        public uint Id;
        public int PrefabId;

        public List<SynchBehaviourBase> SynchBehaviours { get; set; }
        
        void Awake()
        {
            SynchBehaviours = new List<SynchBehaviourBase>(this.GetComponents<SynchBehaviourBase>());
            Debug.Log("create network identity with " + SynchBehaviours.Count + " behaviours");
        } 
    }
}