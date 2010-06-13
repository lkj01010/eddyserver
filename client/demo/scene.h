#ifndef __Scene_H__
#define __Scene_H__

//#define PAGING
#include "boost/shared_ptr.hpp"
#include "OIS/OIS.h"

#include "OGRE/OgreRenderSystemCapabilities.h"
#include "OGRE/OgreFrameListener.h"
#include "OGRE/OgreShadowCameraSetup.h"

#include "camera_controller.h"

namespace Ogre {
	class Terrain;
	class TerrainGlobalOptions;
	class TerrainGroup;
	class SceneManager;
	class Camera;
}

class Character;
class OgreBulletHeightfield;

class btBroadphaseInterface;
class btCollisionDispatcher;
class btConstraintSolver;
class btDynamicsWorld;
class btDefaultCollisionConfiguration;

class Scene
{
public:

	Scene();

	void testCapabilities(const Ogre::RenderSystemCapabilities* caps);
	
	void saveTerrains(bool onlyIfModified);

	void setupContent(Ogre::Root* root);

	const Ogre::Vector3& terrain_pos() { return terrain_pos_; }
	
	void shutdown(Ogre::Root* root);

	bool keyPressed(const OIS::KeyEvent& evt);

	bool keyReleased(const OIS::KeyEvent& evt);

	bool mousePressed(const OIS::MouseEvent& evt, OIS::MouseButtonID id);

	bool mouseReleased(const OIS::MouseEvent& evt, OIS::MouseButtonID id);

	bool mouseMoved(const OIS::MouseEvent& evt);

	bool frameRenderingQueued(const Ogre::FrameEvent& evt);

	Ogre::Camera* camera() const { return camera_; }

	Ogre::SceneManager* scene_manager() const { return scene_manager_; }

	Ogre::TerrainGroup* terrain_group() const { return terrain_group_; }

	btDynamicsWorld* dynamic_world() const { return dynamic_world_.get(); }
private:
	void initPhysics();

	Ogre::TerrainGlobalOptions*		terrain_globals_;
	Ogre::TerrainGroup*					terrain_group_;
	Ogre::Vector3							terrain_pos_;
	Ogre::SceneManager*				scene_manager_;
	Ogre::Camera*							camera_;
	CameraController*					camera_controller_;
	Character*								character_;
	Ogre::ShadowCameraSetupPtr pssm_setup_;

	boost::shared_ptr<OgreBulletHeightfield>	physics_terrain_;
	boost::shared_ptr<btBroadphaseInterface>	overlapping_pair_cache_;
	boost::shared_ptr<btCollisionDispatcher>		dispatcher_;
	boost::shared_ptr<btConstraintSolver>			constraint_solver_;
	boost::shared_ptr<btDynamicsWorld>			dynamic_world_;
	boost::shared_ptr<btDefaultCollisionConfiguration> collision_configuration_;

	void defineTerrain(long x, long y, bool flat = false);

	void getTerrainImage(bool flipX, bool flipY, Ogre::Image& img);

	void initBlendMaps(Ogre::Terrain* terrain);

	void configureTerrainDefaults(Ogre::SceneManager* scene_manager, Ogre::Light* l);
};

#endif
