using System.Threading;
using ENet;
using GSUnity.External;
using GSUnity.Netcode.Packets;
using GSUnity.Netcode.Packets.Commands;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace GSUnity.Netcode.Serializers
{
    public class NetcodeCommandSerializer
    {
        private const int DefaultInstantiateEntityQueueCapacity = 128;
        private const int DefaultDestroyEntityQueueCapacity = 128;
        
        public readonly NetworkCommandQueue SendQueue;

        private NetcodeClient _client;
        
        public uint CommandsSent, SnapshotsSent, DeltaStatesSent;
        public uint InstantiationsSent, DestroysSent;
        
        private volatile bool _isActive;
        private Thread _workerThread;
        
        public NetcodeCommandSerializer(NetcodeClient client)
        {
            _client = client;

            SendQueue = new NetworkCommandQueue(DefaultInstantiateEntityQueueCapacity,
                DefaultDestroyEntityQueueCapacity);
        }

        public void Start()
        {
            _workerThread = new Thread(SerializeCommandLoop);
            _workerThread.Start();
        }

        public void Stop()
        {
            _isActive = false;
        }

        private void SerializeCommandLoop()
        {
            _isActive = true;
            Debug.Log($"Starting {nameof(NetcodeCommandSerializer)} worker {Thread.CurrentThread.ManagedThreadId}.");
            
            while (_isActive)
            {
                if (SendQueue.InstantiateEntityQueue.TryDequeue(out var sendInstantiateCommand))
                {
                    CommandsSent++;
                    InstantiationsSent++;
                    SendInstantiateEntityCommand(sendInstantiateCommand);
                }

                if (SendQueue.DestroyEntityQueue.TryDequeue(out var sendDestroyCommand))
                {
                    CommandsSent++;
                    DestroysSent++;
                    SendDestroyEntityCommand(sendDestroyCommand);
                }
            }
            
            Debug.Log($"Stopped {nameof(NetcodeCommandSerializer)} worker {Thread.CurrentThread.ManagedThreadId}.");
        }

        private void SendInstantiateEntityCommand(NetworkTransportData<InstantiateEntityCommand> sendInstantiateCommand)
        {
            var bitBuffer = new BitBuffer();
            bitBuffer.Add(2, (uint)NetworkPacketType.Command);
            bitBuffer.Add(6, (uint)NetworkCommandId.InstantiateEntity);
            
            bitBuffer.AddUInt(sendInstantiateCommand.Data.Id.Value);
            bitBuffer.AddByte((byte)sendInstantiateCommand.Data.Type);

            var length = UnsafeUtility.SizeOf<InstantiateEntityCommand>() + 1;
            var bytes = new byte[bitBuffer.Length];
            bitBuffer.ToArray(bytes);
            
            _client.SendQueue.Enqueue(new SendNetworkData
            {
                Data = new NetworkPacketData
                {
                    Data = bytes,
                    PacketFlags = PacketFlags.Reliable
                },
                Peer = sendInstantiateCommand.Peer,
                ChannelId = 0
            });
        }
        
        private void SendDestroyEntityCommand(NetworkTransportData<DestroyEntityCommand> sendDestroyEntityCommand)
        {      
            var bitBuffer = new BitBuffer();
            bitBuffer.AddUInt(sendDestroyEntityCommand.Data.Id.Value);
            
            var length = UnsafeUtility.SizeOf<DestroyEntityCommand>() + 1;
            var bytes = new byte[length];
            bitBuffer.ToArray(bytes);
            
            _client.SendQueue.Enqueue(new SendNetworkData
            {
                Data = new NetworkPacketData
                {
                    // Ptr = new IntPtr(ptr),
                    // Length = length,
                    Data = bytes,
                    PacketFlags = PacketFlags.Reliable
                },
                Peer = sendDestroyEntityCommand.Peer,
                ChannelId = 0
            });
        }
    }
}