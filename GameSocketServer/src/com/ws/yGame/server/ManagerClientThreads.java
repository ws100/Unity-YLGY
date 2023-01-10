package com.ws.yGame.server;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;

//线程的总管理类
public class ManagerClientThreads {
    private static HashMap<String,ServerConnectClientThread> hashMap = new HashMap<>();

    //Get方法
    public static HashMap<String,ServerConnectClientThread> getHashMap(){return hashMap;}

    //加入线程集合的方法
    public static void addClientThread(String userId,ServerConnectClientThread serverConnectClientThread) {
        hashMap.put(userId,serverConnectClientThread);
    }

    //根据userId 返回ServerConnectClientThread线程
    public static ServerConnectClientThread getServerConnectClientThread(String userId){return hashMap.get(userId);}

    //根据userId 移除线程对象
    public static void removeServerConnectClientThread(String userId){hashMap.remove(userId);}

    //返回在线用户列表
    public static ArrayList<String> getOnlineUser()
    {
        //遍历集合
        return new ArrayList<>(hashMap.keySet());
    }

}
