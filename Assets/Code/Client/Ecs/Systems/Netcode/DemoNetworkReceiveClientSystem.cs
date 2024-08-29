using GSUnity.Ecs.Components.Netcode;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace GSUnity.Client.Ecs.Systems.Netcode
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct DemoNetworkReceiveClientSystem : ISystem
    {
        private float _timeSinceLastPrint;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetcodeClientRef>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (networkReceiveCommandQueueRef, netcodeClientRef) in SystemAPI
                         .Query<NetworkReceiveCommandQueueRef, NetcodeClientRef>())
            {
                var instantiationCount = 0;
                while (networkReceiveCommandQueueRef.Value.InstantiateEntityQueue.TryDequeue(out var receiveInstantiateEntityCommand))
                {
                    instantiationCount++;
                    //Debug.Log($"Received instantiate entity command: {receiveInstantiateEntityCommand.Data.Id.Value}:{receiveInstantiateEntityCommand.Data.Type}");
                }

                var destroyCount = 0;
                while (networkReceiveCommandQueueRef.Value.DestroyEntityQueue.TryDequeue(out var receiveDestroyEntityCommand))
                {
                    destroyCount++;
                    //Debug.Log($"Received destroy entity command: {receiveDestroyEntityCommand.Data.Id.Value}");
                }

                while (netcodeClientRef.Client.ReceiveConnectionQueue.TryDequeue(out var @event))
                {
                    Debug.Log($"{netcodeClientRef.Client.Name} Received connection event: {@event.Type}");
                }
                
                Debug.Log($"Instantiates: {instantiationCount} Destroys: {destroyCount}");
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}