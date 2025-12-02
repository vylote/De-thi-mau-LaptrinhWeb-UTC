using System;
using System.ComponentModel.DataAnnotations; // Cần thêm namespace này
using System.Collections.Generic;

namespace HoTen_MaSV.Models;

public partial class HangHoa
{
    // MaHang không cần validation vì nó là IDENTITY (tự tăng)
    public int MaHang { get; set; }

    [Required(ErrorMessage = "Mã loại hàng là bắt buộc.")]
    public int MaLoai { get; set; }

    [Required(ErrorMessage = "Tên hàng là bắt buộc.")]
    [StringLength(50)]
    public string TenHang { get; set; } = null!;

    [Required(ErrorMessage = "Giá là bắt buộc.")]
    [Range(100, 5000, ErrorMessage = "Giá trị phải nằm trong khoảng từ 100 đến 5000.")]
    public decimal? Gia { get; set; }

    [Required(ErrorMessage = "Tên file ảnh là bắt buộc.")]
    // Validation đuôi file: .jpg, .png, .gif, .tiff
    [RegularExpression(@"^.+\.(jpg|png|gif|tiff)$",
         ErrorMessage = "Tên file ảnh phải có đuôi .jpg, .png, .gif, hoặc .tiff")]
    public string? Anh { get; set; }

    public virtual LoaiHang MaLoaiNavigation { get; set; } = null!;
}