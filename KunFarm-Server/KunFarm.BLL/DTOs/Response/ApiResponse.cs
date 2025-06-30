namespace KunFarm.BLL.DTOs.Response
{
    public class ApiResponse<T>
    {
        /// <summary>
        /// HTTP status code
        /// </summary>
        /// <example>200</example>
        public int Code { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        /// <example>Success</example>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Response data
        /// </summary>
        public T? Data { get; set; }

        public static ApiResponse<T> Success(T data, string message = "Success", int code = 200)
        {
            return new ApiResponse<T>
            {
                Code = code,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Error(string message, int code = 500, T? data = default)
        {
            return new ApiResponse<T>
            {
                Code = code,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Failure(string message, int code = 400, T? data = default)
        {
            return new ApiResponse<T>
            {
                Code = code,
                Message = message,
                Data = data
            };
        }
    }

    // Non-generic version for responses without data
    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Success(string message = "Success", int code = 200)
        {
            return new ApiResponse
            {
                Code = code,
                Message = message,
                Data = null
            };
        }

        public static new ApiResponse Error(string message, int code = 500)
        {
            return new ApiResponse
            {
                Code = code,
                Message = message,
                Data = null
            };
        }
    }
} 