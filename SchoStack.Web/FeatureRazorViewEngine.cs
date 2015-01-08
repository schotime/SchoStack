using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SchoStack.Web
{
    public class FeatureCsRazorViewEngine : RazorViewEngine
    {
        public FeatureCsRazorViewEngine(string baseFolder)
        {
            var location = "~/%base%/{1}/{0}.cshtml".Replace("%base%", baseFolder);
            var shared = "~/%base%/shared/{0}.cshtml".Replace("%base%", baseFolder);
            var featureLocation = new List<string> { location,shared };
            featureLocation.AddRange(ViewLocationFormats);
            ViewLocationFormats = featureLocation.Where(x => x.EndsWith(".cshtml")).ToArray();

            var partial = "~/%base%/{1}/{0}.cshtml".Replace("%base%", baseFolder);
            var partialFeatureLocation = new List<string> {partial};
            partialFeatureLocation.AddRange(PartialViewLocationFormats);
            var newlist = new List<string>();
            foreach (var item in partialFeatureLocation.Where(x => x.EndsWith(".cshtml")))
            {
                newlist.Add(item);
                newlist.Add(item.Replace("{0}", "_{0}"));
            }

            PartialViewLocationFormats = newlist.ToArray();
        }
    }
}
