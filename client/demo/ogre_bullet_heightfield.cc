#include "ogre_bullet_heightfield.h"
OgreBulletHeightfield::OgreBulletHeightfield(int heightStickWidth,int heightStickLength,
											 void* heightfieldData, btScalar heightScale,
											 btScalar minHeight, btScalar maxHeight,
											 int upAxis, PHY_ScalarType heightDataType,
											 bool flipQuadEdges) : btHeightfieldTerrainShape(heightStickWidth,
											 heightStickLength, heightfieldData, 
											 heightScale, minHeight, maxHeight, 
											 upAxis, heightDataType, flipQuadEdges) {
}

void	OgreBulletHeightfield::processAllTriangles(btTriangleCallback* callback,const btVector3& aabbMin,const btVector3& aabbMax) const {
	// scale down the input aabb's so they are in local (non-scaled) coordinates
	btVector3	localAabbMin = aabbMin*btVector3(1.f/m_localScaling[0],1.f/m_localScaling[1],1.f/m_localScaling[2]);
	btVector3	localAabbMax = aabbMax*btVector3(1.f/m_localScaling[0],1.f/m_localScaling[1],1.f/m_localScaling[2]);

	// account for local origin
	localAabbMin += m_localOrigin;
	localAabbMax += m_localOrigin;

	//quantize the aabbMin and aabbMax, and adjust the start/end ranges
	int	quantizedAabbMin[3];
	int	quantizedAabbMax[3];
	quantizeWithClamp(quantizedAabbMin, localAabbMin,0);
	quantizeWithClamp(quantizedAabbMax, localAabbMax,1);

	// expand the min/max quantized values
	// this is to catch the case where the input aabb falls between grid points!
	for (int i = 0; i < 3; ++i) {
		quantizedAabbMin[i]--;
		quantizedAabbMax[i]++;
	}	

	int startX=0;
	int endX=m_heightStickWidth-1;
	int startJ=0;
	int endJ=m_heightStickLength-1;

	switch (m_upAxis)
	{
	case 0:
		{
			if (quantizedAabbMin[1]>startX)
				startX = quantizedAabbMin[1];
			if (quantizedAabbMax[1]<endX)
				endX = quantizedAabbMax[1];
			if (quantizedAabbMin[2]>startJ)
				startJ = quantizedAabbMin[2];
			if (quantizedAabbMax[2]<endJ)
				endJ = quantizedAabbMax[2];
			break;
		}
	case 1:
		{
			if (quantizedAabbMin[0]>startX)
				startX = quantizedAabbMin[0];
			if (quantizedAabbMax[0]<endX)
				endX = quantizedAabbMax[0];
			if (quantizedAabbMin[2]>startJ)
				startJ = quantizedAabbMin[2];
			if (quantizedAabbMax[2]<endJ)
				endJ = quantizedAabbMax[2];
			break;
		};
	case 2:
		{
			if (quantizedAabbMin[0]>startX)
				startX = quantizedAabbMin[0];
			if (quantizedAabbMax[0]<endX)
				endX = quantizedAabbMax[0];
			if (quantizedAabbMin[1]>startJ)
				startJ = quantizedAabbMin[1];
			if (quantizedAabbMax[1]<endJ)
				endJ = quantizedAabbMax[1];
			break;
		}
	default:
		{
			//need to get valid m_upAxis
			btAssert(0);
		}
	}

	for(int j=startJ; j<endJ; j++)
	{
		for(int x=startX; x<endX; x++)
		{
			btVector3 vertices[3];
			if (j % 2 == 1)
			{
				//first triangle
				getVertex(x,j,vertices[0]);
				getVertex(x+1,j,vertices[1]);
				getVertex(x,j+1,vertices[2]);
				callback->processTriangle(vertices,x,j);
				//second triangle
				getVertex(x+1,j,vertices[0]);
				getVertex(x+1,j+1,vertices[1]);
				getVertex(x,j+1,vertices[2]);
				callback->processTriangle(vertices,x,j);				
			} else
			{
				//first triangle
				getVertex(x,j,vertices[0]);
				getVertex(x+1,j+1,vertices[1]);
				getVertex(x,j+1,vertices[2]);
				callback->processTriangle(vertices,x,j);
				//second triangle
				getVertex(x,j,vertices[0]);
				getVertex(x+1,j,vertices[1]);
				getVertex(x+1,j+1,vertices[2]);
				callback->processTriangle(vertices,x,j);
			}
		}
	}

}

btScalar
OgreBulletHeightfield::getRawHeightFieldValue(int x,int y) const
{
	y = m_heightStickLength - y - 1;
	return btHeightfieldTerrainShape::getRawHeightFieldValue(x, y);
}