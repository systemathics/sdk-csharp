// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelHelpers.cs" company="Systemathics SAS">
//   Copyright (c) Systemathics (rd@systemathics.com)
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Systemathics.Apis.Helpers
{
    #region Usings

    using System;

    using Grpc.Net.Client;

    #endregion

    /// <summary>
    /// Helps with channels.
    /// </summary>
    public static class ChannelHelpers
    {
        #region Constants

        /// <summary>
        /// The default endpoint.
        /// </summary>
        private const string DefaultEndpoint = "grpc.ganymede.cloud";

        #endregion

        #region Public Methods

        /// <summary>
        /// Get a channel suitable to call Ganymede gRPC APIs.
        /// We use <see cref="GetGrpcApiEndpoint"/> to discover the gRPC APIs endpoint.
        /// </summary>
        /// <returns>
        /// A channel suitable to call Ganymede gRPC APIs.
        /// </returns>
        public static GrpcChannel GetChannel() => GrpcChannel.ForAddress(GetGrpcApiEndpoint());

        /// <summary>
        /// Get the gRPC APIs endpoint.
        /// This uses the GRPC_APIS environment variable in the form fdqn:port.
        /// </summary>
        /// <returns>
        /// The gRPC APIs endpoint (secure, e.g: https).
        /// </returns>
        public static Uri GetGrpcApiEndpoint()
        {
            var grpcApis = Environment.GetEnvironmentVariable("GRPC_APIS");
            grpcApis = grpcApis ?? DefaultEndpoint;
            return new Uri($"https://{grpcApis}");
        }

        /// <summary>
        /// Get the gRPC APIs endpoint.
        /// This uses the GRPC_APIS environment variable in the form fdqn:port.
        /// </summary>
        /// <returns>
        /// The gRPC APIs endpoint (insecure, e.g: http).
        /// </returns>
        public static Uri GetInsecureGrpcApiEndpoint()
        {
            var grpcApis = Environment.GetEnvironmentVariable("GRPC_APIS");
            grpcApis = grpcApis ?? DefaultEndpoint;
            return new Uri($"http://{grpcApis}");
        }

        #endregion
    }
}