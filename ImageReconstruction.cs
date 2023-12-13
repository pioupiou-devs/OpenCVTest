using OpenCvSharp;

namespace OpenCVTest;

public class ImageReconstruction
{
    private ORB orb;
    private DescriptorMatcher matcher;

    public ImageReconstruction()
    {
        // Initialize ORB with default parameters
        orb = ORB.Create();
        matcher = DescriptorMatcher.Create("BruteForce-Hamming");
    }

    public Mat ReconstructImage(Mat imageSrc, List<Fragment> fragments)
    {
        // Detect the keypoints in fragments
        DetectKeyPointsInList(ref fragments);

        // Detect the keypoints in the source image
        List<KeyPoint> srcKeyPoints = DetectKeyPoints(imageSrc);

        // Compute the descriptors
        ComputeDescriptorInList(fragments);
        Mat srcDescriptor = ComputeDescriptor(imageSrc, srcKeyPoints.ToArray());

        Fragment fragTest = fragments[24];

        DMatch[] temp = GetMatches(fragTest.Descriptor, srcDescriptor);

        Mat output = new();
        Cv2.DrawMatches(fragTest.Image, fragTest.KeyPoints, imageSrc, srcKeyPoints, temp, output);

        Utils.PrintImage(output);
        Cv2.WaitKey(0);

        // Combine with RANSAC method
        //Mat combineRes = FindHomography(fragments[0].Descriptor, srcDescriptor);

        //Utils.PrintImage(combineRes);
        //Cv2.WaitKey(0);

        return null;
    }

    private DMatch[] GetMatches(Mat fragmentDescriptor, Mat srcDescriptor) => matcher
                    .Match(fragmentDescriptor, srcDescriptor)
                    .OrderBy(t => t.Distance).ToArray();

    private Mat FindHomography(Mat descriptor1, Mat descriptor2)
    {
        return Cv2.FindHomography(descriptor1, descriptor2, HomographyMethods.Ransac);
    }


    #region KeyPoints

    private void DetectKeyPointsInList(ref List<Fragment> fragments) =>
        fragments
            .AsParallel()
            .ForAll(f => f.KeyPoints = DetectKeyPoints(f.Image));

    private List<KeyPoint> DetectKeyPoints(Mat image) => orb.Detect(image)?.ToList();
    #endregion

    #region Descriptors

    private void ComputeDescriptorInList(List<Fragment> fragments) =>
        fragments
            .AsParallel()
            .ForAll(f => f.Descriptor = ComputeDescriptor(f.Image, f.KeyPoints.ToArray()));

    private Mat ComputeDescriptor(Mat imageSrc, KeyPoint[] keypoints)
    {
        Mat output = new();
        orb.Compute(imageSrc, ref keypoints, output);
        return output;
    }


    #endregion
}
