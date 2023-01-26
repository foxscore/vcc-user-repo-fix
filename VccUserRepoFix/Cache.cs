using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MelonLoader;
using VRC.PackageManagement.Core;
using VRC.PackageManagement.Core.Types.Providers;

namespace VccUserRepoFix
{
    public static class Cache
    {
        private static HashSet<string> _userRepos;
        private static IEnumerable<IVRCPackageProvider> _internal;
        
        private static bool IsCacheValid()
        {
            if (_internal is null)
                return false;
            
            var current = Settings.Load().userRepos;
            if (
                _userRepos is null
                || current.Count != _userRepos.Count
                || current.Any(sci => !_userRepos.Contains(sci.url))
            )
            {
                _userRepos = current.Select(sci => sci.url).ToHashSet();
                return false;
            }
            
            return true;
        }

        private static bool _isCacheValid = false;
        // We want to time how long it takes to refresh the cache with a stopwatch
        private static readonly Stopwatch _stopwatch = new Stopwatch();

        public static bool Prefix()
        {
            _isCacheValid = IsCacheValid();

            if (!_isCacheValid)
            {
                MelonLogger.Msg("Cache is invalid, refreshing...");
                _stopwatch.Restart();
            }

            return !_isCacheValid;
        }
        
        public static void Postfix(ref IEnumerable<IVRCPackageProvider> __result)
        {
            if (_isCacheValid)
            {
                __result = _internal;
                return;
            }

            // Print the time it took to refresh the cache
            _stopwatch.Stop();
            var nanoSeconds = _stopwatch.ElapsedTicks * 1000000000 / Stopwatch.Frequency;
            var ms = nanoSeconds / 1000000f;
            MelonLogger.Msg($"Refresh took {ms:0.000}ms");

            // Provide the cache
            _internal = __result;
        }
    }
}