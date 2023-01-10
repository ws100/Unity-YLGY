package com.ws.yGame.common;

public class CardInformation {
    private String opUser;
    private String cardId;

    @Override
    public String toString() {
        return "{" +
                "\"opUser\":\"" + opUser + "\"" +
                ", \"cardId\":\"" + cardId + "\"" +
                '}';
    }

    public String getOpUser() {
        return opUser;
    }

    public void setOpUser(String opUser) {
        this.opUser = opUser;
    }

    public String getCardId() {
        return cardId;
    }

    public void setCardId(String cardId) {
        this.cardId = cardId;
    }
}
