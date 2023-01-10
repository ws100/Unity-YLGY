using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Net;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class MapDataGetter : MonoBehaviour
{
    public string MapJsonUrl = "http://43.139.170.217/ylgy/level1.json";
    public static MapDataGetter mapData;
    public JObject jsonData;
    public GameObject content;
    public GameObject[] level;
    public List<Card> cards;
    public GameObject card;
    public void init()
    {
        mapData = GetComponent<MapDataGetter>();
        StartCoroutine(Get());
    }

    IEnumerator Get()
    {
        UnityWebRequest request = UnityWebRequest.Get(MapJsonUrl);
        print(MapJsonUrl);
        yield return request.SendWebRequest();
        print(request.responseCode);
        if(request.responseCode == 200)
        {
            string receiveContent = request.downloadHandler.text;
            jsonData = JObject.Parse(receiveContent);
            print(jsonData.ToString());
        }

    }

    public void GenerateCards()
    {
        RectTransform conT = content.GetComponent<RectTransform>();
        JObject levelData = JObject.Parse(jsonData["levelData"].ToString());
        print(levelData.ToString());
        JObject TypeData = JObject.Parse(jsonData["blockTypeData"].ToString());
        List<int> rList = new List<int>();
        foreach (KeyValuePair<string, JToken> m in TypeData)
        {
            int typeNum = int.Parse(m.Key.ToString());
            int typeCount = int.Parse(m.Value.ToString())*3;
            for (int i = 0; i < typeCount; i++)
            {
                rList.Add(typeNum);
            }
        }
        for (int i = 0; i < rList.Count; i++)
        {
            int index = Random.Range(i, rList.Count);
            int tmp = rList[i];
            int ran = rList[index];
            rList[i] = ran;
            rList[index] = tmp;
        }
        print(rList.ToString());
        int idx = 0;
        foreach (KeyValuePair<string, JToken> m in levelData)
        {
            JArray subList = JArray.Parse(m.Value.ToString());
            
            foreach (JToken j in subList)
            {
                GameObject o = Instantiate(card,content.transform);
                int cardNum = rList[idx++] - 1;
                o.GetComponent<Image>().sprite = GameManager.Gm.cards[cardNum];
                Card c = o.GetComponent<Card>();
                c.id = j["id"].ToString();
                c.pos = new Vector2(int.Parse(j["rowNum"].ToString()),int.Parse(j["rolNum"].ToString()));
                cards.Add(o.GetComponent<Card>());
                RectTransform rt = o.GetComponent<RectTransform>();
                Vector2 cardPos = new Vector2(conT.position.x+ int.Parse(j["rolNum"].ToString()) * 10, conT.position.y - int.Parse(j["rowNum"].ToString()) * 10);
                rt.position = cardPos;

            }
        }

    }
}
