# UnityProjectionMapping

Some images and an overview of the concept are available on my blog, http://blog.drewmacqu.com/2015/02/projection-mapping-in-unity.html

## Using the software

### Limitations

This project uses OpenCV to calibrate the camera. The OpenCVSharp wrapper was used as it is compatible with Mono. This wrapper depends on specific OpenCV dlls. As a result this project is *Windows only* out of the box. However, only the CalibrateCamera function is used - it would be possible to rewrite this code section to allow use on other platforms.

### Prerequisites

A 3D model of the projection surface is needed. It's important that the model be reasonably accurate as the calibration process can be a bit sensitive.

OpenCV is used. The required DLLs are included in this project in the OpenCVDlls folder. These can be placed in the root folder of the Unity project you’re creating, or in a sensible place on your computer and added to the Windows path variable.

You'll also need the VC11 32-bit redistributable, available from Microsoft: https://www.microsoft.com/en-us/download/confirmation.aspx?id=30679
NB for me it automatically downloads the 64-bit version. You need the 32-bit one. Also on the Microsoft website you need to allow their scripts to run in Chrome - it just works in IE.

### Project setup

Download and import the projection mapping package (UnityProjectionMapping.unitypackage in this repository).

Create an empty GameObject and attach the CorrespondenceAcquisition.cs script to it. The script had two fields that need to be completed:

1. Drag your main camera onto the object's "Main camera" field in the Inspector tab.
2. Drag a crosshair image onto the object’s “Crosshair texture” field. A crosshair image is included in the package, it’s called crosshair.png

Create a tag named “CalibrationSphere”. Unity packages do not export tags so it needs to be created manually.

*Optional*: To help with locating your cursor in the projector screen, it’s useful to have full screen crosshairs. Due to Unity’s rendering pipeline it was necessary for the script that generates these lines to be attached to the main camera. Therefore, add the script DrawLine.cs to your main camera game object.

### Calibration

The system needs to be calibrated so the virtual camera in Unity mimics the properties of the physical projector. This is achieved by defining correspondences between 3D points on the model and its respective point in the projected image.

Start the game, either in the editor or as a standalone build. The game view needs to be being displayed by your projector onto the physical model. For ease I tend to have my desktop mirrored so I can use both the projector and the monitor for calibration.

A correspondence between a 3D point and it’s projection on the 2D image plane is defined as follow:

1. Click a point on the 3D object. This will create a sphere at that point on the model and the screen will become partially opaque. *Tip*: if this doesn’t happen, your object is required to have a collider for ray casting to work. A mesh collider would be required for imported objects - less fine grained colliders would not work.
2. Looking at the image that’s being displayed by your projector. Click on where that newly created sphere is on your physical model. This will create a small crosshair marker at that point. These can be moved by hovering over them and dragging.

This needs to be done at least seven times. After the seventh correspondence has been defined, the extrinsic and intrinsic matrices of the projector will be applied to the Unity camera. At this time the projection should be aligned to the physical object. More points can be added and existing points moved to achieve a better alignment.

You can hide the calibration markers (spheres and crosshairs) by pressing the space bar.

*Tips*: If you get exceptions (and the object disappears in the game view) after 7 points have been defined, something has gone wrong with the calibration. The calibration process is very sensitive to errors, and sometimes just fails. Try defining the points again, being very careful to avoid errors. Choose points that can be identified accurately on the model, such as corners. If you’ve tried it a few times with no luck, it’s likely your virtual model does not accurately reflect the physical model. Try measuring it up again and ensure all the dimensions are correct. Additionally, each point needs to contribute something to the optimisation (e.g. there needs to be linear independence between points). Try choosing points far apart that capture the full shape of the surface, not multiple points on the same plane.

### Saving and loading calibrations

It can be time consuming to specify the correspondences needed to calibrate a projector. It is possible to save correspondences so you don't need to do it every time you restart the applications.

To use the points recorder, create a new empty game object and add the PointsRecordReplay.cs script to it. In the Inspector tab, drag your CorrespondenceAcquisition object (whatever game object you attached the CorrespondenceAcquisition.cs script to) onto the "Points Holder" field.

While the game is running, you can press "R" to record/save your points, and "P" to play/load your points. When playing in the editor, you can choose a different file name to store your points. In standalone mode, the file name "recording.xml" is used.

## Example unity project

This repository contains an example Unity project that shows how the files need to be used (as described above). It contains a perfect cube object that could be mapped onto a physical cube. Seven good points for the cube would be the seven visible vertices.
