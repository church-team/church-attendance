using System.ComponentModel.DataAnnotations;

namespace churchAttendace.Utilities
{
    public class PastDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime dateTime)
            {
                return dateTime < DateTime.Now;
            }
            return false;
        }
    }

    public class NotFutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime dateTime)
            {
                return dateTime <= DateTime.Now.Date;
            }
            return true;
        }
    }
}
