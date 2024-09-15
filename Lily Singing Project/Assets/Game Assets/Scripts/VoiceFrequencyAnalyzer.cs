using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class VoiceFrequencyAnalyzer : MonoBehaviour
{
    public AudioSource audioSource;

    [Tooltip("Recording duration in seconds")]
    public int duration = 2;

    [Tooltip("Minimum frequency for pitch detection [Hz]")]
    [Range(40, 300)]
    public int frequencyMin = 40;

    [Tooltip("Maximum frequency for pitch detection [Hz]")]
    [Range(200, 1500)]
    public int frequencyMax = 600;

    [Tooltip("Number of harmonics to use for pitch estimation")]
    [Range(1, 8)]
    public int harmonicsToUse = 5;

    [Tooltip("Smoothing width for spectrum averaging [Hz]")]
    public float smoothingWidth = 500;

    [Tooltip("Threshold for voiced detection, higher values make detection stricter")]
    public float thresholdSRH = 7;

    [HorizontalLine]

    [ReadOnly] public float frequency;

    const int spectrumSize = 1024;
    const int outputResolution = 200; // Frequency axis resolution for SRH
    float[] spectrum = new float[spectrumSize];
    float[] specRaw = new float[spectrumSize];
    float[] specCum = new float[spectrumSize];
    float[] specRes = new float[spectrumSize];
    float[] srh = new float[outputResolution];

    public List<float> SRH => new List<float>(srh);

    void Start()
    {
        // Start recording audio
        audioSource.loop =  true;
        audioSource.playOnAwake = false;
        audioSource.clip = Microphone.Start(string.Empty, audioSource.loop, duration, AudioSettings.outputSampleRate);
        audioSource.Play();
    }

    void Update()
    {
        frequency = EstimatePitch();
    }

    /// <summary>
    /// Estimates the fundamental frequency of the input audio
    /// </summary>
    /// <returns>Estimated frequency in Hz, or NaN if no clear pitch is detected</returns>
    public float EstimatePitch()
    {
        var nyquistFreq = AudioSettings.outputSampleRate / 2.0f;

        // Retrieve the audio spectrum
        if (!audioSource.isPlaying) return float.NaN;
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Hanning);

        // Logarithmic amplitude of the spectrum to avoid negative infinity
        for (int i = 0; i < spectrumSize; i++)
        {
            specRaw[i] = Mathf.Log(spectrum[i] + 1e-9f);
        }

        // Calculate cumulative sum for smoothing
        specCum[0] = 0;
        for (int i = 1; i < spectrumSize; i++)
        {
            specCum[i] = specCum[i - 1] + specRaw[i];
        }

        // Calculate residual spectrum by subtracting smoothed components
        var halfRange = Mathf.RoundToInt((smoothingWidth / 2) / nyquistFreq * spectrumSize);
        for (int i = 0; i < spectrumSize; i++)
        {
            var indexUpper = Mathf.Min(i + halfRange, spectrumSize - 1);
            var indexLower = Mathf.Max(i - halfRange + 1, 0);
            var upper = specCum[indexUpper];
            var lower = specCum[indexLower];
            var smoothed = (upper - lower) / (indexUpper - indexLower);

            specRes[i] = specRaw[i] - smoothed;
        }

        // Calculate SRH (Summation of Residual Harmonics)
        float bestFreq = 0, bestSRH = 0;
        for (int i = 0; i < outputResolution; i++)
        {
            var currentFreq = (float)i / (outputResolution - 1) * (frequencyMax - frequencyMin) + frequencyMin;
            var currentSRH = GetSpectrumAmplitude(specRes, currentFreq, nyquistFreq);

            // Add harmonic components
            for (int h = 2; h <= harmonicsToUse; h++)
            {
                currentSRH += GetSpectrumAmplitude(specRes, currentFreq * h, nyquistFreq);
                currentSRH -= GetSpectrumAmplitude(specRes, currentFreq * (h - 0.5f), nyquistFreq);
            }

            srh[i] = currentSRH;

            // Record the frequency with the highest SRH score
            if (currentSRH > bestSRH)
            {
                bestFreq = currentFreq;
                bestSRH = currentSRH;
            }
        }

        // If SRH score is below threshold, return NaN (no clear pitch detected)
        return bestSRH < thresholdSRH ? float.NaN : bestFreq;
    }

    // Retrieve the amplitude at the given frequency from the spectrum
    float GetSpectrumAmplitude(float[] spec, float frequency, float nyquistFreq)
    {
        var position = frequency / nyquistFreq * spec.Length;
        var index0 = (int)position;
        var index1 = index0 + 1;
        var delta = position - index0;
        return (1 - delta) * spec[index0] + delta * spec[index1];
    }
}
