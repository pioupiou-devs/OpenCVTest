using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCvSharp.Features2D;
using OpenCvSharp.XFeatures2D;
using OpenCvSharp;

namespace OpenCVTest
{
    public class Benchmarks
    {
        public static string PATH_IMG_1 = @"Resources\Michelangelo_ThecreationofAdam_1707x775.jpg";
        public static string PATH_IMG_2 = @"Resources\frag_eroded\frag_eroded_24.png";
        public static string PATH_RES = @"Resources\result\";

        public static void Test()
        {
            // Charger vos images ici
            Mat image1 = new Mat(PATH_IMG_1, ImreadModes.Color);
            Mat image2 = new Mat(PATH_IMG_2, ImreadModes.Color);

            // Vérifier que les images sont chargées correctement
            if (image1.Empty() || image2.Empty())
            {
                Console.WriteLine("Une ou plusieurs images n'ont pas été chargées correctement.");
                return;
            }

            // Initialiser les détecteurs
            var sift = SIFT.Create();
            var surf = SURF.Create(1000); // Vous pouvez ajuster le seuil ici
            var orb = ORB.Create();
            var akaze = AKAZE.Create();
            var brisk = BRISK.Create();

            // Show the results for each algorithm
            ShowResult(image1, image2, sift, "SIFT");
            ShowResult(image1, image2, surf, "SURF");
            ShowResult(image1, image2, orb, "ORB");
            ShowResult(image1, image2, akaze, "AKAZE");
            ShowResult(image1, image2, brisk, "BRISK");

            Cv2.WaitKey(0);

            // N'oubliez pas de libérer les ressources
            image1.Release();
            image2.Release();
        }
        static void ShowResult(Mat image1, Mat image2, Feature2D detector, string algorithmName)
        {
            // Mesurer le temps d'exécution pour le détecteur actuel
            var stopwatch = new System.Diagnostics.Stopwatch();

            // Detecter les keypoints et calculer les descripteurs
            stopwatch.Start();
            var keypoints1 = detector.Detect(image1);
            var keypoints2 = detector.Detect(image2);
            var descriptors1 = new Mat();
            var descriptors2 = new Mat();
            detector.Compute(image1, ref keypoints1, descriptors1);
            detector.Compute(image2, ref keypoints2, descriptors2);
            var matcher = new BFMatcher(NormTypes.L2, false);
            var matches = matcher.Match(descriptors1, descriptors2);
            stopwatch.Stop();

            Console.WriteLine($"{algorithmName}: {stopwatch.ElapsedMilliseconds} ms, Matches: {matches.Length}");

            // Dessiner les correspondances sur l'image (pour la visualisation)
            var resultImage = GetSideBySideImage(image1.Clone(), image2.Clone());
            Cv2.DrawMatches(image1, keypoints1, image2, keypoints2, matches, resultImage, flags: DrawMatchesFlags.DrawRichKeypoints & DrawMatchesFlags.NotDrawSinglePoints & DrawMatchesFlags.DrawOverOutImg);

            // Afficher le résultat
            Cv2.ImShow($"{algorithmName} Matches", resultImage);

            // Save result
            string savePath = $"{PATH_RES}{algorithmName}_result.png";
            savePath = Path.GetFullPath(savePath);
            if (!Directory.Exists(PATH_RES))
            {
                Directory.CreateDirectory(PATH_RES);
            }
            resultImage.SaveImage(savePath);
            Console.WriteLine($"Save to {savePath} successfully.");

            // N'oubliez pas de libérer les ressources
            resultImage.Release();
        }

        private static Mat GetSideBySideImage(Mat image1, Mat image2)
        {
            // Make sure the images have the same height
            int maxHeight = Math.Max(image1.Height, image2.Height);
            Cv2.Resize(image2, image2, new Size(image1.Width, image1.Height));

            // Create a new image with double width
            Mat result = new Mat(new Size(image1.Width + image2.Width, maxHeight), MatType.CV_8UC3);

            // Copy the first image to the left side of the result image
            image1.CopyTo(result.SubMat(new Rect(0, 0, image1.Width, maxHeight)));

            // Copy the second image to the right side of the result image
            image2.CopyTo(result.SubMat(new Rect(image1.Width, 0, image2.Width, maxHeight)));

            return result;
        }
    }

}