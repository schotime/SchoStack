namespace SchoStack.Web.HtmlTags
{
    public class HtmlConvention
    {
        public HtmlConvention()
        {
            Inputs = new TagConventions();
            Labels = new TagConventions();
            Displays = new TagConventions();
            All = new TagConventions(true);
        }

        public ITagConventions All { get; private set; }
        public ITagConventions Inputs { get; private set; }
        public ITagConventions Labels { get; private set; }
        public ITagConventions Displays { get; private set; }
    }
}