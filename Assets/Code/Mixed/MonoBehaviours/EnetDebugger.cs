﻿using System;
using GSUnity.Ecs.Components.Netcode;
using GSUnity.Ecs.Utils;
using GSUnity.Netcode;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GSUnity.MonoBehaviours
{
    public class EnetDebugger : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _tmpValues;
        [SerializeField] private WorldFlags _worldFlags;
        
        private void Update()
        {
            foreach (var world in World.All)
            {
                if (world.Flags.HasFlag(_worldFlags) && TryGetNetcodeClientRef(world, out var netcodeClientRef))
                {
                    _tmpValues.text = world.Name + GetDebugString(netcodeClientRef);
                }
            }
        }

        private static bool TryGetNetcodeClientRef(World world, out NetcodeClientRef netcodeClientRef)
        {
            return world.EntityManager
                .CreateEntityQuery(typeof(NetcodeClientRef))
                .TryGetSingleton(out netcodeClientRef);
        }
        
        private static string GetDebugString(NetcodeClientRef netcodeClientRef)
        {
            return $"\n\n{netcodeClientRef.Host.PacketsSent}\n" +
                   $"{netcodeClientRef.Host.PacketsReceived}\n\n" +
                   $"{netcodeClientRef.Host.BytesSent}\n" +
                   $"{netcodeClientRef.Host.BytesReceived}\n\n" +
                   $"{netcodeClientRef.CommandSerializer.CommandsSent}\n" +
                   $"{netcodeClientRef.CommandDeserializer.CommandsReceived}\n\n" +
                   $"{netcodeClientRef.CommandSerializer.InstantiationsSent}\n" +
                   $"{netcodeClientRef.CommandDeserializer.InstantiationsReceived}\n\n" +
                   $"{netcodeClientRef.Client.SendCount}\n" +
                   $"{netcodeClientRef.Client.ReceiveCount}\n\n";
        }
    }
}