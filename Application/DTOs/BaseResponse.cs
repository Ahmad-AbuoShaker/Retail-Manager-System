namespace Application.DTOs
{
    public class BaseResponse<T>
    {
        public BaseResponse()
        {
            Errors = new List<string>();
        }
        public BaseResponse(T data, string message = null)
        {
            Message = message;
            Data = data;
            Errors = new List<string>();
        }
        public BaseResponse(string message)
        {
            Message = message;
            Errors = new List<string>();
        }
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
        public T Data { get; set; }
        public Application.Enums.ErrorCode? ErrorCode { get; set; }
    }
}