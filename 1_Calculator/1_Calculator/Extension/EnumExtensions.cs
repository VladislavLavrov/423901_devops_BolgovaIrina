using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
namespace _1_Calculator.Extension
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum val)
        {
            return val.GetType()
            .GetMember(val.ToString())
            .FirstOrDefault()
            ?.GetCustomAttribute < DisplayAttribute > (false)
             ?.Name
             ?? val.ToString();
        }
    }
}