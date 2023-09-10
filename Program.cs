
using System.Text.RegularExpressions;

using OpenCvSharp;

using OpenCVTest;

Scalar WHITE = new(255, 255, 255);
Scalar BLACK = new(0, 0, 0);

List<Fragment> fragmentList = Utils.ExtractFragments(@"Resources\fragments.txt", true);
List<Fragment> imageList = Utils.ExtractFragments(@"Resources\frag_eroded");

int width, height;
(width, height) = Utils.GetSizeFromImage(@"Resources\Michelangelo_ThecreationofAdam_1707x775.jpg");

// White background
Mat background = new(height, width, MatType.CV_8UC3, WHITE);

// Filter fragment with fragment_s.txt
List<int> fragmentToDelete = File.ReadAllLines(@"Resources\fragments_s.txt")
    .Select(l => int.Parse(l.Trim()))
    .ToList();
imageList.RemoveAll(f => fragmentToDelete.Contains(f.Number));

// Combine fragment list and image list by number
foreach (var fragment in fragmentList)
{
    Fragment? image = imageList.Find(f => f.Number == fragment.Number);
    if (image is not null)
        fragment.Image = image?.Image;
}
fragmentList.RemoveAll(f => f.Image is null);

// Draw fragment on background
foreach (var fragment in fragmentList)
{
    Mat? image = fragment.Image;
    if (image is null)
        continue;

    Mat translated = image.Clone();

    // Rotate and translate image
    Cv2.WarpAffine(image, translated, Cv2.GetRotationMatrix2D(new Point2f(image.Width / 2, image.Height / 2), fragment.Angle, 1.0), image.Size());

    Console.WriteLine($"Fragment {fragment.Number} at ({fragment.X}, {fragment.Y}) with angle {fragment.Angle}\n Background [{0}->{background.Width}, {0}->{background.Height}]\n Frag [{fragment.X}->{fragment.X + translated.Width}, {fragment.Y}->{fragment.Y + translated.Height}]\n");

    // Add image to background
    translated.CopyTo(background[new Rect((int)fragment.X, (int)fragment.Y, translated.Width, translated.Height)]);

}

Cv2.ImShow("Background", background);
Cv2.WaitKey(0);
