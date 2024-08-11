namespace POManagementAPI.Models
{
    public enum GeneralResponseStatus
    {
        SUCCESS,
        FAILED,
        ERROR
    }
    public class GeneralResponseModel
    {
        public GeneralResponseModel()
        {
            Status = GeneralResponseStatus.FAILED;
            Message = "Not processed";
        }
        public GeneralResponseStatus Status { get; set; }
        public string Message { get; set; }
    }
}
