using UnityEngine;
using UnityEngine.UI;

namespace MXRUS.SDK {
    /// <summary>
    /// Marks a UI GameObject with Image component for showing a logo
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class MonoImageLogoMarker : MonoBehaviour, ILogoMarker {
        [SerializeField] LogoMarkerType _logoMarkerType;
        [SerializeField] Sprite _defaultLogo;

        public LogoMarkerType LogoMarkerType => _logoMarkerType;

        public Texture2D DefaultLogo => _defaultLogo == null ? null : _defaultLogo.texture;

        private Image _image;

        private void Awake() {
            _image = GetComponent<Image>();
            SetLogo(DefaultLogo);
        }

        public void SetLogo(Texture2D texture) {
            if (texture != null) {
                _image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2);
                _image.color = Color.white;
            }
            else {
                if (DefaultLogo != null) {
                    _image.sprite = Sprite.Create(DefaultLogo, new Rect(0, 0, DefaultLogo.width, DefaultLogo.height), Vector2.one / 2);
                    _image.color = Color.white;
                }
                else {
                    _image.sprite = null;
                    _image.color = Color.clear;
                }
            }
        }
    }
}
