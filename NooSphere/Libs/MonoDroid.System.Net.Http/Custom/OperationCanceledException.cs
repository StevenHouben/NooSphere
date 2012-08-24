using System;

namespace Microsoft.Http
{
  public class OperationCanceledException : Exception
  {
    public OperationCanceledException()
    {
    }

    public OperationCanceledException(string message) : base(message)
    {
    }

    public OperationCanceledException(string message, Exception inner) : base(message, inner)
    {
    }
  }
}
