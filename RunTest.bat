cd Nebula.Core.Elysium.MasterServer\bin\debug
start Nebula.Core.Elysium.MasterServer.exe 2000
cd ..\..\..

cd Nebula.Core.Elysium.NodeServer\bin\debug

start Nebula.Core.Elysium.NodeServer.exe 1000 1001 127.0.0.1 2000
start Nebula.Core.Elysium.NodeServer.exe 1002 1003 127.0.0.1 2000
start Nebula.Core.Elysium.NodeServer.exe 1004 1005 127.0.0.1 2000
start Nebula.Core.Elysium.NodeServer.exe 1006 1007 127.0.0.1 2000
cd ..\..\..