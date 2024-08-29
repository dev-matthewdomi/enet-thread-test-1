using Unity.Entities;

namespace GSUnity.Ecs.Utils
{
    /// <summary>
    /// Netcode specific extension methods for worlds.
    /// </summary>
    public static class WorldExtensions
    {
        /// <summary>
        /// Check if a world is a thin client.
        /// </summary>
        /// <param name="world">A <see cref="World"/> instance</param>
        /// <returns></returns>
        public static bool IsThinClient(this World world)
        {
            return (world.Flags&WorldFlags.GameThinClient) == WorldFlags.GameThinClient;
        }
        /// <summary>
        /// Check if an unmanaged world is a thin client.
        /// </summary>
        /// <param name="world">A <see cref="WorldUnmanaged"/> instance</param>
        /// <returns></returns>
        public static bool IsThinClient(this WorldUnmanaged world)
        {
            return (world.Flags & WorldFlags.GameThinClient) == WorldFlags.GameThinClient;
        }
        /// <summary>
        /// Check if a world is a client.
        /// </summary>
        /// <param name="world">A <see cref="World"/> instance</param>
        /// <returns></returns>
        public static bool IsClient(this World world)
        {
            return (world.Flags & WorldFlags.GameClient) == WorldFlags.GameClient;
        }
        /// <summary>
        /// Check if an unmanaged world is a client.
        /// </summary>
        /// <param name="world">A <see cref="WorldUnmanaged"/> instance</param>
        /// <returns></returns>
        public static bool IsClient(this WorldUnmanaged world)
        {
            return (world.Flags & WorldFlags.GameClient) == WorldFlags.GameClient;
        }
        /// <summary>
        /// Check if a world is a server.
        /// </summary>
        /// <param name="world">A <see cref="World"/> instance</param>
        /// <returns></returns>
        public static bool IsServer(this World world)
        {
            return (world.Flags & WorldFlags.GameServer) == WorldFlags.GameServer;
        }
        /// <summary>
        /// Check if an unmanaged world is a server.
        /// </summary>
        /// <param name="world">A <see cref="WorldUnmanaged"/> instance</param>
        /// <returns></returns>
        public static bool IsServer(this WorldUnmanaged world)
        {
            return (world.Flags & WorldFlags.GameServer) == WorldFlags.GameServer;
        }
    }
}