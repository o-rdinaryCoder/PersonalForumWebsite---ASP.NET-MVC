using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Forum.Data;
using Forum.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Humanizer;

namespace Forum.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public QuestionsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Questions
        public async Task<IActionResult> Index(int? pageNumber)
        {
            var applicationDbContext = _context.Questions.Include(q => q.User).Include(a=>a.Answers);

            //SQL Equivalent query
            //SELECT q.*, u.*, a.*
            //FROM Questions q
            //LEFT JOIN Users u ON q.IdentityUserId = u.Id
            //LEFT JOIN Answers a ON q.Id = a.QuestionId;

            //This query gets all questions from the Questions table and also includes the related User and Answers for every question.


            int pageSize = 3;

            return View(await PaginatedList<Question>.CreateAsync(applicationDbContext.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        

        // GET: Questions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Questions == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.User)
                .Include(c => c.Answers)
                .ThenInclude(q => q.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            //SQL Equivalent
            //SELECT q.*, u.*, a.*, au.*
            //FROM Questions q
            //LEFT JOIN Users u ON q.IdentityUserId = u.Id
            //LEFT JOIN Answers a ON q.Id = a.QuestionId
            //LEFT JOIN Users au ON a.IdentityUserId = au.Id
            //WHERE q.Id = @id
            //LIMIT 1;

            //This query gets a specific question, the user who posted it, answers, etc from the database based on the id provided

            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // GET: Questions/Create
        [Authorize]
        public IActionResult Create()
        {
           
            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,IdentityUserId")] Question question, IFormFile Image)
        {
            if (ModelState.IsValid)
            {
                if (Image != null && Image.Length > 0)
                {
                    var fileName = Path.GetFileName(Image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(stream);
                    }

                    question.IdentityUserId = _userManager.GetUserId(User);

                    question.ImagePath = "/images/" + fileName;
                }

                _context.Add(question);
                await _context.SaveChangesAsync();
                //INSERT INTO Questions(Title, Description, IdentityUserId, ImagePath)
                //VALUES('Question Title', 'Question Description', 'user-id-value', '/images/image.jpg');

                //This query adds a new question to the database and saves the changes. 

                return RedirectToAction(nameof(Index));
            }
            
            return View(question);
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAnswer([Bind("Id,Content, QuestionId, IdentityUserId")] Answer answer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(answer);
                await _context.SaveChangesAsync();              
            }
            var question =await _context.Questions
                .Include(q=>q.User)
                .Include(a =>a.Answers)
                .ThenInclude(q => q.User)
                .FirstOrDefaultAsync(q =>q.Id == answer.QuestionId);

            //SELECT q.*, u.*, a.*, au.*
            //FROM Questions q
            //LEFT JOIN Users u ON q.IdentityUserId = u.Id
            //LEFT JOIN Answers a ON q.Id = a.QuestionId
            //LEFT JOIN Users au ON a.IdentityUserId = au.Id
            //WHERE q.Id = @answer_QuestionId
            //LIMIT 1;

            //This query is used to get the spcific question when starting from an answer.


            return View("Details", question);
        }

        // GET: Questions/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Questions == null)
            {
                return NotFound();
            }

            var question = await _context.Questions.FindAsync(id);

            if (question.IdentityUserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", question.IdentityUserId);
            return View(question);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,IdentityUserId")] Question question)
        {
            if (id != question.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(question);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(question.Id))
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
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", question.IdentityUserId);
            return View(question);
        }

        // GET: Questions/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Questions == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question.IdentityUserId != User.FindFirstValue(ClaimTypes.NameIdentifier)) 
            {
                return NotFound();
            }


            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Questions == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Questions'  is null.");
            }
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionExists(int id)
        {
          return (_context.Questions?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
