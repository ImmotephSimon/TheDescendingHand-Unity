using GameKit.Dependencies.Utilities;
using UnityEngine;

namespace StableFluids.Marbling {

    public class MarblingController : MonoBehaviour
    {
        [SerializeField] RenderTexture _colorInjection = null;
        [SerializeField] RenderTexture _forceField = null;
        [SerializeField, HideInInspector] Shader _shader = null;
        [SerializeField] CustomRenderTexture _canvasRT;

        public float PointForce { get; set; } = 300;
        public float PointFalloff { get; set; } = 75;
        public Vector2 origin = new(0f, -0.2f);
        public CustomRenderTexture Canvas => _canvasRT;

        private float nextForceTime;
        private float forceHz = 15f;
        private Material _material;

        void Start()
        {
            _material = new Material(_shader);
            _material.SetFloat("_Aspect", (float)_forceField.width / _forceField.height);
            Graphics.Blit(Texture2D.blackTexture, _colorInjection);
            Graphics.Blit(Texture2D.blackTexture, _forceField);
        }

        void OnDestroy()
          => Destroy(_material);

        void Update()
        {
            UpdateColorInjection();
            UpdateForceField();
            _canvasRT.Update();
        }

        void UpdateColorInjection()
        {
            if (Time.time >= nextForceTime)
            {
                //_material.color = Color.HSVToRGB(Time.time % 1, 1, 1);
                _material.color = Color.Lerp(
                    new Color(1f, 0.1f, 0.01f),
                    new Color(1f, 0.8f, 0.05f),
                    Random.value
                );
                _material.SetVector("_Origin", origin);
                _material.SetFloat("_Falloff", PointFalloff);
                Graphics.Blit(null, _colorInjection, _material, 0);
            }
            else
            {
                Graphics.Blit(Texture2D.blackTexture, _colorInjection);
            }
        }


        void UpdateForceField()
        {
            float interval = 1f / forceHz;

            if (Time.time >= nextForceTime)
            {
                nextForceTime = Time.time + Random.Range(
                    0.5f / forceHz,
                    1.5f / forceHz
                );

                BlitToForceField(new Vector2(
                    Random.Range(-1f, 1f) * PointForce * 0.025f,
                    Random.Range(0.2f, 0.6f) * PointForce * 0.025f
                ));
            }
            else
            {
                Graphics.Blit(Texture2D.blackTexture, _forceField);
            }
        }

        void BlitToForceField(Vector2 force)
        {
            _material.SetVector("_Origin", origin);
            _material.SetFloat("_Falloff", PointFalloff);
            _material.SetVector("_Force", force);
            Graphics.Blit(null, _forceField, _material, 1);
        }
    }
}