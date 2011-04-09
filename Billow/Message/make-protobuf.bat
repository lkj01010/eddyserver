@echo off
set tool=..\3Party\protobuf-net\net

rem ===============================================
rem  MessagePackage
set proto=MessagePackage\MessagePackage.proto
%tool%\protogen.exe -i:%proto% -o:%proto%.cs -q

pause
