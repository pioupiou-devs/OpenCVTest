using OpenCvSharp;

namespace OpenCVTest
{
    public static class ImageReconstruction
    {
        private static ORB orb;

        static ImageReconstruction()
        {
            // Initialize ORB with default parameters
            orb = ORB.Create();
        }

        public static Mat ReconstructImage(Mat imageSrc, List<Mat> fragments)
        {
            // Perform reconstruction using ORB feature matching
            List<int[]> bestMatches = FindBestMatches(imageSrc, fragments);

            // Assemble the reconstructed image
            Mat reconstructedImage = AssembleImage(imageSrc, fragments, bestMatches);

            return reconstructedImage;
        }

        private static List<int[]> FindBestMatches(Mat imageSrc, List<Mat> fragments)
        {
            List<int[]> bestMatches = new List<int[]>();

            // Extract ORB features from the source image
            KeyPoint[] keypointsSrc;
            Mat descriptorsSrc = DetectAndComputeORB(imageSrc, out keypointsSrc);

            foreach (var fragment in fragments)
            {
                // Extract ORB features from the fragment
                KeyPoint[] keypointsFragment;
                Mat descriptorsFragment = DetectAndComputeORB(fragment, out keypointsFragment);

                // Match ORB features between the source image and the fragment
                DMatch[] matches = MatchORBFeatures(descriptorsSrc, descriptorsFragment);

                // Find the best match based on the number of good matches
                int[] bestMatch = FindBestMatch(matches, keypointsSrc, keypointsFragment);
                if (bestMatch != null)
                {
                    bestMatch[2] = fragments.IndexOf(fragment); // Save the index of the matched fragment
                    bestMatches.Add(bestMatch);
                }
            }

            return bestMatches;
        }

        private static Mat DetectAndComputeORB(Mat image, out KeyPoint[] keypoints)
        {
            // Detect and compute ORB features
            Mat descriptors = new();
            orb.DetectAndCompute(image, null, out keypoints, descriptors);
            return descriptors;
        }

        private static DMatch[] MatchORBFeatures(Mat descriptorsSrc, Mat descriptorsFragment)
        {
            // Match ORB features between the source image and the fragment
            BFMatcher matcher = new BFMatcher(NormTypes.Hamming, true);
            DMatch[] matches = matcher.Match(descriptorsSrc, descriptorsFragment);

            return matches;
        }

        private static int[] FindBestMatch(DMatch[] matches, KeyPoint[] keypointsSrc, KeyPoint[] keypointsFragment)
        {
            // Set a distance threshold for considering a match as good
            double distanceThreshold = 50.0; // Adjust this threshold based on your specific case

            // Filter out matches based on the distance threshold
            var goodMatches = matches.Where(m => m.Distance < distanceThreshold).ToList();

            // If there are enough good matches, consider it a valid match
            if (goodMatches.Count >= 3)
            {
                // Get the coordinates of the matched keypoints in the source and fragment
                List<Point2f> srcPoints = goodMatches.Select(m => keypointsSrc[m.QueryIdx].Pt).ToList();
                List<Point2f> fragmentPoints = goodMatches.Select(m => keypointsFragment[m.TrainIdx].Pt).ToList();

                // Convert the List<Point2f> to Mat
                Mat srcMat = new Mat(srcPoints.Count, 2, MatType.CV_32F, srcPoints.SelectMany(p => new float[] { p.X, p.Y }).ToArray());
                Mat fragmentMat = new Mat(fragmentPoints.Count, 2, MatType.CV_32F, fragmentPoints.SelectMany(p => new float[] { p.X, p.Y }).ToArray());

                // Calculate the transformation matrix using the matched keypoints
                Mat homography = Cv2.FindHomography(srcMat, fragmentMat, HomographyMethods.Ransac);

                // Check if the homography matrix is valid
                if (!homography.Empty())
                {
                    // Extract translation and rotation information from the homography matrix
                    double tx = homography.At<double>(0, 2);
                    double ty = homography.At<double>(1, 2);
                    double theta = Math.Atan2(homography.At<double>(1, 0), homography.At<double>(0, 0)) * (180 / Math.PI);

                    // Return the matched position and rotation information
                    return new int[] { (int)ty, (int)tx, -(int)theta }; // Negate the rotation angle as it might be in the opposite direction
                }
            }

            // If no valid match is found, return null
            return null;
        }


        private static Mat AssembleImage(Mat imageSrc, List<Mat> fragments, List<int[]> bestMatches)
        {
            // Create a blank canvas to assemble the image
            Mat reconstructedImage = Mat.Zeros(imageSrc.Rows, imageSrc.Cols, imageSrc.Type());

            foreach (var match in bestMatches)
            {
                // Paste each fragment onto the reconstructed image at its best-matched position
                int row = match[0];
                int col = match[1];
                PasteFragment(reconstructedImage, fragments[match[2]], row, col);
            }

            return reconstructedImage;
        }

        private static void PasteFragment(Mat image, Mat fragment, int row, int col)
        {
            // Paste the fragment onto the image at the specified position
            Mat roi = new Mat(image, new Rect(col, row, fragment.Cols, fragment.Rows));
            fragment.CopyTo(roi);
        }
    }
}