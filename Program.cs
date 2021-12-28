using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using HandlebarsDotNet;
using System.Text.RegularExpressions;

class App
{
    private static Regex replaceChars = new Regex("[\\s\\.~\\(\\)-]");
    const string templateFilePath = "./template.handlebars";
    public static void Main(string[] args)
    {
        try
        {
            if (!File.Exists(templateFilePath))
            {
               throw new ApplicationException("Template file missing");

            }

            if (args.Length == 2)
            {
                if (!File.Exists(args[0]))
                {
                   throw new ApplicationException($"{args[0]} does not exist");

                }

                using (var image = Image.Load<Rgb24>(args[0]))
                {
                    var code = new StringBuilder();
                    for (int y = 0; y < image.Height; y++)
                    {
                        var pixelRowSpan = image.GetPixelRowSpan(y);

                        // for (int x = 0; x < image.Width; x++)
                        // {
                        //     if (pixelRowSpan[x].B > 128 || pixelRowSpan[x].G > 128 || pixelRowSpan[x].R > 128)
                        //     {
                        //         pixelLine.Add("0x1");
                        //     }
                        //     else
                        //     {
                        //         pixelLine.Add("0x0");
                        //     }
                        // }
 
                        for (int x = 0; x < image.Width; x += 8)
                        {
                            var p1 = (x + 0) < image.Width ? x + 0 :x ;
                            var p2 = (x + 1) < image.Width ? x + 1 :x ;
                            var p3 = (x + 2) < image.Width ? x + 2 :x ;
                            var p4 = (x + 3) < image.Width ? x + 3 :x ;
                            var p5 = (x + 4) < image.Width ? x + 4 :x ;
                            var p6 = (x + 5) < image.Width ? x + 5 :x ;
                            var p7 = (x + 6) < image.Width ? x + 6 :x ;
                            var p8 = (x + 7) < image.Width ? x + 7 :x ;

                            byte d = (byte)
                                (((pixelRowSpan[p1].B > 128 || pixelRowSpan[p1].G > 128 || pixelRowSpan[p1].R > 128) ? (1 << 7) : 0)
                                | ((pixelRowSpan[p2].B > 128 || pixelRowSpan[p2].G > 128 || pixelRowSpan[p2].R > 128) ? (1 << 6) : 0)
                                | ((pixelRowSpan[p3].B > 128 || pixelRowSpan[p3].G > 128 || pixelRowSpan[p3].R > 128) ? (1 << 5) : 0)
                                | ((pixelRowSpan[p4].B > 128 || pixelRowSpan[p4].G > 128 || pixelRowSpan[p4].R > 128) ? (1 << 4) : 0)
                                | ((pixelRowSpan[p5].B > 128 || pixelRowSpan[p5].G > 128 || pixelRowSpan[p5].R > 128) ? (1 << 3) : 0)
                                | ((pixelRowSpan[p6].B > 128 || pixelRowSpan[p6].G > 128 || pixelRowSpan[p6].R > 128) ? (1 << 2) : 0)
                                | ((pixelRowSpan[p7].B > 128 || pixelRowSpan[p7].G > 128 || pixelRowSpan[p7].R > 128) ? (1 << 1) : 0)
                                | ((pixelRowSpan[p8].B > 128 || pixelRowSpan[p8].G > 128 || pixelRowSpan[p8].R > 128) ? (1 << 0) : 0));

                            code.AppendFormat("0X{0:X2},",d);
                        }

                        code.AppendLine();
                    }

                    var imagePath = Path.GetFileName(args[0]);
                    imagePath = replaceChars.Replace(imagePath, "_");

                    var templateText = File.ReadAllText("./template.handlebars");

                    var template = Handlebars.Compile(templateText);

                    var data = new
                    {
                        imageName = imagePath,
                        height = image.Height,
                        width = image.Width,
                        data = code.ToString()
                    };

                    var result = template(data);
                    File.WriteAllText(args[1], result);

                }

            }
            else
            {
                throw new ApplicationException("Unexpected number of args\nUsgae: bitmapper <src.bpm> <dest.c>");
            }
        }
        catch (System.Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }

    }

}