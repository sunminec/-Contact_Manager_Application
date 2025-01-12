using Microsoft.AspNetCore.Mvc;
using Contact_Manager_Application.Data;
using Contact_Manager_Application.Models;
using System.IO;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.Linq;

namespace ContactManagerApp.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var contacts = _context.Contacts.Select(contact => new Contact
            {
                Id = contact.Id,
                Name = contact.Name ?? "",
                DateOfBirth = contact.DateOfBirth,
                Married = contact.Married,
                Phone = contact.Phone ?? "",
                Salary = contact.Salary
            }).ToList();

            return View("Index", contacts);
        }

        [HttpPost]
        public IActionResult UploadCsv(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (var stream = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(stream, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,  
                    MissingFieldFound = null 
                }))
                {
                    var records = csv.GetRecords<Contact>().ToList();
                    _context.Contacts.AddRange(records);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(Contact contact)
        {
            if (contact == null)
            {
                return RedirectToAction("Index");
            }

            var existingContact = _context.Contacts.Find(contact.Id);
            if (existingContact == null)
            {
                return NotFound();
            }

            existingContact.Name = contact.Name ?? "";
            existingContact.Phone = string.IsNullOrEmpty(contact.Phone) ? "" : contact.Phone;
            existingContact.DateOfBirth = contact.DateOfBirth;
            existingContact.Married = contact.Married;
            existingContact.Salary = contact.Salary;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var contact = _context.Contacts.Find(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}