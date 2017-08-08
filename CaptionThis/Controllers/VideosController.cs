using CaptionThis.Data;
using CaptionThis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CaptionThis.Controllers
{
    [Authorize]
    public class VideosController : Controller
    {
        private readonly VideoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VideosController(VideoDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Videos
        public async Task<IActionResult> Index()
        {
            // only get the videos for the user
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var ownerId = user.Id;

            var videos = from v in _context.Videos
                         where v.OwnerId == ownerId
                         select v;

            return View(await videos.ToListAsync());
        }

        // GET: Videos/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var video = await _context.Videos
                .SingleOrDefaultAsync(m => m.Id == id);
            if (video == null)
            {
                return NotFound();
            }

            return View(video);
        }

        // GET: Videos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Videos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Language,Url,Public")] Video video)
        {
            if (ModelState.IsValid)
            {
                _context.Add(video);

                // call the API and get the resulting job ID
                // set the In Progress state
                video.State = "In Progress";

                // set the owner id to the current user
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var ownerId = user.Id;
                video.OwnerId = ownerId;

                // get the owner API token -- if there isn't one, fail here
                if (string.IsNullOrEmpty(user.ApiToken))
                {
                    ModelState.AddModelError("API Token", "You need to add an API Token to your user profile.");
                    return View(video);
                }
                var apiToken = user.ApiToken;

                // call the API to upload the video
                var client = new HttpClient();
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // set the API token in the header
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiToken);

                // request parameters
                queryString["name"] = video.Name;
                queryString["privacy"] = video.Public ? "Public" : "Private";
                queryString["videoUrl"] = video.Url;
                //queryString["callbackUrl"] = $"https://vttgen-notify.azurewebsites.net/api/CompletionNotify?code=t7yfZcsZBerQObWZiDRugAAXt20Ks4ZmFpLAbIorFs8Pm88rIEpCHA==&projectName={video.Name}";
                queryString["callbackUrl"] = $"https://caption-this.azurewebsites.net/api/ProcessingCallback?projectName={video.Name}&emailAddress={user.Email}"; // will be appended with video ID and state

                var requestUri = "https://videobreakdown.azure-api.net/Breakdowns/Api/Partner/Breakdowns?" + queryString;

                HttpResponseMessage response;

                byte[] body = Encoding.UTF8.GetBytes("body");

                using (var content = new ByteArrayContent(body))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
                    response = await client.PostAsync(requestUri, content);

                    if (response.IsSuccessStatusCode)
                    {
                        // this will be the video ID
                        var responseContent = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());
                        video.Id = responseContent;
                    }
                    else
                    {
                        // TODO: Um, do something here.
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(video);
        }

        // GET: Videos/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var video = await _context.Videos.SingleOrDefaultAsync(m => m.Id == id);
            if (video == null)
            {
                return NotFound();
            }
            return View(video);
        }

        // POST: Videos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,Language,Url,Public,State,VttUrl")] Video video)
        {
            if (id != video.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // set the owner id to the current user
                    var user = await _userManager.GetUserAsync(HttpContext.User);
                    var ownerId = user.Id;
                    video.OwnerId = ownerId;

                    _context.Update(video);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VideoExists(video.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(video);
        }

        // GET: Videos/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var video = await _context.Videos
                .SingleOrDefaultAsync(m => m.Id == id);
            if (video == null)
            {
                return NotFound();
            }

            return View(video);
        }

        // POST: Videos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var video = await _context.Videos.SingleOrDefaultAsync(m => m.Id == id);
            _context.Videos.Remove(video);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VideoExists(string id)
        {
            return _context.Videos.Any(e => e.Id == id);
        }
    }
}
