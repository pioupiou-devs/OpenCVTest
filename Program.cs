
using OpenCvSharp;

using OpenCVTest;

Utils.PrintScore("solution.txt");

// Load Image
Mat img = Utils.LoadImage(@"Resources\frag_eroded\frag_eroded_0.png");

// Show the image
Cv2.ImShow("fragment 0", img);

// Wait for key press
Cv2.WaitKey(0);
