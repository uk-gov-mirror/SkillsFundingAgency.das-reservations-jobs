using Azure;
using Azure.Core.Serialization;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using SFA.DAS.Reservations.Domain.Configuration;
using SFA.DAS.Reservations.Domain.Documents;
using SFA.DAS.Reservations.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Application.Services;

public class AzureSearchHelper : IAzureSearchHelper
{
    private readonly ILogger<AzureSearchHelper> _logger;
    private readonly SearchIndexClient _adminClient;
    private readonly DefaultAzureCredential _azureKeyCredential;
    private readonly SearchClientOptions _clientOptions;
    private readonly Uri _endpoint;

    public AzureSearchHelper(ReservationsJobs configuration, ILogger<AzureSearchHelper> logger)
    {
        _logger = logger;
        _clientOptions = new SearchClientOptions
        {
            Serializer = new JsonObjectSerializer(new System.Text.Json.JsonSerializerOptions())
        };

        _azureKeyCredential = new DefaultAzureCredential();
        _endpoint = new Uri(configuration.AzureSearchBaseUrl);
        _adminClient = new SearchIndexClient(_endpoint, _azureKeyCredential, _clientOptions);
    }

    public async Task CreateIndex(string indexName)
    {
        try
        {
            var fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(ReservationAzureSearchDocument));

            var definition = new SearchIndex(indexName, searchFields);

            var suggester = new SearchSuggester("sg", new[] { "CourseTitle", "CourseDescription", "AccountLegalEntityName" });
            definition.Suggesters.Add(suggester);

            await _adminClient.CreateOrUpdateIndexAsync(definition);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failure returned when creating index with name {IndexName}", indexName);
            //throw new RequestFailedException($"Failure returned when creating index with name {indexName}", ex); //temp 
        }
    }

    public async Task DeleteIndex(string indexName)
    {
        var index = await GetIndex(indexName);
        
        try
        {
            if (index?.Value != null)
            {
                await _adminClient.DeleteIndexAsync(indexName,cancellationToken: CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failure returned when deleting index with name {IndexName}", indexName);
            //throw new RequestFailedException($"Failure returned when deleting index with name {indexName}", ex); //temp
        }
    }

    public async Task UploadDocuments(string indexName, IEnumerable<ReservationAzureSearchDocument> documents)
    {
        try
        {
            var searchClient = new SearchClient(_endpoint, indexName, _azureKeyCredential, _clientOptions);
            await searchClient.MergeOrUploadDocumentsAsync(documents);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failure returned when uploading documents to index with name {IndexName}", indexName);
            //throw new RequestFailedException("Failure returned when uploading documents to index", ex); //temp
        }
    }

    public async Task<Response<SearchIndex>> GetIndex(string indexName)
    {
        try
        {
            return await _adminClient.GetIndexAsync(indexName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failure returned when requesting index with name {IndexName}", indexName);
            //throw new RequestFailedException($"Failure returned when requesting index with name {indexName}", ex);
            return null; //temp
        }
    }

    public async Task<List<SearchIndex>> GetIndexes()
    {
        try
        {
            var result = new List<SearchIndex>();

            var indexPageable = _adminClient.GetIndexesAsync();

            await foreach (var index in indexPageable)
            {
                result.Add(index);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failure returned when requesting indexes");
            //throw new RequestFailedException("Failure returned when requesting indexes", ex);
            return new List<SearchIndex>(); //temp
        }
    }

    public async Task<SearchAlias> GetAlias(string aliasName)
    {
        try
        {
            return await _adminClient.GetAliasAsync(aliasName);
        }
        catch (RequestFailedException)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failure returned when requesting alias {AliasName}", aliasName);
            //throw new RequestFailedException($"Failure returned when requesting alias {aliasName}", ex);
            return null; //temp
        }
    }

    public async Task<SearchResults<ReservationAzureSearchDocument>> GetDocuments(string indexName, string reservationId)
    {
        try
        {
            var searchClient = new SearchClient(_endpoint, indexName, _azureKeyCredential, _clientOptions);

            var options = new SearchOptions
            {
                Filter = $"ReservationId eq '{reservationId}'",
                IncludeTotalCount = true
            };
            return await searchClient.SearchAsync<ReservationAzureSearchDocument>("*", options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failure returned when requesting document {ReservationId}", reservationId);
            //temporarily commented out to avoid throwing an exception while functionality is being worked on
            //throw new RequestFailedException($"Failure returned when requesting document {reservationId}", ex);
            return null;
        }
    }

    public async Task UpdateAlias(string aliasName, string indexName)
    {
        try
        {
            var myAlias = new SearchAlias(aliasName, indexName);
            await _adminClient.CreateOrUpdateAliasAsync(aliasName, myAlias);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Failure returned when updating alias {aliasName} for index {indexName}");
            //throw new RequestFailedException($"Failure returned when updating alias {aliasName} for index {indexName}", ex); //temp
        }
    }

    public async Task DeleteDocument(string indexName, string reservationId)
    {
        try
        {
            var searchClient = new SearchClient(_endpoint, indexName, _azureKeyCredential, _clientOptions);
            await searchClient.DeleteDocumentsAsync("Id", new[] { reservationId });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failure returned when deleting document with reference {ReservationId}", reservationId);
        }
    }

    public async Task DeleteDocuments(string indexName, IEnumerable<string> ids)
    {
        try
        {
            var searchClient = new SearchClient(_endpoint, indexName, _azureKeyCredential, _clientOptions);
            await searchClient.DeleteDocumentsAsync("Id", ids);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failure returned when deleting documents with references {Ids}", string.Join(", ", ids));
        }
    }

    public async Task<SearchResults<ReservationAzureSearchDocument>> SearchDocuments(string indexName, string filter, string[] selectFields)
    {
        try
        {
            var searchClient = new SearchClient(_endpoint, indexName, _azureKeyCredential, _clientOptions);
            var searchOptions = new SearchOptions();
            searchOptions.Filter = filter;
            foreach (var field in selectFields)
            {
                searchOptions.Select.Add(field);
            }

            return await searchClient.SearchAsync<ReservationAzureSearchDocument>("*", searchOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failure returned when searching documents with filter {Filter}", filter);
            //throw new RequestFailedException($"Failure returned when searching documents with filter {filter}", ex);
            return null; //temp
        }
    }
} 