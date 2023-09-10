using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ExtensionMethods;

using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace OpenCVTest
{
    public static class Utils
    {
        private const float deltaX = 1.0f; // px
        private const float deltaY = 1.0f; // px
        private const float deltaAngle = 1.0f; // deg

        public static Mat LoadImage(string filepath) =>
            Cv2.ImDecode(File.ReadAllBytes(filepath), ImreadModes.Unchanged);

        /// <summary>
        /// Create a list of fragment from files
        /// </summary>
        /// <param name="folderPath">Path to the folder or file depending of isText</param>
        /// <param name="isText">define if is a text file or a folder of png files</param>
        /// <returns>list of fragments potentially not completed</returns>
        public static List<Fragment> ExtractFragments(string folderPath, bool isText = false) =>
            isText ?
            ExtractFromTextFile(folderPath) :
            ExtractFromPngFile(folderPath);

        private static List<Fragment> ExtractFromTextFile(string folderPath)
        {
            List<Fragment> fragments = new();
            using StreamReader sr = new(folderPath);
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] tokens = line?
                    .Split(' ')
                    .Select(t => t.Replace(".", ","))
                    .ToArray()
                    ?? Array.Empty<string>();

                if (tokens.Length != 4)
                    throw new Exception("Invalid file format");

                int number = int.Parse(tokens[0]);
                float x = float.Parse(tokens[1]);
                float y = float.Parse(tokens[2]);
                float angle = float.Parse(tokens[3]);

                fragments.Add(new Fragment(number, x, y, angle));
            }
            return fragments;
        }

        private static List<Fragment> ExtractFromPngFile(string folderPath)
        {
            List<Fragment> fragments = new();
            // loop over all files in the folder frag_eroded
            foreach (string file in Directory.EnumerateFiles(folderPath))
            {
                if (!Regex.IsMatch(file, "frag_eroded_[0-9]*\\.png$"))
                    continue;

                // Extract the number from the file paht with regex
                int imgNumber = int.Parse(Regex.Match(file, "[0-9]+").Value);

                // Load Image
                Mat frag = Utils.LoadImage(file);
                fragments.Add(new Fragment(imgNumber, image: frag));
            }
            return fragments;
        }

        private static float EvaluateFragment(Fragment solution, Fragment proposedFragment)
        {
            float score = 0.0f;
            if (Math.Abs(solution.X - proposedFragment.X) < deltaX)
            {
                score += 1.0f;
            }
            if (Math.Abs(solution.Y - proposedFragment.Y) < deltaY)
            {
                score += 1.0f;
            }
            if (Math.Abs(solution.Angle - proposedFragment.Angle) < deltaAngle)
            {
                score += 1.0f;
            }
            return score;
        }
        public static Tuple<int, int> GetSizeFromImage(string filepath)
        {
            OpenCvSharp.Size size = Cv2.ImRead(filepath, ImreadModes.Unchanged).Size();
            return new Tuple<int, int>(size.Width, size.Height);
        }
        public static void PrintScore(string proposedSolutionPath)
        {
            // Load the fragment files
            List<Fragment> solution = Utils.ExtractFragments("Resources\\fragments.txt", true);
            List<Fragment> proposedSolution = Utils.ExtractFragments($"Resources\\{proposedSolutionPath}", true);

            float maxFragmentScore = 3.0f;
            float maxScore = maxFragmentScore * solution.Count;
            float score = 0.0f;

            foreach (var (s, p) in solution.Zip(proposedSolution))
                score += Utils.EvaluateFragment(s, p);

            float meanScore = score / solution.Count;

            Console.WriteLine($"Score = {score.ToPercentage(maxScore)}%, Mean score = {meanScore.ToPercentage(maxFragmentScore)}%");
        }
    }
}
