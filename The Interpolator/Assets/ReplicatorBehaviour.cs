using UnityEngine;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Stormancer;
using Stormancer.Core;
using Stormancer.Diagnostics;
using System.Threading.Tasks;
using Stormancer.Plugins;
using System.IO;
using System.Linq;
using System;

namespace Stormancer
{
    public class ReplicatorBehaviour : StormancerIRemoteLogic
    {

        public List<GameObject> Prefabs;
        public List<StormancerNetworkIdentity> LocalObjectToSync;
        public ConcurrentDictionary<long, StormancerNetworkIdentity> SlaveObjects;
        public ConcurrentDictionary<long, StormancerNetworkIdentity> MastersObjects;

        private IClock Clock;

        public override void Init(Scene s)
        {
            if (s != null)
            {
                Clock = s.DependencyResolver.GetComponent<IClock>();
                s.AddRoute("CreateSynch", OnCreateSynch);
                s.AddRoute("DestroySynch", OnDestroySynch);
                s.AddRoute("ForceUpdate", OnForceUpdate);
                s.AddRoute("UpdateSynch", OnUpdateSynch);
            }
        }

        public override void OnConnected()
        {
            int i = 0;
            foreach (StormancerNetworkIdentity ni in LocalObjectToSync)
            {
                RemoteScene.Scene.SendPacket("RegisterObject", s =>
                {
                    using (var writer = new BinaryWriter(s, System.Text.Encoding.UTF8))
                    {

                        writer.Write(ni.PrefabId);
                        writer.Write(i);
                    }
                });
            }
        }

        public void AddObjectToSynch(StormancerNetworkIdentity ni)
        {
            LocalObjectToSync.Add(ni);
            RemoteScene.Scene.SendPacket("RegisterObject", s =>
            {
                using (var writer = new BinaryWriter(s, System.Text.Encoding.UTF8))
                {
                    writer.Write(ni.PrefabId);
                    writer.Write(LocalObjectToSync.Count - 1);
                }
            });
        }

        private void OnCreateSynch(Packet<IScenePeer> packet)
        {
            using (var reader = new BinaryReader(packet.Stream))
            {

                var isMaster = reader.ReadBoolean();

                if (isMaster == true)
                {
                    var id = reader.ReadInt64();
                    var pos = reader.ReadInt32();
                    MastersObjects.TryAdd(id, LocalObjectToSync[pos]);
                    if (SlaveObjects.ContainsKey(id))
                    {
                        StormancerNetworkIdentity temp;
                        SlaveObjects.TryRemove(id, out temp);
                        MainThread.Post(() =>
                        {
                            Destroy(temp.gameObject);
                        });
                    }
                }
                else
                {
                    var id = reader.ReadInt64();
                    var pb = reader.ReadInt32();

                    if (pb < Prefabs.Count && SlaveObjects.ContainsKey(id) == false)
                    {
                        MainThread.Post(() =>
                        {
                            var SynchedGO = Instantiate(Prefabs[pb]);
                            SlaveObjects.TryAdd(id, SynchedGO.GetComponent<StormancerNetworkIdentity>());
                        });
                    }
                }
            }
        }

        private void OnDestroySynch(Packet<IScenePeer> packet)
        {
            using (var reader = new BinaryReader(packet.Stream))
            {
                var id = reader.ReadInt64();
                StormancerNetworkIdentity DestroyedGO;

                if (SlaveObjects.TryRemove(id, out DestroyedGO))
                {
                    MainThread.Post(() =>
                    {
                        Destroy(DestroyedGO.gameObject);
                    });
                }
            }
        }

        private void OnForceUpdate(Packet<IScenePeer> packet)
        {
            using (var reader = new BinaryReader(packet.Stream))
            {
                var id = reader.ReadInt64();
                var SBid = reader.ReadByte();
                StormancerNetworkIdentity SO;

                if (MastersObjects.TryGetValue(id, out SO) && SBid < SO.SynchBehaviours.Count)
                {
                    MainThread.Post(() =>
                    {
                        SO.SynchBehaviours[SBid].ApplyChanges(packet.Stream);
                    });
                }
            }
        }

        private void OnUpdateSynch(Packet<IScenePeer> packet)
        {
            using (var reader = new BinaryReader(packet.Stream))
            {
                var id = reader.ReadInt64();
                var SBid = reader.ReadByte();
                StormancerNetworkIdentity SO;

                if (SlaveObjects.TryGetValue(id, out SO) && SBid < SO.SynchBehaviours.Count)
                {
                    MainThread.Post(() =>
                    {
                        SO.SynchBehaviours[SBid].ApplyChanges(packet.Stream);
                    });
                }
            }
        }

        void Update()
        {
            if (RemoteScene != null && RemoteScene.Scene != null && RemoteScene.Scene.Connected && MastersObjects.Count > 0)
            {
                foreach (KeyValuePair<long, StormancerNetworkIdentity> kvp in MastersObjects)
                {
                    int i = 0;
                    foreach (SynchBehaviourBase SB in kvp.Value.SynchBehaviours)
                    {
                        if (SB.LastSend + SB.getTimeBetweenUpdates() < Clock.Clock)
                        {
                            RemoteScene.Scene.SendPacket("update_synchedObject", stream =>
                            {
                                using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8))
                                {
                                    writer.Write(kvp.Key);
                                    writer.Write(i);
                                    SB.SendChanges(stream);
                                }
                            }, PacketPriority.MEDIUM_PRIORITY, SB.Reliability);
                        }
                        i++;
                    }
                }
            }
        }
    }
}
