using UnityEngine;

namespace MXRUS.SDK {
    /// <summary>
    /// Marks an object to show a logo on
    /// </summary>
    public interface ILogoMarker {
        /// <summary>
        /// The logo type to be shown
        /// </summary>
        LogoMarkerType LogoMarkerType { get; }

        /// <summary>
        /// The default logo shown when the logo set via <see cref="SetLogo(Texture2D)"/> is null
        /// </summary>
        Texture2D DefaultLogo { get; }

        /// <summary>
        /// Sets the logo Texture2D on the object.
        /// If the passed texture is null
        /// - <see cref="DefaultLogo"/> is shown, if available
        /// - Otherwise the marker shows no logo
        /// </summary>
        /// <param name="texture"></param>
        void SetLogo(Texture2D texture);
    }
}
