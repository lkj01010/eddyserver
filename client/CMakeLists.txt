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
#eddy sdk
add_executable(editor 
	editor/editor.cc 
	editor/sdk_camera_man.h 
	editor/sdk_trays.h)
target_link_libraries(editor ${OGRE_LIBRARY} ${OIS_LIBRARY}) 

add_executable(ogretest 
	demo/camera_controller.h 
	demo/camera_controller.cc 
	demo/scene_terrain.h
	demo/scene_terrain.cc
	demo/ogretest.cc)
target_link_libraries(ogretest ${OGRE_Terrain_LIBRARY} ${OGRE_LIBRARY} ${OIS_LIBRARY})
