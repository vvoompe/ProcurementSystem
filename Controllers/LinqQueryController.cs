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
    [Authorize(Roles = "АДМІНІСТРАТОР")]
    public class LinqQueryController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

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

            var categoryId = db.Categories.FirstOrDefault()?.Id ?? 1;
            var supplierId = db.Suppliers.FirstOrDefault()?.Id ?? 1;

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

                if (i % 1000 == 0)
                {
                    db.SaveChanges();
                }
            }

            db.SaveChanges();

            db.Configuration.AutoDetectChangesEnabled = true;
            db.Configuration.ValidateOnSaveEnabled = true;

            TempData["Message"] = $"Додано {toAdd} записів. Тепер у базі {db.Products.Count()}.";
            return RedirectToAction("Index");
        }

        public ActionResult ClearResults()
        {
            _cachedResults.Clear();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RunTests()
        {
            var allProducts = db.Products.AsNoTracking().ToList();

            var smallDataSet = allProducts.Take(20).ToList();
            var largeDataSet = allProducts; 

            _cachedResults.Clear();

            RunBenchmark("Послідовний (foreach/LINQ)", smallDataSet, largeDataSet, (data) =>
            {
                var results = new List<double>();
                foreach (var item in data)
                {
                    if (ComplexCondition(item)) 
                    {
                        results.Add(HeavyCalculation(item));
                    }
                }
            });

            RunBenchmark("PLINQ (.AsParallel)", smallDataSet, largeDataSet, (data) =>
            {
                var results = data.AsParallel()
                                  .Where(p => ComplexCondition(p))
                                  .Select(p => HeavyCalculation(p))
                                  .ToList();
            });

            int cores = Environment.ProcessorCount / 2;
            if (cores < 1) cores = 1;

            RunBenchmark($"PLINQ ({cores} ядер)", smallDataSet, largeDataSet, (data) =>
            {
                var results = data.AsParallel()
                                  .WithDegreeOfParallelism(cores)
                                  .Where(p => ComplexCondition(p))
                                  .Select(p => HeavyCalculation(p))
                                  .ToList();
            });

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
                                HeavyCalculation(item); 
                            }
                        }
                    });
                    threads.Add(t);
                    t.Start();
                }

                foreach (var t in threads) t.Join();
            });

            return RedirectToAction("Index");
        }

        private void RunBenchmark(string name, List<Product> smallData, List<Product> largeData, Action<List<Product>> action)
        {
            var result = new BenchmarkResult { MethodName = name };

            var sw = Stopwatch.StartNew();
            action(smallData);
            sw.Stop();
            result.TimeSmallData = sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            action(largeData);
            sw.Stop();
            result.TimeLargeData = sw.Elapsed.TotalMilliseconds;

            _cachedResults.Add(result);
        }

        private bool ComplexCondition(Product p)
        {
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