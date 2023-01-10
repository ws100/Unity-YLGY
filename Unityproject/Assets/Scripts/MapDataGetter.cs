using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DG.Tweening;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class MapDataGetter : MonoBehaviour
{
    public Camera mainCamera;
    public RectTransform canvasTransform;
    //服务器存储地图数据的地址
    public string MapJsonUrl = "http://43.139.170.217/ylgy/level1.json";
    //单例对象
    public static MapDataGetter mapData;
    //Json对象
    public JObject jsonData;
    //生成卡牌的父物体
    public GameObject content;
    //层对象
    public GameObject[] level;
    //卡牌的数组
    public List<Card> cards;
    //卡牌的游戏对象
    public GameObject card;
    //最大层数
    public int maxLevel;
    //初始化函数
    public List<List<int>> LevelCard;
    public void Init()
    {
        //初始化单例类
        mapData = GetComponent<MapDataGetter>();
        //开始获取地图数据
        StartCoroutine(Get());
    }
    //获取数据
    IEnumerator Get()
    {
        //发送Get请求
        UnityWebRequest request = UnityWebRequest.Get(MapJsonUrl);
        //打印请求的数据
        print(MapJsonUrl);
        //判断是否请求完成
        yield return request.SendWebRequest();
        //打印状态码
        print(request.responseCode);
        //判断请求成功
        if(request.responseCode == 200)
        {
            //将请求的数据转化成string
            string receiveContent = request.downloadHandler.text;
            //将string解析成json数据
            jsonData = JObject.Parse(receiveContent);
            //打印解析出的json数据
            print(jsonData.ToString());
        }

    }
    

    //生成卡牌的函数
    public void GenerateCards()
    {
        //初始化层列数组
        LevelCard = new List<List<int>>();
        //解析Json中的levelData数据(存有每个卡牌的位置)
        JObject levelData = JObject.Parse(jsonData["levelData"].ToString());
        print(levelData.ToString());
        //解析blockTypeData数据(存有每种卡牌的数量)
        JObject TypeData = JObject.Parse(jsonData["blockTypeData"].ToString());
        //定义生成卡牌类别的数组
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
        //将数组乱序类似
        for (int i = 0; i < rList.Count; i++)
        {
            int index = Random.Range(i, rList.Count);
            int tmp = rList[i];
            int ran = rList[index];
            rList[i] = ran;
            rList[index] = tmp;
        }
        //打印乱序之后的数组
        print(rList.ToString());
        int idx = 0;
        //生成卡牌的函数部分
        string parserCsv = "";
        int level = 1;
        foreach (KeyValuePair<string, JToken> m in levelData)
        {
            
            //获取卡牌数组
            List<int> subLevelList = new List<int>();
            JArray subList = JArray.Parse(m.Value.ToString());
            foreach (JToken j in subList)
            {
                //生成游戏物体
                GameObject o = Instantiate(card,content.transform);
                //获取卡牌的类型
                int cardNum = rList[idx++] - 1;
                //获取卡牌类型相对应的图片
                o.GetComponent<Image>().sprite = GameManager.Gm.cards[cardNum];
                //获取Card组件
                Card c = o.GetComponent<Card>();
                //将Json中信息转换成csv
                parserCsv += j["id"].ToString() + "," + level + "," + j["rowNum"] + "," + j["rolNum"] + "\n";
                //设置生成状态
                c.type = cardNum;
                c.id = j["id"].ToString();
                c.xPos = int.Parse(j["rowNum"].ToString());
                c.yPos = int.Parse(j["rolNum"].ToString());
                c.status = Card.st.OnMap;
                c.level = level;
                c.SetButton();
                //将card的序号加入层数组
                subLevelList.Add(idx-1);
                //将Card组件加入列表
                cards.Add(o.GetComponent<Card>());
                //获取生成组件的RectTransform
                RectTransform rt = o.GetComponent<RectTransform>();
                //定义卡牌的出生位置
                Vector2 cardPos = new Vector2(int.Parse(j["rolNum"].ToString()) * 10 - 640, int.Parse(j["rowNum"].ToString()) * 10 - 560);
                //定义卡牌落下之后的位置
                Vector2 gPos = new Vector2(int.Parse(j["rolNum"].ToString()) * 10 - 640, 1000);
                //将卡牌挪到出生位置
                rt.anchoredPosition = gPos;
                //创建出生位置到坐标位置的动画
                rt.DOAnchorPos(cardPos,0.3f+idx*0.006f);
            }
            LevelCard.Add(subLevelList);
            level++;
        }
        maxLevel = level - 1;
        UpdateCardStatus(level-2);
        content.GetComponent<RectTransform>().anchoredPosition = new Vector2(360, 300);
        
    }
    //参数为当前的最高层
    public void UpdateCardStatus(int level)
    {
        //将每一个卡牌更新(不需要更新比当前高层的卡牌)
        for (int i = 0; i < level; i++)
        {
            for (int j = 0;j < LevelCard[i].Count; j++)
            {
                UpdateCardStatus(cards[LevelCard[i][j]]);
            }
        }
    }

    public void UpdateCardStatus(Card c)
    {
        //便历每一个地图上的卡牌
        for (int i = c.level; i < maxLevel; i++)
        {
            for (int j = 0; j < LevelCard[i].Count; j++)
            {
                Card tc = cards[LevelCard[i][j]];
                if(tc.status == Card.st.OnBoard)continue;
                //判断卡牌是否在遮挡范围内
                if (Mathf.Abs(tc.xPos - c.xPos) < 8 && Mathf.Abs(tc.yPos - c.yPos) < 8)
                {
                    //设置被遮挡之后的状态
                    print("z");
                    c.status = Card.st.onMapZ;
                    c.GetComponent<Image>().color = Color.gray;
                    return;
                }
            }
        }
        //没有被遮挡则还原状态
        if (c.status == Card.st.onMapZ)
        {
            c.status = Card.st.OnMap;
            c.GetComponent<Image>().color = Color.white;
        }
    }

    public void ResetCard()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].gameObject.SetActive(false);
        }
        cards = new List<Card>();
    }
}
