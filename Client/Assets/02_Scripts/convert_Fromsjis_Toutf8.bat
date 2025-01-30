for /r %%f in (*.cs) do (call:convert_Fromsjis_Toutf8 %%f)
exit

REM -M=以下のURLはサクラエディタのマクロの配置場所に合わせて記述してください。
:convert_Fromsjis_Toutf8
"C:\Program Files (x86)\sakura\sakura.exe"  %1 -M=C:\Users\student\Documents\convert_Fromsjis_Toutf8.mac
