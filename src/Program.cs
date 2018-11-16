﻿//--------------------------------------------------------------------------------- 
// <copyright file="Program.cs" company="Microsoft">
// Microsoft (R)  Azure SDK 
// Software Development Kit 
//  
// Copyright (c) Microsoft Corporation. All rights reserved.   
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,  
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES  
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.  
// </copyright>
//---------------------------------------------------------------------------------

namespace DocumentDB.GetStarted
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net;
    using System.Configuration;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Newtonsoft.Json;

    /// <summary>
    /// This get-started sample demonstrates the creation of resources and execution of simple queries.  
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The Azure DocumentDB endpoint for running this GetStarted sample.
        /// </summary>
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUrl"];

        /// <summary>
        /// The primary key for the Azure DocumentDB account.
        /// </summary>
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

        /// <summary>
        /// The DocumentDB client instance.
        /// </summary>
        private DocumentClient client;

        /// <summary>
        /// The main method for the demo
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static void Main(string[] args)
        {
            try
            {
                Program p = new Program();
                p.GetStartedDemo().Wait();
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey(true);
            }
        }

        /// <summary>
        /// Run the get started demo for Azure Cosmos DB. This creates a database, collection, two documents, executes a simple query 
        /// and cleans up.
        /// </summary>
        /// <returns>The Task for asynchronous completion.</returns>
        private async Task GetStartedDemo()
        {
            // Create a new instance of the DocumentClient
            this.client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);

            Database databaseInfo = new Database { Id = "FamilyDB_og" };
            await this.client.CreateDatabaseIfNotExistsAsync(databaseInfo);

            DocumentCollection collectionInfo = new DocumentCollection();
            collectionInfo.Id = "FamilyCollection_og";

            // We choose LastName as the partition key since we're storing family information. Data is seamlessly scaled out based on the last name of
            // the inserted entity.
            collectionInfo.PartitionKey.Paths.Add("/LastName");

            // Optionally, you can configure the indexing policy of a collection. Here we configure collections for maximum query flexibility 
            // including string range queries. 
            collectionInfo.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

            // Collections can be reserved with throughput specified in request units/second. 1 RU is a normalized request equivalent to the read
            // of a 1KB document.  Here we create a collection with 400 RU/s. 
            await this.client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(databaseInfo.Id),
                collectionInfo,
                new RequestOptions { OfferThroughput = 400 });

            // Insert a document, here we create a Family object
            Family andersenFamily = new Family
            {
                Id = "Andersen.1",
                LastName = "Andersen",
                Parents = new Parent[]
                {
                    new Parent { FirstName = "Thomas" },
                    new Parent { FirstName = "Mary Kay" }
                },
                Children = new Child[]
                {
                    new Child
                    {
                        FirstName = "Henriette Thaulow",
                        Gender = "female",
                        Grade = 5,
                        Pets = new Pet[]
                        {
                            new Pet { GivenName = "Fluffy" }
                        }
                    }
                },
                Address = new Address { State = "WA", County = "King", City = "Seattle" },
                IsRegistered = true
            };

            await this.CreateFamilyDocumentIfNotExists("FamilyDB_og", "FamilyCollection_og", andersenFamily);

            Family wakefieldFamily = new Family
            {
                Id = "Wakefield.7",
                LastName = "Wakefield",
                Parents = new Parent[]
                {
                    new Parent { FamilyName = "Wakefield", FirstName = "Robin" },
                    new Parent { FamilyName = "Miller", FirstName = "Ben" }
                },
                Children = new Child[]
                {
                    new Child
                    {
                        FamilyName = "Merriam",
                        FirstName = "Jesse",
                        Gender = "female",
                        Grade = 8,
                        Pets = new Pet[]
                        {
                            new Pet { GivenName = "Goofy" },
                            new Pet { GivenName = "Shadow" }
                        }
                    },
                    new Child
                    {
                        FamilyName = "Miller",
                        FirstName = "Lisa",
                        Gender = "female",
                        Grade = 1
                    }
                },
                Address = new Address { State = "NY", County = "Manhattan", City = "NY" },
                IsRegistered = false
            };

            await this.CreateFamilyDocumentIfNotExists("FamilyDB_og", "FamilyCollection_og", wakefieldFamily);

            this.ExecuteSimpleQuery("FamilyDB_og", "FamilyCollection_og");

            // Update the Grade of the Andersen Family child
            andersenFamily.Children[0].Grade = 6;

            await this.ReplaceFamilyDocument("FamilyDB_og", "FamilyCollection_og", andersenFamily);

            this.ExecuteSimpleQuery("FamilyDB_og", "FamilyCollection_og");

            // Delete the document
            await this.DeleteFamilyDocument("FamilyDB_og", "FamilyCollection_og", "Andersen", "Andersen.1");

            // Clean up/delete the database and client
            await this.client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri("FamilyDB_og"));
        }

        /// <summary>
        /// Create the Family document in the collection if another by the same partition key/item key doesn't already exist.
        /// </summary>
        /// <param name="databaseName">The name/ID of the database.</param>
        /// <param name="collectionName">The name/ID of the collection.</param>
        /// <param name="family">The family document to be created.</param>
        /// <returns>The Task for asynchronous execution.</returns>
        private async Task CreateFamilyDocumentIfNotExists(string databaseName, string collectionName, Family family)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, family.Id), new RequestOptions { PartitionKey = new PartitionKey(family.LastName) });
                this.WriteToConsoleAndPromptToContinue("Found {0}", family.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), family);
                    this.WriteToConsoleAndPromptToContinue("Created Family {0}", family.Id);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Execute a simple query using LINQ and SQL. Here we filter using the "LastName" property.
        /// </summary>
        /// <param name="databaseName">The name/ID of the database.</param>
        /// <param name="collectionName">The name/ID of the collection.</param>
        private void ExecuteSimpleQuery(string databaseName, string collectionName)
        {
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            // Run a simple query via LINQ. DocumentDB indexes all properties, so queries can be completed efficiently and with low latency.
            // Here we find the Andersen family via its LastName
            IQueryable<Family> familyQuery = this.client.CreateDocumentQuery<Family>(
                UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
                .Where(f => f.LastName == "Andersen");

            // The query is executed synchronously here, but can also be executed asynchronously via the IDocumentQuery<T> interface
            Console.WriteLine("Running LINQ query...");
            foreach (Family family in familyQuery)
            {
                Console.WriteLine("\tRead {0}", family);
            }

            // Now execute the same query via direct SQL
            IQueryable<Family> familyQueryInSql = this.client.CreateDocumentQuery<Family>(
                UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                "SELECT * FROM Family WHERE Family.LastName = 'Andersen'",
                queryOptions);

            Console.WriteLine("Running direct SQL query...");
            foreach (Family family in familyQueryInSql)
            {
                Console.WriteLine("\tRead {0}", family);
            }

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey(true);
        }

        /// <summary>
        /// Replace the Family document in the collection.
        /// </summary>
        /// <param name="databaseName">The name/ID of the database.</param>
        /// <param name="collectionName">The name/ID of the collection.</param>
        /// <param name="updatedFamily">The family document to be replaced.</param>
        /// <returns>The Task for asynchronous execution.</returns>
        private async Task ReplaceFamilyDocument(string databaseName, string collectionName, Family updatedFamily)
        {
            await this.client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, updatedFamily.Id), updatedFamily);
            this.WriteToConsoleAndPromptToContinue("Replaced Family [{0},{1}]", updatedFamily.LastName, updatedFamily.Id);
        }

        /// <summary>
        /// Delete the Family document in the collection.
        /// </summary>
        /// <param name="databaseName">The name/ID of the database.</param>
        /// <param name="collectionName">The name/ID of the collection.</param>
        /// <param name="partitionKey">The partition key of the document.</param>
        /// <param name="documentKey">The document key of the document.</param>
        /// <returns>The Task for asynchronous execution.</returns>
        private async Task DeleteFamilyDocument(string databaseName, string collectionName, string partitionKey, string documentKey)
        {
            await this.client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, documentKey), new RequestOptions { PartitionKey = new PartitionKey(partitionKey) });
            Console.WriteLine("Deleted Family [{0},{1}]", partitionKey, documentKey);
        }

        /// <summary>
        /// Write to the console, and prompt to continue.
        /// </summary>
        /// <param name="format">The string to be displayed.</param>
        /// <param name="args">Optional arguments.</param>
        private void WriteToConsoleAndPromptToContinue(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey(true);
        }

        /// <summary>
        /// A Family class, e.g. storing census data about families within the United States. We use this data model throughout the 
        /// sample to show how you can store objects within your application logic directly as JSON within Azure DocumentDB. 
        /// </summary>
        public class Family
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            public string LastName { get; set; }

            public Parent[] Parents { get; set; }

            public Child[] Children { get; set; }

            public Address Address { get; set; }

            public bool IsRegistered { get; set; }

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        /// <summary>
        /// A parent class used within Family
        /// </summary>
        public class Parent
        {
            public string FamilyName { get; set; }

            public string FirstName { get; set; }
        }

        /// <summary>
        /// A child class used within Family
        /// </summary>
        public class Child
        {
            public string FamilyName { get; set; }

            public string FirstName { get; set; }

            public string Gender { get; set; }

            public int Grade { get; set; }

            public Pet[] Pets { get; set; }
        }

        /// <summary>
        /// A pet class that belongs to a Child
        /// </summary>
        public class Pet
        {
            public string GivenName { get; set; }
        }

        /// <summary>
        /// An address class containing data attached to a Family.
        /// </summary>
        public class Address
        {
            public string State { get; set; }

            public string County { get; set; }

            public string City { get; set; }
        }
    }
}
