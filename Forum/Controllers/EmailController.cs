namespace Forum.Controllers
{
    using Forum.Services;
    using Microsoft.AspNetCore.Mvc;

    public class EmailController : Controller
    {
        private readonly PostmailService _postmailService;

        public EmailController(PostmailService postmailService)
        {
            _postmailService = postmailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendTestEmail(string recipientEmail)
        {
            var emailSent = await _postmailService.SendEmailAsync(
                recipientEmail,
                "Welcome To Ayush's Forum",
                "This project is an ASP.NET Core-based web application that provides a platform for users to post and discuss questions. Key features include:\r\n\r\n    User authentication and authorization.\r\n    The ability to create, read, update, and delete questions and responses.\r\n    Optional image upload functionality for posts.\r\n    Email notifications, demonstrating integration with external services like Postmail for sending emails.\r\n\r\nThe project showcases the implementation of a model-view-controller (MVC) architecture with seamless user interaction and external API integrations."
            );

            if (emailSent)
            {
                ViewBag.Message = "Email sent successfully!";
            }
            else
            {
                ViewBag.Message = "Failed to send email.";
            }

            return View("Index");
        }
    }

}
