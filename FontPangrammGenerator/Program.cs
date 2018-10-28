using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;

namespace FontPangrammGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string fontsPath = @"E:\Files";
            string text = @"Съешь ещё этих мягких французских булок, да выпей же чаю. 1234567890.";

            var info = FileSystemHelper.GetFontsFiles(fontsPath).ToList();

            info = info.Where(x => x.Contains(".ttf") || x.Contains(".otf")).ToList();
            info.ForEach(x =>
            {
                FontInfo font = Fonts.GetFontInfo(x);

                string path = fontsPath + "\\" + font.Name;
                string newName = path + "\\" + font.ToString();

                FileSystemHelper.CreateDirectory(path);
                FileSystemHelper.ReplaceFile(x, newName);

                font.Path = newName;

                Image pangramm = FontPangramm.Generate(text, font, Color.Black, Color.White);

                pangramm.Save(String.Format("{0}\\pangramm_{1}_{2}.png", fontsPath, font.Name, font.Style), ImageFormat.Png);
            });

            Console.ReadKey();
        }

    }

    public static class FontPangramm
    {
        public static Image Generate(string text, FontInfo fontInfo, Color textColor, Color backColor)
        {
            PrivateFontCollection fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile(fontInfo.Path);

            var currentFontFamuly = fontCollection.Families[fontCollection.Families.Length - 1];

            Font font = new Font(currentFontFamuly, 14f);

            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            SizeF textSize = drawing.MeasureString(text, font);
            SizeF fontNameSize = drawing.MeasureString(fontInfo.GetFullName(), font);


            img.Dispose();
            drawing.Dispose();

            img = new Bitmap(520, 200);

            drawing = Graphics.FromImage(img);
            drawing.Clear(backColor);

            Brush textBrush = new SolidBrush(textColor);

            RectangleF rectangleF = new Rectangle(10, 65, 500, 200);

            drawing.DrawString(text, font, textBrush, rectangleF);
            drawing.DrawString(fontInfo.GetFullName(), font, textBrush, (img.Width / 2) - ((int)fontNameSize.Width / 2), 20);

            drawing.Save();

            textBrush.Dispose();
            fontCollection.Dispose();
            drawing.Dispose();

            return img;
        }
    }

    public static class FileSystemHelper
    {
        public static void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public static void ReplaceFile(string file, string path)
        {
            if (!File.Exists(path))
            {
                File.Move(file, path);
            } else
            {
                Console.WriteLine("File existed:");
                Console.WriteLine(file);
                Console.WriteLine(path);
            }
        }

        public static IEnumerable<string> GetFontsFiles(string path)
        {
            return Directory.EnumerateFiles(path);
        }
    }

    public static class Fonts
    {
        public static FontInfo GetFontInfo(string fontPath)
        {
            System.Windows.Media.GlyphTypeface font 
                = new System.Windows.Media.GlyphTypeface(new Uri(fontPath));

            return new FontInfo()
            {
                Name = font.FamilyNames.ToValuesString().Replace(" ", string.Empty),
                Style = font.FaceNames.ToValuesString(),
                Extension = Path.GetExtension(fontPath),
                Path = fontPath
            };
        }
    }
    
    public class FontInfo
    {
        public string Name { get; set; }

        public string Style { get; set; }

        public string Extension { get; set; }

        public string Path { get; set; }

        public string ToString()
        {
            return String.Format("{0} {1}{2}", Name, Style, Extension);
        }

        public string GetFullName()
        {
            return String.Format("{0} {1}", Name, Style);
        }
    }

    public static class StringHelper
    {
        public static string ToValuesString(this IDictionary<CultureInfo, string> dic)
        {
            return string.Join(" ", dic.Select(k => k.Value).ToArray());
        }
    }
}
