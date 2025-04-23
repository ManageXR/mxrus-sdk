using UnityEngine;

namespace MXRUS.SDK {
    /// <summary>
    /// Marks a 3D GameObject with a Renderer for showing a logo
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class MonoRendererLogoMarker : MonoBehaviour, ILogoMarker {
        [SerializeField] LogoMarkerType _logoMarkerType;
        [SerializeField] Texture2D _defaultLogo;

        public LogoMarkerType LogoMarkerType => _logoMarkerType;

        public Texture2D DefaultLogo => _defaultLogo;

        private Renderer _renderer;

        private void Awake() {
            _renderer = GetComponent<Renderer>();
            SetLogo(DefaultLogo);
        }

        public void SetLogo(Texture2D texture) {
            if (texture != null) {
                _renderer.material.mainTexture = texture;
                _renderer.material.color = Color.white;
            }
            else {
                if (DefaultLogo != null) {
                    _renderer.material.mainTexture = DefaultLogo;
                    _renderer.material.color = Color.white;
                }
                else {
                    _renderer.material.mainTexture = null;
                    _renderer.material.color = Color.clear;
                }
            }
        }
    }
}
