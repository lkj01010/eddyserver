#include "character.h"
#include "scene.h"

#include "OGRE/Terrain/OgreTerrain.h"
#include "OGRE/Terrain/OgreTerrainGroup.h"

using namespace Ogre;
using namespace CharacterPrivate;

Character::Character(Scene* scene)
{
	scene_ = scene;
	setupBody();
	setupCamera();
	setupAnimations();
}

void Character::addTime(Real deltaTime)
{
	updateBody(deltaTime);
	updateAnimations(deltaTime);
	updateCamera(deltaTime);
}

void Character::injectKeyDown(const OIS::KeyEvent& evt)
{
	if (evt.key == OIS::KC_X && (top_anim_id_ == ANIM_IDLE_TOP || top_anim_id_ == ANIM_RUN_TOP))
	{
		// take swords out (or put them back, since it's the same animation but reversed)
		setTopAnimation(ANIM_DRAW_SWORDS, true);
		timer_ = 0;
	}
	else if (evt.key == OIS::KC_E && !swords_drawn_)
	{
		if (top_anim_id_ == ANIM_IDLE_TOP || top_anim_id_ == ANIM_RUN_TOP)
		{
			// start dancing
			setBaseAnimation(ANIM_DANCE, true);
			setTopAnimation(ANIM_NONE);
			// disable hand animation because the dance controls hands
			anims_[ANIM_HANDS_RELAXED]->setEnabled(false);
		}
		else if (base_anim_id_ == ANIM_DANCE)
		{
			// stop dancing
			setBaseAnimation(ANIM_IDLE_BASE);
			setTopAnimation(ANIM_IDLE_TOP);
			// re-enable hand animation
			anims_[ANIM_HANDS_RELAXED]->setEnabled(true);
		}
	}

	// keep track of the player's intended direction
	else if (evt.key == OIS::KC_W) key_direction_.z = -1;
	else if (evt.key == OIS::KC_A) key_direction_.x = -1;
	else if (evt.key == OIS::KC_S) key_direction_.z = 1;
	else if (evt.key == OIS::KC_D) key_direction_.x = 1;

	else if (evt.key == OIS::KC_SPACE )
	{
		if (top_anim_id_ == ANIM_IDLE_TOP || top_anim_id_ == ANIM_RUN_TOP)
		{
			// jump if on ground
			setBaseAnimation(ANIM_JUMP_START, true);
			setTopAnimation(ANIM_NONE);
			timer_ = 0;
		} 
		else if (base_anim_id_ == ANIM_JUMP_LOOP && !two_step_jumping_) 
		{
			two_step_jumping_ = true;
			setBaseAnimation(ANIM_JUMP_START, false);
			setTopAnimation(ANIM_NONE);
			timer_ = 0;
		}
	}

	if (!key_direction_.isZeroLength() && base_anim_id_ == ANIM_IDLE_BASE)
	{
		// start running if not already moving and the player wants to move
		setBaseAnimation(ANIM_RUN_BASE, true);
		if (top_anim_id_ == ANIM_IDLE_TOP) setTopAnimation(ANIM_RUN_TOP, true);
	}
}

void Character::injectKeyUp(const OIS::KeyEvent& evt)
{
	// keep track of the player's intended direction
	if (evt.key == OIS::KC_W && key_direction_.z == -1) key_direction_.z = 0;
	else if (evt.key == OIS::KC_A && key_direction_.x == -1) key_direction_.x = 0;
	else if (evt.key == OIS::KC_S && key_direction_.z == 1) key_direction_.z = 0;
	else if (evt.key == OIS::KC_D && key_direction_.x == 1) key_direction_.x = 0;

	if (key_direction_.isZeroLength() && base_anim_id_ == ANIM_RUN_BASE)
	{
		// stop running if already moving and the player doesn't want to move
		setBaseAnimation(ANIM_IDLE_BASE);
		if (top_anim_id_ == ANIM_RUN_TOP) setTopAnimation(ANIM_IDLE_TOP);
	}
}

void Character::injectMouseMove(const OIS::MouseEvent& evt)
{
	// update camera goal based on mouse movement
	updateCameraGoal(-0.05f * evt.state.X.rel, -0.05f * evt.state.Y.rel, -0.0005f * evt.state.Z.rel);
}

void Character::injectMouseDown(const OIS::MouseEvent& evt, OIS::MouseButtonID id)
{
	if (swords_drawn_ && (top_anim_id_ == ANIM_IDLE_TOP || top_anim_id_ == ANIM_RUN_TOP))
	{
		// if swords are out, and character's not doing something weird, then SLICE!
		if (id == OIS::MB_Left) setTopAnimation(ANIM_SLICE_VERTICAL, true);
		else if (id == OIS::MB_Right) setTopAnimation(ANIM_SLICE_HORIZONTAL, true);
		timer_ = 0;
	}
}

void Character::setupBody()
{
	SceneManager* sceneMgr = scene_->scene_manager();
	// create main model
	body_node_ = sceneMgr->getRootSceneNode()->createChildSceneNode(Vector3::UNIT_Y * kCharHeight);
	body_ = sceneMgr->createEntity("SinbadBody", "Sinbad.mesh");
	body_node_->attachObject(body_);
	Ray ray;
	ray.setOrigin(Vector3(0, 10000, 0));
	ray.setDirection(Vector3::NEGATIVE_UNIT_Y);

	TerrainGroup::RayResult ray_result = scene_->terrain_group()->rayIntersects(ray);

	body_node_->setPosition(ray_result.position + Vector3(0, kCharHeight, 10));

	// create swords and attach to sheath
	sword1_ = sceneMgr->createEntity("SinbadSword1", "Sword.mesh");
	sword2_ = sceneMgr->createEntity("SinbadSword2", "Sword.mesh");
	body_->attachObjectToBone("Sheath.L", sword1_);
	body_->attachObjectToBone("Sheath.R", sword2_);

	// create a couple of ribbon trails for the swords, just for fun
	NameValuePairList params;
	params["numberOfChains"] = "2";
	params["maxElements"] = "80";
	sword_trail_ = (RibbonTrail*)sceneMgr->createMovableObject("RibbonTrail", &params);
	sword_trail_->setMaterialName("LightRibbonTrail");
	sword_trail_->setTrailLength(20);
	sword_trail_->setVisible(false);
	sword_trail_->setCastShadows(false);
	sceneMgr->getRootSceneNode()->attachObject(sword_trail_);


	for (int i = 0; i < 2; i++)
	{
		sword_trail_->setInitialColour(i, 1, 0.8, 0);
		sword_trail_->setColourChange(i, 0.75, 1.25, 1.25, 1.25);
		sword_trail_->setWidthChange(i, 1);
		sword_trail_->setInitialWidth(i, 0.5);
	}

	key_direction_ = Vector3::ZERO;
	vertical_velocity_ = 0;

	Pass* pass = body_->getSubEntity(0)->getMaterial()->getBestTechnique()->getPass(0);
	//assert(pass->hasVertexProgram());
	//assert(pass->getVertexProgram()->isSkeletalAnimationIncluded());
}

void Character::setupAnimations()
{
	// this is very important due to the nature of the exported animations
	body_->getSkeleton()->setBlendMode(ANIMBLEND_CUMULATIVE);

	String animNames[] =
	{"IdleBase", "IdleTop", "RunBase", "RunTop", "HandsClosed", "HandsRelaxed", "DrawSwords",
	"SliceVertical", "SliceHorizontal", "Dance", "JumpStart", "JumpLoop", "JumpEnd"};

	// populate our animation list
	for (int i = 0; i < kNumAnims; i++)
	{
		anims_[i] = body_->getAnimationState(animNames[i]);
		anims_[i]->setLoop(true);
		fading_in_[i] = false;
		fading_out_[i] = false;
	}

	// start off in the idle state (top and bottom together)
	setBaseAnimation(ANIM_IDLE_BASE);
	setTopAnimation(ANIM_IDLE_TOP);

	// relax the hands since we're not holding anything
	anims_[ANIM_HANDS_RELAXED]->setEnabled(true);

	swords_drawn_ = false;
	two_step_jumping_ = false;
}

void Character::setupCamera()
{

	Camera* cam = scene_->camera();
	// create a pivot at roughly the character's shoulder
	camera_pivot_ = cam->getSceneManager()->getRootSceneNode()->createChildSceneNode();
	// this is where the camera should be soon, and it spins around the pivot
	camera_goal_ = camera_pivot_->createChildSceneNode(Vector3(0, 0, 15));
	// this is where the camera actually is
	camera_node_ = cam->getSceneManager()->getRootSceneNode()->createChildSceneNode();
	camera_node_->setPosition(camera_pivot_->getPosition() + camera_goal_->getPosition());

	camera_pivot_->setFixedYawAxis(true);
	camera_goal_->setFixedYawAxis(true);
	camera_node_->setFixedYawAxis(true);

	// our model is quite small, so reduce the clipping planes
	camera_node_->attachObject(cam);

	pivot_pitch_ = 0;
}

void Character::updateBody(Real deltaTime)
{
	goal_direction_ = Vector3::ZERO;   // we will calculate this

	if (key_direction_ != Vector3::ZERO && base_anim_id_ != ANIM_DANCE)
	{
		// calculate actually goal direction in world based on player's key directions
		goal_direction_ += key_direction_.z * camera_node_->getOrientation().zAxis();
		goal_direction_ += key_direction_.x * camera_node_->getOrientation().xAxis();
		goal_direction_.y = 0;
		goal_direction_.normalise();

		Quaternion toGoal = body_node_->getOrientation().zAxis().getRotationTo(goal_direction_);

		// calculate how much the character has to turn to face goal direction
		Real yawToGoal = toGoal.getYaw().valueDegrees();
		// this is how much the character CAN turn this frame
		Real yawAtSpeed = yawToGoal / Math::Abs(yawToGoal) * deltaTime * kTurnSpeed;
		// reduce "turnability" if we're in midair
		if (base_anim_id_ == ANIM_JUMP_LOOP) yawAtSpeed *= 0.2f;

		// turn as much as we can, but not more than we need to
		if (yawToGoal < 0) yawToGoal = std::min<Real>(0, std::max<Real>(yawToGoal, yawAtSpeed)); //yawToGoal = Math::Clamp<Real>(yawToGoal, yawAtSpeed, 0);
		else if (yawToGoal > 0) yawToGoal = std::max<Real>(0, std::min<Real>(yawToGoal, yawAtSpeed)); //yawToGoal = Math::Clamp<Real>(yawToGoal, 0, yawAtSpeed);

		body_node_->yaw(Degree(yawToGoal));

		// move in current body direction (not the goal direction)
		body_node_->translate(0, 0, deltaTime * kRunSpeed * anims_[base_anim_id_]->getWeight(),
			Node::TS_LOCAL);

		if (base_anim_id_ != ANIM_JUMP_LOOP && base_anim_id_ != ANIM_JUMP_START) {
			Vector3 pos = body_node_->getPosition();
			Ray ray;
			ray.setOrigin(Vector3(pos.x, 10000, pos.z));
			ray.setDirection(Vector3::NEGATIVE_UNIT_Y);

			TerrainGroup::RayResult ray_result = scene_->terrain_group()->rayIntersects(ray);

			if (ray_result.hit)
				body_node_->setPosition(pos.x, ray_result.position.y + kCharHeight, pos.z);
		}
	}

	if (base_anim_id_ == ANIM_JUMP_LOOP)
	{
		// if we're jumping, add a vertical offset too, and apply gravity
		body_node_->translate(0, vertical_velocity_ * deltaTime, 0, Node::TS_LOCAL);
		vertical_velocity_ -= kGravity * deltaTime;

		Vector3 pos = body_node_->getPosition();
		Ray ray;
		ray.setOrigin(Vector3(pos.x, 10000, pos.z));
		ray.setDirection(Vector3::NEGATIVE_UNIT_Y);

		TerrainGroup::RayResult ray_result = scene_->terrain_group()->rayIntersects(ray);

		if (pos.y <= ray_result.position.y + kCharHeight && vertical_velocity_ <= 0)
		{
			// if we've hit the ground, change to landing state
			pos.y = ray_result.position.y + kCharHeight;
			body_node_->setPosition(pos);
			setBaseAnimation(ANIM_JUMP_END, true);
			timer_ = 0;
			two_step_jumping_ = false;
		}
	}
}

void Character::updateAnimations(Real deltaTime)
{
	Real baseAnimSpeed = 1;
	Real topAnimSpeed = 1;

	timer_ += deltaTime;

	if (top_anim_id_ == ANIM_DRAW_SWORDS)
	{
		// flip the draw swords animation if we need to put it back
		topAnimSpeed = swords_drawn_ ? -1 : 1;

		// half-way through the animation is when the hand grasps the handles...
		if (timer_ >= anims_[top_anim_id_]->getLength() / 2 &&
			timer_ - deltaTime < anims_[top_anim_id_]->getLength() / 2)
		{
			// so transfer the swords from the sheaths to the hands
			body_->detachAllObjectsFromBone();
			body_->attachObjectToBone(swords_drawn_ ? "Sheath.L" : "Handle.L", sword1_);
			body_->attachObjectToBone(swords_drawn_ ? "Sheath.R" : "Handle.R", sword2_);
			// change the hand state to grab or let go
			anims_[ANIM_HANDS_CLOSED]->setEnabled(!swords_drawn_);
			anims_[ANIM_HANDS_RELAXED]->setEnabled(swords_drawn_);

			// toggle sword trails
			if (swords_drawn_)
			{
				sword_trail_->setVisible(false);
				sword_trail_->removeNode(sword1_->getParentNode());
				sword_trail_->removeNode(sword2_->getParentNode());
			}
			else
			{
				sword_trail_->setVisible(true);
				sword_trail_->addNode(sword1_->getParentNode());
				sword_trail_->addNode(sword2_->getParentNode());
			}
		}

		if (timer_ >= anims_[top_anim_id_]->getLength())
		{
			// animation is finished, so return to what we were doing before
			if (base_anim_id_ == ANIM_IDLE_BASE) setTopAnimation(ANIM_IDLE_TOP);
			else
			{
				setTopAnimation(ANIM_RUN_TOP);
				anims_[ANIM_RUN_TOP]->setTimePosition(anims_[ANIM_RUN_BASE]->getTimePosition());
			}
			swords_drawn_ = !swords_drawn_;
		}
	}
	else if (top_anim_id_ == ANIM_SLICE_VERTICAL || top_anim_id_ == ANIM_SLICE_HORIZONTAL)
	{
		if (timer_ >= anims_[top_anim_id_]->getLength())
		{
			// animation is finished, so return to what we were doing before
			if (base_anim_id_ == ANIM_IDLE_BASE) setTopAnimation(ANIM_IDLE_TOP);
			else
			{
				setTopAnimation(ANIM_RUN_TOP);
				anims_[ANIM_RUN_TOP]->setTimePosition(anims_[ANIM_RUN_BASE]->getTimePosition());
			}
		}

		// don't sway hips from side to side when slicing. that's just embarrasing.
		if (base_anim_id_ == ANIM_IDLE_BASE) baseAnimSpeed = 0;
	}
	else if (base_anim_id_ == ANIM_JUMP_START)
	{
		if (timer_ >= anims_[base_anim_id_]->getLength())
		{
			// takeoff animation finished, so time to leave the ground!
			setBaseAnimation(ANIM_JUMP_LOOP, true);
			// apply a jump acceleration to the character
			vertical_velocity_ = kJumpAccel;
		}
	}
	else if (base_anim_id_ == ANIM_JUMP_END)
	{
		if (timer_ >= anims_[base_anim_id_]->getLength())
		{
			// safely landed, so go back to running or idling
			if (key_direction_ == Vector3::ZERO)
			{
				setBaseAnimation(ANIM_IDLE_BASE);
				setTopAnimation(ANIM_IDLE_TOP);
			}
			else
			{
				setBaseAnimation(ANIM_RUN_BASE, true);
				setTopAnimation(ANIM_RUN_TOP, true);
			}
		}
	}

	// increment the current base and top animation times
	if (base_anim_id_ != ANIM_NONE) anims_[base_anim_id_]->addTime(deltaTime * baseAnimSpeed);
	if (top_anim_id_ != ANIM_NONE) anims_[top_anim_id_]->addTime(deltaTime * topAnimSpeed);

	// apply smooth transitioning between our animations
	fadeAnimations(deltaTime);
}

void Character::fadeAnimations(Real deltaTime)
{
	for (int i = 0; i < kNumAnims; i++)
	{
		if (fading_in_[i])
		{
			// slowly fade this animation in until it has full weight
			Real newWeight = anims_[i]->getWeight() + deltaTime * kAnimFadeSpeed;
			anims_[i]->setWeight(Math::Clamp<Real>(newWeight, 0, 1));
			if (newWeight >= 1) fading_in_[i] = false;
		}
		else if (fading_out_[i])
		{
			// slowly fade this animation out until it has no weight, and then disable it
			Real newWeight = anims_[i]->getWeight() - deltaTime * kAnimFadeSpeed;
			anims_[i]->setWeight(Math::Clamp<Real>(newWeight, 0, 1));
			if (newWeight <= 0)
			{
				anims_[i]->setEnabled(false);
				fading_out_[i] = false;
			}
		}
	}
}

void Character::updateCamera(Real deltaTime)
{

	// place the camera pivot roughly at the character's shoulder
	camera_pivot_->setPosition(body_node_->getPosition() + Vector3::UNIT_Y * kCamHeight);
	// move the camera smoothly to the goal
	Vector3 goalOffset = camera_goal_->_getDerivedPosition() - camera_node_->getPosition();
	camera_node_->translate(goalOffset * deltaTime * 9.0f);
	// always look at the pivot
	Vector3 pos = camera_node_->_getDerivedPosition();
	Ogre::Real height = scene_->terrain_group()->getHeightAtWorldPosition(pos);
	height += kCamHeight;
	if (height > pos.y)
	{
		pos.y = height;
		camera_node_->_setDerivedPosition(pos);
	}
	camera_node_->lookAt(camera_pivot_->_getDerivedPosition(), Node::TS_WORLD);
}

void Character::updateCameraGoal(Real deltaYaw, Real deltaPitch, Real deltaZoom)
{
	camera_pivot_->yaw(Degree(deltaYaw), Node::TS_WORLD);

	// bound the pitch
	if (!(pivot_pitch_ + deltaPitch > 25 && deltaPitch > 0) &&
		!(pivot_pitch_ + deltaPitch < -60 && deltaPitch < 0))
	{
		camera_pivot_->pitch(Degree(deltaPitch), Node::TS_LOCAL);
		pivot_pitch_ += deltaPitch;
	}

	Real dist = camera_goal_->_getDerivedPosition().distance(camera_pivot_->_getDerivedPosition());
	Real distChange = deltaZoom * dist;

	// bound the zoom
	if (!(dist + distChange < 8 && distChange < 0) &&
		!(dist + distChange > 70 && distChange > 0))
	{
		camera_goal_->translate(0, 0, distChange, Node::TS_LOCAL);
	}
}

void Character::setBaseAnimation(AnimID id, bool reset)
{
	if (base_anim_id_ >= 0 && base_anim_id_ < kNumAnims)
	{
		// if we have an old animation, fade it out
		fading_in_[base_anim_id_] = false;
		fading_out_[base_anim_id_] = true;
	}

	base_anim_id_ = id;

	if (id != ANIM_NONE)
	{
		// if we have a new animation, enable it and fade it in
		anims_[id]->setEnabled(true);
		anims_[id]->setWeight(0);
		fading_out_[id] = false;
		fading_in_[id] = true;
		if (reset) anims_[id]->setTimePosition(0);
	}
}

void Character::setTopAnimation(AnimID id, bool reset)
{
	if (top_anim_id_ >= 0 && top_anim_id_ < kNumAnims)
	{
		// if we have an old animation, fade it out
		fading_in_[top_anim_id_] = false;
		fading_out_[top_anim_id_] = true;
	}

	top_anim_id_ = id;

	if (id != ANIM_NONE)
	{
		// if we have a new animation, enable it and fade it in
		anims_[id]->setEnabled(true);
		anims_[id]->setWeight(0);
		fading_out_[id] = false;
		fading_in_[id] = true;
		if (reset) anims_[id]->setTimePosition(0);
	}
}
