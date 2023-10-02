using DotnetAuthAndFileHandling.Interface;

namespace DotnetAuthAndFileHandling.Service
{
    public class CustomerService : ICustomerService
    {
        private readonly DataContext context;

        public CustomerService(DataContext context)
        {
            this.context = context;
        }

        public async Task<string> Create(Customermodal data)
        {
            string response;
            try
            {
                await this.context.Customers.AddAsync(data);
                //if(data.IsActive == null)
                //{
                //    data.IsActive = true;
                //}
                await this.context.SaveChangesAsync();
                response = "Added";
            }
            catch (Exception ex)
            {
                response = $"something went wrong! {ex.Message}";
            }
            return response;
        }

        public async Task<List<Customermodal>> Getall()
        {
            var _data = await this.context.Customers.ToListAsync();
            if (_data == null)
                return _data = null;

            return _data;
        }

        public async Task<Customermodal> Getbyid(long id)
        {
            var _data = await this.context.Customers.FindAsync(id);
            if (_data == null)
                return _data = null;
            return _data;
        }


        public async Task<string> Remove(long id)
        {
            string response;
            try
            {
                var _customer = await this.context.Customers.FindAsync(id);
                if (_customer != null)
                {
                    this.context.Customers.Remove(_customer);
                    await this.context.SaveChangesAsync();
                    response = "Removed";
                }
                else
                {
                    response = "Data not found";
                }

            }
            catch (Exception ex)
            {
                response = ex.Message;
            }
            return response;
        }

        public async Task<string> Update(Customermodal data, long id)
        {
            string response;
            try
            {
                var _customer = await this.context.Customers.FindAsync(id);
                if (_customer != null)
                {
                    _customer.Name = data.Name;
                    _customer.Email = data.Email;
                    _customer.Phone = data.Phone;
                    _customer.IsActive = data.IsActive;
                    await this.context.SaveChangesAsync();
                    response = "Updated";
                }
                else
                {
                    response = "Data not found";
                }

            }
            catch (Exception ex)
            {
                response = ex.Message;
            }
            return response;
        }
    }
}
