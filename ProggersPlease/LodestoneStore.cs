using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using NetStone.Search.Character;

namespace ProggersPlease;

// This class handles fetching and caching lodestone character ids
public class LodestoneStore : IDisposable
{
    private readonly MemoryCache _cache;
    public LodestoneStore() {
        _cache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 100 }); // size limit is # of entries
    }

    public async Task<string?> GetLodestoneId(string name, string world) {
        // check cache first
        if (_cache.TryGetValue($"{name}_{world}", out var cachedId)) {
            return cachedId as string;
        }

        // if not cached, fetch from lodestone
        if (LodestoneClientSingleton.GetClient() is { } client) {
            var response = await client.SearchCharacter(new CharacterSearchQuery() {
                CharacterName = name,
                World = world
            });

            var lodestoneResult = response?.Results.Where(c => c.Name == name).FirstOrDefault();
            if (lodestoneResult is { }) {
                // add to cache
                _cache.Set($"{name}_{world}", lodestoneResult.Id, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromHours(2),
                    Size = 1,
                });
                return lodestoneResult.Id;
            }
        }

        return null;
    }

    public void Dispose() {
        _cache.Dispose();
    }

}
