using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Dropbox.Api;
using System.Linq;

/// <summary>
/// Token Store Binding Token Name Scienario.
/// The Token_url is specified up to the token name. 
/// Does: Lists the user's Dropbox files based on the specific token path - list is regenerated every second.
/// </summary>

namespace TokenStoreBindingExampleCodeControl
{
    public static class tokenName_timer_dropbox
    {
        [FunctionName("tokenName_timer_dropbox")]
        public static async void Run([TimerTrigger("*/1 * * * * * ")]TimerInfo myTimer, ILogger log,
             Binder binder)
        {
            // timer triggered every second (note: may be slowed down since this is an async method)
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var Token_url = Environment.GetEnvironmentVariable("Token_url_service");

            // An implicit binding is used here to access the Token_service param from app settings 
            TokenStoreInputBindingAttribute attribute = new TokenStoreInputBindingAttribute(Token_url, "tokenName", null); // Initialize TokenStoreBinding

            var outputToken = await binder.BindAsync<string>(attribute);

            if (!string.IsNullOrEmpty(outputToken))
            {
                using (var dbx = new DropboxClient(outputToken))
                {
                    var list = await dbx.Files.ListFolderAsync(string.Empty);

                    // show folders then files
                    foreach (var item in list.Entries.Where(i => i.IsFolder))
                    {
                        log.LogInformation($"Directory: {item.Name}");
                    }

                    foreach (var item in list.Entries.Where(i => i.IsFile))
                    {
                        log.LogInformation($"File: {item.Name}");
                    }
                }
            }
        }
    }

}
