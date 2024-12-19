namespace WebApp.UI.Exceptions
{
	[Serializable]
	public class InvalidUserOperationException : Exception
	{
		public InvalidUserOperationException() { }
		public InvalidUserOperationException(string message) : base(message) { }
		public InvalidUserOperationException(string message, Exception inner) : base(message, inner) { }
		protected InvalidUserOperationException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
