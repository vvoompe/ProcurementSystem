using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using ProcurementSystem.Models;
using ProcurementSystem.ViewModels;

namespace ProcurementSystem.Controllers
{
    // [Authorize(Roles = "АДМІНІСТРАТОР")] // Можна розкоментувати для захисту
    public class LinqQueryController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // Статична змінна для зберігання результатів між запитами (для спрощення прикладу)
        private static List<BenchmarkResult> _cachedResults = new List<BenchmarkResult>();

        public ActionResult Index()
        {
            var count = db.Products.Count();
            var model = new BenchmarkViewModel
            {
                TotalRecords = count,
                Results = _cachedResults
            };
            return View(model);
        }

        // Завдання 3: Програмне наповнення бази (100 000 записів)
        [HttpPost]
        public ActionResult GenerateData()
        {
            int currentCount = db.Products.Count();
            int targetCount = 100000;
            int toAdd = targetCount - currentCount;

            if (toAdd <= 0)
            {
                TempData["Message"] = "У базі вже достатньо записів.";
                return RedirectToAction("Index");
            }

            // Отримуємо існуючі зовнішні ключі для прив'язки
            var categoryId = db.Categories.FirstOrDefault()?.Id ?? 1;
            var supplierId = db.Suppliers.FirstOrDefault()?.Id ?? 1;

            // Вимикаємо відстеження змін для швидкості вставки
            db.Configuration.AutoDetectChangesEnabled = false;
            db.Configuration.ValidateOnSaveEnabled = false;

            var random = new Random();

            for (int i = 0; i < toAdd; i++)
            {
                var product = new Product
                {
                    Name = $"AutoProduct_{Guid.NewGuid().ToString().Substring(0, 8)}",
                    Description = "Generated for benchmark",
                    Price = random.Next(10, 5000),
                    Stock = random.Next(1, 1000),
                    CategoryId = categoryId,
                };

                db.Products.Add(product);

                // Зберігаємо пакетами по 1000 штук, щоб не переповнити пам'ять
                if (i % 1000 == 0)
                {
                    db.SaveChanges();
                    // Очищаємо контекст, щоб він не "пухнув"
                    // У EF6 це складніше, тому просто зберігаємо
                }
            }

            db.SaveChanges();

            // Відновлюємо налаштування
            db.Configuration.AutoDetectChangesEnabled = true;
            db.Configuration.ValidateOnSaveEnabled = true;

            TempData["Message"] = $"Додано {toAdd} записів. Тепер у базі {db.Products.Count()}.";
            return RedirectToAction("Index");
        }

        // Очищення результатів
        public ActionResult ClearResults()
        {
            _cachedResults.Clear();
            return RedirectToAction("Index");
        }

        // Завдання 1, 2, 4, 5: Запуск тестів
        [HttpPost]
        public ActionResult RunTests()
        {
            // 1. Завантажуємо дані в пам'ять (Materialization), щоб тестувати C#, а не SQL Server
            var allProducts = db.Products.AsNoTracking().ToList();

            // Якщо записів багато, беремо вибірку для "малого тесту"
            var smallDataSet = allProducts.Take(20).ToList();
            var largeDataSet = allProducts; // Увесь набір (до 100к)

            _cachedResults.Clear();

            // --- Тест 1: Послідовний (Sequential) ---
            RunBenchmark("Послідовний (foreach/LINQ)", smallDataSet, largeDataSet, (data) =>
            {
                var results = new List<double>();
                foreach (var item in data)
                {
                    if (ComplexCondition(item)) // Умова вибірки
                    {
                        results.Add(HeavyCalculation(item)); // Імітація роботи
                    }
                }
            });

            // --- Тест 2: Паралельний (PLINQ) ---
            RunBenchmark("PLINQ (.AsParallel)", smallDataSet, largeDataSet, (data) =>
            {
                var results = data.AsParallel()
                                  .Where(p => ComplexCondition(p))
                                  .Select(p => HeavyCalculation(p))
                                  .ToList();
            });

            // --- Тест 3: PLINQ з налаштуванням ядер (WithDegreeOfParallelism) ---
            int cores = Environment.ProcessorCount / 2; // Використовуємо половину ядер
            if (cores < 1) cores = 1;

            RunBenchmark($"PLINQ ({cores} ядер)", smallDataSet, largeDataSet, (data) =>
            {
                var results = data.AsParallel()
                                  .WithDegreeOfParallelism(cores)
                                  .Where(p => ComplexCondition(p))
                                  .Select(p => HeavyCalculation(p))
                                  .ToList();
            });

            // --- Тест 4: TPL (Parallel.ForEach) ---
            RunBenchmark("TPL (Parallel.ForEach)", smallDataSet, largeDataSet, (data) =>
            {
                var results = new System.Collections.Concurrent.ConcurrentBag<double>();
                Parallel.ForEach(data, (item) =>
                {
                    if (ComplexCondition(item))
                    {
                        results.Add(HeavyCalculation(item));
                    }
                });
            });

            // --- Тест 5: Багатопоточність (Thread) - "Стара школа" ---
            RunBenchmark("Багатопоточність (List<Thread>)", smallDataSet, largeDataSet, (data) =>
            {
                int threadCount = 4;
                var threads = new List<Thread>();
                int batchSize = (int)Math.Ceiling((double)data.Count / threadCount);

                for (int i = 0; i < threadCount; i++)
                {
                    var batch = data.Skip(i * batchSize).Take(batchSize).ToList();
                    var t = new Thread(() =>
                    {
                        foreach (var item in batch)
                        {
                            if (ComplexCondition(item))
                            {
                                HeavyCalculation(item); // Результат не зберігаємо для спрощення коду thread-safety
                            }
                        }
                    });
                    threads.Add(t);
                    t.Start();
                }

                foreach (var t in threads) t.Join(); // Чекаємо завершення всіх потоків
            });

            return RedirectToAction("Index");
        }

        // Допоміжний метод для заміру часу
        private void RunBenchmark(string name, List<Product> smallData, List<Product> largeData, Action<List<Product>> action)
        {
            var result = new BenchmarkResult { MethodName = name };

            // Замір на малих даних
            var sw = Stopwatch.StartNew();
            action(smallData);
            sw.Stop();
            // Переводимо тіки в мілісекунди (більш точно для малих значень)
            result.TimeSmallData = sw.Elapsed.TotalMilliseconds;

            // Замір на великих даних
            sw.Restart();
            action(largeData);
            sw.Stop();
            result.TimeLargeData = sw.Elapsed.TotalMilliseconds;

            _cachedResults.Add(result);
        }

        // Умова вибірки (імітація бізнес-логіки)
        private bool ComplexCondition(Product p)
        {
            // Наприклад: ціна більше 100 і назва не порожня
            return p.Price > 100 && !string.IsNullOrEmpty(p.Name);
        }

        private double HeavyCalculation(Product p)
        {
            double val = (double)p.Price;
            for (int i = 0; i < 2000; i++)
            {
                val = Math.Sqrt(val + i) * Math.Sin(val);
            }
            return val;
        }
    }
}