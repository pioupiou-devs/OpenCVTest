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

using Point = OpenCvSharp.Point;

namespace OpenCVTest
{
    public static class Utils
    {
        public static Mat LoadImage(string filepath) =>
            Cv2.ImDecode(File.ReadAllBytes(filepath), ImreadModes.Unchanged);

        /// <summary>
        /// Create a list of fragment from files
        /// </summary>
        /// <param name="folderPath">Path to the folder or file depending of isText</param>
        /// <param name="isText">define if is a text file or a folder of png files</param>
        /// <returns>list of fragments potentially not completed</returns>
        public static List<Fragment> ExtractFragments(string folderPath, bool isText = false, bool notNullOnly = false)
        {
            List<Fragment> fragments =
                isText ?
            ExtractFromTextFile(folderPath) :
            ExtractFromPngFile(folderPath);

            if (notNullOnly)
                fragments.RemoveAll(f => f.Image is null);

            return fragments;
        }

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

        public static Tuple<int, int> GetSizeFromImage(string filepath)
        {
            OpenCvSharp.Size size = Cv2.ImRead(filepath, ImreadModes.Unchanged).Size();
            return new Tuple<int, int>(size.Width, size.Height);
        }

        public static void PrintImage(Mat image) => PrintImage($"{nameof(image)} :", image);
        public static void PrintImage(string name, Mat image)
        {
            try
            {
                // Show image
                _ = new Window(name, WindowFlags.GuiExpanded);
                Cv2.ImShow(name, image);
            }
            catch (Exception)
            {
                Console.WriteLine("Error while printing image");
            }
        }

        public static void PrintMat(Mat mat) => PrintMat($"{nameof(mat)} : ", mat);
        public static void PrintMat(string name, Mat mat)
        {
            try
            {
                Console.WriteLine($"{name} = ");
                for (int i = 0; i < mat.Rows; i++)
                {
                    string txt = "";
                    for (int j = 0; j < mat.Cols; j++)
                    {
                        var pixel = mat.At<byte>(i, j);

                        if (pixel > 0)
                        {
                            txt += pixel + " ";
                        }
                    }
                    Console.WriteLine($"[{txt}]");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error while printing mat");
            }
        }

    }
}
