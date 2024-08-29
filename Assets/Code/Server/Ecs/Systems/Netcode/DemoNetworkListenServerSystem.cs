using ENet;
using GSUnity.Server.Ecs.Components.Netcode;
using Unity.Burst;
using Unity.Entities;

namespace GSUnity.Server.Ecs.Systems.Netcode
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct DemoNetworkConnectSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var address = new Address { Port = 7979 };
            address.SetHost("127.0.0.1");
            
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, new NetworkRequestListen
            {
                Address = address,
                PeerLimit = 100
            });
            state.Enabled = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}