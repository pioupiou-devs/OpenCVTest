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


        // Find homography for each fragment (to cut in methods)
        foreach (var fragment in intrestingFragment)
        {
            Console.WriteLine($"frag n°{fragment.Number} => {fragment.MatchedKeyPoints.Count}");
            Point2f src = fragment.MatchedKeyPoints[0].Item1;
            Point2f dest = fragment.MatchedKeyPoints[0].Item2;
            Point2f src2 = fragment.MatchedKeyPoints[1].Item1;
            Point2f dest2 = fragment.MatchedKeyPoints[1].Item2;

            Mat matchedPoints1 = new(1, 1, MatType.CV_32FC2, new[] { src.X, src.Y, src2.X, src2.Y });
            Mat matchedPoints2 = new(1, 1, MatType.CV_32FC2, new[] { dest.X, dest.Y, dest2.X, dest2.Y });

            // Find homography using RANSAC
            Mat homographyMatrix = Cv2.FindHomography(matchedPoints1, matchedPoints2, HomographyMethods.Ransac, ransacReprojThreshold: 3.0);

            // Warp the fragment image to align with the source image
            Mat warpedFragment = new Mat();
            Cv2.WarpPerspective(fragment.Image, warpedFragment, homographyMatrix, imageSrc.Size());

            // Optionally, you can blend or stitch the warped fragments into a single image
            // For example, using a simple averaging method:
            Cv2.AddWeighted(imageSrc, 0.5, warpedFragment, 0.5, 0, imageSrc);

            Utils.PrintImage(imageSrc);
            Cv2.WaitKey(0);
        }

        // At this point, the imageSrc contains the reconstructed image with fragments aligned

        return imageSrc;
    }

    #region Matched Keypoints
    private void ExtractMatchedKeyPointsInList(List<Fragment> fragments, List<KeyPoint> srcKeypoints) =>
        fragments
            .AsParallel()
            .ForAll(f => ExtractMatchedKeyPointsForFragment(f, srcKeypoints));
    private void ExtractMatchedKeyPointsForFragment(Fragment fragment, List<KeyPoint> srcKeypoints)
    {
        List<(Point2f, Point2f)> temp = fragment.MatchToSrcImage
            .AsParallel()
            .Select(m => ExtractMatchedKeyPoints(m, fragment.KeyPoints, srcKeypoints)).ToList();

        fragment.MatchedKeyPoints.AddRange(temp);
    }

    private (Point2f, Point2f) ExtractMatchedKeyPoints(DMatch match, List<KeyPoint> keypoints, List<KeyPoint> srcKeypoints)
    {
        Point2f src = keypoints[match.QueryIdx].Pt;
        Point2f dest = srcKeypoints[match.TrainIdx].Pt;
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
    private DMatch[] GetMatches(Mat fragmentDescriptor, Mat srcDescriptor) => _matcher
                    .Match(fragmentDescriptor, srcDescriptor)
                    .OrderBy(t => t.Distance).ToArray();
    #endregion
}
