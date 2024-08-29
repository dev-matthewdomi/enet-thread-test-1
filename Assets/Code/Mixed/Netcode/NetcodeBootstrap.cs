using GSUnity.Ecs.Components;
using Unity.Entities;
using UnityEngine.Scripting;

namespace GSUnity.Netcode
{
    [Preserve] 
    public struct NetcodeBootstrap : ICustomBootstrap
    {
#if UNITY_EDITOR
        private const string WorldName = "EditorClientWorld";
        private const string ServerWorldName = "EditorServerWorld";
        private const string ThinClientWorldNamePrefix = "ThinClient";
        
        private const uint ThinClientCount = 0;
#elif !UNITY_SERVER
        private const string WorldName = "ClientWorld";
#elif !UNITY_CLIENT
        private const string ServerWorldName = "ServerWorld";
#endif
        
        public bool Initialize(string defaultWorldName)
        {
#if UNITY_EDITOR
            for (var i = 0; i < ThinClientCount; i++)
            {
                var thinClientWorld = CreateThinClientWorld($"{ThinClientWorldNamePrefix}{i}");
                //thinClientWorld.EntityManager.CreateSingleton(new WorldIndex { Value = (byte)(i + 1) });
            }
            
#endif
#if !UNITY_SERVER || UNITY_EDITOR
            World.DefaultGameObjectInjectionWorld ??= CreateClientWorld(WorldName);
            
#endif
#if !UNITY_CLIENT || UNITY_EDITOR
            CreateServerWorld(ServerWorldName);
            
#endif
            return true;
        }

#if !UNITY_SERVER || UNITY_EDITOR
        private static World CreateClientWorld(string worldName = "")
        {
            var clientWorld = new World(worldName, WorldFlags.GameClient);
            AddSystems(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.Presentation,
                clientWorld);
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(clientWorld);
            
            return clientWorld;
        }
        
#endif
#if !UNITY_CLIENT || UNITY_EDITOR
        private static World CreateServerWorld(string worldName = "")
        {
            var serverWorld = new World(worldName, WorldFlags.GameServer);
            AddSystems(WorldSystemFilterFlags.ServerSimulation, serverWorld);
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(serverWorld);
            
            return serverWorld;
        }
        
#endif
#if UNITY_EDITOR
        private static World CreateThinClientWorld(string worldName = "")
        {
            var thinClientWorld = new World(worldName, WorldFlags.GameThinClient);
            AddSystems(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation, thinClientWorld);
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(thinClientWorld);
            
            return thinClientWorld;
        }
        
#endif

        private static void AddSystems(WorldSystemFilterFlags worldSystemFlags, World world)
        {
            var systems = DefaultWorldInitialization.GetAllSystems(worldSystemFlags);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);
        }
    }
}