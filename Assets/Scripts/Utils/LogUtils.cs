using UnityEngine;

namespace Utils
{
    public static class Log
    {
        // ReSharper disable Unity.PerformanceAnalysis
        public static void Error(string message)
        {
            Debug.LogError(message);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static void Warn(string message)
        {
            Debug.LogWarning(message);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static void Info(string message)
        {
            Debug.Log(message);
        }
    }
}
