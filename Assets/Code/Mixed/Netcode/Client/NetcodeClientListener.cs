using System;
using System.Threading;
using ENet;
using GSUnity.External;
using GSUnity.Netcode.Packets;
using UnityEngine;
using Event = ENet.Event;
using EventType = ENet.EventType;

namespace GSUnity.Netcode
{
    public class NetcodeClientListener
    {
        private const int DefaultPollTimeMs = 15;
        private const int DefaultReceiveConnectionQueueCapacity = 128;
        private const int DefaultReceiveCommandQueueCapacity = 1024;
        private const int DefaultReceiveSnapshotQueueCapacity = 2048;
        private const int DefaultReceiveInputQueueCapacity = 2048;

        public readonly RingBuffer<Event> ReceiveConnectionQueue;
        public readonly RingBuffer<Event> ReceiveCommandQueue;
        public readonly RingBuffer<Event> ReceiveSnapshotQueue;
        public readonly RingBuffer<Event> ReceiveDeltaStateQueue;
        
        private readonly int _pollTimeMs;
        private readonly Host _host;
        
        private volatile bool _isListening;
        private Thread _workerThread;

        public string Name;
        public uint ReceiveCount;
        public Peer Peer;
        public float LastListenTime, LastEventTime, LastReceiveTime, ElapsedTime;
        public int EventState, ServiceState, State;
        
        public bool IsActive => _workerThread.IsAlive;
        
        public NetcodeClientListener(Host host, 
            int pollTimeMs = DefaultPollTimeMs, 
            int receiveConnectionQueueCapacity = DefaultReceiveConnectionQueueCapacity, 
            int receiveCommandQueueCapacity = DefaultReceiveCommandQueueCapacity, 
            int receiveSnapshotQueueCapacity = DefaultReceiveSnapshotQueueCapacity, 
            int receiveInputQueueCapacity = DefaultReceiveInputQueueCapacity)
        {
            _host = host;
            _pollTimeMs = pollTimeMs;
            
            ReceiveConnectionQueue = new RingBuffer<Event>(receiveConnectionQueueCapacity);
            ReceiveCommandQueue = new RingBuffer<Event>(receiveCommandQueueCapacity);
            ReceiveSnapshotQueue = new RingBuffer<Event>(receiveSnapshotQueueCapacity);
            ReceiveDeltaStateQueue = new RingBuffer<Event>(receiveInputQueueCapacity);
        }
        
        public void Start()
        {
            Library.Initialize();
            
            _workerThread = new Thread(ListenLoop);
            _workerThread.Start();
        }

        public void Stop()
        {
            _isListening = false;
        }

        public void ListenLoop()
        {
            _isListening = true;
            Debug.Log($"Starting {Name}[{nameof(NetcodeClientListener)}] worker {Thread.CurrentThread.ManagedThreadId}.");
            
            while (_isListening)
            {
                var polled = false;
                while (!polled)
                {
                    LastListenTime = ElapsedTime;
                    EventState = _host.CheckEvents(out var netEvent);
                    if (EventState <= 0)
                    {
                        LastEventTime = ElapsedTime;
                        ServiceState = _host.Service(_pollTimeMs, out netEvent);
                        if (ServiceState <= 0)
                            break;
                        
                        polled = true;
                    }

                    LastReceiveTime = ElapsedTime;
                    ReceiveCount++;
                    switch (netEvent.Type)
                    {
                        case EventType.Connect:
                        case EventType.Disconnect:
                        case EventType.Timeout:
                            Peer = netEvent.Peer;
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
                
                // if (Sender.SendQueue.TryDequeue(out var sendNetworkData))
                // {
                //     Sender.SendCount++;
                //     var packet = sendNetworkData.Data.CreatePacket();
                //     if (sendNetworkData.Peer.IsSet)
                //     {                    
                //         //Debug.Log($"Sending data {sendNetworkData.Data.Ptr}[{sendNetworkData.Data.Length}]:{sendNetworkData.Data.PacketFlags} -> {sendNetworkData.ChannelId} -> {sendNetworkData.Peer.ID}");
                //         sendNetworkData.Peer.Send(sendNetworkData.ChannelId, ref packet);
                //     }
                //     else
                //     {
                //         //Debug.Log($"Broadcasting peerless data {sendNetworkData.Data.Ptr}[{sendNetworkData.Data.Length}]:{sendNetworkData.Data.PacketFlags} -> {sendNetworkData.ChannelId} -> {_host.PeersCount} peers");
                //         _host.Broadcast(sendNetworkData.ChannelId, ref packet);
                //     }
                //
                //     //Marshal.FreeHGlobal(sendNetworkData.Data.Ptr);
                //     continue; 
                // }
                //
                // if (Sender.BroadcastQueue.TryDequeue(out var broadcastNetworkData))
                // {
                //     Sender.SendCount++;
                //     var packet = broadcastNetworkData.Data.CreatePacket();
                //     if (broadcastNetworkData.Peers == default)
                //     {                        
                //         //Debug.Log($"Broadcasting data {broadcastNetworkData.Data.Ptr}[{broadcastNetworkData.Data.Length}]:{broadcastNetworkData.Data.PacketFlags} -> {broadcastNetworkData.ChannelId} -> {_host.PeersCount} peers");
                //         _host.Broadcast(broadcastNetworkData.ChannelId, ref packet);
                //     }
                //     else
                //     {                   
                //         //Debug.Log($"Broadcasting data {broadcastNetworkData.Data.Ptr}[{broadcastNetworkData.Data.Length}]:{broadcastNetworkData.Data.PacketFlags} -> {broadcastNetworkData.ChannelId} -> {broadcastNetworkData.Peers.Length} specific peers");
                //         _host.Broadcast(broadcastNetworkData.ChannelId, ref packet, broadcastNetworkData.Peers.ToArray());
                //     }
                //
                //     //Marshal.FreeHGlobal(broadcastNetworkData.Data.Ptr);
                //     continue;
                // }
            }
            
            Debug.Log($"Stopped {Name}[{nameof(NetcodeClientListener)}] worker {Thread.CurrentThread.ManagedThreadId}.");
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
}