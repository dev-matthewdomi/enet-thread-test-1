using Unity.Entities;
using Unity.Mathematics;

namespace GSUnity.Ecs.Components.Network.Inputs
{
    public struct PlayerMovementNetworkInput : IComponentData
    {
        public float2 TargetPosition;
    }
}