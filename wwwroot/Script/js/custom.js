$(document).ready(function () {

    // =======================================================================
    // I. KHAI BÁO CỐ ĐỊNH & HÀM TIỆN ÍCH (Utilities)
    // =======================================================================

    const DEFAULT_SEARCH_VALUE = 'Search';
    const SEARCH_INPUT = $('#searchInput');
    const SEARCH_BUTTON = $('#searchButton');

    /**
     * @description Lấy giá trị tìm kiếm hiện tại từ input, xử lý giá trị mặc định.
     * @returns {string} Chuỗi tìm kiếm đã được làm sạch, hoặc chuỗi rỗng.
     */
    const getSearchQuery = () => {
        const val = SEARCH_INPUT.val();
        // Trả về rỗng nếu giá trị là 'Search' hoặc null, nếu không trả về giá trị đã trim.
        return (val === DEFAULT_SEARCH_VALUE || !val) ? '' : val.trim();
    };

    /**
     * @description Lấy Mã Loại (MaLoai) đang được chọn trên thanh menu (NAV).
     * @returns {number | null} Mã loại hiện tại hoặc null nếu đang ở trang chủ/không chọn gì.
     */
    const getCurrentMaLoai = () => {
        // Tìm thẻ <a> có class 'filter-loaihang' nằm trong thẻ <li> có class 'active'.
        const activeItem = $('.menu ul li.active a.filter-loaihang');
        return activeItem.length ? activeItem.data('maloai') : null;
    };


    // =======================================================================
    // II. HÀM CHUNG GỌI AJAX (CORE FUNCTION)
    // =======================================================================

    /**
     * @description Thực hiện gọi AJAX để tải danh sách sản phẩm đã lọc/tìm kiếm/phân trang.
     * @param {number | null} maLoai - Mã loại hàng để lọc.
     * @param {string} search - Từ khóa tìm kiếm.
     * @param {number} [page=1] - Trang hiện tại cần tải.
     */
    function loadProducts(maLoai, search, page = 1) {

        // Đảm bảo maLoai là null nếu không có giá trị (phù hợp với tham số Controller)
        const maLoaiClean = maLoai || null;

        $.ajax({
            url: '/Home/GetHangHoaByLoai', // Action xử lý lọc và phân trang
            type: 'GET',
            data: {
                maLoai: maLoaiClean,
                search: search,
                page: page
            },
            success: function (data) {
                // Thay thế nội dung chính (Partial View)
                $('#main-content-ajax').html(data);

                // LƯU Ý: Trạng thái Active của menu phải được xử lý trước khi gọi loadProducts
                // hoặc sau khi AJAX thành công nếu cần thay đổi ngoài #main-content-ajax.
            },
            error: function (xhr, status, error) {
                console.error("AJAX Error: " + status + error);
                alert("Có lỗi xảy ra khi tải dữ liệu.");
            }
        });
    }

    // =======================================================================
    // III. XỬ LÝ SỰ KIỆN LỌC & TÌM KIẾM
    // =======================================================================

    /**
     * @description Xử lý logic Tìm kiếm (cả click nút và nhấn Enter).
     * @param {Event} e - Đối tượng sự kiện.
     */
    const handleSearch = (e) => {
        e.preventDefault();

        const search = getSearchQuery();
        const maLoai = getCurrentMaLoai(); // Duy trì trạng thái lọc hiện tại

        // Load sản phẩm (luôn reset về trang 1 khi thực hiện tìm kiếm/lọc mới)
        loadProducts(maLoai, search, 1);
    };

    // 2.1. Bắt sự kiện click nút tìm kiếm
    SEARCH_BUTTON.on('click', handleSearch);

    // 2.2. Bắt sự kiện nhấn ENTER trong input tìm kiếm
    SEARCH_INPUT.on('keypress', function (e) {
        if (e.which === 13) { // KeyCode 13 là Enter
            handleSearch(e); // Gọi hàm xử lý tìm kiếm
        }
    });

    // 1. Logic Lọc theo Loại Hàng (Ủy quyền sự kiện cho các link menu động)
    $(document).on('click', '.filter-loaihang', function (e) {
        e.preventDefault();

        // 💥 Cập nhật trạng thái Active trên Menu (Đảm bảo chỉ có 1 mục active)
        $('.menu ul li').removeClass('active');
        $(this).closest('li').addClass('active');

        const maLoai = $(this).data('maloai');
        const search = getSearchQuery();

        // Load sản phẩm (luôn bắt đầu từ trang 1 khi lọc mới)
        loadProducts(maLoai, search, 1);
    });


    // =======================================================================
    // IV. XỬ LÝ PHÂN TRANG (PAGINATION)
    // =======================================================================

    // 3. Logic Phân Trang (Ủy quyền sự kiện cho các nút phân trang động)
    $(document).on('click', '.page-link', function (e) {
        e.preventDefault();

        // Lấy các tham số lọc/tìm kiếm hiện tại từ data của nút phân trang
        const page = $(this).data('page');
        const maLoai = $(this).data('maloai');
        const search = $(this).data('search');

        // Load sản phẩm với trang mới
        loadProducts(maLoai, search, page);

        // LƯU Ý: Trạng thái Active của nút phân trang được xử lý trong Razor 
        // (_ProductListPartial.cshtml) sau khi AJAX thành công.
    });
});