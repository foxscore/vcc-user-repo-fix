using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using HarmonyLib;
using MelonLoader;
using VRC.PackageManagement.Core;
using VRC.PackageManagement.Core.Types.Providers;
using Harmony = HarmonyLib.Harmony;
using Main = VccUserRepoFix.Main;

[assembly: MelonInfo(typeof(Main), "VccUserRepoFix", "1.0.0", "Fox_score")]
[assembly: MelonGame("VRChat", "CreatorCompanion")]

namespace VccUserRepoFix
{
    public class Main : MelonMod
    {
        private static FieldInfo _providersCacheDirField;
        private static Type _providerType;
        private static FieldInfo _pathField; // Private string _path
        private static IntPtr _providerCtorPtr;

        private static void Fail(string message)
        {
            // Allow a bit of time for the message to be read before exiting
            MelonLogger.Error(message);
            Thread.Sleep(3000);
            Environment.Exit(1);
        }
        
        public override void OnInitializeMelon()
        {
            try
            {
                var harmony = new HarmonyLib.Harmony("VccUserRepoFix");
                
                MelonLogger.Msg("Initializing VccUserRepoFix");

                #region  Repos
                _providersCacheDirField = typeof(Repos).GetField("ProvidersCacheDir", BindingFlags.Public | BindingFlags.Static);
            
                if (_providersCacheDirField is null)
                {
                    Fail("Could not find ProvidersCacheDir field");
                    return;
                }
                
                // Get the getter of the public static User property
                var userGetter = typeof(Repos)
                    .GetProperty("User", BindingFlags.Public | BindingFlags.Static)?
                    .GetGetMethod();
                if (userGetter is null)
                {
                    Fail("Could not find Repos.User getter");
                    return;
                }
                
                // Get the public static Cache.Prefix method
                var cachePrefixMethod = typeof(Cache).GetMethod(nameof(Cache.Prefix), BindingFlags.Public | BindingFlags.Static);
                if (cachePrefixMethod is null)
                {
                    Fail("Could not find Cache.Prefix method");
                    return;
                }
                
                // Get the public static Cache.Postfix method
                var cachePostfixMethod = typeof(Cache).GetMethod(nameof(Cache.Postfix), BindingFlags.Public | BindingFlags.Static);
                if (cachePostfixMethod is null)
                {
                    Fail("Could not find Cache.Postfix method");
                    return;
                }

                // Patch the getter of Repos.User
                MelonLogger.Msg("Attempting to patch Repos.User getter");
                harmony.Patch(userGetter, new HarmonyMethod(cachePrefixMethod), new HarmonyMethod(cachePostfixMethod));

                #endregion
            
                #region Provider
            
                // Find the class of type VRC.PackageManagement.Core.Types.Providers.VPMPackageProvider
                _providerType = typeof(VPMPackageProvider);
                
                // If we found the type, we can get the method we want to patch
                if (_providerType is null)
                {
                    Fail("Could not find VRC.PackageManagement.Core.Types.Providers.VPMPackageProvider");
                    return;
                }
                
                _pathField = _providerType.GetField("_path", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_pathField is null)
                {
                    Fail("Could not find _path field");
                    return;
                }
            
                // Get the private (string = null, string = null) constructor
                var ctor = _providerType.GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null, new[] {typeof(string), typeof(string)}, null
                );
                if (ctor is null)
                {
                    Fail("Could not find constructor");
                    return;
                }
                
                // Get the replacement method OnCtor
                var onCtor = typeof(Main).GetMethod(nameof(OnCtor), BindingFlags.NonPublic | BindingFlags.Static);
                
                if (onCtor is null)
                {
                    Fail("Could not find OnCtor method");
                    return;
                }
                
                // Get the pointer to the constructor
                _providerCtorPtr = ctor.MethodHandle.Value;
            
                // Patch the constructor
                MelonLogger.Msg("Attempting to patch constructor of VPMProvider");
                harmony.Patch(
                    ctor,
                    null,
                    new HarmonyLib.HarmonyMethod(onCtor)
                );
            
                #endregion
                
                MelonLogger.Msg("Initialized VccUserRepoFix");
            }
            catch (Exception e)
            {
                Fail($"Failed to initialize VccUserRepoFix: {e}");
            }
        }
        
        private static void OnCtor(object __instance, string url, string path)
        {
            // If the url starts with https://vpm.directus.app, return
            if (
                !(path is null)
                || (url is null)
                || url.StartsWith("https://vpm.directus.app")
            )
            {
                MelonLogger.Msg("Skipping patch for hardcoded repository");
                return;
            }
            
            MelonLogger.Msg($"Pathing path for {url}");
            
            // If the path is null, we need to set _path to Path.Combine(ProvidersCacheDir, $"{[md5-hash of url]}.json")
            var providersCacheDir = (string) _providersCacheDirField.GetValue(null);
            var hash = BitConverter.ToString(
                System.Security.Cryptography.MD5
                    .Create()
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(url))
                )
                .Replace("-", "")
                .ToLower();
            var pathValue = System.IO.Path.Combine(providersCacheDir, $"{hash}.json");
            
            _pathField.SetValue(__instance, pathValue);
        }
    }
}