using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Assets;

public class Calibration {
    // NB some code adapted from OpenCVSharp camera calibration example:
    // https://github.com/shimat/opencvsharp/blob/master/sample/CStyleSamplesCS/Samples/CalibrateCamera.cs

    private Camera _mainCamera;
    private static int _numImages = 1;
    private double height;
    private double width;

    public Calibration(Camera mainCamera)
    {
        _mainCamera = mainCamera;
        height = (double)Screen.height;
        width = (double)Screen.width;
    }

    public double calibrateFromCorrespondences(List<Vector3> imagePoints, List<Vector3> worldPoints)
    {
        int numPoints = imagePoints.Count;
        CvMat pointCounts = CreatePointCountMatrix(numPoints);
        CvMat imagePointsCvMat = PutImagePointsIntoCVMat(imagePoints, numPoints);
        CvMat worldPointsCvMat = PutObjectPointsIntoCVMat(worldPoints, numPoints, _numImages);
        Size size = new Size(width, height);
        CvMat distortion = new CvMat(1, 4, MatrixType.F64C1);
        CvMat rotation = new CvMat(1, 3, MatrixType.F32C1);
        CvMat translation = new CvMat(1, 3, MatrixType.F32C1);
        CvMat intrinsic = CreateIntrinsicGuess(height, width);
        var flags = CalibrationFlag.ZeroTangentDist | CalibrationFlag.UseIntrinsicGuess | 
            CalibrationFlag.FixK1 | CalibrationFlag.FixK2 | CalibrationFlag.FixK3 | CalibrationFlag.FixK4 | CalibrationFlag.FixK5 | CalibrationFlag.FixK6;

        Cv.CalibrateCamera2(worldPointsCvMat, imagePointsCvMat, pointCounts, size, intrinsic, distortion, rotation, translation,  flags);

        ApplyCalibrationToUnityCamera(intrinsic, rotation, translation);

        return CalculateReprojectionError(imagePoints, numPoints, imagePointsCvMat, worldPointsCvMat, intrinsic, distortion, rotation, translation);
    }

    private void ApplyCalibrationToUnityCamera(CvMat intrinsic, CvMat rotation, CvMat translation)
    {
        CvMat rotationInverse = GetRotationMatrixFromRotationVector(rotation).Transpose(); // transpose is same as inverse for rotation matrix
        CvMat transFinal = (rotationInverse * -1) * translation.Transpose();
        _mainCamera.projectionMatrix = LoadProjectionMatrix((float)intrinsic[0, 0], (float)intrinsic[1, 1], (float)intrinsic[0, 2], (float)intrinsic[1, 2]);
        ApplyTranslationAndRotationToCamera(transFinal, RotationConversion.RotationMatrixToEulerZXY(rotationInverse));
    }

    private static CvMat CreatePointCountMatrix(int numPoints)
    {
        int[] pointCountsValue = new int[_numImages];
        pointCountsValue[0] = numPoints;
        CvMat pointCounts = new CvMat(_numImages, 1, MatrixType.S32C1, pointCountsValue);
        return pointCounts;
    }

    private static CvMat GetRotationMatrixFromRotationVector(CvMat rotation_)
    {
        CvMat rotationFull = new CvMat(3, 3, MatrixType.F32C1);
        // get full rotation matrix from rotation vector
        Cv.Rodrigues2(rotation_, rotationFull); 
        float[] LHSflipBackMatrix = new float[] { 1.0f, 0.0f, 0.0f, 
            0.0f, 1.0f, 0.0f, 
            0.0f, 0.0f, -1.0f };
        CvMat LHSflipBackMatrixM = new CvMat(3, 3, MatrixType.F32C1, LHSflipBackMatrix);
        CvMat rotLHS = rotationFull * LHSflipBackMatrixM; // invert Z (as we did before when savings points) to get from RHS -> LHS
        return rotLHS;
    }

    private double CalculateReprojectionError(List<Vector3> _imagePositions, int pointsCount, CvMat imagePoints, CvMat objectPoints, CvMat intrinsic, CvMat distortion, CvMat rotation_, CvMat translation_)
    {
        // openCV SSD taken from http://stackoverflow.com/questions/23781089/opencv-calibratecamera-2-reprojection-error-and-custom-computed-one-not-agree

        CvMat imagePointsOut = PutImagePointsIntoCVMat(_imagePositions, pointsCount); // will be overwritten, but should be correct size. Hacky and slow imp
        Cv.ProjectPoints2(objectPoints, rotation_, translation_, intrinsic, distortion, imagePointsOut);

        var ar1 = imagePoints.ToArray();
        var ar2 = imagePointsOut.ToArray();
        double diff = 0.0f;
        for (int i = 0; i < pointsCount; i++)
        {
            diff = diff + System.Math.Pow(System.Math.Abs(ar1[i].Val0 - ar2[i].Val0), 2.0) + System.Math.Pow(System.Math.Abs(ar1[i].Val1 - ar2[i].Val1), 2.0);
        }
        diff = System.Math.Sqrt(diff / pointsCount);
        return diff;
    }

    private Matrix4x4 LoadProjectionMatrix(float fx, float fy, float cx, float cy)
    {
        // https://github.com/kylemcdonald/ofxCv/blob/88620c51198fc3992fdfb5c0404c37da5855e1e1/libs/ofxCv/src/Calibration.cpp
        float w = _mainCamera.pixelWidth;
        float h = _mainCamera.pixelHeight;
        float nearDist = _mainCamera.nearClipPlane;
        float farDist = _mainCamera.farClipPlane;

        return MakeFrustumMatrix(
            nearDist * (-cx) / fx, nearDist * (w - cx) / fx,
            nearDist * (cy) / fy, nearDist * (cy - h) / fy,
            nearDist, farDist);
    }

    private Matrix4x4 MakeFrustumMatrix(float left, float right,
                                        float bottom, float top,
                                        float zNear, float zFar)
    {
        // https://github.com/openframeworks/openFrameworks/blob/master/libs/openFrameworks/math/ofMatrix4x4.cpp
        // note transpose of ofMatrix4x4 wr.t OpenGL documentation, since the OSG use post multiplication rather than pre.
        // NB this has been transposed here from the original openframeworks code

        float A = (right + left) / (right - left);
        float B = (top + bottom) / (top - bottom);
        float C = -(zFar + zNear) / (zFar - zNear);
        float D = -2.0f * zFar * zNear / (zFar - zNear);

        var persp = new Matrix4x4();
        persp[0, 0] = 2.0f * zNear / (right - left);
        persp[1, 1] = 2.0f * zNear / (top - bottom);
        persp[2, 0] = A;
        persp[2, 1] = B;
        persp[2, 2] = C;
        persp[2, 3] = -1.0f;
        persp[3, 2] = D;

        var rhsToLhs = new Matrix4x4();
        rhsToLhs[0, 0] = 1.0f;
        rhsToLhs[1, 1] = -1.0f; // Flip Y (RHS -> LHS)
        rhsToLhs[2, 2] = 1.0f;
        rhsToLhs[3, 3] = 1.0f;

        return rhsToLhs * persp.transpose; // see comment above
    }

    private void ApplyTranslationAndRotationToCamera(CvMat translation, Rotation r)
    {
        double tx = translation[0, 0];
        double ty = translation[0, 1];
        double tz = translation[0, 2];

        _mainCamera.transform.position = new Vector3((float)tx, (float)ty, (float)tz);
        _mainCamera.transform.eulerAngles = new Vector3((float)r.X, (float)r.Y, (float)r.Z);
    }

    private CvMat CreateIntrinsicGuess(double height, double width)
    {
        // from https://docs.google.com/spreadsheet/ccc?key=0AuC4NW61c3-cdDFhb1JxWUFIVWpEdXhabFNjdDJLZXc#gid=0
        // taken from http://www.neilmendoza.com/projector-field-view-calculator/
        float hfov = 91.2705674249382f;
        float vfov = 59.8076333281726f;

        double fx = (double)((float)width / (2.0f * Mathf.Tan(0.5f * hfov * Mathf.Deg2Rad)));
        double fy = (double)((float)height / (2.0f * Mathf.Tan(0.5f * vfov * Mathf.Deg2Rad)));

        double cy = height / 2.0;
        double cx = width / 2.0;

        double[] intrGuess = new double[] { fx, 0.0, cx, 
            0.0, fy, cy, 
            0.0, 0.0, 1.0 };
        return new CvMat(3, 3, MatrixType.F64C1, intrGuess);
    }

    private CvMat PutObjectPointsIntoCVMat(List<Vector3> _objectPositions, int pointsCount, int ImageNum)
    {
        CvPoint3D32f[,] objects = new CvPoint3D32f[ImageNum, pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            objects[0, i] = new CvPoint3D32f
            {
                X = _objectPositions[i].x,
                Y = _objectPositions[i].y,
                Z = _objectPositions[i].z * -1
            };
        }

        return new CvMat(pointsCount, 3, MatrixType.F32C1, objects);
    }

    private CvMat PutImagePointsIntoCVMat(List<Vector3> _imagePositions, int pointsCount)
    {
        List<CvPoint2D32f> allCorners = new List<CvPoint2D32f>(pointsCount);

        for (int i = 0; i < _imagePositions.Count; i++)
        {
            allCorners.Add(new CvPoint2D32f(_imagePositions[i].x, _imagePositions[i].y));
        }

        return new CvMat(pointsCount, 1, MatrixType.F32C2, allCorners.ToArray());
    }
}