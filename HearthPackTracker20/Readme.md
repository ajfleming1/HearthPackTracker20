#Hearthstone Pack Tracker

It's been well documented that after opening 39 Hearthstone card packs of the same set without a Legendary card, the 40th pack will be guaranteed to have one. This is an Alexa skill that allows a user to tell Alexa when he or she has opened a pack. These counts are incremented and stored on DynamoDB so the user can see how close they are getting to a legendary card.

## Here are some steps to follow from Visual Studio:

To deploy your function to AWS Lambda, right click the project in Solution Explorer and select *Publish to AWS Lambda*.

To view your deployed function open its Function View window by double-clicking the function name shown beneath the AWS Lambda node in the AWS Explorer tree.

To perform testing against your deployed function use the Test Invoke tab in the opened Function View window.

To configure event sources for your deployed function, for example to have your function invoked when an object is created in an Amazon S3 bucket, use the Event Sources tab in the opened Function View window.

To update the runtime configuration of your deployed function use the Configuration tab in the opened Function View window.

To view execution logs of invocations of your function use the Logs tab in the opened Function View window.

## Here are some steps to follow to get started from the command line:

Once you have edited your function you can use the following command lines to build, test and deploy your function to AWS Lambda from the command line (these examples assume the project name is *EmptyFunction*):

Restore dependencies
```
    cd "HearthPackTracker20"
    dotnet restore
```

Execute unit tests
```
    cd "HearthPackTracker20/test/HearthPackTracker20.Tests"
    dotnet test
```

Deploy function to AWS Lambda
```
    cd "HearthPackTracker20/src/HearthPackTracker20"
    dotnet lambda deploy-function
```
