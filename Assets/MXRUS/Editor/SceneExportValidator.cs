using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

namespace MXRUS.SDK.Editor {
    internal class SceneExportValidator : ISceneExportValidator {
        /// <summary>
        /// Returns a list of violations in the active scene
        /// </summary>
        public List<SceneExportViolation> Validate() {
            var violations = new List<SceneExportViolation>();

            var renderPipelineViolation = GetRenderPipelineViolation();
            if (renderPipelineViolation != null)
                violations.Add(renderPipelineViolation);

            violations.AddRange(GetShaderViolations());
            violations.AddRange(GetScriptViolations());
            violations.AddRange(GetCameraViolations());
            violations.AddRange(GetLightViolations());
            violations.AddRange(GetEventSystemViolations());

            var userAreaViolations = GetUserAreaViolations();
            if (userAreaViolations != null)
                violations.AddRange(GetUserAreaViolations());

            violations.AddRange(GetAudioListenerViolations());

            var sceneNameViolations = GetSceneNameViolation();
            if (sceneNameViolations != null)
                violations.Add(sceneNameViolations);

            return violations;
        }


        /// <summary>
        /// Checks and ensures the project not configured to use a render pipeline other than Universal Render Pipeline
        /// </summary>
        private SceneExportViolation GetRenderPipelineViolation() {
            var renderPipelineAsset = GraphicsSettings.defaultRenderPipeline;
            var violation = new SceneExportViolation(
                SceneExportViolation.Types.UnsupportedRenderPipeline,
                true,
                "Only Universal Render Pipeline is supported.",
                null
            );
            if (renderPipelineAsset != null && renderPipelineAsset.GetType().Name == "UniversalRenderPipelineAsset")
                return null;
            else
                return violation;
        }

        /// <summary>
        /// Checks and ensures the scene doesn't have materials that use unsupported shaders.
        /// Only shaders in the following namespaces/family are supported:
        /// - Error Shader. We allow this so that the export can be tested early without worrying about every material.
        /// - Universal Render Pipeline
        /// - Unlit
        /// - UI
        /// - Sprites
        /// - Skybox
        /// </summary>
        private List<SceneExportViolation> GetShaderViolations() {
            var dependencies = AssetDatabase.GetDependencies(new string[] {
                SceneManager.GetActiveScene().path
            });
            string[] supportedShaders = new string[] {
                "Universal Render Pipeline/",
                "Unlit/",
                "UI/",
                "Sprites/",
                "Skybox/",
                "Hidden/InternalErrorShader"
            };
            var unsupportedMaterials = dependencies
                .Where(x => x.EndsWith(".mat"))
                .Select(x => AssetDatabase.LoadAssetAtPath<Material>(x))
                .Where(x => {
                    // If the shader name matches any of the supported shaders, this material is supported
                    foreach (var supportedShader in supportedShaders) {
                        if (x.shader.name.StartsWith(supportedShader)) {
                            return false;
                        }
                    }
                    return true;
                });
            return unsupportedMaterials.Select(x => new SceneExportViolation(
                SceneExportViolation.Types.UnsupportedShader,
                false,
                "URP, Unlit, UI, Sprites, Skybox and official ManageXR shaders are recommended. " +
                "Other shaders may not run as expected.\n" +
                "Use them only if really required to achieve certain visuals and verify the results before deploying." +
                "These shaders may also increase the export size and they often can be replaced with the recommended ones.",
                x)).ToList();
        }

        /// <summary>
        /// Checks and ensures the scene doesn't use any custom scripts. 
        /// Only scripts in the following assemblies are supported:
        /// - Unity.TextMeshPro for TextMeshPro components
        /// - Unity.RenderPipelines.Universal.Runtime for URP components
        /// - UnityEngine.UI for Canvas and related components
        /// - MXRUS.Embeddings for MXRUS embedding components
        /// </summary>
        /// <returns></returns>
        private List<SceneExportViolation> GetScriptViolations() {
            var allowedAssemblies = new string[] {
                "Unity.TextMeshPro",
                "Unity.RenderPipelines.Universal.Runtime",
                "UnityEngine.UI",
                "MXRUS.Embeddings"
            };

            var unsupportedMonoBehaviours = Object.FindObjectsOfType<MonoBehaviour>()
                .Where(x => {
                    var assemblyName = x.GetType().Assembly.GetName().Name;
                    return !allowedAssemblies.Contains(assemblyName);
                });

            return unsupportedMonoBehaviours.Select(x => new SceneExportViolation(
                SceneExportViolation.Types.CustomScriptFound,
                true,
                "Custom scripts/components are not supported. Please remove or disable the gameobjects on the scene referencing them.",
                x
            ).SetAutoResolver("Custom scripts attached to GameObjects in the scene will be removed.", obj => {
                Undo.DestroyObjectImmediate(obj as MonoBehaviour);
            }))
            .ToList();
        }

        /// <summary>
        /// Checks and ensures the scene doesn't have a camera.
        /// </summary>
        /// <returns></returns>
        private List<SceneExportViolation> GetCameraViolations() {
            var cameras = Object.FindObjectsOfType<Camera>();
            return cameras.Select(x => new SceneExportViolation(
                SceneExportViolation.Types.CameraFound,
                true,
                "Scene cameras are not supported. Please remove cameras from the scene.",
                x
            )
            .SetAutoResolver("GameObjects in the scene with Camera component will be destroyed.", obj => {
                Undo.DestroyObjectImmediate((obj as Camera).gameObject);
            })
            ).ToList();
        }

        /// <summary>
        /// Checks and warns if there are realtime or mixed lights in the scene.
        /// These violations are a warning, not errors that prevents export.
        /// </summary>
        /// <returns></returns>
        private List<SceneExportViolation> GetLightViolations() {
            var nonBakedLights = Object.FindObjectsOfType<Light>()
                .Where(x => x.lightmapBakeType != LightmapBakeType.Baked).ToList();
            return nonBakedLights.Select(x => new SceneExportViolation(
                SceneExportViolation.Types.NonBakedLight,
                false,
                "Realtime and Mixed lights are not recommended. " +
                "Consider lightmapping your scene with baked lights. " +
                "Only use Realtime or Mixed lights if you truly need them " +
                "as they can impact performance.",
                x
            )).ToList();
        }

        /// <summary>
        /// Checks if there are AudioListener components in the scene. There can only
        /// be one AudioListener at a time and the ManageXR Homescreen already has one
        /// on its XR Rig.
        /// </summary>
        /// <returns></returns>
        private List<SceneExportViolation> GetAudioListenerViolations() {
            var audioListeners = Object.FindObjectsOfType<AudioListener>().ToList();
            return audioListeners.Select(x => new SceneExportViolation(
                SceneExportViolation.Types.AudioListenerFound,
                true,
                "The scene cannot have any AudioListeners. When running in the ManageXR " +
                "Homescreen, an AudioListener would already be present.",
                x
            ).SetAutoResolver("AudioListener components in the scene will be removed from their GameObjects.", obj => {
                Undo.DestroyObjectImmediate(obj as AudioListener);
            })
            ).ToList();
        }

        /// <summary>
        /// Checks and ensures the scene doesn't have an EventSystem
        /// </summary>
        /// <returns></returns>
        private List<SceneExportViolation> GetEventSystemViolations() {
            var eventSystems = Object.FindObjectsOfType<EventSystem>();
            return eventSystems.Select(x => new SceneExportViolation(
                SceneExportViolation.Types.EventSystemFound,
                true,
                "There cannot be an EventSystem on the scene. Please remove them from the scene.",
                x
            ).SetAutoResolver("GameObjects in the scene with EventSystem component will be destroyed.", obj => {
                Undo.DestroyObjectImmediate((obj as EventSystem).gameObject);
            })
            ).ToList();
        }

        /// <summary>
        /// Checks and ensures there is one MonoUserAreaProvider on the scene.
        /// </summary>
        /// <returns></returns>
        private List<SceneExportViolation> GetUserAreaViolations() {
            var userAreas = Object.FindObjectsOfType<MonoCircularUserAreaProvider>();
            if (userAreas.Length == 0)
                return new List<SceneExportViolation> {
                    new SceneExportViolation (
                        SceneExportViolation.Types.NoUserAreaProviderFound,
                        true,
                        "No user area provider found on the scene.",
                        null
                    ).SetAutoResolver("A new GameObject will be added to the scene with a user area provider.", obj =>{
                        var userAreaProvider = new GameObject("User Area Provider");
                        userAreaProvider.AddComponent<MonoCircularUserAreaProvider>();
                        Selection.activeGameObject = userAreaProvider;
                        SceneView.lastActiveSceneView.FrameSelected();
                        Undo.RegisterCreatedObjectUndo(userAreaProvider, "Created User Area Provider");
                    })
                };
            else if (userAreas.Length > 1) {
                return userAreas.Select(x => new SceneExportViolation(
                    SceneExportViolation.Types.MultipleUserAreaProvidersFound,
                    true,
                    "Multiple MonoUserAreaProvider components found on the scene. Ensure only one is present.",
                    x
                ))
                .ToList();
            }
            return null;
        }

        private SceneExportViolation GetSceneNameViolation() {
            List<string> reservedNames = new List<string> {
                "Launcher",
                "Video",
                "OculusQuest",
                "Pico",
                "Wave"
            };

            var activeScene = SceneManager.GetActiveScene();
            if(reservedNames.Contains(activeScene.name)) {
                return new SceneExportViolation(
                    SceneExportViolation.Types.SceneNameViolation,
                    true,
                    $"The scene name not allowed. The following names are prohibited: {string.Join(", ", reservedNames)}"
                );
            }
            return null;
        }
    }
}