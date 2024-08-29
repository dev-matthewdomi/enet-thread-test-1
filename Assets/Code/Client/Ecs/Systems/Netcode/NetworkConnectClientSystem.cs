using ENet;
using GSUnity.Client.Ecs.Components.Netcode;
using GSUnity.Ecs.Components.Netcode;
using GSUnity.Netcode;
using GSUnity.Netcode.Serializers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace GSUnity.Client.Ecs.Systems.Netcode
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct NetworkConnectClientSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkRequestConnect>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (requestConnect, entity) in SystemAPI
                         .Query<NetworkRequestConnect>()
                         .WithEntityAccess())
            {
                ecb.RemoveComponent<NetworkRequestConnect>(entity);

                var host = new Host();
                host.Create();
                host.SetBandwidthLimit(1000000000, 1000000000);
                host.Connect(requestConnect.Address);

                // Network
                var sender = new NetcodeClientSender(host) { Name = "Client" };
                var listener = new NetcodeClientListener(host) { Name = "Client" };
                listener.Start();
                sender.Start();
                
                // Command Serializers
                var commandSerializer = new NetcodeCommandSerializer(sender);
                var commandDeserializer = new NetcodeCommandDeserializer(listener);
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
                    Listener = listener,
                    Sender = sender,
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