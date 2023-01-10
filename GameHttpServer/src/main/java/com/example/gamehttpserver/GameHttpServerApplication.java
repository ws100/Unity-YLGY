package com.example.gamehttpserver;

import org.mybatis.spring.annotation.MapperScan;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
@MapperScan("com.example.gamehttpserver.mapper")
public class GameHttpServerApplication {

    public static void main(String[] args) {
        SpringApplication.run(GameHttpServerApplication.class, args);
    }

}
