; Lights Out 安装程序脚本
; 使用 Inno Setup 编译

[Setup]
AppName=Lights Out
AppVersion=1.1
AppPublisher=Lights Out
AppVerName=Lights Out 1.1
DefaultDirName={commonpf}\LightsOut
DefaultGroupName=Lights Out
AllowNoIcons=yes
OutputDir=.\Output
OutputBaseFilename=LightsOut-Setup-1.1
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
UninstallDisplayIcon={app}\LightsOut.exe

[Files]
Source: ".\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Lights Out"; Filename: "{app}\LightsOut.exe"
Name: "{commondesktop}\Lights Out"; Filename: "{app}\LightsOut.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加图标:"

[Run]
Filename: "{app}\LightsOut.exe"; Description: "启动 Lights Out"; Flags: nowait postinstall skipifsilent
