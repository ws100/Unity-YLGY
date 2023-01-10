using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class OnlineUserItem : MonoBehaviour
{
    public Text userNameText;
    public Image onlineImage;
    public Color busyColor;
    public Color normalColor;
    public Button connectBtn;
    public string userId;

    public void Init(string userName, bool isOnline)
    {
        userId = userName;
        if (userName == GameManager.Gm.userId) userName += "(我)";
        userNameText.text = userName;
        if (isOnline) onlineImage.color = normalColor;
        else onlineImage.color = busyColor;
        connectBtn.onClick.RemoveAllListeners();
        connectBtn.onClick.AddListener(delegate
        {
            if (DPlayManager.PlayManager.status != DPlayManager.WaitingStatus.Online) return;
            if(userId == GameManager.Gm.userId)ShowTipMessage.Tip.ShowTip("不能和自己连接");
            else
            {
                Client.clientManager.ConnectWithUser(userId);
                //ShowTipMessage.Tip.ShowTip("与用户"+userId+"的连接已发送");
                //打开等待面板
                DPlayManager.PlayManager.WaitUser(userId);
            }
        });
    }
}
