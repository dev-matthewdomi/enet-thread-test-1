using GSUnity.Ecs.Components.Network;

namespace GSUnity.Netcode.Packets.Commands
{
    public struct InstantiateEntityCommand
    {
        public NetworkEntityId Id;
        public NetworkEntityType Type;
    }
}