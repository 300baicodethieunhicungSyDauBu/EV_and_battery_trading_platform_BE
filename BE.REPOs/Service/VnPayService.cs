using BE.BOs.VnPayModels;
using BE.REPOs.Libs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace BE.REPOs.Service
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
        public bool ValidateSignature(Dictionary<string, string> queryParams);
    }

    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();

            pay.AddRequestData("vnp_Version", _configuration["VnPay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["VnPay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["VnPay:TmnCode"]);
            // ✅ Convert decimal amount to cents (VNPay requires amount in cents)
            // Use long to avoid overflow for large amounts
            var amountInCents = (long)(model.Amount * 100);
            pay.AddRequestData("vnp_Amount", amountInCents.ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["VnPay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["VnPay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription}");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", _configuration["VnPay:ReturnUrl"]);
            pay.AddRequestData("vnp_TxnRef", model.Name);

            return pay.CreateRequestUrl(_configuration["VnPay:BaseUrl"], _configuration["VnPay:HashSecret"]);
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            return pay.GetFullResponseData(collections, _configuration["VnPay:HashSecret"]);
        }

        public bool ValidateSignature(Dictionary<string, string> queryParams)
        {
            var pay = new VnPayLibrary();
            foreach (var kv in queryParams)
            {
                if (!string.IsNullOrEmpty(kv.Key) && kv.Key.StartsWith("vnp_"))
                {
                    pay.AddResponseData(kv.Key, kv.Value);
                }
            }

            var vnpSecureHash = queryParams.ContainsKey("vnp_SecureHash") ? queryParams["vnp_SecureHash"] : "";
            return pay.ValidateSignature(vnpSecureHash, _configuration["VnPay:HashSecret"]);
        }
    }
}