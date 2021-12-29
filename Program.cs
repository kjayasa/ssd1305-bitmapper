using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using HandlebarsDotNet;
using System.Text.RegularExpressions;
using CommandLine;

#nullable enable

[Verb("image", HelpText = "convert an image")]
class ImageOptions
{
    [Option('i', "image-path", Default = true, Required = true, HelpText = "Image file to be processed.")]
    public string ImagePath { get; set; }

    [Option('o', "outpath-path", Required = true, HelpText = "Path to output file")]
    public string OutFilePath { get; set; }

    [Option('t', "template", Required = false, HelpText = "Path to handebar template to be used")]
    public string TemplateFilePath { get; set; }

}

[Verb("font", HelpText = "convert a font")]
class FontOptions
{
    [Option('f', "font-path", Default = true, Required = true, HelpText = "Font file to be processed.")]
    public string FontPath { get; set; }

    [Option('s', "size", Required = true, HelpText = "font size")]
    public int Size { get; set; }

    [Option('o', "outpath-path", Required = true, HelpText = "Path to output file")]
    public string OutFilePath { get; set; }

    [Option('t', "template", Required = false, HelpText = "Path to handebar template to be used")]
    public string TemplateFilePath { get; set; }

}


class App
{

    const string imageConvertTemplateFilePath = "./templates/image-template.handlebars";
    const string fontConvertTemplateFilePath = "./templates/font-template.handlebars";

    public static int Main(string[] args)
    {
        return Parser.Default.ParseArguments<ImageOptions, FontOptions>(args)
          .MapResult(
            (ImageOptions opts) => RunConvertImage(opts),
            (FontOptions opts) => RunConvertFont(opts),
            errs => 1);
    }

    private static int RunConvertFont(FontOptions opts)
    {
        throw new NotImplementedException();
    }

    private static int RunConvertImage(ImageOptions opts)
    {
        try
        {
            var bitmap = Bitmap.FromPath(opts.ImagePath);


            var template = new Template(opts.TemplateFilePath ?? imageConvertTemplateFilePath);
            template.RenderTemplate(new
            {
                imageName = Template.IdentifierFormFilePath(opts.ImagePath),
                height = bitmap.Height,
                width = bitmap.Width,
                data = bitmap.Data
            }, opts.OutFilePath);

            Console.WriteLine($"Rendered output to file {opts.OutFilePath}");
            return 0;

        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
            return -1;
        }
    }



}