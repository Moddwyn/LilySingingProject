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

    public IEnumerator AnalyzeVoiceType()
    {
        // Start recording
        audioSource.clip = Microphone.Start(null, true, 5, AudioSettings.outputSampleRate);
        audioSource.loop = false;

        print("Voice Type Analyzer Started");

        // Wait for 5 seconds
        yield return new WaitForSeconds(5f);

        // Stop recording
        Microphone.End(null);

        print("Voice Type Analyzer Finished");

        // Analyze the recorded clip
        audioSource.Play();
        yield return StartCoroutine(EstimateVoiceType());
    }

    IEnumerator EstimateVoiceType()
    {
        List<float> frequencies = new List<float>();

        // Collect frequencies for 5 seconds
        float elapsedTime = 0f;
        while (elapsedTime < 5f)
        {
            float frequency = estimator.Estimate(audioSource);
            if (!float.IsNaN(frequency))
            {
                frequencies.Add(frequency);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Calculate average frequency
        if (frequencies.Count > 0)
        {
            float averageFrequency = 0f;
            foreach (float freq in frequencies)
            {
                averageFrequency += freq;
            }
            averageFrequency /= frequencies.Count;

            // Determine the voice type based on the average frequency
            detectedVoiceType = DetermineVoiceType(averageFrequency);
        }
        else
        {
            detectedVoiceType = "Unknown";
        }

        print("Voice Type Detected: " + detectedVoiceType);
    }

    string DetermineVoiceType(float averageFrequency)
    {
        // Check male frequencies
        foreach (var config in maleFreq)
        {
            if (averageFrequency >= config.minFrequency && averageFrequency <= config.maxFrequency)
            {
                return config.typeName;
            }
        }

        // Check female frequencies
        foreach (var config in femaleFreq)
        {
            if (averageFrequency >= config.minFrequency && averageFrequency <= config.maxFrequency)
            {
                return config.typeName;
            }
        }

        return "Unknown";
    }

    public float GetFrequency() => estimator.Estimate(audioSource);

    public VoiceTypeConfig GetDetectedVoiceConfig()
    {
        foreach (var config in maleFreq)
        {
            if (config.typeName == detectedVoiceType)
                return config;
        }

        foreach (var config in femaleFreq)
        {
            if (config.typeName == detectedVoiceType)
                return config;
        }

        return null;
    }
}
