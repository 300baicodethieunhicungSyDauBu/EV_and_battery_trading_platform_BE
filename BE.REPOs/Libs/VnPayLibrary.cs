using BE.BOs.VnPayModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BE.REPOs.Libs
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _requestData.Add(key, value);
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _responseData.Add(key, value);
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();

            foreach (var (key, value) in _requestData)
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            // Remove last '&'
            data.Remove(data.Length - 1, 1);
            var queryString = data.ToString();

            var vnpSecureHash = HmacSHA512(vnpHashSecret, queryString);
            return $"{baseUrl}?{queryString}&vnp_SecureHash={vnpSecureHash}";
        }

        public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            var vnPay = new VnPayLibrary();
            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, value);
                }
            }

            var vnpSecureHash = collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;
            var orderId = vnPay.GetResponseData("vnp_TxnRef");
            var vnPayTranId = vnPay.GetResponseData("vnp_TransactionNo");
            var vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
            var orderInfo = vnPay.GetResponseData("vnp_OrderInfo");

            var checkSignature = vnPay.ValidateSignature(vnpSecureHash, hashSecret);
            if (!checkSignature)
            {
                return new PaymentResponseModel
                {
                    Success = false,
                    OrderDescription = orderInfo,
                    OrderId = orderId,
                    TransactionId = vnPayTranId,
                    VnPayResponseCode = vnpResponseCode
                };
            }

            return new PaymentResponseModel
            {
                Success = true,
                PaymentMethod = "VnPay",
                OrderDescription = orderInfo,
                OrderId = orderId,
                TransactionId = vnPayTranId,
                VnPayResponseCode = vnpResponseCode
            };
        }

        private string GetResponseData()
        {
            var data = new StringBuilder();

            if (_responseData.ContainsKey("vnp_SecureHashType"))
                _responseData.Remove("vnp_SecureHashType");
            if (_responseData.ContainsKey("vnp_SecureHash"))
                _responseData.Remove("vnp_SecureHash");

            foreach (var (key, value) in _responseData)
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            data.Remove(data.Length - 1, 1);
            return data.ToString();
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseData();
            var myChecksum = HmacSHA512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private static string HmacSHA512(string key, string inputData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using var hmac = new HMACSHA512(keyBytes);
            var hashValue = hmac.ComputeHash(inputBytes);
            return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
        }

        public string GetIpAddress(HttpContext context)
        {
            try
            {
                var ipAddress = context.Connection.RemoteIpAddress;
                if (ipAddress == null)
                    return "127.0.0.1";

                if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    ipAddress = Dns.GetHostEntry(ipAddress).AddressList
                        .First(x => x.AddressFamily == AddressFamily.InterNetwork);
                }

                return ipAddress.ToString();
            }
            catch
            {
                return "127.0.0.1";
            }
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return CompareInfo.GetCompareInfo("en-US")
                .Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
