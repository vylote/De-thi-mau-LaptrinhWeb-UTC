using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HoTen_MaSV.Models;
using Microsoft.EntityFrameworkCore;
using HoTen_MaSV.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

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
        return _context.HangHoas;
    }

    private List<SelectListItem> GetPriceFilterOptions(string currentValue = "all")
    {
        return new List<SelectListItem>
        {
            new SelectListItem { Value = "all", Text = "Tất cả giá", Selected = currentValue == "all" },
            new SelectListItem { Value = "gt100", Text = "Giá > $100", Selected = currentValue == "gt100" },
            new SelectListItem { Value = "eq100", Text = "Giá = $100", Selected = currentValue == "eq100" },
            new SelectListItem { Value = "lt100", Text = "Giá < $100", Selected = currentValue == "lt100" }
        };
    }

    private IQueryable<HangHoa> ApplyPriceFilter(IQueryable<HangHoa> query, string filter)
    {
        switch (filter)
        {
            case "gt100":
                // Giá > 100
                query = query.Where(h => h.Gia > 100);
                break;
            case "eq100":
                // Giá = 100
                query = query.Where(h => h.Gia == 100);
                break;
            case "lt100":
                // Giá < 100
                query = query.Where(h => h.Gia < 100);
                break;
            case "all":
            default:
                // Theo yêu cầu đề bài: Gia >= 100 (bao gồm cả 100)
                query = query.Where(h => h.Gia >= 100);
                break;
        }
        return query;
    }

    // Tối ưu hóa: Gọi hàm chung để lấy query cơ sở
    public async Task<IActionResult> Index(int page = 1, string priceFilter = "all")
    {
        var allProducts = ApplyPriceFilter(GetBaseProductQuery(), priceFilter);

        // Tối ưu: Tính toán và lấy dữ liệu trong 1 khối
        int totalItems = await allProducts.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

        var hangHoa = await allProducts
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        
        ViewBag.PriceFilterOptions = GetPriceFilterOptions(priceFilter);
        ViewBag.CurrentPriceFilter = priceFilter;
        return View(hangHoa);
    }

    [HttpGet]
    // Tối ưu hóa: Dùng IQueryable để xây dựng truy vấn trước khi gọi DB
    public async Task<IActionResult> GetHangHoaByLoai(int? maLoai, string search = "", string priceFilter="all", int page = 1)
    {
        var productsQuery = ApplyPriceFilter(GetBaseProductQuery(), priceFilter);

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

        var priceFilters = new List<SelectListItem>
        {
            new SelectListItem { Value = "all", Text = "Tất cả giá", Selected = true },
            new SelectListItem { Value = "gt100", Text = "Giá > $100" },
            new SelectListItem { Value = "eq100", Text = "Giá = $100" },
            new SelectListItem { Value = "lt100", Text = "Giá < $100" }
        };

        ViewBag.PriceFilterOptions = GetPriceFilterOptions(priceFilter);
        ViewBag.CurrentPriceFilter = priceFilter;
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
    [HttpGet]
    public async Task<IActionResult> Delete(int? id) // Cần nhận tham số ID
    {
        if (id == null)
        {
            return NotFound();
        }

        var hangHoa = await _context.HangHoas
            .FirstOrDefaultAsync(m => m.MaHang == id);

        if (hangHoa == null)
        {
            return NotFound();
        }

        // Tải thông tin Loại Hàng nếu cần hiển thị trong View (MaLoaiNavigation)
        // Nếu bạn muốn hiển thị TenLoai, bạn cần Include nó ở đây
        // var hangHoa = await _context.HangHoas.Include(h => h.MaLoaiNavigation)...

        return View(hangHoa); // Trả về View xác nhận Delete.cshtml
    }

    // 2. ACTION POST: Thực hiện xóa sau khi xác nhận
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int maHang) // Nhận ID từ input hidden
    {
        var hangHoa = await _context.HangHoas.FindAsync(maHang);

        if (hangHoa != null)
        {
            _context.HangHoas.Remove(hangHoa);
            await _context.SaveChangesAsync();
        }

        // Tối ưu: Nếu muốn giữ lại trang hiện tại (lọc/phân trang), bạn cần giữ lại tham số
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var hangHoa = await _context.HangHoas
            .FirstOrDefaultAsync(m => m.MaHang == id);

        if (hangHoa == null)
        {
            return NotFound();
        }

        ViewBag.MaLoai = new SelectList(await _context.LoaiHangs.ToListAsync(), "MaLoai", "TenLoai");
        return View(hangHoa);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // 💥 SỬA LỖI QUAN TRỌNG:
    // 1. Dùng [Bind] để Model Binder tự động ánh xạ dữ liệu form gửi lên vào đối tượng hangHoa.
    // 2. Không cần truy cập Request.Form thủ công nữa.
    public async Task<IActionResult> Edit(int id, [Bind("MaHang,MaLoai,TenHang,Gia,Anh")] HangHoa hangHoa)
    {
        // Kiểm tra ID có khớp không (Thường là kiểm tra đầu tiên)
        if (id != hangHoa.MaHang)
        {
            return NotFound();
        }

        // 💥 GIỮ NGUYÊN: Giải pháp khắc phục lỗi Model Binding
        ModelState.Remove("MaLoaiNavigation");

        // Validation tự động sẽ chạy dựa trên Data Annotations trong Models/HangHoa.cs
        if (ModelState.IsValid)
        {
            try
            {
                // Update đối tượng hangHoa đã được ánh xạ từ form
                _context.Update(hangHoa);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Logic kiểm tra lỗi đồng thời
                if (!_context.HangHoas.Any(e => e.MaHang == hangHoa.MaHang))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // 💥 BỔ SUNG: Nếu validation thất bại, cần cung cấp lại ViewBag cho Dropdown
        ViewBag.MaLoai = new SelectList(await _context.LoaiHangs.ToListAsync(), "MaLoai", "TenLoai", hangHoa.MaLoai);

        // Trả về View với dữ liệu lỗi để hiển thị validation message
        return View(hangHoa);
    }
}