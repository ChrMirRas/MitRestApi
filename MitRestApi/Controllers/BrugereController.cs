using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MitRestApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.IO;
using Npgsql.Internal;





using System.IO;
using QuestPDF.Fluent;

namespace MitRestApi.Controllers
{
   
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        public class PagedResult<T>
        {
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public required List<T> Items { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var total = await _context.Users.CountAsync();

            if ((page - 1) * pageSize >= total && total != 0)
                return BadRequest("Page number is too high.");

            var users = await _context.Users
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<User>
            {
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                Items = users
            };

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] User newUser)
        {
            if (newUser == null)
                return BadRequest("User data is required.");

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Name = updatedUser.Name;
            user.BirthDate = updatedUser.BirthDate;
            user.Gender = updatedUser.Gender;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Ny endpoint til Excel-export
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel()
        {
            var users = await _context.Users.OrderBy(u => u.Id).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");

            // Headers
            worksheet.Cell(1, 1).Value = "Id";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "BirthDate";
            worksheet.Cell(1, 4).Value = "Gender";

            // Data
            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                worksheet.Cell(i + 2, 1).Value = user.Id;
                worksheet.Cell(i + 2, 2).Value = user.Name;
                worksheet.Cell(i + 2, 3).Value = user.BirthDate.ToString("dd-MMM-yyyy");
                worksheet.Cell(i + 2, 4).Value = user.Gender;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "Users.xlsx";

            return File(stream.ToArray(), contentType, fileName);
        }

        [HttpGet("export/pdf")]
        public IActionResult ExportToPdf()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var users = _context.Users.ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Header().Text("Users List");
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Id");
                            header.Cell().Text("Name");
                            header.Cell().Text("BirthDate");
                        });

                        foreach (var user in users)
                        {
                            table.Cell().Text(user.Id.ToString());
                            table.Cell().Text(user.Name);
                            table.Cell().Text(user.BirthDate.ToShortDateString());
                        }
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", "users.pdf");
        }



    }
}
