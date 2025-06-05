using UnityEngine;

namespace UnityFramework
{
    [CreateAssetMenu(menuName = "UnityFramework/AudioSetting", fileName = "AudioSetting")]
    public class AudioSetting : ScriptableObject
    {
        public AudioGroupConfig[] audioGroupConfigs = null;
    }
}