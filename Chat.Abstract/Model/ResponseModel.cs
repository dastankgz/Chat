namespace Chat.Abstract.Model
{
    public class ResponseModel<T>
    {
        public bool Success { get; set; }
        public T Body { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }

        public static ResponseModel<T> Fail(string code, string desc)
        {
            var request = new ResponseModel<T>
            {
                Success = false,
                ErrorCode = code,
                ErrorDesc = desc,
                Body = default(T)
            };
            return request;
        }

        public static ResponseModel<T> Fail()
        {
            return Fail(null, null);
        }

        public static ResponseModel<T> Ok(T response)
        {
            var request = new ResponseModel<T>
            {
                Success = true,
                Body = response
            };
            return request;
        }

        public override string ToString()
        {
            if (Success)
                return Body.ToString();

            return $"[Success: {Success}; ErrorCode: {ErrorCode}; ErrorDesc: {ErrorDesc}]";
        }
    }
}