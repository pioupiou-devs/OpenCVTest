using System.Collections.Generic;
using System.Xml.Serialization;

using OpenCvSharp;

namespace OpenCVTest;

public class ImageReconstruction
{
    private ORB _orb;
    private DescriptorMatcher _matcher;
    private int _minMatches = 5;

    public ImageReconstruction(int minMatches) : this()
    {
        _minMatches = minMatches;
    }
    public ImageReconstruction()
    {
        // Initialize ORB with default parameters
        _orb = ORB.Create();
        _matcher = DescriptorMatcher.Create("BruteForce-Hamming");
    }

    public Mat ReconstructImage(Mat imageSrc, List<Fragment> fragments)
    {
        // Detect the keypoints in fragments
        DetectKeyPointsInList(fragments);

        // Detect the keypoints in the source image
        List<KeyPoint> srcKeyPoints = DetectKeyPoints(imageSrc);

        // Compute the descriptors
        ComputeDescriptorInList(fragments);
        Mat srcDescriptor = ComputeDescriptor(imageSrc, srcKeyPoints.ToArray());

        // Compute the Matches
        GetMatchesInList(fragments, srcDescriptor);

        // Remove not instresting fragments
        List<Fragment> intrestingFragment = fragments
            .Where(f => f.MatchToSrcImage.Length >= _minMatches).ToList();

        // Extract the matched keypoints
        ExtractMatchedKeyPointsInList(intrestingFragment, srcKeyPoints);

        Mat reconstructedImage = new(new Size(imageSrc.Width, imageSrc.Height),imageSrc.Type());

        // Find homography for each fragment (to cut in methods)
        foreach (var fragment in intrestingFragment)
        {
            Console.WriteLine($"frag n°{fragment.Number} => {fragment.MatchedKeyPoints.Count}");

            List<Point2d> src = new();
            List<Point2d> dest = new();

            // Add all matched keypoints of the fragment to the source and destination Mat
            foreach (var matchedKeyPoint in fragment.MatchedKeyPoints)
            {
                src.Add(matchedKeyPoint.Item1);
                dest.Add(matchedKeyPoint.Item2);
            }

            Point2d[] srcArray = src.ToArray();
            Point2d[] destArray = dest.ToArray();

            // Find homography using RANSAC
            Mat homographyMatrix = Cv2.FindHomography(srcArray, destArray, HomographyMethods.Ransac);

            // Decompose homography matrix
            Mat[] rotations = Array.Empty<Mat>(), translations = Array.Empty<Mat>(), normals = Array.Empty<Mat>();
            Mat cameraCalibration = Mat.Eye(3, 3, MatType.CV_64FC1);
            Cv2.DecomposeHomographyMat(homographyMatrix, cameraCalibration, out rotations, out translations, out normals);

            // Extract rotation angles (in radians) from the rotation matrix
            Mat rotationMatrix = new();
            Cv2.Rodrigues(rotations[0], rotationMatrix);

            // Convert the 3x1 matrix to a 3x3 rotation matrix
            Mat rotMatrix = Mat.Zeros(3,3, MatType.CV_64FC1);
            rotMatrix.Set<double>(0, 0, rotationMatrix.At<double>(0, 0));
            rotMatrix.Set<double>(0, 1, rotationMatrix.At<double>(0, 1));
            rotMatrix.Set<double>(1, 0, rotationMatrix.At<double>(1, 0));
            rotMatrix.Set<double>(1, 1, rotationMatrix.At<double>(1, 1));
            rotMatrix.Set<double>(2, 2, 1.0);


            // Rotate the fragment image
            Mat fragImage = fragment.Image;
            Mat rotatedFragment = new(fragImage.Size(), fragImage.Type());
            float width = fragImage.Width;
            float height = fragImage.Height;

            Utils.PrintMat("convertedRotationMatrix", rotMatrix);
            Utils.PrintImage("rotated fragment", rotatedFragment);
            Utils.PrintImage("fragment", fragImage);
            Cv2.WaitKey(0);

            Cv2.WarpPerspective(fragImage, rotatedFragment, rotMatrix, fragImage.Size());

            // Translate the fragment image
            Mat affineTransform = Cv2.GetAffineTransform(
                new Point2f[] { new Point2f(0, 0), new Point2f(width, 0), new Point2f(0, height) },
                new Point2f[] { new Point2f(GetElement(translations,0), GetElement(translations,1)),
                new Point2f(width + GetElement(translations,0), GetElement(translations,1)),
                new Point2f(GetElement(translations,0), height + GetElement(translations,1)) });

            Mat translatedFragment = new();
            Cv2.WarpAffine(rotatedFragment, translatedFragment, affineTransform, new Size(width, height));

            // Add the translated fragment to the reconstructed image
            Cv2.Add(reconstructedImage, translatedFragment, reconstructedImage);

            // Draw the fragment on the reconstructed image
            Cv2.DrawContours(reconstructedImage, new Mat[] { translatedFragment }, 0, Scalar.Red, 2);

            Utils.PrintImage(reconstructedImage);
            Cv2.WaitKey(0);
        }

        // At this point, the imageSrc contains the reconstructed image with fragments aligned

        return imageSrc;
    }

    private float GetElement(Mat[] translations, int index)
    {
        return (float)translations[0].At<double>(index);
    }

    #region Matched Keypoints
    private void ExtractMatchedKeyPointsInList(List<Fragment> fragments, List<KeyPoint> srcKeypoints) =>
        fragments
            .AsParallel()
            .ForAll(f => ExtractMatchedKeyPointsForFragment(f, srcKeypoints));
    private void ExtractMatchedKeyPointsForFragment(Fragment fragment, List<KeyPoint> srcKeypoints)
    {
        List<(Point2d, Point2d)> temp = fragment.MatchToSrcImage
            .AsParallel()
            .Select(m => ExtractMatchedKeyPoints(m, fragment.KeyPoints, srcKeypoints)).ToList();

        fragment.MatchedKeyPoints.AddRange(temp);
    }

    private (Point2d, Point2d) ExtractMatchedKeyPoints(DMatch match, List<KeyPoint> keypoints, List<KeyPoint> srcKeypoints)
    {
        Point2f srcF = keypoints[match.QueryIdx].Pt;
        Point2d src = new(srcF.X, srcF.Y);
        Point2f destF = srcKeypoints[match.TrainIdx].Pt;
        Point2d dest = new(destF.X, destF.Y);
        return (src, dest);
    }
    #endregion

    #region KeyPoints

    private void DetectKeyPointsInList(List<Fragment> fragments) =>
        fragments
            .AsParallel()
            .ForAll(f => f.KeyPoints = DetectKeyPoints(f.Image));

    private List<KeyPoint> DetectKeyPoints(Mat image) => _orb.Detect(image)?.ToList();
    #endregion

    #region Descriptors

    private void ComputeDescriptorInList(List<Fragment> fragments) =>
        fragments
            .AsParallel()
            .ForAll(f => f.Descriptor = ComputeDescriptor(f.Image, f.KeyPoints.ToArray()));

    private Mat ComputeDescriptor(Mat imageSrc, KeyPoint[] keypoints)
    {
        Mat output = new();
        _orb.Compute(imageSrc, ref keypoints, output);
        return output;
    }


    #endregion

    #region Matches
    private void GetMatchesInList(List<Fragment> fragments, Mat srcDescriptor) =>
        fragments
            .AsParallel()
            .ForAll(f => f.MatchToSrcImage = GetMatches(f.Descriptor, srcDescriptor));
    private DMatch[] GetMatches(Mat fragmentDescriptor, Mat srcDescriptor) =>
        _matcher
            .Match(fragmentDescriptor, srcDescriptor)
            .OrderBy(t => t.Distance).ToArray();
    #endregion
}
