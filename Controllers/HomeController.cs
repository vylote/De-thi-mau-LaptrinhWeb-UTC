using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HoTen_MaSV.Models;
using Microsoft.EntityFrameworkCore;
using HoTen_MaSV.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HoTen_MaSV.Controllers;

public class HomeController : Controller
{
    private readonly ProductDbContext _context;
    private const int PageSize = 4;

    public HomeController(ProductDbContext context)
    {
        _context = context;
    }

    // Phương thức chung để xử lý truy vấn sản phẩm (tránh lặp lại code)
    private IQueryable<HangHoa> GetBaseProductQuery()
    {
        return _context.HangHoas.Where(h => h.Gia > 100);
    }

    // Tối ưu hóa: Gọi hàm chung để lấy query cơ sở
    public async Task<IActionResult> Index(int page = 1)
    {
        var allProducts = GetBaseProductQuery();

        // Tối ưu: Tính toán và lấy dữ liệu trong 1 khối
        int totalItems = await allProducts.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

        var hangHoa = await allProducts
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.IsSearchOrFilter = false;

        return View(hangHoa);
    }

    [HttpGet]
    // Tối ưu hóa: Dùng IQueryable để xây dựng truy vấn trước khi gọi DB
    public async Task<IActionResult> GetHangHoaByLoai(int? maLoai, string search = "", int page = 1)
    {
        var productsQuery = GetBaseProductQuery();

        // 1. Lọc theo Loại Hàng
        if (maLoai.HasValue)
        {
            productsQuery = productsQuery.Where(h => h.MaLoai == maLoai.Value);
        }

        // 2. Lọc theo Tìm kiếm
        if (!string.IsNullOrEmpty(search))
        {
            productsQuery = productsQuery.Where(h => h.TenHang.Contains(search));
        }

        // Tối ưu: Tính toán và lấy dữ liệu trong 1 khối
        int totalItems = await productsQuery.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

        var hangHoa = await productsQuery
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        // Truyền thông tin phân trang và filter/search state
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.CurrentMaLoai = maLoai;
        ViewBag.CurrentSearch = search;
        ViewBag.IsSearchOrFilter = true;

        return PartialView("_ProductListPartial", hangHoa);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Tối ưu hóa: Load LoaiHang và tạo SelectList trước
    public async Task<IActionResult> Create()
    {
        // Tối ưu: Dùng SelectList trực tiếp thay vì gán List và tạo SelectList sau
        ViewBag.MaLoai = new SelectList(await _context.LoaiHangs.ToListAsync(), "MaLoai", "TenLoai");
        return View();
    }

    // Action POST Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaLoai,TenHang,Gia,Anh")] HangHoa hangHoa)
    {
        // 💥 GIỮ NGUYÊN: Giải pháp khắc phục lỗi Model Binding đã thành công
        ModelState.Remove("MaLoaiNavigation");

        // Tối ưu: Loại bỏ khởi tạo biến HangHoa không cần thiết (đã được sửa logic)
        if (ModelState.IsValid)
        {
            _context.HangHoas.Add(hangHoa);
            await _context.SaveChangesAsync();
            // Tối ưu: Dùng tên Action rõ ràng (nameof(Index))
            return RedirectToAction(nameof(Index));
        }

        // Tối ưu: Trả về SelectList khi thất bại
        ViewBag.MaLoai = new SelectList(await _context.LoaiHangs.ToListAsync(), "MaLoai", "TenLoai", hangHoa.MaLoai);
        return View(hangHoa);
    }
}