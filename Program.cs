using OpenCvSharp;

// Create a sample image
Mat img = new Mat(200, 400, MatType.CV_8UC3, Scalar.All(255));

// Draw a line
Cv2.Line(img, new Point(100, 100), new Point(200, 200), Scalar.Red, 3);

// Show the image
Cv2.ImShow("img1", img);

// Filter with a gaussian blur
Cv2.GaussianBlur(img, img, new Size(5, 5), 3);

// Filter with a high pass
Cv2.Laplacian(img, img, MatType.CV_8UC3, 3);

// Show the image
Cv2.ImShow("img2", img);

Cv2.WaitKey(0);
