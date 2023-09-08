
using OpenCvSharp;

using OpenCVTest;

using System.Globalization;

var FRAGMENTS = new List<Fragment>();

// Read file
foreach (var line in File.ReadLines("Resources\\fragments.txt"))
{
    string[] values = line.Split(new char[] { ' ' });
    Fragment f = new Fragment(
        int.Parse(values[0]),
        int.Parse(values[1]),
        int.Parse(values[2]),
        float.Parse(values[3], CultureInfo.InvariantCulture) // Precise the use of a dot instead of a comma
    );
    FRAGMENTS.Add(f);
}

// White background
Mat BG = new Mat(775, 1707, MatType.CV_32FC4);
BG.Rectangle(new Rect(0, 0, 1707, 775), new Scalar(255, 255, 255, 255), -1);

Utils.PrintScore("solution.txt");

// Load Image
Mat img = Utils.LoadImage(@"Resources\frag_eroded\frag_eroded_0.png");

// Fuse pics
Mat fusion = new Mat();
//int x_offset = 50;
//int y_offset = 50;
//BG[y_offset: y_offset + img.Width, x_offset: x_offset + img.Height] = img;
// ^ en mode shlag où on attaque la matrice, idk how to translate it in proper c#
BG.AdjustROI((int)FRAGMENTS[0].X, (int)FRAGMENTS[0].X + img.Height, (int)FRAGMENTS[0].Y, (int)FRAGMENTS[0].Y + img.Width);
img.CopyTo(BG);

// Create a window
Cv2.NamedWindow("A", WindowFlags.AutoSize);

// Show the image
Cv2.ImShow("A", BG);

// Wait for key press
Cv2.WaitKey(0);
