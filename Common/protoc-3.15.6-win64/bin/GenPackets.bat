
REM protoc 를 실행   ./(자기 자신)
REM -I=[$SRC_DIR] : 인풋 디렉토리 설정
REM --csharp_out=[$DST_DIR] : 어디에 출력하는지 설정
REM [$SRC_DIR]/[파일] : 읽을 파일
REM protoc -I=[$SRC_DIR] --csharp_out=[$DST_DIR] [$SRC_DIR/addressbook.proto]
protoc -I=./ --csharp_out=./ ./Protocol.proto




REM : 주석
REM START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml
REM XCOPY /Y GenPackets.cs "../../DummyClient/Packet"
REM XCOPY /Y GenPackets.cs "../../Server/Packet"
REM XCOPY /Y ClientPacketManager.cs "../../DummyClient/Packet"
REM XCOPY /Y ServerPacketManager.cs "../../Server/Packet"