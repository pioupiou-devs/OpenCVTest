
using OpenCvSharp;

using OpenCVTest;

Scalar WHITE = new(255, 255, 255);
Scalar BLACK = new(0, 0, 0);

List<Fragment> fragmentList = Utils.ExtractFragments(@"Resources\fragments.txt", true);
List<Fragment> imageList = Utils.ExtractFragments(@"Resources\frag_eroded");

int width, height;
(width, height) = Utils.GetSizeFromImage(@"Resources\Michelangelo_ThecreationofAdam_1707x775.jpg");

// White background
Mat background = new(height, width, MatType.CV_8UC4, WHITE);

// Filter fragment with fragment_s.txt
List<int> fragmentToDelete = File.ReadAllLines(@"Resources\fragments_s.txt")
    .Select(l => int.Parse(l.Trim()))
    .ToList();
imageList.RemoveAll(f => fragmentToDelete.Contains(f.Number));

// Combine fragment list and image list by number
foreach (Fragment frag in fragmentList)
{
    Fragment? img = imageList.Find(f => f.Number == frag.Number);
    if (img is not null)
        frag.Image = img?.Image;
}
fragmentList.RemoveAll(f => f.Image is null);

int count = 10;
// Draw fragment on background
foreach (Fragment fragment in fragmentList)
{
    Mat? image = fragment.Image;
    if (image is null)
        continue;

    Mat translated = image.Clone();

    // Rotate and translate image
    Cv2.WarpAffine(image, translated, Cv2.GetRotationMatrix2D(new Point2f(image.Width / 2, image.Height / 2), fragment.Angle, 1.0), image.Size());

    // TODO : Find how to re-center after rotation

    // Get the alpha channel from translated
    Mat alpha = new();
    Cv2.ExtractChannel(translated, alpha, 3);

    // Calculate the window
    Rect window = new(new Point(fragment.X, fragment.Y), new Size(translated.Width, translated.Height));
    try
    {
        // Apply translated with alpha channel to background
        var temp = background[window];
        translated.CopyTo(temp, alpha);
    }
    catch (Exception e)
    {
        // TODO : Find how to apply a fragment where his transparant part is out of bounds
        // TODO : For the rest, find why they're not included with the window defined line 53

        Console.WriteLine($"1/ Fragment {fragment.Number} at ({fragment.X}, {fragment.Y}) with angle {fragment.Angle}\n Background [{0}->{background.Width}, {0}->{background.Height}]\n Translated [{fragment.X}->{fragment.X + translated.Width}, {fragment.Y}->{fragment.Y + translated.Height}]\n");
        continue;
    }
}

Cv2.ImShow("Background", background);
Cv2.WaitKey(0);
