using UnityEngine;

namespace Neeto
{
    public struct Phase
    {
        public float offset;
        public float duration;

        public static Phase FromDuration(float duration)
        {
            return new Phase
            {
                duration = duration,
            };
        }
        public static Phase FromClip(AnimationClip clip)
        {
            return new Phase
            {
                duration = clip.length,
            };
        }
    }
    public static partial class PhaseExtensions
    {
        public static Phase WithOffset(this Phase ts, float offset)
        {
            return new Phase
            {
                duration = ts.duration,
                offset = offset
            };
        }

    }
}