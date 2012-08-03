namespace SchoStack.Web
{
    public interface ICommandHandler<TInput>
    {
        void Handle(TInput command);
    }
}