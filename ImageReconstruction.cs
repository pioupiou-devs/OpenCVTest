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

        int deltaW = 200, deltaH = 200, deltaX = 200, deltaY = 200;
        Mat reconstructedImage = new(new Size(imageSrc.Width + (deltaY + deltaH), imageSrc.Height + (deltaX + deltaW)), imageSrc.Type());

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
            Mat cameraCalibration = Mat.Eye(3, 3, MatType.CV_64FC1);
            Cv2.DecomposeHomographyMat(homographyMatrix, cameraCalibration, out var rotations, out var translations, out _);

            #region checks
            // Check translations
            Console.WriteLine("Translations: ");
            foreach (var t in translations)
            {
                Utils.PrintMat("t", t);
                Console.WriteLine();
            }

            Console.WriteLine("------------------------------------------------------------------------------------");

            // Check rotations
            Console.WriteLine("Rotations: ");
            foreach (var r in rotations)
            {
                Utils.PrintMat("r", r);
                Console.WriteLine();
            }

            Console.WriteLine("------------------------------------------------------------------------------------");
            #endregion checks

            double[] rotationAngles = new double[3];
            for (int i = 0; i < 3; i++)
            {
                var rotMat = rotations[i].RowRange(0, 3).ColRange(0, 3);
                rotationAngles[i] = Math.Atan2(rotMat.At<double>(1, 0), rotMat.At<double>(0, 0));
            }

            // Output rotation angles
            Console.WriteLine("Rotation Angles (in radians):");
            Console.WriteLine($"Rotation 0: {rotationAngles[0]}");
            Console.WriteLine($"Rotation 1: {rotationAngles[1]}");
            Console.WriteLine($"Rotation 2: {rotationAngles[2]}");

            // Output translation vectors
            Console.WriteLine("Translation Vectors:");
            Console.WriteLine($"Tx0: {translations[0].At<double>(0)}");
            Console.WriteLine($"Ty0: {translations[0].At<double>(1)}");
            Console.WriteLine($"Tx1: {translations[1].At<double>(0)}");
            Console.WriteLine($"Ty1: {translations[1].At<double>(1)}");
            Console.WriteLine($"Tx2: {translations[2].At<double>(0)}");
            Console.WriteLine($"Ty2: {translations[2].At<double>(1)}");



            Mat? image = fragment.Image;
            Mat translated = image?.Clone();

            int pointIndex = 2;
            float tetha = (float)rotationAngles[pointIndex]; //fragment.Angle; // TODO calc ?
            float tx = translations[pointIndex].At<float>(0);//fragment.X; // TODO calc ?
            float ty = translations[pointIndex].At<float>(1);//fragment.Y; // TODO calc ?

            // Rotate and translate image
            Mat rotationMatrix = Cv2.GetRotationMatrix2D(new Point2f(image.Width / 2, image.Height / 2), tetha, 1.0);
            Cv2.WarpAffine(image, translated, rotationMatrix, image.Size());

            Utils.PrintImage("image", image);
            Utils.PrintImage("translated", translated);

            // Get the alpha channel from translated
            Mat alpha = new();
            Cv2.ExtractChannel(translated, alpha, 3);

            // Calculate the window
            Rect window = new(
                new Point((tx - (translated.Width / 2)) + deltaX, (ty - (translated.Height / 2)) + deltaY),
                new Size(translated.Width, translated.Height));
            var temp = null as Mat;
            try
            {
                // Apply translated with alpha channel to background
                temp = reconstructedImage[window];
                Utils.PrintImage("reconstructedImage", reconstructedImage);
                Utils.PrintImage("temp", temp);
            }
            catch (Exception e)
            {

                Console.WriteLine($"error : {e.Message}, when working on fragment {fragment.Number}");
                continue;
            }
            translated.CopyTo(temp, alpha);

            Utils.PrintImage("translated2", translated);
            Utils.PrintImage("reconstructedImage2", reconstructedImage);
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
