#ifndef __Terrain_H__
#define __Terrain_H__

//#define PAGING
#include "OIS/OIS.h"

#include "OGRE/OgreRenderSystemCapabilities.h"
#include "OGRE/OgreFrameListener.h"
#include "OGRE/OgreShadowCameraSetup.h"

#include "OGRE/Terrain/OgreTerrain.h"
#include "OGRE/Terrain/OgreTerrainGroup.h"
#include "OGRE/Terrain/OgreTerrainQuadTreeNode.h"
#include "OGRE/Terrain/OgreTerrainMaterialGeneratorA.h"

class SceneTerrain
{
public:

	SceneTerrain();

	void testCapabilities(const Ogre::RenderSystemCapabilities* caps);
	
	bool frameRenderingQueued(const Ogre::FrameEvent& evt);

	void saveTerrains(bool onlyIfModified);

	bool keyPressed (const OIS::KeyEvent &e);
	
	void setupContent(Ogre::SceneManager* scene_manager, Ogre::Light* l, Ogre::Camera* camera);

	const Ogre::Vector3& terrain_pos() { return terrain_pos_; }

	Ogre::TerrainGroup::RayResult findIntersect(const Ogre::Ray& ray);
	
	void shutdown();

protected:

	Ogre::TerrainGlobalOptions* terrain_globals_;
	Ogre::TerrainGroup* terrain_group_;
	Ogre::Vector3 terrain_pos_;

	Ogre::ShadowCameraSetupPtr pssm_setup_;

	void defineTerrain(long x, long y, bool flat = false);

	void getTerrainImage(bool flipX, bool flipY, Ogre::Image& img);

	void initBlendMaps(Ogre::Terrain* terrain);

	void configureTerrainDefaults(Ogre::SceneManager* scene_manager, Ogre::Light* l);
		
	Ogre::MaterialPtr buildDepthShadowMaterial(const Ogre::String& textureName);

	void configureShadows(bool enabled, bool depthShadows);

};

#endif
