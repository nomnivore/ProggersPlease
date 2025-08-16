using System;
using System.Collections;
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
        // check cache first using GetCachedId
        if (GetCachedId(name, world) is { } cachedId) {
            return cachedId;
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
                _cache.Set(Utils.ToKey(name, world), lodestoneResult.Id, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromHours(2),
                    Size = 1,
                });
                return lodestoneResult.Id;
            }
        }

        return null;
    }

    public async Task<string?> GetLodestoneId(string key)
    {
        // split key into name and world
        var (name, world) = Utils.FromKey(key);

        return await GetLodestoneId(name, world);
    }

    public string? GetCachedId(string name, string world) {
        return GetCachedId(Utils.ToKey(name, world));

    }

    public string? GetCachedId(string key) {

        if (_cache.TryGetValue(key, out var cachedId))
        {
            return cachedId as string;
        }
        return null;
    }

    public void RemoveCachedId(string name, string world) {
        RemoveCachedId(Utils.ToKey(name, world));
    }

    public void RemoveCachedId(string? key) {
        if (key is null) {
            return;
        }

        _cache.Remove(key);
    }

    public void EmptyCache()
    {
        _cache.Clear();
    }

    public IEnumerable GetCacheKeys()
    {
        var keys = _cache.Keys.AsEnumerable();

        return keys;
    }

    public int GetCacheSize() {
        return _cache.Count;
    }

    public void Dispose() {
        _cache.Dispose();
    }

}
