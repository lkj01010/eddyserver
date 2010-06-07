#include "camera_controller.h"

CameraController::CameraController(Ogre::Camera* cam)
: camara_(0)
, target_(0)
, orbiting_(false)
, zooming_(false)
, top_speed_(150)
, velocity_(Ogre::Vector3::ZERO)
, going_forward_(false)
, going_back_(false)
, going_left_(false)
, going_right_(false)
, going_up_(false)
, going_down_(false)
, fast_move_(false)
{

	setCamera(cam);
	setStyle(CS_FREELOOK);
}

void CameraController::setYawPitchDist(Ogre::Radian yaw, Ogre::Radian pitch, Ogre::Real dist)
{
	camara_->setPosition(target_->_getDerivedPosition());
	camara_->setOrientation(target_->_getDerivedOrientation());
	camara_->yaw(yaw);
	camara_->pitch(-pitch);
	camara_->moveRelative(Ogre::Vector3(0, 0, dist));
}

void CameraController::setStyle(CameraStyle style)
{
	if (mStyle != CS_ORBIT && style == CS_ORBIT)
	{
		setTarget(target_ ? target_ : camara_->getSceneManager()->getRootSceneNode());
		camara_->setFixedYawAxis(true);
		manualStop();
		setYawPitchDist(Ogre::Degree(0), Ogre::Degree(15), 150);

	}
	else if (mStyle != CS_FREELOOK && style == CS_FREELOOK)
	{
		camara_->setAutoTracking(false);
		camara_->setFixedYawAxis(true);
	}
	else if (mStyle != CS_MANUAL && style == CS_MANUAL)
	{
		camara_->setAutoTracking(false);
		manualStop();
	}
	mStyle = style;

}

void CameraController::manualStop()
{
	if (mStyle == CS_FREELOOK)
	{
		going_forward_ = false;
		going_back_ = false;
		going_left_ = false;
		going_right_ = false;
		going_up_ = false;
		going_down_ = false;
		velocity_ = Ogre::Vector3::ZERO;
	}
}

bool CameraController::frameRenderingQueued(const Ogre::FrameEvent& evt)
{
	if (mStyle == CS_FREELOOK)
	{
		// build our acceleration vector based on keyboard input composite
		Ogre::Vector3 accel = Ogre::Vector3::ZERO;
		if (going_forward_) accel += camara_->getDirection();
		if (going_back_) accel -= camara_->getDirection();
		if (going_right_) accel += camara_->getRight();
		if (going_left_) accel -= camara_->getRight();
		if (going_up_) accel += camara_->getUp();
		if (going_down_) accel -= camara_->getUp();

		// if accelerating, try to reach top speed in a certain time
		Ogre::Real topSpeed = fast_move_ ? top_speed_ * 20 : top_speed_;
		if (accel.squaredLength() != 0)
		{
			accel.normalise();
			velocity_ += accel * topSpeed * evt.timeSinceLastFrame * 10;
		}
		// if not accelerating, try to stop in a certain time
		else velocity_ -= velocity_ * evt.timeSinceLastFrame * 10;

		Ogre::Real tooSmall = std::numeric_limits<Ogre::Real>::epsilon();

		// keep camera velocity below top speed and above epsilon
		if (velocity_.squaredLength() > topSpeed * topSpeed)
		{
			velocity_.normalise();
			velocity_ *= topSpeed;
		}
		else if (velocity_.squaredLength() < tooSmall * tooSmall)
			velocity_ = Ogre::Vector3::ZERO;

		if (velocity_ != Ogre::Vector3::ZERO) camara_->move(velocity_ * evt.timeSinceLastFrame);
	}

	return true;
}

void CameraController::injectKeyDown(const OIS::KeyEvent& evt)
	{
		if (mStyle == CS_FREELOOK)
		{
			if (evt.key == OIS::KC_W || evt.key == OIS::KC_UP) going_forward_ = true;
			else if (evt.key == OIS::KC_S || evt.key == OIS::KC_DOWN) going_back_ = true;
			else if (evt.key == OIS::KC_A || evt.key == OIS::KC_LEFT) going_left_ = true;
			else if (evt.key == OIS::KC_D || evt.key == OIS::KC_RIGHT) going_right_ = true;
			else if (evt.key == OIS::KC_PGUP) going_up_ = true;
			else if (evt.key == OIS::KC_PGDOWN) going_down_ = true;
			else if (evt.key == OIS::KC_LSHIFT) fast_move_ = true;
		}
	}

	/*-----------------------------------------------------------------------------
	| Processes key releases for free-look style movement.
	-----------------------------------------------------------------------------*/
void CameraController::injectKeyUp(const OIS::KeyEvent& evt)
{
	if (mStyle == CS_FREELOOK)
	{
		if (evt.key == OIS::KC_W || evt.key == OIS::KC_UP) going_forward_ = false;
		else if (evt.key == OIS::KC_S || evt.key == OIS::KC_DOWN) going_back_ = false;
		else if (evt.key == OIS::KC_A || evt.key == OIS::KC_LEFT) going_left_ = false;
		else if (evt.key == OIS::KC_D || evt.key == OIS::KC_RIGHT) going_right_ = false;
		else if (evt.key == OIS::KC_PGUP) going_up_ = false;
		else if (evt.key == OIS::KC_PGDOWN) going_down_ = false;
		else if (evt.key == OIS::KC_LSHIFT) fast_move_ = false;
	}
}

/*-----------------------------------------------------------------------------
| Processes mouse movement differently for each style.
-----------------------------------------------------------------------------*/
void CameraController::injectMouseMove(const OIS::MouseEvent& evt)
{
	if (mStyle == CS_ORBIT)
	{
		Ogre::Real dist = (camara_->getPosition() - target_->_getDerivedPosition()).length();

		if (orbiting_)   // yaw around the target, and pitch locally
		{
			camara_->setPosition(target_->_getDerivedPosition());

			camara_->yaw(Ogre::Degree(-evt.state.X.rel * 0.25f));
			camara_->pitch(Ogre::Degree(-evt.state.Y.rel * 0.25f));

			camara_->moveRelative(Ogre::Vector3(0, 0, dist));

			// don't let the camera go over the top or around the bottom of the target
		}
		else if (zooming_)  // move the camera toward or away from the target
		{
			// the further the camera is, the faster it moves
			camara_->moveRelative(Ogre::Vector3(0, 0, evt.state.Y.rel * 0.004f * dist));
		}
		else if (evt.state.Z.rel != 0)  // move the camera toward or away from the target
		{
			// the further the camera is, the faster it moves
			camara_->moveRelative(Ogre::Vector3(0, 0, -evt.state.Z.rel * 0.0008f * dist));
		}
	}
	else if (mStyle == CS_FREELOOK)
	{
		camara_->yaw(Ogre::Degree(-evt.state.X.rel * 0.15f));
		camara_->pitch(Ogre::Degree(-evt.state.Y.rel * 0.15f));
	}
}

/*-----------------------------------------------------------------------------
| Processes mouse presses. Only applies for orbit style.
| Left button is for orbiting, and right button is for zooming.
-----------------------------------------------------------------------------*/
void CameraController::injectMouseDown(const OIS::MouseEvent& evt, OIS::MouseButtonID id)
{
	if (mStyle == CS_ORBIT)
	{
		if (id == OIS::MB_Left) orbiting_ = true;
		else if (id == OIS::MB_Right) zooming_ = true;
	}
}

/*-----------------------------------------------------------------------------
| Processes mouse releases. Only applies for orbit style.
| Left button is for orbiting, and right button is for zooming.
-----------------------------------------------------------------------------*/
void CameraController::injectMouseUp(const OIS::MouseEvent& evt, OIS::MouseButtonID id)
{
	if (mStyle == CS_ORBIT)
	{
		if (id == OIS::MB_Left) orbiting_ = false;
		else if (id == OIS::MB_Right) zooming_ = false;
	}
}