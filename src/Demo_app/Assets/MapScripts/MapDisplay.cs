using UnityEngine;

namespace MapScripts
{
    public class MapDisplay : MonoBehaviour
    {
        public Renderer textureRenderer;
        private Vector3 _originalScale;
        private bool _isInitialized;
        private Texture2D _currentTexture;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (!textureRenderer)
            {
                textureRenderer = GetComponent<Renderer>();
            }

            if (!textureRenderer) return;
            _originalScale = transform.localScale;
            _isInitialized = true;

            SetupMaterial();
        }

        public void DrawTexture(Texture2D texture)
        {
            if (!_isInitialized)
            {
                Initialize();
                if (!_isInitialized) return;
            }

            if (!texture)
            {
                Debug.LogWarning("Null");
                return;
            }

            _currentTexture = texture;
            if (textureRenderer.material) 
            {
                textureRenderer.material.mainTexture = texture;
            }
            transform.localScale = new Vector3(texture.width, 1, texture.height);
            SetupMaterial();
            gameObject.SetActive(true);
        }

        private void SetupMaterial()
        {
            textureRenderer.sharedMaterial.mainTextureScale = Vector2.one;
            textureRenderer.sharedMaterial.mainTextureOffset = Vector2.zero;
        }

        public void ClearMap()
        {
            if (!_isInitialized) return;

            gameObject.SetActive(false);
            _currentTexture = null;

            if (textureRenderer.sharedMaterial)
            {
                textureRenderer.sharedMaterial.mainTexture = null;
            }

            transform.localScale = _originalScale;
        }

        public void SetVisibility(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return gameObject.activeSelf;
        }

        public Texture2D GetCurrentTexture()
        {
            return _currentTexture;
        }

        public void CenterMap()
        {
            transform.position = Vector3.zero;
        }

        public void ScaleMap(float scale)
        {
            if (_currentTexture != null)
            {
                transform.localScale = new Vector3(_currentTexture.width * scale, 1, _currentTexture.height * scale);
            }
            else if (textureRenderer.sharedMaterial != null && textureRenderer.sharedMaterial.mainTexture != null)
            {
                Texture2D texture = textureRenderer.sharedMaterial.mainTexture as Texture2D;
                if (texture != null)
                {
                    transform.localScale = new Vector3(texture.width * scale, 1, texture.height * scale);
                }
            }
        }

        public Vector2 GetMapSize()
        {
            return _currentTexture
                ? new Vector2(_currentTexture.width, _currentTexture.height)
                : Vector2.zero;
        }

        public void SetMapPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void RedrawCurrentTexture()
        {
            if (_currentTexture != null)
            {
                DrawTexture(_currentTexture);
            }
        }
    }
}