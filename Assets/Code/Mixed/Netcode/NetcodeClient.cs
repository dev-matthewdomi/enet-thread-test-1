using System;
using System.Threading;
using ENet;
using GSUnity.External;
using GSUnity.Netcode.Packets;
using Unity.Collections;
using UnityEngine;
using Event = ENet.Event;
using EventType = ENet.EventType;

namespace GSUnity.Netcode
{
    public class NetcodeClient
    {
        private const int DefaultPollTimeMs = 15;
        private const int DefaultReceiveConnectionQueueCapacity = 128;
        private const int DefaultReceiveCommandQueueCapacity = 1024;
        private const int DefaultReceiveSnapshotQueueCapacity = 2048;
        private const int DefaultReceiveInputQueueCapacity = 2048;

        private const int DefaultSendQueueCapacity = 1024;
        private const int DefaultBroadcastQueueCapacity = 512;
        
        public readonly RingBuffer<Event> ReceiveConnectionQueue;
        public readonly RingBuffer<Event> ReceiveCommandQueue;
        public readonly RingBuffer<Event> ReceiveSnapshotQueue;
        public readonly RingBuffer<Event> ReceiveDeltaStateQueue;
        
        public readonly RingBuffer<SendNetworkData> SendQueue;
        public readonly RingBuffer<BroadcastNetworkData> BroadcastQueue;
        
        private readonly int _pollTimeMs;
        private readonly Host _host;
        
        private volatile bool _isActive;
        private Thread _workerThread;

        public string Name;
        public uint ReceiveCount, SendCount;
        
        public bool IsActive => _isActive || _workerThread.IsAlive;
        
        public NetcodeClient(Host host, 
            int pollTimeMs = DefaultPollTimeMs, 
            int receiveConnectionQueueCapacity = DefaultReceiveConnectionQueueCapacity, 
            int receiveCommandQueueCapacity = DefaultReceiveCommandQueueCapacity, 
            int receiveSnapshotQueueCapacity = DefaultReceiveSnapshotQueueCapacity, 
            int receiveInputQueueCapacity = DefaultReceiveInputQueueCapacity,
            int sendQueueCapacity = DefaultSendQueueCapacity,  
            int broadcastQueueCapacity = DefaultBroadcastQueueCapacity)
        {
            _host = host;
            _pollTimeMs = pollTimeMs;
            
            ReceiveConnectionQueue = new RingBuffer<Event>(receiveConnectionQueueCapacity);
            ReceiveCommandQueue = new RingBuffer<Event>(receiveCommandQueueCapacity);
            ReceiveSnapshotQueue = new RingBuffer<Event>(receiveSnapshotQueueCapacity);
            ReceiveDeltaStateQueue = new RingBuffer<Event>(receiveInputQueueCapacity);
            
            SendQueue = new RingBuffer<SendNetworkData>(sendQueueCapacity);
            BroadcastQueue = new RingBuffer<BroadcastNetworkData>(broadcastQueueCapacity);
        }
        
        public void Start()
        {
            Library.Initialize();
            
            _workerThread = new Thread(ListenLoop);
            _workerThread.Start();
        }

        public void Stop()
        {
            _isActive = false;
        }

        public void ListenLoop()
        {
            _isActive = true;
            Debug.Log($"Starting {Name}[{nameof(NetcodeClient)}] worker {Thread.CurrentThread.ManagedThreadId}.");
            
            while (_isActive)
            {
                while (SendQueue.TryDequeue(out var sendNetworkData))
                {
                    var packet = sendNetworkData.Data.CreatePacket();
                    if (sendNetworkData.Peer.IsSet)
                    {        
                        SendCount++;            
                        sendNetworkData.Peer.Send(sendNetworkData.ChannelId, ref packet);
                    }
                    else
                    {
                        SendCount += _host.PeersCount;
                        _host.Broadcast(sendNetworkData.ChannelId, ref packet);
                    }
                }
                
                while (BroadcastQueue.TryDequeue(out var broadcastNetworkData))
                {
                    SendCount++;
                    var packet = broadcastNetworkData.Data.CreatePacket();
                    if (broadcastNetworkData.Peers == default)
                    {       
                        SendCount += _host.PeersCount;                 
                        _host.Broadcast(broadcastNetworkData.ChannelId, ref packet);
                    }
                    else
                    {               
                        SendCount += (uint)broadcastNetworkData.Peers.Length;    
                        _host.Broadcast(broadcastNetworkData.ChannelId, ref packet, broadcastNetworkData.Peers.ToArray());
                    }
                }
                
                var polled = false;
                while (!polled)
                {
                    if (_host.CheckEvents(out var netEvent) <= 0)
                    {
                        if (_host.Service(_pollTimeMs, out netEvent) <= 0)
                            break;
                        
                        polled = true;
                    }

                    ReceiveCount++;
                    switch (netEvent.Type)
                    {
                        case EventType.Connect:
                        case EventType.Disconnect:
                        case EventType.Timeout:
                            ReceiveConnectionQueue.Enqueue(netEvent);
                            break;
                        case EventType.Receive:
                            ReceiveEvent(netEvent);
                            break;
                        case EventType.None:
                            throw new Exception();
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            
            Debug.Log($"Stopped {Name}[{nameof(NetcodeClient)}] worker {Thread.CurrentThread.ManagedThreadId}.");
        }

        private void ReceiveEvent(Event netEvent)
        {
            var packetType = NetworkPacketType.Command; // TODO: PeekPacketType(netEvent.Packet.Data)
            switch (packetType)
            {
                case NetworkPacketType.Command:
                    ReceiveCommandQueue.Enqueue(netEvent);
                    break;
                case NetworkPacketType.Snapshot:
                    ReceiveSnapshotQueue.Enqueue(netEvent);
                    break;
                case NetworkPacketType.DeltaState:
                    ReceiveDeltaStateQueue.Enqueue(netEvent);
                    break;
                default:
                    Debug.LogWarning($"Packet of unknown type {packetType} received from peer {netEvent.Peer.ID}");
                    break;
            }
        }
    }

    public struct NetworkPacketData
    {
        public byte[] Data;
        public PacketFlags PacketFlags;

        public Packet CreatePacket()
        {
            Packet packet = default;
            packet.Create(Data, PacketFlags);
            return packet;
        }
    }
         
    public struct SendNetworkData
    {
        public NetworkPacketData Data;
             
        public byte ChannelId;
        public Peer Peer;
    }

    public struct BroadcastNetworkData
    {
        public NetworkPacketData Data;
             
        public byte ChannelId;
        public NativeArray<Peer> Peers;
    }
}