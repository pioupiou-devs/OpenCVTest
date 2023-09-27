
using System.Security.Principal;

using OpenCvSharp;

using OpenCVTest;

Scalar WHITE = new(255, 255, 255);
Scalar BLACK = new(0, 0, 0);

List<Fragment> fragmentList = Utils.ExtractFragments(@"Resources\fragments.txt", true);
List<Fragment> imageList = Utils.ExtractFragments(@"Resources\frag_eroded");

int width, height;
(width, height) = Utils.GetSizeFromImage(@"Resources\Michelangelo_ThecreationofAdam_1707x775.jpg");
var realImage = Cv2.ImRead(@"Resources\Michelangelo_ThecreationofAdam_1707x775.jpg", ImreadModes.Unchanged);

// White background
Mat background = new(height, width, MatType.CV_8UC4, WHITE);

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
foreach (Fragment fragment in fragmentList)//new List<Fragment> (){fragmentList.FirstOrDefault()}) //TODO : Remove FirstOrDefault
{
    Mat? image = fragment.Image;
    if (image is null)
        continue;

    Mat translated = image.Clone();

    // Rotate and translate image
    Mat rotationMatrix = Cv2.GetRotationMatrix2D(new Point2f(image.Width/2, image.Height / 2), fragment.Angle, 1.0);
    Cv2.WarpAffine(image, translated, rotationMatrix, image.Size());
    //Utils.PrintImage(nameof(image), image);
    ////Utils.PrintMat(nameof(rotationMatrix), rotationMatrix);
    //Utils.PrintImage(nameof(translated), translated);
    //Cv2.WaitKey(0);

    // Get the alpha channel from translated
    Mat alpha = new();
    Cv2.ExtractChannel(translated, alpha, 3);

    // Calculate the window
    Rect window = new(
        new Point(fragment.X- (translated.Width / 2), fragment.Y - (translated.Height / 2)), 
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
        Console.WriteLine($"1/ Fragment {fragment.Number} at ({fragment.X}, {fragment.Y}) with angle {fragment.Angle}\n Background [{0}->{background.Width}, {0}->{background.Height}]\n Translated [{fragment.X}->{fragment.X + translated.Width}, {fragment.Y}->{fragment.Y + translated.Height}]\n");
        count++;
        continue;
    }
    translated.CopyTo(temp, alpha);
}

Console.WriteLine($"Count = {count}");

Cv2.ImShow("Background", background);
Cv2.WaitKey(0);
