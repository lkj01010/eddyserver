set(EDDY_DEPENDENCIES_DIR "" CACHE PATH "Path to prebuilt EDDY dependencies")
include(FindPkgMacros)
include(MacroLogFeature)
getenv_path(EDDY_DEPENDENCIES_DIR)

  set(EDDY_DEP_SEARCH_PATH 
    ${EDDY_DEPENDENCIES_DIR}
    ${ENV_EDDY_DEPENDENCIES_DIR}
    "${EDDY_BINARY_DIR}/dependencies"
    "${EDDY_SOURCE_DIR}/dependencies"
    "${EDDY_BINARY_DIR}/../dependencies"
    "${EDDY_SOURCE_DIR}/../dependencies"
  )

message(STATUS "Search path: ${EDDY_DEP_SEARCH_PATH}, ${CMAKE_LIBRARY_PATH}" )

# Set hardcoded path guesses for various platforms
if (UNIX)
  set(EDDY_DEP_SEARCH_PATH ${EDDY_DEP_SEARCH_PATH} /usr/local)
endif ()

# give guesses as hints to the find_package calls
set(CMAKE_PREFIX_PATH ${EDDY_DEP_SEARCH_PATH} ${CMAKE_PREFIX_PATH})
set(CMAKE_FRAMEWORK_PATH ${EDDY_DEP_SEARCH_PATH} ${CMAKE_FRAMEWORK_PATH})

#######################################################################
# Core dependencies
#######################################################################
#boost
set(Boost_USE_STATIC_LIBS TRUE)
set(Boost_ADDITIONAL_VERSIONS "1.42" "1.42.0" "1.43" "1.43.0")

if (UNIX)
# Components that need linking (NB does not include header-only components like bind)
set(EDDY_BOOST_COMPONENTS thread date_time system)
find_package(Boost COMPONENTS ${EDDY_BOOST_COMPONENTS} QUIET)
if (NOT Boost_FOUND)
	# Try again with the other type of libs
	set(Boost_USE_STATIC_LIBS NOT ${Boost_USE_STATIC_LIBS})
	find_package(Boost COMPONENTS ${EDDY_BOOST_COMPONENTS} QUIET)
endif()
macro_log_feature(Boost_THREAD_FOUND "boost-thread" "Used for threading support" "http://boost.org" FALSE "" "")
macro_log_feature(Boost_DATE_TIME_FOUND "boost-date_time" "Used for threading support" "http://boost.org" FALSE "" "")
endif (UNIX)

find_package(Boost REQUIRED)
macro_log_feature(Boost_FOUND "boost" "Boost (general)" "http://boost.org" FALSE "" "")

find_package(Protobuf)
if (NOT PROTOBUF_FOUND)
	if (WIN32)
		set(PROTOBUF_LIBRARY_NAMES "libprotobuf")
	else (WIN32)
		set(PROTOBUF_LIBRARY_NAMES "protobuf")
	endif (WIN32)
	find_path(PROTOBUF_INCLUDE_DIR NAMES "google/protobuf/dynamic_message.h" "google/protobuf/wire_format_lite.h" HINTS "${EDDY_SOURCE_DIR}/dependencies/include" PATH_SUFFIXES "")
	find_library(PROTOBUF_LIBRARY_REL NAMES ${PROTOBUF_LIBRARY_NAMES} HINTS ${EDDY_SOURCE_DIR}/dependencies PATH_SUFFIXES "" release relwithdebinfo minsizerel)
	find_library(PROTOBUF_LIBRARY_DBG NAMES ${PROTOBUF_LIBRARY_NAMES} HINTS ${EDDY_SOURCE_DIR}/dependencies PATH_SUFFIXES "" debug)
	make_library_set(PROTOBUF_LIBRARY)
	findpkg_finish(PROTOBUF)
	
	if (WIN32)
		set(PROTOBUF_PROTOC_LIBRARY_NAMES "libprotoc")
	else (WIN32)
		set(PROTOBUF_PROTOC_LIBRARY_NAMES "protoc")
	endif (WIN32)
	find_path(PROTOBUF_PROTOC_INCLUDE_DIR NAMES "google/protobuf/dynamic_message.h" "google/protobuf/wire_format_lite.h" HINTS "${EDDY_SOURCE_DIR}/dependencies/include" PATH_SUFFIXES "")
	find_library(PROTOBUF_PROTOC_LIBRARY_REL NAMES ${PROTOBUF_PROTOC_LIBRARY_NAMES} HINTS ${EDDY_SOURCE_DIR}/dependencies PATH_SUFFIXES "" release relwithdebinfo minsizerel)
	find_library(PROTOBUF_PROTOC_LIBRARY_DBG NAMES ${PROTOBUF_PROTOC_LIBRARY_NAMES} HINTS ${EDDY_SOURCE_DIR}/dependencies PATH_SUFFIXES "" debug)
	make_library_set(PROTOBUF_PROTOC_LIBRARY)
	findpkg_finish(PROTOBUF_PROTOC)
endif (NOT PROTOBUF_FOUND)

macro_log_feature(PROTOBUF_FOUND "protobuf" "Protobuf (general)" "http://code.google.com/p/protobuf/" FALSE "" "")

find_package(Bullet)
if (NOT BULLET_FOUND)
	find_path(BULLET_INCLUDE_DIR NAMES "btBulletCollisionCommon.h" HINTS "${EDDY_SOURCE_DIR}/dependencies/include" PATH_SUFFIXES "")
	find_library(BULLET_COLLISION_LIBRARY_REL NAMES "BulletCollision" HINTS ${EDDY_SOURCE_DIR}/dependencies PATH_SUFFIXES "" release relwithdebinfo minsizerel)
	find_library(BULLET_COLLISION_LIBRARY_DBG NAMES "BulletCollision" HINTS ${EDDY_SOURCE_DIR}/dependencies PATH_SUFFIXES "" debug)
	
	find_library(BULLET_DYNAMICS_LIBRARY_REL NAMES "BulletDynamics" HINTS ${EDDY_SOURCE_DIR}/dependencies PATH_SUFFIXES "" release relwithdebinfo minsizerel)
	find_library(BULLET_DYNAMICS_LIBRARY_DBG NAMES "BulletDynamics" HINTS ${EDDY_SOURCE_DIR}/dependencies PATH_SUFFIXES "" debug)
	
	find_library(BULLET_SOFTBODY_LIBRARY_REL NAMES "BulletSoftBody" HINTS ${EDDY_SOURCE_DIR}/dependencies PATH_SUFFIXES "" release relwithdebinfo minsizerel)
	find_library(BULLET_SOFTBODY_LIBRARY_DBG NAMES "BulletSoftBody" HINTS ${EDDY_SOURCE_DIR}/dependencies PATH_SUFFIXES "" debug)
	
	find_library(LINEAR_MATH_LIBRARY_REL NAMES "LinearMath" HINTS ${EDDY_SOURCE_DIR}/dependencies PATH_SUFFIXES "" release relwithdebinfo minsizerel)
	find_library(LINEAR_MATH_LIBRARY_DBG NAMES "LinearMath" HINTS ${EDDY_SOURCE_DIR}/dependencies PATH_SUFFIXES "" debug)

	set(BULLET_LIBRARIES_REL 
	${BULLET_COLLISION_LIBRARY_REL}
	${BULLET_DYNAMICS_LIBRARY_REL}
	${BULLET_SOFTBODY_LIBRARY_REL}
	${LINEAR_MATH_LIBRARY_REL})
	
	set(BULLET_LIBRARIES_DBG 
	${BULLET_COLLISION_LIBRARY_DBG}
	${BULLET_DYNAMICS_LIBRARY_DBG}
	${BULLET_SOFTBODY_LIBRARY_DBG}
	${LINEAR_MATH_LIBRARY_DBG})

	make_library_set(BULLET_LIBRARIES)
	findpkg_finish(BULLET)
endif (BULLET_FOUND)

macro_log_feature(BULLET_FOUND "bullet" "bullet (general)" "http://www.bulletphysics.org/" FALSE "" "")

if (EDDY_BUILD_CLIENT)
find_package(OGRE REQUIRED)
macro_log_feature(OGRE_FOUND "OGRE" "OGRE (general)" "http://www.ogre3d.org/" FALSE "" "") 

find_package(OIS REQUIRED)
macro_log_feature(OIS_FOUND "OIS" "Input library needed for the samples" "http://sourceforge.net/projects/wgois" FALSE "" "") 
endif (EDDY_BUILD_CLIENT)

# Display results, terminate if anything required is missing
MACRO_DISPLAY_FEATURE_LOG()

# Add library and include paths from the dependencies
include_directories(
  ${PROTOBUF_INCLUDE_DIRS}
  ${Boost_INCLUDE_DIRS}
)

link_directories(
  ${Boost_LIBRARY_DIRS}
)

if (EDDY_BUILD_CLIENT)
include_directories(
  ${OIS_INCLUDE_DIRS}
  ${OGRE_INCLUDE_DIRS}
)
link_directories(
  ${OIS_LIBRARY_DIRS}
  ${OGRE_LIBRARY_DIRS}
)
endif (EDDY_BUILD_CLIENT)