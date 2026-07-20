namespace EmailAPIService.Exceptions;

public class TemporaryFailureException : Exception
{
    public TemporaryFailureException(string message)
        : base(message)
    {
    }
}