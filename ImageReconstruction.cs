using OpenCvSharp;

public class ImageReconstruction
{
    private readonly Mat _originalImage;
    private readonly Mat _fragmentImage;

    public ImageReconstruction(string originalImagePath, string fragmentImagePath)
    {
        _originalImage = Cv2.ImRead(originalImagePath);
        _fragmentImage = Cv2.ImRead(fragmentImagePath);
    }

    public void PerformReconstruction()
    {
        // Exercice 1: Détection de points d'intérêt et association
        var keyPointsOriginal = DetectKeyPoints(_originalImage);
        var keyPointsFragment = DetectKeyPoints(_fragmentImage);

        var descriptorsOriginal = ComputeDescriptors(_originalImage, keyPointsOriginal);
        var descriptorsFragment = ComputeDescriptors(_fragmentImage, keyPointsFragment);

        var matches = MatchKeyPoints(descriptorsOriginal, descriptorsFragment);

        // Exercice 2: Filtrage des associations par RANSAC
        var inlierMatches = FilterAssociationsRANSAC(keyPointsOriginal, keyPointsFragment, matches);

        // Exercice 3: Évaluation des résultats
        EvaluateResults(inlierMatches);

        // Exercice 4: Filtrage des associations par conservation de la distance Euclidienne
        var euclideanMatches = FilterAssociationsEuclidean(keyPointsOriginal, keyPointsFragment, matches);
        EvaluateResults(euclideanMatches);
    }

    private KeyPoint[] DetectKeyPoints(Mat image)
    {
        var orb = ORB.Create();
        return orb.Detect(image);
    }

    private Mat ComputeDescriptors(Mat image, KeyPoint[] keyPoints)
    {
        var orb = ORB.Create();
        var descriptors = new Mat();
        orb.Compute(image,ref keyPoints, descriptors);
        return descriptors;
    }

    private DMatch[] MatchKeyPoints(Mat descriptors1, Mat descriptors2)
    {
        var bfMatcher = new BFMatcher(NormTypes.Hamming);
        return bfMatcher.Match(descriptors1, descriptors2);
    }

    private DMatch[] FilterAssociationsRANSAC(KeyPoint[] keyPoints1, KeyPoint[] keyPoints2, DMatch[] matches)
    {
        var points1 = Array.ConvertAll(matches, m => keyPoints1[m.QueryIdx].Pt);
        var points2 = Array.ConvertAll(matches, m => keyPoints2[m.TrainIdx].Pt);

        var homographyMatrix = Cv2.FindHomography(InputArray.Create(points1), InputArray.Create(points2), HomographyMethods.Ransac, 3.0);

        // Utilisez la matrice de transformation pour filtrer les associations
        var inlierMatches = new List<DMatch>();
        for (int i = 0; i < matches.Length; i++)
        {
            var point1 = points1[i];
            var point2 = points2[i];

            var transformedPoint = Cv2.PerspectiveTransform(new[] { point1 }, homographyMatrix);
            var distance = Cv2.Norm(point2 - transformedPoint[0].ToPoint());

            if (distance < 3.0) // Choisissez une valeur de seuil appropriée
            {
                inlierMatches.Add(matches[i]);
            }
        }

        return inlierMatches.ToArray();
    }

    private DMatch[] FilterAssociationsEuclidean(KeyPoint[] keyPoints1, KeyPoint[] keyPoints2, DMatch[] matches)
    {
        var euclideanMatches = new List<DMatch>();
        for (int i = 0; i < matches.Length; i++)
        {
            var point1 = keyPoints1[matches[i].QueryIdx].Pt;
            var point2 = keyPoints2[matches[i].TrainIdx].Pt;

            var distance = Cv2.Norm(point1 - point2);

            if (distance < 50.0) // Choisissez une valeur de seuil appropriée
            {
                euclideanMatches.Add(matches[i]);
            }
        }

        return euclideanMatches.ToArray();
    }

    private void EvaluateResults(DMatch[] matches)
    {
        var inlierImage = new Mat();
        Cv2.DrawMatches(_originalImage, keyPointsOriginal, _fragmentImage, keyPointsFragment, matches, inlierImage);
        Cv2.ImShow("Inlier Matches", inlierImage);
        Cv2.WaitKey(0);
    }
}
