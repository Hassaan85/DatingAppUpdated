using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Extentions
{
    public static class DateTimeExtentions
    {
        public static int CalculateAge (this DateOnly dob)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var age = today.Year - dob.Year;

            if (dob > today.AddYears(-age)) age--;

            return age;
        }
        
    }
}