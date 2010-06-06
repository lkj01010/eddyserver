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

#include	"camera_controller.h"

const int kCharHeight = 5;         // height of character's center of mass above groun
const size_t kNumModels = 5;

using namespace Ogre;

class MyApp : public WindowEventListener, 
	public FrameListener,
	public OIS::KeyListener,
	public OIS::MouseListener {
public:
	MyApp() {
		root_			= NULL;
		scene_manager_	= NULL;
		camera_			= NULL;
		viewport_		= NULL;
		config_dir_		= "config/";
		eclapse_time_	= 0.0f;
		body_node_		= NULL;
		body_ent_		= NULL;
		sword1_			= NULL;
		sword2_			= NULL;
		camera_controller_ = NULL;
		OIS::InputManager* input_manager_ = NULL;   // OIS input manager
		OIS::Keyboard*	key_board_ = NULL;       // keyboard device
		OIS::Mouse*		mouse_ = NULL;             // mouse device
	}

	void init() {
		// init root
#ifdef _DEBUG
		root_ = OGRE_NEW Ogre::Root(config_dir_ + "plugins_d.cfg", 
#else
		root_ = OGRE_NEW Ogre::Root(config_dir_ + "plugins.cfg", 
#endif
			config_dir_ + "ogre.cfg", 
			config_dir_ + "ogre.log");
		root_->addFrameListener(this);

		if (!root_->restoreConfig())
			root_->showConfigDialog();

		// window
		root_->initialise(true, "Eddy Demo");
		Ogre::WindowEventUtilities::addWindowEventListener(root_->getAutoCreatedWindow(), this);

		// resources
		loadResources();
		Ogre::TextureManager::getSingleton().setDefaultNumMipmaps(5);

		// scene manager
		scene_manager_ = root_->createSceneManager(Ogre::ST_GENERIC);
		scene_manager_->setShadowCameraSetup(Ogre::ShadowCameraSetupPtr(new Ogre::LiSPSMShadowCameraSetup));
		scene_manager_->setShadowTexturePixelFormat(Ogre::PF_FLOAT16_R);
		//scene_manager_->setShadowTextureCasterMaterial(CUSTOM_CASTER_MATERIAL);
		//scene_manager_->setShadowTextureReceiverMaterial(CUSTOM_RECEIVER_MATERIAL);
		//scene_manager_->setShadowTextureSelfShadow(true);

		scene_manager_->setShadowTechnique(Ogre::SHADOWTYPE_TEXTURE_ADDITIVE);
		scene_manager_->setShadowTextureSize(512);
		scene_manager_->setShadowTextureCount(2);
		scene_manager_->setShadowColour(Ogre::ColourValue(0.6, 0.6, 0.6));
		scene_manager_->setAmbientLight(Ogre::ColourValue(0.3, 0.3, 0.3));

		// create a floor mesh resource
		Ogre::MeshManager::getSingleton().createPlane("floor", 
			Ogre::ResourceGroupManager::DEFAULT_RESOURCE_GROUP_NAME,
			Ogre::Plane(Ogre::Vector3::UNIT_Y, -1), 
			250, 250, 25, 25, true, 1, 15, 15, 
			Ogre::Vector3::UNIT_Z);

		// add a floor to our scene using the floor mesh we created
		Ogre::Entity* floor = scene_manager_->createEntity("Floor", "floor");
		floor->setMaterialName("Rockwall");
		scene_manager_->getRootSceneNode()->attachObject(floor);

		// add a blue spotlight
		Ogre::Light* l = scene_manager_->createLight();
		Ogre::Vector3 dir;
		l->setType(Ogre::Light::LT_SPOTLIGHT);
		l->setPosition(-70, 100, 30);
		l->setSpotlightRange(Ogre::Degree(30),Ogre::Degree(75));
		dir = -l->getPosition();
		dir.normalise();
		l->setDirection(dir);
		l->setDiffuseColour(0.3, 0.5, 0.5);

		// create main model
		body_node_ = scene_manager_->getRootSceneNode()->createChildSceneNode(Vector3::UNIT_Y * kCharHeight);
		body_ent_ = scene_manager_->createEntity("SinbadBody", "Sinbad.mesh");
		body_node_->attachObject(body_ent_);

		// create swords and attach to sheath
		sword1_ = scene_manager_->createEntity("SinbadSword1", "Sword.mesh");
		sword2_ = scene_manager_->createEntity("SinbadSword2", "Sword.mesh");
		body_ent_->attachObjectToBone("Sheath.L", sword1_);
		body_ent_->attachObjectToBone("Sheath.R", sword2_);

		// camera
		camera_ = scene_manager_->createCamera("MainCamera");
		viewport_ = root_->getAutoCreatedWindow()->addViewport(camera_);
		camera_->setAspectRatio((Ogre::Real)viewport_->getActualWidth() 
			/ (Ogre::Real)viewport_->getActualHeight());
		camera_->setNearClipDistance(5);
		camera_->setFarClipDistance(100000);
		camera_->setPosition(10, 15, 50);
		camera_->lookAt(10, 15, 0);

		camera_controller_ = new CameraController(camera_);

		// input
		setupInput();

		root_->startRendering();

	}

	/*-----------------------------------------------------------------------------
	| Sets up OIS input.
	-----------------------------------------------------------------------------*/
	void setupInput()
	{
		OIS::ParamList pl;
		size_t winHandle = 0;
		std::ostringstream winHandleStr;

		RenderWindow* window = root_->getAutoCreatedWindow();
		window->getCustomAttribute("WINDOW", &winHandle);
		winHandleStr << winHandle;

		pl.insert(std::make_pair("WINDOW", winHandleStr.str()));

		input_manager_ = OIS::InputManager::createInputSystem(pl);

		createInputDevices();      // create the specific input devices

		windowResized(window);    // do an initial adjustment of mouse area
	}

	/*-----------------------------------------------------------------------------
	| Creates the individual input devices. I only create a keyboard and mouse
	| here because they are the most common, but you can override this method
	| for other modes and devices.
	-----------------------------------------------------------------------------*/
	void createInputDevices()
	{
		key_board_ = static_cast<OIS::Keyboard*>(input_manager_->createInputObject(OIS::OISKeyboard, true));
		mouse_ = static_cast<OIS::Mouse*>(input_manager_->createInputObject(OIS::OISMouse, true));

		key_board_->setEventCallback(this);
		mouse_->setEventCallback(this);
	}

	/*-----------------------------------------------------------------------------
	| Destroys OIS input devices and the input manager.
	-----------------------------------------------------------------------------*/
	void shutdownInput()
	{
		if (input_manager_)
		{
			input_manager_->destroyInputObject(key_board_);
			input_manager_->destroyInputObject(mouse_);

			OIS::InputManager::destroyInputSystem(input_manager_);
			input_manager_ = NULL;
		}
	}

	/*-----------------------------------------------------------------------------
	| Captures input device states.
	-----------------------------------------------------------------------------*/
	void captureInputDevices()
	{
		key_board_->capture();
		mouse_->capture();
	}

	void clear() {
		// clear
		scene_manager_->clearScene();
		root_->destroySceneManager(scene_manager_);
		Ogre::WindowEventUtilities::removeWindowEventListener(root_->getAutoCreatedWindow(), this);
		delete(camera_controller_);
		shutdownInput();
		OGRE_DELETE(root_);
	}

	void go() {
		init();
		clear();
	}

	virtual bool keyPressed(const OIS::KeyEvent& evt) {
		if (camera_controller_ != NULL)
			camera_controller_->injectKeyDown(evt);
		return true;
	}

	virtual bool keyReleased(const OIS::KeyEvent& evt) {
		if (camera_controller_ != NULL)
			camera_controller_->injectKeyUp(evt);
		return true;
	}

	virtual bool mousePressed(const OIS::MouseEvent& evt, OIS::MouseButtonID id) {
		if (camera_controller_ != NULL)
			camera_controller_->injectMouseDown(evt, id);
		return true;
	}

	virtual bool mouseReleased(const OIS::MouseEvent& evt, OIS::MouseButtonID id) {
		if (camera_controller_ != NULL)
			camera_controller_->injectMouseUp(evt, id);
		return true;
	}

	virtual bool mouseMoved(const OIS::MouseEvent& evt) {
		if (camera_controller_ != NULL)
			camera_controller_->injectMouseMove(evt);
		return true;
	}
private:
	bool frameRenderingQueued(const Ogre::FrameEvent& evt) {
		eclapse_time_ += evt.timeSinceLastFrame;

		if (camera_controller_ != NULL)
			camera_controller_->frameRenderingQueued(evt);

		return true;
	}

	bool frameStarted(const Ogre::FrameEvent& evt) {
		captureInputDevices();
		return true;
	}
	void loadResources() {
		Ogre::ConfigFile configFile;
#ifdef _DEBUG
		configFile.load(config_dir_ + "resources_d.cfg");
#else
		configFile.load(config_dir_ + "resources.cfg");
#endif

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

	Root*           root_;
	SceneManager*   scene_manager_;
	Camera*         camera_;
	Viewport*       viewport_;
	String          config_dir_;
	float           eclapse_time_;
	SceneNode*		body_node_;
	Entity*			body_ent_;
	Entity*			sword1_;
	Entity*			sword2_;
	CameraController* camera_controller_;
	OIS::InputManager* input_manager_;   // OIS input manager
	OIS::Keyboard*	key_board_;       // keyboard device
	OIS::Mouse*		mouse_;             // mouse device
};

int main() {
	MyApp app;
	app.go();
	return 0;
}
