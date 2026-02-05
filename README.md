# DalamudMemoEngine

[memo-engin](https://github.com/open-xiv/memo-engine) 的 Dalamud 封装


## 如何使用

```csharp
var memoEngine = new DalamudMemoEngine.DalamudMemoEngine(pluginInterface);
// 使用自定义时间源
// var memoEngine = new DalamudMemoEngine.DalamudMemoEngine(pluginInterface, () => YourStandardTimeManager.UTCNowOffset);
memoEngine.Enable();
memoEngine.OnFightFinalized += OnFightFinalized;
```

插件卸载时不要忘了 Dispose
```
memoEngine.Dispose();
```