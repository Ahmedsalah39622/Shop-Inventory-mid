using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using ShopInventory.Models;

namespace ShopInventory.Controllers
{
    public class InstallmentPaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public InstallmentPaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /InstallmentPayments/Create/{installmentId}
        public async Task<IActionResult> Create(int installmentId)
        {
            var inst = await _context.Installments.Include(i => i.Customer).FirstOrDefaultAsync(i => i.Id == installmentId);
            if (inst == null) return NotFound();
            ViewBag.Installment = inst;
            return View();
        }

        // POST: /InstallmentPayments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int installmentId, decimal amount, string paymentMethod)
        {
            var inst = await _context.Installments.FirstOrDefaultAsync(i => i.Id == installmentId);
            if (inst == null) return NotFound();

            // basic validation
            if (amount <= 0) ModelState.AddModelError("Amount", "Amount must be positive");
            if (!ModelState.IsValid)
            {
                ViewBag.Installment = inst;
                return View();
            }

            var payment = new InstallmentPayment
            {
                InstallmentId = installmentId,
                Amount = amount,
                PaymentMethod = paymentMethod
            };

            // update installment remaining amount and months
            inst.RemainingAmount = Math.Max(0, inst.RemainingAmount - amount);
            if (inst.NumberOfMonths > 0)
            {
                inst.NumberOfMonths = Math.Max(0, inst.NumberOfMonths - 1);
                inst.MonthlyAmount = inst.NumberOfMonths > 0 ? Math.Round(inst.RemainingAmount / inst.NumberOfMonths, 2) : 0;
            }

            // set next payment date to one month from now if there are still months remaining
            try
            {
                if (inst.NumberOfMonths > 0 && inst.RemainingAmount > 0)
                {
                    inst.NextPaymentDate = DateTime.Now.AddMonths(1);
                }
                else
                {
                    // fully paid, clear next payment date and mark completed
                    inst.NextPaymentDate = null;
                    inst.MonthlyAmount = 0;
                    inst.Status = "تم الانتهاء من الدفعات";
                }
            }
            catch
            {
                // ignore date adjustment errors
            }

            _context.InstallmentPayments.Add(payment);
            _context.Update(inst);
            await _context.SaveChangesAsync();

            // redirect to details page for the installment so the received payment is visible
            return RedirectToAction("Details", "InstallmentPayments", new { installmentId = installmentId });
        }

        // GET: /InstallmentPayments/Details/{installmentId}
        public async Task<IActionResult> Details(int installmentId)
        {
            var payments = await _context.InstallmentPayments
                .Where(p => p.InstallmentId == installmentId)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
            ViewBag.InstallmentId = installmentId;
            return View(payments);
        }
    }
}
