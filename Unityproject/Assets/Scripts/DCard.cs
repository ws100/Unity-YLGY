using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DCard : Card
{
    public enum Dst
    {
        OnMap,
        OnMyBoard,
        OnOpBoard,
    }

    public Dst dStatus;
    public void SetButton()
    {
        btn.onClick.AddListener(MouseDown);
    }

    private void MouseDown()
    {
        //判断卡牌是否在地图上
        if (dStatus != Dst.OnMap) return;
        //判断是否被遮蔽
        if (status != st.OnMap) return;
        //判断游戏是否开始
        if (DPlayManager.PlayManager.status != DPlayManager.WaitingStatus.OnPlaying) return;
        //判断是否是自己的回合
        if (!DPlayManager.PlayManager.isMyTime) return;
        //将点击的卡牌传给管理相应的组件
        DPlayManager.PlayManager.FetchCard(id,true);
        //将卡牌ID发给对手
        Client.clientManager.SendCardId(id);
        //设置状态
        status = st.OnBoard;
        dStatus = Dst.OnMyBoard;
        DPlayManager.PlayManager.isMyTime = false;
    }
}
