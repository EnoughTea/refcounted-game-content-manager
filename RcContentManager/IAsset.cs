using System;

namespace RcContentManager
{
    /// <summary>
    ///     Represents a loadable content piece with a unique name.
    ///     Inheritors should provide a default constructor.
    /// </summary>
    public interface IAsset
    {
        /// <summary>Gets the unique asset name.</summary>
        public string Name { get; }

        /// <summary>Loads this asset using the specified services. Multiple load calls should be okay.</summary>
        /// <remarks>It is preferrable to handle 'normal' content loading exceptions inside this method.</remarks>
        void Load(IServiceProvider services);

        /// <summary>Unloads this asset using the specified services. Multiple unload calls should be okay.</summary>
        /// <remarks>It is preferrable to handle 'normal' content unloading exceptions inside this method.</remarks>
        void Unload(IServiceProvider services);
    }
}