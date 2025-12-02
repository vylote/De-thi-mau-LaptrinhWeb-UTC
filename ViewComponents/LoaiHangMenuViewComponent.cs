
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HoTen_MaSV.Data;

namespace HoTen_MaSV.ViewComponents
{
    public class LoaiHangMenuViewComponent : ViewComponent
    {
        private readonly ProductDbContext _context;

        public LoaiHangMenuViewComponent(ProductDbContext context) => _context = context;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var loaiHang = await _context.LoaiHangs.ToListAsync();
            return View(loaiHang);
        }
    }
}