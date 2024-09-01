using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class VoiceTypeAnalyzer : MonoBehaviour
{
    public AudioPitchEstimator estimator;
    public AudioSource audioSource;

    [HorizontalLine]
    [ReadOnly] public string detectedVoiceType = "Unknown";
    [ReadOnly] public bool isCapturing = false; // Indicator to show if capturing is in progress

    [HorizontalLine]
    [ReadOnly] public List<float> recordedFrequencies = new List<float>(); // List to display recorded frequencies

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && !isCapturing)
        {
            StartCoroutine(CaptureVoiceType());
        }
    }

    private IEnumerator CaptureVoiceType()
    {
        isCapturing = true; // Indicate that the capturing has started
        float captureDuration = 5f; // Duration to capture frequencies
        float captureInterval = 1f; // Interval at which frequencies are sampled
        recordedFrequencies.Clear(); // Clear the list of recorded frequencies

        float elapsed = 0f;
        while (elapsed < captureDuration)
        {
            float currentFrequency = GetFrequency();

            if (!float.IsNaN(currentFrequency)) // Check if the frequency is valid
            {
                recordedFrequencies.Add(currentFrequency);
            }
            
            elapsed += captureInterval;
            yield return new WaitForSeconds(captureInterval);
        }

        detectedVoiceType = DetermineVoiceType(recordedFrequencies);
        isCapturing = false; // Indicate that the capturing has ended
    }

    private string DetermineVoiceType(List<float> frequencies)
    {
        if (frequencies.Count == 0)
        {
            return "Unknown"; // If no valid frequencies were recorded
        }

        // Calculate the average frequency
        float averageFrequency = 0f;
        foreach (float freq in frequencies)
        {
            averageFrequency += freq;
        }
        averageFrequency /= frequencies.Count;

        // Compare the average frequency against the configured ranges
        foreach (var config in maleFreq)
        {
            if (averageFrequency >= config.minFrequency && averageFrequency <= config.maxFrequency)
            {
                return config.typeName + " (Male)";
            }
        }

        foreach (var config in femaleFreq)
        {
            if (averageFrequency >= config.minFrequency && averageFrequency <= config.maxFrequency)
            {
                return config.typeName + " (Female)";
            }
        }

        return "Unknown";
    }
}
