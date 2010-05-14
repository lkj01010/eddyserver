##########################################################################
# Basic Usage:                                                           #
#                                                                        #
#   On Unix variants:                                                    #
#     ccmake EDDY_DIRECTORY	                                             #
#                                                                        #
#     (c)onfigure options to your liking, then (g)enerate                #
#     makefiles. Use "make" to build, "make test" to test, "make         #
#     install" to install, and "make package" to build binary            #
#     packages.                                                          #
#                                                                        #
#   On Windows:                                                          #
#     run the CMake GUI, load the eddy directory, and generate			 #
#     project files or makefiles for your environment.                   #
#                                                                        #
# For more information about CMake, see http://www.cmake.org             #
##########################################################################
cmake_minimum_required(VERSION 2.6.4 FATAL_ERROR)

project(eddy)

#diagnosing info
message("c++ compiler ... " ${CMAKE_CXX_COMPILER})

#output path
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY "${PROJECT_BINARY_DIR}/lib")
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY "${PROJECT_BINARY_DIR}/lib")
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY "${PROJECT_BINARY_DIR}/bin")

#build type
if(NOT CMAKE_BUILD_TYPE)
	message("build type not set")
	set(CMAKE_BUILD_TYPE "Debug")
endif(NOT CMAKE_BUILD_TYPE)
message("build type ... " ${CMAKE_BUILD_TYPE})

#default build shared libs
set(BUILD_SHARED_LIBS true)

#compile options
if(CMAKE_COMPILER_IS_GNUCXX)
	set(EDDY_FLAGS_COMMON "-std=c++0x -Wall -Werror -Wno-deprecated")
	set(EDDY_FLAGS_DEBUG "${EDDY_FLAGS_COMMON} -O0 -ggdb -rdynamic")
	set(EDDY_FLAGS_RELWITHDEBINFO "${EDDY_FLAGS_COMMON} -O3 -ggdb -rdynamic")
	set(EDDY_FLAGS_RELEASE "${EDDY_FLAGS_COMMON} -O3")
	set(CMAKE_CXX_FLAGS_DEBUG ${EDDY_FLAGS_DEBUG})
	set(CMAKE_C_FLAGS_DEBUG ${EDDY_FLAGS_DEBUG})
	set(CMAKE_CXX_FLAGS_RELWITHDEBINFO ${EDDY_FLAGS_RELWITHDEBINFO})
	set(CMAKE_C_FLAGS_RELWITHDEBINFO ${EDDY_FLAGS_RELWITHDEBINFO})
	set(CMAKE_CXX_FLAGS_RELEASE ${EDDY_FLAGS_RELEASE})
	set(CMAKE_C_FLAGS_RELEASE ${EDDY_FLAGS_RELEASE})
endif(CMAKE_COMPILER_IS_GNUCXX)

#eddy sdk
include_directories(.)
link_directories(lib)
add_subdirectory(demo)
add_subdirectory(editor)

