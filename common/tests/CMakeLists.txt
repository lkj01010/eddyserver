set(EDDYSDK eddy_sdk)

add_executable(servertest servertest.cc)
target_link_libraries(servertest ${EDDYSDK}) 

add_executable(clienttest clienttest.cc)
target_link_libraries(clienttest ${EDDYSDK})

add_executable(boosttest boosttest.cc)
target_link_libraries(boosttest ${EDDYSDK})

add_executable(refservertest refservertest.cc)
target_link_libraries(refservertest ${EDDYSDK}) 

add_executable(refclienttest refclienttest.cc)
target_link_libraries(refclienttest ${EDDYSDK})

add_custom_command(OUTPUT messagetest.pb.h messagetest.pb.cc
	COMMAND protoc --cpp_out=./ messagetest.proto
	DEPENDS messagetest.proto)

add_executable(messagetest messagetest.cc messagetest.pb.cc)
target_link_libraries(messagetest ${EDDYSDK} ${Protoc_LIBRARIES} ${Protobuf_LIBRARIES})
if (linux)
target_link_libraries(messagetest pthread)
endif (linux)

add_custom_command(OUTPUT proto.pb.h proto.pb.cc
	COMMAND protoc --cpp_out=./ proto.proto
	DEPENDS proto.proto)

add_executable(protoserver protoserver.cc proto.pb.cc)
target_link_libraries(protoserver ${EDDYSDK} ${Protoc_LIBRARIES} ${Protobuf_LIBRARIES})
if (linux)
target_link_libraries(protoserver pthread)
endif (linux)

add_executable(protoclient protoclient.cc proto.pb.cc)
target_link_libraries(protoclient ${EDDYSDK} ${Protoc_LIBRARIES} ${Protobuf_LIBRARIES})
if (linux)
target_link_libraries(protoclient pthread)
endif (linux)
