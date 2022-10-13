using System;
using System.Collections.Generic;

namespace RcContentManager
{
    /// <summary>Something that manages content loading/unloading.</summary>
    public interface IContentManager
    {
        /// <summary>Gets the service provider which can be used in asset loading.</summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>Gets a loaded asset with the given name.</summary>
        T? Find<T>(string assetName) where T : class, IAsset;

        /// <summary>Gets the currenly loaded assets.</summary>
        IEnumerable<IAsset> GatherCurrenlyLoadedAssets();

        /// <summary>Loads the given asset.</summary>
        void Load(IAsset asset);

        /// <summary>Unloads all assets.</summary>
        void Unload();

        /// <summary>Unloads asset with the given name.</summary>
        void Unload(string assetName);

        /// <summary>Unloads the given asset.</summary>
        void Unload(IAsset asset);
    }
}