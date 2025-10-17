using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Platform.Donations.Service.Models;
using Platform.Donations.Service.Services;

namespace Platform.Donations.Service.Controllers
{
    public class DonationsController : Controller
    {
        private readonly IDonationService _donationService;
        private readonly IPaymentService _paymentService;

        public DonationsController(IDonationService donationService, IPaymentService paymentService)
        {
            _donationService = donationService;
            _paymentService = paymentService;
        }

        // GET: Donations
        public IActionResult Index()
        {
            return View();
        }

        // GET: Donations/Create
        public IActionResult Create()
        {
            return View(new DonationModel());
        }

        // POST: Donations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonationModel donation)
        {
            if (ModelState.IsValid)
            {
                // Process payment
                var paymentResult = await _paymentService.ProcessPaymentAsync(donation);

                if (paymentResult.Success)
                {
                    donation.PaymentTransactionId = paymentResult.TransactionId;
                    donation.Status = DonationStatus.Completed;

                    await _donationService.CreateDonationAsync(donation);

                    return RedirectToAction(nameof(Success), new { id = donation.Id });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Payment failed: {paymentResult.ErrorMessage}");
                }
            }

            return View(donation);
        }

        // GET: Donations/Success/5
        public async Task<IActionResult> Success(int id)
        {
            var donation = await _donationService.GetDonationAsync(id);
            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }

        // GET: Donations/MyDonations
        public async Task<IActionResult> MyDonations(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return View("EmailRequired");
            }

            var donations = await _donationService.GetDonationsByDonorAsync(email);
            return View(donations);
        }

        // POST: Donations/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var success = await _donationService.CancelDonationAsync(id);

            if (success)
            {
                return RedirectToAction(nameof(Index));
            }

            return NotFound();
        }
    }
}
