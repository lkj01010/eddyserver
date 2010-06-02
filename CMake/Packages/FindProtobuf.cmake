#-------------------------------------------------------------------
# This file is part of the CMake build system for EDDY
#     (Object-oriented Graphics Rendering Engine)
# For the latest info, see http://www.EDDY3d.org/
#
# The contents of this file are placed in the public domain. Feel
# free to make use of it in any way you like.
#-------------------------------------------------------------------

include(FindPkgMacros)
findpkg_begin(Protobuf)

# Get path, convert backslashes as ${ENV_${var}}
getenv_path(Protobuf_HOME)
getenv_path(EDDY_SDK)
getenv_path(EDDY_HOME)
getenv_path(EDDY_SOURCE)

# construct search paths
set(Protobuf_PREFIX_PATH 
  ${EDDY_SOURCE}/dependencies ${ENV_EDDY_SOURCE}/dependencies
  ${EDDY_SDK} ${ENV_EDDY_SDK}
  ${EDDY_HOME} ${ENV_EDDY_HOME})

if (UNIX)
	set(Protobuf_PREFIX_PATH ${Protobuf_PREFIX_PATH} "/usr" "/usr/local")
endif (UNIX)

create_search_paths(Protobuf)
# redo search if prefix path changed
clear_if_changed(Protobuf_PREFIX_PATH
	Protobuf_LIBRARY_FWK
	Protobuf_LIBRARY_REL
	Protobuf_LIBRARY_DBG
	Protobuf_INCLUDE_DIR
	)

if (WIN32)
	set(Protobuf_LIBRARY_NAMES libprotobuf)
else (WIN32)
	set(Protobuf_LIBRARY_NAMES protobuf)
endif (WIN32)

get_debug_names(Protobuf_LIBRARY_NAMES)

use_pkgconfig(Protobuf_PKGC Protobuf)

# For Protobuf, prefer static library over framework (important when referencing Protobuf source build)
set(CMAKE_FIND_FRAMEWORK "LAST")

findpkg_framework(Protobuf)

find_path(Protobuf_INCLUDE_DIR NAMES "google/protobuf/dynamic_message.h" "google/protobuf/wire_format_lite.h" HINTS ${Protobuf_INC_SEARCH_PATH} ${Protobuf_PKGC_INCLUDE_DIRS} PATH_SUFFIXES "")
find_library(Protobuf_LIBRARY_REL NAMES ${Protobuf_LIBRARY_NAMES} HINTS ${Protobuf_LIB_SEARCH_PATH} ${Protobuf_PKGC_LIBRARY_DIRS} PATH_SUFFIXES "" release relwithdebinfo minsizerel)
if (WIN32)
	find_library(Protobuf_LIBRARY_DBG NAMES ${Protobuf_LIBRARY_NAMES_DBG} HINTS ${Protobuf_LIB_SEARCH_PATH} ${Protobuf_PKGC_LIBRARY_DIRS} PATH_SUFFIXES "" debug)
endif (WIN32)
make_library_set(Protobuf_LIBRARY)

findpkg_finish(Protobuf)

# protoc
findpkg_begin(Protoc)

# Get path, convert backslashes as ${ENV_${var}}
getenv_path(Protoc_HOME)
getenv_path(EDDY_SDK)
getenv_path(EDDY_HOME)
getenv_path(EDDY_SOURCE)

# construct search paths
set(Protoc_PREFIX_PATH 
	${EDDY_SOURCE}/dependencies ${ENV_EDDY_SOURCE}/dependencies
	${EDDY_SDK} ${ENV_EDDY_SDK}
	${EDDY_HOME} ${ENV_EDDY_HOME})

if (UNIX)
	set(Protoc_PREFIX_PATH ${Protoc_PREFIX_PATH} "/usr" "/usr/local")
endif (UNIX)

create_search_paths(Protoc)
# redo search if prefix path changed
clear_if_changed(Protoc_PREFIX_PATH
	Protoc_LIBRARY_FWK
	Protoc_LIBRARY_REL
	Protoc_LIBRARY_DBG
	Protoc_INCLUDE_DIR
	)

if (WIN32)
	set(Protoc_LIBRARY_NAMES libprotoc)
else (WIN32)
	set(Protoc_LIBRARY_NAMES protoc)
endif (WIN32)
get_debug_names(Protoc_LIBRARY_NAMES)

use_pkgconfig(Protoc_PKGC Protoc)

#findpkg_framework(Protoc)

find_path(Protoc_INCLUDE_DIR NAMES "google/protobuf/dynamic_message.h" "google/protobuf/wire_format_lite.h" HINTS ${Protoc_INC_SEARCH_PATH} ${Protoc_PKGC_INCLUDE_DIRS} PATH_SUFFIXES "")
find_library(Protoc_LIBRARY_REL NAMES ${Protoc_LIBRARY_NAMES} HINTS ${Protoc_LIB_SEARCH_PATH} ${Protoc_PKGC_LIBRARY_DIRS} PATH_SUFFIXES "" release relwithdebinfo minsizerel)
if (WIN32)
	find_library(Protoc_LIBRARY_DBG NAMES ${Protoc_LIBRARY_NAMES_DBG} HINTS ${Protoc_LIB_SEARCH_PATH} ${Protoc_PKGC_LIBRARY_DIRS} PATH_SUFFIXES "" debug)
endif (WIN32)
make_library_set(Protoc_LIBRARY)

findpkg_finish(Protoc)
# Reset framework finding
set(CMAKE_FIND_FRAMEWORK "FIRST")
