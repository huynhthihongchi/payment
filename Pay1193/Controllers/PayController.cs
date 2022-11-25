using Microsoft.AspNetCore.Mvc;
using Pay1193.Entity;
using Pay1193.Models;
using Pay1193.Services;
using Pay1193.Services.Implement;

namespace Pay1193.Controllers
{
    public class PayController : Controller
    {
        private readonly IPayService _payService;
        private readonly IEmployee _employeeService;
        private readonly INationalInsuranceService _nationalInsuranceService;
        private readonly ITaxService _taxService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private decimal overtimeHours;
        private decimal contractualEarnings;
        private decimal overtimeEarnings;
        private decimal nationalInsurance;
        private decimal totalEarnings;
        private decimal tax;
        private decimal unionFee;
        private decimal studentLoan;
        private decimal totalDeduction;


        public PayController(IEmployee employeeService, ITaxService taxService, IPayService payService, INationalInsuranceService nationalInsuranceService, IWebHostEnvironment webHostEnvironment)
        {
            _payService = payService;
            _employeeService = employeeService;
            _nationalInsuranceService = nationalInsuranceService;
            _taxService = taxService;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var pay = _payService.GetAll().Select(paymentRecord => new PayIndexViewModel
            {
                FullName = _employeeService.GetById(paymentRecord.EmployeeId).FullName,
                DatePay = paymentRecord.DatePay,
                MonthPay = paymentRecord.MonthPay,
                TaxYearId = paymentRecord.TaxYearId,
                TaxYear = _payService.GetTaxYearById(paymentRecord.TaxYearId).YearOfTax,
                TotalEarnings = paymentRecord.TotalEarnings,
                TotalDeduction = paymentRecord.TotalDeduction,
                NetPayment = paymentRecord.NetPayment,
                Employee = paymentRecord.Employee
            }).ToList();
            return View(pay);
        }
        
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.employees = _employeeService.GetAllEmployeesForPayroll();
            ViewBag.taxYears = _payService.GetAllTaxYear();
            var model = new PayCreateViewModel();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Create(PayCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                decimal overtimRate = _payService.OvertimeRate(model.HourlyRate);

                var pay = new PaymentRecord
                {
                    EmployeeId = model.EmployeeId,
                    Employee = _employeeService.GetById(model.EmployeeId),
                    DatePay = model.DatePay,
                    MonthPay = model.MonthPay,
                    TaxYearId =  model.TaxYear.Id,
                    TaxCode = model.TaxCode,
                    HourlyRate = model.HourlyRate,
                    HourWorked = model.HourWorked,
                    ContractualHours = model.ContractualHours,
                    OvertimeHours = overtimeHours = _payService.OverTimeHours(model.HourWorked, model.ContractualHours),
                    ContractualEarnings = contractualEarnings = _payService.ContractualEarning(model.ContractualHours, model.HourWorked, model.HourlyRate),
                    OvertimeEarnings = overtimeEarnings = _payService.OvertimeEarnings(overtimeHours, overtimRate),
                    Tax = tax = _taxService.TaxAmount(totalEarnings),
                    UnionFee = unionFee = _employeeService.UnionFee(model.EmployeeId),
                    SLC = studentLoan = _employeeService.StudentLoanRepaymentAmount(model.EmployeeId, totalEarnings),
                    TotalEarnings = totalEarnings = _payService.TotalEarnings(overtimeEarnings, contractualEarnings),
                    TotalDeduction = totalDeduction = _payService.TotalDeduction(tax, nationalInsurance, studentLoan, unionFee),
                    NetPayment = _payService.NetPay(totalEarnings, totalDeduction),
                    NiC = nationalInsurance = _nationalInsuranceService.NIContribution(totalEarnings)
                };
                await _payService.CreateAsync(pay);
                return RedirectToAction("Index");
            }
            ViewBag.employees = _employeeService.GetAllEmployeesForPayroll();
            ViewBag.taxYears = _payService.GetAllTaxYear();
            return View(model);
        }
        public IActionResult Detail(int id)
        {
            var paymentRecord = _payService.GetById(id);
            if (paymentRecord == null)
            {
                return NotFound();
            }

            var model = new PayDetailViewModel()
            {
                Id = paymentRecord.Id,
                EmployeeId = paymentRecord.EmployeeId,
                FullName = _employeeService.GetById(paymentRecord.EmployeeId).FullName,
                PayDate = paymentRecord.DatePay,
                PayMonth = paymentRecord.MonthPay,
                TaxYearId = paymentRecord.TaxYearId,
                Year = _payService.GetTaxYearById(paymentRecord.TaxYearId).YearOfTax,
                TaxCode = paymentRecord.TaxCode,
                HourlyRate = paymentRecord.HourlyRate,
                HourWorked = paymentRecord.HourWorked,
                ContractualHours = paymentRecord.ContractualHours,
                OvertimeHours = paymentRecord.OvertimeHours,
                OvertimeRate = _payService.OvertimeRate(paymentRecord.HourlyRate),
                ContractualEarnings = paymentRecord.ContractualEarnings,
                OvertimeEarnings = paymentRecord.OvertimeEarnings,
                Tax = paymentRecord.Tax,
                NiC = paymentRecord.NiC,
                UnionFee = paymentRecord.UnionFee,
                SLC = paymentRecord.SLC,
                TotalEarnings = paymentRecord.TotalEarnings,
                TotalDeduction = paymentRecord.TotalDeduction,
                Employee = paymentRecord.Employee,
                TaxYear = paymentRecord.TaxYear,
                NetPayment = paymentRecord.NetPayment
            };
            return View(model);
        }
    }
}
