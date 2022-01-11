using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            this._db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> objList = _db.Product;
            foreach (var obj in objList)
            {
                obj.Category = _db.Category.FirstOrDefault(x => x.Id == obj.CategoryId);
            }

            return View(objList);
        }

        //GET - UPSERT
        public async Task<IActionResult> Upsert(int? id)
        {

            //IEnumerable<SelectListItem> CategoryDropDown = _db.Category.Select(x => new SelectListItem
            //{
            //    Text = x.Name,
            //    Value = x.Id.ToString()
            //});
            ////ViewBag.CategoryDropDown = CategoryDropDown;
            //ViewData["CategoryDropDown"] = CategoryDropDown;
            //  Product product = new Product();

            ProductVM viewModel = new ProductVM
            {
                Product = new Product(),
                CategorySelectList = GetCategorySelectList()
            };

            if (id.HasValue)
            {
                viewModel.Product = await _db.Product.FindAsync(id);
                if (viewModel.Product == null)
                {
                    return NotFound();
                }
            }
            return View(viewModel);
        }

        //POST - UPSERT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;
                string upload = webRootPath + WC.ImagePath;
                string fileName = Guid.NewGuid().ToString();
                if(files.Count > 0)
                    fileName += Path.GetExtension(files[0].FileName);
                if (productVM.Product.Id <= 0)
                {
                    using (var fileStream = new FileStream(Path.Combine(upload, fileName), FileMode.Create))
                    {
                        await files[0].CopyToAsync(fileStream);
                    }

                    productVM.Product.Image = fileName;
                    _db.Product.Add(productVM.Product);
                    
                }
                else
                {
                    var objFromDB = _db.Product.AsNoTracking().FirstOrDefault(x => x.Id == productVM.Product.Id);
                    if(objFromDB == null)
                    {
                        return BadRequest();
                    }

                    if (files.Count > 0)
                    {
                        var oldFile = Path.Combine(upload, objFromDB.Image);
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }
                        using (var fileStream = new FileStream(Path.Combine(upload, fileName), FileMode.Create))
                        {
                            await files[0].CopyToAsync(fileStream);
                        }                        
                    }
                    else
                    {
                        fileName = objFromDB.Image;
                    }
                    productVM.Product.Image = fileName;
                    _db.Product.Update(productVM.Product);
                }

                await _db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            productVM.CategorySelectList = GetCategorySelectList();//To keep from the dropdown being empty if ModelState is not valid.
            return View(productVM);

        }

        private IEnumerable<SelectListItem> GetCategorySelectList()
        {
            return _db.Category.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _db.Product.Include(x=> x.Category).FirstOrDefault(x=> x.Id == id);
            if (obj == null)
            {
                return NotFound();
            }          

            return View(obj);
        }

        //POST - DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task< IActionResult> DeletePost(int? id)
        {
            var obj = await _db.Product.FindAsync(id);
            if (obj != null)
            {
                string webRootPath = _webHostEnvironment.WebRootPath;
                string upload = webRootPath + WC.ImagePath;
                var oldFile = Path.Combine(upload, obj.Image);
                if (System.IO.File.Exists(oldFile))
                {
                    System.IO.File.Delete(oldFile);
                }
                _db.Product.Remove(obj);
                await _db.SaveChangesAsync();


            }

            return RedirectToAction("Index");

        }


    }
}
