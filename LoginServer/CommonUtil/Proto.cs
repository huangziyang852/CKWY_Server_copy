
namespace Hzy.Common.Proto
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }

        public ErrorCode ErrorCode { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    public class RegisterUserRequest
    {
        public required string Account { get; set; }
        public required string Password { get; set; }
        public required string Sign { get; set; }
    }

    public enum ErrorCode
    {
        NONE = 1,
        BadRegistRequest = 10001,
        BadLoginRequest = 10002,
        WrongLoginToken = 10003,
        SystemError = 10004,
        TokenOutOfTime = 10005,
    }
}