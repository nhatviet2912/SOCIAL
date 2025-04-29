namespace Application.Common.Exception;
using System;

public class CustomException : Exception
{
    public int ErrorCode { get; set; }

    public CustomException(int errorCode, string message, Exception? innerException = null) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}