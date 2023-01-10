using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //单例对象
    public static GameManager Gm;
    //获取地图的脚本对象
    public MapDataGetter mapData;
    //游戏提示的类
    public ShowTipMessage tip;
    //卡牌的图片数组
    public Sprite[] cards;
    //版面上的物体
    public GameObject[] boardObject;
    //版面上物体的数量
    public int boardNumber;
    //版面上每个种类物体的数目
    public int[] boardType;
    //版面上物体的顺序
    public List<int> boardOrder;
    //实际显示中版面上物体顺序
    public List<int> showBoardOrder;
    //显示为空的Sprite
    public Sprite noneObjSprite;
    //客户端的Socket单例类
    public Client client;
    //登录状态
    public bool isLogin;
    public string userId;
    //游戏状态
    public enum GameStatus {OnMenu,PlayingS,PlayingA,PlayingAI,PlayingB,OnWaiting,Pause };
    public GameStatus gameStatus;
    //游戏面板
    public GameObject menuPanel;
    public GameObject playingPanelS;
    public GameObject playingPanelA;
    public GameObject playingPanelAI;
    public GameObject playingPanelB;
    public Text pauseButton;
    public GameObject lostPanel;
    //游戏大厅管理
    public GameObject userContent;
    public GameObject playerRoomPanel;
    //在线用户列表
    public struct UserItemStruct
    {
        public string User;
        public bool IsOnline;
    }
    public List<UserItemStruct> onlineUsers;
    public GameObject onlineUserItemGameObject;
    public List<GameObject> onlineUserGameObjectList;
    //排行榜管理
    public GameObject rankListPanel;
    public GameObject rankListItem;
    public GameObject rankListContent;
    //设置管理
    public GameObject settingsPanel;
    //声音设置
    public AudioClip[] audioClips;
    public AudioSource audioSource;
    //登录注册面板
    public GameObject loginPanel;
    public GameObject regPanel;
    private void Awake()
    {
        //初始化GameManager的单例
        Gm = GetComponent<GameManager>();
        //初始化地图获取的脚本
        mapData.Init();
        //初始化面板
        InitBoard();
        //初始化消息提示
        tip.Init();
        //初始化在线玩家的两个List
        onlineUsers = new List<UserItemStruct>();
        onlineUserGameObjectList = new List<GameObject>();
        isLogin = false;
        gameStatus = GameStatus.OnMenu;
    }
    public void InitBoard()
    {
        //初始化数组,全部置零
        boardType = new int[cards.Length];
        for (int i = 0; i < cards.Length; i++)
        {
            boardType[i] = 0;
        }
        boardOrder = new List<int>();
    }
    //开始按钮的点击事件
    public void GameStart()
    {
        //生成卡牌
        mapData.GenerateCards();
        playingPanelS.SetActive(true);
        menuPanel.SetActive(false);
        gameStatus = GameStatus.PlayingS;
        for (int i = 0; i < 7; i++)
        {
            boardObject[i].GetComponent<Image>().sprite = noneObjSprite;
        }
    }
    //将卡牌移动到版面
    public void EnterBoard(int type,GameObject o,int level)
    {
        //更新遮挡关系
        MapDataGetter.mapData.UpdateCardStatus(level);
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
            boardObject[orderInBoard].GetComponent<Image>().sprite = noneObjSprite;
            //将之后的Sprite重新赋值
            for (int i = 0; i < moveArray.Count; i++)
            {
                boardObject[orderInBoard + i + 1].GetComponent<Image>().sprite = cards[moveArray[i]];
            }
        }

        //定义移动到版面的位置向量
        Vector2 pos = boardObject[orderInBoard].transform.position;
        //将物体移动到版面
        o.transform.DOMove(pos,1.0f);
        //开始协程定时
        //需要一个协程在动画播放结束之后执行一些方法
        boardType[type]++;
        bool isPop;
        if (boardType[type] == 3) isPop = true;
        else isPop = false;
        StartCoroutine(AfterMove(o,orderInBoard,type,isPop,orderInBoard));
        boardNumber++;
        
        //消除的逻辑
        if (boardType[type] == 3)
        {

            //设置数据
            boardType[type] = 0;
            boardNumber -= 3;
            boardOrder.Remove(type);
        }
        //游戏失败
        if (boardNumber == 7)
        {
            gameStatus = GameStatus.Pause;
            lostPanel.SetActive(true);
            print("游戏失败");
        }
    }
    IEnumerator AfterMove(GameObject o,int pos,int type,bool isPop,int orderInBoard)
    {
        //等待1.0秒执行
        yield return new WaitForSeconds(1.0f);
        //将初始的物体关闭
        
        //将版面上的空物体显示对应的图像
        //刷新版面的图像(弃用)
        //int objCount = 0;
        //for (int i = 0; i < boardOrder.Count; i++)
        //{
        //    int m = boardType[boardOrder[i]];
        //    for (int j = 0; j < m; j++)
        //    {
        //        boardObject[objCount++].GetComponent<Image>().sprite = cards[boardOrder[i]];
        //    }
        //}
        ////将剩余的图片置空
        //for (int i = objCount; i < 7; i++)
        //{
        //    boardObject[i].GetComponent<Image>().sprite = noneObjSprite;
        //}
        if (showBoardOrder.Count >= pos)
        {
            o.SetActive(false);
            boardObject[pos].GetComponent<Image>().sprite = cards[type];
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
                    boardObject[i].GetComponent<Image>().sprite = cards[showBoardOrder[i]];
                }
                else
                {
                    boardObject[i].GetComponent<Image>().sprite = noneObjSprite;
                }
            }

            yield break;
        }
    }

    public void BackToMenu()
    {
        playingPanelA.SetActive(false);
        playingPanelB.SetActive(false);
        playerRoomPanel.SetActive(false);
        playingPanelS.SetActive(false);
        playingPanelAI.SetActive(false);
        gameStatus = GameStatus.OnMenu;
        menuPanel.SetActive(true);
    }

    public void ShowRank()
    {
        rankListPanel.SetActive(true);
    }

    public void Login()
    {
        loginPanel.SetActive(true);
    }

    public void Reg()
    {
        regPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);   
    }

    public void StartGameAuto()
    {
        playingPanelA.SetActive(true);
        menuPanel.SetActive(false);
        gameStatus = GameStatus.PlayingA;
    }

    public void StartGameAI()
    {
        playingPanelAI.SetActive(true);
        menuPanel.SetActive(false);
    }

    public void StartGameBoth()
    {
        
    }

    public void OpenPlayerRoom()
    {
        
        print("进入游戏大厅");
        if (isLogin == true)
        {
            playerRoomPanel.SetActive(true);
            ShowTipMessage.Tip.ShowTip("用户"+userId+"你好");
            //初始化网络连接
            client.InitClient();
            client.GetUserList();
            //发送消息获取玩家列表
            gameStatus = GameStatus.OnWaiting;
            DPlayManager.PlayManager.status = DPlayManager.WaitingStatus.Online;
            menuPanel.SetActive(false);
        }
        else
        {
            ShowTipMessage.Tip.ShowTip("用户未登录");
        }
    }

    public void RefreshOnlineList()
    {
        for (int i = 0; i < onlineUsers.Count; i++)
        {
            if (i < onlineUserGameObjectList.Count)
            {
                onlineUserGameObjectList[i].SetActive(true);
                OnlineUserItem item = onlineUserGameObjectList[i].GetComponent<OnlineUserItem>();
                item.Init(onlineUsers[i].User,onlineUsers[i].IsOnline);
            }
            else
            {
                GameObject o = Instantiate(onlineUserItemGameObject, userContent.transform);
                onlineUserGameObjectList.Add(o);
                OnlineUserItem item = o.GetComponent<OnlineUserItem>();
                item.Init(onlineUsers[i].User, onlineUsers[i].IsOnline);
            }
        }

        for (int i = onlineUsers.Count; i < onlineUserGameObjectList.Count; i++)
        {
            onlineUserGameObjectList[i].SetActive(false);
        }
    }

    public void PlayingPause()
    {
        if (gameStatus == GameStatus.PlayingS)
        {
            gameStatus = GameStatus.Pause;
            pauseButton.text = "继续";
        }

        else if (gameStatus == GameStatus.Pause)
        {
            gameStatus = GameStatus.PlayingS;
            pauseButton.text = "暂停";
        }

    }
    public void PlayingBack()
    {
        lostPanel.SetActive(false);
        gameStatus = GameStatus.OnMenu;
        playingPanelS.SetActive(false);
        menuPanel.SetActive(true);
        mapData.ResetCard();
        InitBoard();
        boardNumber = 0;
        showBoardOrder = new List<int>();
        for (int i = 0; i < 7; i++)
        {
            boardObject[i].GetComponent<Image>().sprite = noneObjSprite;
        }
    }

}
