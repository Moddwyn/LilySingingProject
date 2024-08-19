using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class RollManager : MonoBehaviour
{
    public NotePlacer notePlacer;
    bool isRolling = false;

    [HorizontalLine]
    
    public AudioSource songSource;
    public float offsetStart = 1;
    [ReadOnly] public float currentTime;

    void Update()
    {
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
}
