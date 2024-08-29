using ENet;

namespace GSUnity.Netcode.Serializers
{
    public struct NetworkTransportData<T>
    {
        public Peer Peer;
        public T Data;
    }
}