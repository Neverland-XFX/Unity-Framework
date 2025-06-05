namespace UnityFramework
{
    internal class ResourceLogger : YooAsset.ILogger
    {
        public void Log(string message)
        {
            UnityFramework.Log.Info(message);
        }

        public void Warning(string message)
        {
            UnityFramework.Log.Warning(message);
        }

        public void Error(string message)
        {
            UnityFramework.Log.Error(message);
        }

        public void Exception(System.Exception exception)
        {
            UnityFramework.Log.Fatal(exception.Message);
        }
    }
}