using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.FormFlow;

namespace SinergijaBot.Dialogs
{
    [LuisModel("67ae8e7f-f82d-46e7-9102-f1d05626935b", "c10581d6707d4fc5aed63ab9d6e29f74")]

    [Serializable]
    public class LuisRootDialog : LuisDialog<object>
    {
        private string Name = "";
        private string Country = "";
        private bool HasBeenIdentified = false;

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Command not found, sorry: '{result.Query}'. Type 'help' for help";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        //Simple repsonse

        [LuisIntent("Hello")]
        public async Task Hello(IDialogContext context, LuisResult result)
        {
            string message = "Well hello to you too! use \"List commands\" to show you, what can I do";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }



        


        //Session

        [LuisIntent("SessionDetails")]
        public async Task SessionDetails(IDialogContext context, LuisResult result)
        {
            EntityRecommendation Session;
            if (!result.TryFindEntity("Session", out Session))
            {
                Session = new EntityRecommendation(type: "Session") { Entity = string.Empty };
            }

            int x;
            string message;
            if (int.TryParse(Session.Entity, out x))
            {

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                HeroCard heroCard = new HeroCard()
                {
                    Title = x.ToString(),
                    Subtitle = $"That is the session ID.",
                    Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "Click for full agenda",
                                Type = ActionTypes.OpenUrl,
                                Value = "https://www.mssinergija.net/en/sinergija16/agenda/Pages/Agenda-intro.aspx"
                            }
                        }
                };


                resultMessage.Attachments.Add(heroCard.ToAttachment());

                await context.PostAsync(resultMessage);
            }
            else
            {
                message = "you need to provide session number";
                await context.PostAsync(message);

            }

            context.Wait(this.MessageReceived);
        }



        [LuisIntent("ShowSpeaker")]
        public async Task ShowSpeaker(IDialogContext context, LuisResult result)
        {
            EntityRecommendation Speaker;
            if (!result.TryFindEntity("Speaker", out Speaker))
            {
                Speaker = new EntityRecommendation(type: "Session") { Entity = string.Empty };
            }

            var resultMessage = context.MakeMessage();
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            resultMessage.Attachments = new List<Attachment>();


            ThumbnailCard thumbnailCard = new ThumbnailCard()
            {
                Title = "Bob",
                Subtitle = $"Could not find \"{Speaker.Entity}\". But here is Bob",
                Images = new List<CardImage>()
                        {
                            new CardImage() { Url = "https://www.mssinergija.net/sr/sinergija16/vesti/PublishingImages/Predavaci/Bojan_Vrhovnik_150x150.jpg" }
                        },
            };


            resultMessage.Attachments.Add(thumbnailCard.ToAttachment());

            await context.PostAsync(resultMessage);


            context.Wait(this.MessageReceived);
        }


        //[LuisIntent("Hello")]
        public async Task Hello2(IDialogContext context, LuisResult result)
        {

            if (HasBeenIdentified)
            {
                string message = $"Well hello {Name}! Welcome back";
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
            else
            {
                string message = "Well hello to you too! Provide your data and I will remember you, ok?";
                await context.PostAsync(message);
                

                var nameQuery = new NameQuery();

                var nameFormDialog = new FormDialog<NameQuery>(nameQuery, this.BuildNameForm, FormOptions.PromptInStart, result.Entities);
           
                context.Call(nameFormDialog, this.ResumeAfterNameFormDialog);

            }

        }
        private IForm<NameQuery> BuildNameForm()
        {
            OnCompletionAsyncDelegate<NameQuery> processNameSearch = async (context, state) =>
            {
                var message = "Hello to ";
                if (!string.IsNullOrEmpty(state.UserName))
                {
                    Name = state.UserName;
                    HasBeenIdentified = true;
                    message += Name + " ";
                }
                if (!string.IsNullOrEmpty(state.Country))
                {
                    Country = state.Country;

                    message += "from "+Country;
                }

                await context.PostAsync(message);
            };

            return new FormBuilder<NameQuery>()
                //.Field(nameof(NameQuery.UserName), (state) => string.IsNullOrEmpty(state.UserName))
                //.Field(nameof(NameQuery.Country), (state) => string.IsNullOrEmpty(state.Country))
                .Field(nameof(NameQuery.UserName))
                .Field(nameof(NameQuery.Country))
                .OnCompletion(processNameSearch)
                .Build();
        }

        private async Task ResumeAfterNameFormDialog(IDialogContext context, IAwaitable<NameQuery> result)
        {
            try
            {
                var searchQuery = await result;
                //magic 
                //await context.PostAsync(resultMessage);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }

    }
}