package com.ws.yGame.server;

import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONArray;
import com.ws.yGame.common.*;

import javax.swing.*;
import java.io.InputStream;
import java.io.ObjectInputStream;
import java.io.OutputStream;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

public class ServerConnectClientThread extends Thread{
    private Socket socket;
    private String userId;
    private String status;

    public Socket getSocket() {
        return socket;
    }

    public void setStatus(String status) {
        this.status = status;
    }

    public String getStatus() {
        return status;
    }

    public String getUserId() {
        return userId;
    }

    public ServerConnectClientThread(Socket socket, String userId) {
        this.socket = socket;
        this.userId = userId;
        this.status = "online";
    }
    //不断接收客户端的信息
    @Override
    public void run() {
        while (true)
        {
            try {
                InputStream inputStream = socket.getInputStream();
                OutputStream outputStream = socket.getOutputStream();
                System.out.println("服务端和客户端" + userId + " 保持通信，读取数据...");
                byte[] buf = new byte[1024];
                int readLen = 0;
                readLen = inputStream.read(buf);
                if(readLen == -1)break;
                String res = new String(buf,0,readLen);
                Message message = JSON.parseObject(res, Message.class);
                System.out.println("收到客户端"+userId+"的消息:"+message.toString());
                //根据消息类型处理数据
                if(message.getMessageType().equals(MessageType.GET_USER_LIST_MESSAGE))
                {
                    ArrayList<String> onlineUser = ManagerClientThreads.getOnlineUser();
                    ArrayList<UserStatus> userStatuses = new ArrayList<>();
                    for(int i = 0;i<onlineUser.size();i++)
                    {
                        UserStatus userStatus = new UserStatus();
                        userStatus.setUserId(onlineUser.get(i));
                        userStatus.setStatus(ManagerClientThreads.getServerConnectClientThread(onlineUser.get(i)).getStatus());
                        userStatuses.add(userStatus);
                    }
                    Message messageOfOnlineUser = new Message();
                    messageOfOnlineUser.setMessageType(MessageType.SEND_USER_LIST_MESSAGE);
                    messageOfOnlineUser.setString(userStatuses);
                    System.out.println(messageOfOnlineUser);
                    outputStream.write(messageOfOnlineUser.toString().getBytes());
                }
                else if(message.getMessageType().equals(MessageType.CONNECT_WITH_USER))
                {
                    String opUser = message.getJsonContent();
                    if(ManagerClientThreads.getServerConnectClientThread(opUser) == null)
                    {
                        Message messageOfConnectFault = new Message();
                        messageOfConnectFault.setMessageType(MessageType.CONNECT_FAULT);
                        messageOfConnectFault.jsonContent = "";
                        System.out.println(messageOfConnectFault);
                        outputStream.write(messageOfConnectFault.toString().getBytes());
                    }
                    //向对方发送消息
                    else
                    {
                        ServerConnectClientThread opServerConnectClientThread =
                                ManagerClientThreads.getServerConnectClientThread(opUser);
                        OutputStream opOutputStream = opServerConnectClientThread.getSocket().getOutputStream();
                        Message connectWith = new Message();
                        connectWith.setMessageType(MessageType.CONNECT_WITH_USER);
                        connectWith.jsonContent = userId;
                        opOutputStream.write(connectWith.toString().getBytes());
                    }
                }
                //用户表示同意连接(同意xxx的连接)
                else if(message.getMessageType().equals(MessageType.CONNECT_SUCCEED))
                {
                    UserNameAndRandomSeed userNameAndRandomSeed =
                            JSON.parseObject(message.getJsonContent(),UserNameAndRandomSeed.class);
                    String opUser = userNameAndRandomSeed.getOpUser();
                    if(ManagerClientThreads.getServerConnectClientThread(opUser) == null)
                    {
                        Message messageOfConnectFault = new Message();
                        messageOfConnectFault.setMessageType(MessageType.CONNECT_FAULT);
                        messageOfConnectFault.jsonContent = "";
                        System.out.println(messageOfConnectFault);
                        outputStream.write(messageOfConnectFault.toString().getBytes());
                    }
                    //向对方发送消息
                    else
                    {
                        ServerConnectClientThread opServerConnectClientThread =
                                ManagerClientThreads.getServerConnectClientThread(opUser);
                        OutputStream opOutputStream = opServerConnectClientThread.getSocket().getOutputStream();
                        Message connectWith = new Message();
                        connectWith.setMessageType(MessageType.CONNECT_SUCCEED);
                        connectWith.jsonContent = userNameAndRandomSeed.toString();
                        opOutputStream.write(connectWith.toString().getBytes());
                    }
                }
                //用户表示拒绝连接(拒绝xxx的连接)
                else if(message.getMessageType().equals(MessageType.CONNECT_REFUSED))
                {
                    String opUser = message.getJsonContent();
                    if(ManagerClientThreads.getServerConnectClientThread(opUser) == null)
                    {
                        Message messageOfConnectFault = new Message();
                        messageOfConnectFault.setMessageType(MessageType.CONNECT_FAULT);
                        messageOfConnectFault.jsonContent = "";
                        System.out.println(messageOfConnectFault);
                        outputStream.write(messageOfConnectFault.toString().getBytes());
                    }
                    //向对方发送消息
                    else
                    {
                        ServerConnectClientThread opServerConnectClientThread =
                                ManagerClientThreads.getServerConnectClientThread(opUser);
                        OutputStream opOutputStream = opServerConnectClientThread.getSocket().getOutputStream();
                        Message connectWith = new Message();
                        connectWith.setMessageType(MessageType.CONNECT_REFUSED);
                        connectWith.jsonContent = userId;
                        opOutputStream.write(connectWith.toString().getBytes());
                    }
                }
                //用户连接中卡牌id的发送
                else if(message.getMessageType().equals(MessageType.SEND_CARD_ID))
                {
                    String jsonCardString = message.getJsonContent();
                    CardInformation cardInformation = JSON.parseObject(jsonCardString,CardInformation.class);
                    String opUser = cardInformation.getOpUser();
                    String cardId = cardInformation.getCardId();
                    if(ManagerClientThreads.getServerConnectClientThread(opUser) == null)
                    {
                        Message messageOfConnectFault = new Message();
                        messageOfConnectFault.setMessageType(MessageType.CONNECT_FAULT);
                        messageOfConnectFault.jsonContent = "";
                        System.out.println(messageOfConnectFault);
                        outputStream.write(messageOfConnectFault.toString().getBytes());
                    }
                    //向对方发送消息
                    else
                    {
                        ServerConnectClientThread opServerConnectClientThread =
                                ManagerClientThreads.getServerConnectClientThread(opUser);
                        OutputStream opOutputStream = opServerConnectClientThread.getSocket().getOutputStream();
                        Message connectWith = new Message();
                        connectWith.setMessageType(MessageType.SEND_CARD_ID);
                        CardInformation cardInformationToOpUser = new CardInformation();
                        cardInformationToOpUser.setOpUser(userId);
                        cardInformationToOpUser.setCardId(cardId);
                        connectWith.jsonContent = cardInformationToOpUser.toString();
                        opOutputStream.write(connectWith.toString().getBytes());
                    }
                }
            } catch (Exception e){
                e.printStackTrace();
            }
        }
        System.out.println("用户" + userId + "断开连接");
        ManagerClientThreads.removeServerConnectClientThread(userId);
    }
}
