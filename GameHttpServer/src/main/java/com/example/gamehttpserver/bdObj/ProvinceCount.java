package com.example.gamehttpserver.bdObj;

public class ProvinceCount {
    private int id;
    private String provinceName;
    private int count;

    @Override
    public String toString() {
        return "{" +
                "id=" + id +
                ", provinceName='" + provinceName + '\'' +
                ", count=" + count +
                '}';
    }

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }

    public String getProvinceName() {
        return provinceName;
    }

    public void setProvinceName(String provinceName) {
        this.provinceName = provinceName;
    }

    public int getCount() {
        return count;
    }

    public void setCount(int count) {
        this.count = count;
    }
}
