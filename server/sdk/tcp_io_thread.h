// =====================================================================================
// 
//       Filename:  tcp_io_thread.h
// 
//    Description:  TCPIOThread
// 
//        Version:  1.0
//        Created:  2009-12-06 14:58:44
//       Revision:  none
//       Compiler:  g++
// 
//         Author:  liaoxinwei (Comet), cometliao@gmail.com
//        Company:  eddy
// 
// =====================================================================================

#ifndef  TCP_IO_THREAD_H_
#define  TCP_IO_THREAD_H_

#include    <list>
#include    <map>

#include    <boost/function.hpp>
#include    <boost/thread.hpp>
#include    <boost/asio/io_service.hpp>
#include    <boost/asio/deadline_timer.hpp>

#include    "sdk/tcp_defs.h"
#include    "sdk/tcp_session_queue.h"
#include    "sdk/spinlock.h"

namespace eddy {

class TCPIOThreadManager;

// =====================================================================================
//        Class:  TCPIOThread
//  Description:  Manages TCPSession for dealing with TCP I/O, including cpu-consuming 
//                operations such as compression, encryption, network I/O, etc.
// =====================================================================================
class TCPIOThread : public boost::noncopyable {
 public:
  // ====================  TYPEDEFS      =======================================
  typedef std::list<boost::function<void ()> > CommandList;

  // ====================  LIFECYCLE     =======================================
  TCPIOThread(TCPIOThreadID id, TCPIOThreadManager& manager);

  // ====================  ACCESSORS     =======================================
  TCPIOThreadID id() const { return id_; }
  boost::asio::io_service& io_service() { return io_service_; }
  TCPSessionQueue& session_queue() { return session_queue_; }
  TCPIOThreadManager& manager() { return manager_; }

  // ====================  MUTATORS      =======================================

  // ====================  OPERATIONS    =======================================
  // The command will be buffered, posted to specific thread at next 
  // manager().sync_interval() expired, executed in that thead, then discarded.
  // Must call in this thread.
  void PostCommandFromMe(TCPIOThreadID id, const boost::function<void ()>& command);
  // Posts command to this thread
  void PostCommandToMe(const boost::function<void ()>& command);
  // runs in a new thread.
  void RunThread();
  // runs in current thread.
  void Run();
  void Join();
  void Stop();

 private:
  friend class TCPIOThreadManager;
  // ====================  OPERATIONS    =======================================
  void HandleSync();
  void HandleStop();

  // ====================  DATA MEMBERS  =======================================
  TCPIOThreadID                     id_;
  TCPIOThreadManager&               manager_;
  TCPSessionQueue                   session_queue_;
  boost::asio::io_service           io_service_;
  CommandList                       commands_from_self_;
  spinlock                          commands_from_other_lock_; 
  CommandList                       commands_from_other_; 
  std::map<int, CommandList>        command_lists_to_be_sent_;
  boost::shared_ptr<boost::thread>  thread_;
  boost::asio::deadline_timer       sync_timer_;
}; // -----  end of class TCPIOThread  -----

}

#endif   // ----- #ifndef TCP_IO_THREAD_H_  -----
