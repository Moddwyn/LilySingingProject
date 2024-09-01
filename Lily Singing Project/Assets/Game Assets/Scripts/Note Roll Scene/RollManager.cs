using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Audio;

public class RollManager : MonoBehaviour
{
    public NotePlacer notePlacer;
    public VoiceTypeAnalyzer voiceTypeAnalyzer;
    public AudioPitchEstimator pitchEstimator;
    public AudioMixer audioMixer;
    bool isRolling = false;

    [HorizontalLine]
    
    public AudioSource songSource;
    public float offsetStart = 1;
    [Range(0.5f, 2.0f)] public float pitchOffset = 1.0f;
    [ReadOnly] public float currentTime;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A)) StartCoroutine(SetPitchOffsetBasedOnVoiceType());

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isRolling = true;
            songSource.Stop();
            songSource.time = offsetStart;
            songSource.Play();
        }

        if (isRolling)
        {
            UpdateBlockPositions();
        }
        
        currentTime = songSource.time;
    }

    void UpdateBlockPositions()
    {
        foreach (NoteBlock noteBlock in notePlacer.placedBlocks)
        {
            float startTime = noteBlock.blockPlacement.startTime;
            float timeDifference = startTime - currentTime;
            float xPos = timeDifference * 100f * notePlacer.widthStretchMultipliter + notePlacer.xOffset;

            RectTransform noteTransform = noteBlock.GetComponent<RectTransform>();
            noteTransform.anchoredPosition = new Vector2(xPos, noteTransform.anchoredPosition.y);
        }
    }

    IEnumerator SetPitchOffsetBasedOnVoiceType()
    {
        yield return voiceTypeAnalyzer.AnalyzeVoiceType(); // Analyze the user's voice type

        float originalSongFreq = EstimateSongFrequency();

        var voiceConfig = voiceTypeAnalyzer.GetDetectedVoiceConfig();
        if (voiceConfig != null)
        {
            float userComfortableFreq = (voiceConfig.minFrequency + voiceConfig.maxFrequency) / 2f;

            // Calculate the pitch offset needed to shift the song to the user's comfortable frequency range
            pitchOffset = Mathf.Pow(2f, (Mathf.Log(userComfortableFreq / originalSongFreq) / Mathf.Log(2f))); // Calculate pitch offset

            audioMixer.SetFloat("PitchShifter", pitchOffset);
        }
    }

    float EstimateSongFrequency()
    {
        float estimatedFrequency = pitchEstimator.Estimate(songSource);
        return !float.IsNaN(estimatedFrequency) ? estimatedFrequency : 440f;
    }
}
