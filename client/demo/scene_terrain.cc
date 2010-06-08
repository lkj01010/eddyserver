#include "scene_terrain.h"

#include "OGRE/OgreMaterialManager.h"

#define TERRAIN_PAGE_MIN_X 0
#define TERRAIN_PAGE_MIN_Y 0
#define TERRAIN_PAGE_MAX_X 0
#define TERRAIN_PAGE_MAX_Y 0

#define TERRAIN_FILE_PREFIX String("testTerrain")
#define TERRAIN_FILE_SUFFIX String("dat")
#define TERRAIN_WORLD_SIZE 12000.0f
#define TERRAIN_SIZE 513

using namespace Ogre;

SceneTerrain::SceneTerrain()
: terrain_globals_(NULL)
, terrain_group_(0)
, terrain_pos_(0,0,0)
{
}

void SceneTerrain::testCapabilities(const RenderSystemCapabilities* caps)
{
	if (!caps->hasCapability(RSC_VERTEX_PROGRAM) || !caps->hasCapability(RSC_FRAGMENT_PROGRAM))
	{
		OGRE_EXCEPT(Exception::ERR_NOT_IMPLEMENTED, "Your graphics card does not support vertex or fragment shaders, "
			"so you cannot run this sample. Sorry!", "Sample_Terrain::testCapabilities");
	}
}

TerrainGroup::RayResult SceneTerrain::findIntersect(const Ray& ray)
{
		return terrain_group_->rayIntersects(ray);
}

bool SceneTerrain::frameRenderingQueued(const FrameEvent& evt)
{
#if 0
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
#endif
	return true;
}

void SceneTerrain::saveTerrains(bool onlyIfModified)
{
	terrain_group_->saveAllTerrains(onlyIfModified);
}

bool SceneTerrain::keyPressed (const OIS::KeyEvent &e)
{
#if 0
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
#endif

	return true;
}



void SceneTerrain::defineTerrain(long x, long y, bool flat)
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

void SceneTerrain::getTerrainImage(bool flipX, bool flipY, Image& img)
{
	img.load("terrain.png", ResourceGroupManager::DEFAULT_RESOURCE_GROUP_NAME);
	if (flipX)
		img.flipAroundY();
	if (flipY)
		img.flipAroundX();

}

void SceneTerrain::initBlendMaps(Terrain* terrain)
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

void SceneTerrain::configureTerrainDefaults(SceneManager* scene_manager, Light* l)
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
	terrain_globals_->setCompositeMapAmbient(scene_manager->getAmbientLight());
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


MaterialPtr SceneTerrain::buildDepthShadowMaterial(const String& textureName)
{
#if 0
	String matName = "DepthShadows/" + textureName;

	MaterialPtr ret = MaterialManager::getSingleton().getByName(matName);
	if (ret.isNull())
	{
		MaterialPtr baseMat = MaterialManager::getSingleton().getByName("Ogre/shadow/depth/integrated/pssm");
		ret = baseMat->clone(matName);
		Pass* p = ret->getTechnique(0)->getPass(0);
		p->getTextureUnitState("diffuse")->setTextureName(textureName);

		Vector4 splitPoints;
		const PSSMShadowCameraSetup::SplitPointList& splitPointList = 
			static_cast<PSSMShadowCameraSetup*>(pssm_setup_.get())->getSplitPoints();
		for (int i = 0; i < 3; ++i)
		{
			splitPoints[i] = splitPointList[i];
		}
		p->getFragmentProgramParameters()->setNamedConstant("pssmSplitPoints", splitPoints);


	}

	return ret;
#endif
	return MaterialPtr();
}


void SceneTerrain::configureShadows(bool enabled, bool depthShadows)
{
#if 0
	TerrainMaterialGeneratorA::SM2Profile* matProfile = 
		static_cast<TerrainMaterialGeneratorA::SM2Profile*>(terrain_globals_->getDefaultMaterialGenerator()->getActiveProfile());
	matProfile->setReceiveDynamicShadowsEnabled(enabled);
#ifdef SHADOWS_IN_LOW_LOD_MATERIAL
	matProfile->setReceiveDynamicShadowsLowLod(true);
#else
	matProfile->setReceiveDynamicShadowsLowLod(false);
#endif

	// Default materials
	for (EntityList::iterator i = mHouseList.begin(); i != mHouseList.end(); ++i)
	{
		(*i)->setMaterialName("Examples/TudorHouse");
	}

	if (enabled)
	{
		// General scene setup
		mSceneMgr->setShadowTechnique(SHADOWTYPE_TEXTURE_ADDITIVE_INTEGRATED);
		mSceneMgr->setShadowFarDistance(3000);

		// 3 textures per directional light (PSSM)
		mSceneMgr->setShadowTextureCountPerLightType(Ogre::Light::LT_DIRECTIONAL, 3);

		if (pssm_setup_.isNull())
		{
			// shadow camera setup
			PSSMShadowCameraSetup* pssmSetup = new PSSMShadowCameraSetup();
			pssmSetup->setSplitPadding(mCamera->getNearClipDistance());
			pssmSetup->calculateSplitPoints(3, mCamera->getNearClipDistance(), mSceneMgr->getShadowFarDistance());
			pssmSetup->setOptimalAdjustFactor(0, 2);
			pssmSetup->setOptimalAdjustFactor(1, 1);
			pssmSetup->setOptimalAdjustFactor(2, 0.5);

			pssm_setup_.bind(pssmSetup);

		}
		mSceneMgr->setShadowCameraSetup(pssm_setup_);

		if (depthShadows)
		{
			mSceneMgr->setShadowTextureCount(3);
			mSceneMgr->setShadowTextureConfig(0, 2048, 2048, PF_FLOAT32_R);
			mSceneMgr->setShadowTextureConfig(1, 1024, 1024, PF_FLOAT32_R);
			mSceneMgr->setShadowTextureConfig(2, 1024, 1024, PF_FLOAT32_R);
			mSceneMgr->setShadowTextureSelfShadow(true);
			mSceneMgr->setShadowCasterRenderBackFaces(true);
			mSceneMgr->setShadowTextureCasterMaterial("PSSM/shadow_caster");

			MaterialPtr houseMat = buildDepthShadowMaterial("fw12b.jpg");
			for (EntityList::iterator i = mHouseList.begin(); i != mHouseList.end(); ++i)
			{
				(*i)->setMaterial(houseMat);
			}

		}
		else
		{
			mSceneMgr->setShadowTextureCount(3);
			mSceneMgr->setShadowTextureConfig(0, 2048, 2048, PF_X8B8G8R8);
			mSceneMgr->setShadowTextureConfig(1, 1024, 1024, PF_X8B8G8R8);
			mSceneMgr->setShadowTextureConfig(2, 1024, 1024, PF_X8B8G8R8);
			mSceneMgr->setShadowTextureSelfShadow(false);
			mSceneMgr->setShadowCasterRenderBackFaces(false);
			mSceneMgr->setShadowTextureCasterMaterial(StringUtil::BLANK);
		}

		matProfile->setReceiveDynamicShadowsDepth(depthShadows);
		matProfile->setReceiveDynamicShadowsPSSM(static_cast<PSSMShadowCameraSetup*>(pssm_setup_.get()));

	}
	else
	{
		mSceneMgr->setShadowTechnique(SHADOWTYPE_NONE);
	}

#endif
}


void SceneTerrain::setupContent(SceneManager* scene_manager, Light* l)
{
	terrain_globals_ = OGRE_NEW Ogre::TerrainGlobalOptions();

	MaterialManager::getSingleton().setDefaultTextureFiltering(TFO_ANISOTROPIC);
	MaterialManager::getSingleton().setDefaultAnisotropy(7);

	terrain_group_ = OGRE_NEW TerrainGroup(scene_manager, Terrain::ALIGN_X_Z, TERRAIN_SIZE, TERRAIN_WORLD_SIZE);
	terrain_group_->setFilenameConvention(TERRAIN_FILE_PREFIX, TERRAIN_FILE_SUFFIX);
	terrain_group_->setOrigin(terrain_pos_);

	configureTerrainDefaults(scene_manager, l);

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
	TerrainMaterialGeneratorA::SM2Profile* mat_profile = 
	static_cast<TerrainMaterialGeneratorA::SM2Profile*>(terrain_globals_->getDefaultMaterialGenerator()->getActiveProfile());
	mat_profile->setReceiveDynamicShadowsEnabled(false);

	mat_profile->setReceiveDynamicShadowsLowLod(false);
}

void SceneTerrain::shutdown()
{
	OGRE_DELETE terrain_group_;
	OGRE_DELETE terrain_globals_;
}
