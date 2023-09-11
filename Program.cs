
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

// Draw fragment on background
foreach (Fragment fragment in fragmentList)
{
    Mat? image = fragment.Image;
    if (image is null)
        continue;

    Mat translated = image.Clone();

    // Rotate and translate image
    Cv2.WarpAffine(image, translated, Cv2.GetRotationMatrix2D(new Point2f(image.Width / 2, image.Height / 2), fragment.Angle, 1.0), image.Size());

    Console.WriteLine($"Fragment {fragment.Number} at ({fragment.X}, {fragment.Y}) with angle {fragment.Angle}\n Background [{0}->{background.Width}, {0}->{background.Height}]\n Frag [{fragment.X}->{fragment.X + translated.Width / 2}, {fragment.Y}->{fragment.Y + translated.Height / 2}]\n");

    // Add image to background
    Rect rect = new((int)fragment.X - translated.Width / 2, (int)fragment.Y - translated.Height / 2, translated.Width, translated.Height);
    if (rect.X < 0 || rect.Y < 0 || rect.X + rect.Width > background.Width || rect.Y + rect.Height > background.Height)
        continue;
    var temp = background[rect];
    Mat dst = new Mat();
    Cv2.AddWeighted(temp, 0.0, translated, 1.0, 0.0, dst);

    background[rect] = dst;


}

Cv2.ImShow("Background", background);
Cv2.WaitKey(0);
