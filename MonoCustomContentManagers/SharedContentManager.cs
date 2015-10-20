using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MonoCustomContentManagers {
    /// <summary>Content manager which can be used to preserve shared assets: an asset that is loaded for Level #1
    ///  and re-used in Level #2 can be kept in memory instead of being destroyed and reloaded.
    ///  Load Level #2 assets first, then unload Level #1 assets. </summary>
    /// <remarks>Class is "thread-safe" as in it maintains internal state consistently.
    /// You still should lock when you need logical consistency maintained across multiple operations in a sequence,
    ///  e.g.loading an asset from one thread while unloading an asset from another.
    /// </remarks>
    public class SharedContentManager : ContentManager {
        /// <summary> Initializes a new instance of the <see cref="SharedContentManager"/> class. </summary>
        /// <param name="game">The game reference.</param>
        /// <exception cref="InvalidOperationException">By design, the class assumes that all your content
        ///  managers will have the same root path and use the same service provider.</exception>
        public SharedContentManager(Game game): this(game.Services, game.Content.RootDirectory) {
        }

        /// <summary> Initializes a new instance of the <see cref="SharedContentManager"/> class. </summary>
        /// <param name="serviceProvider">The service provider, must be the same for all shared managers. </param>
        /// <param name="rootDirectory">Content root path, must be the same for all shared managers. </param>
        /// <exception cref="InvalidOperationException">By design, the class assumes that all your content
        ///  managers will have the same root path and use the same service provider.</exception>
        public SharedContentManager(IServiceProvider serviceProvider, string rootDirectory)
            : base(serviceProvider, rootDirectory) {

            lock (RealManagerLocker) {
                if (_realManager == null) {
                    _realManager = new RefCountedContentManager(serviceProvider, rootDirectory);
                } else if (_realManager.ServiceProvider != serviceProvider ||
                           _realManager.RootDirectory != rootDirectory) {
                    throw new InvalidOperationException(
                        "All shared content managers must have the same root path and service provider.");
                }
            }

            _loadedAssets = new HashSet<string>();
        }

        private static readonly object RealManagerLocker = new object();
        private static RefCountedContentManager _realManager;    // Holds assets shared between all managers.
        private readonly HashSet<string> _loadedAssets;  // Shows assets loaded via this manager.

        /// <summary> Loads an asset  </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns></returns>
        public override T Load<T>(string assetName) {
            Contract.Requires(!string.IsNullOrEmpty(assetName));

            assetName = CustomAssets.CleanAssetPath(assetName);
            lock (_loadedAssets) {
                _loadedAssets.Add(assetName);
            }

            return _realManager.Load<T>(assetName);
        }

        public void Unload(string assetName) {
            Contract.Requires(!string.IsNullOrEmpty(assetName));

            assetName = CustomAssets.CleanAssetPath(assetName);
            lock (_loadedAssets) {
                if (_loadedAssets.Contains(assetName)) {
                    _realManager.Unload(assetName);
                }
            }
        }

        /// <summary> Unloads all assets loaded with this manager. </summary>
        public override void Unload() {
            lock (_loadedAssets) {
                foreach (var loadedAsset in _loadedAssets) {
                    _realManager.Unload(loadedAsset);
                }

                _loadedAssets.Clear();
            }

            base.Unload();
        }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            return _loadedAssets.Count.ToString(CultureInfo.InvariantCulture) + " assets loaded, with " + _realManager;
        }
    }
}