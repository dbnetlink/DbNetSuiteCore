using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DbNetSuiteCore.Constants.Resource;
using DbNetSuiteCore.Models;

namespace DbNetSuiteCore.Services
{
    public class Resource : DbNetSuite
	{
        private readonly string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

        protected string Name => QueryParam("name");
        protected string FontSize => QueryParam("font-size") ?? Settings?.FontSize;
        protected string FontFamily => QueryParam("font-family") ?? Settings?.FontFamily;
        protected bool Debug => IsDebugMode();

        public Resource(AspNetCoreServices services) : base(services)
		{
		}

        public async Task<object> Process()
		{
            object result;
            switch (Action.ToLower())
			{
				case RequestAction.Script:
					result = await Script();
					break;
				case RequestAction.Css:
					result = await Css();
					break;
				case RequestAction.Image:
					result = await Image();
					break;
				case RequestAction.Font:
					result = await Font();
					break;
				default:
                    result = null; 
					break;
            }

			return result;
		}

		private async Task<string> Script()
        {
            string[] libraries = new string[]
            {
                "libraries.jquery-3.6.1.min",
                "libraries.jquery-ui.min",
                "libraries.jquery.timepicker.min"
            };
            string[] appScripts = {
                "DbNetSuite",
                "DbNetGridEdit",
                "DbNetGrid",
                "DbNetCombo",
                "DbNetEdit",
                "DbColumn",               
				"GridColumn",
                "EditColumn",
                "Dialog",
				"ViewDialog",
				"SearchDialog",
				"LookupDialog",
                "EditDialog"
			};

			List<string> scripts = new List<string>();

			foreach (string scriptName in libraries)
            {
                scripts.Add(await GetResourceString($"JavaScript.{scriptName}.js"));
			}

            foreach (string scriptName in appScripts)
            {
                scripts.Add(await GetResourceString($"JavaScript.{scriptName}{(Debug ? string.Empty : ".min")}.js"));
            }

            HttpContext.Response.ContentType = $"application/javascript";
			return String.Join(Environment.NewLine, scripts.ToArray());
		}

		private async Task<string> Css()
		{
			string fontSizeVariable = "--main-font-size";
            string fontFamilyVariable = "--main-font-family";

            string[] styleSheetNames = new string[] {
				"libraries.jquery-ui.min",
                "libraries.jquery-ui.theme.min",
                "libraries.jquery.timepicker.min",
                "_reboot",
                "dbnetsuite",
                "dbnetgrid",
                "dbnetcombo",
                "dbnetedit"
            };
			List<string> scripts = new List<string>();

			foreach (string styleSheetName in styleSheetNames)
			{
				string css = await GetResourceString($"CSS.{styleSheetName}.css");
                css = StripBOM(css);

                switch (styleSheetName)
				{
					case "dbnetsuite":
						if (string.IsNullOrEmpty(FontSize) == false)
						{
							css = css.Replace($"{fontSizeVariable}: small", $"{fontSizeVariable}: {FontSize}");
						}
                        if (string.IsNullOrEmpty(FontFamily) == false)
                        {
                            css = css.Replace($"{fontFamilyVariable}: verdana", $"{fontFamilyVariable}: {FontFamily}");
                        }
                        break;
                }

                scripts.Add(css);
			}
			HttpContext.Response.ContentType = $"text/css";
			return String.Join(Environment.NewLine, scripts.ToArray());
		}

		private async Task<byte[]> Image()
		{
			HttpContext.Response.ContentType = $"image/{Name.Split('.').Last()}";
			var image = await GetResource($"Images.{Name}");
			return image;
		}

		private async Task<byte[]> Font()
		{
			HttpContext.Response.ContentType = $"font/woff2";
			return await GetResource($"Fonts.{Name}.woff2");
		}

        public string StripBOM(string resource)
        {
            if (resource.StartsWith(_byteOrderMarkUtf8, StringComparison.Ordinal))
            {
                resource = resource.Remove(0, _byteOrderMarkUtf8.Length);
            }

            return resource;
        }

        private bool IsDebugMode()
        {
            bool isDebugMode = Settings.Debug;

            if (string.IsNullOrEmpty(QueryParam("debug")) == false)
            {
                bool.TryParse(QueryParam("debug"), out isDebugMode);
            }

            return isDebugMode;
        }
    }
}