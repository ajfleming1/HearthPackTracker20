using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Models;

namespace HearthPackTracker20.Model
{
    public class PackDBHelper
    {
        private string accessKey = Properties.Resources.accessKey;
        private string secretKey = Properties.Resources.secretKey;

        private static string dynamoDBTableName = Properties.Resources.dynamoDBTableName;
        private static string hashKey = Properties.Resources.hashKey;

        private AmazonDynamoDBClient client { get; set; }
        private DynamoDBContext context { get; set; }

        /// <summary>
        /// Creates a new instance of PackDBHelper
        /// </summary>
        public PackDBHelper()
        {
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
        }

        /// <summary>
        /// Calls VerifyTable with dynamoDBTableName
        /// </summary>
        /// <returns>Task (void)</returns>
        public async Task VerifyTable()
        {
            await this.VerifyTable(dynamoDBTableName);
        }

        /// <summary>
        /// Makes sure the table is created and can be used
        /// </summary>
        /// <param name="tableName">Table to be verified</param>
        /// <returns>Task (void)</returns>
        public async Task VerifyTable(string tableName)
        {
            var tableResponse = await client.ListTablesAsync();
            if (!tableResponse.TableNames.Contains(tableName) && !CreateTable(tableName).Result)
            {
                throw new Exception("Could not find or create table: " + tableName);
            }

            context = new DynamoDBContext(client);
        }

        /// <summary>
        /// Creates the table on the very first run
        /// </summary>
        /// <param name="tableName">Table to be created</param>
        /// <returns>Task (void)</returns>
        private async Task<bool> CreateTable(string tableName)
        {
            await client.CreateTableAsync(new CreateTableRequest
            {
                TableName = tableName,
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 3,
                    WriteCapacityUnits = 1
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = hashKey,
                        KeyType = KeyType.HASH
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition {AttributeName = hashKey, AttributeType=ScalarAttributeType.S }
                }
            });

            bool isTableAvailable = false;
            int waitLimit = 10;
            int waitCount = 0;
            while (!isTableAvailable)
            {
                Thread.Sleep(5000);
                var tableStatus = await client.DescribeTableAsync(tableName);
                isTableAvailable = tableStatus.Table.TableStatus == "ACTIVE";
                waitCount++;
                if (waitLimit == waitCount)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Saves a pack to a user's row
        /// </summary>
        /// <param name="pack">The pack to be saved</param>
        /// <returns>Task (void)</returns>
        public async Task SavePack(Packs pack)
        {
            await context.SaveAsync<Packs>(pack);
        }

        /// <summary>
        /// Gets the users packs. Creates and saves a new user when applicable.
        /// </summary>
        /// <param name="userId">User of the skill</param>
        /// <returns>A user's pack</returns>
        public async Task<Packs> GetPacks(string userId)
        {
            await VerifyTable();
            List<ScanCondition> conditions = new List<ScanCondition>();
            conditions.Add(new ScanCondition(Properties.Resources.hashKey, ScanOperator.Equal, userId));
            var allDocs = await context.ScanAsync<Packs>(conditions).GetRemainingAsync();

            if(allDocs.Count == 0)
            {
                return this.CreateInitialPack(userId);
            }

            return allDocs[0];
        }

        /// <summary>
        /// Creates and empty set for a new user
        /// </summary>
        /// <param name="userId">User id of the new user</param>
        /// <returns>User's empty pack</returns>
        private Packs CreateInitialPack(string userId)
        {
            var user = new Packs()
            {
                UserId = userId,
                Pack = new PackMap()
                {
                    ClassicCount = 0,
                    WitchwoodCount = 0,
                    FrozenThroneCount = 0,
                    GadgetzanCount = 0,
                    GVGCount = 0,
                    KoboldsCount = 0,
                    OldGodsCount = 0,
                    TGTCount = 0,
                    UnGoroCount = 0
                }
            };

            Task t = this.SavePack(user);
            t.Wait();
            return user;
        }
    }
}
