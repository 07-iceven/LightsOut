# LightsOut - 定时关机小工具

LightsOut 是一款基于 WPF 开发的现代 Windows 定时关机工具。它拥有简洁直观的界面，支持设置多个定时关机任务，并提供倒计时提醒和开机自启功能。

## 功能特点

- **多时段设置**: 支持添加多个定时关机时间点，灵活满足不同需求。
- **倒计时显示**: 实时显示距离下次关机还剩多少时间。
- **托盘运行**: 支持最小化到系统托盘，后台静默运行。
- **开机自启**: 可选跟随系统启动，无需手动开启。
- **现代 UI**: 采用 WPF-UI 风格，完美契合 Windows 11 Fluent Design 视觉体验。
- **自动保存**: 关机任务和状态设置会自动保存，下次启动自动加载。
- **多语言支持**: 支持中文、英文、日文三种语言。

## 技术栈

- **框架**: .NET 8.0 (WPF)
- **模式**: MVVM (使用 CommunityToolkit.Mvvm)
- **UI 库**: WPF-UI
- **组件**: Hardcodet.NotifyIcon.Wpf (系统托盘支持)

## 版本

当前版本: **1.3**

## 如何使用

### 运行环境
- Windows 10 或更高版本

### 安装方式

#### 方式一：使用安装包（推荐）
1. 下载 `LightsOut-Setup-1.2.1.exe` 安装包
2. 双击运行安装程序
3. 按照向导完成安装
4. 从开始菜单或桌面快捷方式启动

#### 方式二：从源码运行
1. 克隆或下载本仓库代码。
2. 使用 Visual Studio 2022 或更高版本打开项目。
3. 编译并运行项目。

### 操作指南
1. **添加时间**: 在主界面设置小时和分钟，点击“添加”按钮。
2. **启用/禁用**: 通过列表右侧的开关控制单个任务。
3. **开启服务**: 确保主界面的总开关处于开启状态，倒计时才会开始工作。
4. **设置**: 在设置面板可以开启“开机自启动”和切换语言。

## 开发者指南

### 发布应用程序

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishTrimmed=false -o ./publish
```

### 使用 Inno Setup 打包

1. 下载并安装 [Inno Setup](https://jrsoftware.org/isdl.php)
2. 使用 Inno Setup Compiler 打开 `LightsOut.iss`
3. 按 `F9` 或点击 Build → Compile 编译
4. 或使用命令行执行 `ISCC.exe .\LightsOut.iss`
5. 安装包将生成在 `Output/LightsOut-Setup-1.2.1.exe`

## 项目结构

- **Models**: 数据模型（如关机时间配置）。
- **ViewModels**: 业务逻辑处理。
- **Views**: 界面布局 (XAML)。
- **Helpers**: 辅助工具类（如设置持久化服务）。
- **Resources**: 多语言资源文件。
