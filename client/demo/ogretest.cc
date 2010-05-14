// =====================================================================================
// 
//       Filename:  ogretest.cc
// 
//    Description:  test ogre
// 
//        Version:  1.0
//        Created:  2010-01-18 21:51:49
//       Revision:  none
//       Compiler:  g++
// 
//         Author:  liaoxinwei (Comet), cometliao@gmail.com
//        Company:  eddy
// 
// =====================================================================================

#include    <vector>
#include    <iostream>

#include    "OGRE/OgreRoot.h"
#include    "OGRE/OgreFrameListener.h"
#include    "OGRE/OgreWindowEventUtilities.h"
#include    "OGRE/OgreRenderWindow.h"
#include    "OGRE/OgreCamera.h"
#include    "OGRE/OgreConfigFile.h"
#include    "OGRE/OgreEntity.h"
#include    "OGRE/OgreSubEntity.h"
#include    "OGRE/OgreMeshManager.h"
#include    "OGRE/OgreShadowCameraSetupLiSPSM.h"

const size_t kNumModels = 5;

class MyApp : public Ogre::WindowEventListener, public Ogre::FrameListener {
 public:
  MyApp() {
    root_         = NULL;
    sceneManager_ = NULL;
    camera_       = NULL;
    viewport_     = NULL;
    configDir_    = "config/";
    eclapse_time_ = 0.0f;
  }

  void Go () {
    // init root
    root_ = OGRE_NEW Ogre::Root(configDir_ + "plugins.cfg", 
                                configDir_ + "ogre.cfg", 
                                configDir_ + "ogre.log");
    root_->addFrameListener(this);
    
    if (!root_->restoreConfig())
      root_->showConfigDialog();

    // window
    root_->initialise(true, "Eddy Demo");
    Ogre::WindowEventUtilities::addWindowEventListener(root_->getAutoCreatedWindow(), this);

    // resources
    LoadResources();
    Ogre::TextureManager::getSingleton().setDefaultNumMipmaps(5);

    // scene manager
    sceneManager_ = root_->createSceneManager(Ogre::ST_GENERIC);
    sceneManager_->setShadowCameraSetup(Ogre::ShadowCameraSetupPtr(new Ogre::LiSPSMShadowCameraSetup));
    sceneManager_->setShadowTexturePixelFormat(Ogre::PF_FLOAT16_R);
    //sceneManager_->setShadowTextureCasterMaterial(CUSTOM_CASTER_MATERIAL);
    //sceneManager_->setShadowTextureReceiverMaterial(CUSTOM_RECEIVER_MATERIAL);
    //sceneManager_->setShadowTextureSelfShadow(true);

    sceneManager_->setShadowTechnique(Ogre::SHADOWTYPE_TEXTURE_ADDITIVE);
    sceneManager_->setShadowTextureSize(512);
    sceneManager_->setShadowTextureCount(2);
    sceneManager_->setShadowColour(Ogre::ColourValue(0.6, 0.6, 0.6));
    sceneManager_->setAmbientLight(Ogre::ColourValue(0.3, 0.3, 0.3));

    // create a floor mesh resource
    Ogre::MeshManager::getSingleton().createPlane("floor", 
                                                  Ogre::ResourceGroupManager::DEFAULT_RESOURCE_GROUP_NAME,
                                                  Ogre::Plane(Ogre::Vector3::UNIT_Y, -1), 
                                                  250, 250, 25, 25, true, 1, 15, 15, 
                                                  Ogre::Vector3::UNIT_Z);

    // add a floor to our scene using the floor mesh we created
    Ogre::Entity* floor = sceneManager_->createEntity("Floor", "floor");
    floor->setMaterialName("Examples/Rockwall");
    //floor->setCastShadows(false);
    //floor->setReceiveShadows(true);
    floor->setRenderQueueGroup(50);
    sceneManager_->getRootSceneNode()->attachObject(floor);

    // add a blue spotlight
    Ogre::Light* l = sceneManager_->createLight();
    Ogre::Vector3 dir;
    l->setType(Ogre::Light::LT_SPOTLIGHT);
    l->setPosition(-70, 100, 30);
    l->setSpotlightRange(Ogre::Degree(30),Ogre::Degree(75));
    dir = -l->getPosition();
    dir.normalise();
    l->setDirection(dir);
    l->setDiffuseColour(0.3, 0.5, 0.5);

    // skybox
    sceneManager_->setSkyBox(true, "Examples/CloudyNoonSkyBox");
    // models
    for (size_t i = 0; i < kNumModels; ++i) {
      Ogre::String name = "jaiqua" + Ogre::StringConverter::toString(i);
      Ogre::SceneNode* sceneNode = sceneManager_->getRootSceneNode()
          ->createChildSceneNode(name);
      sceneNode->setPosition(0, 0, 0);
      sceneNode->yaw(Ogre::Radian(Ogre::Math::PI));
      Ogre::Entity* entity = sceneManager_->createEntity(name, "jaiqua.mesh");
      sceneNode->attachObject(entity);

      for (size_t j = 0; j < entity->getNumSubEntities(); ++j) {
        Ogre::SubEntity* subEntity = entity->getSubEntity(j);
        subEntity->setMaterial(subEntity->getMaterial()->clone(name));
        Ogre::Pass* pass = subEntity->getMaterial()->getBestTechnique()->getPass(0);
        Ogre::ColourValue diffuse = pass->getDiffuse();
        Ogre::ColourValue ambient = pass->getAmbient();
        diffuse.a = (1.0f - 1.0f / kNumModels * i);
        ambient.a = (1.0f - 1.0f / kNumModels * i);
        entity->setRenderQueueGroup(50);
        if (j != 0) {
          diffuse.a /= 2;
          ambient.a /= 2;
#if 0
          entity->setCastShadows(false);
          subEntity->getMaterial()->setTransparencyCastsShadows(false);
          subEntity->getMaterial()->setDepthWriteEnabled(false);
          subEntity->getMaterial()->setSceneBlending(Ogre::SBT_TRANSPARENT_ALPHA);
          //subEntity->getMaterial()->setReceiveShadows(false);
          //subEntity->getMaterial()->setCastShadows(false);
#endif
          entity->setRenderQueueGroup(51);
        }
        pass->setDiffuse(diffuse);
        pass->setAmbient(ambient);
      }

      entities_.push_back(entity);
    }

    // camera
    camera_ = sceneManager_->createCamera("MainCamera");
    viewport_ = root_->getAutoCreatedWindow()->addViewport(camera_);
    camera_->setAspectRatio((Ogre::Real)viewport_->getActualWidth() 
                            / (Ogre::Real)viewport_->getActualHeight());
    camera_->setNearClipDistance(5);
    camera_->setFarClipDistance(100000);
    camera_->setPosition(10, 15, 50);
    camera_->lookAt(10, 15, 0);

    root_->startRendering();

    // clear
    sceneManager_->clearScene();
    root_->destroySceneManager(sceneManager_);
    Ogre::WindowEventUtilities::removeWindowEventListener(root_->getAutoCreatedWindow(), this);
    OGRE_DELETE(root_);
  }

 private:
  bool frameRenderingQueued(const Ogre::FrameEvent& evt) {
    eclapse_time_ += evt.timeSinceLastFrame;

    if (animationStates_.size() < entities_.size()) {
      if (eclapse_time_ > 0.3f * animationStates_.size()) {
        animationStates_.push_back(entities_[animationStates_.size()]->getAnimationState("Sneak"));
        animationStates_.back()->setEnabled(true);
        animationStates_.back()->setLoop(true);
      }
    }

    for(size_t i = 0; i < animationStates_.size(); ++i) 
        animationStates_[i]->addTime(evt.timeSinceLastFrame);

    return true;
  }

  void LoadResources() {
    Ogre::ConfigFile configFile;
    configFile.load(configDir_ + "resources.cfg");

    Ogre::ConfigFile::SectionIterator seci = configFile.getSectionIterator();
    Ogre::String sec, type, arch;

    // go through all specified resource groups
    while (seci.hasMoreElements())
    {   
      sec = seci.peekNextKey();
      Ogre::ConfigFile::SettingsMultiMap* settings = seci.getNext();
      Ogre::ConfigFile::SettingsMultiMap::iterator i;

      // go through all resource paths
      for (i = settings->begin(); i != settings->end(); i++)
      {   
        type = i->first;
        arch = i->second;

        Ogre::ResourceGroupManager::getSingleton().addResourceLocation(arch, type, sec);
      }
    }

    Ogre::ResourceGroupManager::getSingleton().initialiseAllResourceGroups();
    Ogre::TextureManager::getSingleton().setDefaultNumMipmaps(5);
  }

  virtual bool windowClosing(Ogre::RenderWindow* rw) { return true; }

  virtual bool frameEnded(const Ogre::FrameEvent& evt) { 
    return !root_->getAutoCreatedWindow()->isClosed(); 
  }

  Ogre::Root*           root_;
  Ogre::SceneManager*   sceneManager_;
  Ogre::Camera*         camera_;
  Ogre::Viewport*       viewport_;
  Ogre::String          configDir_;
  float                 eclapse_time_;
  std::vector<Ogre::AnimationState*> animationStates_;
  std::vector<Ogre::Entity*> entities_;
};

int main() {
  MyApp app;
  app.Go();
  return 0;
}
