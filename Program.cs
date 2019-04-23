using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StringCompression
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new StringCompression();
            var brute = c.BruteCompress();
            var builder = c.BuilderCompress();
            var threaded = c.ThreadedCompress();
            var parallel = c.ParallelCompress();
            
            if (brute != builder || brute != parallel || brute != threaded)
            {
                throw new Exception("compression results don't match!");
            }
            else
            {
                Console.WriteLine("compression results for all algorithms match. continuing to banchmarking..");
            }

            var summary = BenchmarkRunner.Run<StringCompression>();
            Console.WriteLine(summary);
            Console.ReadKey();
        }
    }

    public class StringCompression
    {
        private const int _size = 100;

        private readonly string _input;

        public StringCompression()
        {
            _input = GenerateInput(_size);
        }

        private string GenerateInput(int size)
        {
            var rand = new Random(0);
            var charSet = "abcdefghijklmnopqrstuvwxyz";
            var builder = new StringBuilder(0, size);
            while (builder.Length < size)
            {
                var c = charSet[rand.Next(charSet.Length)];
                var count = rand.Next(10);
                builder.Append(new string(Enumerable.Repeat(c, count).ToArray()));
            }
            return builder.ToString();
        }

        [Benchmark]
        public string BruteCompress()
        {
            return BruteCompress(_input);
        }

        [Benchmark]
        public string BuilderCompress()
        {
            return BuilderCompress(_input);
        }

        [Benchmark]
        public string ThreadedCompress()
        {
            return ThreadedCompress(_input);
        }

        [Benchmark]
        public string ParallelCompress()
        {
            return ParallelCompress(_input);
        }

        public string BruteCompress(string s)
        {
            var compressed = "";
            var consecutive = 1;
            for (var i = 1; i < s.Length; i++)
            {                
                if (s[i] != s[i - 1])
                {
                    compressed += "" + s[i - 1] + consecutive;
                    consecutive = 0;
                }
                consecutive++;
            }
            compressed += "" + s.Last() + consecutive;
            return compressed.Length < s.Length ? compressed : s;
        }

        public string BuilderCompress(string s)
        {
            var builder = new StringBuilder("");
            var consecutive = 1;
            for (var i = 1; i < s.Length; i++)
            {
                if (s[i] != s[i - 1])
                {
                    builder.Append("" + s[i - 1] + consecutive);
                    consecutive = 0;
                }
                consecutive++;
            }

            builder.Append("" + s.Last() + consecutive);

            var compressed = builder.ToString();

            return compressed.Length < s.Length ? compressed : s;
        }

        public string ThreadedCompress(string s)
        {
            var threads = 32;
            while (threads > Math.Log2(s.Length))
            {
                threads /= 2;
            }

            var segmentSize = (int)Math.Ceiling((double)s.Length / threads);

            var segments = new List<string>();
            var prev = 0;
            var ready = false;
            for (var i = 1; i < s.Length; i++)
            {
                if (i % segmentSize == 0)
                {
                    ready = true;
                }
                if (ready && s[i] != s[i - 1])
                {
                    ready = false;
                    segments.Add(s.Substring(prev, i - prev));
                    prev = i;
                }
            }
            segments.Add(s.Substring(prev));

            var results = new ConcurrentDictionary<int, string>();
            var tasks = new Task[threads];
            for (var i = 0; i < threads; i++)
            {
                var localIndex = i;
                tasks[localIndex] = Task.Run(() =>
                {
                    var compressed = BuilderCompress(segments[localIndex]);
                    results.TryAdd(localIndex, compressed);
                });
            }

            Task.WaitAll(tasks);

            var compressed = results.Values.Aggregate("", (s, n) => s + n);
            return compressed.Length < s.Length ? compressed : s;
        }

        public string ParallelCompress(string s)
        {
            var processes = 8;
            while (processes > Math.Log2(s.Length))
            {
                processes /= 2;
            }

            var segmentSize = (int)Math.Ceiling((double)s.Length / processes);

            var segments = new List<string>();
            var prev = 0;
            var ready = false;
            for (var i = 1; i < s.Length; i++)
            {
                if (i % segmentSize == 0)
                {
                    ready = true;
                }
                if (ready && s[i] != s[i - 1])
                {
                    ready = false;
                    segments.Add(s.Substring(prev, i - prev));
                    prev = i;                    
                }
            }
            segments.Add(s.Substring(prev));

            var results = new ConcurrentDictionary<int, string>();
            Parallel.For(0, processes, index =>
            {   
                var compressed = BuilderCompress(segments[index]);
                results.TryAdd(index, compressed);
            });

            var compressed = results.Values.Aggregate("", (s, n) => s + n);
            return compressed.Length < s.Length ? compressed : s;
        }
    }
}
