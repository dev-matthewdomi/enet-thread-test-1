using GSUnity.Ecs.Components.Network;

namespace GSUnity.Netcode.Packets.Commands
{
    public struct DestroyEntityCommand
    {
        public NetworkEntityId Id;
    }
}