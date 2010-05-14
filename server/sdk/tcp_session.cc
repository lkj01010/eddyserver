// =====================================================================================
// 
//       Filename:  tcp_session.cc
// 
//    Description:  TCPSession
// 
//        Version:  1.0
//        Created:  2009-12-05 13:49:41
//       Revision:  none
//       Compiler:  g++
// 
//         Author:  liaoxinwei (Comet), cometliao@gmail.com
//        Company:  eddy
// 
// =====================================================================================

#include    "sdk/tcp_session.h"

#include    <iostream>
#include    <cassert>

#include    <boost/asio.hpp> 
#include    <boost/bind.hpp> 

#include    "sdk/tcp_io_thread_manager.h"
#include    "sdk/tcp_io_thread.h"
#include    "sdk/tcp_session_queue.h"
#include    "sdk/tcp_session_handler.h"
#include    "sdk/net_message_filter_interface.h"

namespace eddy { 

TCPSession::TCPSession(TCPIOThreadManager& io_thread_manager,
                       FilterPointer filter)
    : id_(kInvalidTCPSessionID),
    thread_(io_thread_manager.GetThread()),
    socket_(thread_.io_service()),
    filter_(filter) {
      buffer_to_be_sent_.reserve(16);
      buffer_sending_.reserve(16);
      buffer_receiving_.resize(16);
    }

TCPSession::~TCPSession() {
  Close();
}

void TCPSession::PostMessageList(NetMessageVector& messageList) {
  assert(messages_to_be_sent_.empty());
  messages_to_be_sent_.swap(messageList);

  size_t bytes_wanna_write = filter_->BytesWannaWrite(messages_to_be_sent_);

  if (bytes_wanna_write == 0)
    return;

  buffer_to_be_sent_.reserve(buffer_to_be_sent_.size() + bytes_wanna_write);
  filter_->Write(messages_to_be_sent_, buffer_to_be_sent_);

  messages_to_be_sent_.clear();

  if (buffer_sending_.size() == 0) {    // not sending
    buffer_sending_.swap(buffer_to_be_sent_);
    boost::asio::async_write(socket_, 
                             boost::asio::buffer(&buffer_sending_[0], buffer_sending_.size()),
                             boost::bind(&TCPSession::HandleWrite, shared_from_this(),
                                       _1,
                                       _2));
  }
}

void TCPSession::Init(TCPSessionID id) {
  assert(id_ == kInvalidTCPSessionID);
  id_ = id;

  boost::asio::ip::tcp::no_delay option(true);
  socket_.set_option(option);

  if (!thread_.session_queue().Add(shared_from_this())) 
    assert(false);

  size_t bytes_wanna_read = filter_->BytesWannaRead();

  if (bytes_wanna_read == 0) 
    return;

  if (bytes_wanna_read == size_t(-1)) {
    socket_.async_read_some(boost::asio::buffer(&buffer_receiving_[0], 
                                                buffer_receiving_.capacity()),
                            boost::bind(&TCPSession::HandleRead, shared_from_this(),
                                      _1,
                                      _2));
  } else {
    buffer_receiving_.resize(bytes_wanna_read);
    boost::asio::async_read(socket_,
                            boost::asio::buffer(&buffer_receiving_[0], bytes_wanna_read),
                            boost::bind(&TCPSession::HandleRead, shared_from_this(),
                                      _1,
                                      _2));
  }
}

namespace {

void SendMessageListToHandler(TCPIOThreadManager& manager,
                              TCPSessionID id,
                              NetMessageVector* messageList) {
  boost::shared_ptr<TCPSessionHandler> sessionHandler = manager.GetSessionHandler(id);

  if (sessionHandler == NULL)
    return;

  for_each(messageList->begin(), messageList->end(),
           boost::bind(&TCPSessionHandler::OnMessage, 
                     sessionHandler, _1));

  delete messageList;
}

void PackMessageList(boost::shared_ptr<TCPSession> session) {
  NetMessageVector* messageList(new NetMessageVector);
  messageList->swap(session->messages_received());
  session->thread().PostCommandFromMe(TCPIOThreadManager::kMainThreadID,
                                      boost::bind(&SendMessageListToHandler,
                                                boost::ref(session->thread().manager()),
                                                session->id(),
                                                messageList));
}

} // 

void TCPSession::HandleRead(boost::weak_ptr<TCPSession> weak_session, 
                            const boost::system::error_code& error,
                            size_t bytes_transferred) {
  if (weak_session.expired())
    return;

  boost::shared_ptr<TCPSession> session = weak_session.lock();

  if (error) {
    session->Close();
    return;
  }

  bool wanna_post = session->messages_received_.empty();

  session->buffer_receiving_.resize(bytes_transferred);

  size_t bytes_read = session->filter_->Read(session->messages_received_,
                                             session->buffer_receiving_);
  assert(bytes_read == bytes_transferred);

  session->buffer_receiving_.clear();

  wanna_post = wanna_post && !session->messages_received_.empty();

  if (wanna_post) {
    session->thread_.PostCommandFromMe(session->thread_.id(),
                                       boost::bind(&PackMessageList, session));
  }

  size_t bytes_wanna_read = session->filter_->BytesWannaRead();

  if (bytes_wanna_read == 0)
    return;

  if (bytes_wanna_read == size_t(-1)) {
    session->socket_.async_read_some(boost::asio::buffer(&session->buffer_receiving_[0], 
                                                         session->buffer_receiving_.capacity()),
                                     boost::bind(&TCPSession::HandleRead, weak_session,
                                               _1,
                                               _2));
  } else {
    session->buffer_receiving_.resize(bytes_wanna_read);
    boost::asio::async_read(session->socket_,
                            boost::asio::buffer(&session->buffer_receiving_[0], bytes_wanna_read),
                            boost::bind(&TCPSession::HandleRead, weak_session,
                                      _1,
                                      _2));
  }
}

void TCPSession::HandleWrite(boost::weak_ptr<TCPSession> weak_session,
                             const boost::system::error_code& error,
                             size_t bytes_transferred) {
  if (weak_session.expired())
    return;

  boost::shared_ptr<TCPSession> session = weak_session.lock();

  if (error) {
    session->Close();
    return;
  }

  session->buffer_sending_.clear();
#if 0
  if (session->id_ == kInvalidTCPSessionID) {
    return;
  }

  assert(bytes_transferred == session->bytes_sending_);
#endif

  if (session->buffer_to_be_sent_.empty()) {
    size_t bytes_wanna_write 
        = session->filter_->BytesWannaWrite(session->messages_to_be_sent_);

    if (bytes_wanna_write == 0)
      return;

    session->buffer_to_be_sent_.reserve(session->buffer_to_be_sent_.size() 
                                        + bytes_wanna_write);

    session->filter_->Write(session->messages_to_be_sent_, 
                            session->buffer_to_be_sent_);
  }

  session->buffer_sending_.swap(session->buffer_to_be_sent_);
  boost::asio::async_write(session->socket_, 
                           boost::asio::buffer(&session->buffer_sending_[0], 
                                               session->buffer_sending_.size()),
                           boost::bind(&TCPSession::HandleWrite, weak_session,
                                       _1,
                                       _2));
}

void TCPSession::Close() {
  if (id_ == kInvalidTCPSessionID)
    return;

  socket_.close();
  thread_.session_queue().Remove(id_);
  thread_.PostCommandFromMe(TCPIOThreadManager::kMainThreadID, 
                            boost::bind(&TCPIOThreadManager::OnSessionClose,
                                        &thread_.manager(),
                                        id_));
  id_ = kInvalidTCPSessionID;
  buffer_sending_.clear();
}

} // namespace
