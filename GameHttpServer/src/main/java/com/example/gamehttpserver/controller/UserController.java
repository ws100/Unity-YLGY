package com.example.gamehttpserver.controller;

import com.baomidou.mybatisplus.core.conditions.query.QueryWrapper;
import com.example.gamehttpserver.bdObj.User;
import com.example.gamehttpserver.mapper.UserMapper;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;
import java.util.Objects;

@RestController
public class UserController {
    @Autowired
    private UserMapper userMapper;

    @GetMapping("/userLogin")
    public String login(String userName, String password) {
        QueryWrapper<User> wrapper = new QueryWrapper<User>();
        wrapper.eq("username", userName);
        List<User> userList = userMapper.selectList(wrapper);
        if (userList.size() == 0) return "用户不存在";
        else {
            if (Objects.equals(password, userList.get(0).getPassword())) return "登录成功";
            else return "登录失败";
        }
    }

    @GetMapping("/userReg")
    public String reg(User user)
    {
        QueryWrapper<User> wrapper = new QueryWrapper<User>();
        wrapper.eq("username", user.getUsername());
        System.out.println(user);
        List<User> userList = userMapper.selectList(wrapper);
        if(userList.size() == 0) {
            userMapper.insert(user);
            return "注册成功";
        }
        else
        {
            return "用户已存在";
        }
    }
}
