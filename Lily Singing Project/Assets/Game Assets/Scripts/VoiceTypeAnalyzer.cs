using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum VoiceType
{
    Bass,
    Baritone,
    Tenor,
    Alto,
    MezzoSoprano,
    Soprano,
    Unknown
}

public class VoiceTypeAnalyzer : MonoBehaviour
{
    public VoiceFrequencyAnalyzer analyzer;
    public AudioSource audioSource;
    public UIVoiceNoteChecker lowNoteChcker;
    public UIVoiceNoteChecker highNoteChecker;

    [HorizontalLine]

    public TMP_Text resultText;
    public Button continueButton;

    [HorizontalLine]
    [ReadOnly] public VoiceTypeConfig detectedVoiceType;
    [ReadOnly] public bool isCapturing = false; // Indicator to show if capturing is in progress

    [HorizontalLine]
    [ReadOnly] public Note lowestRecorded;
    [ReadOnly] public Note highestRecorded;
    [ReadOnly] public List<float> recordedFrequencies = new List<float>(); // List to display recorded frequencies

    [HorizontalLine]
    [Header("DEBUG")]
    public VoiceType manualVoiceType;

    [HorizontalLine]

    public List<VoiceTypeConfig> maleFreq = new List<VoiceTypeConfig>();
    public List<VoiceTypeConfig> femaleFreq = new List<VoiceTypeConfig>();
    [ReadOnly] public Note lowestVoiceConfigFreq;
    [ReadOnly] public Note highestVoiceConfigFreq;

    [System.Serializable]
    public class VoiceTypeConfig
    {
        public VoiceType voiceType;
        public int minFrequency;
        public int maxFrequency;
    }

    void OnValidate()
    {
        SetVoiceConfigFreqRange();
    }

    void Awake()
    {
        SetVoiceConfigFreqRange();
    }

    void Start()
    {
        detectedVoiceType = null;
        continueButton.gameObject.SetActive(false);
    }

    void SetVoiceConfigFreqRange()
    {
        lowestVoiceConfigFreq = Note.GetNoteFromFrequency(GetVoiceTypeConfig(VoiceType.Bass).minFrequency);
        highestVoiceConfigFreq = Note.GetNoteFromFrequency(GetVoiceTypeConfig(VoiceType.Soprano).maxFrequency);
    }

    [Button]
    public void SetManualVoiceType()
    {
        VoiceTypeConfig typeConfig = GetVoiceTypeConfig(manualVoiceType);
        lowestRecorded = Note.GetNoteFromFrequency(typeConfig.minFrequency);
        highestRecorded = Note.GetNoteFromFrequency(typeConfig.maxFrequency);

        SetFinalVoiceType();
    }

    public void SetFinalVoiceType()
    {
        if (lowestRecorded.frequency == 0 || highestRecorded.frequency == 0)
        {
            return;
        }
        detectedVoiceType = DetermineVoiceType(lowestRecorded.frequency, highestRecorded.frequency);
        resultText.text = $"You were identified as {detectedVoiceType.voiceType}!";

        continueButton.gameObject.SetActive(true);
    }

    public IEnumerator CaptureVoiceType(Action<float> OnProgress, Action<float> OnFinish)
    {
        isCapturing = true; // Indicate that the capturing has started

        print($"Starting voice analysis...");
        float captureDuration = 5f; // Duration to capture frequencies
        float captureInterval = 1f; // Interval at which frequencies are sampled
        recordedFrequencies.Clear(); // Clear the list of recorded frequencies

        float elapsed = 0f;
        while (elapsed < captureDuration)
        {
            float currentFrequency = analyzer.frequency;

            if (!float.IsNaN(currentFrequency)) // Check if the frequency is valid
            {
                recordedFrequencies.Add(currentFrequency);
            }

            elapsed += captureInterval;
            OnProgress?.Invoke(elapsed);
            yield return new WaitForSeconds(captureInterval);
        }

        isCapturing = false;

        // Calculate the average frequency
        float averageFrequency = 0f;
        foreach (float freq in recordedFrequencies)
        {
            averageFrequency += freq;
        }
        averageFrequency /= recordedFrequencies.Count;

        OnFinish?.Invoke(averageFrequency);
    }

    public VoiceTypeConfig DetermineVoiceType(float lowestFrequency, float highestFrequency)
    {
        // First, check if the voice falls into the male frequency ranges
        foreach (var config in maleFreq)
        {
            if (lowestFrequency >= config.minFrequency && highestFrequency <= config.maxFrequency)
            {
                print($"Detected Male Voice: {config.voiceType}");
                return config; // Return the matched male voice type
            }
        }

        // If no male voice types match, check the female frequency ranges
        foreach (var config in femaleFreq)
        {
            if (lowestFrequency >= config.minFrequency && highestFrequency <= config.maxFrequency)
            {
                print($"Detected Female Voice: {config.voiceType}");
                return config; // Return the matched female voice type
            }
        }

        // If no match is found, return "Unknown"
        float averageFrequency = (lowestFrequency + highestFrequency) / 2;

        VoiceTypeConfig closestMatch = GetClosestVoiceType(averageFrequency);
        print($"Could not determine voice type directly. Returning closest match: {closestMatch.voiceType}");
        return closestMatch;
    }

    VoiceTypeConfig GetClosestVoiceType(float averageFrequency)
    {
        VoiceTypeConfig closestConfig = null;
        float closestDifference = float.MaxValue;

        // Check male frequency ranges
        foreach (var config in maleFreq)
        {
            float configAvg = (config.minFrequency + config.maxFrequency) / 2;
            float difference = Mathf.Abs(averageFrequency - configAvg);

            if (difference < closestDifference)
            {
                closestDifference = difference;
                closestConfig = config;
            }
        }

        // Check female frequency ranges
        foreach (var config in femaleFreq)
        {
            float configAvg = (config.minFrequency + config.maxFrequency) / 2;
            float difference = Mathf.Abs(averageFrequency - configAvg);

            if (difference < closestDifference)
            {
                closestDifference = difference;
                closestConfig = config;
            }
        }

        return closestConfig;
    }

    public VoiceTypeConfig GetVoiceTypeConfig(VoiceType voiceType)
    {
        // Search through male frequencies first
        foreach (var config in maleFreq)
        {
            if (config.voiceType == voiceType)
            {
                return config;
            }
        }

        // Search through female frequencies if not found in maleFreq
        foreach (var config in femaleFreq)
        {
            if (config.voiceType == voiceType)
            {
                return config;
            }
        }

        // Return null or a default value if no match is found
        return null;  // or you can return a default VoiceTypeConfig if needed
    }

}
