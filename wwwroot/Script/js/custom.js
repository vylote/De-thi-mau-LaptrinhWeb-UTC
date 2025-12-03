$(document).ready(function () {

    // =======================================================================
    // I. KHAI BÁO CỐ ĐỊNH & HÀM TIỆN ÍCH (Utilities)
    // =======================================================================

    const DEFAULT_SEARCH_VALUE = 'Search';
    const SEARCH_INPUT = $('#searchInput');
    const SEARCH_BUTTON = $('#searchButton');
    const PRICE_FILTER = $('#priceFilterDropdown'); // Dropdown lọc giá

    /**
     * @description Lấy giá trị lọc giá hiện tại từ dropdown.
     * @returns {string} Mã lọc giá ('all', 'gt100', etc.).
     */
    const getSelectedPriceRange = () => {
        // Lấy giá trị (value) của option đang được chọn.
        const selectedOption = PRICE_FILTER.val();
        // Trả về giá trị hoặc null (mặc dù giá trị mặc định nên là 'all')
        return selectedOption || null;
    }

    /**
     * @description Lấy giá trị tìm kiếm hiện tại từ input, xử lý giá trị mặc định.
     * @returns {string} Chuỗi tìm kiếm đã được làm sạch, hoặc chuỗi rỗng.
     */
    const getSearchQuery = () => {
        const val = SEARCH_INPUT.val();
        return (val === DEFAULT_SEARCH_VALUE || !val) ? '' : val.trim();
    };

    /**
     * @description Lấy Mã Loại (MaLoai) đang được chọn trên thanh menu (NAV).
     * @returns {number | null} Mã loại hiện tại hoặc null nếu không có mục loại hàng nào active.
     */
    const getCurrentMaLoai = () => {
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
     * @param {string} priceRange - Mã lọc giá ('all', 'gt100', etc.).
     */
    function loadProducts(maLoai, search, page = 1, priceRange = 'all') {

        const maLoaiClean = maLoai || null;

        $.ajax({
            url: '/Home/GetHangHoaByLoai', // Action xử lý lọc và phân trang
            type: 'GET',
            data: {
                maLoai: maLoaiClean,
                search: search,
                page: page,
                priceFilter: priceRange // 💥 ĐÃ SỬA: Dùng tên tham số Controller
            },
            success: function (data) {
                $('#main-content-ajax').html(data);
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
     * @description Hàm tổng hợp logic và tham số để kích hoạt loadProducts.
     */
    const handleFilterLoad = (page = 1) => {
        const search = getSearchQuery();
        const maLoai = getCurrentMaLoai();
        const priceRange = getSelectedPriceRange(); // Lấy giá trị lọc giá

        // Load sản phẩm với các tham số hiện tại (page=1 nếu là lọc mới)
        loadProducts(maLoai, search, page, priceRange);
    };

    // --- 1. Logic Lọc theo Loại Hàng (Ủy quyền sự kiện) ---
    $(document).on('click', '.filter-loaihang', function (e) {
        e.preventDefault();

        // Cập nhật trạng thái Active trên Menu
        $('.menu ul li').removeClass('active');
        $(this).closest('li').addClass('active');

        // Load sản phẩm (luôn bắt đầu từ trang 1 khi lọc mới)
        handleFilterLoad(1);
    });

    // --- 2. Logic Tìm Kiếm (Xử lý Click và Enter) ---
    // Bắt sự kiện click nút tìm kiếm
    SEARCH_BUTTON.on('click', handleFilterLoad);

    // Bắt sự kiện nhấn ENTER trong input tìm kiếm
    SEARCH_INPUT.on('keypress', function (e) {
        if (e.which === 13) { // KeyCode 13 là Enter
            e.preventDefault();
            handleFilterLoad(1); // Luôn bắt đầu từ trang 1
        }
    });

    // --- 3. Logic Lọc theo Giá (Bắt sự kiện change) ---
    // 💥 BỔ SUNG: Bắt sự kiện thay đổi giá trị của dropdown
    PRICE_FILTER.on('change', function () {
        handleFilterLoad(1); // Luôn bắt đầu từ trang 1 khi thay đổi bộ lọc
    });


    // =======================================================================
    // IV. XỬ LÝ PHÂN TRANG (PAGINATION)
    // =======================================================================

    // 4. Logic Phân Trang (Ủy quyền sự kiện cho các nút phân trang động)
    $(document).on('click', '.page-link', function (e) {
        e.preventDefault();

        // Lấy các tham số lọc/tìm kiếm được lưu trong data của nút phân trang
        const page = $(this).data('page');
        const maLoai = $(this).data('maloai');
        const search = $(this).data('search');
        // 💥 ĐÃ SỬA: Lấy priceRange từ data của nút phân trang
        const priceRange = $(this).data('pricerange');

        // Load sản phẩm với trang mới
        loadProducts(maLoai, search, page, priceRange);
    });
});