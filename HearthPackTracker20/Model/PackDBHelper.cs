using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;

namespace HearthPackTracker20.Model
{
    public class PackDBHelper
    {
        private string accessKey = Properties.Resources.accessKey;
        private string secretKey = Properties.Resources.secretKey;

        private static string dynamoDBTableName = "Markets";
        private static string hashKey = "MarketId";

        private AmazonDynamoDBClient client { get; set; }
        private DynamoDBContext context { get; set; }

        public PackDBHelper()
        {
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
        }

        public async Task VerifyTable()
        {
            await this.VerifyTable(dynamoDBTableName);
        }

        public async Task VerifyTable(string tableName)
        {
            var tableResponse = await client.ListTablesAsync();
            if (!tableResponse.TableNames.Contains(tableName) && !CreateTable(tableName).Result)
            {
                throw new Exception("Could not find or create table: " + tableName);
            }

            context = new DynamoDBContext(client);
        }

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

        public async Task SaveMarket(Pack pack)
        {
            await context.SaveAsync<Pack>(pack);
        }

        public async Task<Pack> GetMarket(string marketId)
        {
            List<ScanCondition> conditions = new List<ScanCondition>();
            conditions.Add(new ScanCondition("MarketId", ScanOperator.Equal, marketId));
            var allDocs = await context.ScanAsync<Pack>(conditions).GetRemainingAsync();

            return allDocs[0];
        }
    }
}
