// =====================================================================================
// 
//       Filename:  tcp_io_thread.cc
// 
//    Description:  TCPIOThread
// 
//        Version:  1.0
//        Created:  2009-12-06 20:22:27
//       Revision:  none
//       Compiler:  g++
// 
//         Author:  liaoxinwei (Comet), cometliao@gmail.com
//        Company:  eddy
// 
// =====================================================================================

#include    "sdk/tcp_io_thread.h"

#include    <iostream>
#include    <functional>

#include    <boost/thread.hpp>
#include    <boost/make_shared.hpp>
#include    <boost/bind/apply.hpp>

#include    "sdk/tcp_io_thread_manager.h"

namespace eddy { 

using namespace std;
using namespace boost;

TCPIOThread::TCPIOThread(TCPIOThreadID id, TCPIOThreadManager& manager) 
    : id_(id), manager_(manager), sync_timer_(io_service_) {
    }

void TCPIOThread::RunThread() {
  if (thread_ != NULL) //already running
    return;

  thread_ = shared_ptr<thread>(make_shared<thread>(bind(&TCPIOThread::Run, this)));
}

void TCPIOThread::Run() {
  if (id_ != TCPIOThreadManager::kMainThreadID)
    sync_timer_.expires_from_now(manager_.sync_interval() / 2);
  else
    sync_timer_.expires_from_now(manager_.sync_interval());

  sync_timer_.async_wait(bind(&TCPIOThread::HandleSync, this));

  boost::system::error_code error;
  io_service_.run(error);

  if (error) 
    std::cerr << error.message() << std::endl; 
}

void TCPIOThread::Join() {
  if (thread_ != NULL)
    thread_->join();
}

void TCPIOThread::PostCommandFromMe(TCPIOThreadID id, 
                                    const function<void ()>& command) {
  if (id == id_) 
    commands_from_self_.push_back(command);
  else 
    command_lists_to_be_sent_[id].push_back(command);
}

void TCPIOThread::PostCommandToMe(const function<void ()>& command) {
  commands_from_other_lock_.lock();
  commands_from_other_.push_back(command);
  commands_from_other_lock_.unlock();
}

void TCPIOThread::Stop() {
  PostCommandToMe(bind(&TCPIOThread::HandleStop, this));
}

void TCPIOThread::HandleSync() {
  // handle my commands
  {
    for_each(commands_from_self_.begin(), 
             commands_from_self_.end(),
             bind(apply<void>(), _1));
    commands_from_self_.clear();
  }

  // send commands to other threads
  for (std::map<int, CommandList>::iterator it = command_lists_to_be_sent_.begin();
       it != command_lists_to_be_sent_.end();
       ++it) {

    if (it->second.empty())
      continue;

    TCPIOThread& other = manager_.GetThread(it->first);
    other.commands_from_other_lock_.lock();
    other.commands_from_other_.splice(other.commands_from_other_.end(), it->second);
    other.commands_from_other_lock_.unlock();
    assert(it->second.empty());
  }

  // handle commands received from other threads
  {
    CommandList temp_command_list;
    commands_from_other_lock_.lock();
    commands_from_other_.swap(temp_command_list);
    commands_from_other_lock_.unlock();

    for_each(temp_command_list.begin(), temp_command_list.end(),
             bind(apply<void>(), _1));
  }

  // register timer
  {
    sync_timer_.expires_at(sync_timer_.expires_at() + 
                           manager_.sync_interval());
    sync_timer_.async_wait(bind(&TCPIOThread::HandleSync, this));
  }
}

void TCPIOThread::HandleStop() {
  session_queue_.Clear();
  io_service_.stop();
}

}
