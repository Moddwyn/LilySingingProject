using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIVoiceNoteChecker : MonoBehaviour
{
    public Button display;
    public Image progressImage;
    public TMP_Text displayText;

    [HorizontalLine]

    public CheckingType checkingType;
    public enum CheckingType { Low, High }
    public VoiceTypeAnalyzer typeAnalyzer;

    [HorizontalLine]

    [ReadOnly] public bool recording;
    [ReadOnly] public int countdownTime;


    void Start()
    {
        progressImage.fillAmount = 0;
        SetDisplayText("START");

        display.onClick.AddListener(() =>
        {
            if (recording == false)
                StartCoroutine(StartRecording());
        });
    }

    IEnumerator StartRecording()
    {
        int interval = 5;

        recording = true;
        countdownTime = 3;

        while (countdownTime > 0)
        {
            SetDisplayText(countdownTime + "");
            countdownTime -= 1;
            yield return new WaitForSeconds(1);
        }

        SetDisplayText("SING!");

        StartCoroutine(typeAnalyzer.CaptureVoiceType(
        (progress) =>
        {
            progressImage.fillAmount = progress / interval;
        },
        (frequency) =>
        {
            if (IsFrequencyOutOfBounds(frequency))
            {
                // If the frequency is out of bounds, set the text to "Try again"
                SetDisplayText("Try again");
            }
            else
            {
                Note recordedNote = Note.GetNoteFromFrequency(frequency);
                SetDisplayText(Note.GetNoteNameFormatted(recordedNote.noteName) + "" + recordedNote.octave);

                if (checkingType == CheckingType.Low)
                {
                    typeAnalyzer.lowestRecorded = recordedNote;
                }
                else
                {
                    typeAnalyzer.highestRecorded = recordedNote;
                }

                typeAnalyzer.SetFinalVoiceType();
            }
        }));
        recording = false;
    }

    bool IsFrequencyOutOfBounds(float frequency)
    {
        foreach (var config in typeAnalyzer.maleFreq)
        {
            if (frequency >= config.minFrequency && frequency <= config.maxFrequency)
                return false;
        }

        foreach (var config in typeAnalyzer.femaleFreq)
        {
            if (frequency >= config.minFrequency && frequency <= config.maxFrequency)
                return false;
        }

        // If frequency doesn't fall within any valid range, it's out of bounds
        return true;
    }

    void SetDisplayText(string text)
    {
        displayText.text = text;
    }

}
