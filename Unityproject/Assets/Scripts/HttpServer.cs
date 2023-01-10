using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class HttpServer : MonoBehaviour
{
    public static HttpServer hts;
    //服务器的地址(测试地址为本地ip)
    public string serverIp;

    private void Awake()
    {
        hts = GetComponent<HttpServer>();
    }

    public void Reg(string name, string password)
    {
        StartCoroutine(RegHttp(name,password));
    }
    public void LogIn(string name, string password)
    {
        StartCoroutine(LoginHttp(name, password));
    }
    public void GetOnlineUsers()
    {

    }
    public void ConnectWithPlayer(int id)
    {

    }

    IEnumerator LoginHttp(string name,string password)
    {
        // http://127.0.0.1:8080/userLogin?password=22&userName=22
        string url = serverIp + "userLogin?password=" + name + "&userName=" + password;
        
        UnityWebRequest request = UnityWebRequest.Get(url);
        
        yield return request.SendWebRequest();
        if (request.responseCode == 200)
        {
            string res = request.downloadHandler.text;
            LoginManager.loginManager.LoginBack(res,name);
        }
    }

    IEnumerator RegHttp(string name,string password)
    {
        //http://127.0.0.1:8080/userReg?password=1345&username=zazaza
        string url = serverIp + "userReg?password=" + password + "&username=" + name;
        print(url);
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        print(request.responseCode);
        if (request.responseCode == 200)
        {
            string res = request.downloadHandler.text;
            print("成功响应");
            RegManger.regManger.RegBack(res,name);
        }
    }
}
