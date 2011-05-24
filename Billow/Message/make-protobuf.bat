@echo off
set tool=..\3Party\protobuf-net\net

rem ===============================================
rem  MessagePackage
set proto=MessagePackage.proto
%tool%\protogen.exe -i:MessagePackage\%proto% -o:..\Eddy\Message\%proto%.cs -q

pause
