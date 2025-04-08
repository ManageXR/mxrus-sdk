using System.Collections.Generic;

namespace MXRUS.SDK.Editor {
    public interface ISceneExportValidator {
        List<SceneExportViolation> Validate();
    }
}
