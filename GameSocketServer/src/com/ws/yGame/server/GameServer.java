package com.ws.yGame.server;

import com.alibaba.fastjson.JSON;
import com.ws.yGame.common.Message;
import com.ws.yGame.common.MessageType;
import com.ws.yGame.common.User;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.Objects;
import java.util.concurrent.ConcurrentHashMap;

public class GameServer {
    // 创建服务器的Socket对象
    private ServerSocket serverSocket = null;

    public GameServer()
    {
        //创建服务器的Socket开始监听客户端
        try {
            System.out.println("服务器开始在9999端口监听");
            serverSocket = new ServerSocket(9999);
            while (true){//持续监听直到有客户端连接
                Socket socket = serverSocket.accept();
                //定义与客户端的输入输出流
                InputStream inputStream = socket.getInputStream();
                OutputStream outputStream = socket.getOutputStream();
                byte[] buf = new byte[1024];
                int readLen = 0;
                //第一次连接接受帐号的信息
                readLen = inputStream.read(buf);
                String res = new String(buf,0,readLen);
                //将收到的信息的解析
                Message message = JSON.parseObject(res, Message.class);
                System.out.println("收到客户端的第一条消息:"+message.toString());
                //判断发送的是否是登录信息
                if(Objects.equals(message.getMessageType(), MessageType.LOG_IN_MESSAGE)) {
                    //将收到的登录信息解析成User类
                    String jsonContent = message.getJsonContent();
                    User user = JSON.parseObject(jsonContent, User.class);
                    System.out.println("用户" + user + "登录");
                    //创建新线程接收用户输入
                    ServerConnectClientThread serverConnectClientThread =
                            new ServerConnectClientThread(socket, user.getUserName());
                    serverConnectClientThread.start();
                    //将线程加入线程管理类管理
                    ManagerClientThreads.addClientThread(user.getUserName(), serverConnectClientThread);
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }


    }
}
