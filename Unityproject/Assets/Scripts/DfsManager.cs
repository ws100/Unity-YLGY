using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using DG.Tweening;
using Random = UnityEngine.Random;
public class DfsManager : MonoBehaviour
{
    public static DfsManager Dfs;
    //dfs计算出的路径步骤
    public List<int> step;
    public List<int> maxStep;
    //是否完成数据加载
    public bool isLoaded = false;
    //是否完成计算
    public bool isCalc = false;
    //dfs的最大深度
    public int dfsMaxLength;
    //dfs的当前深度
    public int dfsLength;
    //设定模拟的时间
    public Text calcTime;
    //模拟的终点时间
    public int targetTime;
    //地图上可以点击的卡牌数组
    public List<int> avMap;
    //每层对应的卡牌
    public List<List<int>> pLevelMap;
    public List<List<int>> mapList;
    public List<bool> isOnMap;
    public List<bool> isOnShowMap;
    public int[] boardScore = new[] { 0, 10000, 100000 };
    public double cT = 0;
    //卡牌的数组
    public List<Card> cards;
    //生成卡牌的父物体
    public GameObject content;
    //动画播放的步骤
    public int aniStep = 0; 
    //版面上物体的数量
    public int boardNumber;
    //版面上每个种类物体的数目
    public int[] boardType;
    //版面上物体的顺序
    public List<int> boardOrder;
    //实际显示中版面上物体顺序
    public List<int> showBoardOrder;
    //版面上的物体
    public GameObject[] boardObject;
    //展示信息的文字组件
    public Text informationText;
    void Start()
    {
        Dfs = GetComponent<DfsManager>();
    }
    //开始计算
    public void StartCalc()
    {
        if (!isLoaded)
        {
            ShowTipMessage.Tip.ShowTip("请先加载数据");
            return;
        }
        int t = int.Parse(calcTime.text);
        if (calcTime.text == ""||t <= 0)
        {
            ShowTipMessage.Tip.ShowTip("模拟时间设置错误");
            return;
        }
        targetTime = t;
        cT = 0;
        dfs(0, 0, 0, 0, 0, 0, 0);
        string res = "";
        for (int i = 0; i < maxStep.Count; i++)
        {
            if (i != maxStep.Count - 1) res += maxStep[i].ToString() + "->";
            else res += maxStep[i].ToString();
        }
        print(res);
        string showText = "";
        showText += "DFS执行次数:" + t.ToString() + "\n";
        showText += "DFS最大深度:" + dfsMaxLength.ToString() + "\n";
        showText += "搜索最大步数:" + maxStep.Count + "\n";
        showText += "搜索最优解:\n";
        showText += res;
        informationText.text = showText;
        isCalc = true;
    }
    //加载数据
    public void LoadData()
    {
        //清除之前的数据
        if (cards != null)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].gameObject.SetActive(false);
            }
        }
        isLoaded = true;
        isCalc = false;
        //存储卡牌的数组
        cards = new List<Card>();
        pLevelMap = new List<List<int>>();
        mapList = new List<List<int>>();
        isOnMap = new List<bool>();
        //获取json数据
        JObject jsonData = MapDataGetter.mapData.jsonData;
        //解析Json中的levelData数据(存有每个卡牌的位置)
        JObject levelData = JObject.Parse(jsonData["levelData"].ToString());
        //解析blockTypeData数据(存有每种卡牌的数量)
        JObject typeData = JObject.Parse(jsonData["blockTypeData"].ToString());
        //定义生成卡牌类别的数组
        List<int> rList = new List<int>();
        foreach (KeyValuePair<string, JToken> m in typeData)
        {
            int typeNum = int.Parse(m.Key.ToString());
            int typeCount = int.Parse(m.Value.ToString()) * 3;
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

        int level = 1;
        int idx = 0;
        List<string> idList = new List<string>();
        foreach (KeyValuePair<string, JToken> m in levelData)
        {
            //获取卡牌数组
            List<int> subLevelList = new List<int>();
            JArray subList = JArray.Parse(m.Value.ToString());
            foreach (JToken j in subList)
            {
                List<int> mapSubList = new List<int>();
                mapSubList.Add(level);
                mapSubList.Add(int.Parse(j["rolNum"].ToString()));
                mapSubList.Add(int.Parse(j["rowNum"].ToString()));
                subLevelList.Add(idx);
                mapSubList.Add(rList[idx++]);
                mapList.Add(mapSubList);
                idList.Add(j["id"].ToString());
                GameObject o = Instantiate(MapDataGetter.mapData.card, content.transform);
                o.GetComponent<Image>().sprite = GameManager.Gm.cards[mapSubList[3] - 1];
                Card c = o.GetComponent<Card>();
                c.type = mapSubList[3] - 1;
                c.id = j["id"].ToString();
                c.xPos = mapSubList[1];
                c.yPos = mapSubList[2];
                c.status = Card.st.OnMap;
                c.level = level;
                c.cid = idx - 1;
                c.ShowId();
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
            pLevelMap.Add(subLevelList);
            level++;
        }
        content.GetComponent<RectTransform>().anchoredPosition = new Vector2(360, 300);
        for (int i = 0; i < mapList.Count; i++)
        {
            isOnMap.Add(true);
        }

        for (int i = 0; i < pLevelMap.Count; i++)
        {
            string s = "";
            for (int j = 0; j < pLevelMap[i].Count; j++)
            {
                s += pLevelMap[i][j].ToString() + " ";
            }
            print(s);
        }

        for (int i = 0; i < mapList.Count; i++)
        {
            string w = "";
            for (int j = 0; j < mapList[i].Count; j++)
            {
                w += mapList[i][j].ToString() + " ";
            }
            print(w);
        }
        UpdateAvailableShowInBoard(24);
    }

    void UpdateAvailableShowInBoard(int level)
    {
        for (int i = 0; i < level; i++)
        {
            for (int j = 0; j < pLevelMap[i].Count; j++)
            {
                if (cards[pLevelMap[i][j]].status == Card.st.OnBoard) continue;
                else
                {
                    cards[pLevelMap[i][j]].status = Card.st.onMapZ;
                    cards[pLevelMap[i][j]].GetComponent<Image>().color = Color.gray;
                    bool flag = false;
                    for (int m = i + 1; m < level; m++)
                    {
                        for (int n = 0; n < pLevelMap[m].Count; n++)
                        {
                            if (cards[pLevelMap[m][n]].status == Card.st.OnBoard) continue;
                            if (Mathf.Abs(mapList[pLevelMap[m][n]][1] - mapList[pLevelMap[i][j]][1]) < 8 &&
                                Mathf.Abs(mapList[pLevelMap[m][n]][2] - mapList[pLevelMap[i][j]][2]) < 8)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if(flag)break;
                    }

                    if (!flag)
                    {
                        cards[pLevelMap[i][j]].GetComponent<Image>().color = Color.white;
                        cards[pLevelMap[i][j]].status = Card.st.OnMap;
                    }
                }
            }
        }
    }

    void UpdateAvailable(int level)
    {
        avMap = new List<int>();
        for (int i = 0; i < level; i++)
        {
            for (int j = 0; j < pLevelMap[i].Count; j++)
            {
                if (!isOnMap[pLevelMap[i][j]]) continue;
                else
                {
                    bool flag = false;
                    for (int m = i + 1; m < level; m++)
                    {
                        for (int n = 0; n < pLevelMap[m].Count; n++)
                        {
                            if (!isOnMap[pLevelMap[m][n]]) continue;
                            if (Mathf.Abs(mapList[pLevelMap[m][n]][1] - mapList[pLevelMap[i][j]][1]) < 8 &&
                                Mathf.Abs(mapList[pLevelMap[m][n]][2] - mapList[pLevelMap[i][j]][2]) < 8)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if(flag)break;
                    }
                    if(!flag)avMap.Add(pLevelMap[i][j]);
                }
            }
        }
    }
    public void StartAni()
    {
        if (!isLoaded)
        {
            ShowTipMessage.Tip.ShowTip("请先加载数据");
            return;
        }
        if (!isCalc)
        {
            ShowTipMessage.Tip.ShowTip("请先进行计算");
            return;
        }
        isLoaded = false;
        isCalc = false;
        //初始化数组,全部置零
        boardType = new int[GameManager.Gm.cards.Length];
        for (int i = 0; i < GameManager.Gm.cards.Length; i++)
        {
            boardType[i] = 0;
        }
        boardOrder = new List<int>();
        isOnShowMap = new List<bool>();
        for (int i = 0; i < cards.Count; i++)
        {
            isOnShowMap.Add(true);
        }

        for (int i = 0; i < boardObject.Length; i++)
        {
            boardObject[i].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
        }
        aniStep = 0;
    }
    //主动画的协程
    public void MainAni()
    {
        if (aniStep == maxStep.Count)
        {
            ShowTipMessage.Tip.ShowTip("已经是最后一步");
            return;
        }
        if(GameManager.Gm.gameStatus != GameManager.GameStatus.PlayingA)return;
        //获取当前步骤的基本信息
        int cardId = maxStep[aniStep];
        int type = mapList[cardId][3] - 1;
        int level = mapList[cardId][0];
        cards[cardId].DfsDown();
        isOnShowMap[cardId] = false;
        UpdateAvailableShowInBoard(24);
        //查找应当移动到的位置
        int orderInBoard = -1;
        //是否列表中已存在
        bool isNeedMove = false;
        for (int i = 0; i < boardOrder.Count; i++)
        {
            //和本类型匹配
            if (boardOrder[i] == type) orderInBoard = i;
        }
        //如果在列表中存在则重新计算
        
        if (orderInBoard != -1)
        {
            orderInBoard = 0;
            isNeedMove = true;
            
            for (int i = 0; i < boardOrder.Count; i++)
            {
                //和本类型匹配
                if (boardOrder[i] == type)
                {
                    //找到type的数量
                    orderInBoard += boardType[boardOrder[i]];
                    break;
                }
                //将type的数量计入数据
                orderInBoard += boardType[boardOrder[i]];
            }
        }
        else
        {
            //放到末尾
            orderInBoard = boardNumber;
            //将此卡牌放到最后
            boardOrder.Add(type);
        }
        //所有的格子后移
        //判断是否需要后移
        if (type == boardOrder[boardOrder.Count - 1]) isNeedMove = false;
        print(isNeedMove);
        if (isNeedMove)
        {
            //将boardOrder之后的元素后移一个单位
            //保存后面元素的类型属性
            List<int> moveArray = new List<int>();
            bool flag = false;//是否第一次匹配到卡牌
            for (int i = 0; i < boardOrder.Count; i++)
            {
                if (boardOrder[i] == type) flag = true;
                if (flag == true&&boardOrder[i] != type)
                {
                    for (int j = 0; j < boardType[boardOrder[i]]; j++)
                    {
                        moveArray.Add(boardOrder[i]);
                    }
                }
            }
            //将空位的Sprite制空
            boardObject[orderInBoard].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
            //将之后的Sprite重新赋值
            for (int i = 0; i < moveArray.Count; i++)
            {
                boardObject[orderInBoard + i + 1].GetComponent<Image>().sprite = GameManager.Gm.cards[moveArray[i]];
            }
        }

        //定义移动到版面的位置向量
        Vector2 pos = boardObject[orderInBoard].transform.position;
        //将物体移动到版面
        cards[cardId].transform.DOMove(pos,1.0f);
        //开始协程定时
        //需要一个协程在动画播放结束之后执行一些方法
        boardType[type]++;
        bool isPop;
        if (boardType[type] == 3) isPop = true;
        else isPop = false;
        StartCoroutine(AfterMove(cards[cardId].gameObject,orderInBoard,type,isPop,orderInBoard));
        boardNumber++;
        
        //消除的逻辑
        if (boardType[type] == 3)
        {
            //设置数据
            boardType[type] = 0;
            boardNumber -= 3;
            boardOrder.Remove(type);
        }
        aniStep++;
        ShowTipMessage.Tip.ShowTip("第" + aniStep + "步/共" + maxStep.Count + "步");
    }
    IEnumerator AfterMove(GameObject o,int pos,int type,bool isPop,int orderInBoard)
    {
        //等待1.0秒执行
        yield return new WaitForSeconds(1.0f);
        if (showBoardOrder.Count >= pos)
        {
            o.SetActive(false);
            boardObject[pos].GetComponent<Image>().sprite = GameManager.Gm.cards[type];
            showBoardOrder.Insert(pos, type);
            if (isPop == true)
            {
                while (showBoardOrder.FindAll(t => t == type).Count > 0)
                {
                    showBoardOrder.Remove(type);
                }
            }

            for (int i = 0; i < 7; i++)
            {
                if (i < showBoardOrder.Count)
                {
                    boardObject[i].GetComponent<Image>().sprite = GameManager.Gm.cards[showBoardOrder[i]];
                }
                else
                {
                    boardObject[i].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
                }
            }

            yield break;
        }
    }
    void dfs(int pos1, int pos2, int pos3, int pos4, int pos5, int pos6, int pos7)
    {
        dfsMaxLength = Mathf.Max(dfsLength, dfsMaxLength);
        cT++;
        if (cT > targetTime) return;
        UpdateAvailable(24);
        //拷贝avMap中的数据(深拷贝)
        List<int> cAvMap = new List<int>();
        for (int i = 0; i < avMap.Count; i++)
        {
            cAvMap.Add(avMap[i]);
        }
        //计算版面的卡牌个数
        int boardCount = 7;
        List<int> posList = new List<int>(){pos1,pos2,pos3,pos4,pos5,pos6,pos7};
        for (int i = 0; i < 7; i++)
        {
            if (posList[i] == 0)
            {
                boardCount = i;
                break;
            }
        }
        //判断格子已满的状态
        if (boardCount == 7)
        {
            if (step.Count >= maxStep.Count)
            {
                maxStep = new List<int>();
                for (int i = 0; i < step.Count; i++)
                {
                    maxStep.Add(step[i]);
                }
            }
            return;
        }
        //计算版面中各类卡牌的出现次数
        Dictionary<int, int> boardTypeCount = new Dictionary<int, int>();
        for (int i = 0; i < 7; i++)
        {
            if (!boardTypeCount.ContainsKey(posList[i]))
            {
                boardTypeCount.Add(posList[i],1);
            }
            else
            {
                boardTypeCount[posList[i]]++;
            }
        }
        if (boardTypeCount.Count == 6)return;
        //根据场上的卡牌生成卡牌类型的集合
        List<int> curTypeList = new List<int>();
        for (int i = 0; i < cAvMap.Count; i++)
        {
            curTypeList.Add(mapList[cAvMap[i]][3]);
        }
        //计算场上各类型卡牌的出现次数
        Dictionary<int, int> curTypeCounter = new Dictionary<int, int>();
        for (int i = 0; i < curTypeList.Count; i++)
        {
            if (!curTypeCounter.ContainsKey(curTypeList[i]))
            {
                curTypeCounter.Add(curTypeList[i],1);
            }
            else
            {
                curTypeCounter[curTypeList[i]]++;
            }
        }

        List<int> popCard = new List<int>();
        List<int> popIdList = new List<int>();
        if (boardCount <= 4)
        {
            //找到可以消除的类型
            foreach (KeyValuePair<int,int> i in curTypeCounter)
            {
                if(i.Value >= 3)popCard.Add(i.Key);
            }
            //找到可以消除类型的卡牌
            for (int i = 0; i < popCard.Count; i++)
            {
                int tcCount = 0;
                for (int j = 0; j < cAvMap.Count; j++)
                {
                    if(tcCount == 3)break;
                    if (mapList[cAvMap[j]][3] == popCard[i])
                    {
                        tcCount = tcCount + 1;
                        popIdList.Add(cAvMap[j]);
                    }
                }
            }
            //执行消除操作
            for (int i = 0; i < popIdList.Count; i++)
            {
                isOnMap[popIdList[i]] = false;
                step.Add(popIdList[i]);
            }
            UpdateAvailable(24);
            cAvMap = new List<int>();
            for (int i = 0; i < avMap.Count; i++)
            {
                cAvMap.Add(avMap[i]);
            }
        }

        Dictionary<int, int> scoreDict = new Dictionary<int, int>();
        for (int i = 0; i < cAvMap.Count; i++)
        {
            //计算卡牌被点击之后开启的卡牌数目
            isOnMap[cAvMap[i]] = false;
            UpdateAvailable(mapList[cAvMap[i]][0]);
            int c = avMap.Count - cAvMap.Count;
            //计算当前版面的潜在价值
            List<int> pTypeList = new List<int>();
            for (int j = 0; j < avMap.Count; j++)
            {
                pTypeList.Add(mapList[avMap[j]][3]);
            }
            Dictionary<int, int> pTypeCount = new Dictionary<int, int>();
            for (int j = 0; j < pTypeList.Count; j++)
            {
                if (!pTypeCount.ContainsKey(pTypeList[j]))
                {
                    pTypeCount.Add(pTypeList[j],1);
                }
                else
                {
                    pTypeCount[pTypeList[j]]++;
                }
            }
            //根据开启的两对数目和三对数目计算得分
            int p = 0;
            foreach (KeyValuePair<int,int> j in pTypeCount)
            {
                if (j.Value == 2) p += 100;
                else if (j.Value >= 3) p += 1000; 
            }
            //计算场上的卡牌与版面卡牌的匹配情况
            foreach (KeyValuePair<int,int> j in pTypeCount)
            {
                if (boardTypeCount.ContainsKey(j.Key))
                {
                    if (j.Value + boardTypeCount[j.Key] == 2) p += 100;
                    else if (j.Value + boardTypeCount[j.Key] >= 3) p += 1000;
                }
            }
            //恢复状态
            isOnMap[cAvMap[i]] = true;
            //计算总得分
            int cType = 0;
            if(boardTypeCount.ContainsKey(mapList[cAvMap[i]][3]))cType = boardTypeCount[mapList[cAvMap[i]][3]];
            int score = boardScore[cType] + p * 2;
            scoreDict.Add(cAvMap[i],score);
        }
        //为计算的得分排序
        Dictionary<int,int> sortedDict = scoreDict.OrderBy(x => x.Value).ToDictionary(x => x.Key,x=>x.Value);
        //遍历状态开始dfs
        foreach (KeyValuePair<int, int> i in sortedDict)
        {
            int cardId = i.Key;
            int cardType = mapList[cardId][3];
            isOnMap[cardId] = false;
            UpdateAvailable(mapList[cardId][0]);
            List<int> tPosList = new List<int>();
            for (int j = 0; j < posList.Count; j++)
            {
                tPosList.Add(posList[j]);
            }
            if (!boardTypeCount.ContainsKey(cardType)||boardTypeCount[cardType] != 2)
            {
                tPosList[boardCount] = cardType;
            }
            else
            {
                tPosList.Remove(cardType);
                tPosList.Remove(cardType);
                tPosList.Add(0);
                tPosList.Add(0);
            }
            step.Add(cardId);
            dfsLength++;
            dfs(tPosList[0],tPosList[1],tPosList[2],tPosList[3],tPosList[4],tPosList[5],tPosList[6]);
            dfsLength--;
            isOnMap[cardId] = true;
            step.RemoveAt(step.Count - 1);
        }
        for (int i = 0; i < popIdList.Count; i++)
        {
            isOnMap[popIdList[i]] = true;
            step.RemoveAt(step.Count - 1);
        }
    }

    public void BackToMenu()
    {
        GameManager.Gm.gameStatus = GameManager.GameStatus.OnMenu;
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].gameObject.SetActive(false);
        }
        cards = new List<Card>();
        for (int i = 0; i < 7; i++)
        {
            boardObject[i].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
        }
        GameManager.Gm.playingPanelA.SetActive(false);
        GameManager.Gm.menuPanel.SetActive(true);
    }
}
