using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    //卡牌的状态
    public enum st { OnMap,OnBoard,onMapZ};
    //层数
    public int level;
    //状态
    public st status;
    public string id;
    public int type;
    public int xPos;
    public int yPos;
    public Button btn;
    public Text idText;
    public int cid;
    //给自身的Button组件注册点击事件
    public void SetButton()
    {
        btn.onClick.AddListener(MouseDown);
    }
    //点击事件函数
    private void MouseDown()
    {
        if (GameManager.Gm.gameStatus != GameManager.GameStatus.PlayingS) return; 
        print("down");
        //判断是否在地图上
        if (status != st.OnMap) return;
        //设置状态
        status = st.OnBoard;
        //调用位置移动方法,将卡牌移动到版面上
        GameManager.Gm.EnterBoard(type,gameObject,level);
        
    }

    public void DfsDown()
    {
        if(GameManager.Gm.gameStatus != GameManager.GameStatus.PlayingA)return;
        if(status != st.OnMap)return;
        status = st.OnBoard;
        //移动事件在dfs脚本中执行
    }

    public void ShowId()
    {
        idText.text = cid.ToString();
    }
}
