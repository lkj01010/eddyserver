#ifndef __Character_H__
#define __Character_H__

#include "Ogre.h"
#include "OIS.h"

namespace CharacterPrivate
{
	const float kTurnSpeed = 500.0f;      // character turning in degrees per second
	const float kAnimFadeSpeed = 15.0f;   // animation crossfade speed in % of full weight per second
	const float kJumpAccel = 40.0f;   // character jump acceleration in upward units per squared second
	const float kGravity = 90.0f;          // gravity in downward units per squared second
	const float kCharHeight	= 5.0f;          // height of character's center of mass above ground

	const int kNumAnims	= 13;           // number of animations the character has
	const int kCamHeight	= 2;           // height of camera above character's center of mass
	const int kRunSpeed		= 17;           // character running speed in units per second
}

class Scene;

class Character
{
private:

	// all the animations our character has, and a null ID
	// some of these affect separate body parts and will be blended together
	enum AnimID
	{
		ANIM_IDLE_BASE,
		ANIM_IDLE_TOP,
		ANIM_RUN_BASE,
		ANIM_RUN_TOP,
		ANIM_HANDS_CLOSED,
		ANIM_HANDS_RELAXED,
		ANIM_DRAW_SWORDS,
		ANIM_SLICE_VERTICAL,
		ANIM_SLICE_HORIZONTAL,
		ANIM_DANCE,
		ANIM_JUMP_START,
		ANIM_JUMP_LOOP,
		ANIM_JUMP_END,
		ANIM_NONE
	};

public:


	Character(Scene* scene);

	void addTime(Ogre::Real deltaTime);

	void injectKeyDown(const OIS::KeyEvent& evt);

	void injectKeyUp(const OIS::KeyEvent& evt);

	void injectMouseMove(const OIS::MouseEvent& evt);

	void injectMouseDown(const OIS::MouseEvent& evt, OIS::MouseButtonID id);

	void injectMouseUp(const OIS::MouseEvent& evt, OIS::MouseButtonID id) {}

private:

	void setupBody();

	void setupAnimations();

	void setupCamera();

	void updateBody(Ogre::Real deltaTime);

	void updateAnimations(Ogre::Real deltaTime);

	void fadeAnimations(Ogre::Real deltaTime);

	void updateCamera(Ogre::Real deltaTime);

	void updateCameraGoal(Ogre::Real deltaYaw, Ogre::Real deltaPitch, Ogre::Real deltaZoom);

	void setBaseAnimation(AnimID id, bool reset = false);

	void setTopAnimation(AnimID id, bool reset = false);

	Scene* scene_;
	Ogre::Camera* camera_;
	Ogre::SceneNode* body_node_;
	Ogre::SceneNode* camera_pivot_;
	Ogre::SceneNode* camera_goal_;
	Ogre::SceneNode* camera_node_;
	Ogre::Real pivot_pitch_;
	Ogre::Entity* body_;
	Ogre::Entity* sword1_;
	Ogre::Entity* sword2_;
	Ogre::RibbonTrail* sword_trail_;
	Ogre::AnimationState* anims_[CharacterPrivate::kNumAnims];    // master animation list
	AnimID base_anim_id_;                   // current base (full- or lower-body) animation
	AnimID top_anim_id_;                    // current top (upper-body) animation
	bool fading_in_[CharacterPrivate::kNumAnims];            // which animations are fading in
	bool fading_out_[CharacterPrivate::kNumAnims];           // which animations are fading out
	bool swords_drawn_;
	bool two_step_jumping_;
	Ogre::Vector3 key_direction_;      // player's local intended direction based on WASD keys
	Ogre::Vector3 goal_direction_;     // actual intended direction in world-space
	Ogre::Real vertical_velocity_;     // for jumping
	Ogre::Real timer_;                // general timer to see how long animations have been playing
};

#endif
