# Test Cases – Tester 2: Hoàng Phước Tất Khang

- **Nhiệm vụ**: Kiểm thử chức năng
- **Phạm vi**: Giao dịch (Transactions) + Tải lên (Upload)
- **Môi trường**: Web
- **Tổng số test case**: 50
- **Ngày test**: 2026-05-11
- **Kết quả**: ✅ 50/50 PASS

---

## MODULE: NẠP TIỀN (Deposit) – Web

```tsv
#	Dự án	Môi trường	Tên module	Chức năng	Task	Test Plan Start date	End date	Progress	Status	Note
1	Book Management	Web	Deposit	Function	Mở trang deposit.html khi đã đăng nhập → hiển thị số dư hiện tại chính xác	2026-05-11	2026-05-11	done	✅ PASS	Số dư hiển thị đúng (browser verified)
2	Book Management	Web	Deposit	Function	Nhấn nút mệnh giá 50.000đ → ô nhập tự động điền 50000	2026-05-11	2026-05-11	done	✅ PASS	Input tự động điền 50000 (browser verified)
3	Book Management	Web	Deposit	Function	Nhấn nút mệnh giá 100.000đ → ô nhập tự động điền 100000	2026-05-11	2026-05-11	done	✅ PASS	Input tự động điền 100000 (browser verified)
4	Book Management	Web	Deposit	Function	Nhấn nút mệnh giá 500.000đ → ô nhập tự động điền 500000	2026-05-11	2026-05-11	done	✅ PASS	Input tự động điền 500000 (browser verified)
5	Book Management	Web	Deposit	Function	Nhập số tiền 250000 tùy ý → nhấn Xác nhận → Toast "Nạp thành công 250.000 VNĐ vào ví!" → số dư cập nhật	2026-05-11	2026-05-11	done	✅ PASS	Toast hiển thị đúng, số dư cập nhật (browser verified)
6	Book Management	Web	Deposit	Function	Nạp tiền thành công → số dư trên trang cập nhật realtime (không cần F5)	2026-05-11	2026-05-11	done	✅ PASS	Số dư cập nhật không cần F5 (browser verified)
7	Book Management	Web	Deposit	Validation	Để trống ô nhập tiền → nhấn Xác nhận → Toast "Vui lòng nhập số tiền hợp lệ (tối thiểu 10.000đ)"	2026-05-11	2026-05-11	done	✅ PASS	Toast lỗi hiển thị đúng (browser verified)
8	Book Management	Web	Deposit	Validation	Nhập số tiền = 0 → nhấn Xác nhận → Toast lỗi tối thiểu 10.000đ	2026-05-11	2026-05-11	done	✅ PASS	Toast lỗi hiển thị (browser verified)
9	Book Management	Web	Deposit	Validation	Nhập số tiền = 5000 (< 10000) → nhấn Xác nhận → Toast lỗi tối thiểu 10.000đ	2026-05-11	2026-05-11	done	✅ PASS	Toast lỗi hiển thị (browser verified)
10	Book Management	Web	Deposit	Validation	Nhập số tiền âm (-50000) → nhấn Xác nhận → Toast lỗi	2026-05-11	2026-05-11	done	✅ PASS	Toast lỗi hiển thị (browser verified)
11	Book Management	Web	Deposit	Permission	Chưa đăng nhập truy cập /deposit.html → bị chuyển hướng về trang login	2026-05-11	2026-05-11	done	✅ PASS	Redirect tới /login.html?returnUrl=%2Fdeposit.html (browser verified)
12	Book Management	Web	Deposit	UI	Trong khi xử lý nạp tiền → nút Xác nhận bị disabled + hiển thị spinner "Đang xử lý giao dịch..."	2026-05-11	2026-05-11	done	✅ PASS	Nút disabled + text "Đang xử lý giao dịch..." (browser verified)
```

---

## MODULE: MUA SÁCH (Buy Book) – Web

```tsv
#	Dự án	Môi trường	Tên module	Chức năng	Task	Test Plan Start date	End date	Progress	Status	Note
13	Book Management	Web	Buy Book	Function	User đăng nhập + đủ số dư → nhấn "Mua ngay" → hiển thị modal xác nhận với tên sách + giá	2026-05-11	2026-05-11	done	✅ PASS	Modal.confirm hiển thị tên + giá (browser+code verified)
14	Book Management	Web	Buy Book	Function	Nhấn Xác nhận trên modal → Toast "Mua sách thành công! 🎉" → sách biến mất khỏi trang chủ	2026-05-11	2026-05-11	done	✅ PASS	Toast thành công + sách ẩn khỏi homepage (browser+code verified)
15	Book Management	Web	Buy Book	Function	Sau khi mua thành công → số dư ví giảm đúng bằng giá sách	2026-05-11	2026-05-11	done	✅ PASS	API: mua 50000 → balance 490000→440000 (API verified)
16	Book Management	Web	Buy Book	Function	Sau khi mua → sách xuất hiện trong tab "Sách đã mua" ở trang hồ sơ cá nhân	2026-05-11	2026-05-11	done	✅ PASS	loadPurchasedBooks() hiển thị sách với coverUrl+title (code+API verified)
17	Book Management	Web	Buy Book	Negative	User có số dư không đủ → nhấn Mua → Toast "Thanh toán thất bại, vui lòng kiểm tra số dư"	2026-05-11	2026-05-11	done	✅ PASS	Toast lỗi hiển thị (browser+code verified)
18	Book Management	Web	Buy Book	Negative	Mua sách đã được người khác mua (đã bán) → Toast lỗi "Sách này đã được bán"	2026-05-11	2026-05-11	done	✅ PASS	API 409: {"message":"Sách này đã được bán."} (API verified)
19	Book Management	Web	Buy Book	Negative	Mua sách có giá = 0 hoặc null → Toast "Sách chưa có giá hợp lệ để thanh toán"	2026-05-11	2026-05-11	done	✅ PASS	Server validate price>0.01; Frontend: Toast.error('Sách chưa có giá hợp lệ') (API+code verified)
20	Book Management	Web	Buy Book	Permission	Người dùng chưa đăng nhập nhấn "Mua ngay" → hiển thị modal yêu cầu đăng nhập	2026-05-11	2026-05-11	done	✅ PASS	Auth.promptLoginRequired() hiển thị modal (code verified)
21	Book Management	Web	Buy Book	Function	Nhấn Hủy trên modal xác nhận mua → không thực hiện giao dịch, quay lại trang chủ bình thường	2026-05-11	2026-05-11	done	✅ PASS	if(!confirmed) return; — không gọi API (browser+code verified)
22	Book Management	Web	Buy Book	UI	Sách đã mua hiển thị badge "✅ Đã mua" thay vì nút "Mua" trên trang chủ	2026-05-11	2026-05-11	done	✅ PASS	purchasedBookIds.has(b.id) → "✅ Đã mua" (code verified)
23	Book Management	Web	Buy Book	UI	Trong popup chi tiết sách → sách đã mua hiển thị "Sách của bạn" + badge "✅ Đã mua" thay nút Mua	2026-05-11	2026-05-11	done	✅ PASS	isOwned → "Sách của bạn" + "✅ Đã mua" (code verified)
```

---

## MODULE: LỊCH SỬ GIAO DỊCH & SÁCH ĐÃ MUA – Web

```tsv
#	Dự án	Môi trường	Tên module	Chức năng	Task	Test Plan Start date	End date	Progress	Status	Note
24	Book Management	Web	Purchased	Function	Mở trang user-profile.html → tab "Sách đã mua" → hiển thị danh sách sách đã mua (ảnh bìa + tiêu đề)	2026-05-11	2026-05-11	done	✅ PASS	Grid render coverUrl + title (code verified)
25	Book Management	Web	Purchased	Function	Chưa mua sách nào → tab "Sách đã mua" hiển thị "Bạn chưa mua cuốn sách nào" + nút "Khám phá sách ngay"	2026-05-11	2026-05-11	done	✅ PASS	Empty state: 🛒 + "Bạn chưa mua cuốn sách nào" + nút (code verified)
26	Book Management	Web	Purchased	Permission	Chưa đăng nhập → truy cập /user-profile.html → bị chuyển hướng về login	2026-05-11	2026-05-11	done	✅ PASS	Auth.requireLogin() redirect (code verified)
27	Book Management	Web	Transactions	Function	API GET /api/transactions/sold (public) → trả về danh sách ID sách đã bán → sách đã bán bị ẩn khỏi trang chủ	2026-05-11	2026-05-11	done	✅ PASS	Trả 6 items (API verified)
28	Book Management	Web	Transactions	Function	API GET /api/transactions/my → trả lịch sử giao dịch (nạp tiền + mua sách) của user hiện tại	2026-05-11	2026-05-11	done	✅ PASS	Trả 5 transactions (API verified)
29	Book Management	Web	Transactions	Permission	Admin truy cập API GET /api/transactions/all → trả toàn bộ giao dịch hệ thống	2026-05-11	2026-05-11	done	✅ PASS	Trả 32 transactions (API verified)
30	Book Management	Web	Transactions	Permission	User thường gọi API GET /api/transactions/all → bị chặn 403 Forbidden	2026-05-11	2026-05-11	done	✅ PASS	403 Forbidden (API verified)
31	Book Management	Web	Transactions	Permission	Không có JWT gọi API GET /api/transactions/purchased → bị chặn 401 Unauthorized	2026-05-11	2026-05-11	done	✅ PASS	401 Unauthorized (API verified)
32	Book Management	Web	Transactions	Validation	API POST /api/transactions/deposit với amount = 0 → trả ValidationProblem	2026-05-11	2026-05-11	done	✅ PASS	400 Bad Request (API verified)
33	Book Management	Web	Transactions	Validation	API POST /api/transactions/deposit với amount âm → trả ValidationProblem	2026-05-11	2026-05-11	done	✅ PASS	400 Bad Request (API verified)
34	Book Management	Web	Transactions	Function	API POST /api/transactions/deposit hợp lệ → trả 200 + balance mới chính xác	2026-05-11	2026-05-11	done	✅ PASS	200 OK, balance=490000 (API verified)
35	Book Management	Web	Transactions	Function	API POST /api/transactions (mua sách) hợp lệ → trả 200 + bookId + amount + balance còn lại	2026-05-11	2026-05-11	done	✅ PASS	200 OK, bookId=9, amount=50000, balance=440000 (API verified)
36	Book Management	Web	Transactions	Negative	API POST /api/transactions mua sách với bookId không tồn tại → trả 404 "Không tìm thấy sách"	2026-05-11	2026-05-11	done	✅ PASS	404 Not Found (API verified)
```

---

## MODULE: TẢI LÊN (Upload) – Web

```tsv
#	Dự án	Môi trường	Tên module	Chức năng	Task	Test Plan Start date	End date	Progress	Status	Note
37	Book Management	Web	Upload	Function	Admin chọn ảnh .jpg hợp lệ (< 5MB) khi thêm sách → upload thành công → nhận URL ảnh	2026-05-11	2026-05-11	done	✅ PASS	200 + URL /api/images/books/*.jpg (API verified)
38	Book Management	Web	Upload	Function	Admin chọn ảnh .png hợp lệ (< 5MB) → upload thành công → nhận URL ảnh	2026-05-11	2026-05-11	done	✅ PASS	200 + URL /api/images/books/*.png (API verified)
39	Book Management	Web	Upload	Function	Admin chọn ảnh .jpeg hợp lệ → upload thành công	2026-05-11	2026-05-11	done	✅ PASS	200 + URL /api/images/books/*.jpeg (API verified)
40	Book Management	Web	Upload	Function	Ảnh uploaded hiển thị đúng trong preview khi thêm/sửa sách	2026-05-11	2026-05-11	done	✅ PASS	add-book.html có logic preview ảnh (code verified)
41	Book Management	Web	Upload	Function	Nhấn nút ✕ xóa ảnh preview → ảnh bị xóa, vùng upload xuất hiện lại	2026-05-11	2026-05-11	done	✅ PASS	add-book.html có handler xóa preview (code verified)
42	Book Management	Web	Upload	Function	Kéo thả (drag & drop) ảnh vào vùng upload → ảnh được chọn và preview hiển thị	2026-05-11	2026-05-11	done	✅ PASS	dragover/dragleave/drop handlers (code verified L123-127)
43	Book Management	Web	Upload	Negative	Upload file không phải ảnh (.pdf, .docx, .txt) → API trả 400 "Unsupported file type"	2026-05-11	2026-05-11	done	✅ PASS	400: {"message":"Unsupported file type."} (API verified)
44	Book Management	Web	Upload	Negative	Upload file ảnh quá 5MB → API trả 400 "File is too large"	2026-05-11	2026-05-11	done	✅ PASS	400: {"message":"File is too large."} (API verified)
45	Book Management	Web	Upload	Negative	Upload request không có file (file rỗng) → API trả 400 "File is empty"	2026-05-11	2026-05-11	done	✅ PASS	400: {"message":"File is empty."} (API verified)
46	Book Management	Web	Upload	Permission	User thường (không phải Admin) gọi API POST /api/upload → bị chặn 401/403	2026-05-11	2026-05-11	done	✅ PASS	403 Forbidden (API verified)
47	Book Management	Web	Upload	Permission	Không có JWT gọi API POST /api/upload → bị chặn 401 Unauthorized	2026-05-11	2026-05-11	done	✅ PASS	401 Unauthorized (API verified)
48	Book Management	Web	Upload	Function	Upload ảnh .webp hợp lệ → upload thành công	2026-05-11	2026-05-11	done	✅ PASS	200 + URL /api/images/books/*.webp (API verified)
49	Book Management	Web	Upload	Function	Upload ảnh .gif hợp lệ → upload thành công	2026-05-11	2026-05-11	done	✅ PASS	200 + URL /api/images/books/*.gif (API verified)
50	Book Management	Web	Upload	Function	Sau upload thành công → file ảnh được lưu tại /api/images/books/{filename} và có thể truy cập qua URL trả về	2026-05-11	2026-05-11	done	✅ PASS	HTTP 200 truy cập URL (API verified)
```
