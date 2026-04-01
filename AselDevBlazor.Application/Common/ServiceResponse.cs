using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AselDevBlazor.Application.Common
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }


        public ServiceResponse() { }

        public ServiceResponse(string message = "") {
            Success = false;
            Data = default;
            Message = message;
            StatusCode = 200;
        }
        public ServiceResponse(T data, string message = "", int statusCode = 200)
        {
            Success = true;
            Data = data;
            Message = message;
            StatusCode = statusCode;
        }



        public ServiceResponse(string message, int statusCode)
        {
            Success = false;
            Data = default;
            Message = message;
            StatusCode = statusCode;
        }
    }

    // Non-generic version for void operations (Delete, Update)
    public class ServiceResponse : ServiceResponse<object>
    {
        public static ServiceResponse Ok(string message = "Success")
            => new() { Success = true, Message = message, StatusCode = 200 };

        public static ServiceResponse NotFound(string message = "Record not found")
            => new() { Success = false, Message = message, StatusCode = 404 };

        public static ServiceResponse ServerError(string message = "An unexpected error occurred")
            => new() { Success = false, Message = message, StatusCode = 500 };

        public static ServiceResponse Error(string message = "Something went wrong")
           => new() { Success = false, Message = message, StatusCode = 200 };

    }
}
