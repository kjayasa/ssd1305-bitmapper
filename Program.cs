using CommandLine;

#nullable enable

namespace Bitmapper
{

    [Verb("image", HelpText = "convert an image")]
    class ImageOptions
    {
        [Option('p', "image-path", Default = true, Required = true, HelpText = "Image file to be processed.")]
        public string ImagePath { get; set; }

        [Option('o', "outpath-path", Required = true, HelpText = "Path to output file")]
        public string OutFilePath { get; set; }

        [Option('t', "template", Required = false, HelpText = "Path to handebar template to be used")]
        public string TemplateFilePath { get; set; }

        [Option('b', "bias", Required = false, HelpText = "Luminance Bias .default is 128")]
        public byte? Bias { get; set; }



    }

    [Verb("font", HelpText = "convert a font")]
    class FontOptions
    {
        [Option('p', "font-path", SetName = "fontName", HelpText = "Font file to be processed.")]
        public string FontPath { get; set; }

        [Option('n', "font-name", SetName = "fontName", HelpText = "Name of font already installed on system to be processed.")]
        public string FontName { get; set; }

        [Option('s', "size", Required = true, HelpText = "Size")]
        public byte Size { get; set; }

        [Option('o', "output-file", Required = true, HelpText = "Path to output file")]
        public string OutputFile { get; set; }

        [Option('t', "template", Required = false, HelpText = "Path to handebar template to be used")]
        public string TemplateFilePath { get; set; }

        [Option('c', "char-path", Required = false, HelpText = "Path to file with all chars")]
        public string CharSetPath { get; set; }

        [Option('b', "bias", Required = false, HelpText = "Luminance Bias .default is 128")]
        public byte? Bias { get; set; }

    }

    [Verb("fonts", HelpText = "convert multiple font")]
    class FontsOptions
    {
        [Option('p', "font-path", SetName = "fontName", HelpText = "Font file to be processed.")]
        public string FontPath { get; set; }


        [Option('s', "sizes", Required = true, HelpText = "font size sepeated by ,")]
        public string Sizes { get; set; }

        [Option('o', "outpath-path", Required = true, HelpText = "Path to output file")]
        public string OutFilePath { get; set; }

        [Option('t', "template", Required = false, HelpText = "Path to handebar template to be used")]
        public string TemplateFilePath { get; set; }

        [Option('c', "char-path", Required = false, HelpText = "Path to file with all chars")]
        public string CharSetPath { get; set; }

        [Option('b', "bias", Required = false, HelpText = "Luminance Bias .default is 128")]
        public byte? Bias { get; set; }

    }

    class Program
    {

        const string imageConvertTemplateFilePath = "./templates/image-template.handlebars";
        const string fontConvertTemplateFilePath = "./templates/font-template.handlebars";

        public static int Main(string[] args)
        {
            try
            {
                return Parser.Default.ParseArguments<ImageOptions, FontOptions, FontsOptions>(args)
             .MapResult(
               (ImageOptions opts) => RunConvertImage(opts),
               (FontOptions opts) => RunConvertFont(opts),
                (FontsOptions opts) => RunConvertFonts(opts),
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
            FontMap fontmap = null;



            if (!string.IsNullOrWhiteSpace(opts.FontName))
            {
                fontmap = FontMap.FromSystemFont(opts.FontName, opts.Size, opts.CharSetPath, opts.Bias);

            }
            else if (!string.IsNullOrWhiteSpace(opts.FontPath))
            {
                fontmap = FontMap.FromFontFile(opts.FontPath, opts.Size, opts.CharSetPath, opts.Bias);

            }
            else
            {
                throw new ApplicationException("Invalid font specification");
            }

            var template = new Template(opts.TemplateFilePath ?? fontConvertTemplateFilePath);
            template.RenderTemplate(fontmap, opts.OutputFile);

            Console.WriteLine($"Rendered output to file {opts.OutputFile}");



            return 0;
        }


        private static int RunConvertFonts(FontsOptions opts)
        {

            var sizes = opts.Sizes.Split(',').Select(s => Convert.ToInt32(s));
            //TODO: filtes out unsupported extensions
            //var vaildExtensions = "ttf;woff;woff2".Split(";");
            //foreach (var fontfile in Directory.EnumerateFiles(opts.FontPath).Where(p => vaildExtensions.Contains(Path.GetExtension(p))))
            foreach (var fontfile in Directory.EnumerateFiles(opts.FontPath))
            {
                foreach (var size in sizes)
                {
                    var fontmap = FontMap.FromFontFile(fontfile, size, opts.CharSetPath, opts.Bias);

                    var outputFileNAme =Path.Combine(opts.OutFilePath,$"{Path.GetFileNameWithoutExtension(fontfile)}-{size}.c");

                    var template = new Template(opts.TemplateFilePath ?? fontConvertTemplateFilePath);
                    template.RenderTemplate(fontmap, outputFileNAme);

                    Console.WriteLine($"Rendered output to file {outputFileNAme}");

                }

            }



            return 0;
        }

        private static int RunConvertImage(ImageOptions opts)
        {
            var bitmap = Bitmap.FromPath(opts.ImagePath, opts.Bias);


            var template = new Template(opts.TemplateFilePath ?? imageConvertTemplateFilePath);
            template.RenderTemplate(new
            {
                ImageName = Path.GetFileNameWithoutExtension(opts.ImagePath),
                Bitmap = bitmap
            }, opts.OutFilePath);

            Console.WriteLine($"Rendered output to file {opts.OutFilePath}");
            return 0;
        }



    }
}

