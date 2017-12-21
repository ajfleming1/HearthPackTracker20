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
                        Text = "Welcome to the Dev Version of Hearthstone Pack Tracker. " +
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
                    var packCounter = 0;
                    var packType = string.Empty;
                    var legendaryCount = 0;

                    switch (intentRequest.Intent.Name)
                    {
                        case "AMAZON.HelpIntent":
                            output = new PlainTextOutputSpeech()
                            {
                                Text = "Drew, you are here."
                            };

                            returnResponse = ResponseBuilder.Ask(output, reprompt);
                            returnResponse = this.GetHelpResponse();
                            break;
                        case "CurrentCount":
                            returnResponse = this.GetCurrentCountResponse();
                            break;
                        case "Multipack":
                            packCounter = Convert.ToInt32(intentRequest.Intent.Slots["PackCounter"].Value);
                            packType = intentRequest.Intent.Slots["Packtype"].Value;
                            returnResponse = this.GetPackOpenedResponse(packCounter, packType);
                            break;
                        case "PackOpened":
                            packType = intentRequest.Intent.Slots["Packtype"].Value;
                            legendaryCount = Convert.ToInt32(intentRequest.Intent.Slots["LegendCount"].Value);
                            returnResponse = this.GetPackOpenedResponse(1, packType);
                            break;
                        case "PackTypes":
                            returnResponse = this.GetPackTypesReponse();
                            break;
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

        private SkillResponse GetPackTypesReponse()
        {
            var speech = new Alexa.NET.Response.PlainTextOutputSpeech()
            {
                Text = "Pack Types"
            };

            var finalResponse = ResponseBuilder.Tell(speech);
            return finalResponse;
        }

        private SkillResponse GetHelpResponse()
        {
            var speech = new Alexa.NET.Response.PlainTextOutputSpeech()
            {
                Text = "Help"
            };

            var finalResponse = ResponseBuilder.Tell(speech);
            return finalResponse;
        }

        private SkillResponse GetCurrentCountResponse()
        {
            var speech = new Alexa.NET.Response.PlainTextOutputSpeech()
            {
                Text = "Current Count"
            };

            var finalResponse = ResponseBuilder.Tell(speech);
            return finalResponse;
        }

        private SkillResponse GetPackOpenedResponse(int packCounter, string packType, int legendaryCount = 0)
        {
            var speech = new Alexa.NET.Response.PlainTextOutputSpeech()
            {
                Text = String.Format("Pack Opened. {0}, {1}, {2}.", packCounter, packType, legendaryCount)
            };

            var finalResponse = ResponseBuilder.Tell(speech);
            return finalResponse;
        }
    }
}
