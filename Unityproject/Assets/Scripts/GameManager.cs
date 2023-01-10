using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Gm;
    public MapDataGetter mapData;
    public Sprite[] cards;
    private void Awake()
    {
        Gm = GetComponent<GameManager>();
        mapData.init();
    }
    public void GameStart()
    {
        mapData.GenerateCards();
    }
}
