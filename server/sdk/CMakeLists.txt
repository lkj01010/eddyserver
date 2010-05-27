aux_source_directory(. EDDY_SDK_SOURCE)
add_library(eddy_sdk ${EDDY_SDK_SOURCE})

set(BOOST_DEBUG_LIB boost_thread boost_system)
set(BOOST_RELEASE_LIB boost_thread boost_system)

if(CMAKE_BUILD_TYPE STREQUAL "Release")
message("eddy sdk linking ..." ${BOOST_RELEASE_LIB})
target_link_libraries(eddy_sdk ${BOOST_RELEASE_LIB})
elseif(CMAKE_BUILD_TYPE STREQUAL "Debug")
message("eddy sdk linking ..." ${BOOST_DEBUG_LIB})
target_link_libraries(eddy_sdk ${BOOST_DEBUG_LIB})
endif(CMAKE_BUILD_TYPE STREQUAL "Release")

target_link_libraries(eddy_sdk tcmalloc)

message("------------------------------------------")

