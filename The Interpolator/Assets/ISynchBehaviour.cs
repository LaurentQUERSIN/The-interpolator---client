using UnityEngine;
using System.Collections;
using Stormancer.Core;
using System.IO;

namespace Stormancer
{
    public abstract class SynchBehaviourBase : MonoBehaviour
    {
        //Unity
        public long timeBetweenUpdate = 200;

        //Not Unity
        public long LastSend { get; set; }
        public long LastChanged { get; set; }

        public void SetTimeBetweenUpdates(long t)
        {
            if (t >= 50)
            {
                timeBetweenUpdate = t;
            }
        }

        public long getTimeBetweenUpdates()
        {
            return timeBetweenUpdate;
        }

        public abstract void SendChanges(Stream stream);
        public abstract void ApplyChanges(Stream stream);

        public PacketReliability Reliability = PacketReliability.UNRELIABLE_SEQUENCED;

        public bool synch { get; private set; }

        public void SynchImmediate()
        {
            synch = true;
        }
    }
}
