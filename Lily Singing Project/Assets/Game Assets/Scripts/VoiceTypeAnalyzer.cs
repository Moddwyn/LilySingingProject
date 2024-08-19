using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class VoiceTypeAnalyzer : MonoBehaviour
{
    public AudioPitchEstimator estimator;
    public AudioSource audioSource;

    [HorizontalLine]

    public List<VoiceTypeConfig> maleFreq = new List<VoiceTypeConfig>();
    public List<VoiceTypeConfig> femaleFreq = new List<VoiceTypeConfig>();

    [System.Serializable]
    public class VoiceTypeConfig
    {
        public string typeName;
        public float minFrequency;
        public float maxFrequency;
    }

    public float GetFrequency() => estimator.Estimate(audioSource);
}
