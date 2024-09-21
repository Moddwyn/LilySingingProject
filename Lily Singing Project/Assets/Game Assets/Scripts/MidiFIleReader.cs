using System;
using System.Collections.Generic;
using System.IO;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class MidiFileReader : MonoBehaviour
{
    [ValidateInput("IsIndexWithinSongDataCount", "Your index is not in Song Datas")]
    [OnValueChanged("UpdateCurrentSongData")][MinValue(0)] 
    public int currentSongIndex;
    public TMP_Text songTitleText;
    
    public List<SongData> songDatas = new List<SongData>();
    public List<MidiNoteData> midiNoteDatas = new List<MidiNoteData>();

    [HorizontalLine]

    [ReadOnly] public SongData currentSongData;
    [ReadOnly] public float minFrequency;
    [ReadOnly] public float maxFrequency;

    void OnValidate()
    {
        UpdateCurrentSongData();
    }

    bool IsIndexWithinSongDataCount()
    {
        return currentSongIndex < songDatas.Count;
    }

    public void UpdateCurrentSongData()
    {
        if (songDatas.Count == 0 || IsIndexWithinSongDataCount() == false || currentSongIndex < 0) return;
        currentSongData = songDatas[currentSongIndex];

        if(songTitleText != null)
            songTitleText.text = "Song: " + currentSongData.midiFileName;
    }

    public void ConvertMidiFileToNoteData(Action<List<MidiNoteData>> OnGenerate = null)
    {
        if (currentSongData == null)
        {
            Debug.LogError("Current song not assigned.");
            return;
        }

        midiNoteDatas.Clear();

        string midiFilePath = Path.Combine(Application.streamingAssetsPath, currentSongData.midiFileName) + ".mid";

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
                    int octave = note.Octave + currentSongData.octaveOffset;

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

        if (midiNoteDatas != null && midiNoteDatas.Count > 0)
        {
            SetSongFrequencyRange();
            OnGenerate?.Invoke(midiNoteDatas);
        }

    }

    public void AdjustAllNotesByOctaves(int octaves)
    {
        foreach (var noteData in midiNoteDatas)
        {
            noteData.note.octave += octaves;
            noteData.note.frequency = Note.GetFrequencyFromNote(noteData.note.noteName, noteData.note.octave);
        }
    }

    public float GetTimeOfLowestNote()
    {
        float lowestFrequency = float.MaxValue;
        float timeInSeconds = 0f;

        foreach (var noteData in midiNoteDatas)
        {
            if (noteData.note.frequency < lowestFrequency)
            {
                lowestFrequency = noteData.note.frequency;
                timeInSeconds = noteData.timeInSeconds;
            }
        }

        return timeInSeconds;
    }

    public float GetTimeOfHighestNote()
    {
        float highestFrequency = float.MinValue;
        float timeInSeconds = 0f;

        // Iterate through all midiNoteDatas to find the highest frequency
        foreach (var noteData in midiNoteDatas)
        {
            if (noteData.note.frequency > highestFrequency)
            {
                highestFrequency = noteData.note.frequency;
                timeInSeconds = noteData.timeInSeconds;
            }
        }

        return timeInSeconds;
    }
    
    void SetSongFrequencyRange()
    {
        minFrequency = float.MaxValue;
        maxFrequency = float.MinValue;
        float totalFrequency = 0;

        foreach (var midiNoteData in midiNoteDatas)
        {
            float noteFrequency = midiNoteData.note.frequency;

            if (noteFrequency < minFrequency)
                minFrequency = noteFrequency;

            if (noteFrequency > maxFrequency)
                maxFrequency = noteFrequency;

            totalFrequency += noteFrequency;
        }

        if (midiNoteDatas.Count > 0)
        {
            float averageFrequency = totalFrequency / midiNoteDatas.Count;
            print($"Average frequency: {averageFrequency}Hz");
        }
        else
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

[Serializable]
public class SongData
{
    [Dropdown("GetMidiFileNames"), AllowNesting]
    public string midiFileName;
    public int octaveOffset;
    public AudioClip soundClip;

    List<string> GetMidiFileNames()
    {
        List<string> midiFiles = new List<string>();

        string path = Application.streamingAssetsPath;
        if (Directory.Exists(path))
        {
            // Get all .mid files in the StreamingAssets directory
            string[] files = Directory.GetFiles(path, "*.mid");

            foreach (string filePath in files)
            {
                // Get only the file name without the path or extension
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                midiFiles.Add(fileName);
            }
        }
        else
        {
            Debug.LogError("StreamingAssets folder not found!");
        }

        return midiFiles;
    }
}

