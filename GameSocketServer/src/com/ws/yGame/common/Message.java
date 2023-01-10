package com.ws.yGame.common;

import com.alibaba.fastjson.JSON;

public class Message {
    //定义消息的类型
    public String messageType;
    //消息的内容(json格式的字符串)
    public String jsonContent;

    public String getMessageType() {
        return messageType;
    }

    public void setMessageType(String messageType) {
        this.messageType = messageType;
    }

    public String getJsonContent() {
        return jsonContent;
    }

    public void setString(Object o)
    {
        jsonContent = JSON.toJSONString(o);
    }

    @Override
    public String toString() {
        return JSON.toJSONString(this);
    }
}
