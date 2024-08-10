using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPages : MonoBehaviour
{
    public RectTransform pagesRect;
    public float speed = 2;

    public int currentPage;

    void Start() 
    {
        currentPage = 1;
    }

    void Update()
    {
        int yPos = currentPage == 0? -1080 : (currentPage == 1? 0 : 1080);
        pagesRect.anchoredPosition = Vector3.MoveTowards(pagesRect.anchoredPosition, new Vector3(0, yPos, 0), Time.deltaTime * speed);
    }

    public void MoveToPage(int page)
    {
        currentPage = page;
    }
}
