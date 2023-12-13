using OpenCvSharp;

using OpenCVTest;

Mat realImage = Cv2.ImRead(@"Resources\Michelangelo_ThecreationofAdam_1707x775.jpg", ImreadModes.Unchanged);
List<Fragment> imageList = Utils.ExtractFragments(@"Resources\frag_eroded", notNullOnly: true);

ImageReconstruction reconstructor = new();
reconstructor.ReconstructImage(realImage, imageList);

//Cv2.ImShow("Reconstructed image", reconstructedimage);

/*
Scalar WHITE = new(255, 255, 255);
Scalar BLACK = new(0, 0, 0);

List<Fragment> fragmentList = Utils.ExtractFragments(@"Resources\fragments.txt", true);
List<Fragment> imageList = Utils.ExtractFragments(@"Resources\frag_eroded");

int width, height;
(width, height) = Utils.GetSizeFromImage(@"Resources\Michelangelo_ThecreationofAdam_1707x775.jpg");
var realImage = Cv2.ImRead(@"Resources\Michelangelo_ThecreationofAdam_1707x775.jpg", ImreadModes.Unchanged);

int deltaW = 200, deltaH = 200, deltaX = 200, deltaY = 200;

// White background
Mat background = new(height + (deltaY + deltaH), width + (deltaX + deltaW), MatType.CV_8UC4, BACKGROUND_COLOR);

// Add greyscaled realImage to background at position (deltaX, deltaY) with alpha  30%
Mat realImageGrey = new();
Mat realImageAlpha = new();

Cv2.CvtColor(realImage, realImageGrey, ColorConversionCodes.BGRA2GRAY);
Cv2.CvtColor(realImageGrey, realImageAlpha, ColorConversionCodes.BGR2BGRA);

Mat alphaN = new();
Cv2.ExtractChannel(realImageAlpha, alphaN, 3);
Mat tempImage = background[new Rect(new Point(deltaX, deltaY), realImage.Size())];
Cv2.AddWeighted(tempImage, 0.7, realImageAlpha, 0.3, 0, tempImage);
realImageAlpha.CopyTo(tempImage, alphaN);

Console.WriteLine($"Fragment list count = {fragmentList.Count}, Image list count = {imageList.Count}");

// Combine fragment list and image list by number
foreach (Fragment frag in fragmentList)
{
    Fragment? img = imageList.Find(f => f.Number == frag.Number);
    if (img is not null)
        frag.Image = img?.Image;
}
fragmentList.RemoveAll(f => f.Image is null);

int count = 0;
// Draw fragment on background
foreach (Fragment fragment in fragmentList)
{
    Mat? image = fragment.Image;

    Mat translated = image?.Clone();

    // Rotate and translate image
    Mat rotationMatrix = Cv2.GetRotationMatrix2D(new Point2f(image.Width / 2, image.Height / 2), fragment.Angle, 1.0);
    Cv2.WarpAffine(image, translated, rotationMatrix, image.Size());

    // Get the alpha channel from translated
    Mat alpha = new();
    Cv2.ExtractChannel(translated, alpha, 3);

    // Calculate the window
    Rect window = new(
        new Point((fragment.X - (translated.Width / 2)) + deltaX, (fragment.Y - (translated.Height / 2)) + deltaY),
        new Size(translated.Width, translated.Height));
    var temp = null as Mat;
    try
    {
        // Apply translated with alpha channel to background
        temp = background[window];
    }
    catch (Exception e)
    {

        Console.WriteLine($"error : {e.Message}, when working on fragment {fragment.Number}");
        count++;
        continue;
    }
    translated.CopyTo(temp, alpha);
}

// Crop the deltas
Rect crop = new(deltaX, deltaY, width, height);
background = background[crop];

Console.WriteLine($"Count of error {count} on a total of {imageList.Count} fragments. The number of listed fragments is {fragmentList.Count}");

// Evaluate the score
Evaluator evaluator = new(1, 1, 1);
evaluator.PrintScore(@"solution.txt");

Cv2.ImShow("Background", background);
Cv2.WaitKey(0);
*/