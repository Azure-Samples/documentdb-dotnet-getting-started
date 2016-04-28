//--------------------------------------------------------------------------------- 
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
using System;
using System.Configuration;
using Microsoft.Azure.Documents;

namespace DocumentDB.GetStarted
{
    /// <summary>
    /// This get-started sample demonstrates the creation of resources and execution of simple queries.  
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main method for the demo
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static void Main(string[] args)
        {
            try
            {
                GetStartedDemo();
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("\nEnd of demo, press any key to exit.");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Run the get started demo for Azure DocumentDB. 
        /// This creates a database, collection, two documents, 
        /// executes a simple query and cleans up.
        /// </summary>
        static void GetStartedDemo()
        {
            Console.WriteLine("Starting demo...");
            string endpointUri = ConfigurationManager.AppSettings["EndPointUri"];
            string primaryKey = ConfigurationManager.AppSettings["PrimaryKey"];
            var docDb = new DocumentDB(endpointUri, primaryKey, "FamilyDB", "FamilyCollection");

            Console.WriteLine("Adding sample data...");
            docDb.CreateFamilyDocumentIfNotExists(Families.Andersen).Wait();
            docDb.CreateFamilyDocumentIfNotExists(Families.Wakefield).Wait();

            Console.WriteLine("Reading data...");
            foreach (var family in docDb.GetFamilyLinq(Families.Andersen.LastName))
            {
                Console.WriteLine("\n\tRead\n {0}", family);
            }

            Console.WriteLine("Updating data...");
            // Update the Grade of the Andersen Family child
            Families.Andersen.Children[0].Grade = 6;
            docDb.ReplaceFamilyDocument(Families.Andersen.Id, Families.Andersen).Wait();
            Console.WriteLine("Reading data...");
            foreach (var family in docDb.GetFamilySql(Families.Andersen.LastName))
            {
                Console.WriteLine("\n\tRead\n {0}", family);
            }

            // Delete the document
            docDb.DeleteFamilyDocument(Families.Andersen.Id).Wait();
            docDb.DeleteDatabaseAsync().Wait();
        }        
    }
}
