using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text;

namespace Bitmapper
{
    public class Bitmap
    {

        public int Height { get; private set; }
        public int Width { get; private set; }
        public string Data { get; private set; }

        private Bitmap()
        {
            Height = 0;
            Width = 0;
            Data = "";
        }

        public static Bitmap FromImage(Image<L8> image,byte? tolerance)
        {
            var code = new StringBuilder();
            byte bias=tolerance??128;

            for (int y = 0; y < image.Height; y++)
            {
                var pixelRowSpan = image.GetPixelRowSpan(y);
                for (int x = 0; x < image.Width; x += 8)
                {
                    var p1 = (x + 0) < image.Width ? x + 0 : x;
                    var p2 = (x + 1) < image.Width ? x + 1 : x;
                    var p3 = (x + 2) < image.Width ? x + 2 : x;
                    var p4 = (x + 3) < image.Width ? x + 3 : x;
                    var p5 = (x + 4) < image.Width ? x + 4 : x;
                    var p6 = (x + 5) < image.Width ? x + 5 : x;
                    var p7 = (x + 6) < image.Width ? x + 6 : x;
                    var p8 = (x + 7) < image.Width ? x + 7 : x;

                    byte d = (byte)
                        (((pixelRowSpan[p1].PackedValue > bias) ? (1 << 7) : 0)
                       | ((pixelRowSpan[p2].PackedValue > bias) ? (1 << 6) : 0)
                       | ((pixelRowSpan[p3].PackedValue > bias) ? (1 << 5) : 0)
                       | ((pixelRowSpan[p4].PackedValue > bias) ? (1 << 4) : 0)
                       | ((pixelRowSpan[p5].PackedValue > bias) ? (1 << 3) : 0)
                       | ((pixelRowSpan[p6].PackedValue > bias) ? (1 << 2) : 0)
                       | ((pixelRowSpan[p7].PackedValue > bias) ? (1 << 1) : 0)
                       | ((pixelRowSpan[p8].PackedValue > bias) ? (1 << 0) : 0));

                    code.AppendFormat("0X{0:X2},", d);
                }

                code.AppendLine();
            }
            return new Bitmap
            {
                Height = image.Height,
                Width = image.Width,
                Data = code.ToString()

            };
        }

        public static Bitmap FromPath(string path,byte? tolerance)
        {
            if (!File.Exists(path))
            {
                throw new ApplicationException($"{path} does not exist");

            }

            using (var image = Image.Load<L8>(path))
            {
                return FromImage(image,tolerance);
            }

        }

    }
}


