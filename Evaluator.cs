using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExtensionMethods;

using OpenCvSharp;

namespace OpenCVTest
{
    public class Evaluator
    {
        private float deltaX, deltaY = 1.0f; // px
        private float deltaAngle = 1.0f; // deg

        public Evaluator(float deltaX = 1.0f, float deltaY = 1.0f, float deltaAngle = 1.0f)
        {
            this.deltaX = deltaX;
            this.deltaY = deltaY;
            this.deltaAngle = deltaAngle;
        }

        private float CalculateScore(string proposedSolutionPath)
        {
            // Load the fragment files
            List<Fragment> imageList = Utils.ExtractFragments(@"Resources\frag_eroded");
            List<Fragment> solution = Utils.ExtractFragments("Resources\\fragments.txt", true);
            List<Fragment> proposedSolution = Utils.ExtractFragments($"Resources\\{proposedSolutionPath}", true);


            // Combine fragment list and image list by number for solution and proposed solution
            foreach (Fragment frag in solution)
            {
                Fragment? img = imageList.Find(f => f.Number == frag.Number);
                if (img is not null)
                    frag.Image = img?.Image;
            }

            foreach (Fragment frag in proposedSolution)
            {
                Fragment? img = imageList.Find(f => f.Number == frag.Number);
                if (img is not null)
                    frag.Image = img?.Image;
            }

            float score = 0.0f;

            // Check for proposed solution fragment not in solution
            foreach (Fragment proposedFragment in proposedSolution)
            {
                Fragment? solutionFragment = solution.Find(f => f.Number == proposedFragment.Number);

                if(proposedFragment.Image is null)
                    continue;

                // Check if the proposed fragment is misplaced
                bool isMisplaced = IsMisplaced(solutionFragment ?? new Fragment(0), proposedFragment);
                if (solutionFragment is null || isMisplaced)
                {
                    // Calculate the surface of the fragments
                    float proposedSurface = GetSurface(proposedFragment.Image);

                    if (solutionFragment is null)
                        score += proposedSurface;
                    else
                    {
                        float solutionSurface = GetSurface(solutionFragment.Image);
                        // Calculate the difference between the surface of the fragments
                        float surfaceDiff = Math.Abs(proposedSurface - solutionSurface);
                        score += surfaceDiff;
                    }
                }
            }

            // Get the total surface of the solution fragments
            float totalSurface = solution.Where(f => f.Image is not null).Sum(f => GetSurface(f.Image));

            // Calculate the score
            score /= totalSurface;
            return score;
        }

        public void PrintScore(string proposedSolutionPath)
        {
            float score = CalculateScore(proposedSolutionPath);
            Console.WriteLine($"Error Score for \"{proposedSolutionPath}\" = {score} (~ {Math.Round(score*100f,2)}%)");
        }

        private bool IsMisplaced(Fragment solution, Fragment proposedFragment) =>
            (Math.Abs(solution.X - proposedFragment.X) < deltaX) ||
            (Math.Abs(solution.Y - proposedFragment.Y) < deltaY) ||
            (Math.Abs(solution.Angle - proposedFragment.Angle) < deltaAngle);

        private float GetSurface(Mat image)
        {
            // Binaryze the image
            Mat binary = new();
            Cv2.CvtColor(image, binary, ColorConversionCodes.BGRA2GRAY);
            Cv2.Threshold(binary, binary, 0, 255, ThresholdTypes.Binary);

            // Calculate the surface
            return Cv2.CountNonZero(binary);
        }

    }
}
