#ifndef _OGRE_BULLET_HEIGHTFIELD_H_
#define _OGRE_BULLET_HEIGHTFIELD_H_

#include "BulletCollision/CollisionShapes/btHeightfieldTerrainShape.h"

class OgreBulletHeightfield : public btHeightfieldTerrainShape {
public:
		OgreBulletHeightfield(int heightStickWidth,int heightStickLength,
	                          void* heightfieldData, btScalar heightScale,
	                          btScalar minHeight, btScalar maxHeight,
	                          int upAxis, PHY_ScalarType heightDataType,
	                          bool flipQuadEdges);

		virtual void	processAllTriangles(btTriangleCallback* callback,const btVector3& aabbMin,const btVector3& aabbMax) const;

		virtual btScalar	getRawHeightFieldValue(int x,int y) const;
};

#endif