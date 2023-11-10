// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelHelpers.cs" company="Systemathics SAS">
//   Copyright (c) Systemathics (rd@systemathics.com)
// </copyright>
// <summary>
//   Helps to create channels to access Systemathics Ganymede authenticated API.
// </summary>
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
        private const string DefaultEndpoint = "https://grpc.ganymede.cloud";

        #endregion

        #region Public Methods

        /// <summary>
        /// Get a channel suitable to call Ganymede gRPC API.
        /// This uses the GRPC_APIS environment variable in the form http[s]://fdqn[:port] (if no scheme is give, we'll assume https).
        /// If none is detected, use <see cref="DefaultEndpoint" />.
        /// </summary>
        /// <returns>
        /// A channel suitable to call Ganymede gRPC API.
        /// </returns>
        public static GrpcChannel GetChannel() 
        {
            var endpoint = Environment.GetEnvironmentVariable("GRPC_APIS");
            endpoint = string.IsNullOrEmpty(endpoint) ? DefaultEndpoint : endpoint;
            if (!endpoint.StartsWith("http"))
            {
                // Assume https if no scheme was given
                endpoint = $"https://{endpoint}";
            }

            return GrpcChannel.ForAddress(new Uri(endpoint));
        }

        #endregion
    }
}