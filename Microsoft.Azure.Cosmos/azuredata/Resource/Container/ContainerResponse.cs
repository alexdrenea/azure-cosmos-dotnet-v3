//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Azure.Data.Cosmos
{
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Documents;

    /// <summary>
    /// The cosmos container response
    /// </summary>
    public class ContainerResponse : Response<ContainerProperties>
    {
        private readonly Response rawResponse;
        private readonly Headers cosmosHeaders;

        /// <summary>
        /// Create a <see cref="ContainerResponse"/> as a no-op for mock testing
        /// </summary>
        protected ContainerResponse()
            : base()
        {
        }

        /// <summary>
        /// A private constructor to ensure the factory is used to create the object.
        /// This will prevent memory leaks when handling the HttpResponseMessage
        /// </summary>
        internal ContainerResponse(
            Response response,
            ContainerProperties containerProperties,
            Container container)
        {
            this.rawResponse = response;
            this.Value = containerProperties;
            this.Container = container;
            ResponseMessage responseMessage = response as ResponseMessage;
            if (responseMessage != null)
            {
                this.cosmosHeaders = responseMessage.CosmosHeaders;
            }
        }

        /// <summary>
        /// The reference to the cosmos container. This allows additional operations on the container
        /// or for easy access to other references like Items, StoredProcedures, etc..
        /// </summary>
        public virtual Container Container { get; private set; }

        /// <inheritdoc/>
        public override ContainerProperties Value { get; }

        /// <inheritdoc/>
        public override Response GetRawResponse() => this.rawResponse;

        /// <summary>
        /// Gets the request charge for this request from the Azure Cosmos DB service.
        /// </summary>
        /// <value>
        /// The request charge measured in request units.
        /// </value>
        public virtual double RequestCharge => this.cosmosHeaders?.RequestCharge ?? 0;

        /// <summary>
        /// Gets the activity ID for the request from the Azure Cosmos DB service.
        /// </summary>
        /// <value>
        /// The activity ID for the request.
        /// </value>
        public virtual string ActivityId => this.cosmosHeaders?.ActivityId;

        /// <summary>
        /// Gets the entity tag associated with the resource from the Azure Cosmos DB service.
        /// </summary>
        /// <value>
        /// The entity tag associated with the resource.
        /// </value>
        /// <remarks>
        /// ETags are used for concurrency checking when updating resources. 
        /// </remarks>
        public virtual string ETag => this.cosmosHeaders?.ETag;

        internal virtual string MaxResourceQuota => this.cosmosHeaders?.GetHeaderValue<string>(HttpConstants.HttpHeaders.MaxResourceQuota);

        internal virtual string CurrentResourceQuotaUsage => this.cosmosHeaders?.GetHeaderValue<string>(HttpConstants.HttpHeaders.CurrentResourceQuotaUsage);

        /// <summary>
        /// Get <see cref="Cosmos.Container"/> implicitly from <see cref="ContainerResponse"/>
        /// </summary>
        /// <param name="response">ContainerResponse</param>
        public static implicit operator Container(ContainerResponse response)
        {
            return response.Container;
        }
    }
}