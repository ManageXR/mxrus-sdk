using UnityEngine;

namespace MXRUS.SDK {
    /// <summary>
    /// Metadata embedding for the user area in an MXRUS environment
    /// </summary>
    interface IUserAreaProvider {
        /// <summary>
        /// The start position of the user in this environment
        /// </summary>
        Vector3 UserStartPosition { get; }

        /// <summary>
        /// The start rotation (direction) of the user in this environment.
        /// </summary>
        Quaternion UserStartRotation { get; }
    }
}
