using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// Creates the animation curve for drive torque curve.
    /// </summary>
    public struct AnimationCurveBlob
    {
        private BlobArray<float> m_Keys;
        private float m_Length;
        private float m_KeyCount;

        // When t exceeds the curve time, repeat it
        public float CalculateNormalizedTime(float t)
        {
            var normalizedT = t * m_Length;
            return normalizedT - math.floor(normalizedT);
        }

        public float Evaluate(float t)
        {
            // Loops time value between 0...1
            t = math.clamp(t, 0, 1);

            // Find index and interpolation value in the array
            var sampleT = t * m_KeyCount;
            var sampleTFloor = math.floor(sampleT);

            var interpolation = sampleT - sampleTFloor;
            var index = (int)math.clamp(sampleTFloor, 0, m_KeyCount - 1);

            return math.lerp(m_Keys[index], m_Keys[index + 1], interpolation);
        }

        public static BlobAssetReference<AnimationCurveBlob> CreateBlob(AnimationCurve curve, Allocator allocator,
            Allocator allocatorForTemp = Allocator.TempJob)
        {
            using var blob = new BlobBuilder(allocatorForTemp);
            ref var anim = ref blob.ConstructRoot<AnimationCurveBlob>();
            int keyCount = 20;

            var endTime = curve[curve.length - 1].time;
            anim.m_Length = 1.0F / endTime;
            anim.m_KeyCount = keyCount;

            var array = blob.Allocate(ref anim.m_Keys, keyCount + 1);
            for (var i = 0; i < keyCount; i++)
            {
                var t = (float)i / (keyCount - 1) * endTime;
                array[i] = curve.Evaluate(t);
            }

            array[keyCount] = array[keyCount - 1];

            return blob.CreateBlobAssetReference<AnimationCurveBlob>(allocator);
        }
    }
}