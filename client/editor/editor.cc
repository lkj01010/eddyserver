// =====================================================================================
// 
//       Filename:  editor.cc
// 
//    Description:  editor sample
// 
//        Version:  1.0
//        Created:  2010-03-07 14:40:41
//       Revision:  none
//       Compiler:  g++
// 
//         Author:  liaoxinwei (Comet), cometliao@gmail.com
//        Company:  eddy
// 
// =====================================================================================

#include    <iostream>
#include    <sstream>

#include    <OGRE/OgreRoot.h>
#include    <OGRE/OgreFrameListener.h>
#include    <OGRE/OgreWindowEventUtilities.h>
#include    <OGRE/OgreRenderWindow.h>
#include    <OGRE/OgreCamera.h>
#include    <OGRE/OgreConfigFile.h>

#include    "sdk_trays.h"

using namespace OgreBites;

class MyApp : public Ogre::WindowEventListener, 
    public Ogre::FrameListener,
    public OIS::KeyListener,
    public OIS::MouseListener,
    public SdkTrayListener
{
 public:
  MyApp() {
    root_           = NULL;
    sceneManager_   = NULL;
    camera_         = NULL;
    viewport_       = NULL;
    tray_manager_   = NULL;
    input_manager_  = NULL;
    mouse_          = NULL;
    keyboard_       = NULL;
    configDir_      = "config/";
    eclapse_time_   = 0.0f;
  }

  void Go () {
    Setup();
    root_->startRendering();
    Clear();
  }

 private:
  bool frameRenderingQueued(const Ogre::FrameEvent& evt) {
    eclapse_time_ += evt.timeSinceLastFrame;

    return true;
  }

  void Setup() {
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

    SetupInput();

    // trayMgr
    tray_manager_ = new SdkTrayManager("BrowserControls", 
                                       root_->getAutoCreatedWindow(), mouse_, this);
    tray_manager_->showBackdrop("SdkTrays/Bands");
    tray_manager_->getTrayContainer(TL_NONE)->hide();
    tray_manager_->showLogo(TL_BOTTOMRIGHT);

    // scene manager
    sceneManager_ = root_->createSceneManager(Ogre::ST_GENERIC);

    // skybox
    //sceneManager_->setSkyBox(true, "Examples/CloudyNoonSkyBox");

    // camera
    camera_ = sceneManager_->createCamera("MainCamera");
    viewport_ = root_->getAutoCreatedWindow()->addViewport(camera_);
    camera_->setAspectRatio((Ogre::Real)viewport_->getActualWidth() 
                            / (Ogre::Real)viewport_->getActualHeight());
    camera_->setNearClipDistance(5);
    camera_->setFarClipDistance(100000);
    camera_->setPosition(10, 15, 50);
    camera_->lookAt(10, 15, 0);
  }

  void Clear() {
    // clear
    sceneManager_->clearScene();
    root_->destroySceneManager(sceneManager_);
    ShutdownInput();
    Ogre::WindowEventUtilities::removeWindowEventListener(root_->getAutoCreatedWindow(), this);
    OGRE_DELETE(root_);
  }

  void SetupInput() {
    OIS::ParamList pl;
    size_t win_handle = 0;
    std::ostringstream win_handle_str;

    root_->getAutoCreatedWindow()->getCustomAttribute("WINDOW", &win_handle);
    win_handle_str << win_handle;

    pl.insert(std::make_pair("WINDOW", win_handle_str.str()));

    input_manager_ = OIS::InputManager::createInputSystem(pl);

    keyboard_ = static_cast<OIS::Keyboard*>(input_manager_->createInputObject(OIS::OISKeyboard, true));
    mouse_ = static_cast<OIS::Mouse*>(input_manager_->createInputObject(OIS::OISMouse, true));

    keyboard_->setEventCallback(this);
    mouse_->setEventCallback(this);

    windowResized(root_->getAutoCreatedWindow());    // do an initial adjustment of mouse area
  }

  void ShutdownInput() {
    if (input_manager_)
    {
      input_manager_->destroyInputObject(keyboard_);
      input_manager_->destroyInputObject(mouse_);

      OIS::InputManager::destroyInputSystem(input_manager_);
      input_manager_ = 0;
    }
  }

  void LoadResources() {
    Ogre::ConfigFile configFile;
    configFile.load(configDir_ + "resources.cfg");

    Ogre::ConfigFile::SectionIterator seci = configFile.getSectionIterator();
    Ogre::String sec, type, arch;

    // go through all specified resource groups
    while (seci.hasMoreElements()) {   
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

  virtual void windowResized(Ogre::RenderWindow* rw) {
    if (!tray_manager_) return;

    Ogre::OverlayContainer* center = tray_manager_->getTrayContainer(TL_CENTER);
    Ogre::OverlayContainer* left = tray_manager_->getTrayContainer(TL_LEFT);

    if (center->isVisible() && rw->getWidth() < 1280 - center->getWidth()) {
      while (center->isVisible()) {
        tray_manager_->moveWidgetToTray(tray_manager_->getWidget(TL_CENTER, 0), TL_LEFT);
      }
    }
    else if (left->isVisible() && rw->getWidth() >= 1280 - left->getWidth()) {
      while (left->isVisible()) {
        tray_manager_->moveWidgetToTray(tray_manager_->getWidget(TL_LEFT, 0), TL_CENTER);
      }
    }

  }


  virtual bool mouseMoved(const OIS::MouseEvent& evt) { return true; }
  virtual bool mousePressed(const OIS::MouseEvent& evt, OIS::MouseButtonID id) { return true; }
  virtual bool mouseReleased(const OIS::MouseEvent& evt, OIS::MouseButtonID id) { return true; }
  virtual bool keyPressed(const OIS::KeyEvent& evt) { return true; }
  virtual bool keyReleased(const OIS::KeyEvent& evt) { return true; }
  virtual void setupWidgets() {
    tray_manager_->destroyAllWidgets();
  }

  virtual bool frameEnded(const Ogre::FrameEvent& evt) { 
    return !root_->getAutoCreatedWindow()->isClosed(); 
  }

  Ogre::Root*             root_;
  Ogre::SceneManager*     sceneManager_;
  Ogre::Camera*           camera_;
  Ogre::Viewport*         viewport_;
  Ogre::String            configDir_;
  SdkTrayManager*         tray_manager_;
  OIS::InputManager*      input_manager_;   // OIS input manager
  OIS::Keyboard*          keyboard_;       // keyboard device
  OIS::Mouse*             mouse_;          // mouse device
  float                   eclapse_time_;
};

int main() {
  MyApp app;
  app.Go();
  return 0;
}
