REM Copies Dlls from the RSSDK directory into the appropriate folders in the sample project
copy /y "%RSSDK_DIR%\bin\win32\libpxcclr.unity.dll" "Assets\Plugins.Managed\libpxcclr.unity.dll"
copy /y "%RSSDK_DIR%\framework\common\pxcclr.cs\src\pxcmdefs.extensions.cs" "Assets\Plugins.Managed\pxcmdefs.extensions.cs"

REM native libpxccpp2c dlls
copy /y "%RSSDK_DIR%\bin\win32\libpxccpp2c.dll" "Assets\Plugins\x86\libpxccpp2c.dll"
copy /y "%RSSDK_DIR%\bin\x64\libpxccpp2c.dll" "Assets\Plugins\x86_64\libpxccpp2c.dll"