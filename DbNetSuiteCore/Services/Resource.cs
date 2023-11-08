using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DbNetSuiteCore.Constants.Resource;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;

namespace DbNetSuiteCore.Services
{
    public class Resource : DbNetSuite
	{
        protected string Name => QueryParam("name");
        protected string FontSize => QueryParam("font-size") ?? Settings?.FontSize;
        protected string FontFamily => QueryParam("font-family") ?? Settings?.FontFamily;
        protected bool Debug => IsDebugMode();

        public Resource(AspNetCoreServices services) : base(services)
		{
		}

        public new async Task<object> Process()
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
                "libraries.jquery-ui.min"
            };
            string[] appScripts = {
                "DbNetSuite",
                "DbNetGridEdit",
                "DbNetGrid",
                "DbNetCombo",
                "DbNetEdit",
                "DbNetFile",
                "DbColumn",               
				"GridColumn",
                "EditColumn",
                "Dialog",
				"ViewDialog",
				"SearchDialog",
				"LookupDialog",
                "EditDialog",
                "MessageBox",
                "BrowseDialog",
                "UploadDialog",
                "ImageViewer"
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
                "_reboot",
                "dbnetsuite",
                "dbnetgrid",
                "dbnetcombo",
                "dbnetedit",
                "dbnetfile"
            };
			List<string> scripts = new List<string>();

			foreach (string styleSheetName in styleSheetNames)
			{
				string css = await GetResourceString($"CSS.{styleSheetName}.css");
                css = TextHelper.StripBOM(css);

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