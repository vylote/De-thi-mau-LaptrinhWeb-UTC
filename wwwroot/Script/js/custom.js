$(document).ready(function () {

    // Khai báo hằng số cho giá trị mặc định của ô tìm kiếm
    const DEFAULT_SEARCH_VALUE = 'Search';
    const SEARCH_INPUT = $('#searchInput');
    const SEARCH_BUTTON = $('#searchButton');

    // Hàm lấy giá trị tìm kiếm an toàn
    const getSearchQuery = () => {
        // Trả về chuỗi rỗng nếu giá trị là DEFAULT_SEARCH_VALUE, nếu không trả về giá trị đã trim.
        const val = SEARCH_INPUT.val();
        return (val === DEFAULT_SEARCH_VALUE || !val) ? '' : val.trim();
    };

    // Hàm lấy Mã Loại đang được chọn (Lấy từ item đang có class 'active')
    const getCurrentMaLoai = () => {
        const activeItem = $('.menu ul li.active a.filter-loaihang');
        return activeItem.length ? activeItem.data('maloai') : null;
    };


    // 🛠️ HÀM CHUNG GỌI AJAX (Load Sản phẩm)
    function loadProducts(maLoai, search, page = 1) {

        // Đảm bảo maLoai là null nếu nó là undefined/0/''
        const maLoaiClean = maLoai || null;

        $.ajax({
            url: '/Home/GetHangHoaByLoai',
            type: 'GET',
            data: {
                maLoai: maLoaiClean,
                search: search,
                page: page
            },
            success: function (data) {
                $('#main-content-ajax').html(data);
            },
            error: function (xhr, status, error) {
                console.error("AJAX Error: " + status + error);
                // Có thể sử dụng một alert/modal thân thiện hơn ở môi trường production
                alert("Có lỗi xảy ra khi tải dữ liệu.");
            }
        });
    }

    // --- 1. Logic Lọc theo Loại Hàng (Dùng ủy quyền sự kiện) ---
    $(document).on('click', '.filter-loaihang', function (e) {
        e.preventDefault();

        // Cập nhật active class cho menu
        $('.menu ul li').removeClass('active');
        $(this).closest('li').addClass('active');

        const maLoai = $(this).data('maloai');
        const search = getSearchQuery(); // Lấy giá trị tìm kiếm hiện tại

        // Load sản phẩm (luôn bắt đầu từ trang 1 khi lọc mới)
        loadProducts(maLoai, search, 1);
    });

    // --- 2. Logic Tìm Kiếm (Xử lý Click và Enter) ---
    const handleSearch = (e) => {
        e.preventDefault();
        const search = getSearchQuery();
        const maLoai = getCurrentMaLoai();

        // Load sản phẩm (luôn bắt đầu từ trang 1 khi tìm kiếm mới)
        loadProducts(maLoai, search, 1);
    };

    // Bắt sự kiện click nút tìm kiếm
    SEARCH_BUTTON.on('click', handleSearch);

    // Bắt sự kiện nhấn ENTER trong input tìm kiếm
    SEARCH_INPUT.on('keypress', function (e) {
        if (e.which === 13) {
            handleSearch(e); // Gọi hàm xử lý tìm kiếm
        }
    });

    // --- 3. Logic Phân Trang (Dùng ủy quyền sự kiện) ---
    $(document).on('click', '.page-link', function (e) {
        e.preventDefault();

        // Lấy các tham số từ thuộc tính data của nút phân trang
        const page = $(this).data('page');
        const maLoai = $(this).data('maloai');
        const search = $(this).data('search');

        // Load sản phẩm với tham số hiện tại
        loadProducts(maLoai, search, page);
    });
});