using UnityEngine;
using UnityEngine.VFX;

#if UNITY_EDITOR
#endif


namespace Neeto
{
    public class VFXVolumeGizmo : MonoBehaviour
	{
		[GetComponent] public VisualEffect vfx;

        public string volumeVariableName = "Volume";
        public string particleSizeVariableName = "ParticleSize";

        public Color gizmoColor = Color.cyan;

        [SyncedProperty(nameof(Volume))]
        public Vector3 _volume;
        public Vector3 Volume
        {
            get => vfx.GetVector3(volumeVariableName);
            set => vfx.SetVector3(volumeVariableName, value);
        }

        [SyncedProperty(nameof(ParticleSize))]
        public float _particleSize;
        public float ParticleSize
        {
            get => vfx.GetFloat(particleSizeVariableName);
            set => vfx.SetFloat(particleSizeVariableName, value);
        }

        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            var _volume = this.Volume;
            _volume += Vector3.one * ParticleSize;

            var _gizmoColor = gizmoColor;

            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireCube(transform.position, _volume);

            _gizmoColor.a = .1f;
            Gizmos.color = _gizmoColor;
            Gizmos.DrawCube(transform.position, _volume);
        }

        private void Start()
        {
            
        }
    }
}