// =====================================================================================
// 
//       Filename:  net_message.cc
// 
//    Description:  NetMessage
// 
//        Version:  1.0
//        Created:  2010-05-13 17:23:46
//       Revision:  none
//       Compiler:  g++
// 
//         Author:  liaoxinwei (Comet), cometliao@gmail.com
//        Company:  eddy
// 
// =====================================================================================

#include    <boost/make_shared.hpp>

#include    <sdk/net_message.h>

namespace eddy {

void NetMessage::Swap(NetMessage& other) {
  if (this->is_dynamic()) {
    if (other.is_dynamic()) {
      std::swap(this->dynamic_data_, other.dynamic_data_);
    } else {
      std::swap(this->dynamic_data_, other.dynamic_data_);
      std::copy(other.static_data_, 
                other.static_data_ + other.static_size_, 
                this->static_data_);
      std::swap(this->static_size_, other.static_size_);
    }
  } else {
    if (other.is_dynamic()) {
      std::swap(this->dynamic_data_, other.dynamic_data_);
      std::copy(this->static_data_, 
                this->static_data_ + this->static_size_, 
                other.static_data_);
      std::swap(this->static_size_, other.static_size_);
    } else {
      char temp_data_[kDynamicThreshold];
      std::copy(this->static_data_, 
                this->static_data_ + this->static_size_, 
                temp_data_);
      std::copy(other.static_data_, 
                other.static_data_ + other.static_size_, 
                this->static_data_);
      std::copy(temp_data_, 
                temp_data_ + this->static_size_, 
                other.static_data_);
      std::swap(this->static_size_, other.static_size_);
    }
  }
}


void NetMessage::Reserve(size_t size) {
  if (this->is_dynamic()) {
    dynamic_data_->reserve(size);
  } else {
    if (size < kDynamicThreshold)
      return;
    else
      this->SetDynamic();
  }
}


void NetMessage::SetDynamic() {
  if (this->is_dynamic())
    return;

  dynamic_data_ = boost::make_shared<DynamicVector>();
  dynamic_data_->insert(dynamic_data_->end(), 
                        static_data_, 
                        static_data_ + static_size_);
}


NetMessage& NetMessage::operator = (const NetMessage& other) {
  if (this == &other)
    return *this;

  if (other.is_dynamic()) {
    this->dynamic_data_ = other.dynamic_data_;
  } else {
    this->dynamic_data_->clear();
    this->static_size_ = other.static_size_;
    std::copy(other.static_data_, 
              other.static_data_ + other.static_size_, 
              this->static_data_);
  }

  return *this;
}


std::streamsize NetMessage::Write(const char_type* data, std::streamsize n) {
  if (!this->is_dynamic()) {
    if (static_size_ + n <= kDynamicThreshold) {  // static alloc
      std::copy(data, data + n, static_data_ + static_size_);
      static_size_ += n;
      return n;
    } else {
      this->SetDynamic();
    }
  }

  if (dynamic_data_.unique()) {
    dynamic_data_->insert(dynamic_data_->end(), data, data + n);
  } else { // copy on write
    boost::shared_ptr<DynamicVector> temp(dynamic_data_);
    dynamic_data_   = boost::make_shared<DynamicVector>();
    *dynamic_data_  = *temp; 
    dynamic_data_->insert(dynamic_data_->end(), data, data + n);
  }

  return n;
}

} // --- end of eddy