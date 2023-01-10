package com.ws.yGame.common;

public class UserNameAndRandomSeed {
    private String opUser;
    private String randomSeed;

    public String getOpUser() {
        return opUser;
    }

    public String getRandomSeed() {
        return randomSeed;
    }

    public void setOpUser(String opUser) {
        this.opUser = opUser;
    }

    public void setRandomSeed(String randomSeed) {
        this.randomSeed = randomSeed;
    }

    @Override
    public String toString() {
        return "{" +
                "\"opUser\":\"" + opUser + "\"" +
                ", \"randomSeed\":\"" + randomSeed + "\"" +
                '}';
    }
}
