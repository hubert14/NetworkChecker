using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StableNetworkChecker
{
    class Program
    {
        private static string _address;
        private static string _protocol;
        private static Uri _uri;

        static async Task Main(string[] args)
        {
            Console.Write("Enter protocol (for example, http or https, or enter nothing for auto https set: ");
            var protocol = Console.ReadLine();
            _protocol = string.IsNullOrWhiteSpace(protocol) ? "https" : protocol;

            Console.Write("\nEnter URL to check is your connection stable to this address: ");
            _address = Console.ReadLine();
            _uri = new Uri($"{_protocol}://{_address}");

            Console.Write("\nEnter requests count: ");
            int.TryParse(Console.ReadLine(), out var tryCount);
            if (tryCount == 0) return;

            var client = new HttpClient();

            // Need to JIT compile before measurements for getting clear results
            await GetAveragePacketsTimeInterval(1, client);

            Console.Clear();
            var results = new double[tryCount];
            for (var i = 0; i <= tryCount - 1; i++)
            {
                results[i] = await GetAveragePacketsTimeInterval(10, client);
                Console.WriteLine($"Try {i + 1} ping: {results[i]}");
            }

            Console.WriteLine("------------");
            Console.WriteLine($"Average ping: {results.Average()}");
            Console.WriteLine($"Average ping difference: {FindAverageDifference(results)}");
            Console.WriteLine($"Max ping difference: {FindMaxDifference(results)}");
            Console.ReadLine();
        }

        private static double FindMaxDifference(IReadOnlyList<double> results)
        {
            double maxDifference = 0;

            for (var i = 1; i <= results.Count - 1; i++)
            {
                var range = Math.Abs(results[i] - results[i - 1]);
                if (range > maxDifference) maxDifference = range;
            }

            return maxDifference;
        }

        private static double FindAverageDifference(IReadOnlyList<double> results)
        {
            var buffer = new double[results.Count];

            for (var i = 1; i <= results.Count - 1; i++)
            {
                var range = Math.Abs(results[i] - results[i - 1]);
                buffer[i - 1] = range;
            }

            return buffer.Average();
        }

        private static async Task<double> GetAveragePacketsTimeInterval(int tryCount, HttpClient client)
        {
            var timeArray = new double[tryCount];

            for (var i = 0; i <= tryCount - 1; i++)
            {
                var beforeSending = DateTime.Now.TimeOfDay;
                await client.GetAsync(_uri);
                var afterSending = DateTime.Now.TimeOfDay;

                timeArray[i] = (afterSending - beforeSending).TotalMilliseconds;
            }

            return timeArray.Average();
        }
    }
}