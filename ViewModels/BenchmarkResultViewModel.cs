using System.Collections.Generic;

namespace ProcurementSystem.ViewModels
{
    public class BenchmarkResult
    {
        public string MethodName { get; set; }
        public double TimeSmallData { get; set; } 
        public double TimeLargeData { get; set; } 
    }

    public class BenchmarkViewModel
    {
        public int TotalRecords { get; set; }
        public List<BenchmarkResult> Results { get; set; }
    }
}