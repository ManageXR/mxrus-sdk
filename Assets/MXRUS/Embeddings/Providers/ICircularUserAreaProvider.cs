using UnityEngine;

namespace MXRUS.SDK {
    /// <summary>
    /// Metadata embedding for a circular user area in an MXRUS environment
    /// </summary>
    interface ICircularUserAreaProvider {
        /// <summary>
        /// The distance the user is allowed to walk in, relative to <see cref="UserStartPosition"/>
        /// </summary>
        float UserWalkableRadius { get; }
    }
}
