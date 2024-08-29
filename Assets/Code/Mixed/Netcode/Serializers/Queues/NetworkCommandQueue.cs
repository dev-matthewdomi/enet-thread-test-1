using GSUnity.External;
using GSUnity.Netcode.Packets.Commands;

namespace GSUnity.Netcode.Serializers
{
    public struct NetworkCommandQueue
    {
        public readonly RingBuffer<NetworkTransportData<InstantiateEntityCommand>> InstantiateEntityQueue;
        public readonly RingBuffer<NetworkTransportData<DestroyEntityCommand>> DestroyEntityQueue;

        public NetworkCommandQueue(int instantiateEntityQueueCapacity, int destroyEntityQueueCapacity)
        {
            InstantiateEntityQueue = new RingBuffer<NetworkTransportData<InstantiateEntityCommand>>(instantiateEntityQueueCapacity);
            DestroyEntityQueue = new RingBuffer<NetworkTransportData<DestroyEntityCommand>>(destroyEntityQueueCapacity);
        }
    }
}