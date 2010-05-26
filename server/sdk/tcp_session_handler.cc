// =====================================================================================
// 
//       Filename:  tcp_session_handler.cc
// 
//    Description:  TCPSessionHandler
// 
//        Version:  1.0
//        Created:  2009-12-05 17:08:04
//       Revision:  none
//       Compiler:  g++
// 
//         Author:  liaoxinwei (Comet), cometliao@gmail.com
//        Company:  eddy
// 
// =====================================================================================

#include    "sdk/tcp_session_handler.h"

#include    <functional>

#include    "sdk/tcp_session.h"
#include    "sdk/tcp_session_queue.h"
#include    "sdk/tcp_io_thread_manager.h"
#include    "sdk/tcp_io_thread.h"

namespace eddy { 
  
using namespace std;

TCPSessionHandler::TCPSessionHandler() 
    : session_id_(kInvalidTCPSessionID),
    io_thread_manager_(NULL),
    send_delay_(boost::posix_time::not_a_date_time), 
    send_delay_timer_(NULL) {
    }

TCPSessionHandler::~TCPSessionHandler() {
  if (send_delay_timer_ != NULL)
    delete send_delay_timer_;
}

namespace {
void SendMessageListToSession(TCPIOThread& session_thread,
                              TCPSessionID session_id,
                              NetMessageVector* messageList) {
  boost::shared_ptr<TCPSession> session 
      = session_thread.session_queue().Get(session_id); 

  if (session == NULL)
    return;

  session->PostMessageList(*messageList);
  delete messageList;
}

void PackMessageList(boost::shared_ptr<TCPSessionHandler> handler,
                     TCPIOThread& handler_thread,
                     TCPIOThread& session_thread,
                     TCPSessionID session_id) {
  if (handler->messages_to_be_sent().empty())
    return;

  NetMessageVector* messageList(new NetMessageVector);
  messageList->swap(handler->messages_to_be_sent());
  handler->messages_to_be_sent().reserve(messageList->capacity());
  session_thread.Post(boost::bind(&SendMessageListToSession,
                                  boost::ref(session_thread),
                                  session_id,
                                  messageList));
}

} // 

void TCPSessionHandler::HandleSendTimer(const boost::system::error_code& error) {
  if (error)
    return;

  TCPIOThread& main_thread = io_thread_manager_
      ->GetMainThread();
  TCPIOThread& session_thread = io_thread_manager_
      ->GetThread(session_thread_id_);

  PackMessageList(shared_from_this(), main_thread, session_thread, session_id_);
}

void TCPSessionHandler::SendMessage(NetMessage& message) {
  if (IsClosed())
    return;

  if (message.empty())
    return;

  bool wanna_send = messages_to_be_sent_.empty();
  messages_to_be_sent_.push_back(message);

  if (wanna_send) {
    if (send_delay_.is_not_a_date_time()) {
      TCPIOThread& main_thread = io_thread_manager_
          ->GetMainThread();
      TCPIOThread& session_thread = io_thread_manager_
          ->GetThread(session_thread_id_);

      main_thread.Post(boost::bind(&PackMessageList, 
                                   shared_from_this(),
                                   boost::ref(main_thread),
                                   boost::ref(session_thread),
                                   session_id_));
    } else {
      send_delay_timer_->expires_from_now(send_delay_);
      send_delay_timer_->async_wait(boost::bind(&TCPSessionHandler::HandleSendTimer, 
                                               this, _1));
    }
  }
}

namespace {
void CloseSession(TCPIOThread& thread, TCPSessionID id) {
  TCPSessionQueue::SessionPointer session = thread.session_queue().Get(id);

  if (session == NULL)
    return;

  session->Close();
}
}

void TCPSessionHandler::Init(TCPSessionID session_id, 
                             TCPIOThreadID session_thread_id,
                             TCPIOThreadManager* manager) {
  session_id_ = session_id;
  session_thread_id_ = session_thread_id;
  io_thread_manager_ = manager;
  send_delay_timer_ = new boost::asio::deadline_timer(manager->io_service());
}

void TCPSessionHandler::Dispose() {
  session_id_ = kInvalidTCPSessionID;
}

void TCPSessionHandler::Close() {
  if (IsClosed())
    return;

  TCPIOThread& session_thread = io_thread_manager_->GetThread(session_thread_id_);

  TCPIOThread& main_thread = io_thread_manager_->GetMainThread();

  PackMessageList(shared_from_this(), main_thread, session_thread, session_id_);

  session_thread.Post(boost::bind(&CloseSession, 
                                  boost::ref(session_thread), 
                                  session_id_));
}

void TCPSessionHandler::SetSendDelay(const boost::posix_time::time_duration& delay) {
  send_delay_ = delay;
}

namespace {
void SetSessionReceiveDelay(TCPIOThread& thread,
                            TCPSessionID id, 
                            const boost::posix_time::time_duration& delay) {
  boost::shared_ptr<TCPSession> session = thread.session_queue().Get(id);

  if (!session)
    return;

  session->set_receive_delay(delay);
}
}

void TCPSessionHandler::SetReceiveDelay(const boost::posix_time::time_duration& delay) {
  TCPIOThread& session_thread = io_thread_manager_->GetThread(session_thread_id_);
  session_thread.Post(boost::bind(&SetSessionReceiveDelay, boost::ref(session_thread), 
                                  session_id_, delay));
}

} // end of namespace eddy
