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
#include    <boost/asio/error.hpp> 
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
    filter_(filter),
    num_handlers_(0),
    closed_(false) {
      buffer_to_be_sent_.reserve(16);
      buffer_sending_.reserve(16);
      buffer_receiving_.resize(16);
    }

TCPSession::~TCPSession() {
}

void TCPSession::PostMessageList(NetMessageVector& messageList) {
  assert(messages_to_be_sent_.empty());
  messages_to_be_sent_.swap(messageList);

  size_t bytes_wanna_write = filter_->BytesWannaWrite(messages_to_be_sent_);

  if (bytes_wanna_write == 0)
    return;

  buffer_to_be_sent_.reserve(bytes_wanna_write);
  filter_->Write(messages_to_be_sent_, buffer_to_be_sent_);

  messages_to_be_sent_.clear();

  if (buffer_sending_.size() == 0) {    // not sending
    buffer_sending_.swap(buffer_to_be_sent_);
    ++ num_handlers_;
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
    ++ num_handlers_;
    socket_.async_read_some(boost::asio::buffer(&buffer_receiving_[0], 
                                                buffer_receiving_.capacity()),
                            boost::bind(&TCPSession::HandleRead, this,
                                        _1,
                                        _2));
  } else {
    buffer_receiving_.resize(bytes_wanna_read);
    ++ num_handlers_;
    boost::asio::async_read(socket_,
                            boost::asio::buffer(&buffer_receiving_[0], bytes_wanna_read),
                            boost::bind(&TCPSession::HandleRead, this,
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

void TCPSession::HandleRead(const boost::system::error_code& error,
                            size_t bytes_transferred) {
  -- num_handlers_;
  assert(num_handlers_ >= 0);

  if (error) {
    if (closed_) {
      if (num_handlers_ == 0)
        HandleClose();
      // else do nothing
    } else {
      Close();
    }
    return;
  }

  bool wanna_post = this->messages_received_.empty();

  this->buffer_receiving_.resize(bytes_transferred);

  size_t bytes_read = this->filter_->Read(this->messages_received_,
                                          this->buffer_receiving_);
  assert(bytes_read == bytes_transferred);

  this->buffer_receiving_.clear();

  wanna_post = wanna_post && !this->messages_received_.empty();

  if (wanna_post) {
    this->thread_.PostCommandFromMe(this->thread_.id(),
                                    boost::bind(&PackMessageList, shared_from_this()));
  }

  size_t bytes_wanna_read = this->filter_->BytesWannaRead();

  if (bytes_wanna_read == 0)
    return;

  if (bytes_wanna_read == size_t(-1)) {
    ++ num_handlers_;
    this->socket_.async_read_some(boost::asio::buffer(&this->buffer_receiving_[0], 
                                                      this->buffer_receiving_.capacity()),
                                  boost::bind(&TCPSession::HandleRead, this,
                                              _1,
                                              _2));
  } else {
    this->buffer_receiving_.resize(bytes_wanna_read);
    ++ num_handlers_;
    boost::asio::async_read(this->socket_,
                            boost::asio::buffer(&this->buffer_receiving_[0], bytes_wanna_read),
                            boost::bind(&TCPSession::HandleRead, this,
                                        _1,
                                        _2));
  }
}

void TCPSession::HandleWrite(const boost::system::error_code& error,
                             size_t bytes_transferred) {
  -- num_handlers_;
  assert(num_handlers_ >= 0);

  if (error) {
    if (closed_) {
      if (num_handlers_ == 0)
        HandleClose();
      // else do nothing
    } else {
      Close();
    }
    return;
  }

  this->buffer_sending_.clear();

  if (this->buffer_to_be_sent_.empty()) {
    size_t bytes_wanna_write 
        = this->filter_->BytesWannaWrite(this->messages_to_be_sent_);

    if (bytes_wanna_write == 0)
      return;

    this->buffer_to_be_sent_.reserve(this->buffer_to_be_sent_.size() 
                                     + bytes_wanna_write);

    this->filter_->Write(this->messages_to_be_sent_, 
                         this->buffer_to_be_sent_);
  }

  this->buffer_sending_.swap(this->buffer_to_be_sent_);
  ++ num_handlers_;
  boost::asio::async_write(this->socket_, 
                           boost::asio::buffer(&this->buffer_sending_[0], 
                                               this->buffer_sending_.size()),
                           boost::bind(&TCPSession::HandleWrite, this,
                                       _1,
                                       _2));
}

void TCPSession::HandleClose() {
  assert(id_ != kInvalidTCPSessionID);
  thread_.PostCommandFromMe(TCPIOThreadManager::kMainThreadID, 
                            boost::bind(&TCPIOThreadManager::OnSessionClose,
                                        &thread_.manager(),
                                        id_));
  thread_.session_queue().Remove(id_);
}

void TCPSession::Close() {
  if (closed_)
    return;

  boost::system::error_code ec;
  socket_.shutdown(boost::asio::ip::tcp::socket::shutdown_both, ec);
  socket_.close();
  closed_ = true;

  if (num_handlers_ == 0)
    HandleClose();
}

} // namespace
