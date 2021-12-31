using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bitmapper
{
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

            RegesterHelpers();

            var templateText = File.ReadAllText(templatePath);
            this.template = Handlebars.Compile(templateText);

        }

        private void RegesterHelpers()
        {
            Handlebars.RegisterHelper("to_itentifier", (writer, context, parameters) =>
            {
                writer.Write(replaceChars.Replace((parameters[0]?.ToString()?? "").ToLower(),"_"));
            });

        }

        public void RenderTemplate(object data, string outputFile) => File.WriteAllText(outputFile, this.template(data));

    }
}

