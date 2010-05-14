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

TCPSessionHandler::TCPSessionHandler() : session_id_(kInvalidTCPSessionID) {}

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
  assert(!handler->messages_to_be_sent().empty());
  if (handler->messages_to_be_sent().empty())
    return;

  NetMessageVector* messageList(new NetMessageVector);
  messageList->swap(handler->messages_to_be_sent());
  handler->messages_to_be_sent().reserve(messageList->capacity());
  handler_thread.PostCommandFromMe(handler->session_thread_id(),
                                      boost::bind(&SendMessageListToSession,
                                                boost::ref(session_thread),
                                                session_id,
                                                messageList));
}

} // 

void TCPSessionHandler::SendMessage(NetMessage& message) {
  if (IsClosed())
    return;

  if (message.empty())
    return;

  bool wanna_send = messages_to_be_sent_.empty();
  messages_to_be_sent_.push_back(message);

  if (wanna_send) {
    TCPIOThread& main_thread = io_thread_manager_
        ->GetThread(TCPIOThreadManager::kMainThreadID);
    TCPIOThread& session_thread = io_thread_manager_
        ->GetThread(session_thread_id_);
    main_thread.PostCommandFromMe(TCPIOThreadManager::kMainThreadID,
                                  boost::bind(&PackMessageList, 
                                       shared_from_this(),
                                       boost::ref(main_thread),
                                       boost::ref(session_thread),
                                       session_id_));
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
                             TCPIOThreadManager& io_thread_manager) {
  session_id_ = session_id;
  session_thread_id_ = session_thread_id;
  io_thread_manager_ = &io_thread_manager;
}

void TCPSessionHandler::Dispose() {
  io_thread_manager_ = NULL;
  session_id_ = kInvalidTCPSessionID;
}

void TCPSessionHandler::Close() {
  if (IsClosed())
    return;

  TCPIOThread& session_thread = io_thread_manager_->GetThread(session_thread_id_);

  TCPIOThread& main_thread = io_thread_manager_
      ->GetThread(TCPIOThreadManager::kMainThreadID);

  main_thread.PostCommandFromMe(session_thread_id_, 
                                boost::bind(&CloseSession, 
                                     boost::ref(session_thread), 
                                     session_id_));
}

}
