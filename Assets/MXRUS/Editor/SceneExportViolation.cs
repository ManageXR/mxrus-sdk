using UnityEngine;

namespace MXRUS.SDK.Editor {
    internal class SceneExportViolation {
        /// <summary>
        /// Enumerates all the difference kind of violations 
        /// a scene can have that can prevent or affect export.
        /// </summary>
        public enum Types {
            /// <summary>
            /// If the project is using an unsupported render pipeline.
            /// Only the Universal Rendering Pipeline is supported.
            /// </summary>
            UnsupportedRenderPipeline,

            /// <summary>
            /// If a material is using an unsupported shader.
            /// Only URP shaders and select in-built shaders are supported.
            /// </summary>
            UnsupportedShader,

            /// <summary>
            /// If a gameobject on the scene has a custom/user-authored script
            /// </summary>
            CustomScriptFound,

            /// <summary>
            /// If the scene has a camera we prevent export 
            /// </summary>
            CameraFound,

            /// <summary>
            /// If the scene has a realtime or mixed light. This doesn't block export
            /// but is used to show a warning about potential performance issues.
            /// </summary>
            NonBakedLight,

            /// <summary>
            /// If the scene has an EventSystem. The homescreen has its own and it doesn't
            /// allow another.
            /// </summary>
            EventSystemFound,

            /// <summary>
            /// If the scene doesn't have any user area provider
            /// </summary>
            NoUserAreaProviderFound,

            /// <summary>
            /// If the scene has multiple user area providers
            /// </summary>
            MultipleUserAreaProvidersFound,

            /// <summary>
            /// if the scene has an AudioListener 
            /// </summary>
            AudioListenerFound
        }

        /// <summary>
        /// The type of violation
        /// </summary>
        public Types Type { get; private set; }

        /// <summary>
        /// Whether this violation prevents exporting an mxrus file
        /// </summary>
        public bool PreventsExport { get; private set; }

        /// <summary>
        /// Description of the violation
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Reference to the object related to this violation
        /// </summary>
        public Object Object { get; private set; }

        public SceneExportViolation(Types type, bool preventsExport, string description, Object obj) {
            Type = type;
            PreventsExport = preventsExport;
            Description = description;
            Object = obj;
        }
    }
}