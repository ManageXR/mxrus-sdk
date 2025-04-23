namespace MXRUS.SDK {
    /// <summary>
    /// The different states the loader can be in
    /// </summary>
    public enum SceneLoaderState {
        /// <summary>
        /// The instance is awaiting load operation.
        /// This is the state the instance starts in.
        /// On invoking <see cref="Unload"/>, the instance resets back to this state.
        /// </summary>
        Idle,

        /// <summary>
        /// The instance is currently loading an mxrus file.
        /// </summary>
        Loading,

        /// <summary>
        /// The instance failed to load an mxrus file. <see cref="_bundles"/> is empty.
        /// </summary>
        Error,

        /// <summary>
        /// The instance has successfully load an mxrus file and all asset bundles are available.
        /// </summary>
        Success
    }
}