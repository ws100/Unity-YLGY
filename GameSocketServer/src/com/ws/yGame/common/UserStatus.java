package com.ws.yGame.common;

public class UserStatus {
    private String userId;
    private String status;

    public String getStatus() {
        return status;
    }

    public String getUserId() {
        return userId;
    }

    public void setStatus(String status) {
        this.status = status;
    }

    public void setUserId(String userId) {
        this.userId = userId;
    }
}
