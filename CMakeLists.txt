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

if(WIN32)
  set(CMAKE_USE_RELATIVE_PATHS true)
  set(CMAKE_SUPPRESS_REGENERATION true)
endif()

project(EDDY)

set(CMAKE_MODULE_PATH 
  "${EDDY_SOURCE_DIR}/CMake"
  "${EDDY_SOURCE_DIR}/CMake/Utils" 
  "${EDDY_SOURCE_DIR}/CMake/Packages"
)

#diagnosing info
message("c++ compiler ... " ${CMAKE_CXX_COMPILER})

#output path
set(EXECUTABLE_OUTPUT_PATH ${PROJECT_BINARY_DIR}/bin)
set(LIBRARY_OUTPUT_PATH ${PROJECT_BINARY_DIR}/lib)

#build type
if (NOT CMAKE_BUILD_TYPE)
  # CMake defaults to leaving CMAKE_BUILD_TYPE empty. This screws up
  # differentiation between debug and release builds.
  set(CMAKE_BUILD_TYPE "Debug" CACHE STRING "Choose the type of build, options are: None (CMAKE_CXX_FLAGS or CMAKE_C_FLAGS used) Debug Release RelWithDebInfo MinSizeRel." FORCE)
endif ()

message("build type ... " ${CMAKE_BUILD_TYPE})

#default shared
#set(BUILD_SHARED_LIBS true)

#find dependencies
include(Dependencies)

#compile options
if (CMAKE_COMPILER_IS_GNUCXX)
	#check_cxx_compiler_flag(-msse EDDY_GCC_HAS_SSE)
	if (EDDY_GCC_HAS_SSE)
		add_definitions(-msse)
	endif ()
	set(EDDY_FLAGS_COMMON "-Wall -Werror -Wno-deprecated")
	set(EDDY_FLAGS_DEBUG "${EDDY_FLAGS_COMMON} -O0 -ggdb -rdynamic")
	set(EDDY_FLAGS_RELWITHDEBINFO "${EDDY_FLAGS_COMMON} -O3 -ggdb -rdynamic")
	set(EDDY_FLAGS_RELEASE "${EDDY_FLAGS_COMMON} -O3")
	set(CMAKE_CXX_FLAGS_DEBUG ${EDDY_FLAGS_DEBUG})
	set(CMAKE_C_FLAGS_DEBUG ${EDDY_FLAGS_DEBUG})
	set(CMAKE_CXX_FLAGS_RELWITHDEBINFO ${EDDY_FLAGS_RELWITHDEBINFO})
	set(CMAKE_C_FLAGS_RELWITHDEBINFO ${EDDY_FLAGS_RELWITHDEBINFO})
	set(CMAKE_CXX_FLAGS_RELEASE ${EDDY_FLAGS_RELEASE})
	set(CMAKE_C_FLAGS_RELEASE ${EDDY_FLAGS_RELEASE})
endif (CMAKE_COMPILER_IS_GNUCXX)
if (MSVC)
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /fp:fast")
  # Enable intrinsics on MSVC in debug mode
  set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} /Oi")
  if (CMAKE_CL_64)
    # Visual Studio bails out on debug builds in 64bit mode unless
	# this flag is set...
	set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} /bigobj")
	set(CMAKE_CXX_FLAGS_RELWITHDEBINFO "${CMAKE_CXX_FLAGS_RELWITHDEBINFO} /bigobj")
  endif (CMAKE_CL_64)
endif (MSVC)

#options
option(EDDY_BUILD_COMMON "Build common libs" TRUE)
option(EDDY_BUILD_CLIENT "Build client" TRUE)

#supress relative path warning
cmake_policy(SET CMP0015 NEW)
link_directories(lib)

if (EDDY_BUILD_COMMON)
	include_directories(common)
	add_subdirectory(common)
endif (EDDY_BUILD_COMMON)

if (EDDY_BUILD_CLIENT)
	add_subdirectory(client)
endif (EDDY_BUILD_CLIENT)
