; Lights Out 安装程序脚本
; 使用 Inno Setup 6 编译（https://jrsoftware.org/isinfo.php）
;
; 打包步骤：
;   1) 先发布应用（生成 publish 目录）：
;      dotnet publish -c Release -r win-x64 --self-contained true -p:PublishTrimmed=false -o ./publish
;   2) 再编译本脚本：
;      ISCC.exe LightsOut.iss
;   3) 安装包生成在 Output\LightsOut-Setup-1.2.1.exe

#define MyAppName "Lights Out"
#define MyAppVersion "1.2.1"
#define MyAppPublisher "Lights Out"
#define MyAppExeName "LightsOut.exe"
#define MyAppId "{{D922B9E6-485E-4E93-B657-B316F731710C}"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=.\Output
OutputBaseFilename=LightsOut-Setup-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
; 发布产物为 win-x64 自包含，仅允许 64 位系统安装（含 ARM64 上的 x64 兼容）
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoVersion={#MyAppVersion}
; 提示：若使用 Inno Setup 6.3+，可取消下一行注释以在程序托盘运行时自动关闭后安装
; CloseApplications=yes
; RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimp"; MessagesFile: "Languages\ChineseSimplified.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent
