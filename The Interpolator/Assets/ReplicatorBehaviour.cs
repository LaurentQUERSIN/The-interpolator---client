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
    public struct ReplicatorDTO
    {
        public uint Id;
        public int PrefabId;
    }

    public class ReplicatorBehaviour : StormancerIRemoteLogic
    {

        public List<GameObject> Prefabs;
        public List<StormancerNetworkIdentity> LocalObjectToSync;
        public ConcurrentDictionary<uint, StormancerNetworkIdentity> SlaveObjects;
        public ConcurrentDictionary<uint, StormancerNetworkIdentity> MastersObjects;

        private IClock Clock;

        public override void Init(Scene s)
        {
            if (s != null)
            {
                Clock = s.DependencyResolver.GetComponent<IClock>();
                s.AddRoute("CreateObject", OnCreateObject);
                s.AddRoute("DestroyObject", OnDestroyObject);
                s.AddRoute("ForceUpdate", OnForceUpdate);
                s.AddRoute("UpdateObject", OnUpdateObject);
            }
        }

        public override void OnConnected()
        {
            foreach (StormancerNetworkIdentity ni in LocalObjectToSync)
            {
                AddObjectToSynch(ni);
            }
        }

        public void AddObjectToSynch(StormancerNetworkIdentity ni)
        {
            var dto = new ReplicatorDTO();
            dto.PrefabId = ni.PrefabId;
            RemoteScene.Scene.RpcTask<ReplicatorDTO, ReplicatorDTO>("RegisterObject", dto).ContinueWith(response =>
            {
                dto = response.Result;
                ni.Id = dto.Id;
                MastersObjects.TryAdd(dto.Id, ni);
                if (SlaveObjects.ContainsKey(dto.Id))
                {
                    StormancerNetworkIdentity trash;
                    SlaveObjects.TryRemove(dto.Id, out trash);
                    MainThread.Post(() =>
                    {
                        Destroy(trash.gameObject);
                    });
                }
            });
        }

        public void RemoveSynchObject(StormancerNetworkIdentity ni)
        {
            var dto = new ReplicatorDTO();
            dto.Id = ni.Id;
            RemoteScene.Scene.Send<ReplicatorDTO>("RemoveObject", dto);
            MastersObjects.TryRemove(ni.Id, out ni);
        }

        private void OnCreateObject(Packet<IScenePeer> packet)
        {
            var dto = packet.ReadObject<ReplicatorDTO>();

            if (dto.PrefabId < Prefabs.Count && SlaveObjects.ContainsKey(dto.Id) == false && MastersObjects.ContainsKey(dto.Id) == false)
            {
                MainThread.Post(() =>
                {
                    var SynchedGO = Instantiate(Prefabs[dto.PrefabId]);
                    SlaveObjects.TryAdd(dto.Id, SynchedGO.GetComponent<StormancerNetworkIdentity>());
                });
            }
        }

        private void OnDestroyObject(Packet<IScenePeer> packet)
        {
            var dto = packet.ReadObject<ReplicatorDTO>();
            StormancerNetworkIdentity DestroyedGO;

            if (SlaveObjects.TryRemove(dto.Id, out DestroyedGO))
            {
                MainThread.Post(() =>
                {
                    Destroy(DestroyedGO.gameObject);
                });
            }
        }

        private void OnForceUpdate(Packet<IScenePeer> packet)
        {
            using (var reader = new BinaryReader(packet.Stream))
            {
                var id = reader.ReadUInt32();
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

        private void OnUpdateObject(Packet<IScenePeer> packet)
        {
            using (var reader = new BinaryReader(packet.Stream))
            {
                var id = reader.ReadUInt32();
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
                foreach (KeyValuePair<uint, StormancerNetworkIdentity> kvp in MastersObjects)
                {
                    int i = 0;
                    foreach (SynchBehaviourBase SB in kvp.Value.SynchBehaviours)
                    {
                        if (SB.LastSend + SB.getTimeBetweenUpdates() < Clock.Clock)
                        {
                            SB.LastSend = Clock.Clock;
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
