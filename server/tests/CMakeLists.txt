add_executable(servertest servertest.cc)
target_link_libraries(servertest eddy_sdk) 

add_executable(clienttest clienttest.cc)
target_link_libraries(clienttest eddy_sdk)

add_executable(boosttest boosttest.cc)
target_link_libraries(boosttest eddy_sdk)

add_executable(refservertest refservertest.cc)
target_link_libraries(refservertest eddy_sdk) 

add_executable(refclienttest refclienttest.cc)
target_link_libraries(refclienttest eddy_sdk)

