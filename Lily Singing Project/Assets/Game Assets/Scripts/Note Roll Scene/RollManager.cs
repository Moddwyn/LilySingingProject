using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Audio;

public class RollManager : MonoBehaviour
{
    public NotePlacer notePlacer;
    public VoiceTypeAnalyzer typeAnalyzer;
    public AudioPitchEstimator pitchEstimator;
    public AudioMixer audioMixer;
    bool isRolling = false;

    [HorizontalLine]

    public AudioSource songSource;
    public float offsetStart = 1;
    public float pitchMultiplier = 1;
    [ReadOnly] public float currentTime;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isRolling = true;
            songSource.Stop();
            
            pitchMultiplier = typeAnalyzer.detectedVoiceType != null? typeAnalyzer.detectedVoiceType.suggestedPitch : 1;
            audioMixer.SetFloat("PitchShifter", pitchMultiplier+0.05f);
            songSource.time = offsetStart;
            notePlacer.PlaceNotes(pitchMultiplier);

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
}
