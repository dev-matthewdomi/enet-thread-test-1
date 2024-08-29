using ENet;
using GSUnity.Ecs.Components.Netcode;
using GSUnity.Netcode;
using GSUnity.Netcode.Serializers;
using GSUnity.Server.Ecs.Components.Netcode;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace GSUnity.Server.Ecs.Systems.Netcode
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct NetworkListenServerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkRequestListen>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (requestListen, entity) in SystemAPI
                         .Query<NetworkRequestListen>()
                         .WithEntityAccess())
            {
                ecb.RemoveComponent<NetworkRequestListen>(entity);

                var host = new Host();
                host.Create(requestListen.Address, requestListen.PeerLimit);
                host.SetBandwidthLimit(1000000000, 1000000000);

                // Network
                var client = new NetcodeClient(host) { Name = "Server" };
                client.Start();
                
                // Command Serializers
                var commandSerializer = new NetcodeCommandSerializer(client);
                var commandDeserializer = new NetcodeCommandDeserializer(client);
                ecb.AddComponent(entity, new NetworkReceiveCommandQueueRef
                {
                    Value = commandDeserializer.ReceiveQueue
                });
                ecb.AddComponent(entity, new NetworkSendCommandQueueRef
                {
                    Value = commandSerializer.SendQueue
                });
                commandSerializer.Start();
                commandDeserializer.Start();
                
                ecb.AddComponent(entity, new NetcodeClientRef
                {
                    Host = host,
                    Client = client,
                    CommandSerializer = commandSerializer,
                    CommandDeserializer = commandDeserializer
                });
            }
            ecb.Playback(state.EntityManager);
        }

        public void OnDestroy(ref SystemState state)
        {
            foreach (var netcodeClientRef in SystemAPI.Query<NetcodeClientRef>())
            {
                netcodeClientRef.DisposeLater();
            }
            
            Library.Deinitialize();
        }
    }
}