#include "scene.h"

#include "OGRE/OgreMaterialManager.h"
#include "OGRE/OgreGpuProgramManager.h"
#include "OGRE/OgreShadowCameraSetupPSSM.h"

#include "OGRE/Terrain/OgreTerrain.h"
#include "OGRE/Terrain/OgreTerrainGroup.h"
#include "OGRE/Terrain/OgreTerrainMaterialGeneratorA.h"

#include "character.h"

#define TERRAIN_PAGE_MIN_X 0
#define TERRAIN_PAGE_MIN_Y 0
#define TERRAIN_PAGE_MAX_X 0
#define TERRAIN_PAGE_MAX_Y 0

#define TERRAIN_FILE_PREFIX String("testTerrain")
#define TERRAIN_FILE_SUFFIX String("dat")
#define TERRAIN_WORLD_SIZE 12000.0f
#define TERRAIN_SIZE 513

const int kCharHeight = 5;         // height of character's center of mass above groun

using namespace Ogre;

Scene::Scene()
: terrain_globals_(NULL)
, terrain_group_(NULL)
, terrain_pos_(0,0,0)
{
	scene_manager_	= NULL;
	camera_			= NULL;
	camera_controller_ = NULL;
	character_			= NULL;
}

void Scene::testCapabilities(const RenderSystemCapabilities* caps)
{
	if (!caps->hasCapability(RSC_VERTEX_PROGRAM) || !caps->hasCapability(RSC_FRAGMENT_PROGRAM))
	{
		OGRE_EXCEPT(Exception::ERR_NOT_IMPLEMENTED, "Your graphics card does not support vertex or fragment shaders, "
			"so you cannot run this sample. Sorry!", "Sample_Terrain::testCapabilities");
	}
}

#if 0
bool Scene::frameRenderingQueued(const FrameEvent& evt)
{

	if (!mFly)
	{
		// clamp to terrain
		Vector3 camPos = mCamera->getPosition();
		Ray ray;
		ray.setOrigin(Vector3(camPos.x, terrain_pos_.y + 10000, camPos.z));
		ray.setDirection(Vector3::NEGATIVE_UNIT_Y);

		TerrainGroup::RayResult rayResult = terrain_group_->rayIntersects(ray);
		Real distanceAboveTerrain = 50;
		Real fallSpeed = 300;
		Real newy = camPos.y;
		if (rayResult.hit)
		{
			if (camPos.y > rayResult.position.y + distanceAboveTerrain)
			{
				mFallVelocity += evt.timeSinceLastFrame * 20;
				mFallVelocity = std::min(mFallVelocity, fallSpeed);
				newy = camPos.y - mFallVelocity * evt.timeSinceLastFrame;

			}
			newy = std::max(rayResult.position.y + distanceAboveTerrain, newy);
			mCamera->setPosition(camPos.x, newy, camPos.z);

		}
	}

	return true;
}
#endif

void Scene::saveTerrains(bool onlyIfModified)
{
	terrain_group_->saveAllTerrains(onlyIfModified);
}

#if 0
bool Scene::keyPressed (const OIS::KeyEvent &e)
{

	switch (e.key)
	{
	case OIS::KC_S:
		// CTRL-S to save
		if (mKeyboard->isKeyDown(OIS::KC_LCONTROL) || mKeyboard->isKeyDown(OIS::KC_RCONTROL))
		{
			saveTerrains(true);
		}
		else
			return SdkSample::keyPressed(e);
		break;
	case OIS::KC_F10:
		// dump
		{
			TerrainGroup::TerrainIterator ti = terrain_group_->getTerrainIterator();
			while (ti.hasMoreElements())
			{
				Ogre::uint32 tkey = ti.peekNextKey();
				TerrainGroup::TerrainSlot* ts = ti.getNext();
				if (ts->instance && ts->instance->isLoaded())
				{
					ts->instance->_dumpTextures("terrain_" + StringConverter::toString(tkey), ".png");
				}
			}
		}
		break;
	default:
		return SdkSample::keyPressed(e);
	}


	return true;
}
#endif



void Scene::defineTerrain(long x, long y, bool flat)
{
	// if a file is available, use it
	// if not, generate file from import

	// Usually in a real project you'll know whether the compact terrain data is
	// available or not; I'm doing it this way to save distribution size

	if (flat)
	{
		terrain_group_->defineTerrain(x, y, 0.0f);
	}
	else
	{
		String filename = terrain_group_->generateFilename(x, y);
		if (ResourceGroupManager::getSingleton().resourceExists(terrain_group_->getResourceGroup(), filename))
		{
			terrain_group_->defineTerrain(x, y);
		}
		else
		{
			Image img;
			getTerrainImage(x % 2 != 0, y % 2 != 0, img);
			terrain_group_->defineTerrain(x, y, &img);
		}
	}
}

void Scene::getTerrainImage(bool flipX, bool flipY, Image& img)
{
	img.load("terrain.png", ResourceGroupManager::DEFAULT_RESOURCE_GROUP_NAME);
	if (flipX)
		img.flipAroundY();
	if (flipY)
		img.flipAroundX();

}

void Scene::initBlendMaps(Terrain* terrain)
{
	TerrainLayerBlendMap* blendMap0 = terrain->getLayerBlendMap(1);
	TerrainLayerBlendMap* blendMap1 = terrain->getLayerBlendMap(2);
	Real minHeight0 = 70;
	Real fadeDist0 = 30;
	Real minHeight1 = 150;
	Real fadeDist1 = 60;
	float* pBlend0 = blendMap0->getBlendPointer();
	float* pBlend1 = blendMap1->getBlendPointer();
	for (Ogre::uint16 y = 0; y < terrain->getLayerBlendMapSize(); ++y)
	{
		for (Ogre::uint16 x = 0; x < terrain->getLayerBlendMapSize(); ++x)
		{
			Real tx, ty;

			blendMap0->convertImageToTerrainSpace(x, y, &tx, &ty);
			Real height = terrain->getHeightAtTerrainPosition(tx, ty);
			Real val = (height - minHeight0) / fadeDist0;
			val = Math::Clamp(val, (Real)0, (Real)1);
			*pBlend0++ = val;

			val = (height - minHeight1) / fadeDist1;
			val = Math::Clamp(val, (Real)0, (Real)1);
			*pBlend1++ = val;
		}
	}
	blendMap0->dirty();
	blendMap1->dirty();
	blendMap0->update();
	blendMap1->update();
}

void Scene::configureTerrainDefaults(SceneManager* scene_manager_, Light* l)
{
	// Configure global
	terrain_globals_->setMaxPixelError(8);
	// testing composite map

	terrain_globals_->setCompositeMapDistance(3000);
	//terrain_globals_->setUseRayBoxDistanceCalculation(true);
	//terrain_globals_->getDefaultMaterialGenerator()->setDebugLevel(1);
	//terrain_globals_->setLightMapSize(256);

	//matProfile->setLightmapEnabled(false);
	// Important to set these so that the terrain knows what to use for derived (non-realtime) data
	terrain_globals_->setLightMapDirection(l->getDerivedDirection());
	terrain_globals_->setCompositeMapAmbient(scene_manager_->getAmbientLight());
	//terrain_globals_->setCompositeMapAmbient(ColourValue::Red);
	terrain_globals_->setCompositeMapDiffuse(l->getDiffuseColour());

	// Configure default import settings for if we use imported image
	Terrain::ImportData& defaultimp = terrain_group_->getDefaultImportSettings();
	defaultimp.terrainSize = TERRAIN_SIZE;
	defaultimp.worldSize = TERRAIN_WORLD_SIZE;
	defaultimp.inputScale = 600;
	defaultimp.minBatchSize = 33;
	defaultimp.maxBatchSize = 65;
	// textures
	defaultimp.layerList.resize(3);
	defaultimp.layerList[0].worldSize = 100;
	defaultimp.layerList[0].textureNames.push_back("dirt_grayrocky_diffusespecular.dds");
	defaultimp.layerList[0].textureNames.push_back("dirt_grayrocky_normalheight.dds");
	defaultimp.layerList[1].worldSize = 30;
	defaultimp.layerList[1].textureNames.push_back("grass_green-01_diffusespecular.dds");
	defaultimp.layerList[1].textureNames.push_back("grass_green-01_normalheight.dds");
	defaultimp.layerList[2].worldSize = 200;
	defaultimp.layerList[2].textureNames.push_back("growth_weirdfungus-03_diffusespecular.dds");
	defaultimp.layerList[2].textureNames.push_back("growth_weirdfungus-03_normalheight.dds");


}

void Scene::setupContent(Ogre::Root* root)
{
	// scene manager
	scene_manager_ = root->createSceneManager(Ogre::ST_GENERIC);

	scene_manager_->setFog(Ogre::FOG_LINEAR, ColourValue(0.7, 0.7, 0.8), 0, 10000, 25000);
	scene_manager_->setSkyBox(true, "CloudyNoonSkyBox");

	Vector3 lightdir(-0.55, -0.4, -0.75);
	lightdir.normalise();

	Light* l = scene_manager_->createLight("sun");
	l->setType(Light::LT_DIRECTIONAL);
	l->setDirection(lightdir);
	l->setDiffuseColour(ColourValue::White);
	l->setSpecularColour(ColourValue(0.4, 0.4, 0.4));

	scene_manager_->setAmbientLight(ColourValue(0.3, 0.3, 0.3));
	// camera_
	camera_ = scene_manager_->createCamera("MainCamera");
	Viewport* viewport = root->getAutoCreatedWindow()->addViewport(camera_);
	viewport->setBackgroundColour(ColourValue(1.0f, 1.0f, 0.8f));
	camera_->setAspectRatio((Ogre::Real)viewport->getActualWidth() 
		/ (Ogre::Real)viewport->getActualHeight());
	camera_->setNearClipDistance(0.1);
	camera_->setFarClipDistance(20000);

	// terrain
	terrain_globals_ = OGRE_NEW Ogre::TerrainGlobalOptions();

	MaterialManager::getSingleton().setDefaultTextureFiltering(TFO_ANISOTROPIC);
	MaterialManager::getSingleton().setDefaultAnisotropy(7);

	terrain_group_ = OGRE_NEW TerrainGroup(scene_manager_, Terrain::ALIGN_X_Z, TERRAIN_SIZE, TERRAIN_WORLD_SIZE);
	terrain_group_->setFilenameConvention(TERRAIN_FILE_PREFIX, TERRAIN_FILE_SUFFIX);
	terrain_group_->setOrigin(terrain_pos_);

	configureTerrainDefaults(scene_manager_, l);

	for (long x = TERRAIN_PAGE_MIN_X; x <= TERRAIN_PAGE_MAX_X; ++x)
		for (long y = TERRAIN_PAGE_MIN_Y; y <= TERRAIN_PAGE_MAX_Y; ++y)
			defineTerrain(x, y, false);
	// sync load since we want everything in place when we start
	terrain_group_->loadAllTerrains(true);

	TerrainGroup::TerrainIterator ti = terrain_group_->getTerrainIterator();
	while(ti.hasMoreElements())
	{
		Terrain* t = ti.getNext()->instance;
		initBlendMaps(t);
	}

	terrain_group_->freeTemporaryResources();
	saveTerrains(true);

	// General scene setup
	scene_manager_->setShadowTechnique(SHADOWTYPE_TEXTURE_ADDITIVE_INTEGRATED);
	scene_manager_->setShadowFarDistance(3000);

	// 3 textures per directional light (PSSM)
	scene_manager_->setShadowTextureCountPerLightType(Ogre::Light::LT_DIRECTIONAL, 3);

	if (pssm_setup_.isNull())
	{
		// shadow camera_ setup
		PSSMShadowCameraSetup* pssmSetup = new PSSMShadowCameraSetup();
		pssmSetup->setSplitPadding(camera_->getNearClipDistance());
		pssmSetup->calculateSplitPoints(3, camera_->getNearClipDistance(), scene_manager_->getShadowFarDistance());
		pssmSetup->setOptimalAdjustFactor(0, 2);
		pssmSetup->setOptimalAdjustFactor(1, 1);
		pssmSetup->setOptimalAdjustFactor(2, 0.5);

		pssm_setup_.bind(pssmSetup);

	}
	scene_manager_->setShadowCameraSetup(pssm_setup_);

	scene_manager_->setShadowTextureCount(3);
	scene_manager_->setShadowTextureConfig(0, 2048, 2048, PF_FLOAT32_R);
	scene_manager_->setShadowTextureConfig(1, 1024, 1024, PF_FLOAT32_R);
	scene_manager_->setShadowTextureConfig(2, 1024, 1024, PF_FLOAT32_R);
	scene_manager_->setShadowTextureSelfShadow(true);
	scene_manager_->setShadowCasterRenderBackFaces(true);

	Vector4 splitPoints;
	const PSSMShadowCameraSetup::SplitPointList& splitPointList = 
		static_cast<PSSMShadowCameraSetup*>(pssm_setup_.get())->getSplitPoints();
	for (int i = 0; i < 3; ++i)
	{
		splitPoints[i] = splitPointList[i];
	}
	TerrainMaterialGeneratorA::SM2Profile* mat_profile = 
		static_cast<TerrainMaterialGeneratorA::SM2Profile*>(terrain_globals_->getDefaultMaterialGenerator()->getActiveProfile());
	mat_profile->setReceiveDynamicShadowsEnabled(true);

	mat_profile->setReceiveDynamicShadowsLowLod(false);
	GpuSharedParametersPtr p = GpuProgramManager::getSingleton().getSharedParameters("pssm_params");
	p->setNamedConstant("pssmSplitPoints", splitPoints);

	mat_profile->setReceiveDynamicShadowsDepth(true);
	mat_profile->setReceiveDynamicShadowsPSSM(static_cast<PSSMShadowCameraSetup*>(pssm_setup_.get()));

	//camera_controller_ = new CameraController(camera_);

	character_ = new Character(this);
}

void Scene::shutdown(Ogre::Root* root)
{
	scene_manager_->clearScene();
	root->destroySceneManager(scene_manager_);
	scene_manager_ = NULL;

	if (camera_controller_ != NULL) {
		delete(camera_controller_);
		camera_controller_ = NULL;
	}

	delete character_;
	character_ = NULL;
	OGRE_DELETE terrain_group_;
	OGRE_DELETE terrain_globals_;
}

bool Scene::keyPressed(const OIS::KeyEvent& evt) {
	if (camera_controller_ != NULL)
		camera_controller_->injectKeyDown(evt);

	character_->injectKeyDown(evt);
	return true;
}

bool Scene::keyReleased(const OIS::KeyEvent& evt) {
	if (camera_controller_ != NULL)
		camera_controller_->injectKeyUp(evt);

	character_->injectKeyUp(evt);
	return true;
}

bool Scene::mousePressed(const OIS::MouseEvent& evt, OIS::MouseButtonID id) {
	if (camera_controller_ != NULL)
		camera_controller_->injectMouseDown(evt, id);

	character_->injectMouseDown(evt, id);
	return true;
}

bool Scene::mouseReleased(const OIS::MouseEvent& evt, OIS::MouseButtonID id) {
	if (camera_controller_ != NULL)
		camera_controller_->injectMouseUp(evt, id);

	character_->injectMouseUp(evt, id);
	return true;
}

bool Scene::mouseMoved(const OIS::MouseEvent& evt) {
	if (camera_controller_ != NULL)
		camera_controller_->injectMouseMove(evt);

	character_->injectMouseMove(evt);
	return true;
}

bool Scene::frameRenderingQueued(const Ogre::FrameEvent& evt) {
	if (camera_controller_ != NULL)
		camera_controller_->frameRenderingQueued(evt);

	character_->addTime(evt.timeSinceLastFrame);
	return true;
}
