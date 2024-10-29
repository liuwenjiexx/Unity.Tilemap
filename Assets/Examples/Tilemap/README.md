# Tilemap Example

## Dynamic

运行时随机生成地图，运行游戏，点击 TilemapCreator `Build` 按钮

## CustomLoad

```c#
TilemapCreator.InstantiateGameObject
TilemapCreator.DestroyGameObject
```

定制加载和销毁回调，在需要频繁 `Build` 的游戏中，如三消游戏，实现对象池复用对象

## Occlusion

遮挡剔除样例，场景包含2个Tilemap 和2个相机（透视和正交），运行游戏， `WSAD` 键移动镜头，所见的区域动态显示和隐藏 `Tile`

**实现**

- TilemapOcclusion

## Lazy

延迟加载例子，支持超大的地图，运行游戏，`WSAD`键移动镜头，根据镜头所看见范围动态生成 `Tile`，样例中包含10万个`Tile`，不会有卡顿

`TilemapLazy` 需要和 `TilemapOcclusion` 一起才能实现按需加载

**实现**

- TilemapOcclusion

- TilemapLazy

## NavMesh

 `NavMesh` 寻路样例，运行游戏，鼠标点击地面，`Player` 会移动到目标点

**实现**

- `Tile` 添加 `NavMeshObstacle` 组件支持 `NavMesh`

