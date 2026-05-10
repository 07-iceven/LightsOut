; LightsOut 安装程序脚本
; 使用 Inno Setup 编译

[Setup]
AppName=LightsOut
AppVersion=1.0
AppPublisher=LightsOut
DefaultDirName={commonpf}\LightsOut
DefaultGroupName=LightsOut
AllowNoIcons=yes
OutputDir=.\Output
OutputBaseFilename=LightsOut-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
UninstallDisplayIcon={app}\LightsOut.exe

[Files]
Source: ".\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\LightsOut"; Filename: "{app}\LightsOut.exe"
Name: "{commondesktop}\LightsOut"; Filename: "{app}\LightsOut.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加图标:"

[Run]
Filename: "{app}\LightsOut.exe"; Description: "启动 LightsOut"; Flags: nowait postinstall skipifsilent
