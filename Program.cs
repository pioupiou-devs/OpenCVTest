using OpenCvSharp;

// Load the image Michelangelo_ThecreationofAdam_1707x775.jpg with the standard file library
Mat img = new Mat("Resources\\Michelangelo_ThecreationofAdam_1707x775.jpg", ImreadModes.Color);

// Create a window
Cv2.NamedWindow("Michelangelo_ThecreationofAdam_1707x775", WindowFlags.AutoSize);

// Show the image
Cv2.ImShow("Michelangelo_ThecreationofAdam_1707x775", img);

// Wait for key press
Cv2.WaitKey(0);
