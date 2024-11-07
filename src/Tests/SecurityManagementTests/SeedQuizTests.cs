using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tuvi.Core;
using Tuvi.Core.Impl.SecurityManagement;

namespace SecurityManagementTests
{
    public class SeedQuizTests
    {
        [Test]
        public void TaskNumbersRange()
        {
            var testSeed = TestData.GetTestSeed();
            ISeedQuiz quiz = SecurityManagerCreator.CreateSeedQuiz(testSeed);
            int[] task = quiz.GenerateTask();

            Assert.GreaterOrEqual(testSeed.Length, task.Length);
            foreach (var number in task)
            {
                Assert.GreaterOrEqual(number, 0);
                Assert.Less(number, testSeed.Length);
            }
        }

        [Test]
        public void TaskRandomizationRange()
        {
            var testSeed = TestData.GetTestSeed();
            ISeedQuiz quiz = SecurityManagerCreator.CreateSeedQuiz(testSeed);

            using var tokenSource = new CancellationTokenSource();
            var cancelationToken = tokenSource.Token;
            tokenSource.CancelAfter(5000);

            Action testDelegate = new Action(
                () =>
                {
                    int[] task;
                    for (int i = 0; i < testSeed.Length; i++)
                    {
                        do
                        {
                            task = quiz.GenerateTask();
                            cancelationToken.ThrowIfCancellationRequested();
                        }
                        while (task.Contains(i) == false);
                    }
                });

                Assert.DoesNotThrow(() => Task.Run(testDelegate, cancelationToken).Wait());
        }

        [Test]
        public void TaskNumbersUnique()
        {
            var testSeed = TestData.GetTestSeed();
            ISeedQuiz quiz = SecurityManagerCreator.CreateSeedQuiz(testSeed);
            int[] task = quiz.GenerateTask();
            
            Assert.AreEqual(task.Length, task.Distinct().Count());
        }

        [Test]
        public void SolutionIsRight()
        {
            var testSeed = TestData.GetTestSeed();
            ISeedQuiz quiz = SecurityManagerCreator.CreateSeedQuiz(testSeed);
            int[] task = quiz.GenerateTask();

            string[] solution = new string[task.Length];
            for (int i = 0; i < solution.Length; i++)
            {
                solution[i] = testSeed[task[i]];
            }

            Assert.IsTrue(quiz.VerifySolution(solution, out bool[] res));
            Assert.IsFalse(res.Any(e => e == false));
        }

        [Test]
        public void SolutionIsNull()
        {
            var testSeed = TestData.GetTestSeed();
            ISeedQuiz quiz = SecurityManagerCreator.CreateSeedQuiz(testSeed);
            var task = quiz.GenerateTask();

            Assert.IsFalse(quiz.VerifySolution(null, out bool[] res));
        }

        [Test]
        public void SolutionIsPartiallyProvided()
        {
            var testSeed = TestData.GetTestSeed();
            ISeedQuiz quiz = SecurityManagerCreator.CreateSeedQuiz(testSeed);
            var task = quiz.GenerateTask();

            var partialSolution = new string[3]
            {
                testSeed[task[0]],
                testSeed[task[1]],
                testSeed[task[2]]
            };
            Assert.IsFalse(quiz.VerifySolution(partialSolution, out bool[] res));
        }

        [Test]
        public void SolutionIsPartiallyTrue()
        {
            var testSeed = TestData.GetTestSeed();
            ISeedQuiz quiz = SecurityManagerCreator.CreateSeedQuiz(testSeed);
            var task = quiz.GenerateTask();

            var partialSolution = new string[6]
            {
                testSeed[task[0]],
                "abra",
                testSeed[task[2]],
                "kadabra",
                "435345",
                testSeed[task[5]]
            };
            Assert.IsFalse(quiz.VerifySolution(partialSolution, out bool[] res));
            Assert.IsTrue(res.SequenceEqual(
                new bool[]
                {
                    true,
                    false,
                    true,
                    false,
                    false,
                    true
                }));
        }

        [Test]
        public void SolutionWordsAreEmpty()
        {
            var testSeed = TestData.GetTestSeed();
            ISeedQuiz quiz = SecurityManagerCreator.CreateSeedQuiz(testSeed);
            var task = quiz.GenerateTask();

            var emptySolutionWords = new string[task.Length];
            Assert.IsFalse(quiz.VerifySolution(emptySolutionWords, out bool[] res));
            Assert.IsFalse(res.Any(e => e == true));
        }

        [Test]
        public void VerifyCalledBeforeGenerate()
        {
            var testSeed = TestData.GetTestSeed();
            ISeedQuiz quiz = SecurityManagerCreator.CreateSeedQuiz(testSeed);
            var testSolution = testSeed.Take(6).ToArray();

            Assert.IsFalse(quiz.VerifySolution(testSolution, out bool[] res));
            Assert.IsNull(res);
        }
    }
}
