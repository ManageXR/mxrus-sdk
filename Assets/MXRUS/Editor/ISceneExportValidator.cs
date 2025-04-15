using System.Collections.Generic;

namespace MXRUS.SDK.Editor {
    internal interface ISceneExportValidator {
        List<SceneExportViolation> Validate();
    }
}
