using System.Web.Mvc;

namespace SchoStack.Web
{
    public interface IControllerContext
    {
        bool IsValidModel { get; }
        ControllerContext Context { get; }
    }

    public class SchoStackControllerContext : IControllerContext
    {
        private readonly ControllerContext _context;

        public SchoStackControllerContext(ControllerContext context)
        {
            _context = context;
        }

        public ControllerContext Context { get { return _context; } }

        public bool IsValidModel
        {
            get
            {
                return _context.Controller.ViewData.ModelState.IsValid;
            }
        }
    }
}