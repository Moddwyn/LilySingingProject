using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NaughtyAttributes;
using UnityEngine;

public class MidiFileReader : MonoBehaviour
{
    public string midiFileName;
    public List<MidiNoteData> midiNoteDatas;
    [ReadOnly] public float minFrequency;
    [ReadOnly] public float maxFrequency;

    public void ConvertMidiFileToNoteData(Action OnGenerate)
    {
        if (string.IsNullOrEmpty(midiFileName))
        {
            Debug.LogError("MIDI file name not assigned.");
            return;
        }

        midiNoteDatas = new List<MidiNoteData>();

        string midiFilePath = Path.Combine(Application.streamingAssetsPath, midiFileName) + ".mid";

        if (File.Exists(midiFilePath))
        {
            using (var stream = new FileStream(midiFilePath, FileMode.Open, FileAccess.Read))
            {
                var midiFile = MidiFile.Read(stream);
                var tempoMap = midiFile.GetTempoMap();

                foreach (var note in midiFile.GetNotes())
                {
                    // Convert note time and length to MetricTimeSpan
                    var noteTime = note.Time;
                    var noteLength = note.Length;

                    var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(noteTime, tempoMap);
                    var metricLengthSpan = TimeConverter.ConvertTo<MetricTimeSpan>(noteLength, tempoMap);

                    var timeInSeconds = metricTimeSpan.TotalSeconds;
                    var durationInSeconds = metricLengthSpan.TotalSeconds;

                    string noteNameRaw = note.NoteName.ToString();
                    int octave = note.Octave;

                    Note.Name noteNameEnum = ParseNoteName(noteNameRaw);

                    MidiNoteData noteData = new MidiNoteData
                    {
                        note = new Note { noteName = noteNameEnum, octave = octave, frequency = Note.GetFrequencyFromNote(noteNameEnum, octave) },
                        timeInSeconds = (float)timeInSeconds,
                        durationInSeconds = (float)durationInSeconds
                    };
                    midiNoteDatas.Add(noteData);
                }
            }
        }
        else
        {
            Debug.LogError($"MIDI file not found at path: {midiFilePath}");
        }

        if(midiNoteDatas != null && midiNoteDatas.Count > 0)
        {
            SetSongFrequencyRange();
            OnGenerate?.Invoke();
        }
        
    }

    void SetSongFrequencyRange()
    {
        minFrequency = float.MaxValue;
        maxFrequency = float.MinValue;

        foreach (var midiNoteData in midiNoteDatas)
        {
            float noteFrequency = midiNoteData.note.frequency;

            if (noteFrequency < minFrequency)
                minFrequency = noteFrequency;

            if (noteFrequency > maxFrequency)
                maxFrequency = noteFrequency;
        }

        if (midiNoteDatas.Count == 0)
        {
            minFrequency = 0f;
            maxFrequency = 0f;
        }
    }

    Note.Name ParseNoteName(string noteNameRaw)
    {
        switch (noteNameRaw)
        {
            case "CSharp": return Note.Name.CSharp;
            case "DSharp": return Note.Name.DSharp;
            case "FSharp": return Note.Name.FSharp;
            case "GSharp": return Note.Name.GSharp;
            case "ASharp": return Note.Name.ASharp;
            default: return (Note.Name)Enum.Parse(typeof(Note.Name), noteNameRaw);
        }
    }

}

[Serializable]
public class MidiNoteData
{
    public Note note;
    public float timeInSeconds;
    public float durationInSeconds;
}

