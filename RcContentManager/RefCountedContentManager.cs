using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcContentManager
{
    /// <summary>
    ///     Each <see cref="Load" /> increases asset refcount, each <see cref="Unload(IAsset)" /> /
    ///     <see cref="Unload(string)" /> decreases asset refcount. Asset will be disposed when
    ///     its refcount reaches zero or total <see cref="Unload()" /> will be called.
    /// </summary>
    /// <remarks>
    ///     Class is "thread-safe" as in it maintains internal state consistensy in individual operations,
    ///     such as loading or unloading assets from the different threads.
    /// </remarks>
    public class RefCountedContentManager : IContentManager
    {
        private readonly ConcurrentDictionary<string, AssetHandle> _assets;
        private readonly RefCounter<AssetHandle> _references;

        public RefCountedContentManager(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            _assets         = new ConcurrentDictionary<string, AssetHandle>();
            _references     = new RefCounter<AssetHandle>();
        }

        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; }

        /// <summary> Gets previously loaded asset without increasing its refcount. </summary>
        /// <typeparam name="T">Asset type.</typeparam>
        /// <param name="assetName">Asset name.</param>
        /// <returns>Found asset or null.</returns>
        /// <exception cref="ArgumentException">Asset name should not be null or empty</exception>
        public T? Find<T>(string assetName) where T : class, IAsset
        {
            if (string.IsNullOrEmpty(assetName))
                throw new ArgumentException("Asset name should not be null or empty", nameof(assetName));

            return _assets.TryGetValue(assetName, out var assetHandle)
                ? assetHandle.Asset as T
                : null;
        }

        /// <inheritdoc />
        public IEnumerable<IAsset> GatherCurrenlyLoadedAssets() => _assets.Select(_ => _.Value.Asset);

        /// <summary>
        ///     Loads the given asset or already loaded asset using the specified asset name.
        ///     Increases this asset refcount.
        /// </summary>
        /// <param name="asset">Asset to load.</param>
        /// <returns>true if asset is loaded; false otherwise.</returns>
        /// <exception cref="System.ArgumentException">Asset name should not be null or empty</exception>
        public void Load(IAsset asset)
        {
            if (string.IsNullOrEmpty(asset.Name))
                throw new ArgumentException("Asset name should not be null or empty", nameof(asset));

            var assetHandle = _assets.GetOrAdd(asset.Name, _ => new AssetHandle(asset));
            lock (assetHandle) {
                try {
                    assetHandle.Load(ServiceProvider);
                    _references.Retain(assetHandle);
                }
                catch (Exception) {
                    _assets.TryRemove(asset.Name, out _);
                    _references.Clear(assetHandle);
                    throw;
                }
            }
        }

        /// <summary> Immediately unloads all loaded assets regardless of their refcount. </summary>
        public void Unload()
        {
            var handlesToClear = _assets.Values.ToArray();
            foreach (var assetHandle in handlesToClear)
                lock (assetHandle) {
                    assetHandle.Unload(ServiceProvider);
                    _references.Clear(assetHandle);
                    _assets.TryRemove(assetHandle.Asset.Name, out _);
                }
        }

        /// <summary>
        ///     Decreases refcount for the asset with the specified name.
        ///     When refcount hits zero, asset is unloaded.
        /// </summary>
        /// <param name="assetName">File path of the asset to unload.</param>
        /// <exception cref="ArgumentException">Asset name should not be null or empty</exception>
        public void Unload(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                throw new ArgumentException("Asset name should not be null or empty", nameof(assetName));

            if (!_assets.TryGetValue(assetName, out var assetHandle)) return;

            lock (assetHandle) {
                bool lastReference = _references.Count(assetHandle) == 1;
                if (lastReference) {
                    assetHandle.Unload(ServiceProvider);
                    _assets.TryRemove(assetName, out _);
                }

                _references.Release(assetHandle);
            }
        }

        /// <summary>
        ///     Decreases refcount for the given asset. When refcount hits zero, asset is unloaded.
        /// </summary>
        /// <param name="asset">Asset to unload.</param>
        public void Unload(IAsset asset) => Unload(asset.Name);

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() =>
            $"{_assets.Count.ToString(CultureInfo.InvariantCulture)} loaded assets";
    }

    internal class AssetHandle
    {
        public AssetHandle(IAsset asset) => Asset = asset;

        public IAsset Asset { get; }

        public bool IsLoaded { get; set; }

        public void Load(IServiceProvider services)
        {
            if (!IsLoaded) {
                Asset.Load(services);
                IsLoaded = true;
            }
        }

        public void Unload(IServiceProvider services)
        {
            if (IsLoaded) {
                Asset.Unload(services);
                IsLoaded = false;
            }
        }
    }
}