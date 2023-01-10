package com.example.gamehttpserver.controller;

import com.baomidou.mybatisplus.core.conditions.query.QueryWrapper;
import com.example.gamehttpserver.bdObj.ProvinceCount;
import com.example.gamehttpserver.mapper.ProvinceCountMapper;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

import java.sql.Wrapper;
import java.util.List;

@RestController
public class ProvinceRankController {

    @Autowired
    private ProvinceCountMapper provinceCountMapper;

    @GetMapping("/provinceRank")
    public String getProvinceRank()
    {
        QueryWrapper<ProvinceCount> wrapper = new QueryWrapper<>();
        wrapper.orderByDesc("count");
        List<ProvinceCount> countMappers = provinceCountMapper.selectList(wrapper);
        return countMappers.toString();
    }
}
