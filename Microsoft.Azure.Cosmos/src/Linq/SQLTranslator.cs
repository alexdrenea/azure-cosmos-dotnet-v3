﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Cosmos.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Microsoft.Azure.Cosmos.Sql;

    /// <summary>
    /// Wrapper class for translating LINQ to DocDB SQL.
    /// </summary>
    internal static class SqlTranslator
    {
        /// <summary>
        /// This function exists for testing only.
        /// </summary>
        /// <param name="inputExpression">Expression to translate.</param>
        /// <param name="serializationOptions">Optional serializer options.</param>
        /// <returns>A string describing the expression translation.</returns>
        internal static string TranslateExpression(
            Expression inputExpression,
            CosmosSerializationOptions serializationOptions = null)
        {
            TranslationContext context = new TranslationContext(serializationOptions);

            inputExpression = ConstantEvaluator.PartialEval(inputExpression);
            SqlScalarExpression scalarExpression = ExpressionToSql.VisitNonSubqueryScalarExpression(inputExpression, context);
            return scalarExpression.ToString();
        }

        internal static string TranslateExpressionOld(
            Expression inputExpression,
            CosmosSerializationOptions serializationOptions = null)
        {
            TranslationContext context = new TranslationContext(serializationOptions);

            inputExpression = ConstantFolding.Fold(inputExpression);
            SqlScalarExpression scalarExpression = ExpressionToSql.VisitNonSubqueryScalarExpression(inputExpression, context);
            return scalarExpression.ToString();
        }

        internal static SqlQuerySpec TranslateQuery(
            Expression inputExpression,
            CosmosSerializationOptions serializationOptions = null)
        {
            inputExpression = ConstantEvaluator.PartialEval(inputExpression);
            TranslationContext translationContext = null;
            SqlQuery query = ExpressionToSql.TranslateQuery(inputExpression, out translationContext);
            string queryText = query.ToParameterizedString(translationContext.literalToParamStr);
            SqlParameterCollection sqlParameters = new SqlParameterCollection();
            foreach (KeyValuePair<object, string> kvp in translationContext.literalToParamStr)
            {
                sqlParameters.Add(new SqlParameter(kvp.Value, kvp.Key));
            }
            SqlQuerySpec sqlQuerySpec = new SqlQuerySpec(queryText, sqlParameters);
            return sqlQuerySpec;
        }
    }
}
