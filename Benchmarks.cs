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

        public static void Test()
        {
            // Charger vos images ici
            Mat image1 = new Mat("path/to/image1.jpg", ImreadModes.Color);
            Mat image2 = new Mat("path/to/image2.jpg", ImreadModes.Color);

            // Initialiser les détecteurs
            var sift = SIFT.Create();
            var surf = SURF.Create(1000); // Vous pouvez ajuster le seuil ici
            var orb = ORB.Create();
            var akaze = AKAZE.Create();
            var brisk = BRISK.Create();

            // Mesurer le temps d'exécution pour chaque détecteur
            var stopwatch = new System.Diagnostics.Stopwatch();

            // SIFT
            stopwatch.Start();
            var keypointsSift1 = sift.Detect(image1);
            var keypointsSift2 = sift.Detect(image2);
            var descriptorsSift1 = new Mat();
            var descriptorsSift2 = new Mat();
            sift.Compute(image1, ref keypointsSift1, descriptorsSift1);
            sift.Compute(image2, ref keypointsSift2, descriptorsSift2);
            var matcherSift = new BFMatcher(NormTypes.L2, false);
            var matchesSift = matcherSift.Match(descriptorsSift1, descriptorsSift2);
            stopwatch.Stop();
            Console.WriteLine($"SIFT: {stopwatch.ElapsedMilliseconds} ms, Matches: {matchesSift.Length}");

            // SURF
            stopwatch.Restart();
            var keypointsSurf1 = surf.Detect(image1);
            var keypointsSurf2 = surf.Detect(image2);
            var descriptorsSurf1 = new Mat();
            var descriptorsSurf2 = new Mat();
            surf.Compute(image1, ref keypointsSurf1, descriptorsSurf1);
            surf.Compute(image2, ref keypointsSurf2, descriptorsSurf2);
            var matcherSurf = new BFMatcher(NormTypes.L2, false);
            var matchesSurf = matcherSurf.Match(descriptorsSurf1, descriptorsSurf2);
            stopwatch.Stop();
            Console.WriteLine($"SURF: {stopwatch.ElapsedMilliseconds} ms, Matches: {matchesSurf.Length}");

            // ORB
            stopwatch.Restart();
            var keypointsOrb1 = orb.Detect(image1);
            var keypointsOrb2 = orb.Detect(image2);
            var descriptorsOrb1 = new Mat();
            var descriptorsOrb2 = new Mat();
            orb.Compute(image1, ref keypointsOrb1, descriptorsOrb1);
            orb.Compute(image2, ref keypointsOrb2, descriptorsOrb2);
            var matcherOrb = new BFMatcher(NormTypes.Hamming, false);
            var matchesOrb = matcherOrb.Match(descriptorsOrb1, descriptorsOrb2);
            stopwatch.Stop();
            Console.WriteLine($"ORB: {stopwatch.ElapsedMilliseconds} ms, Matches: {matchesOrb.Length}");

            // AKAZE
            stopwatch.Restart();
            var keypointsAkaze1 = akaze.Detect(image1);
            var keypointsAkaze2 = akaze.Detect(image2);
            var descriptorsAkaze1 = new Mat();
            var descriptorsAkaze2 = new Mat();
            akaze.Compute(image1, ref keypointsAkaze1, descriptorsAkaze1);
            akaze.Compute(image2, ref keypointsAkaze2, descriptorsAkaze2);
            var matcherAkaze = new BFMatcher(NormTypes.L2, false);
            var matchesAkaze = matcherAkaze.Match(descriptorsAkaze1, descriptorsAkaze2);
            stopwatch.Stop();
            Console.WriteLine($"AKAZE: {stopwatch.ElapsedMilliseconds} ms, Matches: {matchesAkaze.Length}");

            // BRISK
            stopwatch.Restart();
            var keypointsBrisk1 = brisk.Detect(image1);
            var keypointsBrisk2 = brisk.Detect(image2);
            var descriptorsBrisk1 = new Mat();
            var descriptorsBrisk2 = new Mat();
            brisk.Compute(image1, ref keypointsBrisk1, descriptorsBrisk1);
            brisk.Compute(image2, ref keypointsBrisk2, descriptorsBrisk2);
            var matcherBrisk = new BFMatcher(NormTypes.Hamming, false);
            var matchesBrisk = matcherBrisk.Match(descriptorsBrisk1, descriptorsBrisk2);
            stopwatch.Stop();
            Console.WriteLine($"BRISK: {stopwatch.ElapsedMilliseconds} ms, Matches: {matchesBrisk.Length}");

            // Dessiner les correspondances sur l'image (pour la visualisation)
            Mat resultImage = new Mat();
            if (keypointsSift1.Length > 0 && keypointsSift2.Length > 0)
            {
                Cv2.DrawMatches(image1, keypointsSift1, image2, keypointsSift2, matchesSift, resultImage);
                Cv2.ImShow("SHITFT Matches", resultImage);
            }

            Mat resultImage1 = new();
            if (keypointsSurf1.Length > 0 && keypointsSurf2.Length > 0)
            {
                Cv2.DrawMatches(image1, keypointsSurf1, image2, keypointsSurf2, matchesSurf, resultImage1);
                Cv2.ImShow("SURF Matches", resultImage1);
            }

            Mat resultImage2 = new();
            if (keypointsOrb1.Length > 0 && keypointsOrb2.Length > 0)
            {
                Cv2.DrawMatches(image1, keypointsOrb1, image2, keypointsOrb2, matchesSurf, resultImage2);
                Cv2.ImShow("ORB Matches", resultImage2);
            }



            Cv2.WaitKey(0);

            // N'oubliez pas de libérer les ressources
            image1.Release();
            image2.Release();
            resultImage.Release();
        }
    }

}