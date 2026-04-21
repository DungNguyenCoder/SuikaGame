using LitMotion;
using LitMotion.Adapters;
using UnityEngine;

namespace Development.Utils
{
    public static class LitMotionPrewarm
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Prewarm()
        {
            MotionDispatcher.EnsureStorageCapacity<float, NoOptions, FloatMotionAdapter>(300);
            MotionDispatcher.EnsureStorageCapacity<Vector3, NoOptions, Vector3MotionAdapter>(300);
        }
    }
}