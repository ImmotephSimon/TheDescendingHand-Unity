using UnityEngine;

namespace StableFluids.Marbling
{
    public sealed class MarblingFluidSimulator : MonoBehaviour
    {
        [SerializeField] RenderTexture _velocityField;
        [SerializeField] RenderTexture _forceField;
        [SerializeField] Shader _kernelShader;

        [SerializeField] float Viscosity = 1e-6f;

        FluidSimulation _simulation;

        private void Awake()
        {
            _simulation = new FluidSimulation(_velocityField, _kernelShader);
        }
        void Start()
        {
            _simulation.ClearVelocityField();
        }

        void Update()
        {
            _simulation.Viscosity = Viscosity;

            _simulation.PreStep();
            _simulation.ApplyForceField(_forceField);
            _simulation.PostStep();
        }

        void OnDestroy()
        {
            _simulation?.Dispose();
        }
    }
}