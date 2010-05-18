// =====================================================================================
// 
//       Filename:  messagetest.cc
// 
//    Description:  test protobuf
// 
//        Version:  1.0
//        Created:  2010-05-18 22:12:58
//       Revision:  none
//       Compiler:  g++
// 
//         Author:  liaoxinwei (Comet), cometliao@gmail.com
//        Company:  eddy
// 
// =====================================================================================

#include    <iostream>
#include    <string>

#include    <boost/iostreams/stream.hpp>

#include    "tests/messagetest.pb.h"

#include    "sdk/net_message.h"
#include    "sdk/container_device.h"

int main() {
  using namespace std;
  using namespace eddy;
  using namespace messagetest;
  using namespace boost::iostreams;

  MessageA message1;
  MessageA message2;

  message1.set_name("my message");
  message1.set_id(69);
  
  typedef ContainerDevice<NetMessage> NetMessageDevice;
  NetMessage net_message;
  stream<NetMessageDevice>  io(net_message);

  message1.SerializeToOstream(&io);
  io.flush();
  io.seekg(0, std::ios_base::beg); // seek to the beginning
  message2.ParseFromIstream(&io);

  cout << message2.DebugString() << endl;
  return 0;
}
