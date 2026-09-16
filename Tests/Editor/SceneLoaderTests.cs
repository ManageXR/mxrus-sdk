using System.Collections;
using System.IO;
using System.Threading.Tasks;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace MXRUS.SDK.Tests {
    /// <summary>
    /// Tests for <see cref="SceneLoader"/> when the mxrus file does not hold every asset bundle.
    /// </summary>
    public class SceneLoaderTests {
        private const string UNITY_GENERATED_ASSET_BUNDLE_EXT = ".unitygenerated";
        private const string TEMP_EXTRACT_DIRNAME_POSTFIX = "-extract";
        private const float LOAD_TIMEOUT_SECONDS = 30f;

        private string _workingDirPath;
        private string _extractLocation;

        [SetUp]
        public void SetUp() {
            _workingDirPath = Path.Combine(Path.GetTempPath(), "mxrus-sceneloader-tests-" + Path.GetRandomFileName());
            _extractLocation = Path.Combine(_workingDirPath, "extracts");
            Directory.CreateDirectory(_extractLocation);
        }

        [TearDown]
        public void TearDown() {
            LogAssert.ignoreFailingMessages = false;

            if (Directory.Exists(_workingDirPath))
                Directory.Delete(_workingDirPath, recursive: true);
        }

        /// <summary>
        /// Regression test for the SceneLoader crash reported in Sentry as CUSTOM-LAUNCHER-30M.
        /// The loader found the Unity generated bundle while it built the list of bundle names.
        /// That step runs before the error handling in Load, so an mxrus file without that bundle
        /// made Load throw InvalidOperationException. The caller got no result, the state stayed
        /// at Loading and the extract directory stayed on disk.
        /// </summary>
        [UnityTest]
        public IEnumerator Load_MxrusFileWithoutUnityGeneratedBundle_ReportsErrorAndDeletesExtractDirectory() {
            // The loader reports every missing bundle as an error, and the test files are not
            // real asset bundles, so Unity reports an error for each one. Those errors are the
            // expected result here. Do not let them fail the test.
            LogAssert.ignoreFailingMessages = true;

            string mxrusFilePath = CreateMxrusFileWithoutUnityGeneratedBundle("no-unity-generated");
            string extractDirPath = Path.Combine(
                _extractLocation,
                Path.GetFileNameWithoutExtension(mxrusFilePath) + TEMP_EXTRACT_DIRNAME_POSTFIX
            );

            var loader = new SceneLoader();
            Task<bool> loadTask = loader.Load(mxrusFilePath, _extractLocation);
            yield return WaitUntilCompleted(loadTask);

            Assert.IsFalse(
                loadTask.IsFaulted,
                "Load must not throw when the Unity generated bundle is absent. It threw: " + loadTask.Exception
            );
            Assert.IsFalse(loadTask.Result, "Load must return false when the Unity generated bundle is absent.");
            Assert.AreEqual(SceneLoaderState.Error, loader.State, "Load must set the state to Error.");

            // This is the assertion that shows the failure went through the error handling in Load.
            // The loader deletes the extract directory only on that path.
            Assert.IsFalse(
                Directory.Exists(extractDirPath),
                "Load must delete the extract directory " + extractDirPath + ". A directory left behind " +
                "means the failure escaped the error handling in Load."
            );
        }

        /// <summary>
        /// Writes an mxrus file that holds the assets bundle and the scene bundle but no bundle
        /// with the <see cref="UNITY_GENERATED_ASSET_BUNDLE_EXT"/> extension.
        /// </summary>
        private string CreateMxrusFileWithoutUnityGeneratedBundle(string fileName) {
            string contentDirPath = Path.Combine(_workingDirPath, fileName + "-content");
            Directory.CreateDirectory(contentDirPath);

            // The content of these files does not matter. The loader must fail on the absent
            // Unity generated bundle, and it must fail in the same way it fails for any bundle.
            File.WriteAllText(Path.Combine(contentDirPath, "assets"), "not an asset bundle");
            File.WriteAllText(Path.Combine(contentDirPath, "scene"), "not an asset bundle");

            Assert.IsEmpty(
                Directory.GetFiles(contentDirPath, "*" + UNITY_GENERATED_ASSET_BUNDLE_EXT),
                "This test needs an mxrus file without a Unity generated bundle."
            );

            string mxrusFilePath = Path.Combine(_workingDirPath, fileName + ".mxrus");
            new SharpZipLibCompressionUtility().CompressDirectory(contentDirPath, mxrusFilePath);
            return mxrusFilePath;
        }

        private IEnumerator WaitUntilCompleted(Task task) {
            float deadline = Time.realtimeSinceStartup + LOAD_TIMEOUT_SECONDS;
            while (!task.IsCompleted) {
                if (Time.realtimeSinceStartup > deadline) {
                    Assert.Fail("Load did not complete within " + LOAD_TIMEOUT_SECONDS + " seconds.");
                    yield break;
                }
                yield return null;
            }
        }
    }
}
