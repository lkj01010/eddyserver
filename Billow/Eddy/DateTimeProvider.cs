using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eddy
{
    public class DateTimeProvider
    {
        public static DateTime Now
        {
            get 
            {
                lock (typeof(DateTimeProvider))
                {
                    return DateTime.Now;
                }
            }
        }
    }
}
