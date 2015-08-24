using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MonoCustomCP {
    /// <summary>Content manager which can be used to preserve shared assets: an asset that is loaded for Level #1
    ///  and re-used in Level #2 can be kept in memory instead of being destroyed and reloaded.
    ///  Load Level #2 assets first, then unload Level #1 assets. </summary>
    public class SharedContentManager : ContentManager {
        /// <summary> Initializes a new instance of the <see cref="SharedContentManager"/> class. </summary>
        /// <param name="game">The game reference.</param>
        public SharedContentManager(Game game) : this(game.Services, game.Content.RootDirectory) {
        }

        /// <summary> Initializes a new instance of the <see cref="SharedContentManager"/> class. </summary>
        /// <param name="serviceProvider">The service provider, must be the same for all shared managers. </param>
        /// <param name="rootDirectory">Content root path, must be the same for all shared managers. </param>
        /// <exception cref="System.InvalidOperationException">By design, the class assumes that all your content
        ///  managers will have the same root path and use the same service provider.</exception>
        public SharedContentManager(IServiceProvider serviceProvider, string rootDirectory)
            : base(serviceProvider, rootDirectory) {
            if (_common == null) {
                _common = new RefCountedContentManager(serviceProvider, rootDirectory);
            } else if (_common.ServiceProvider != serviceProvider || _common.RootDirectory != rootDirectory) {
                throw new InvalidOperationException("By design, the class assumes that all your content managers " +
                    "will have the same root path and use the same service provider.");
            }

            _loadedAssets = new HashSet<string>();
        }

        private static RefCountedContentManager _common;    // Holds assets shared between all managers.
        private readonly HashSet<string> _loadedAssets;  // Shows assets loaded via this manager.

        public override T Load<T>(string assetName) {
            Contract.Requires(!String.IsNullOrEmpty(assetName));

            assetName = CustomContentPipeline.GetCleanPath(assetName);
            lock (_loadedAssets) {
                _loadedAssets.Add(assetName);
            }

            return _common.Load<T>(assetName);
        }

        public void Unload(string assetName) {
            Contract.Requires(!String.IsNullOrEmpty(assetName));

            assetName = CustomContentPipeline.GetCleanPath(assetName);
            lock (_loadedAssets) {
                if (_loadedAssets.Contains(assetName)) {
                    _common.Unload(assetName);
                }
            }
        }

        public override void Unload() {
            lock (_loadedAssets) {
                foreach (var loadedAsset in _loadedAssets) {
                    _common.Unload(loadedAsset);
                }

                _loadedAssets.Clear();
            }

            base.Unload();
        }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            return _loadedAssets.Count.ToString(CultureInfo.InvariantCulture) + " assets loaded, with " + _common;
        }
    }
}