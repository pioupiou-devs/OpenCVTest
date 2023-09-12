
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
    Rect window = new(new Point(fragment.X, fragment.Y), new Size(translated.Width, translated.Height));

    Console.WriteLine($"1/ Fragment {fragment.Number} at ({fragment.X}, {fragment.Y}) with angle {fragment.Angle}\n Background [{0}->{background.Width}, {0}->{background.Height}]\n Translated [{fragment.X}->{fragment.X + translated.Width}, {fragment.Y}->{fragment.Y + translated.Height}]\n");

    Rect newWindow = new Rect(0, 0, translated.Width, translated.Height);
    bool modified = false;
    if (window.X <= 0)
    {
        newWindow.X = window.X;
        window.X = 0;
        modified = true;
    }

    if (window.Width + window.X >= background.Width)
    {
        int delta = background.Width - window.Width;
        if (delta <= background.Width / 2)
        {
            window.Width -= delta / 2;
            window.X += delta / 2;
        }
        else
            window.Width -= delta;
        
        newWindow.Width = window.Width;
        newWindow.X = window.X;
        modified = true;
    }

    if (window.Y <= 0)
    {
        newWindow.Y = window.Y;
        window.Y = 0;
        modified = true;
    }
    if ((window.Height + window.Y >= background.Height))
    {
        int delta = background.Height - window.Height;

        if (delta <= background.Height / 2)
        {
            window.Height -= delta / 2;
            window.Y += delta / 2;
        }
        else
            window.Height -= delta;

        newWindow.Height = window.Height;
        newWindow.Y = window.Y;

        modified = true;
    }

    if (modified)
    {
        Cv2.ImShow("before", translated);
        translated = translated[newWindow];
        alpha = alpha[newWindow];
        Cv2.ImShow("after", translated);
        Cv2.WaitKey(0);

        Console.WriteLine($"\tWindows : [{window.X},{window.Y};{window.Width + window.X},{window.Height + window.Y}]");
        Console.WriteLine($"\tBackground : [0,0;{background.Width},{background.Height}]");
    }
    // Apply dst with alpha channel to background and ignore the out of bounds pixels
    var temp = background[window];
    translated.CopyTo(temp, alpha);
}

Cv2.ImShow("Background", background);
Cv2.WaitKey(0);
