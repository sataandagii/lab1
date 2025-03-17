namespace _1lab
{
    public class Program
    {
        public static void Main()
        {
            var educationSystem = CreateTestEducationSystem();
            DemonstrateQueries(educationSystem);
        }

        public static OnlineEducationSystem CreateTestEducationSystem()
        {
            // Курси
            var courses = new List<Course>
            {
                new Course
                {
                    Title = "Вступ до програмування",
                    Tests = new List<Test>(),
                    CreatedDate = DateTime.Now.AddMonths(-12)
                },
                new Course
                {
                    Title = "Веб-розробка",
                    Tests = new List<Test>(),
                    CreatedDate = DateTime.Now.AddMonths(-6)
                },
                new Course
                {
                    Title = "Data Science",
                    Tests = new List<Test>(),
                    CreatedDate = DateTime.Now.AddMonths(-10)
                },
                new Course
                {
                    Title = "Штучний інтелект",
                    Tests = new List<Test>(),
                    CreatedDate = DateTime.Now.AddMonths(-14)
                }
            };

            // Додаємо тести до курсів
            foreach (var course in courses)
            {
                var regularTest = new Test
                {
                    Course = course,
                    IsFinalTest = false
                };

                var finalTest = new Test
                {
                    Course = course,
                    IsFinalTest = true
                };

                course.Tests.Add(regularTest);
                course.Tests.Add(finalTest);
            }

            // Базові користувачі
            var users = new List<User>();
            string[] names = { "Іван", "Марія", "Олег", "Наталія" };

            for (int i = 0; i < names.Length; i++)
            {
                var user = new User
                {
                    Name = names[i],
                    Registrations = new List<CourseRegistration>(),
                    Attendances = new List<Attendance>()
                };
                users.Add(user);
            }

            // Генератор випадкових чисел
            var random = new Random(42);

            // Додаємо реєстрації та відвідування для базових користувачів
            for (int i = 0; i < 4; i++)
            {
                var user = users[i];
                var course = courses[i % courses.Count];

                // Реєстрація
                var registration = new CourseRegistration
                {
                    User = user,
                    Course = course,
                    CompletionPercentage = 0.6 + (i * 0.1), // від 60% до 90%
                    TestResults = new List<TestResult>()
                };

                foreach (var test in course.Tests)
                {
                    double score;
                    if (course.Title == "Вступ до програмування")
                        score = 50.0 + (i * 15.0); // від 50 до 95
                    else if (course.Title == "Веб-розробка")
                        score = 70.0 + (i * 10.0); // від 70 до 100
                    else
                        score = 45.0 + (i * 12.0); // від 45 до 81

                    var testResult = new TestResult
                    {
                        Test = test,
                        Score = score,
                        CompletedDate = DateTime.Now.AddDays(-30 + i)
                    };
                    registration.TestResults.Add(testResult);
                }

                user.Registrations.Add(registration);

                // Відвідування
                for (int day = 1; day <= 8; day++)
                {
                    user.Attendances.Add(new Attendance
                    {
                        User = user,
                        Course = course,
                        Date = DateTime.Now.AddDays(-day * 7)
                    });
                }
            }

            // Користувач для запиту 1: високе проходження, низькі оцінки
            var highCompletionLowScoreUser = new User
            {
                Name = "Спецiальний учень (результат 1 запиту)",
                Registrations = new List<CourseRegistration>(),
                Attendances = new List<Attendance>()
            };

            var highCompletionReg = new CourseRegistration
            {
                User = highCompletionLowScoreUser,
                Course = courses[0], // Програмування
                CompletionPercentage = 0.85, // 85% проходження
                TestResults = new List<TestResult>()
            };

            foreach (var test in courses[0].Tests)
            {
                highCompletionReg.TestResults.Add(new TestResult
                {
                    Test = test,
                    Score = 40.0, // Оцінка нижче 50
                    CompletedDate = DateTime.Now.AddDays(-20)
                });
            }

            highCompletionLowScoreUser.Registrations.Add(highCompletionReg);
            users.Add(highCompletionLowScoreUser);

            // Користувач для запиту 3: висока відвідуваність, без тестів
            var highAttendanceNoTestsUser = new User
            {
                Name = "Спецiальний учень (результат 3 запиту)",
                Registrations = new List<CourseRegistration>(),
                Attendances = new List<Attendance>()
            };

            var noTestsReg = new CourseRegistration
            {
                User = highAttendanceNoTestsUser,
                Course = courses[1], // Веб-розробка
                CompletionPercentage = 0.5,
                TestResults = new List<TestResult>() // Пустий список результатів тестів
            };

            highAttendanceNoTestsUser.Registrations.Add(noTestsReg);

            // Висока відвідуваність (9 з 10 днів - 90%)
            for (int day = 1; day <= 9; day++)
            {
                highAttendanceNoTestsUser.Attendances.Add(new Attendance
                {
                    User = highAttendanceNoTestsUser,
                    Course = courses[1],
                    Date = DateTime.Now.AddDays(-day * 7)
                });
            }

            users.Add(highAttendanceNoTestsUser);

            // Додаємо дані для запиту 4 (курси з підвищенням невдач)
            AddTestFailureData(users, courses[3], 10, 3, 6); // ШІ: 30% історичних невдач
            AddTestFailureData(users, courses[3], 10, 6, 0); // ШІ: 60% недавніх невдач (у 2 рази більше)

            AddTestFailureData(users, courses[2], 10, 3, 7); // DS: 30% історичних невдач
            AddTestFailureData(users, courses[2], 10, 7, 0); // DS: 70% недавніх невдач (більше ніж у 2 рази)

            return new OnlineEducationSystem
            {
                Users = users,
                Courses = courses
            };
        }

        private static void AddTestFailureData(List<User> users, Course course, int userCount,
                                              int failCount, int monthsAgo)
        {
            var random = new Random();
            bool isHistorical = monthsAgo >= 6;

            for (int i = 0; i < userCount; i++)
            {
                string period = isHistorical ? "Іст" : "Недавн";
                var user = new User
                {
                    Name = $"{course.Title}_{period}_{i + 1}",
                    Registrations = new List<CourseRegistration>(),
                    Attendances = new List<Attendance>()
                };

                var registration = new CourseRegistration
                {
                    User = user,
                    Course = course,
                    CompletionPercentage = 0.7 + (random.NextDouble() * 0.3),
                    TestResults = new List<TestResult>()
                };

                bool failedTest = i < failCount;

                foreach (var test in course.Tests)
                {
                    if (test.IsFinalTest)
                    {
                        double score = failedTest ? 40.0 : 75.0;
                        int offset = isHistorical ? i % 3 : i % 5;

                        var testResult = new TestResult
                        {
                            Test = test,
                            Score = score,
                            CompletedDate = DateTime.Now.AddMonths(-monthsAgo - offset)
                        };

                        registration.TestResults.Add(testResult);
                    }
                    else
                    {
                        var testResult = new TestResult
                        {
                            Test = test,
                            Score = 65.0 + (random.NextDouble() * 20.0),
                            CompletedDate = DateTime.Now.AddMonths(-monthsAgo - (i % 3))
                        };

                        registration.TestResults.Add(testResult);
                    }
                }

                user.Registrations.Add(registration);
                users.Add(user);
            }
        }

        public static void DemonstrateQueries(OnlineEducationSystem educationSystem)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;


            Console.WriteLine("1. Користувачi, якi пройшли бiльше 70% курсiв, але мають середню оцiнку менше 50%");
            var usersWithHighCompletionLowScore = educationSystem.FindUsersWithHighCompletionLowScore().ToList();

            if (usersWithHighCompletionLowScore.Any())
            {
                foreach (var user in usersWithHighCompletionLowScore)
                {
                    Console.WriteLine($"   - {user.Name}");

                    foreach (var reg in user.Registrations.Where(r => r.CompletionPercentage > 0.7))
                    {
                        double avgScore = reg.TestResults.Any() ? reg.TestResults.Average(tr => tr.Score) : 0;
                        Console.WriteLine($"       Курс: {reg.Course.Title}");
                        Console.WriteLine($"       Процент проходження: {reg.CompletionPercentage:P0}, Середнiй балл: {avgScore:F1}");
                    }
                }
            }
            else
            {
                Console.WriteLine("   Не знайдено користувачів з заданими критеріями");
            }

            Console.WriteLine("\n2. Середнiй вiдсоток правильних вiдповiдей по курсу:");
            var averageTestScores = educationSystem.CalculateAverageTestScoresByCourse();
            foreach (var courseScore in averageTestScores)
            {
                Console.WriteLine($"   - {courseScore.Key.Title}: {courseScore.Value:F2}%");

            }

            Console.WriteLine("\n3. Користувачi, якi вiдвiдують бiльше 80% занять, але не складають тести:");
            var usersWithHighAttendanceLowTesting = educationSystem.FindUsersWithHighAttendanceLowTesting(educationSystem.Courses).ToList();

            if (usersWithHighAttendanceLowTesting.Any())
            {
                foreach (var user in usersWithHighAttendanceLowTesting)
                {
                    Console.WriteLine($"   - {user.Name}");
                }
            }
            else
            {
                Console.WriteLine("   Не знайдено користувачів з заданими критеріями");
            }

            Console.WriteLine("\n4. Курси з підвищенням кількості студентів, які не складають фінальний тест:");
            var coursesWithIncreasedFailureRate = educationSystem.FindCoursesWithIncreasedTestFailureRate().ToList();
            if (coursesWithIncreasedFailureRate.Any())
            {
                foreach (var course in coursesWithIncreasedFailureRate)
                {
                    Console.WriteLine($"   - {course.Title} (створений: {course.CreatedDate.ToShortDateString()})");
                }
            }
            else
            {
                Console.WriteLine("   Не знайдено курсів з заданими критеріями");
            }
        }

    }
}