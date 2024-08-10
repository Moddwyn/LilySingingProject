using System.Collections;
using System.Collections.Generic;
using System.IO;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

public class MidiFIleReader : MonoBehaviour
{
    public string midiFileName;
    public List<MidiNoteData> midiNotes;

    void Start()
    {
        if (string.IsNullOrEmpty(midiFileName))
        {
            Debug.LogError("MIDI file name not assigned.");
            return;
        }

        midiNotes = new List<MidiNoteData>();
        string midiFilePath = Path.Combine(Application.streamingAssetsPath, midiFileName);

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

                    MidiNoteData noteData = new MidiNoteData
                    {
                        fullNoteName = GetFormattedNoteName(note.NoteName.ToString(), note.Octave),
                        timeInSeconds = (float)timeInSeconds,
                        durationInSeconds = (float)durationInSeconds
                    };
                    midiNotes.Add(noteData);
                }
            }
        }
        else
        {
            Debug.LogError($"MIDI file not found at path: {midiFilePath}");
        }
    }

    private string GetFormattedNoteName(string noteName, int octave)
    {
        switch (noteName)
        {
            case "CSharp": return $"C#{octave}";
            case "DSharp": return $"D#{octave}";
            case "FSharp": return $"F#{octave}";
            case "GSharp": return $"G#{octave}";
            case "ASharp": return $"A#{octave}";
            default: return $"{noteName}{octave}";
        }
    }

}

[System.Serializable]
public class MidiNoteData
{
    public string fullNoteName;
    public float timeInSeconds;
    public float durationInSeconds;
}

