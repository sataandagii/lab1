using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1lab
{
    public class User
    {
        public string Name { get; set; }
        public List<CourseRegistration> Registrations { get; set; }
        public List<Attendance> Attendances { get; set; }
    }
}
