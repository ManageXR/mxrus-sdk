using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using System.Linq;

namespace MXRUS.SDK {
    public class SceneLoader : ISceneLoader {
        private const string TAG = "SceneLoader";
        private const string ASSETS_ASSETBUNDLE_NAME = "assets";
        private const string SCENE_ASSETBUNDLE_NAME = "scene";
        private const string UNITY_GENERATED_ASSET_BUNDLE_EXT = ".unitygenerated";
        private const string TEMP_EXTRACT_DIRNAME_POSTFIX = "-extract";

        private readonly Dictionary<string, AssetBundle> _bundles = new Dictionary<string, AssetBundle>();

        /// <summary>
        /// The global folder where .mxrus files will be extracted by default
        /// </summary>
        public static string DefaultExtractsLocation { 
            get {
                if (!string.IsNullOrEmpty(_defaultExtractsLocation))
                    return _defaultExtractsLocation;
#if UNITY_EDITOR
                _defaultExtractsLocation = Application.dataPath.Replace("Assets", "Temp");
#else
                _defaultExtractsLocation = Application.persistentDataPath;
#endif
                return _defaultExtractsLocation;
            }
            set {
                if (string.IsNullOrEmpty(value))
                    throw new Exception("Cannot set DefaultExtractsLocation to null/empty");
                _defaultExtractsLocation = value;
            }
        }
        private static string _defaultExtractsLocation = null;

        public SceneLoaderState State { get; private set; } = SceneLoaderState.Idle;

        public string SceneName {
            get {
                if (!_bundles.ContainsKey(SCENE_ASSETBUNDLE_NAME)) {
                    Debug.unityLogger.Log(LogType.Error, TAG, "scene asset bundle not loaded");
                    return null;
                }

                var sceneBundle = _bundles[SCENE_ASSETBUNDLE_NAME];
                var scenePaths = sceneBundle.GetAllScenePaths();
                if (scenePaths.Length == 0) {
                    Debug.unityLogger.Log(LogType.Error, TAG, "There are no scenes in scene bundle");
                    return null;
                }
                else if (scenePaths.Length > 1)
                    Debug.unityLogger.Log(LogType.Warning, TAG, "There are multiple scenes in scene bundle. " +
                        "Only the name of the scene at index 0 will be returned.");

                var scenePath = sceneBundle.GetAllScenePaths()[0];
                if (string.IsNullOrEmpty(scenePath))
                    return null;
                return Path.GetFileNameWithoutExtension(scenePath);
            }
        }

        public AssetBundle Assets =>
            _bundles.ContainsKey(ASSETS_ASSETBUNDLE_NAME) ? _bundles[ASSETS_ASSETBUNDLE_NAME] : null;


        public async Task<bool> Load(string sourceFilePath, string extractLocation = null) {
            // Determine extract location and ensure it exists
            extractLocation = string.IsNullOrEmpty(extractLocation) ? DefaultExtractsLocation : extractLocation;
            if (!Directory.Exists(extractLocation))
                Directory.CreateDirectory(extractLocation);

            UnloadBundles();
            State = SceneLoaderState.Loading;

            Debug.unityLogger.Log(LogType.Log, TAG, $"Loading {sourceFilePath}");

            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(sourceFilePath);
            string extractDirName = fileNameWithoutExt + TEMP_EXTRACT_DIRNAME_POSTFIX;
            string extractDirPath = Path.Combine(extractLocation, extractDirName);


            // Extract the file to destination path
            Debug.unityLogger.Log(LogType.Log, TAG, $"Extracting {sourceFilePath} to {extractDirPath}");
            ICompressionUtility compressionUtility = new SharpZipLibCompressionUtility();
            compressionUtility.ExtractToDirectory(sourceFilePath, extractDirPath);

            // Attempt to load the bundles from the extract directory
            var bundleNames = new string[] { 
                ASSETS_ASSETBUNDLE_NAME, 
                SCENE_ASSETBUNDLE_NAME, 
                GetUnityGeneratedBundleName(extractDirPath)
            };
            Debug.unityLogger.Log(LogType.Log, TAG, $"Attempting to load the following asset bundles: {string.Join(", ", bundleNames)}");

            List<string> failedBundleNames = new List<string>();
            foreach (var bundleName in bundleNames) {
                try {
                    var loadedBundle = await LoadAssetBundleAsync(Path.Combine(extractDirPath, bundleName));
                    _bundles.Add(bundleName, loadedBundle);
                    Debug.unityLogger.Log(LogType.Log, TAG, $"Added {bundleName} to Bundles Dictionary");
                }
                catch {
                    failedBundleNames.Add(bundleName);
                    Debug.unityLogger.Log(LogType.Error, TAG, $"Failed to load AssetBundle {bundleName}");
                }
            }

            // Regardless of whether any bundles failed to load, always delete the extract directory
            if (Directory.Exists(extractDirPath)) {
                Directory.Delete(extractDirPath, recursive: true);
            }

            if (failedBundleNames.Count == 0) {
                State = SceneLoaderState.Success;
                return true;
            }
            else {
                UnloadBundles();
                State = SceneLoaderState.Error;
                var msg = $"Failed to load the following asset bundles: {string.Join(", ", failedBundleNames)}";
                Debug.unityLogger.Log(LogType.Error, TAG, msg);
                return false;
            }
        }

        public void Unload() {
            UnloadBundles();
            State = SceneLoaderState.Idle;
        }

        private void UnloadBundles() {
            foreach (var pair in _bundles) {
                pair.Value.Unload(true);
            }
            _bundles.Clear();
        }

        private Task<AssetBundle> LoadAssetBundleAsync(string path) {
            var source = new TaskCompletionSource<AssetBundle>();
            var loadRequest = AssetBundle.LoadFromFileAsync(path);
            loadRequest.completed += operation => {
                if (operation.isDone && loadRequest.assetBundle != null) {
                    source.SetResult(loadRequest.assetBundle);
                }
                else {
                    source.SetException(new Exception("Failed to load asset bundle " + path));
                }
            };
            return source.Task;
        }

        // Unity generates an additional asset bundle when we export the mxrus file
        // During export, this file is renamed to have a custom extension that can be used
        // to find it.
        private string GetUnityGeneratedBundleName(string directoryPath) {
            return Directory.GetFiles(directoryPath, "*", SearchOption.TopDirectoryOnly)
                .First(x => Path.GetExtension(x).Equals(UNITY_GENERATED_ASSET_BUNDLE_EXT));
        }
    }
}