// =====================================================================================
// 
//       Filename:  refclienttest.cc
// 
//    Description:  ref
// 
//        Version:  1.0
//        Created:  2009-12-11 17:40:42
//       Revision:  none
//       Compiler:  g++
// 
//         Author:  liaoxinwei (Comet), cometliao@gmail.com
//        Company:  eddy
// 
// =====================================================================================


#include <cstdlib>
#include <cstring>
#include <iostream>
#include <boost/asio.hpp>
#include <boost/bind.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/date_time/posix_time/posix_time.hpp>

using boost::asio::ip::tcp;

using namespace boost::asio;
using namespace boost::posix_time;
using namespace std;

const char kLongMessage[] = { "This is a longlonglonglonglonglonglong\n"
  "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong\n"
  "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong\n"
  "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong\n"
  "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong\n"
  "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong\n"
  "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong\n"
  "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong\n"
  "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong\n"
  "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong\n"
  "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong\n"
  "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong\n"
  "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong\n"
  "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong\n"
  "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong\n"
  "longlonglonglonglonglonglonglonglonglonglonglonglonglonglonglonglong\n"
  "message.\n" };

class Session {
 public:
  Session(io_service& io_service, const tcp::endpoint& endpoint) :
  socket_(io_service) {
    buffer_[9] = '\0';
    socket_.connect(endpoint);
    async_write(socket_, buffer(kLongMessage, 9),
                        boost::bind(&Session::HandleWrite, this));
    async_read(socket_, buffer(buffer_, 9), boost::bind(&Session::HandleRead, this));
  }

 private:
  void HandleWrite() {
  }
  void HandleRead() {
    async_write(socket_, buffer(kLongMessage, 9),
                        boost::bind(&Session::HandleWrite, this));
    async_read(socket_, buffer(buffer_, 9), boost::bind(&Session::HandleRead, this));
    cout << buffer_ << endl;
    ptime new_time = microsec_clock::local_time();
    cout << new_time - time << endl;
    time = new_time;
  }

  tcp::socket socket_;
  char buffer_[10];
  ptime time;
};

int main(int argc, char* argv[])
{
  try
  {
    boost::asio::io_service io_service;

    tcp::resolver resolver(io_service);
    tcp::resolver::query query(tcp::v4(), "127.0.0.1", "20000");
    tcp::resolver::iterator iterator = resolver.resolve(query);

    list<boost::shared_ptr<Session> > list;
    for (size_t i = 0; i<1000; ++i)
    {
      list.push_back(boost::shared_ptr<Session>(new Session(io_service, *iterator)));
    }
    io_service.run();
  }
  catch (std::exception& e)
  {
    std::cerr << "Exception: " << e.what() << "\n";
  }

  return 0;
}
