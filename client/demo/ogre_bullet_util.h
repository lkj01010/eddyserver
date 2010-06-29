#ifndef _OGRE_BULLET_UTIL_H_
#define _OGRE_BULLET_UTIL_H_

#include "OGRE/OgreVector3.h"
#include "LinearMath/btVector3.h"

Ogre::Vector3 toOgreVector(const btVector3& vec) {
	return Ogre::Vector3(vec[0], vec[1], vec[2]);
}

btVector3 toBulletVector(const Ogre::Vector3& vec) {
	return btVector3(vec.x, vec.y, vec.z);
}

#endif