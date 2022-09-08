// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TokenHelpersTestsFixture.cs" company="Systemathics SAS">
//   Copyright (c) Systemathics (rd@systemathics.com)
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Systemathics.Apis.Tests
{
    #region Usings

    using System;

    using Systemathics.Apis.Helpers;

    using NUnit.Framework;

    #endregion

    /// <summary>
    /// The token helpers tests fixture.
    /// </summary>
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class TokenHelpersTestsFixture
    {
        /// <summary>
        /// Test <see cref="TokenHelpers.GetToken"/>.
        /// </summary>
        [Test]
        public void GetTokenTest()
        {
            try
            {
                Environment.SetEnvironmentVariable("CLIENT_ID", "<here>");
                Environment.SetEnvironmentVariable("CLIENT_SECRET", "<here>");

                string token = null;
                Assert.DoesNotThrow(() => token = TokenHelpers.GetToken());
                StringAssert.StartsWith("Bearer", token);
            }
            finally
            {
                Environment.SetEnvironmentVariable("CLIENT_ID", null);
                Environment.SetEnvironmentVariable("CLIENT_SECRET", null);
            }
        }
    }
}