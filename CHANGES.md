# 改动记录（相对上游）

本仓库基于 [memory-yiyi/PopupBlocker](https://github.com/memory-yiyi/PopupBlocker)（作者 yiyiOfficial）修改而来。
按 GPL-3.0 的要求，这里逐条记录本分支相对上游的全部改动，改动日期为 2026-09-17。

## 界面动效

- `Themes/Generic.xaml`
  - 圆形图标按钮的悬停/按下由瞬间换色改为叠加层透明度补间（进入 150/80 毫秒，离开 250/300 毫秒）。
  - Class/Title 切换按钮改为双层色块交叉过渡（180 毫秒），文字颜色随之一同淡换。
  - 规则卡片、进程分组卡片出现时淡入并上移 10 像素（220/300 毫秒，减速曲线）。
- `Views/MainWindow.xaml`
  - 窗口内容整体淡入（280 毫秒）。
  - 显式声明页面切换动画与时长（`FadeInWithSlide`，250 毫秒），便于按喜好调整。

## 列表项的退出与重排

- 新增 `Controls/FluidStackPanel.cs`：比较子元素的新旧布局槽位并补间，列表增删时其余卡片平滑滑动而不是瞬间跳位。
- 新增 `Controls/AnimatedRemoveCommand.cs`：删除前先让目标卡片淡出 180 毫秒再真正执行删除，并带兜底计时器保证删除一定发生。
- `Controls/InterceptorRuleCard.cs`、`Controls/InterceptorRulesCard.cs`：对外暴露 `AnimatedRemoveCommand`。
- `ViewModels/PopupBlockViewModel.cs`：界面列表由“整份替换”改为可观察集合就地增删。原来每次规则变化都会重建所有卡片，导致入场动画重播、已展开的进程分组被收拢、重排动画无从谈起。
- `Models/BlockRules.cs`：子规则集合改用 `ObservableCollection`，往已有进程追加规则时子卡片可以就地增删。

## 深色主题

- 新增 `Commons/ThemeSwitcher.cs`：切换主题时用当前界面的快照做交叉淡出；装饰性部分全部做了容错，失败也不影响主题本身切换成功。
- `ViewModels/SettingViewModel.cs`、`Views/Pages/SettingPage.xaml`：新增“深色主题”开关，随设置持久化，下次启动自动生效。
- `Views/Tray.xaml.cs`：切换主题时显式更新主窗口的背景效果（界面库只会自动处理 `Application.MainWindow`）。

## 单实例与设置持久化

- `App.xaml.cs`、`Views/Tray.xaml.cs`：新增单实例机制。关闭窗口后程序仍在托盘运行，此时再次启动不会再产生第二个进程，而是把已有窗口唤回前台；开机自启触发的重复启动静默退出。
- `ViewModels/SettingViewModel.cs`：启动拦截、开机自启、切换主题任一失败都不再中断整个设置的加载（原来一处异常就会把用户的全部设置丢掉换成默认值）；读取与保存失败会记录原因。
- `Utility/Commons/FileOperation.cs`：配置文件读写放宽共享模式，减少因临时占用导致的读取失败。

## 托盘图标

- `Views/Tray.xaml`、`Views/Tray.xaml.cs`：
  - 修复关闭主窗口后托盘图标消失的问题：界面库把图标的宿主窗口挂在 `Application.MainWindow` 名下，而窗口关闭会连带销毁挂在自己名下的窗口，系统随即撤掉图标。改为让图标宿主固定在常驻的托盘宿主窗口上。
  - 左键点击托盘图标改由程序自行处理，直接唤出主窗口。
  - 新增 `TaskbarCreated` 处理：资源管理器重启或系统更新后，系统会要求各程序重新注册托盘图标，界面库不处理这件事，不补这一步图标会永久消失。

## 构建

- `PopupBlocker.Core.csproj`：新增 `SkipComReference` 开关。.NET SDK 自带的 MSBuild 不支持 `COMReference`（报 MSB4803），命令行编译时传 `-p:SkipComReference=true` 改用预生成的互操作程序集；在 Visual Studio 中不传该开关，行为与上游一致。
- 新增 `PopupBlocker.Core/lib/Interop.TaskScheduler.dll`（tlbimp 生成的 COM 互操作程序集）及其说明文件。

