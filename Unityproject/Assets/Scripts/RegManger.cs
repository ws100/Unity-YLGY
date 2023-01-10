using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegManger : MonoBehaviour
{
    public Text userNameInput;
    public Text passWordInput;
    public Text passWordInputV;
    public static RegManger regManger;

    private void Awake()
    {
        regManger = GetComponent<RegManger>();
    }

    public void Reg()
    {
        string userName = userNameInput.text;
        string passWord = passWordInput.text;
        string passWordV = passWordInputV.text;
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
        if (passWord != passWordV)
        {
            ShowTipMessage.Tip.ShowTip("两次密码输入不一致");
            return;
        }

        HttpServer.hts.Reg(userName, passWord);
        
    }

    public void Back()
    {
        gameObject.SetActive(false);
    }

    public void RegBack(string res,string userName)
    {
        if (res == "注册成功")
        {
            GameManager.Gm.userId = userName;
            GameManager.Gm.isLogin = true;
            ShowTipMessage.Tip.ShowTip("注册成功");
            gameObject.SetActive(false);
        }
        else if (res == "用户已存在")
        {
            ShowTipMessage.Tip.ShowTip("用户已存在");
        }
        else
        {
            ShowTipMessage.Tip.ShowTip("未知错误");
        }
    }
}
