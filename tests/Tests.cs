// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tests.cs" company="">
//   
// </copyright>
// <summary>
//   The cs unit tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Systemathics.Apis.Tests
{
    #region Usings

    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Google.Protobuf.WellKnownTypes;

    using Grpc.Core;
    using Grpc.Net.Client;
    using Systemathics.Apis.Type.V1;
    using Systemathics.Apis.Services.V1;

    using Auth0.AuthenticationApi;
    using Auth0.AuthenticationApi.Models;

    using Google.Type;

    using Microsoft.Extensions.Configuration;
    using Microsoft.VisualBasic;

    using NUnit.Framework;

    using DateInterval = Systemathics.Apis.Type.V1.DateInterval;
    using DayOfWeek = Google.Type.DayOfWeek;
    using Field = Systemathics.Apis.Type.V1.Field;

    #endregion

    /// <summary>
    /// The cs unit tests.
    /// </summary>
    [TestFixture]
    public class Tests
    {
        /// <summary>
        /// The get app settings.
        /// </summary>
        /// <returns>
        /// The <see cref="IConfiguration"/>.
        /// </returns>
        internal static IConfiguration GetAppSettings()
        {
            // Build configuration from local json
            var directory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var builder = new ConfigurationBuilder().SetBasePath(directory).AddJsonFile("appsettings.json").AddEnvironmentVariables();
            if (File.Exists(Path.Combine(directory, "appsettings.private.json")))
            {
                builder.AddJsonFile("appsettings.private.json");
            }

            return builder.Build();
        }

        /// <summary>
        /// The get access token.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        internal static async Task<string> GetAccessToken()
        {
            var configuration = GetAppSettings();
            var clientId = configuration["CLIENT_ID"] ?? throw new Exception("Missing environment variable CLIENT_ID");
            var clientSecret = configuration["CLIENT_SECRET"] ?? throw new Exception("Missing environment variable CLIENT_SECRET");
            var appAuth0Settings = configuration.GetSection("Auth0");
            var auth0Client = new AuthenticationApiClient(appAuth0Settings["Domain"]);
            var tokenRequest = new ClientCredentialsTokenRequest()
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Audience = appAuth0Settings["Audience"]
            };
            var tokenResponse = await auth0Client.GetTokenAsync(tokenRequest);
            return tokenResponse.AccessToken;

        }

        /// <summary>
        /// The test constraints.
        /// </summary>
        [Test]
        public async Task TestBarsRequest()
        {
            // --> Constraints
            var today = DateTime.Today;
            var dateIntervals = new DateInterval
            {
                StartDate = new Date { Year = 2017, Month = 01, Day = 01 },
                EndDate = new Date { Year = today.Year, Month = today.Month, Day = today.Day }
            };

            var timeInterval = new TimeInterval
            {
                StartTime = new TimeOfDay { Hours = 00, Minutes = 00, Seconds = 00 },
                EndTime = new TimeOfDay { Hours = 06, Minutes = 00, Seconds = 00 }
            };

            var excludedDays = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };

            // Create constraints
            var constraints = new Constraints();
            constraints.DateIntervals.Add(dateIntervals);
            constraints.ExcludedDays.Add(excludedDays);
            constraints.TimeIntervals.Add(timeInterval);

            // Generate bars request
            var memo = new Memo { Provider = "ICE", Group = "FUTURES_599", Stream = "L1", Ticker = @"F:NK225M\J20" };
            var begintime = new TimeOfDay { Hours = 00, Minutes = 00, Seconds = 00 };
            var duration = new Duration { Seconds = 5 * 60 };
            var request = new BarsRequest
            {
                Constraints = constraints,
                BeginTime = begintime,
                Memo = memo,
                Sampling = duration
            };

            Assert.IsNotNull(request);

            // Retrieve access token
            var accessToken = await GetAccessToken();
            Assert.IsNotNull(accessToken);

            // Create channel and grpc client
            var uri = "https://apis.systemathics.cloud";
            var channel = GrpcChannel.ForAddress(uri);
            var client = new BarsService.BarsServiceClient(channel);
            var headers = new Metadata();

            headers.Add("Authorization", $"Bearer {accessToken}");

            var reply = client.Bars(request, headers);

            var bars = reply.Bars;
            Assert.IsNotEmpty(bars);
        }

        /// <summary>
        /// The test constraints.
        /// </summary>
        [Test]
        public async Task TestBollingerBandsRequest()
        {
            // Space (Nikkei 225, April 2020)
            var memo = new Memo { Provider = "ICE", Group = "FUTURES_599", Stream = "L1", Ticker = @"F:NK225M\J20" };

            // Time
            Constraints Constraints()
            {
                var dateIntervals = new DateInterval
                                        {
                                            StartDate = new Date { Year = 2020, Month = 03, Day = 15 },
                                            EndDate = new Date { Year = 2020, Month = 03, Day = 16 }
                                        };

                var timeInterval = new TimeInterval
                                       {
                                           StartTime = new TimeOfDay { Hours = 00, Minutes = 00, Seconds = 00 },
                                           EndTime = new TimeOfDay { Hours = 06, Minutes = 00, Seconds = 00 }
                                       };

                var excludedDays = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };

                // Create constraints
                var constraints = new Constraints();
                constraints.DateIntervals.Add(dateIntervals);
                constraints.ExcludedDays.Add(excludedDays);
                constraints.TimeIntervals.Add(timeInterval);
                return constraints;
            }

            // Bollinger Bands request
            var request = new BollingerBandsRequest
                              {
                                  Memo = memo,
                                  Constraints = Constraints(),
                                  Field = Field.TradePrice,
                                  Length = 100,
                                  Deviations = 0.4
                              };

            // Create channel and gRPC client
            var channel = GrpcChannel.ForAddress("https://apis.systemathics.cloud");
            var headers = new Metadata { { "Authorization", $"Bearer {await GetAccessToken()}" } };
            var client = new BollingerBandsService.BollingerBandsServiceClient(channel);

            var counter = 0;
            await foreach (var reply in client.BollingerBands(request, headers).ResponseStream.ReadAllAsync())
            {
                counter++;
                Assert.Greater(reply.LowerBand, 0);
                Assert.Greater(reply.MiddleBand, 0);
                Assert.Greater(reply.UpperBand, 0);
            }

            TestContext.WriteLine($"Received {counter:N0} {nameof(BollingerBandsResponse)}");
        }

        /// <summary>
        /// The test constraints.
        /// </summary>
        [Test]
        public void TestConstraints()
        {
            // Create date intervals
            var today = DateTime.Today;
            var dateIntervals = new DateInterval
                                    {
                                        StartDate = new Date { Year = 2017, Month = 01, Day = 01 },
                                        EndDate = new Date { Year = today.Year, Month = today.Month, Day = today.Day }
                                    };

            // Create time intervals
            var timeInterval = new TimeInterval
                                   {
                                       StartTime = new TimeOfDay { Hours = 00, Minutes = 00, Seconds = 00 },
                                       EndTime = new TimeOfDay { Hours = 06, Minutes = 00, Seconds = 00 }
                                   };

            // Filter days
            var excludedDays = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };

            // Create constraints
            var constraints = new Constraints();
            constraints.DateIntervals.Add(dateIntervals);
            constraints.ExcludedDays.Add(excludedDays);
            constraints.TimeIntervals.Add(timeInterval);

            // Assert
            Assert.IsNotNull(constraints);
            Assert.IsNotNull(constraints.DateIntervals);
            Assert.IsNotNull(constraints.ExcludedDays);
            Assert.IsNotNull(constraints.TimeIntervals);
        }

        /// <summary>
        /// The test date interval.
        /// </summary>
        [Test]
        public void TestDateInterval()
        {
            // Create start and end date
            var today = DateTime.Today;

            var startDate = new Date { Year = 2017, Month = 01, Day = 01 };
            var endDate = new Date { Year = today.Year, Month = today.Month, Day = today.Day };

            // Generate date intervals
            var dateIntervals = new DateInterval { StartDate = startDate, EndDate = endDate };

            // Assert
            Assert.AreEqual(dateIntervals.StartDate, startDate);
            Assert.AreEqual(dateIntervals.EndDate, endDate);

            Console.WriteLine("StartDate: " + dateIntervals.StartDate);
            Console.WriteLine("EndDate: " + dateIntervals.EndDate);
        }

        /// <summary>
        /// The test google types.
        /// </summary>
        [Test]
        public void TestGoogleTypes()
        {
            // Date
            var today = DateTime.Today;
            var date = new Date { Year = today.Year, Month = today.Month, Day = today.Day };
            Assert.IsNotNull(date);
            Console.WriteLine($"Date: {date.Year}-{date.Month}-{date.Month}");

            // Dayofweek
            var days = new[]
                           {
                               DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday,
                               DayOfWeek.Saturday, DayOfWeek.Saturday, DayOfWeek.Sunday
                           };
            Assert.IsTrue(days.Length == 7);
            foreach (var day in days)
            {
                Console.WriteLine(day);
            }

            // Timeofday
            var time = new TimeOfDay { Hours = 00, Minutes = 00, Seconds = 00 };
            Assert.IsNotNull(time);
            Console.WriteLine($"Time: {time.Hours}:{time.Minutes}:{time.Seconds}");
        }

        /// <summary>
        ///     The test memo.
        /// </summary>
        [Test]
        public void TestMemo()
        {
            // Arrange
            var provider = "ICE";
            var group = "FUTURES_599";
            var stream = "L1";
            var ticker = @"F:NK225M\J20";

            var memo = new Memo { Provider = provider, Group = group, Stream = stream, Ticker = ticker };

            // Assert
            Assert.AreEqual(memo.Provider, provider);
            Assert.AreEqual(memo.Group, group);
            Assert.AreEqual(memo.Stream, stream);
            Assert.AreEqual(memo.Ticker, ticker);

            Console.WriteLine("Displaying memo: ");
            Console.WriteLine("Provider: " + memo.Provider);
            Console.WriteLine("Group: " + memo.Group);
            Console.WriteLine("Stream: " + memo.Stream);
            Console.WriteLine("Ticker: " + memo.Ticker);
        }

        /// <summary>
        /// The test time interval.
        /// </summary>
        [Test]
        public void TestTimeInterval()
        {
            // Create start and end time
            var startTime = new TimeOfDay { Hours = 00, Minutes = 00, Seconds = 00 };
            var endTime = new TimeOfDay { Hours = 06, Minutes = 00, Seconds = 00 };

            var timeInterval = new TimeInterval { StartTime = startTime, EndTime = endTime };

            // Assert
            Assert.AreEqual(timeInterval.StartTime, startTime);
            Assert.AreEqual(timeInterval.EndTime, endTime);

            Console.WriteLine("StartTime: " + timeInterval.StartTime);
            Console.WriteLine("EndTime: " + timeInterval.EndTime);
        }
    }
}