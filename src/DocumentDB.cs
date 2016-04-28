using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using DocumentDB.DbSchema;

namespace DocumentDB.GetStarted
{
    class DocumentDB
    {
        /// <summary>The DocumentDB client instance.</summary>
        private DocumentClient client;
        /// <summary>The name of the database.</summary>
        private string dbName;
        /// <summary>The name of the collection.</summary>
        private string collName;
        /// <summary>Set some common query options.</summary>
        private FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

        /// <summary>
        /// Constructor for the DocumentDB class
        /// </summary>
        /// <param name="endpointUri">The Azure DocumentDB endpoint</param>
        /// <param name="primaryKey">The primary key for the Azure DocumentDB</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="collectionName">The name of the collection.</param>
        public DocumentDB(string endpointUri, string primaryKey, string databaseName, string collectionName)
        {
            dbName = databaseName;
            collName = collectionName;
            // Create a new instance of the DocumentClient
            client = new DocumentClient(new Uri(endpointUri), primaryKey);            
            CreateDatabaseIfNotExists().Wait();
            CreateDocumentCollectionIfNotExists().Wait();
        }

        /// <summary>
        /// Clean up/delete the database and client
        /// </summary>
        /// <returns>The Task for asynchronous execution</returns>
        public async Task DeleteDatabaseAsync()
        {
            await this.client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(dbName));
        }


        /// <summary>
        /// Create a database with the specified name if it doesn't exist. 
        /// </summary>
        /// <returns>Returns a success message.</returns>
        private async Task<string> CreateDatabaseIfNotExists()
        {
            // Check to verify a database with the id=FamilyDB does not exist
            try
            {
                await this.client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(dbName));
                return string.Format("Found {0}", dbName);
            }
            catch (DocumentClientException de)
            {
                // If the database does not exist, create a new database
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDatabaseAsync(new Database { Id = dbName });
                    return string.Format("Created {0}", dbName);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Create a collection with the specified name if it doesn't exist.
        /// </summary>
        /// <returns>Returns a success message.</returns>
        private async Task<string> CreateDocumentCollectionIfNotExists()
        {
            try
            {
                await this.client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(dbName, collName));
                return string.Format("Found {0}", collName);
            }
            catch (DocumentClientException de)
            {
                // If the document collection does not exist, create a new collection
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    DocumentCollection collectionInfo = new DocumentCollection();
                    collectionInfo.Id = collName;

                    // Optionally, you can configure the indexing policy of a collection. Here we configure collections for maximum query flexibility 
                    // including string range queries. 
                    collectionInfo.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

                    // DocumentDB collections can be reserved with throughput specified in request units/second. 1 RU is a normalized request equivalent to the read
                    // of a 1KB document.  Here we create a collection with 400 RU/s. 
                    await this.client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(dbName),
                        new DocumentCollection { Id = collName },
                        new RequestOptions { OfferThroughput = 400 });

                    return string.Format("Created {0}", collName);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Create the Family document in the collection if another by the same ID doesn't already exist.
        /// </summary>
        /// <param name="family">The family document to be created.</param>
        /// <returns>Returns a success message.</returns>
        public async Task<string> CreateFamilyDocumentIfNotExists(Family family)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(dbName, collName, family.Id));
                return string.Format("Found {0}", family.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(dbName, collName), family);
                    return string.Format("Created Family {0}", family.Id);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Execute a simple query using LINQ.
        /// </summary>
        /// <param name="LastName">The last name to filter by.</param>
        /// <returns>the family that matches the give last name.</returns>
        public IQueryable<Family> GetFamilyLinq(string LastName)
        {            
            return this.client.CreateDocumentQuery<Family>(
                UriFactory.CreateDocumentCollectionUri(dbName, collName), queryOptions)
                .Where(f => f.LastName == LastName);            
        }

        /// <summary>
        /// Execute a simple query using SQL.
        /// </summary>
        /// <param name="LastName">The last name to filter by.</param>
        /// <returns>the family that matches the give last name.</returns>
        public IQueryable<Family> GetFamilySql(string LastName)
        {
            return this.client.CreateDocumentQuery<Family>(
                UriFactory.CreateDocumentCollectionUri(dbName, collName),
                "SELECT * FROM Family WHERE Family.lastName = '" + LastName + "'",
                queryOptions);            
        }

        /// <summary>
        /// Replace the Family document in the collection.
        /// </summary>
        /// <param name="familyId">The ID of the document</param>
        /// <param name="updatedFamily">The family document to be replaced.</param>
        /// <returns>Returns a success message.</returns>
        public async Task<string> ReplaceFamilyDocument(string familyId, Family updatedFamily)
        {
            try
            {
                await this.client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(dbName, collName, familyId), updatedFamily);
                return string.Format("Replaced Family {0}", familyId);
            }
            catch (DocumentClientException de)
            {
                throw de;
            }
        }

        /// <summary>
        /// Delete the Family document in the collection.
        /// </summary>
        /// <param name="familyId">The ID of the document.</param>
        /// <returns>Returns a success message.</returns>
        public async Task<string> DeleteFamilyDocument(string familyId)
        {
            try
            {
                await this.client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(dbName, collName, familyId));
                return string.Format("Deleted Family {0}", familyId);
            }
            catch (DocumentClientException de)
            {
                throw de;
            }
        }
    }
}
