using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExtensionMethods;

namespace OpenCVTest
{
    public static class Utils
    {
        private const float deltaX = 1.0f; // px
        private const float deltaY = 1.0f; // px
        private const float deltaAngle = 1.0f; // deg

        private static List<Fragment> ExtractFragment(string filePath)
        {
            List<Fragment> fragments = new();
            using (StreamReader sr = new(filePath))
            {
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

        public static void PrintScore(string proposedSolutionPath)
        {
            // Load the fragment files
            List<Fragment> solution = Utils.ExtractFragment("Resources\\fragments.txt");
            List<Fragment> proposedSolution = Utils.ExtractFragment($"Resources\\{proposedSolutionPath}");

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
