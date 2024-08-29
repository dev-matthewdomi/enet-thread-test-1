using ENet;
using GSUnity.Client.Ecs.Components.Netcode;
using Unity.Burst;
using Unity.Entities;

namespace GSUnity.Client.Ecs.Systems.Netcode
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct DemoNetworkConnectClientSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var address = new Address { Port = 7979 };
            address.SetHost("127.0.0.1");
            
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, new NetworkRequestConnect
            {
                Address = address
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