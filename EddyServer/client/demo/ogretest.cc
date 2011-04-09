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

#include   "scene.h"


using namespace Ogre;

class MyApp : public WindowEventListener, 
	public FrameListener,
	public OIS::KeyListener,
	public OIS::MouseListener {
public:
	MyApp() {
		root_			= NULL;
		config_dir_		= "config/";
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

		// input
		setupInput();
		scene_.setupContent(root_);
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

	void shutdown() {
		// clear
		shutdownInput();
		Ogre::WindowEventUtilities::removeWindowEventListener(root_->getAutoCreatedWindow(), this);	
		OGRE_DELETE(root_);
	}

	void go() {
		init();
		shutdown();
	}

private:
	virtual bool keyPressed(const OIS::KeyEvent& evt) {
		return scene_.keyPressed(evt);
	}

	virtual bool keyReleased(const OIS::KeyEvent& evt) {
		return scene_.keyReleased(evt);
	}

	virtual bool mousePressed(const OIS::MouseEvent& evt, OIS::MouseButtonID id) {
		return scene_.mousePressed(evt, id);
	}

	virtual bool mouseReleased(const OIS::MouseEvent& evt, OIS::MouseButtonID id) {
		return scene_.mouseReleased(evt, id);
	}

	virtual bool mouseMoved(const OIS::MouseEvent& evt) {
		return scene_.mouseMoved(evt);
	}

	bool frameRenderingQueued(const Ogre::FrameEvent& evt) {
		return scene_.frameRenderingQueued(evt);
	}

	bool frameStarted(const Ogre::FrameEvent& evt) {
		captureInputDevices();
		return true;
	}

	/*-----------------------------------------------------------------------------
	| Captures input device states.
	-----------------------------------------------------------------------------*/
	void captureInputDevices()
	{
		key_board_->capture();
		mouse_->capture();
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
	
	Scene scene_;
	String config_dir_;
	OIS::InputManager* input_manager_;   // OIS input manager
	OIS::Keyboard*	key_board_;       // keyboard device
	OIS::Mouse*		mouse_;             // mouse device
};

int main() {
	MyApp app;
	app.go();
	return 0;
}
