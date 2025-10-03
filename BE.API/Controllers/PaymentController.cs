using BE.API.DTOs.Request;
using BE.BOs.Models;
using BE.REPOs.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepo _paymentRepo;

        public PaymentController(IPaymentRepo paymentRepo)
        {
            _paymentRepo = paymentRepo;
        }

        [HttpGet]
        //[Authorize(Policy = "AdminOnly")]
        public ActionResult GetAllPayments()
        {
            try
            {
                var payments = _paymentRepo.GetAllPayments();
                var response = payments.Select(p => new
                {
                    p.PaymentId,
                    p.OrderId,
                    p.PayerId,
                    p.PaymentType,
                    p.Amount,
                    p.PaymentMethod,
                    p.Status,
                    p.CreatedDate,
                    PayerName = p.Payer?.FullName,
                    OrderDetails = new
                    {
                        p.Order?.OrderId,
                        p.Order?.TotalAmount,
                        p.Order?.Status
                    }
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public ActionResult GetPaymentById(int id)
        {
            try
            {
                var payment = _paymentRepo.GetPaymentById(id);
                if (payment == null)
                {
                    return NotFound();
                }

                // Verify if user has access to this payment
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (payment.PayerId != userId && !User.IsInRole("1")) // Not owner and not admin
                {
                    return Forbid();
                }

                var response = new
                {
                    payment.PaymentId,
                    payment.OrderId,
                    payment.PayerId,
                    payment.PaymentType,
                    payment.Amount,
                    payment.PaymentMethod,
                    payment.Status,
                    payment.CreatedDate,
                    PayerName = payment.Payer?.FullName,
                    OrderDetails = new
                    {
                        payment.Order?.OrderId,
                        payment.Order?.TotalAmount,
                        payment.Order?.Status
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        //[Authorize(Policy = "MemberOnly")]
        public ActionResult CreatePayment([FromBody] PaymentRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

                var payment = new Payment
                {
                    OrderId = request.OrderId,
                    PayerId = userId,
                    PaymentType = request.PaymentType,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod
                };

                var createdPayment = _paymentRepo.CreatePayment(payment);

                var response = new
                {
                    createdPayment.PaymentId,
                    createdPayment.OrderId,
                    createdPayment.Amount,
                    createdPayment.PaymentMethod,
                    createdPayment.Status,
                    createdPayment.CreatedDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        //[Authorize(Policy = "AdminOnly")]
        public ActionResult UpdatePaymentStatus(int id, [FromBody] PaymentRequest request)
        {
            try
            {
                var payment = _paymentRepo.GetPaymentById(id);
                if (payment == null)
                {
                    return NotFound();
                }

                payment.Status = request.Status;
                var updatedPayment = _paymentRepo.UpdatePayment(payment);

                var response = new
                {
                    updatedPayment.PaymentId,
                    updatedPayment.Status,
                    UpdatedDate = DateTime.Now
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("order/{orderId}")]
        public ActionResult GetPaymentsByOrder(int orderId)
        {
            try
            {
                var payments = _paymentRepo.GetPaymentsByOrderId(orderId);
                var response = payments.Select(p => new
                {
                    p.PaymentId,
                    p.PaymentType,
                    p.Amount,
                    p.PaymentMethod,
                    p.Status,
                    p.CreatedDate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("user/{payerId}")]
        public ActionResult GetPaymentsByPayer(int payerId)
        {
            try
            {
                // Verify if user has access
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (payerId != userId && !User.IsInRole("1")) // Not owner and not admin
                {
                    return Forbid();
                }

                var payments = _paymentRepo.GetPaymentsByPayerId(payerId);
                var response = payments.Select(p => new
                {
                    p.PaymentId,
                    p.OrderId,
                    p.PaymentType,
                    p.Amount,
                    p.PaymentMethod,
                    p.Status,
                    p.CreatedDate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
