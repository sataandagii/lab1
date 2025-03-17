using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1lab
{
    public class CourseRegistration
    {
        public User User { get; set; }
        public Course Course { get; set; }
        public List<TestResult> TestResults { get; set; }
        public double CompletionPercentage { get; set; }
    }
}
