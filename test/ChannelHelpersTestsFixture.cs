// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelHelpersTestsFixture.cs" company="Systemathics SAS">
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
    /// The channel helpers tests fixture.
    /// </summary>
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class ChannelHelpersTestsFixture
    {
        #region Public Methods

        /// <summary>
        /// Test <see cref="ChannelHelpers.GetGrpcApiEndpoint"/>.
        /// </summary>
        [Test]
        public void GetGrpcApiEndpointTest()
        {
            try
            {
                Environment.SetEnvironmentVariable("GRPC_APIS", "grpc2.ganymede.cloud");

                Uri uri = null;
                Assert.DoesNotThrow(() => uri = ChannelHelpers.GetGrpcApiEndpoint());
                Assert.IsNotNull(uri);
                Assert.AreEqual("https", uri.Scheme);
                Assert.AreEqual("grpc2.ganymede.cloud", uri.Host);
            }
            finally
            {
                Environment.SetEnvironmentVariable("GRPC_APIS", null);
            }
        }

        /// <summary>
        /// Test <see cref="ChannelHelpers.GetGrpcApiEndpoint"/>.
        /// </summary>
        [Test]
        public void GetGrpcApiEndpointTest2()
        {
            Uri uri = null;
            Assert.DoesNotThrow(() => uri = ChannelHelpers.GetGrpcApiEndpoint());
            Assert.IsNotNull(uri);
            Assert.AreEqual("https", uri.Scheme);
            Assert.AreEqual("grpc.ganymede.cloud", uri.Host);
        }

        #endregion
    }
}