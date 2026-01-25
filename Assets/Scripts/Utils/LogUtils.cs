using UnityEngine;

namespace Utils
{
    public static class Log
    {
        public static void Error(string message)
        {
            Debug.LogError(message);
        }

        public static void Warn(string message)
        {
            Debug.LogWarning(message);
        }

        public static void Info(string message)
        {
            Debug.Log(message);
        }
    }
}
