using CommandLine;

#nullable enable

namespace Bitmapper
{

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
        [Option('f', "font-path", SetName = "fontName", HelpText = "Font file to be processed.")]
        public string FontPath { get; set; }

        [Option('n', "font-name", SetName = "fontName", HelpText = "Name of font already installed on system to be processed.")]
        public string FontName { get; set; }

        [Option('s', "size", Required = true, HelpText = "font size")]
        public int Size { get; set; }

        [Option('o', "outpath-path", Required = true, HelpText = "Path to output file")]
        public string OutFilePath { get; set; }

        [Option('t', "template", Required = false, HelpText = "Path to handebar template to be used")]
        public string TemplateFilePath { get; set; }

        [Option('c', "char-path", Required = false, HelpText = "Path to file with all chars")]
        public string CharSetPath { get; set; }

    }


    class Program
    {

        const string imageConvertTemplateFilePath = "./templates/image-template.handlebars";
        const string fontConvertTemplateFilePath = "./templates/font-template.handlebars";

        public static int Main(string[] args)
        {
            try
            {
                return Parser.Default.ParseArguments<ImageOptions, FontOptions>(args)
             .MapResult(
               (ImageOptions opts) => RunConvertImage(opts),
               (FontOptions opts) => RunConvertFont(opts),
               errs => 1);

            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                return -1;
            }

        }

        private static int RunConvertFont(FontOptions opts)
        {
            FontMap fontmap=null;

            if (!string.IsNullOrWhiteSpace(opts.FontName))
            {
                 fontmap = FontMap.FromSystemFont(opts.FontName, opts.Size, opts.CharSetPath);

            }
            else if (!string.IsNullOrWhiteSpace(opts.FontPath))
            {
                 fontmap = FontMap.FromFontFile(opts.FontPath, opts.Size, opts.CharSetPath);
                
            }
            else
            {
                throw new ApplicationException("Invalid font specification");
            }

            var template = new Template(opts.TemplateFilePath ?? fontConvertTemplateFilePath);
            template.RenderTemplate(fontmap, opts.OutFilePath);

            Console.WriteLine($"Rendered output to file {opts.OutFilePath}");
            return 0;
        }

        private static int RunConvertImage(ImageOptions opts)
        {
            var bitmap = Bitmap.FromPath(opts.ImagePath);


            var template = new Template(opts.TemplateFilePath ?? imageConvertTemplateFilePath);
            template.RenderTemplate(new
            {
                ImageName = Path.GetFileNameWithoutExtension(opts.ImagePath),
                Bitmap=bitmap
            }, opts.OutFilePath);

            Console.WriteLine($"Rendered output to file {opts.OutFilePath}");
            return 0;
        }



    }
}

