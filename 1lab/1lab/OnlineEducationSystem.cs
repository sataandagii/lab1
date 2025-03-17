using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1lab
{
    public class OnlineEducationSystem
    {
        public List<User> Users { get; set; }
        public List<Course> Courses { get; set; }

        // Запит 1: Користувачі з високим проходженням курсів (>70%) та низькою середньою оцінкою (<50)
        public IEnumerable<User> FindUsersWithHighCompletionLowScore()
        {
            var result = Users.Where(u =>
                u.Registrations.Count(r => r.CompletionPercentage > 0.7) > 0 &&
                u.Registrations.Average(r => r.TestResults.Any() ?
                    r.TestResults.Average(tr => tr.Score) : 0) < 50
            );

            var result2 = from user in Users
                          where user.Registrations.Count(r => r.CompletionPercentage > 0.7) > 0 &&
                              (from registration in user.Registrations
                               let avgScore = registration.TestResults.Any() ?
                               registration.TestResults.Average(tr => tr.Score) : 0
                               select avgScore).Average() < 50
                          select user;
                          

            return result2;
        }

        // Запит 2: Середній відсоток правильних відповідей для кожного курсу
        public Dictionary<Course, double> CalculateAverageTestScoresByCourse()
        {
            // Варіант 1 з використанням розширюючих методів
            var result1 = Courses.ToDictionary(
                course => course,
                course =>
                {
                    var testResults = Users.SelectMany(u => u.Registrations)//обираємо реєстрації
                        .Where(r => r.Course == course)//такі у яких курс є тим, який ми зараз розглядаємо
                        .SelectMany(r => r.TestResults)//обираємо всі результати тестів з цієї реєстрації
                        .Where(tr => course.Tests.Contains(tr.Test));//фільтруємо результати тестів, де тест є частиною курсу 

                    return testResults.Any() ? testResults.Average(tr => tr.Score) : 0;//обрахунок середнього
                }
            );



            // Варіант 2 без використання розширюючих методів
            var result2 = (from course in Courses
                           let testResults = from user in Users //обираємо користувача
                                             from registration in user.Registrations//у цього користувача дістаємо реєстрації
                                             where registration.Course == course//фільтруємо реєстрації поточного курсу
                                             from testResult in registration.TestResults//дістаємо результати тестів з реєстрації
                                             where course.Tests.Contains(testResult.Test)//фільтруємо тільки результати тестів поточного курсу
                                             select testResult //отримуємо сам результат
                           let averageScore = testResults.Any() ? testResults.Average(tr => tr.Score) : 0 //обрахунок середнього
                           //створюємо список ключ-значення
                           select new
                           {
                               Course = course,
                               AverageScore = averageScore
                           }).ToDictionary(x => x.Course, x => x.AverageScore);

            return result1;
        }

        // Запит 3: Користувачі з високою відвідуваністю (>80%) але без тестів
        public IEnumerable<User> FindUsersWithHighAttendanceLowTesting(List<Course> coursesToCheck)
        {
            // Варіант 1 з використанням розширюючих методів
            var result1 = Users.Where(user =>
                coursesToCheck.Any(course => //перевірка чи існує хочаб один курс
                {
                    //перевірка чи зареєстроваинй користувач на поточний курс
                    var registration = user.Registrations.FirstOrDefault(r => r.Course == course);
                    if (registration == null) return false;

                    //перевірка чи проходив користувач тести
                    if (registration.TestResults.Any()) return false;//якщо проходив, то не підходить


                    int totalDays = 10; //будемо вважати що у всіх курсів 10 днів проходження

                    //рахуємо кількість відвідуваностей цього курсу користувачпем
                    int userAttendances = user.Attendances.Count(a => a.Course == course);

                    //перевіряємо чи висока відвідуваність
                    return (double)userAttendances / totalDays > 0.8;
                })
            );

            // Варіант 2 без використання розширюючих методів
            var result2 = from user in Users
                          where (from course in coursesToCheck
                                 let registration =
                                     (from r in user.Registrations where r.Course == course select r).FirstOrDefault()//знаходимо реєстрації на потрібний курс
                                 where registration != null
                                 let hasTakenTests =
                                     (from tr in registration.TestResults select tr).Any()//шукаємо чи проходив тести
                                 let totalDays = 10
                                 let userAttendances =
                                     (from a in user.Attendances where a.Course == course select a).Count()//присутність учня на поточному курсі
                                 let attendanceRate = (double)userAttendances / totalDays//розрахунок проценту присутності
                                 where attendanceRate > 0.8 && hasTakenTests == false //перевірка умови запиту
                                 select course).Any()
                          select user;

            return result1; // или result2
        }

        // Запит 4: Курси зі збільшенням відсотка невдач у тестах за останні 6 місяців
        public IEnumerable<Course> FindCoursesWithIncreasedTestFailureRate()
        {
            DateTime sixMonthsAgo = DateTime.Now.AddMonths(-6);

            var result = Courses.Where(course =>
            {
                var finalTests = course.Tests.Where(t => t.IsFinalTest).ToList(); // обираємо фінальні тести для курсу
                if (!finalTests.Any()) // якщо фінальних тестів немає - курс не підходить
                    return false;

                var studentsOnCourse = Users.Where(u => u.Registrations.Any(r => r.Course == course)).ToList(); // знаходимо всіх студентів, зареєстрованих на курс
                if (!studentsOnCourse.Any()) // якщо студентів немає - курс не підходить
                    return false;

                // Студенти, які проходили тести за останні 6 місяців
                var recentStudents = studentsOnCourse.Where(s =>
                    s.Registrations.Any(r => r.Course == course &&
                        r.TestResults.Any(tr => finalTests.Contains(tr.Test) && tr.CompletedDate > sixMonthsAgo))).ToList();

                // Студенти, які проходили тести 6-12 місяців тому
                var historicalStudents = studentsOnCourse.Where(s =>
                    s.Registrations.Any(r => r.Course == course &&
                        r.TestResults.Any(tr => finalTests.Contains(tr.Test) &&
                            tr.CompletedDate <= sixMonthsAgo &&
                            tr.CompletedDate > sixMonthsAgo.AddMonths(-6)))).ToList();

                if (recentStudents.Count == 0 || historicalStudents.Count == 0) // якщо немає студентів в одній з груп - курс не підходить
                    return false;

                // Кількість студентів, які провалили фінальні тести за останні 6 місяців
                var recentFailedStudents = recentStudents.Count(s => {
                    var registration = s.Registrations.First(r => r.Course == course); // отримуємо реєстрацію на курс
                    var finalTestResults = registration.TestResults
                        .Where(tr => finalTests.Contains(tr.Test) && tr.CompletedDate > sixMonthsAgo); // фільтруємо фінальні тести останніх 6 місяців
                    return finalTestResults.Any(tr => tr.Score < 50); // перевіряємо, чи є невдачі (менше 50 балів)
                });

                // Кількість студентів, які провалили фінальні тести 6-12 місяців тому
                var historicalFailedStudents = historicalStudents.Count(s => {
                    var registration = s.Registrations.First(r => r.Course == course); // отримуємо реєстрацію на курс
                    var finalTestResults = registration.TestResults
                        .Where(tr => finalTests.Contains(tr.Test) &&
                            tr.CompletedDate <= sixMonthsAgo &&
                            tr.CompletedDate > sixMonthsAgo.AddMonths(-6)); // фільтруємо фінальні тести 6-12 місяців тому
                    return finalTestResults.Any(tr => tr.Score < 50); // перевіряємо, чи є невдачі (менше 50 балів)
                });

                double recentFailureRate = (double)recentFailedStudents / recentStudents.Count; // розрахунок відсотку невдач за останні 6 місяців
                double historicalFailureRate = (double)historicalFailedStudents / historicalStudents.Count; // розрахунок відсотку невдач 6-12 місяців тому

                return historicalFailureRate > 0 && recentFailureRate >= historicalFailureRate * 2; // перевіряємо, чи кількість невдач зросла у 2 рази
            });


            var result2 = Courses.Where(course =>
            {
                var finalTests = course.Tests.Where(t => t.IsFinalTest).ToList(); // обираємо фінальні тести для курсу
                if (!finalTests.Any()) // якщо фінальних тестів немає - курс не підходить
                    return false;

                var studentsOnCourse = Users.Where(u => u.Registrations.Any(r => r.Course == course)).ToList(); // знаходимо всіх студентів, зареєстрованих на курс
                if (!studentsOnCourse.Any()) // якщо студентів немає - курс не підходить
                    return false;

                // Студенти, які проходили тести за останні 6 місяців
                var recentStudents = studentsOnCourse.Where(s =>
                    s.Registrations.Any(r => r.Course == course &&
                        r.TestResults.Any(tr => finalTests.Contains(tr.Test) && tr.CompletedDate > sixMonthsAgo))).ToList();

                // Студенти, які проходили тести 6-12 місяців тому
                var historicalStudents = studentsOnCourse.Where(s =>
                    s.Registrations.Any(r => r.Course == course &&
                        r.TestResults.Any(tr => finalTests.Contains(tr.Test) &&
                            tr.CompletedDate <= sixMonthsAgo &&
                            tr.CompletedDate > sixMonthsAgo.AddMonths(-6)))).ToList();

                if (recentStudents.Count == 0 || historicalStudents.Count == 0) // якщо немає студентів в одній з груп - курс не підходить
                    return false;

                // Кількість студентів, які провалили фінальні тести за останні 6 місяців
                var recentFailedStudents = recentStudents.Count(s => {
                    var registration = s.Registrations.First(r => r.Course == course); // отримуємо реєстрацію на курс
                    var finalTestResults = registration.TestResults
                        .Where(tr => finalTests.Contains(tr.Test) && tr.CompletedDate > sixMonthsAgo); // фільтруємо фінальні тести останніх 6 місяців
                    return finalTestResults.Any(tr => tr.Score < 50); // перевіряємо, чи є невдачі (менше 50 балів)
                });

                // Кількість студентів, які провалили фінальні тести 6-12 місяців тому
                var historicalFailedStudents = historicalStudents.Count(s => {
                    var registration = s.Registrations.First(r => r.Course == course); // отримуємо реєстрацію на курс
                    var finalTestResults = registration.TestResults
                        .Where(tr => finalTests.Contains(tr.Test) &&
                            tr.CompletedDate <= sixMonthsAgo &&
                            tr.CompletedDate > sixMonthsAgo.AddMonths(-6)); // фільтруємо фінальні тести 6-12 місяців тому
                    return finalTestResults.Any(tr => tr.Score < 50); // перевіряємо, чи є невдачі (менше 50 балів)
                });

                double recentFailureRate = (double)recentFailedStudents / recentStudents.Count; // розрахунок відсотку невдач за останні 6 місяців
                double historicalFailureRate = (double)historicalFailedStudents / historicalStudents.Count; // розрахунок відсотку невдач 6-12 місяців тому

                return historicalFailureRate > 0 && recentFailureRate >= historicalFailureRate * 2; // перевіряємо, чи кількість невдач зросла у 2 рази
            });


            return result;
        }
    }
}
