using System.Threading.Tasks;

using UnityEngine;

namespace MXRUS.SDK {
    /// <summary>
    /// Allows loading an mxrus file into memory. 
    /// </summary>
    public interface ISceneLoader {
        /// <summary>
        /// The current state of the loader
        /// </summary>
        SceneLoaderState State { get; }

        /// <summary>
        /// The name of the scene inside the <see cref="SCENE_ASSETBUNDLE_NAME"/> AssetBundle in the mxrus file
        /// </summary>
        string SceneName { get; }

        /// <summary>
        /// The AssetBundle containing the assets packages in the mxrus file
        /// </summary>
        AssetBundle Assets { get; }

        /// <summary>
        /// Asynchronously loads an mxrus file
        /// </summary>
        /// <param name="sourceFilePath">Path to the mxrus file to load</param>
        /// <param name="extractLocation">Directory used to extract the mxrus file temporarily</param>
        /// <returns></returns>
        Task<bool> Load(string sourceFilePath, string extractLocation);

        /// <summary>
        /// Unloads the AssetBundles of an mxrus file that may have been loaded previously.
        /// Also resets the internal state of the loader.
        /// </summary>
        void Unload();
    }
}
