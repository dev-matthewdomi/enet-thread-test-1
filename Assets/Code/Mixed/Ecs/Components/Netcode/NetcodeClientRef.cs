using System.Threading;
using ENet;
using GSUnity.Netcode;
using GSUnity.Netcode.Serializers;
using Unity.Entities;
using UnityEngine;

namespace GSUnity.Ecs.Components.Netcode
{
    public class NetcodeClientRef : IComponentData
    {
        public Host Host;
        public NetcodeClient Client;

        public NetcodeCommandSerializer CommandSerializer;
        public NetcodeCommandDeserializer CommandDeserializer;

        private bool _isDisposed;
        
        public void DisposeLater()
        {
            if (_isDisposed)
                return;
            
            if (!Host.IsSet)
            {
                Debug.LogWarning("Attempted to dispose host that is not set!");
                return;
            }
            
            Client?.Stop();
            CommandSerializer?.Stop();
            CommandDeserializer?.Stop();
            
            var disposeThread = new Thread(DisposeHostAfterFinish);
            disposeThread.Start();

            _isDisposed = true;
        }

        private void DisposeHostAfterFinish()
        {
            //while (Sender.IsActive || Listener.IsActive)
            while (Client.IsActive)
            {
                Thread.Sleep(10);
            }
            
            Debug.Log($"Disposing host");
            Host.Flush();
            Host.Dispose();
        }
    }
}