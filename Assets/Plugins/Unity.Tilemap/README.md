## ﻿Tilemap Editor

### 菜单

- **GameObject/3D Object/Tilemap**

  创建Tilemap生成器

- **Assets/Create/Tilemap/Tile**

  创建 Tile 资源配置

  

### 创建Tilemap

1. 菜单【GameObject/3D Object/Tilemap】创建Tilemap生成器
2. 点击【Create Tilemap】按钮创建 Tilemap
3. 设置Tile, 点击【Tiles +】添加Tile
4. 设置Tile层，点击【Layers +】添加Tile层
5. 点击【Build】按钮生成地图



### Tile

一个 Tile 资源包含边(Edge)，外角(OuterCorner)，内角(InnerCorner)，块(Block)，地块(Ground)5个部分组成。

设置Tile 将Tile prefab 拖到对应类型的Tile图标上。



##### 配置属性

- Rotation

  修正角度偏移

- Offset

  修正位置偏移

  

  **预览属性**

- Ambient Color

  预览时环境光

- Light Intensity

  预览时光照强度



##### Tile 制作规范

- 大小

  统一为Unity中1个单位，即1米，统一单位后Tile 才可以作为公共资源进行复用 

- 方向

  z 轴为前方，如果方向不正确需要修改【Tile Rotation】

- 轴

  轴设置为模型的中心，如果轴不正确则修改【Tile Offset】



##### 预览 Tile

选中 Tile 显示【Tile Preview】预览窗口，检查资源是否连接正确，鼠标左键拖拽可以旋转视角，鼠标右键点击可切换的模板



### Tilemap

地图生成器的配置



##### 属性

- Width

  地图宽(x)

- Height

  地图高(z)

- Scale

  缩放，作用于Tile和装饰物。

- Tiles

  Tile

  - Group

    Tile 组ID

  - Tile

    Tile 资源

  - Weight

    生成权重

- Layers 

  Tile层，【+】添加Tile层。

  - 勾选框

    是否启用

  - Active

    选中该层

  - 菜单

    - Move Up

      层级向上移动

    - Move Down

      层级向下移动

    - Delete

      删除该层

  - Note

    层说明信息

  - Input

    先计算mask，再mask转地图

    - Mask 

      对输入 mask 进行操作

    - Mask To Map

      mask转换为地图数据

  - Algrihm

    地块生成算法

  - Tile

    Tiles组ID，生成地块的Tile资源

  - Tile Block Size

    地块的单位大小，对地块进行拆分，默认为1，不进行拆分

  - Offset Height

    层高

  - Flags

    层标志位

    - Tile Block

      生成块

    - Tile Ground

      生成地块

  - Output

    先进行地图转mask，再计算mask

    - Map To Mask

      地图转换为 mask，默认为Or

    - Mask

      对输出 mask 进行操作

- Decorates

  生成装饰物，【+】添加装饰生成算法。

  - 勾选框

    是否启用

  - Prefab

    装饰物件模型

  - Offset

    位置偏移

  - Offset Rotation

    旋转偏移

  - Use Tile Rotation

    使用Tile的方向

  - Random Offset

    随机位置

  - Random Rotation

    随机方向

  - Random Scale

    随机缩放

  - Algrithm

    装饰物生成算法





### 编辑器配置

菜单 Editor/Preferences/Tilemap Editor

##### 属性

##### 	**格子**

- Show Grid

  是否显示参考格子

- Grid Color

  格子颜色

- Grid Center Color

  中间参考线颜色

- Hover Color

  鼠标选中的的Grid块的颜色

  

  **选中层**

- Ground Color

  选中时绘制地块的颜色

- Hide Unselected Layer

  当有选中的层时隐藏未选择的其它层

- Closed Block Number

  在相邻的块上显示编号

- Closed Block Number Color

  编号的颜色