using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Random = UnityEngine.Random;

public class DPlayManager : MonoBehaviour
{
    //变量定义
    #region Var
    public enum WaitingStatus
    {
        WaitingUser,MeetUser,Online,Offline,OnPlaying
    }
    public GameObject wPanel;
    //是否是自己的回合
    public bool isMyTime = false;
    //等待界面对象
    public GameObject waitingPanel;
    //等待界面的文字
    public Text waitingText;
    //对手的用户名
    public string opUser;
    //自身的单例类
    public static DPlayManager PlayManager;
    //双人游戏的状态
    public WaitingStatus status;
    //接收用户邀请的按钮
    public Button acceptBtn;
    //拒绝用户邀请的按钮
    public Button refuseBtn;
    //自身的卡牌列表
    public List<int> myCardList;
    //对方的卡牌列表
    public List<int> opCardList;
    //自身显示的卡牌列表
    public List<int> myCardListInShow;
    //对方显示卡牌的列表
    public List<int> opCardListInShow;
    //是否被需要更新
    public bool needUpDateMapMy = false;
    public bool needUpDateMapOp = false;
    public bool needUpDateShowMy = false;
    public bool needUpDateShowOp = false;
    public string updateId;
    //版面上的物体
    public GameObject[] myCardBoardObjectList;
    public GameObject[] opCardBoardObjectList;
    //根据string类的CardId找到Card的索引
    public Dictionary<string, int> CardIdToIndex;
    //生成卡牌的界面
    public GameObject dCardContent;
    public GameObject dCard;
    //界面中的卡牌列表
    public List<DCard> dCards;
    //卡牌的最大层数
    public int maxLevel;
    //每层的对应卡牌编号
    public List<List<int>> LevelCard;
    #endregion

    //工具函数
    #region UserFunction
    //找到最后一个元素返回索引
    public int FindLastInList(int cardType,List<int> cardList)
    {
        int pos = -1;
        for (int i = 0; i < cardList.Count; i++)
        {
            if (cardList[i] == cardType) pos = i;
        }
        return pos;
    }
    //找到第一个元素返回索引
    public int FindFirstInList(int cardType,List<int> cardList)
    {
        int pos = -1;
        for (int i = 0; i < cardList.Count; i++)
        {
            if (cardList[i] == cardType)
            {
                pos = i;
                break;
            }
        }
        return pos;
    }
    //找到三个连着的数字并返回
    public int FindThreeInList(List<int> cardList)
    {
        int foundCount = 0;
        int prId = -1;
        for (int i = 0; i < cardList.Count; i++)
        {
            //和之前的type不相等
            if (cardList[i] != prId)
            {
                prId = cardList[i];
                foundCount = 1;
            }
            //和之前的cardType相等
            else
            {
                foundCount++;
                if (foundCount == 3) return i;
            }
        }
        return -1;
    }
    #endregion
    
    private void Awake()
    {
        status = WaitingStatus.Offline;
        PlayManager = GetComponent<DPlayManager>();
    }
    public void WaitUser(string user)
    {
        opUser = user;
        //打开等待的面板
        waitingPanel.SetActive(true);
        waitingText.text = "等待用户" + user + "的连接";
        acceptBtn.gameObject.SetActive(false);
        refuseBtn.gameObject.SetActive(false);
    }
    public void UserAccept(int randomSeed)
    {
        ShowTipMessage.Tip.ShowTip("用户" + opUser + "已同意");
        Boolean b = new bool();
        b = true;
        EnterBtn(b,randomSeed);
    }
    public void MeetUser(string user)
    {
        //ShowTipMessage.Tip.ShowTip("用户" + opUser + "请求连接");
        waitingPanel.SetActive(true);
        opUser = user;
        waitingText.text = "用户" + user + "请求连接";
        acceptBtn.gameObject.SetActive(true);
        refuseBtn.gameObject.SetActive(true);
    }
    public void UserRefuse()
    {
        ShowTipMessage.Tip.ShowTip("用户"+opUser+"拒绝连接");
        waitingPanel.SetActive(false);
        opUser = "";
        status = WaitingStatus.Online;
    }
    public void EnterBtn(bool t,int randomSeed)
    {
        //建立双人场景
        waitingPanel.SetActive(false);
        wPanel.SetActive(false);
        status = WaitingStatus.OnPlaying;
        GameManager.Gm.playingPanelB.SetActive(true);
        GameManager.Gm.gameStatus = GameManager.GameStatus.PlayingB;
        if (t == true) isMyTime = true;
        else
        {
            isMyTime = false;
        }
        isMyTime = t;
        //加载场景
        //初始化数组
        dCards = new List<DCard>();
        CardIdToIndex = new Dictionary<string, int>();
        myCardList = new List<int>();
        opCardList = new List<int>();
        myCardListInShow = new List<int>();
        opCardListInShow = new List<int>();
        LevelCard = new List<List<int>>();
        //从MapGetter中获取游戏地图数据
        JObject jsonData = MapDataGetter.mapData.jsonData;
        RectTransform conT = dCardContent.GetComponent<RectTransform>();
        //解析Json中的levelData数据(存有每个卡牌的位置)
        JObject levelData = JObject.Parse(jsonData["levelData"].ToString());
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
        Random.InitState(1000);
        for (int i = 0; i < rList.Count; i++)
        {
            int index = Random.Range(i, rList.Count);
            int tmp = rList[i];
            int ran = rList[index];
            rList[i] = ran;
            rList[index] = tmp;
        }
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
                GameObject o = Instantiate(dCard,dCardContent.transform);
                //获取卡牌的类型
                int cardNum = rList[idx++] - 1;
                //获取卡牌类型相对应的图片
                o.GetComponent<Image>().sprite = GameManager.Gm.cards[cardNum];
                //获取Card组件
                DCard c = o.GetComponent<DCard>();
                CardIdToIndex.Add(j["id"].ToString(),idx - 1);
                //将Json中信息转换成csv
                parserCsv += j["id"].ToString() + "," + level + "," + j["rowNum"] + "," + j["rolNum"] + "\n";
                //设置生成状态
                c.type = cardNum;
                c.id = j["id"].ToString();
                c.xPos = int.Parse(j["rowNum"].ToString());
                c.yPos = int.Parse(j["rolNum"].ToString());
                c.status = DCard.st.OnMap;
                c.level = level;
                c.SetButton();
                //将card的序号加入层数组
                subLevelList.Add(idx - 1);
                //将Card组件加入列表
                dCards.Add(o.GetComponent<DCard>());
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
        dCardContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(360, 300);
    }
    //向对方发送同意的请求
    public void Accept()
    {
        int seed = Random.Range(1, 1000000);
        Client.clientManager.AcceptUser(opUser,seed);
        EnterBtn(false,seed);
    }
    //向对方发送拒接的请求
    public void Refuse()
    {
        Client.clientManager.RefuseUser(opUser);
        waitingPanel.SetActive(false);
    }
    private void Update()
    {
        //更新列表
        if (status != WaitingStatus.OnPlaying) return;
        //自身的版面需要更新
        if (needUpDateShowMy)
        {
            needUpDateShowMy = false;
            //根据数组更新显示
            for (int i = 0; i < myCardListInShow.Count; i++)
            {
                int cardType = myCardListInShow[i];
                myCardBoardObjectList[i].GetComponent<Image>().sprite = GameManager.Gm.cards[cardType];
            }
            //更新剩余的空白项
            for (int i = myCardListInShow.Count; i < 7; i++)
            {
                myCardBoardObjectList[i].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
            }
        }
        //对方的版面需要更新
        if (needUpDateShowOp)
        {
            needUpDateShowOp = false;
            //根据数组更新显示
            for (int i = 0; i < opCardListInShow.Count; i++)
            {
                int cardType = opCardListInShow[i];
                opCardBoardObjectList[i].GetComponent<Image>().sprite = GameManager.Gm.cards[cardType];
            }
            //更新剩余的空白项
            for (int i = opCardListInShow.Count; i < 7; i++)
            {
                opCardBoardObjectList[i].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
            }
        }
        //我方点击
        if (needUpDateMapMy)
        {
            needUpDateMapMy = false;
            //1.通过id解析内容
            int cardId = CardIdToIndex[updateId];
            Card c = dCards[cardId];
            int type = c.type;
            //2.计算卡牌落下的位置
            int pos;
            if (FindLastInList(type, myCardList) == -1) pos = myCardList.Count;
            else pos = FindLastInList(type, myCardList) + 1;
            //3.修改Board显示、地图遮挡显示,开始卡牌动画
            UpdateCardStatus(c.level);
            for (int i = pos; i < myCardList.Count; i++)
            {
                myCardBoardObjectList[i + 1].GetComponent<Image>().sprite = GameManager.Gm.cards[myCardList[i]];
            }
            myCardBoardObjectList[pos].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
            Vector3 tarPos = myCardBoardObjectList[pos].transform.position;
            c.gameObject.transform.DOMove(tarPos,1.0f);
            //4.更新CardList数组(消除逻辑)
            myCardList.Insert(pos,type);
            if (FindThreeInList(myCardList) != -1)
            {
                while (myCardList.FindAll(t => t == type).Count > 0)
                {
                    myCardList.Remove(type);
                }
            }
            //5.开启协程定时更新CardListInShow数组(插入并消除)
            StartCoroutine(UpdateMyAnimation(c.gameObject, type));
        }
        //对方点击
        if (needUpDateMapOp)
        {
            needUpDateMapOp = false;
            //1.通过id解析内容
            int cardId = CardIdToIndex[updateId];
            Card c = dCards[cardId];
            int type = c.type;
            //2.计算卡牌落下的位置
            int pos;
            if (FindLastInList(type, opCardList) == -1) pos = opCardList.Count;
            else pos = FindLastInList(type, opCardList) + 1;
            //3.修改Board显示,开始卡牌动画
            UpdateCardStatus(c.level);
            for (int i = pos; i < opCardList.Count; i++)
            {
                opCardBoardObjectList[i + 1].GetComponent<Image>().sprite = GameManager.Gm.cards[opCardList[i]];
            }
            opCardBoardObjectList[pos].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
            Vector3 tarPos = opCardBoardObjectList[pos].transform.position;
            c.gameObject.transform.DOMove(tarPos,1.0f);
            //4.更新CardList数组(消除逻辑)
            opCardList.Insert(pos,type);
            if (FindThreeInList(opCardList) != -1)
            {
                while (opCardList.FindAll(t => t == type).Count > 0)
                {
                    opCardList.Remove(type);
                }
            }
            //5.开启协程定时更新CardListInShow数组(插入并消除)
            StartCoroutine(UpdateOpAnimation(c.gameObject,type));
        }
    }
    //开始协程等待更新
    IEnumerator UpdateMyAnimation(GameObject o, int type)
    {
        //等待1秒之后执行
        yield return new WaitForSeconds(1.0f);
        //设置待更新的卡牌
        needUpDateShowMy = true;
        //将新增的卡牌加入,并计算消除逻辑
        int pos = FindLastInList(type,myCardListInShow);
        if (pos == -1) pos = myCardListInShow.Count;
        //将存储的卡牌插入显示的数组
        myCardListInShow.Insert(pos,type);
        //判断卡牌三消
        if (FindThreeInList(myCardListInShow) != -1)
        {
            while (myCardListInShow.FindAll(t => t == type).Count > 0)
            {
                myCardListInShow.Remove(type);
            }
        }
        o.gameObject.SetActive(false);
        yield break;
    }
    IEnumerator UpdateOpAnimation(GameObject o, int type)
    {
        //等待1秒之后执行
        yield return new WaitForSeconds(1.0f);
        //设置待更新的卡牌
        needUpDateShowOp = true;
        //将新增的卡牌加入,并计算消除逻辑
        int pos = FindLastInList(type,opCardListInShow);
        if (pos == -1) pos = opCardListInShow.Count;
        //将存储的卡牌插入显示的数组
        opCardListInShow.Insert(pos,type);
        //判断卡牌三消
        if (FindThreeInList(opCardListInShow) != -1)
        {
            while (opCardListInShow.FindAll(t => t == type).Count > 0)
            {
                opCardListInShow.Remove(type);
            }
        }
        o.gameObject.SetActive(false);
        yield break;
    }
    //更行地图上的卡牌的遮挡关系
    //参数为当前的最高层
    public void UpdateCardStatus(int level)
    {
        //将每一个卡牌更新(不需要更新比当前高层的卡牌)
        for (int i = 0; i < level; i++)
        {
            for (int j = 0;j < LevelCard[i].Count; j++)
            {
                UpdateCardStatus(dCards[LevelCard[i][j]]);
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
                Card tc = dCards[LevelCard[i][j]];
                if(tc.status == Card.st.OnBoard)continue;
                //判断卡牌是否在遮挡范围内
                if (Mathf.Abs(tc.xPos - c.xPos) < 8 && Mathf.Abs(tc.yPos - c.yPos) < 8)
                {
                    //设置被遮挡之后的状态
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
    public void FetchCard(string id,bool isMyOpt)
    {
        //通过id解析卡牌信息
        int index = CardIdToIndex[id];
        DCard dCard = dCards[index];
        int type = dCard.type;
        //我方出牌的相关操作
        if (isMyOpt)
        {
            //显示消息提示
            ShowTipMessage.Tip.ShowTip("等待对方出牌");
            needUpDateMapMy = true;
            updateId = id;
        }
        else
        {
            //显示消息提示
            ShowTipMessage.Tip.ShowTip("对方已出牌");
            //设置牌面信息
            dCard.dStatus = DCard.Dst.OnOpBoard;
            dCard.status = Card.st.OnBoard;
            needUpDateMapOp = true;
            isMyTime = true;
            updateId = id;
        }
        
    }

    public void BackToMenu()
    {
        for (int i = 0; i < dCards.Count; i++)
        {
            dCards[i].gameObject.SetActive(false);
        }
        dCards = new List<DCard>();
        myCardList = new List<int>();
        opCardList = new List<int>();
        myCardListInShow = new List<int>();
        opCardListInShow = new List<int>();
        for (int i = 0; i < myCardBoardObjectList.Length; i++)
        {
            myCardBoardObjectList[i].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
        }
        for (int i = 0; i < opCardBoardObjectList.Length; i++)
        {
            opCardBoardObjectList[i].GetComponent<Image>().sprite = GameManager.Gm.noneObjSprite;
        }
        waitingPanel.SetActive(false);
        wPanel.SetActive(false);
        GameManager.Gm.gameStatus = GameManager.GameStatus.OnMenu;
        GameManager.Gm.menuPanel.SetActive(true);
        GameManager.Gm.playingPanelB.SetActive(false);
    }
}

