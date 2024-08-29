using Unity.Entities;
using Unity.Mathematics;

namespace GSUnity.Ecs.Components.Network.Snapshots
{
    public class PlayerMovementNetworkSnapshot : IComponentData
    {
        public float2 TargetPosition;
    }
}