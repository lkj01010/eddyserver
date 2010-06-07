#ifndef CAMERA_CONTROLLER_INC_
#define CAMERA_CONTROLLER_INC_

#include "Ogre.h"
#include "OIS/OIS.h"
#include <limits>

/*=============================================================================
| Utility class for controlling the camera in samples.
=============================================================================*/
class CameraController
{
public:
	enum CameraStyle   // enumerator values for different styles of camera movement
	{
		CS_FREELOOK,
		CS_ORBIT,
		CS_MANUAL
	};

	CameraController(Ogre::Camera* cam);

	virtual ~CameraController() {}

	/*-----------------------------------------------------------------------------
	| Swaps the camera on our camera man for another camera.
	-----------------------------------------------------------------------------*/
	virtual void setCamera(Ogre::Camera* cam)
	{
		camara_ = cam;
	}

	virtual Ogre::Camera* getCamera()
	{
		return camara_;
	}

	/*-----------------------------------------------------------------------------
	| Sets the target we will revolve around. Only applies for orbit style.
	-----------------------------------------------------------------------------*/
	virtual void setTarget(Ogre::SceneNode* target)
	{
		if (target != target_)
		{
			target_ = target;
			if(target)
			{
				setYawPitchDist(Ogre::Degree(0), Ogre::Degree(15), 150);
				camara_->setAutoTracking(true, target_);
			}
			else
			{
				camara_->setAutoTracking(false);
			}

		}
	}

	virtual Ogre::SceneNode* getTarget()
	{
		return target_;
	}

	/*-----------------------------------------------------------------------------
	| Sets the spatial offset from the target. Only applies for orbit style.
	-----------------------------------------------------------------------------*/
	virtual void setYawPitchDist(Ogre::Radian yaw, Ogre::Radian pitch, Ogre::Real dist);

	/*-----------------------------------------------------------------------------
	| Sets the camera's top speed. Only applies for free-look style.
	-----------------------------------------------------------------------------*/
	virtual void setTopSpeed(Ogre::Real topSpeed)
	{
		top_speed_ = topSpeed;
	}

	virtual Ogre::Real getTopSpeed()
	{
		return top_speed_;
	}

	/*-----------------------------------------------------------------------------
	| Sets the movement style of our camera man.
	-----------------------------------------------------------------------------*/
	virtual void setStyle(CameraStyle style);

	virtual CameraStyle getStyle()
	{
		return mStyle;
	}

	/*-----------------------------------------------------------------------------
	| Manually stops the camera when in free-look mode.
	-----------------------------------------------------------------------------*/
	virtual void manualStop();

	virtual bool frameRenderingQueued(const Ogre::FrameEvent& evt);

	/*-----------------------------------------------------------------------------
	| Processes key presses for free-look style movement.
	-----------------------------------------------------------------------------*/
	virtual void injectKeyDown(const OIS::KeyEvent& evt);

	/*-----------------------------------------------------------------------------
	| Processes key releases for free-look style movement.
	-----------------------------------------------------------------------------*/
	virtual void injectKeyUp(const OIS::KeyEvent& evt);

	/*-----------------------------------------------------------------------------
	| Processes mouse movement differently for each style.
	-----------------------------------------------------------------------------*/
	virtual void injectMouseMove(const OIS::MouseEvent& evt);

	/*-----------------------------------------------------------------------------
	| Processes mouse presses. Only applies for orbit style.
	| Left button is for orbiting, and right button is for zooming.
	-----------------------------------------------------------------------------*/
	virtual void injectMouseDown(const OIS::MouseEvent& evt, OIS::MouseButtonID id);

	/*-----------------------------------------------------------------------------
	| Processes mouse releases. Only applies for orbit style.
	| Left button is for orbiting, and right button is for zooming.
	-----------------------------------------------------------------------------*/
	virtual void injectMouseUp(const OIS::MouseEvent& evt, OIS::MouseButtonID id);

protected:

	Ogre::Camera* camara_;
	CameraStyle mStyle;
	Ogre::SceneNode* target_;
	bool orbiting_;
	bool zooming_;
	Ogre::Real top_speed_;
	Ogre::Vector3 velocity_;
	bool going_forward_;
	bool going_back_;
	bool going_left_;
	bool going_right_;
	bool going_up_;
	bool going_down_;
	bool fast_move_;
};
#endif