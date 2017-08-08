using CaptionThis.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace CaptionThis.API
{
    [Produces("application/json")]
    public class ProcessingCallbackController : Controller
    {
        private readonly VideoDbContext _context;
        private readonly IConfiguration _configuration;

        public ProcessingCallbackController(VideoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [Route("api/ProcessingCallback")]
        public async Task JobComplete(string projectName, string emailAddress, string id, string state)
        {
            // get the video to update
            var video = await _context.Videos
                .SingleOrDefaultAsync(m => m.Id == id);

            var vttUrl = string.Empty;

            // if found update
            if (video != null)
            {
                video.State = state;

                // get the VTT URL
                var client = new HttpClient();
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _configuration["ViApiKey"]);

                var uri = $"https://videobreakdown.azure-api.net/Breakdowns/Api/Partner/Breakdowns/{id}/VttUrl";

                var response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());
                    vttUrl = responseContent;
                    video.VttUrl = vttUrl;

                    // notify the user
                    var mail = new SendGridClient(_configuration["Authentication:SendGrid:ClientSecret"]);
                    var from = new EmailAddress("donotreply@captionthis.media", "Caption This!");
                    var subject = $"[Caption This] Job ID: {projectName} is {state}";
                    var to = new EmailAddress(emailAddress);
                    System.Diagnostics.Trace.TraceInformation($"Sending notification to ${emailAddress}.");
                    var plainTextContent = $"Your media file {projectName} was proecessed for transcript interpretation and the WebVTT caption file is complete.  You can get it at {vttUrl}.";
                    var htmlContent = $"Your media file <strong>{projectName}</strong> was proecessed for transcript interpretation and the WebVTT caption file is complete.  You can get it at {vttUrl}.";
                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                    var mailResponse = await mail.SendEmailAsync(msg);

                    System.Diagnostics.Trace.TraceInformation($"Mail Response Status: {mailResponse.StatusCode.ToString()}");

                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}