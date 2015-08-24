using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.IO;
using Microsoft.Xna.Framework;

namespace MonoCustomCP {
    /// <summary> Stores loading and unloading methods for custom types. </summary>
    public static class CustomContentPipeline {
        private static readonly ConcurrentDictionary<Type, Func<RefCountedContentManager, string, object>> CustomLoadMethods =
            new ConcurrentDictionary<Type, Func<RefCountedContentManager, string, object>>();
        private static readonly ConcurrentDictionary<Type, Action<RefCountedContentManager, object, string>> CustomUnloadMethods =
            new ConcurrentDictionary<Type, Action<RefCountedContentManager, object, string>>();

        /// <summary> Registers the custom unload method for given asset type. This method will be called
        ///  when it is time for an asset to unload itself. </summary>
        /// <typeparam name="T">Asset type.</typeparam>
        /// <param name="assetUnload">Custom asset unload method.</param>
        public static void RegisterUnload<T>(Action<RefCountedContentManager, object, string> assetUnload) {
            Contract.Requires(assetUnload != null);

            CustomUnloadMethods.AddOrUpdate(typeof(T), assetUnload, (_, __) => assetUnload);
        }

        /// <summary> Registers the custom load method for given asset type. This method will be called
        ///  when it is time for an asset to load itself. </summary>
        /// <typeparam name="T">Asset type.</typeparam>
        /// <param name="assetLoad">Custom asset dispose method.</param>
        public static void RegisterLoad<T>(Func<RefCountedContentManager, string, object> assetLoad) {
            Contract.Requires(assetLoad != null);

            CustomLoadMethods.AddOrUpdate(typeof(T), assetLoad, (_, __) => assetLoad);
        }

        /// <summary> Finds previously registered custom unload method for given asset type. </summary>
        /// <param name="assetType">Asset type for which unload method was registered.</param>
        /// <returns>Found custom disposal method or null.</returns>
        public static Action<RefCountedContentManager, object, string> FindUnload(Type assetType) {
            Contract.Requires(assetType != null);

            Action<RefCountedContentManager, object, string> customDisposal;
            CustomUnloadMethods.TryGetValue(assetType, out customDisposal);
            return customDisposal;
        }

        /// <summary> Finds previously registered custom load method for given asset type. </summary>
        /// <param name="assetType">Asset type for which load method was registered.</param>
        /// <returns>Found custom load method or null.</returns>
        public static Func<RefCountedContentManager, string, object> FindLoad(Type assetType) {
            Contract.Requires(assetType != null);

            Func<RefCountedContentManager, string, object> customLoad;
            CustomLoadMethods.TryGetValue(assetType, out customLoad);
            return customLoad;
        }

        /// <summary> Clears previous custom unload method for given type. </summary>
        /// <typeparam name="T">Asset type for which unload method was registered.</typeparam>
        public static void ClearUnload<T>() {
            Action<RefCountedContentManager, object, string> customDisposal;
            CustomUnloadMethods.TryRemove(typeof(T), out customDisposal);
        }

        /// <summary> Clears previous custom load method for given type. </summary>
        /// <typeparam name="T">Asset type for which load method was registered.</typeparam>
        public static void ClearLoad<T>() {
            Func<RefCountedContentManager, string, object> customLoad;
            CustomLoadMethods.TryRemove(typeof(T), out customLoad);
        }

        /// <summary> Opens stream for the given file in title storage data. Don't forget to close it. </summary>
        /// <param name="contentRootPath">Content root path.</param>
        /// <param name="relativePath">Relative path to the target file.</param>
        /// <returns>
        /// Open stream to the given file.
        /// </returns>
        public static Stream OpenTitleStorage(string contentRootPath, string relativePath) {
            Contract.Requires(!Path.IsPathRooted(relativePath));

            var fontFilePath = Path.Combine(contentRootPath, relativePath);
            return TitleContainer.OpenStream(fontFilePath);
        }

        public static string GetCleanPath(string path, char directorySeparator = '\\',
            char altDirectorySeparator = '/') {
            int pathEnd;
            path = path.Replace(altDirectorySeparator, directorySeparator);
            for (int i = 1; i < path.Length; i = Math.Max(pathEnd - 1, 1)) {
                i = path.IndexOf(@"\..\", i, StringComparison.OrdinalIgnoreCase);
                if (i < 0) { return path; }

                pathEnd = path.LastIndexOf(directorySeparator, i - 1) + 1;
                path = path.Remove(pathEnd, (i - pathEnd) + @"\..\".Length);
            }

            return path;
        }
    }
}