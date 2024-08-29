using GSUnity.Ecs.Components.Netcode;
using GSUnity.Ecs.Components.Network;
using GSUnity.Netcode.Packets.Commands;
using GSUnity.Netcode.Serializers;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace GSUnity.Server.Ecs.Systems.Netcode
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct DemoNetworkSendServerSystem : ISystem
    {
        private uint _sendCount;
        private float _timeSinceLastSend, _sendFrequency, _timeSinceLastPrint;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _sendFrequency = 1 / 30f;
        }
        
        public void OnUpdate(ref SystemState state)
        {
            if (_timeSinceLastSend < _sendFrequency)
            {
                _timeSinceLastSend += SystemAPI.Time.DeltaTime;
                return;
            }
            _timeSinceLastSend -= _sendFrequency;

            // var instantiateNetworkEntityCommand = new InstantiateEntityCommand
            // {
            //     Id = new NetworkEntityId { Value = _sendCount },
            //     Type = NetworkEntityType.Player
            // };
            //
            // foreach (var netcodeCommandSendQueueRef in SystemAPI.Query<NetworkSendCommandQueueRef>())
            // {
            //     for (var i = 0; i < 500; i++)
            //     {
            //         netcodeCommandSendQueueRef.Value.InstantiateEntityQueue.Enqueue(new NetworkTransportData<InstantiateEntityCommand>
            //         {
            //             Data = instantiateNetworkEntityCommand
            //         });
            //     }
            // }
            
            foreach (var (netcodeCommandSendQueueRef, netcodeClientRef) in SystemAPI.Query<NetworkSendCommandQueueRef, NetcodeClientRef>())
            {
                if (!netcodeClientRef.Listener.Peer.IsSet)
                    continue;
                
                for (var i = 0; i < 100; i++)
                {
                    var instantiateNetworkEntityCommand = new InstantiateEntityCommand
                    {
                        Id = new NetworkEntityId { Value = _sendCount },
                        Type = NetworkEntityType.Player
                    };
                    netcodeCommandSendQueueRef.Value.InstantiateEntityQueue.Enqueue(new NetworkTransportData<InstantiateEntityCommand>
                    {
                        Data = instantiateNetworkEntityCommand,
                        Peer = netcodeClientRef.Listener.Peer
                    });
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}