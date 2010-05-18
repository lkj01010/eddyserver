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

add_custom_command(OUTPUT messagetest.pb.h messagetest.pb.cc
	COMMAND protoc --cpp_out=./ messagetest.proto
	DEPENDS messagetest.proto)

add_executable(messagetest messagetest.cc messagetest.pb.cc)
target_link_libraries(messagetest eddy_sdk protoc pthread)

