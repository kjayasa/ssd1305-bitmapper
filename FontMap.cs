using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
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



        public static FontMap FromFontFile(string fontPath, int size, string? codepointsPath, byte? tolerance) =>
            Create(System.IO.Path.GetFileNameWithoutExtension(fontPath), GetGlyphsFromCodepoint(GetFromFontFile(fontPath, size), codepointsPath, tolerance));

        public static FontMap FromSystemFont(string fontName, int size, string? codepointsPath, byte? tolerance) =>
            Create(fontName, GetGlyphsFromCodepoint(GetSystemFont(fontName, size), codepointsPath, tolerance));

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

        private static IEnumerable<Glyphs> GetGlyphsFromCodepoint(Font font, string codepointsPath, byte? tolerance)
        {
            return GetGlyphs(font, string.IsNullOrWhiteSpace(codepointsPath) ? GetAllASCIIGlyphs() : GetAllGlyphs(codepointsPath), tolerance);

        }


        private static IEnumerable<Glyphs> GetGlyphs(Font font, IEnumerable<(string Symbol, uint Code)> allGlyphs, byte? tolerance)
        {

            var ret = new List<Glyphs>();
            foreach (var glyph in allGlyphs)
            {
                using (var image = GenerateGlyphImage(glyph.Symbol, font))
                {
                    ret.Add(new Glyphs
                    {
                        Symbol = glyph.Symbol,
                        Code = glyph.Code,
                        Bitmap = Bitmap.FromImage(image, tolerance)
                    });

                    if(glyph.Code==65){
                        image.SaveAsBmp($"./sample/{font.Name}-{font.Size}-{glyph.Code}.bmp");
                    }

                    
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


        //based on https://stackoverflow.com/a/53023454
        //thanks  https://stackoverflow.com/users/234855/tocsoft ,https://stackoverflow.com/users/10167996/flashover 
        private static Image<L8> GenerateGlyphImage(string text, Font font)
        {

            RendererOptions style = new RendererOptions(font, 72);
            var glyphs = TextBuilder.GenerateGlyphs(text, style);

            var glyphHeight = glyphs.Bounds.Height > 0 ? glyphs.Bounds.Height : 1;
            var scale = (font.Size / glyphHeight);

            if (scale < 1)
            { // the renderd glyph is larger than font height scale it to fit
                glyphs = glyphs.Scale(scale);
                glyphHeight = font.Size;
            }

            var glyphwidth = glyphs.Bounds.Width > 0 ? glyphs.Bounds.Width : font.Size;
            glyphs = glyphs.Translate(-glyphs.Bounds.Location); // move to top left corner

            var img = new Image<L8>(Convert.ToInt32(Math.Ceiling(glyphwidth)),Convert.ToInt32(Math.Ceiling(glyphHeight)), new L8(0));

            img.Mutate(i => i.Fill(new DrawingOptions { GraphicsOptions = new GraphicsOptions { Antialias = false } }, Brushes.Solid(Color.White), glyphs));
            return img;

        }

    }
}
