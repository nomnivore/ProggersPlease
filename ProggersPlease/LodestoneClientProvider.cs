using System;
using System.Threading.Tasks;
using NetStone;

namespace ProggersPlease
{
    class LodestoneClientProvider
    {
        private static LodestoneClient? _instance;
        private static readonly Object _lock = new object();
        private static Task? _initTask;

        public static LodestoneClient? GetClient() {
            return _instance;
        }

        // create the client in the background
        public static void Initialize() {
            lock (_lock) {
                if (_initTask == null) {
                    _initTask = Task.Run(async () => {
                        _instance = await LodestoneClient.GetClientAsync();
                    });
                }
            }
        }

        // if needed, wait for the client to be initialized
        public static async Task WaitForClient() {
            if (_initTask != null) {
                await _initTask;
            }
        }

        public static void Dispose() {
            _instance?.Dispose();
            _instance = null;
            _initTask = null;
        }
    }
}
