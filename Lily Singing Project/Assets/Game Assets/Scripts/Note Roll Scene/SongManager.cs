using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SongManager : MonoBehaviour
{
    public VoiceTypeAnalyzer typeAnalyzer;
    public MidiFileReader midiFileReader;
    public RollManager rollManager;
    public AudioSource songSource;
    public AudioMixer songMixer;

    [HorizontalLine]

    public int semitoneOffset;
    [ReadOnly] public Note currentLowestSongAdjustedNote;
    [ReadOnly] public Note currentHighestSongAdjustedNote;

    [HorizontalLine]

    public TMP_Text semitoneOffsetText;
    public TMP_Text lowestNoteText;
    public TMP_Text highestNoteText;
    public Button offsetSemiDownButton;
    public Button offsetSemiUpButton;
    public Button lowSongPreviewButton;
    public Button highSongPreviewButton;
    public Button startSongButton;

    void Start()
    {
        offsetSemiDownButton.onClick.AddListener(() => OffsetSemitone(-1));
        offsetSemiUpButton.onClick.AddListener(() => OffsetSemitone(1));

        lowSongPreviewButton.onClick.AddListener(PreviewLowNote);
        highSongPreviewButton.onClick.AddListener(PreviewHighNote);
        
        startSongButton.onClick.AddListener(() =>
        {
            StopAllCoroutines();
            songSource.Stop();
            rollManager.StartSong();
        });
        
        UpdateSemitoneText();

        midiFileReader.ConvertMidiFileToNoteData();
        SetSongNoteRangeBySemitoneOffset(semitoneOffset);
    }

    void SetSongNoteRangeBySemitoneOffset(int semiOffset)
    {
        float lowFreq = OffsetFrequencyBySemitone(midiFileReader.minFrequency, semiOffset);
        float highFreq = OffsetFrequencyBySemitone(midiFileReader.maxFrequency, semiOffset);

        currentLowestSongAdjustedNote = Note.GetNoteFromFrequency(lowFreq);
        currentHighestSongAdjustedNote = Note.GetNoteFromFrequency(highFreq);

        float pitchMultiplier = Mathf.Pow(2, semitoneOffset / 12f);
        songMixer.SetFloat("PitchShifter", pitchMultiplier);

        string noteNameLowProper = Note.GetNoteNameFormatted(currentLowestSongAdjustedNote.noteName);
        string noteNameHighProper = Note.GetNoteNameFormatted(currentHighestSongAdjustedNote.noteName);
        lowestNoteText.text = $"{noteNameLowProper}{currentLowestSongAdjustedNote.octave}";
        highestNoteText.text = $"{noteNameHighProper}{currentHighestSongAdjustedNote.octave}";
    }

    void SuggestSemitoneOffset()
    {
        // Get the min and max frequencies of the detected voice type (e.g., Bass)
        float voiceMinFreq = typeAnalyzer.detectedVoiceType.minFrequency; // E2 = 82 Hz
        float voiceMaxFreq = typeAnalyzer.detectedVoiceType.maxFrequency; // E4 = 330 Hz

        // Get the current song's min and max frequencies
        float songMinFreq = midiFileReader.minFrequency; // A3 = 220 Hz
        float songMaxFreq = midiFileReader.maxFrequency; // B4 = 494 Hz

        // Calculate semitone offset to bring the song's max note down to fit the voice type's max frequency
        int semitoneOffsetForMaxFreq = Mathf.FloorToInt(12 * Mathf.Log(voiceMaxFreq / songMaxFreq, 2));

        // Calculate semitone offset to bring the song's min note down to fit the voice type's min frequency
        int semitoneOffsetForMinFreq = Mathf.FloorToInt(12 * Mathf.Log(voiceMinFreq / songMinFreq, 2));

        // We need to use the greater downward shift to fit the entire song in the vocal range
        int suggestedSemitoneOffset = Mathf.Min(semitoneOffsetForMaxFreq, semitoneOffsetForMinFreq);

        StartCoroutine(AdjustSemitoneOffsetTowardsTarget(suggestedSemitoneOffset));
    }

    IEnumerator AdjustSemitoneOffsetTowardsTarget(int targetOffset)
    {
        // Gradually adjust the semitone offset one by one until the target is reached
        while (semitoneOffset != targetOffset)
        {
            int step = targetOffset > semitoneOffset ? 1 : -1; // Decide whether to increment or decrement

            // Try adjusting by one semitone
            bool canAdjust = OffsetSemitone(step);

            if (!canAdjust)
            {
                // If OffsetSemitone returns false, stop the process
                yield break;
            }

            // Wait for a short duration between steps (e.g., 0.1 seconds) for gradual change
            yield return null;
        }
    }

    bool OffsetSemitone(int amount)
    {
        // Calculate the new frequencies based on the semitone offset
        float newLowFreq = OffsetFrequencyBySemitone(currentLowestSongAdjustedNote.frequency, semitoneOffset + amount);
        float newHighFreq = OffsetFrequencyBySemitone(currentHighestSongAdjustedNote.frequency, semitoneOffset + amount);

        // Get the frequency limits from the voice configurations
        float lowestAllowedFreq = typeAnalyzer.lowestVoiceConfigFreq.frequency; // Lowest allowed frequency (e.g., Bass)
        float highestAllowedFreq = typeAnalyzer.highestVoiceConfigFreq.frequency; // Highest allowed frequency (e.g., Soprano)

        // Check if the new frequency is within the allowed range, if not, clamp the semitoneOffset
        if (newLowFreq < lowestAllowedFreq || newHighFreq > highestAllowedFreq)
        {
            // If out of bounds, don't update the offset and return
            print("Cannot shift further: out of human voice range");
            return false;
        }

        // Update semitoneOffset if the new frequencies are within the valid range
        semitoneOffset += amount;
        SetSongNoteRangeBySemitoneOffset(semitoneOffset);
        UpdateSemitoneText();

        return true;
    }


    void UpdateSemitoneText()
    {
        semitoneOffsetText.text = $"{semitoneOffset}";
    }

    void PreviewLowNote()
    {
        float lowNoteTime = midiFileReader.GetTimeOfLowestNote(); // Get the time in seconds of the lowest note
        PlayPreview(lowNoteTime);
    }

    void PreviewHighNote()
    {
        float highNoteTime = midiFileReader.GetTimeOfHighestNote(); // Get the time in seconds of the highest note
        PlayPreview(highNoteTime);
    }

    void PlayPreview(float noteTime)
    {
        StopAllCoroutines();
        songSource.Stop();
        songSource.clip = midiFileReader.currentSongData.soundClip;

        // Start 2 seconds before the note time, but not before the start of the song
        float previewStartTime = Mathf.Max(noteTime - 2f, 0f);

        // Ensure the preview lasts for 10 seconds or until the song ends
        float previewEndTime = Mathf.Min(previewStartTime + 10f, songSource.clip.length);

        // Set the AudioSource to play from the start time
        songSource.time = previewStartTime;
        songSource.Play();

        // Schedule stopping the song after the preview duration
        StartCoroutine(StopPreviewAfterDuration(previewEndTime - previewStartTime));
    }

    IEnumerator StopPreviewAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        songSource.Stop();
    }

    public static float OffsetFrequencyBySemitone(float frequency, int amount)
    {
        // Calculate the frequency offset by the number of semitones
        float offsetFrequency = frequency * Mathf.Pow(2, amount / 12f);
        return offsetFrequency;
    }

}
