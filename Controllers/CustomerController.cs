using ClosedXML.Excel;
using DotnetAuthAndFileHandling.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace DotnetAuthAndFileHandling.Controllers
{
    [AllowAnonymous]

    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService service;
        private readonly IWebHostEnvironment environment;
        private readonly DataContext context;

        public CustomerController(ICustomerService service, IWebHostEnvironment environment, DataContext _context)
        {
            this.service = service;
            this.environment = environment;
            this.context = _context;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var data = await this.service.Getall();
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }


        [HttpGet("GetbyCustomerId")]
        public async Task<IActionResult> GetbyCustomerId(long id)
        {
            var data = await this.service.Getbyid(id);
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(Customermodal _data)
        {
            var data = await this.service.Create(_data);
            return Ok(_data);
        }
        [HttpPut("Update")]
        public async Task<IActionResult> Update(Customermodal _data, long id)
        {
            var data = await this.service.Update(_data, id);
            return Ok(_data);
        }

        [HttpDelete("Remove")]
        public async Task<IActionResult> Remove(long id)
        {
            var data = await this.service.Remove(id);
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpGet("Exportexcel")]
        public async Task<IActionResult> Exportexcel()
        {
            try
            {
                string Filepath = GetFilepath();
                string excelpath = Filepath + "\\customerinfo.xlsx";
                DataTable dt = new DataTable();
                dt.Columns.Add("Id", typeof(long));
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("Email", typeof(string));
                dt.Columns.Add("Phone", typeof(string));
                dt.Columns.Add("ActiveStatus", typeof(bool));
                var data = await this.service.Getall();
                if (data != null && data.Count > 0)
                {
                    data.ForEach(item =>
                    {
                        dt.Rows.Add(item.Id, item.Name, item.Email, item.Phone, item.IsActive);
                    });
                }
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.AddWorksheet(dt, "Customer Info");
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);

                        if (System.IO.File.Exists(excelpath))
                        {
                            System.IO.File.Delete(excelpath);
                        }
                        wb.SaveAs(excelpath);

                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Customer.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [NonAction]
        private string GetFilepath()
        {
            return this.environment.WebRootPath + "\\Export";
        }

        // another
        [HttpPut("UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile formFile, long customerId)
        {
            string response;
            try
            {
                string Filepath = Getfilepath(customerId);
                if (!System.IO.Directory.Exists(Filepath))
                {
                    System.IO.Directory.CreateDirectory(Filepath);
                }

                string imagepath = Filepath + "\\" + customerId.ToString() + ".png";
                if (System.IO.File.Exists(imagepath))
                {
                    System.IO.File.Delete(imagepath);
                }
                using (FileStream stream = System.IO.File.Create(imagepath))
                {
                    await formFile.CopyToAsync(stream);
                    response = "Image Upoaded";
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }
            return Ok(response);
        }

        [HttpPut("MultiUploadImage")]
        public async Task<IActionResult> MultiUploadImage(IFormFileCollection filecollection, long customerId)
        {
            string response;
            int passcount = 0; int errorcount = 0;
            try
            {
                string Filepath = Getfilepath(customerId);
                if (!System.IO.Directory.Exists(Filepath))
                {
                    System.IO.Directory.CreateDirectory(Filepath);
                }
                foreach (var file in filecollection)
                {
                    string imagepath = Filepath + "\\" + file.FileName;
                    if (System.IO.File.Exists(imagepath))
                    {
                        System.IO.File.Delete(imagepath);
                    }
                    using (FileStream stream = System.IO.File.Create(imagepath))
                    {
                        await file.CopyToAsync(stream);
                        passcount++;
                    }
                }


            }
            catch (Exception ex)
            {
                errorcount++;
                response = ex.Message;
            }
            response = passcount + " Files uploaded &" + errorcount + " files failed";
            return Ok(response);
        }

        [HttpGet("GetImage")]
        public async Task<IActionResult> GetImage(long customerId)
        {
            string Imageurl = string.Empty;
            string hosturl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string Filepath = Getfilepath(customerId);
                string imagepath = Filepath + "\\" + customerId.ToString() + ".png";
                if (System.IO.File.Exists(imagepath))
                {
                    Imageurl = hosturl + "/Upload/Images/" + customerId.ToString() + "/" + customerId.ToString() + ".png";
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
            }
            return Ok(Imageurl);

        }

        [HttpGet("GetMultiImage")]
        public async Task<IActionResult> GetMultiImage(long customerId)
        {
            List<string> Imageurl = new List<string>();
            string hosturl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string Filepath = Getfilepath(customerId);

                if (System.IO.Directory.Exists(Filepath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Filepath);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        string filename = fileInfo.Name;
                        string imagepath = Filepath + "\\" + filename;
                        if (System.IO.File.Exists(imagepath))
                        {
                            string _Imageurl = hosturl + "/Upload/Images/" + customerId.ToString() + "/" + filename;
                            Imageurl.Add(_Imageurl);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
            }
            return Ok(Imageurl);

        }

        [HttpGet("download")]
        public async Task<IActionResult> download(long customerId)
        {
            try
            {
                string Filepath = Getfilepath(customerId);
                string imagepath = Filepath + "\\" + customerId.ToString() + ".png";
                if (System.IO.File.Exists(imagepath))
                {
                    MemoryStream stream = new MemoryStream();
                    using (FileStream fileStream = new FileStream(imagepath, FileMode.Open))
                    {
                        await fileStream.CopyToAsync(stream);
                    }
                    stream.Position = 0;
                    return File(stream, "image/png", customerId.ToString() + ".png");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }


        }

        [HttpGet("remove")]
        public async Task<IActionResult> remove(long customerId)
        {
            try
            {
                string Filepath = Getfilepath(customerId);
                string imagepath = Filepath + "\\" + customerId.ToString() + ".png";
                if (System.IO.File.Exists(imagepath))
                {
                    System.IO.File.Delete(imagepath);
                    return Ok("pass");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }


        }

        [HttpGet("multiremove")]
        public async Task<IActionResult> multiremove(long customerId)
        {
            try
            {
                string Filepath = Getfilepath(customerId);
                if (System.IO.Directory.Exists(Filepath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Filepath);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        fileInfo.Delete();
                    }
                    return Ok("Deleted");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }


        }

        //[HttpPut("DBMultiUploadImage")]
        //public async Task<IActionResult> DBMultiUploadImage(IFormFileCollection filecollection, long customerId)
        //{
        //    string response;
        //    int passcount = 0; int errorcount = 0;
        //    try
        //    {
        //        foreach (var file in filecollection)
        //        {
        //            using (MemoryStream stream = new MemoryStream())
        //            {
        //                await file.CopyToAsync(stream);
        //                this.context.Images.Add(new dbImage()
        //                {
        //                    CustomerId = customerId,
        //                    Customerimage = stream.ToArray()
        //                });
        //                await this.context.SaveChangesAsync();
        //                passcount++;
        //            }
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        errorcount++;
        //        response = ex.Message;
        //    }
        //    response = passcount + " Files uploaded &" + errorcount + " files failed";
        //    return Ok(response);
        //}


        //[HttpGet("GetDBMultiImage")]
        //public async Task<IActionResult> GetDBMultiImage(long customerId)
        //{
        //    List<string> Imageurl = new List<string>();
        //    try
        //    {
        //        var _productimage = this.context.Images.Where(item => item.CustomerId == customerId).ToList();
        //        if (_productimage != null && _productimage.Count > 0)
        //        {
        //            _productimage.ForEach(item =>
        //            {
        //                Imageurl.Add(Convert.ToBase64String(item.Customerimage));
        //            });
        //        }
        //        else
        //        {
        //            return NotFound();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //    return Ok(Imageurl);

        //}


        //[HttpGet("dbdownload")]
        //public async Task<IActionResult> dbdownload(long customerId)
        //{
        //    try
        //    {
        //        var _productimage = await this.context.Images.FirstOrDefaultAsync(item => item.CustomerId == customerId);
        //        if (_productimage != null)
        //        {
        //            return File(_productimage.Customerimage, "image/png", customerId.ToString() + ".png");
        //        }
        //        else
        //        {
        //            return NotFound();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return NotFound();
        //    }


        //}

        [NonAction]
        private string Getfilepath(long CustomerId)
        {
            return this.environment.WebRootPath + "\\Upload\\Images\\" + CustomerId.ToString();

        }
    }

}