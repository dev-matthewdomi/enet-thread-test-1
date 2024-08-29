using System;
using System.Threading;
using ENet;
using GSUnity.Ecs.Components.Network;
using GSUnity.External;
using GSUnity.Netcode.Packets.Commands;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace GSUnity.Netcode.Serializers
{
    public class NetcodeCommandDeserializer
    {
        private const int DefaultInstantiateEntityQueueCapacity = 1024;
        private const int DefaultDestroyEntityQueueCapacity = 1024;
        
        public readonly NetworkCommandQueue ReceiveQueue;

        private NetcodeClient _client;

        public uint CommandsReceived, SnapshotsReceived, DeltaStatesReceived;
        public uint InstantiationsReceived, DestroysReceived;
        
        private volatile bool _isActive;
        private Thread _workerThread;
        
        public NetcodeCommandDeserializer(NetcodeClient client)
        {
            _client = client;

            ReceiveQueue = new NetworkCommandQueue(DefaultInstantiateEntityQueueCapacity,
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
            Debug.Log($"Starting {nameof(NetcodeCommandDeserializer)} worker {Thread.CurrentThread.ManagedThreadId}.");
            
            while (_isActive)
            {
                if (_client.ReceiveCommandQueue.TryDequeue(out var receiveCommand))
                {
                    CommandsReceived++;
                    
                    var bytes = new byte[receiveCommand.Packet.Length];
                    receiveCommand.Packet.CopyTo(bytes);
                    
                    var bitBuffer = new BitBuffer();
                    bitBuffer.FromArray(bytes, bytes.Length);
                    bitBuffer.Read(2);
                    
                    var commandId = (NetworkCommandId)bitBuffer.Read(6);
                    switch (commandId)
                    {
                        case NetworkCommandId.InstantiateEntity:
                            ReceiveInstantiateEntityCommand(ref bitBuffer, receiveCommand.Peer);
                            break;
                        case NetworkCommandId.DestroyEntity:
                            ReceiveDestroyEntityCommand(ref bitBuffer, receiveCommand.Peer);
                            break;
                        default:
                            Debug.LogWarning($"Received command of unknown type {commandId} from peer {receiveCommand.Peer.ID}");
                            break;
                    }
                    
                    receiveCommand.Packet.Dispose();
                }
            }
            
            Debug.Log($"Stopped {nameof(NetcodeCommandDeserializer)} worker {Thread.CurrentThread.ManagedThreadId}.");
        }

        private void ReceiveInstantiateEntityCommand(ref BitBuffer bitBuffer, Peer peer)
        {
            var instantiateEntityCommand = new InstantiateEntityCommand
            {
                Id = new NetworkEntityId
                {
                    Value = bitBuffer.ReadUInt()
                },
                Type = (NetworkEntityType)bitBuffer.ReadByte()
            };

            InstantiationsReceived++;
            ReceiveQueue.InstantiateEntityQueue.Enqueue(new NetworkTransportData<InstantiateEntityCommand>
            {
                Peer = peer,
                Data = instantiateEntityCommand
            });
        }
        
        private void ReceiveDestroyEntityCommand(ref BitBuffer bitBuffer, Peer peer)
        {
            // var destroyEntityCommand = new DestroyEntityCommand
            // {
            //     Id = new NetworkEntityId
            //     {
            //         Value = reader.ReadUInt()
            //     }
            // };
            // if (reader.HasFailedReads)
            //     return;
            var destroyEntityCommand = new DestroyEntityCommand
            {
                Id = new NetworkEntityId
                {
                    Value = bitBuffer.ReadUInt()
                }
            };

            DestroysReceived++;
            ReceiveQueue.DestroyEntityQueue.Enqueue(new NetworkTransportData<DestroyEntityCommand>
            {
                Peer = peer,
                Data = destroyEntityCommand
            });
        }
    }
}