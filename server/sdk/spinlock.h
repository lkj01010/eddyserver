// =====================================================================================
// 
//       Filename:  spinlock.h
// 
//    Description:  spinlock
// 
//        Version:  1.0
//        Created:  2009-12-06 17:40:03
//       Revision:  none
//       Compiler:  g++
// 
//         Author:  liaoxinwei (Comet), cometliao@gmail.com
//        Company:  eddy
// 
// =====================================================================================

#ifndef  SPINLOCK_H_
#define  SPINLOCK_H_

#include    <boost/smart_ptr/detail/spinlock.hpp>

namespace eddy { 
using boost::detail::spinlock;
}

#endif   // ----- #ifndef SPINLOCK_H_  -----
