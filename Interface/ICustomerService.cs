namespace DotnetAuthAndFileHandling.Interface
{
    public interface ICustomerService
    {
        Task<List<Customermodal>> Getall();
        Task<Customermodal> Getbyid(long id);
        Task<string> Remove(long id);
        Task<string> Create(Customermodal data);

        Task<string> Update(Customermodal data, long id);
    }
}
