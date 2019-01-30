namespace Common
{
    public interface IUserAgeValidator
    {
        bool IsValid(User user);
    }

    public class UserAgeValidator : IUserAgeValidator
    {
        public bool IsValid(User user)
        {
            return user.Age >= 18;
        }
    }
}