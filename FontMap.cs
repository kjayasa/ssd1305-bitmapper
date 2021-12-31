using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Bitmapper
{

    public class Glyphs
    {
        public string Symbol { get; set; }
        public Bitmap Bitmap { get; set; }
        public uint Code { get; set; }
    }

    public class FontMap
    {
        public string FontName { get; set; }
        public int LineHight { get; set; }
        public IEnumerable<Glyphs> Glyphs { get; private set; }



        public static FontMap FromFontFile(string fontPath, int size, string? codepointsPath) =>
            Create(Path.GetFileNameWithoutExtension(fontPath), GetGlyphsFromCodepoint(GetFromFontFile(fontPath, size), codepointsPath));

        public static FontMap FromSystemFont(string fontName, int size, string? codepointsPath) =>
            Create(fontName, GetGlyphsFromCodepoint(GetSystemFont(fontName, size), codepointsPath));

        private static FontMap Create(string fontName, IEnumerable<Glyphs> glyphs) => new FontMap
        {
            Glyphs = glyphs,
            LineHight = glyphs.Max(g => g.Bitmap.Height),
            FontName = fontName
        };

        private static Font GetSystemFont(string fontName, int size)
        {
            var collection = SystemFonts.Find(fontName);

            if (collection != null)
            {
                return collection.CreateFont(size);
            }
            else
            {
                throw new ApplicationException($"{fontName} not found");
            }

        }

        private static Font GetFromFontFile(string fontPath, int size)
        {
            if (!File.Exists(fontPath))
            {
                throw new ApplicationException($"{fontPath} does not exist");

            }
            var collection = new FontCollection();
            var family = collection.Install(fontPath);

            if (collection != null)
            {
                return family.CreateFont(size, FontStyle.Regular);
            }
            else
            {
                throw new ArgumentException($"Unable to load {fontPath}");
            }

        }

        private static IEnumerable<Glyphs> GetGlyphsFromCodepoint(Font font, string codepointsPath)
        {
           return GetGlyphs(font,string.IsNullOrWhiteSpace(codepointsPath)?GetAllASCIIGlyphs():GetAllGlyphs(codepointsPath));

        }


        private static IEnumerable<Glyphs> GetGlyphs(Font font,  IEnumerable<(string Symbol, uint Code)> allGlyphs)
        {          

            var ret = new List<Glyphs>();
            foreach (var glyph in allGlyphs)
            {

                var options = new RendererOptions(font);

                var rect = TextMeasurer.Measure(glyph.Symbol, options);


                using (var image = new Image<L8>((int)Math.Ceiling(rect.Width), (int)Math.Ceiling(rect.Height), new L8(0)))
                {
                    image.Mutate(i => i.DrawText(glyph.Symbol, font, Color.White, new PointF(0, 0)));
                    ret.Add(new Glyphs
                    {
                        Symbol = glyph.Symbol,
                        Code = glyph.Code,
                        Bitmap = Bitmap.FromImage(image)
                    });

                }
            }

            return ret;

        }


         private static IEnumerable<(string Symbol, uint Code)> GetAllASCIIGlyphs()
        {
            var ret = new List<(string Symbol, uint Code)>();
            for (uint asciiValue = 32; asciiValue <= 127; asciiValue++)
            {
                ret.Add(($"{Convert.ToChar(asciiValue)}", asciiValue));
            }
            return ret;

        }


        private static IEnumerable<(string Symbol, uint Code)> GetAllGlyphs(string codepointsPath)
        {

            if (!File.Exists(codepointsPath))
            {
                throw new ApplicationException($"{codepointsPath} does not exist");

            }

            var ret = new List<(string Symbol, uint Code)>();

            var codepoints = File.ReadAllText(codepointsPath).Split(',');

            var unicode = new UnicodeEncoding();

            foreach (var codepoint_string in codepoints)
            {
                if (uint.TryParse(codepoint_string, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var codePoint))
                {

                    var charSring = unicode.GetString(BitConverter.GetBytes(codePoint));

                    if (charSring.Last() == '\0')
                    {
                        charSring = charSring.Remove(charSring.Length - 1);
                    }

                    ret.Add((charSring, codePoint));

                }

            }

            return ret;

        }

    }
}
