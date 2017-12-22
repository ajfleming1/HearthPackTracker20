using System;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using HearthPackTracker20.Model;
using Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace HearthPackTracker20
{
    public class Function
    {        
        /// <summary>
        /// Handles getting and setting a user's row in dynamodb
        /// </summary>
        private PackDBHelper hearthDBHelper = new PackDBHelper();

        /// <summary>
        /// Our Logger
        /// </summary>
        ILambdaLogger log;

        private readonly int MAX_PACKS = 40;

        /// <summary>
        /// Entry point for the Alexa skill. Handles all the intents and calls worker methods
        /// </summary>
        /// <param name="input">Contains user id and request type</param>
        /// <param name="context">Allows use of the logger to CloudWatch</param>
        /// <returns>Spoken phrase to let the user know what happened</returns>
        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            var userId = string.Empty;
            try
            {
                log = context.Logger;
                SkillResponse returnResponse = new SkillResponse();
                returnResponse.Response = new ResponseBody();

                // Get a db connection
                await hearthDBHelper.VerifyTable();
                if (input.Session != null)
                {
                    userId = input.Session.User.UserId;
                }
                else
                {
                    userId = "unknownUserDump";
                }
                
                if (input.GetRequestType() == typeof(LaunchRequest))
                {
                    log.LogLine($"Default LaunchRequest made");
                    var output = new PlainTextOutputSpeech()
                    {
                        Text = "Welcome to Hearthstone Pack Tracker. " +
                               "For instructions, please say 'Help' or 'How Does This Work.'"
                    };

                    var reprompt = new Reprompt()
                    {
                        OutputSpeech = new PlainTextOutputSpeech()
                        {
                            Text = "For instructions, please say 'Help' or 'How Does This Work'."
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
                            returnResponse = this.GetHelpResponse();
                            break;
                        case "CurrentCount":
                            returnResponse = await this.GetCurrentCountResponse(userId);
                            break;
                        case "MultiPack":
                            packCounter = Convert.ToInt32(intentRequest.Intent.Slots["PackCounter"].Value);
                            packType = intentRequest.Intent.Slots["Packtype"].Value;
                            returnResponse = await this.GetPackOpenedResponse(userId, packCounter, packType);
                            break;
                        case "PackOpened":
                            packType = intentRequest.Intent.Slots["Packtype"].Value;
                            legendaryCount = Convert.ToInt32(intentRequest.Intent.Slots["LegendCount"].Value);
                            returnResponse = await this.GetPackOpenedResponse(userId, 1, packType, legendaryCount);
                            break;
                        case "PackTypes":
                            returnResponse = this.GetPackTypesReponse();
                            break;
                        default:
                            var defaultOutput = new PlainTextOutputSpeech()
                            {
                                Text = "Sorry, I didn't understand what you said."
                            };

                            returnResponse = ResponseBuilder.Tell(defaultOutput);
                            break;
                    }
                }
                else
                {
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
                log.LogLine("Exception caught: " + e + " - " + e.InnerException);
                var output = new PlainTextOutputSpeech()
                {
                    Text = "Unable to complete request at this time."
                };

                return ResponseBuilder.Tell(output);
            }
        }

        /// <summary>
        /// Gives a response of all the valid pack types
        /// </summary>
        /// <returns>All the valid pack types as a spoken response</returns>
        public SkillResponse GetPackTypesReponse()
        {
            var speech = new Alexa.NET.Response.PlainTextOutputSpeech()
            {
                Text = "The pack types are Kobolds and Catacombs, Knights of the Frozen Throne, Journey to Un'goro, " +
                       "Mean Streets of Gadgetzan, Whispers of the Old Gods, The Grand Tournament, Goblins Versus Gnomes and the Classic Set."
            };

            var reprompt = new Reprompt()
            {
                OutputSpeech = new PlainTextOutputSpeech()
                {
                    Text = "You can now say 'I opened X packs of Pack Type'."
                }
            };

            return ResponseBuilder.Ask(speech, reprompt);
        }

        /// <summary>
        /// Gievs help to the user with some sample phrases
        /// </summary>
        /// <returns>Sample phrases as a spoken response</returns>
        public SkillResponse GetHelpResponse()
        {
            var speech = new Alexa.NET.Response.PlainTextOutputSpeech()
            {
                Text = "This is a skill to track the number of packs you open in the digital card game Hearthstone. " +
                       "You can say 'I opened X packs of Pack Type' or " +
                       "'I opened a pack of Pack type and got X legendary cards'. For a list of valid pack types say " +
                       "'What are the valid pack types'."
            };

            var reprompt = new Reprompt()
            {
                OutputSpeech = new PlainTextOutputSpeech()
                {
                    Text = "Say 'I opened X packs of Pack Type'."
                }
            };

            return ResponseBuilder.Ask(speech, reprompt);
        }

        /// <summary>
        /// Processes the intent of Current Count
        /// </summary>
        /// <returns>A response of the pack type with the most packs purchaced without a legendary</returns>
        public async Task<SkillResponse> GetCurrentCountResponse(string userId)
        {
            var user = await hearthDBHelper.GetPacks(userId);
            var maxCount = 0;
            var maxPackType = "Classic";
            if(maxCount < user.Pack.ClassicCount)
            {
                maxCount = user.Pack.ClassicCount;
                maxPackType = "Classic";
            }

            if (maxCount < user.Pack.FrozenThroneCount)
            {
                maxCount = user.Pack.FrozenThroneCount;
                maxPackType = "Frozen Throne";
            }

            if (maxCount < user.Pack.GadgetzanCount)
            {
                maxCount = user.Pack.GadgetzanCount;
                maxPackType = "Gadgetzan";
            }

            if (maxCount < user.Pack.GVGCount)
            {
                maxCount = user.Pack.GVGCount;
                maxPackType = "Goblins Versus Gnomes";
            }

            if (maxCount < user.Pack.KoboldsCount)
            {
                maxCount = user.Pack.KoboldsCount;
                maxPackType = "Kobolds and Catacombs";
            }

            if (maxCount < user.Pack.OldGodsCount)
            {
                maxCount = user.Pack.OldGodsCount;
                maxPackType = "Whispers of the Old Gods";
            }

            if (maxCount < user.Pack.TGTCount)
            {
                maxCount = user.Pack.TGTCount;
                maxPackType = "The Grand Tourament";
            }

            if (maxCount < user.Pack.UnGoroCount)
            {
                maxCount = user.Pack.UnGoroCount;
                maxPackType = "Journey to Un'Goro";
            }

            maxCount = (MAX_PACKS - maxCount) < 0 ? 1 : (MAX_PACKS - maxCount);
            var speech = new Alexa.NET.Response.PlainTextOutputSpeech()
            {
                Text = string.Format("You are closest to a legendary in the {0} set with at most {1} packs remaining.", maxPackType, maxCount)
            };

            return ResponseBuilder.Tell(speech);
        }

        /// <summary>
        /// Handles the event and generates a response when a pack opened intent is received
        /// </summary>
        /// <param name="packCounter">Number of packs opened</param>
        /// <param name="packType">The set of the packs that were opened</param>
        /// <param name="legendaryCount">Number of legendary cards in the opened packs</param>
        /// <returns>Reponse acknowledging the pack openings</returns>
        public async Task<SkillResponse> GetPackOpenedResponse(string userId, int packCounter, string packType, int legendaryCount = 0)
        {
            var user = await hearthDBHelper.GetPacks(userId);
            if(legendaryCount > 0)
            {
                this.ResetPackCount(user, packType);
            }
            else
            {
                this.IncrementPackCount(packCounter, user, packType);
            }

            return await this.GetCurrentCountResponse(userId);
        }

        /// <summary>
        /// Increments the pack type for user by the pack counter
        /// </summary>
        /// <param name="packCounter">Number of packs opened</param>
        /// <param name="user">User's model object</param>
        /// <param name="packType">Pack that was opened</param>
        private void IncrementPackCount(int packCounter, Packs user, string packType)
        {
            var packs = user.Pack;
            if (PackTypeSynonyms.Classic.Contains(packType.ToLower()))
            {
                packs.ClassicCount += packCounter;
            }

            if (PackTypeSynonyms.Kobolds.Contains(packType.ToLower()))
            {
                packs.KoboldsCount += packCounter;
            }

            if (PackTypeSynonyms.UnGoro.Contains(packType.ToLower()))
            {
                packs.UnGoroCount += packCounter;
            }

            if (PackTypeSynonyms.GVG.Contains(packType.ToLower()))
            {
                packs.GVGCount += packCounter;
            }

            if (PackTypeSynonyms.Gadgetzan.Contains(packType.ToLower()))
            {
                packs.GadgetzanCount += packCounter;
            }

            if (PackTypeSynonyms.FrozenThrone.Contains(packType.ToLower()))
            {
                packs.FrozenThroneCount += packCounter;
            }

            if (PackTypeSynonyms.OldGods.Contains(packType.ToLower()))
            {
                packs.OldGodsCount += packCounter;
            }

            if (PackTypeSynonyms.TGT.Contains(packType.ToLower()))
            {
                packs.TGTCount += packCounter;
            }

            Task t = hearthDBHelper.SavePack(user);
            t.Wait();
        }

        /// <summary>
        /// Resets the packType counter to 0
        /// </summary>
        /// <param name="user">User's model object</param>
        /// <param name="packType">Set that was opened</param>
        private void ResetPackCount(Packs user, string packType)
        {
            var packs = user.Pack;
            if (PackTypeSynonyms.Classic.Contains(packType.ToLower()))
            {
                packs.ClassicCount = 0;
            }

            if (PackTypeSynonyms.Kobolds.Contains(packType.ToLower()))
            {
                packs.KoboldsCount = 0;
            }

            if (PackTypeSynonyms.UnGoro.Contains(packType.ToLower()))
            {
                packs.UnGoroCount = 0;
            }

            if (PackTypeSynonyms.GVG.Contains(packType.ToLower()))
            {
                packs.GVGCount = 0;
            }

            if (PackTypeSynonyms.Gadgetzan.Contains(packType.ToLower()))
            {
                packs.GadgetzanCount = 0;
            }

            if (PackTypeSynonyms.FrozenThrone.Contains(packType.ToLower()))
            {
                packs.FrozenThroneCount = 0;
            }

            if (PackTypeSynonyms.OldGods.Contains(packType.ToLower()))
            {
                packs.OldGodsCount = 0;
            }

            if (PackTypeSynonyms.TGT.Contains(packType.ToLower()))
            {
                packs.TGTCount = 0;
            }

            Task t = hearthDBHelper.SavePack(user);
            t.Wait();
        }
    }
}
