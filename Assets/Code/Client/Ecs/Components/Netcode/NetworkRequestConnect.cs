using ENet;
using GSUnity.Netcode;
using Unity.Entities;

namespace GSUnity.Client.Ecs.Components.Netcode
{
    public struct NetworkRequestConnect : IComponentData
    {
        public Address Address;
    }
}