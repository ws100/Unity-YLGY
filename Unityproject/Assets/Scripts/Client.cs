using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Client : MonoBehaviour
{
    private static string USER_LOGIN_MESSAGE = "1";
    private static string GET_USER_LIST_MESSAGE = "2";
    private static string SEND_USER_LIST_MESSAGE = "3";
    private static string CONNECT_WITH_USER = "4";
    private static string CONNECT_SUCCEED = "5";
    private static string CONNECT_REFUSED = "6";
    private static string CONNECT_FAULT = "7";
    private static string SEND_CARD_ID = "8";

    private Thread thread;
    //消息类
    [Serializable]
    public class Message
    {
        public string messageType;
        public string jsonContent;
        public void setString(object o)
        {
            jsonContent = JsonConvert.SerializeObject(o);
        }
    }
    
    //客户端连接类的单例
    public static Client clientManager;
    //客户端的Socket连接
    public static TcpClient client;
    //ip和端口
    public string serverIp;
    public string serverPort;
    public void InitClient()
    {
        //设置单例
        clientManager = GetComponent<Client>();
        //将客户端的Socket连接到服务器
        client = new TcpClient();
        client.Connect(IPAddress.Parse(serverIp), Convert.ToInt32(serverPort));
        //设置IP和端口并连接
        print("服务器连接成功");
        //创建新线程接受服务器的信息
        thread = new Thread(ReceiveFromServer);
        User user = new User();
        user.userName = GameManager.Gm.userId;
        user.passWord = "";
        thread.Start();
        Message message = new Message();
        message.messageType = "1";
        message.setString(user);
        SendObjectJson(message);
    }
    //接收服务端的信息
    private void ReceiveFromServer()
    {
        while (true)
        {
            byte[] buffer = new byte[1024];
            int realSize = client.Client.Receive(buffer);
            if (realSize <= 0)
            {
                break;
            }
            string message = Encoding.UTF8.GetString(buffer, 0, realSize);
            print("收到消息-->"+message);
            //将消息解析
            JObject msgJObject = (JObject)JsonConvert.DeserializeObject(message);
            string messageType = msgJObject["messageType"].ToString();
            //获取用户列表的消息
            if (messageType == SEND_USER_LIST_MESSAGE)
            {
                //将消息的内容解析成数组
                string content = msgJObject["jsonContent"].ToString();
                JArray userJArray = (JArray)JsonConvert.DeserializeObject(content);
                GameManager.Gm.onlineUsers = new List<GameManager.UserItemStruct>();
                //便历数组转化成对应的User对象
                for (int i = 0; i < userJArray.Count; i++)
                {
                    //解析用户名和状态
                    string userName = userJArray[i]["userId"].ToString();
                    string status = userJArray[i]["status"].ToString();
                    bool isOnline;
                    if (status == "online") isOnline = true;
                    else isOnline = false;
                    print(userName);
                    print(status);
                    //将数据传入列表
                    GameManager.UserItemStruct onlineUserItem = new GameManager.UserItemStruct();
                    onlineUserItem.User = userName;
                    onlineUserItem.IsOnline = isOnline;
                    GameManager.Gm.onlineUsers.Add(onlineUserItem);
                }
                //通过线程辅助类刷新列表显示
                Loom.QueueOnMainThread((parma)=>
                {
                    GameManager.Gm.RefreshOnlineList();
                },null);
            }
            //收到用户的连接请求
            else if (messageType == CONNECT_WITH_USER)
            {
                string opUser = msgJObject["jsonContent"].ToString();
                Loom.QueueOnMainThread((parma)=>
                {
                    DPlayManager.PlayManager.MeetUser(opUser);
                },null);

            }
            //收到对方拒接的请求
            else if (messageType == CONNECT_REFUSED)
            {
                Loom.QueueOnMainThread((parma)=>
                {
                    DPlayManager.PlayManager.UserRefuse();
                },null);
            }
            //收到对方同意连接的请求
            else if (messageType == CONNECT_SUCCEED)
            {
                string jsonContent = msgJObject["jsonContent"].ToString();
                JObject msgSubJObject = (JObject)JsonConvert.DeserializeObject(jsonContent);
                string randomSeed = msgSubJObject["randomSeed"].ToString();
                int seed = int.Parse(randomSeed);
                Loom.QueueOnMainThread((parma)=>
                {
                    DPlayManager.PlayManager.UserAccept(seed);
                },null);
            }
            //收到另一个用户的发送卡牌请求
            else if(messageType == SEND_CARD_ID)
            {
                string jsonContent = msgJObject["jsonContent"].ToString();
                JObject msgSubJObject = (JObject)JsonConvert.DeserializeObject(jsonContent);
                string cardId = msgSubJObject["cardId"].ToString();
                print("对方选牌"+cardId);
                Loom.QueueOnMainThread((parma)=>
                {
                    DPlayManager.PlayManager.FetchCard(cardId,false);
                },null);
            }

        }
    }
    //向Socket服务器发送可序列化的类对象Json
    public void SendObjectJson(object o)
    {
        string jsonStr = JsonConvert.SerializeObject(o);
        byte[] buffer = Encoding.UTF8.GetBytes(jsonStr);
        client.Client.Send(buffer);
        print("发送消息-->"+jsonStr);
    }
    //Send状态(弃用)
    [Obsolete]
    private static void SendCallBack(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;
        socket.EndSend(ar);

    }
    //向Socket服务器发送可序列化的类对象(弃用)
    [Obsolete]
    public void SendObject(object o)
    {
        MemoryStream memoryStream = new MemoryStream();
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(memoryStream, o);
        memoryStream.Flush();
        byte[] buffer = new byte[1024];
        memoryStream.Position = 0;
    }
    //用户基本信息类
    [Serializable]
    public class User 
    {
        public string userName;
        public string passWord;
    }
    //用户选择卡牌类
    [Serializable]
    public class CardInformation
    {
        public string opUser;
        public string cardId;
    }
    //随机种子
    public class UserNameAndRandomSeed
    {
        public string opUser;
        public string randomSeed;
    }
    //向服务器发送请求获取用户列表
    public void GetUserList()
    {
        Message message = new Message();
        message.messageType = GET_USER_LIST_MESSAGE;
        message.jsonContent = "";
        SendObjectJson(message);
    }
    //发送用户连接请求
    public void ConnectWithUser(string userId)
    {
        Message message = new Message();
        message.messageType = CONNECT_WITH_USER;
        message.jsonContent = userId;
        SendObjectJson(message);
    }
    //发送拒接用户的请求
    public void RefuseUser(string user)
    {
        Message message = new Message();
        message.messageType = CONNECT_REFUSED;
        message.jsonContent = user;
        SendObjectJson(message);
    }
    //发送接收用户的请求
    public void AcceptUser(string user,int randomSeed)
    {
        Message message = new Message();
        message.messageType = CONNECT_SUCCEED;
        UserNameAndRandomSeed userNameAndRandomSeed = new UserNameAndRandomSeed();
        userNameAndRandomSeed.opUser = user;
        userNameAndRandomSeed.randomSeed = randomSeed.ToString();
        message.setString(userNameAndRandomSeed);
        SendObjectJson(message);
    }
    //关闭Socket通道
    public void CloseSocket()
    {
        thread.Abort();
        client.Close();
    }
    //将选择的卡牌信息发送给对方
    public void SendCardId(string cardId)
    {
        if (DPlayManager.PlayManager.status != DPlayManager.WaitingStatus.OnPlaying) return;
        string opUser = DPlayManager.PlayManager.opUser;
        CardInformation cardInformation = new CardInformation();
        cardInformation.cardId = cardId;
        cardInformation.opUser = opUser;
        Message message = new Message();
        message.messageType = SEND_CARD_ID;
        message.setString(cardInformation);
        SendObjectJson(message);
    }
}
