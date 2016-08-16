﻿using System;
using System.Collections.Generic;
using System.IO;
using NSwag;
using Xunit;
using System.Linq;
using jab.Interfaces;
using Xunit.Sdk;

namespace jab.tests
{
    public partial class ApiBestPracticeTestBase
    {
        // Standard web service formats
        private static readonly List<string> StandardFormats = new List<string>
        {
            "application/x-www-form-urlencoded",
            "application/xml",
            "application/json"
        };

        /// <summary>
        /// Operations using the "DELETE" verb should not accept form encoded data.
        /// </summary>
        /// <param name="operation"></param>
        [Theory, ApiOperationsData(testDefinition)]
        public void DeleteMethodsShouldNotTakeFormEncodedData(
            IJabApiOperation operation)
        {
            if (operation.Method == SwaggerOperationMethod.Delete)
            {
                Assert.Null(operation.Operation.ActualConsumes);
            }
            else
            {
                Assert.True(true);
            }
        }

        /// <summary>
        /// Use the "DELETE" verb for delete or removal operations.
        /// </summary>
        /// <param name="operation"></param>
        [Theory, ApiOperationsData(testDefinition)]
        public void UseDeleteVerbForDelete(
            IJabApiOperation operation)
        {
            List<string> deleteSynonyms = new List<string>
            {
                "delete",
                "remove"
            };

            Assert.False(operation.Method != SwaggerOperationMethod.Delete
                && deleteSynonyms.Any(term => operation.Path.IndexOf(term, 0, StringComparison.InvariantCultureIgnoreCase) != -1),
                $"{operation.Path} should use 'DELETE' verb instead of '{operation.Method}'");
        }

        /// <summary>
        /// Do not include secrets in query parameters. These get logged or included in browser history.
        /// <para></para>
        /// Similar to https://www.owasp.org/index.php/REST_Security_Cheat_Sheet#Authentication_and_session_management.
        /// </summary>
        /// <param name="operation"></param>
        [Theory, ApiOperationsData(testDefinition)]
        public void NoSecretsInQueryParameters(IJabApiOperation operation)
        {
            List<string> secretSynonyms = new List<string>
            {
                "password",
                "secret",
                "key"
            };

            IList<SwaggerParameter> queryParametersContainingSecrets =
                new List<SwaggerParameter>(operation.Operation.ActualParameters.Where(
                    parameter => parameter.Kind == SwaggerParameterKind.Query
                                 && secretSynonyms.Any(term => parameter.Name.IndexOf(term, 0, StringComparison.InvariantCultureIgnoreCase) != -1)));

            // TODO: Move this to a separate method.
            if (queryParametersContainingSecrets.Count > 0)
            {
                throw new XunitException(
                    $"{string.Concat(operation.Service.BaseUrl, operation.Path)} passes secrets in the following query parameters '{(string.Join(", ", queryParametersContainingSecrets.Select(p => p.Name)))}'");
            }
        }

        /// <summary>
        /// Do not include secrets in query parameters. These get logged or included in browser history.
        /// </summary>
        /// <param name="operation"></param>
        [Theory, ParameterisedClassData(typeof(ApiOperations), testDefinition)]
        public void NoNonStandardProductFormats(IJabApiOperation operation)
        {
            IList<string> nonStandardFormats =
                operation.Operation.ActualProduces.Where(product => !StandardFormats.Contains(product)).ToList();

            // TODO: Move this to a separate method.
            if (nonStandardFormats.Count > 0)
            {
                throw new XunitException(
                    $"{string.Concat(operation.Service.BaseUrl, operation.Path)} produces the nonstandard formats '{(string.Join(", ", nonStandardFormats))}'");
            }
        }

        /// <summary>
        /// Do not include secrets in query parameters. These get logged or included in browser history.
        /// </summary>
        /// <param name="operation"></param>
        [Theory, ParameterisedClassData(typeof(ApiOperations), testDefinition)]
        public void NoNonStandardConsumptionFormats(IJabApiOperation operation)
        {
            IList<string> nonStandardFormats =
                operation.Operation.ActualConsumes.Where(product => !StandardFormats.Contains(product)).ToList();

            // TODO: Move this to a separate method.
            if (nonStandardFormats.Count > 0)
            {
                throw new XunitException(
                    $"{string.Concat(operation.Service.BaseUrl, operation.Path)} consumes the nonstandard formats '{(string.Join(", ", nonStandardFormats))}'");
            }
        }
    }
}
