using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class LoginManager : MonoBehaviour
{
    public Text userNameInput;
    public Text passWordInput;
    public static LoginManager loginManager;
    private void Awake()
    {
        loginManager = GetComponent<LoginManager>();
    }

    public void Login()
    {
        string userName = userNameInput.text;
        string passWord = passWordInput.text;
        if (userName == "")
        {
            ShowTipMessage.Tip.ShowTip("用户名为空");
            return;
        }
        if (passWord == "")
        {
            ShowTipMessage.Tip.ShowTip("密码为空");
            return;
        }
        HttpServer.hts.LogIn(userName, passWord);
    }

    public void Back()
    {
        gameObject.SetActive(false);
    }

    public void LoginBack(string res,string userName)
    {
        if (res == "登录成功")
        {
            GameManager.Gm.userId = userName;
            GameManager.Gm.isLogin = true;
            ShowTipMessage.Tip.ShowTip("登录成功");
            gameObject.SetActive(false);
        }
        else if(res == "密码失败")
        {
            ShowTipMessage.Tip.ShowTip("密码错误");
        }
        else if (res == "用户不存在")
        {
            ShowTipMessage.Tip.ShowTip("未知错误");
        }
    }
}
