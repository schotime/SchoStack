namespace SchoStack.Web
{
    public interface IHandler<TInput, TOutput>
    {
        TOutput Handle(TInput input);
    }

    public interface IHandler<TOutput>
    {
        TOutput Handle();
    }
}