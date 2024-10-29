# ﻿Tilemap

**优点**

- 支持遮挡剔除
- 支持延迟加载
- 定制加载和销毁
- 三消拼接类型游戏位置和旋转计算

**缺点**

- Tile不支持方向的判定，单面墙不能区分方向

## Tile

Tile 包含 Edge(边)，OuterCorner(外角)，InnerCorner(内角)，Block(块)，Ground(地面)5个部分组成。

**创建**

菜单 `Assets/Create/Tilemap/Tile` 

![Tile](Doc~\Images\Tile.PNG)

### 类型

- **Edge**

  边，块类型，阻挡的边

- **OuterCorner**

  外角，块类型，阻挡的外角

- **InnerCorner**

  内角，块类型，阻挡的内角

- **Block**

  块，块类型，阻挡块

- **Ground**

  地块，非块类型，非阻挡区域，为可行走区域

**操作**

- 添加

  拖拽 Prefab 到 Pattern 图标，可以添加多个

- 修改

  拖拽 Prefab 到 Tile 图标，替换 Prefab

- 删除

  右键点击 Tile 图标，选择菜单 `Delete` 删除

### 属性

- Weight

  随机权重

- Rotation

  是否允许旋转



### 预览

选中 Tile 下方显示 `Tile Preview` 预览窗口，检查`Tile`是否拼接正确



![Tile_Preview](Doc~\Images\Tile_Preview.PNG)

**操作**

- 旋转视角

  鼠标左键拖拽可以旋转视角，

- 切换模板

  鼠标右键点击可切换不同的模板，内置3个模板，

  - 4个块，预览外角组成最小的块，
  - Ground类型块，预览地面，
  - 完整的模式，预览5种类型块

- 预览不同的Tile

  如果同一类型Tile 有多个，可以点击 Tile 选中不同的 Tile 预览，Tile 黄色边框为选中状态。

**属性**

- Ambient Color

  预览时环境光

- Light Intensity

  预览时光照强度



### 格式

- 大小

  统一为Unity中1个单位，即1米，统一单位后Tile 才可以作为公共资源进行复用 

- 方向

  z 轴为前方，如果方向不正确需要修改 `Tile Rotation`

- 轴

  轴设置为模型的中心，如果轴不正确则修改 `Tile Offset`



## Pattern

格子图标为组合模式，格子类型只有块类型和非块类型

![Tile_Pattern_Type](Doc~\Images\Tile_Pattern_Type.PNG)

### 块类型

- 蓝色

  块区域，Tile占用位置

- 灰色

  包含块，该位置为块

- 红色

  排除块，该位置不能为块

### 属性

- Rotation

  修正角度偏移，复用其它 `Tilemap` 插件的资源

- Offset

  修正位置偏移

### 模式

- Edge

  ![Tile_Pattern_Edge](Doc~\Images\Tile_Pattern_Edge.PNG)

- OuterCorner

  ![Tile_Pattern_OuterConner](Doc~\Images\Tile_Pattern_OuterConner.PNG)

- InnerCorner

  ![Tile_Pattern_InnerCorner](Doc~\Images\Tile_Pattern_InnerCorner.PNG)

- Block

  ![Tile_Pattern_Block](Doc~\Images\Tile_Pattern_Block.PNG)

- Ground

  ![Tile_Pattern_Ground](Doc~\Images\Tile_Pattern_Ground.PNG)

### 编辑模式

双击 Pattern 图标进入编辑模式

![Tile_Pattern](Doc~\Images\Tile_Pattern.PNG)

- `Tile Type` 类型下拉列表

  点击格子切换不同的类型，内置5个基础类型模式(Edge,OuterCorner,InnerCorner,Block,Ground)

- `Default` 按钮

  恢复默认模式

- `Apply` 按钮

  保存模式

- `Cancel` 按钮

  取消保存，退出模式编辑

  

### 定制模式

单列墙

![SingleEdge](Doc~\Images\SingleEdge.PNG)



## Tilemap

地图生成器的配置



菜单 `Assets/Create/Tilemap/Tilemap` 创建 `Tilemap Asset`

![Tilemap](Doc~\Images\Tilemap.PNG)





### **属性**

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
  
- Data

  - Provider

    数据提供程序，支持 `Json`，`Xml` 数据格式

**按钮**

- Build

  重新生成Tilemap数据和Tile

- Build Data

  只重新生成Tilemap数据，不生成Tile

- Build Tiles

  重新生成Tile

- Build Decorates

  重新生成装饰

- Load

  加载Tilemap数据

- Save As

  保存Tilemap数据





## 场景添加 Tilemap

![TilemapCreator](Doc~\Images\TilemapCreator.PNG)

1. `GameObject` 添加 `TilemapCreator` 组件
2. 点击 `Create Tilemap` 按钮创建 `Tilemap` 或设置已有的 `Tilemap Asset`
3. 设置 Tile, 点击 `Tiles +` 添加 Tile
4. 设置 Tile 层，点击 `Layers +` 添加 Tile 层
5. 点击 `Build` 按钮生成地图



**手动编辑地图**

点击 Layer `Active` 按钮，该层处于可编辑中，鼠标左键填充地块，右键取消地块，再次点击`Active`按钮取消编辑

![TilemapEditor_Fill](Doc~\Images\TilemapEditor_Fill.PNG)



## 编辑器配置

菜单 `Editor/Preferences/Tilemap Editor`

![Tilemap_Editor_Settings](Doc~\Images\Tilemap_Editor_Settings.PNG)

### 属性

**格子**

- Show Grid

  是否显示参考格子

- Grid Color

  格子颜色

- Grid Center Color

  中间参考线颜色

- Hover Color

  鼠标选中的的Grid块的颜色

- Fill Size

  笔刷填充大小

  

**选中层**

- Ground Color

  选中时绘制地块的颜色

- Hide Unselected Layer

  当有选中的层时隐藏未选择的其它层

- Closed Block Number

  在相邻的块上显示编号

- Closed Block Number Color

  编号的颜色



## 地图生成

### 地图结构

#### 层

包含该层所有配置，依次按层顺序生成

**类结构**

```c#
class TilemapLayer{
    int layerIndex;
	int tileGroup;
    float offsetHeight;
    BlockAlgorithm algorithm;
    TileAlgorithm tileAlgorithm;
    DecorateConfig[] decorates;
}
```

- layerIndex

  层索引，地图生成顺序

- tileGroup

  该层所使用的 Tile，Tile组可包含多个Tile，根据权重随机选择Tile，一个层只能指定一个Tile组，如果需要绘制多种Tile需要建立多个层

- offsetHeight

  该层位置偏移高度

- algorithm

  地图数据算法

- tileAlgorithm 

  瓷砖算法

- decorates

  装饰配置

  

#### 数据

地图数据为 `bool` 类型数组，由`BlockAlgorithm` 生成，坐标系为左下坐标系，(0, 0)位置在左下，`true` 为 `Block` 阻挡，`false` 非阻挡，包含一些地图逻辑操作的基本方法，获取指定位置的 Tile 类型

**类结构**

```c#
class TilemapData
{
    int width;
    int height;
    bool[] data;
}
```



### 数据

基类 `BlockAlgorithm`，生成 `TilemapData` 块数据

**类结构**

```c#
class BlockAlgorithm
{
	public virtual void Generate(TilemapData map , TilemapData mask, out Vector3 startPosition, out Vector3 endPosition);
}
```



**内置**

- RandomAlgorithm

  随机地图，生成障碍物

- FrameAlgorithm

  矩形围墙

  ![Map_Frame](Doc~\Images\Map_Frame.PNG)

- MirrorAlgorithm

  对称地图，生成障碍物，水平或垂直对称

  ![Map_Mirror](Doc~\Images\Map_Mirror.PNG)

- BSPDungeonAlgorithm

  地牢类型地图，分布均匀且连通

  ![Map_BSP](Doc~\Images\Map_BSP.PNG)

- MaskAlgorithm

  将 `Mask` 转为地图，支持逻辑操作（Copy, And, Or, Not, Xor)

### 瓷砖

基类 `TileAlgorithm`

**内置**

- DefaultTileAlgorithm

  默认的瓷砖生成

- MirrorTileAlgorithm

  镜像对称拼接，经过旋转实现4个块组合的大地块

  ![Tile_Mirror](Doc~\Images\Tile_Mirror.PNG)

  ![Tile_Mirror2](Doc~\Images\Tile_Mirror2.PNG)

### 装饰

基类 `TilemapDecorateAlgorithm`

**内置**

- RandomDecorateAlgorithm

  指定Tile类型，随机位置生成

- PatternDecorateAlgorithm

  指定Tile类型，连续生成





## 功能

### TilemapOcclusion

Tile 遮挡剔除，添加到相机，相机所见的区域动态显示和隐藏 `Tile`，支持多个Tilemap和 Camera

### TilemapLazy

Tile 延迟加载，根据镜头所看见范围动态生成 `Tile`，`TilemapLazy` 需要和 `TilemapOcclusion` 一起才能实现按需加载



## 扩展

### 加载和销毁

```c#
TilemapCreator.InstantiateGameObject
TilemapCreator.DestroyGameObject
    
public struct TilemapInstantiateData
{
    public int x;
    public int y;
    public GameObject prefab;
    public Transform parent;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
    public TileObjectType objectType;
}
```

定制加载和销毁回调

- 实现对象池，需要频繁变化的地图游戏中(三消游戏)复用对象。

- 数据与表现分离， `Tilemap` 计算位置信息，实例化到其它位置。

