﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Cosmos.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.CosmosElements;
    using Microsoft.Azure.Cosmos.Json;
    using Microsoft.Azure.Cosmos.Query;
    using Microsoft.Azure.Documents;
    using Moq;

    internal static class QueryResponseMessageFactory
    {
        private static readonly CosmosSerializer cosmosSerializer = new CosmosJsonDotNetSerializer();
        public const int SPLIT = -1;

        public static (QueryResponseCore queryResponse, IList<ToDoItem> items) Create(
            string itemIdPrefix,
            string continuationToken,
            string collectionRid,
            int itemCount = 50)
        {
            // Use -1 to represent a split response
            if (itemCount == QueryResponseMessageFactory.SPLIT)
            {
                return (CreateSplitResponse(collectionRid), new List<ToDoItem>().AsReadOnly());
            }

            IList<ToDoItem> items = ToDoItem.CreateItems(itemCount, itemIdPrefix);
            MemoryStream memoryStream = (MemoryStream)cosmosSerializer.ToStream<IList<ToDoItem>>(items);
            long responseLengthBytes = memoryStream.Length;

            IJsonNavigator jsonNavigator = JsonNavigator.Create(memoryStream.ToArray());
            IJsonNavigatorNode jsonNavigatorNode = jsonNavigator.GetRootNode();
            CosmosArray cosmosArray = CosmosArray.Create(jsonNavigator, jsonNavigatorNode);

            QueryResponseCore message = QueryResponseCore.CreateSuccess(
                    result: cosmosArray,
                    continuationToken: continuationToken,
                    disallowContinuationTokenMessage: null,
                    activityId: Guid.NewGuid().ToString(),
                    requestCharge: 42,
                    queryMetricsText: null,
                    queryMetrics: new Dictionary<string, QueryMetrics>(),
                    requestStatistics: null,
                    responseLengthBytes: responseLengthBytes);

            return (message, items);
        }

        public static QueryResponseCore CreateQueryResponse(
            IList<ToDoItem> items,
            bool isOrderByQuery,
            string continuationToken,
            string collectionRid)
        {
            MemoryStream memoryStream;
            string json;
            if (isOrderByQuery)
            {
                memoryStream = SerializeForOrderByQuery(items);
                using(StreamReader sr = new StreamReader(SerializeForOrderByQuery(items)))
                {
                    json = sr.ReadToEnd();
                }
            }
            else
            {
                memoryStream = (MemoryStream)cosmosSerializer.ToStream<IList<ToDoItem>>(items);
            }

            long responseLengthBytes = memoryStream.Length;

            IJsonNavigator jsonNavigator = JsonNavigator.Create(memoryStream.ToArray());
            IJsonNavigatorNode jsonNavigatorNode = jsonNavigator.GetRootNode();
            CosmosArray cosmosArray = CosmosArray.Create(jsonNavigator, jsonNavigatorNode);

            QueryResponseCore message = QueryResponseCore.CreateSuccess(
                    result: cosmosArray,
                    requestCharge: 4,
                    activityId: Guid.NewGuid().ToString(),
                    queryMetricsText: null,
                    queryMetrics: null,
                    requestStatistics: null,
                    responseLengthBytes: responseLengthBytes,
                    disallowContinuationTokenMessage: null,
                    continuationToken: continuationToken);

            return message;
        }

        public static QueryResponseCore CreateSplitResponse(string collectionRid)
        {
            QueryResponseCore splitResponse = QueryResponseCore.CreateFailure(
               statusCode: HttpStatusCode.Gone,
               subStatusCodes: SubStatusCodes.PartitionKeyRangeGone,
               errorMessage: "Partition split error",
               requestCharge: 10.4,
               activityId: Guid.NewGuid().ToString(),
               queryMetricsText: null,
               queryMetrics: null);

            return splitResponse;
        }

        private static MemoryStream SerializeForOrderByQuery(IList<ToDoItem> items)
        {
            OrderByReturnStructure[] payload = items.Select(item => new OrderByReturnStructure()
            {
                _rid = item._rid,
                payload = item,
                orderByItems = new OrderbyItems[] { new OrderbyItems(item.id) }
            }).ToArray();

            return (MemoryStream)cosmosSerializer.ToStream<OrderByReturnStructure[]>(payload);
        }

        private class OrderByReturnStructure
        {
            public string _rid { get; set; }
            public ToDoItem payload { get; set; }
            public OrderbyItems[] orderByItems { get; set; }
        }

        private class OrderbyItems
        {
            public OrderbyItems(string item)
            {
                this.item = item;
            }

            public string item { get; set; }
        }
    }
}
