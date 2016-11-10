using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using Microsoft.Xna.Framework.Content;

namespace MonoCustomContentManagers
{
    /// <summary> Each <see cref="Load{T}"/> increases asset refcount, each <see cref="Unload(string)"/>
    ///  decreases asset refcount. Asset will be disposed when its refcount reaches 0 or total
    ///  <see cref="Unload()"/> will be called. </summary>
    /// <remarks>Class is "thread-safe" as in it maintains internal state consistently in individual operations.
    /// You still should lock when you need logical consistency maintained across multiple operations in a sequence,
    ///  e.g.loading an asset from one thread while unloading an asset from another.
    /// </remarks>
    public class RefCountedContentManager : ContentManager
    {
        /// <summary> Initializes a new instance of the <see cref="RefCountedContentManager" /> class. </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="rootDirectory">Content root.</param>
        public RefCountedContentManager(IServiceProvider serviceProvider, string rootDirectory)
            : base(serviceProvider, rootDirectory)
        {
            _assets = new ConcurrentDictionary<string, AssetHandle>();
            _references = new RefCounter<AssetHandle>();
            _references.Released += commonAsset => {
                if (_assets.TryRemove(commonAsset.Name, out commonAsset)) {
                    var customDisposal = CustomAssets.FindUnload(commonAsset.Asset?.GetType());
                    customDisposal?.Invoke(this, commonAsset.Asset, commonAsset.Name);
                    commonAsset.Dispose(); // Possible multiple dispose calls should be okay.
                }
            };
        }

        /// <summary> Asset holder, uses asset name as a key. </summary>
        private readonly ConcurrentDictionary<string, AssetHandle> _assets;

        /// <summary> Reference counting for common assets. </summary>
        private readonly RefCounter<AssetHandle> _references;

        /// <summary> Gets previously loaded asset without increasing its refcount. </summary>
        /// <typeparam name="T">Asset type.</typeparam>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns>Found asset or null.</returns>
        /// <exception cref="ArgumentException">Asset name should not be null or empty</exception>
        public T Find<T>(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) {
                throw new ArgumentException("Asset name should not be null or empty", nameof(assetName));
            }

            assetName = CustomAssets.CleanAssetPath(assetName);
            var found = default(T);
            AssetHandle assetHandle;
            if (_assets.TryGetValue(assetName, out assetHandle)) {
                found = (T)assetHandle.Asset;
            }

            return found;
        }

        /// <summary> Loads asset or gets already loaded asset using the specified asset name.
        ///  Increases asset refcount. </summary>
        /// <typeparam name="T">Asset type.</typeparam>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns>Loaded or found asset.</returns>
        /// <exception cref="ArgumentException">Null or empty asset name.</exception>
        public override T Load<T>(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) {
                throw new ArgumentException("Null or empty asset name.", nameof(assetName));
            }

            assetName = CustomAssets.CleanAssetPath(assetName);
            object loadedAsset = null;
            _assets.AddOrUpdate(assetName,
                                _ => {
                                    var freshAsset = new AssetHandle {
                                        Asset = CustomLoad<T>(assetName),
                                        Name = assetName
                                    };
                                    loadedAsset = freshAsset.Asset;
                                    _references.Retain(freshAsset);
                                    return freshAsset;
                                },
                                (_, existingAsset) => {
                                    loadedAsset = existingAsset.Asset;
                                    _references.Retain(existingAsset);
                                    return existingAsset;
                                });

            return (T)loadedAsset;
        }

        /// <summary> Unloads all loaded assets regardless of their refcount. </summary>
        public override void Unload()
        {
            // Clear references first, since reference release logic uses asset map.
            _references.Clear();
            _assets.Clear();
        }

        /// <summary> Unloads asset with the specified asset name. Decreases asset refcount. </summary>
        /// <param name="assetName">Name of the asset to unload.</param>
        /// <exception cref="ArgumentException">Asset name should not be null or empty</exception>
        public void Unload(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) {
                throw new ArgumentException("Asset name should not be null or empty", nameof(assetName));
            }

            assetName = CustomAssets.CleanAssetPath(assetName);
            AssetHandle assetHandle;
            if (_assets.TryGetValue(assetName, out assetHandle)) {
                _references.Release(assetHandle);
            }
        }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString()
        {
            return _assets.Count.ToString(CultureInfo.InvariantCulture) + " shared assets";
        }

        /// <summary> Loads asset either via custom load method for an asset or built-in
        ///  <see cref="ContentManager.ReadAsset{T}"/>. </summary>
        private object CustomLoad<T>(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) {
                throw new ArgumentException("Asset name should not be null or empty", nameof(assetName));
            }

            object asset;
            var customLoad = CustomAssets.FindLoad(typeof(T));
            if (customLoad != null) {
                // There is a custom load method for current asset type.
                asset = customLoad(this, assetName);
                if (asset == null) {
                    throw new ContentLoadException($"Custom loading failed for asset '{assetName}'");
                }
            }
            else {
                // We are loading a standard asset.
                asset = ReadAsset<T>(assetName, _ => { });
            }

            return asset;
        }

        private class AssetHandle
        {
            public object Asset;
            public string Name;

            public void Dispose()
            {
                var asset = Interlocked.Exchange(ref Asset, null);
                (asset as IDisposable)?.Dispose();
            }

            /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
            /// <returns> A <see cref="string" /> that represents this instance. </returns>
            public override string ToString()
            {
                return Asset != null ? $"h '{Name}'" : $"h '{Name}' (null)";
            }
        }
    }
}