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
#include   "scene_terrain.h"

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

		scene_manager_->setShadowTechnique(Ogre::SHADOWTYPE_NONE);
		scene_manager_->setShadowTextureSize(512);
		scene_manager_->setShadowTextureCount(2);
		scene_manager_->setShadowColour(Ogre::ColourValue(0.6, 0.6, 0.6));

		scene_manager_->setFog(Ogre::FOG_LINEAR, ColourValue(0.7, 0.7, 0.8), 0, 10000, 25000);
		scene_manager_->setSkyBox(true, "CloudyNoonSkyBox");

		Vector3 lightdir(0.55, -0.3, 0.75);
		lightdir.normalise();

		Light* l = scene_manager_->createLight("sun");
		l->setType(Light::LT_DIRECTIONAL);
		l->setDirection(lightdir);
		l->setDiffuseColour(ColourValue::White);
		l->setSpecularColour(ColourValue(0.4, 0.4, 0.4));

		scene_manager_->setAmbientLight(ColourValue(0.3, 0.3, 0.3));

		// find character pos
		terrain_.setupContent(scene_manager_, l);
		Ray ray;
		ray.setOrigin(Vector3(0, 10000, 0));
		ray.setDirection(Vector3::NEGATIVE_UNIT_Y);

		TerrainGroup::RayResult ray_result = terrain_.findIntersect(ray);
		// camera
		camera_ = scene_manager_->createCamera("MainCamera");
		viewport_ = root_->getAutoCreatedWindow()->addViewport(camera_);
		viewport_->setBackgroundColour(ColourValue(1.0f, 1.0f, 0.8f));
		camera_->setAspectRatio((Ogre::Real)viewport_->getActualWidth() 
			/ (Ogre::Real)viewport_->getActualHeight());
		camera_->setNearClipDistance(0.1);
		camera_->setFarClipDistance(20000);
		camera_->setPosition(ray_result.position + Vector3(0, kCharHeight, 10));
		camera_->lookAt(ray_result.position);

		camera_controller_ = new CameraController(camera_);

		// create main model
		body_node_ = scene_manager_->getRootSceneNode()->createChildSceneNode(Vector3::UNIT_Y * kCharHeight + ray_result.position);
		body_ent_ = scene_manager_->createEntity("SinbadBody", "Sinbad.mesh");
		body_node_->attachObject(body_ent_);

		// create swords and attach to sheath
		sword1_ = scene_manager_->createEntity("SinbadSword1", "Sword.mesh");
		sword2_ = scene_manager_->createEntity("SinbadSword2", "Sword.mesh");
		body_ent_->attachObjectToBone("Sheath.L", sword1_);
		body_ent_->attachObjectToBone("Sheath.R", sword2_);

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
		terrain_.shutdown();
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
	SceneTerrain terrain_;
	OIS::InputManager* input_manager_;   // OIS input manager
	OIS::Keyboard*	key_board_;       // keyboard device
	OIS::Mouse*		mouse_;             // mouse device
};

int main() {
	MyApp app;
	app.go();
	return 0;
}
