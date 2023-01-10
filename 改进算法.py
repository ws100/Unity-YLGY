import requests
import random
import numpy as np
from collections import Counter
import time
# 全局变量
# 根据id索引到卡牌的属性
mapDict = {}
mapList = []
curMap = []
avMap = []
pLevelMap = {}
step = []
isOnMap = []
boardScore = [0, 10000, 100000]
sFlag = 0
maxStep = []
tGap = 0
dfsLength = 0
dfsMaxLength = 0
# level从1开始


def upDateAvailable(level):
    # 通过当前状态的地图数据更新现在的
    # 清空avMap
    avMap.clear()
    for i in range(level):
        for j in pLevelMap[i]:
            if not isOnMap[j]:
                continue
            else:
                flag = False
                for m in range(i + 1, level):
                    for n in pLevelMap[m]:
                        if not isOnMap[n]:
                            continue
                        # n和j的坐标比较
                        if abs(mapList[n][1] - mapList[j][1]) < 8 and abs(mapList[n][2] - mapList[j][2]) < 8:
                            flag = True
                            break
                    if flag:
                        break
                if not flag:
                    avMap.append(j)
            pass
    pass


def loadData():
    # 羊了个羊算法分析
    dataUrl = "http://43.139.170.217/ylgy/level2_12_24.json"
    res = requests.get(dataUrl).json()
    levelData = res['levelData']
    idList = []
    for i in levelData:
        subData = levelData[i]
        levelMapData = []
        for j in subData:
            s = str(j['id'])
            idList.append(s)
            levelMapData = s.split('-')
            mapList.append(levelMapData)
    blockTypeData = res['blockTypeData']
    pRandomData = []
    for c in blockTypeData:
        for t in range(blockTypeData[c] * 3):
            pRandomData.append(c)
    pSubRandomData = []
    le = 1
    for i in range(le):
        pSubRandomData.append(pRandomData[int(len(pRandomData) * (i / le)): int(len(pRandomData) * ((i + 1) / le))])
    for i in range(le):
        random.shuffle(pSubRandomData[i])
    randomList = np.hstack(pSubRandomData)
    print(randomList)
    pRandomData = randomList.tolist()
    for j in range(len(pRandomData)):
        mapList[j].append(pRandomData[j])
    for i in range(len(idList)):
        mapDict[idList[i]] = i
        isOnMap.append(True)
    for i in range(len(mapList)):
        if len(pLevelMap.keys()) == 0 or int(mapList[i][0]) - 1 not in pLevelMap.keys():
            pLevelMap[int(mapList[i][0]) - 1] = []
        pLevelMap[int(mapList[i][0]) - 1].append(i)
    print(mapDict)
    print(pLevelMap)


# 数据传输
# 版面数据-参数传递
# 地图数据-全局变量(需要回溯)
# 可点击的卡牌(更新)
# 数据存储
# 经历的步数(栈存储)
def dfs(pos1, pos2, pos3, pos4, pos5, pos6, pos7):
    global dfsMaxLength
    global dfsLength
    global maxStep
    dfsMaxLength = max(dfsLength, dfsMaxLength)
    ct = time.time()
    if ct - t >= tGap:
        return
    # 便历到全部的
    if len(step) == 267:
        print("找到成功的步骤", step)
        return
    # 拷贝当前可点击卡牌的集合
    cAvMap = avMap.copy()
    # 计算当前版面卡牌的个数
    boardCount = 7
    posList = [pos1, pos2, pos3, pos4, pos5, pos6, pos7]
    for i in range(len(posList)):
        if posList[i] == 0:
            boardCount = i
            break
    # 格子已满结束状态
    if boardCount == 7:
        print("格子已满结束状态("+str(len(step))+")")
        print(step)
        if len(step) >= len(maxStep):
            maxStep = step.copy()
        return
    # 计算当前版面上各类型卡牌的数目
    BoardTypeCount = Counter(posList)
    # 6种相同的,剪枝
    if len(BoardTypeCount.keys()) == 6:
        return
    # 计算可以三消的卡牌个数
    threeCard = []
    # 根据场上的卡牌生成卡牌类型的集合
    curTypeList = [mapList[i][3] for i in cAvMap]
    # 完成三消
    curTypeCounter = Counter(curTypeList)
    popCard = []
    popIdList = []
    if boardCount <= 4:
        # 查找可以消除的卡牌类型
        for i in curTypeCounter:
            if curTypeCounter[i] >= 3:
                popCard.append(i)
        # 将所有可以消除的加入一个集合
        for i in popCard:
            tcCount = 0
            for j in cAvMap:
                if tcCount == 3:
                    break
                if mapList[j][3] == i:
                    tcCount = tcCount + 1
                    popIdList.append(j)
        # 执行消除操作
        for i in popIdList:
            isOnMap[i] = False
            step.append(i)
        upDateAvailable(24)
        cAvMap = avMap.copy()
    scoreDict = {}
    for i in range(len(cAvMap)):
        # 计算每个卡牌被点击之后开启的卡牌数量
        isOnMap[cAvMap[i]] = False
        upDateAvailable(mapList[cAvMap[i]][0])
        c = len(avMap) - len(cAvMap)
        # 计算当前版面的潜在价值
        pTypeList = []
        for j in avMap:
            pTypeList.append(mapList[j][3])
        pTypeCount = Counter(pTypeList)
        # 根据开启的两对数目和三对数目计算得分
        p = 0
        for j in pTypeCount:
            if pTypeCount[j] == 2:
                p = p + 100
            elif pTypeCount[j] >= 3:
                p = p + 1000
        # 计算场上的卡牌与版面卡牌的匹配情况
        for j in pTypeCount:
            if j in BoardTypeCount.keys():
                if pTypeCount[j] + BoardTypeCount[j] == 2:
                    p = p + 100
                elif pTypeCount[j] + BoardTypeCount[j] >= 3:
                    p = p + 1000
        # 恢复状态
        isOnMap[cAvMap[i]] = True
        # 计算总得分
        # 当前选中的卡牌在版面上的数量
        cType = BoardTypeCount[mapList[cAvMap[i]][3]]
        score = boardScore[cType] + p*2
        # 记录当前的状态
        scoreDict[cAvMap[i]] = score
    # 将计算的得分和状态排序
    sortedScore = sorted(scoreDict.items(), key=lambda x: -x[1])
    # 遍历状态开始dfs
    for i in sortedScore:
        cardId = i[0]
        cardType = mapList[cardId][3]
        isOnMap[cardId] = False
        upDateAvailable(mapList[cardId][0])
        tPosList = posList.copy()
        # 按照类型更新版面的状态
        if BoardTypeCount[cardType] != 2:
            tPosList[boardCount] = cardType
        else:
            tPosList.remove(cardType)
            tPosList.remove(cardType)
            tPosList.append(0)
            tPosList.append(0)
        step.append(cardId)
        print(tPosList)
        dfsLength = dfsLength + 1
        dfs(tPosList[0],tPosList[1],tPosList[2],tPosList[3],tPosList[4],tPosList[5],tPosList[6])
        # 恢复状态
        dfsLength = dfsLength - 1
        isOnMap[cardId] = True
        step.pop()
    # 恢复消除的三连状态
    for i in popIdList:
        isOnMap[i] = True
        step.pop()


loadData()
mapList = np.array(mapList, dtype='int')
print(pLevelMap[1])
print(mapList)
upDateAvailable(len(pLevelMap))
print(avMap)
tGap = int(input("输入搜索时间"))
t = time.time()
dfs(0, 0, 0, 0, 0, 0, 0)
print("搜索时间:", tGap)
print("dfs最大深度", dfsMaxLength)
print("搜索最大步数", len(maxStep))
print("搜索最大步数数组:", maxStep)
