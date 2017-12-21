using System;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using HearthPackTracker20.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace HearthPackTracker20
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            try
            {
                var log = context.Logger;
                SkillResponse returnResponse = new SkillResponse();
                returnResponse.Response = new ResponseBody();

                // Get a db connection
                var hearthDBHelper = new PackDBHelper();
                await hearthDBHelper.VerifyTable();
                string userId = "";
                if (input.Session != null)
                {
                    userId = input.Session.User.UserId;
                }
                else
                {
                    userId = "unknownUserDump";
                }

                // log.LogLine("User Id: " + input.Session.User.UserId);
                if (input.GetRequestType() == typeof(LaunchRequest))
                {
                    log.LogLine($"Default LaunchRequest made");
                    var output = new PlainTextOutputSpeech()
                    {
                        Text = "Welcome to Open Market. " +
                               "For instructions, please say Help or How does this work. "
                    };

                    var reprompt = new Reprompt()
                    {
                        OutputSpeech = new PlainTextOutputSpeech()
                        {
                            Text = "Drew, Please add text here at some point."
                        }
                    };

                    returnResponse = ResponseBuilder.Ask(output, reprompt);
                }
                else if (input.GetRequestType() == typeof(IntentRequest))
                {
                    var intentRequest = (IntentRequest)input.Request;
                    log.LogLine($"Triggered " + intentRequest.Intent.Name);
                    var output = new PlainTextOutputSpeech()
                    {
                        Text = "Please include pack type and legendary amount in your request"
                    };
                    returnResponse = ResponseBuilder.Tell(output);

                    var reprompt = new Reprompt();


                    var slots = intentRequest.Intent.Slots;
                    switch (intentRequest.Intent.Name)
                    {
                        #region Help
                        case "AMAZON.HelpIntent":
                            output = new PlainTextOutputSpeech()
                            {
                                Text = "Drew, you are here."
                            };

                            returnResponse = ResponseBuilder.Ask(output, reprompt);
                            break;
                            #endregion

                    }
                }
                else
                {
                    // returnResponse.Response.ShouldEndSession = false;
                    var output = new PlainTextOutputSpeech()
                    {
                        Text = "Sorry, I didn't understand what you said."
                    };

                    returnResponse = ResponseBuilder.Tell(output);
                }

                return returnResponse;
            }
            catch (Exception e)
            {
                var logg = context.Logger;
                logg.LogLine("Exception caught: " + e + " - " + e.InnerException);
                SkillResponse returnResponse = new SkillResponse();
                returnResponse.Response = new ResponseBody();

                var output = new PlainTextOutputSpeech()
                {
                    Text = "Unable to complete request. Have you enabled zip code permissions in your Alexa App?."
                };

                returnResponse = ResponseBuilder.Tell(output);
                // returnResponse.Response.ShouldEndSession = false;

                return returnResponse;
            }
        }
    }
}
