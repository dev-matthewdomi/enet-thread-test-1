using System.Threading;
using ENet;
using GSUnity.External;
using Unity.Collections;
using UnityEngine;

namespace GSUnity.Netcode
{
    public struct NetworkPacketData
    {
        public byte[] Data;
        public PacketFlags PacketFlags;

        public Packet CreatePacket()
        {
            Packet packet = default;
            //packet.Create(Ptr, Length, PacketFlags);
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
    
    public class NetcodeClientSender
    {
        private const int DefaultIdleTimeMs = 1;
        private const int DefaultSendQueueCapacity = 1024;
        private const int DefaultBroadcastQueueCapacity = 512;
        
        public readonly RingBuffer<SendNetworkData> SendQueue;
        public readonly RingBuffer<BroadcastNetworkData> BroadcastQueue;
        
        private readonly int _idleTimeMs;
        private readonly Host _host;
        
        private volatile bool _isSending;
        private Thread _workerThread;

        public string Name;
        public uint SendCount;
        
        public bool IsActive => _workerThread.IsAlive;
        
        public NetcodeClientSender(Host host, 
            int idleTimeMs = DefaultIdleTimeMs, 
            int sendQueueCapacity = DefaultSendQueueCapacity,  
            int broadcastQueueCapacity = DefaultBroadcastQueueCapacity)
        {
            _host = host;
            _idleTimeMs = idleTimeMs;
            SendQueue = new RingBuffer<SendNetworkData>(sendQueueCapacity);
            BroadcastQueue = new RingBuffer<BroadcastNetworkData>(broadcastQueueCapacity);
        }
        
        public void Start()
        {
            Library.Initialize();
            
            _workerThread = new Thread(SendLoop);
            _workerThread.Start();
        }

        public void Stop()
        {
            _isSending = false;
        }

        public void SendLoop()
        {
            _isSending = true;
            Debug.Log($"Starting {nameof(NetcodeClientSender)} worker {Thread.CurrentThread.ManagedThreadId}.");
            
            while (_isSending)
            {
                if (SendQueue.TryDequeue(out var sendNetworkData))
                {
                    SendCount++;
                    var packet = sendNetworkData.Data.CreatePacket();
                    if (sendNetworkData.Peer.IsSet)
                    {                    
                        //Debug.Log($"Sending data {sendNetworkData.Data.Ptr}[{sendNetworkData.Data.Length}]:{sendNetworkData.Data.PacketFlags} -> {sendNetworkData.ChannelId} -> {sendNetworkData.Peer.ID}");
                        sendNetworkData.Peer.Send(sendNetworkData.ChannelId, ref packet);
                    }
                    else
                    {
                        //Debug.Log($"Broadcasting peerless data {sendNetworkData.Data.Ptr}[{sendNetworkData.Data.Length}]:{sendNetworkData.Data.PacketFlags} -> {sendNetworkData.ChannelId} -> {_host.PeersCount} peers");
                        _host.Broadcast(sendNetworkData.ChannelId, ref packet);
                    }

                    //Marshal.FreeHGlobal(sendNetworkData.Data.Ptr);
                    continue; 
                }
                
                if (BroadcastQueue.TryDequeue(out var broadcastNetworkData))
                {
                    SendCount++;
                    var packet = broadcastNetworkData.Data.CreatePacket();
                    if (broadcastNetworkData.Peers == default)
                    {                        
                        //Debug.Log($"Broadcasting data {broadcastNetworkData.Data.Ptr}[{broadcastNetworkData.Data.Length}]:{broadcastNetworkData.Data.PacketFlags} -> {broadcastNetworkData.ChannelId} -> {_host.PeersCount} peers");
                        _host.Broadcast(broadcastNetworkData.ChannelId, ref packet);
                    }
                    else
                    {                   
                        //Debug.Log($"Broadcasting data {broadcastNetworkData.Data.Ptr}[{broadcastNetworkData.Data.Length}]:{broadcastNetworkData.Data.PacketFlags} -> {broadcastNetworkData.ChannelId} -> {broadcastNetworkData.Peers.Length} specific peers");
                        _host.Broadcast(broadcastNetworkData.ChannelId, ref packet, broadcastNetworkData.Peers.ToArray());
                    }

                    //Marshal.FreeHGlobal(broadcastNetworkData.Data.Ptr);
                    continue;
                }
                
                // If nothing is queued then throttle the thread by waiting a little bit
                Thread.Sleep(_idleTimeMs);
            }
            
            Debug.Log($"Stopped {nameof(NetcodeClientSender)} worker {Thread.CurrentThread.ManagedThreadId}.");
        }
    }
}