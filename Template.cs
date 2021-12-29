using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

internal class Template
{
    private static Regex replaceChars = new Regex("[^\\w\\d_]");
    private readonly HandlebarsTemplate<object, object> template;

    public Template(string templatePath)
    {
        if (!File.Exists(templatePath))
        {
            throw new ApplicationException($"Template file missing at {templatePath}");
        }

        var templateText = File.ReadAllText(templatePath);
        this.template = Handlebars.Compile(templateText);
    }

    public void RenderTemplate(object data, string outputFile) => File.WriteAllText(outputFile, this.template(data));

    public static string IdentifierFormFilePath(string filepath)
    {
        var filename = replaceChars.Replace(Path.GetFileName(filepath), "_");

        if (!char.IsLetter(filename, 0))
        {
            filename = $"_{filename}";
        }

        return filename;


    }

}

