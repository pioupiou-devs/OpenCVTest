
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

    // Get the alpha channel from translated
    Mat alpha = new();
    Cv2.ExtractChannel(translated, alpha, 3);



    // Calculate the window
    Rect window = new((int)fragment.X - translated.Width / 2, (int)fragment.Y - translated.Height / 2, translated.Width, translated.Height);

    if (window.X < 0 || window.Y < 0 || window.X + window.Width > background.Width || window.Y + window.Height > background.Height)
    {
        Console.WriteLine($"1/ Fragment {fragment.Number} at ({fragment.X}, {fragment.Y}) with angle {fragment.Angle}\n Background [{0}->{background.Width}, {0}->{background.Height}]\n Translated [{fragment.X}->{fragment.X + translated.Width}, {fragment.Y}->{fragment.Y + translated.Height}]\n");

        // Crop the translated image to fit the background
        Rect crop = new(0,0,translated.Width,translated.Height);

        if (window.X < 0)
        {
            crop.Width = window.X;
        }
        else if (window.X + translated.Width > background.Width)
        {
            crop.Width = background.Width - (window.X + translated.Width);
        }
        else if (window.Y < 0)
        {
            crop.Height = window.Y;
        }
        else if (window.Y + translated.Height > background.Height)
        {
            crop.Height = background.Height - (window.Y + translated.Height);
        }

        crop.Width = Math.Abs(crop.Width);
        crop.Height = Math.Abs(crop.Height);

        translated = translated[crop];
        alpha = alpha[crop];
        
        Console.WriteLine($"2/ Fragment {fragment.Number} at ({fragment.X}, {fragment.Y}) with angle {fragment.Angle}\n Background [{0}->{background.Width}, {0}->{background.Height}]\n Translated [{fragment.X}->{fragment.X + translated.Width}, {fragment.Y}->{fragment.Y + translated.Height}]\n by crop [{crop.X}->{crop.Width},{crop.Y}->{crop.Height}]");
    }

    // Apply dst with alpha channel to background and ignore the out of bounds pixels
    translated.CopyTo(background[window], alpha);
}

Cv2.ImShow("Background", background);
Cv2.WaitKey(0);
