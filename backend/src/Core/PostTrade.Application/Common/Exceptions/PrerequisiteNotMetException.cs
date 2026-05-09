namespace PostTrade.Application.Common.Exceptions;

public class PrerequisiteNotMetException : Exception
{
    public PrerequisiteNotMetException(string message) : base(message) { }
}
