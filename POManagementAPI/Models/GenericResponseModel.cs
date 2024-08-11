namespace POManagementAPI.Models
{
    public class GenericResponseModel<T>:GeneralResponseModel
    {
        public GenericResponseModel(T data):base()
        {
            this.Data = data;
        }
        public T Data { get; set; }
    }
}
