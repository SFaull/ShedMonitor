############################################################################################
#      NSIS Installation Script created by NSIS Quick Setup Script Generator v1.09.18
#               Entirely Edited with NullSoft Scriptable Installation System
#              by Vlasis K. Barkas aka Red Wine red_wine@freemail.gr Sep 2006
############################################################################################
!define PROJECT_ROOT ".."
!define APP_NAME "Smart Monitor"
!define COMP_NAME "Faull Gizmos"
!define VERSION "00.01.00.00"
!define COPYRIGHT "${COMP_NAME} © 2019"
!define DESCRIPTION "Display Panel for MQTT Sensor Data"
!define INSTALLER_NAME "${PROJECT_ROOT}\nsi\SmartMonitor.exe"
!define MAIN_APP_EXE "SmartMonitor.exe"
!define MAIN_APP_PATH "SmartMonitor\bin\Release"
!define INSTALL_TYPE "SetShellVarContext all"
!define REG_ROOT "HKLM"
!define REG_APP_PATH "Software\Microsoft\Windows\CurrentVersion\App Paths\${MAIN_APP_EXE}"
!define UNINSTALL_PATH "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}"

!define REG_START_MENU "Start Menu Folder"

var SM_Folder

######################################################################
!getdllversion "${PROJECT_ROOT}\${MAIN_APP_PATH}\${MAIN_APP_EXE}" expv_
VIProductVersion  "${expv_1}.${expv_2}.${expv_3}.${expv_4}"
VIAddVersionKey "ProductName"  "${APP_NAME}"
VIAddVersionKey "CompanyName"  "${COMP_NAME}"
VIAddVersionKey "LegalCopyright"  "${COPYRIGHT}"
VIAddVersionKey "FileDescription"  "${DESCRIPTION}"
VIAddVersionKey "FileVersion"  "${expv_1}.${expv_2}.${expv_3}.${expv_4}"

######################################################################

SetCompressor ZLIB
Name "${APP_NAME}"
Caption "${APP_NAME}"
OutFile "${INSTALLER_NAME}"
BrandingText "${APP_NAME}"
XPStyle on
InstallDirRegKey "${REG_ROOT}" "${REG_APP_PATH}" ""
InstallDir "$PROGRAMFILES\${COMP_NAME}"

######################################################################

!include "x64.nsh"
!include "MUI.nsh"
!include "FileFunc.nsh"

!define MUI_ABORTWARNING
!define MUI_UNABORTWARNING

!insertmacro MUI_PAGE_WELCOME

!ifdef LICENSE_TXT
!insertmacro MUI_PAGE_LICENSE "${LICENSE_TXT}"
!endif

!ifdef REG_START_MENU
!define MUI_STARTMENUPAGE_DEFAULTFOLDER ""
!define MUI_STARTMENUPAGE_REGISTRY_ROOT "${REG_ROOT}"
!define MUI_STARTMENUPAGE_REGISTRY_KEY "${UNINSTALL_PATH}"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "${REG_START_MENU}"
!insertmacro MUI_PAGE_STARTMENU Application $SM_Folder
!endif

!insertmacro MUI_PAGE_INSTFILES

!define MUI_FINISHPAGE_RUN "$INSTDIR\${MAIN_APP_EXE}"
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM

!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "English"

######################################################################

Section -MainProgram
SetOverwrite ifnewer
##!getdllversion "$%windir%\explorer.exe" expv_
##!echo "Explorer.exe version is ${expv_1}.${expv_2}.${expv_3}.${expv_4}"

##${GetFileVersion} "${PROJECT_ROOT}\${MAIN_APP_PATH}\${MAIN_APP_EXE}" $R0
DetailPrint "GUI Version is ${expv_1}.${expv_2}.${expv_3}.${expv_4}"
##StrCpy $EXEversion "${expv_1}.${expv_2}.${expv_3}.${expv_4}"
##DetailPrint "$EXEversion"

${INSTALL_TYPE}
SetOverwrite ifnewer
SetOutPath "$INSTDIR"
File "${PROJECT_ROOT}\${MAIN_APP_PATH}\${MAIN_APP_EXE}"	## Copy files
File "${PROJECT_ROOT}\${MAIN_APP_PATH}\${MAIN_APP_EXE}.config"
File "${PROJECT_ROOT}\${MAIN_APP_PATH}\LiveCharts.dll"
File "${PROJECT_ROOT}\${MAIN_APP_PATH}\LiveCharts.Wpf.dll"
File "${PROJECT_ROOT}\${MAIN_APP_PATH}\M2Mqtt.Net.dll"
File "${PROJECT_ROOT}\${MAIN_APP_PATH}\MaterialDesignColors.dll"
File "${PROJECT_ROOT}\${MAIN_APP_PATH}\MaterialDesignThemes.Wpf.dll"
SectionEnd

######################################################################

Section -Icons_Reg
SetOutPath "$INSTDIR"
WriteUninstaller "$INSTDIR\uninstall.exe"

!ifdef REG_START_MENU
!insertmacro MUI_STARTMENU_WRITE_BEGIN Application
CreateDirectory "$SMPROGRAMS\$SM_Folder"
CreateShortCut "$SMPROGRAMS\$SM_Folder\${APP_NAME}.lnk" "$INSTDIR\${MAIN_APP_EXE}"
CreateShortCut "$DESKTOP\${APP_NAME}.lnk" "$INSTDIR\${MAIN_APP_EXE}"
!ifdef WEB_SITE
WriteIniStr "$INSTDIR\${APP_NAME} website.url" "InternetShortcut" "URL" "${WEB_SITE}"
CreateShortCut "$SMPROGRAMS\$SM_Folder\${APP_NAME} Website.lnk" "$INSTDIR\${APP_NAME} website.url"
!endif
!insertmacro MUI_STARTMENU_WRITE_END
!endif

!ifndef REG_START_MENU
CreateDirectory "$SMPROGRAMS\${COMP_NAME}"
CreateShortCut "$SMPROGRAMS\${COMP_NAME}\${APP_NAME}.lnk" "$INSTDIR\${MAIN_APP_EXE}"
CreateShortCut "$DESKTOP\${APP_NAME}.lnk" "$INSTDIR\${MAIN_APP_EXE}"
!ifdef WEB_SITE
WriteIniStr "$INSTDIR\${APP_NAME} website.url" "InternetShortcut" "URL" "${WEB_SITE}"
CreateShortCut "$SMPROGRAMS\${COMP_NAME}\${APP_NAME} Website.lnk" "$INSTDIR\${APP_NAME} website.url"
!endif
!endif

WriteRegStr ${REG_ROOT} "${REG_APP_PATH}" "" "$INSTDIR\${MAIN_APP_EXE}"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "DisplayName" "${APP_NAME}"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "UninstallString" "$INSTDIR\uninstall.exe"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "DisplayIcon" "$INSTDIR\${MAIN_APP_EXE}"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "DisplayVersion" "${VERSION}"
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "Publisher" "${COMP_NAME}"

!ifdef WEB_SITE
WriteRegStr ${REG_ROOT} "${UNINSTALL_PATH}"  "URLInfoAbout" "${WEB_SITE}"
!endif

SectionEnd

######################################################################

Section Uninstall
${INSTALL_TYPE}
##${GetFileVersion} "$INSTDIR\${MAIN_APP_EXE}" $R0
DetailPrint "Program Version is ${expv_1}.${expv_2}.${expv_3}.${expv_4}"
RMDir /r "$APPDATA\${COMP_NAME}\${APP_NAME}\${expv_1}.${expv_2}.${expv_3}.${expv_4}"

Delete "$INSTDIR\${MAIN_APP_EXE}"
Delete "$INSTDIR\${MAIN_APP_EXE}.config"
Delete "$INSTDIR\LiveCharts.dll"
Delete "$INSTDIR\LiveCharts.Wpf.dll"
Delete "$INSTDIR\M2Mqtt.Net.dll"
Delete "$INSTDIR\MaterialDesignColors.dll"
Delete "$INSTDIR\MaterialDesignThemes.Wpf.dll"

!ifdef WEB_SITE
Delete "$INSTDIR\${APP_NAME} website.url"
!endif

Delete "$INSTDIR\uninstall.exe"
!ifdef WEB_SITE
Delete "$INSTDIR\${APP_NAME} website.url"
!endif

RmDir "$INSTDIR"

!ifdef REG_START_MENU
!insertmacro MUI_STARTMENU_GETFOLDER "Application" $SM_Folder
Delete "$SMPROGRAMS\$SM_Folder\${APP_NAME}.lnk"
!ifdef WEB_SITE
Delete "$SMPROGRAMS\$SM_Folder\${APP_NAME} Website.lnk"
!endif
Delete "$DESKTOP\${APP_NAME}.lnk"

RmDir "$SMPROGRAMS\$SM_Folder"
!endif

!ifndef REG_START_MENU
Delete "$SMPROGRAMS\${COMP_NAME}\${APP_NAME}.lnk"
!ifdef WEB_SITE
Delete "$SMPROGRAMS\${COMP_NAME}\${APP_NAME} Website.lnk"
!endif
Delete "$DESKTOP\${APP_NAME}.lnk"

RmDir "$SMPROGRAMS\${COMP_NAME}"
!endif

DeleteRegKey ${REG_ROOT} "${REG_APP_PATH}"
DeleteRegKey ${REG_ROOT} "${UNINSTALL_PATH}"
SectionEnd

######################################################################

