using WebApp.UI.Exceptions;

namespace WebApp.UI.Services
{
    public class UserValidationService
    {
        public void ValidateUserEmail(string email)
        {
            if (!email.Contains("@"))
                throw new InvalidUserOperationException("User email is invalid.");
        }
    }
}
