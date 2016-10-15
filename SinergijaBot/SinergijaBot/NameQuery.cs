using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.FormFlow;


namespace SinergijaBot
{
    [Serializable]
    public class NameQuery
    {
        [Prompt("Please enter your name")]
        [Optional]
        public string UserName { get; set; }

        [Prompt("Which country")]
        [Optional]
        public string Country { get; set; }
    }
}