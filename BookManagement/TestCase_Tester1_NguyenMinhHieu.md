# Test Cases – Tester 1: Nguyễn Minh Hiếu

- **Nhiệm vụ**: Kiểm thử chức năng Web
- **Phạm vi**: Xác thực (Authentication) + Quản lý sách (Books Management)
- **Môi trường**: Web
- **Tổng số test case**: 50

---

## MODULE: ĐĂNG NHẬP (Login) – Web

```tsv
#	Dự án	Môi trường	Tên module	Chức năng	Task	Test Plan Start date	End date	Progress	Status	Note
1	Book Management	Web	Login	Validation	Để trống cả hai trường Identifier và Password → nhấn Đăng nhập → hiển thị Toast lỗi "Vui lòng điền đầy đủ tài khoản và mật khẩu!"			pending	
2	Book Management	Web	Login	Validation	Nhập Identifier hợp lệ, để trống Password → nhấn Đăng nhập → hiển thị Toast lỗi			pending	
3	Book Management	Web	Login	Validation	Để trống Identifier, nhập Password hợp lệ → nhấn Đăng nhập → hiển thị Toast lỗi			pending	
4	Book Management	Web	Login	Function	Đăng nhập bằng email hợp lệ + password đúng → Toast "Đăng nhập thành công!" → chuyển hướng về trang chủ			pending	
5	Book Management	Web	Login	Function	Đăng nhập bằng username hợp lệ + password đúng → Toast "Đăng nhập thành công!" → chuyển hướng về trang chủ			pending	
6	Book Management	Web	Login	Negative	Nhập email đúng + password sai → Toast lỗi "Sai tài khoản hoặc mật khẩu"			pending	
7	Book Management	Web	Login	Negative	Nhập username không tồn tại + password bất kỳ → Toast lỗi "Sai tài khoản hoặc mật khẩu"			pending	
8	Book Management	Web	Login	Negative	Nhập email sai định dạng (thiếu @) + password bất kỳ → Toast lỗi			pending	
9	Book Management	Web	Login	Function	Đăng nhập thành công → token JWT được lưu vào session/localStorage			pending	
10	Book Management	Web	Login	Function	Đăng nhập với returnUrl trong query → sau khi login thành công → chuyển hướng đúng returnUrl			pending	
11	Book Management	Web	Login	UI	Nhấn link "Quên mật khẩu?" → chuyển đến trang /forgot-password.html			pending	
12	Book Management	Web	Login	UI	Nhấn link "Đăng ký ngay" → chuyển đến trang /register.html			pending	
13	Book Management	Web	Login	UI	Trong khi xử lý đăng nhập → nút Đăng nhập bị disabled + hiển thị spinner "Đang xử lý..."			pending	
```

---

## MODULE: ĐĂNG KÝ (Register) – Web

```tsv
#	Dự án	Môi trường	Tên module	Chức năng	Task	Test Plan Start date	End date	Progress	Status	Note
14	Book Management	Web	Register	Validation	Để trống tất cả các trường → nhấn Tiếp tục → Toast "Vui lòng điền đầy đủ thông tin!"			pending	
15	Book Management	Web	Register	Validation	Để trống Họ và tên → nhấn Tiếp tục → Toast cảnh báo			pending	
16	Book Management	Web	Register	Validation	Để trống Tên đăng nhập → nhấn Tiếp tục → Toast cảnh báo			pending	
17	Book Management	Web	Register	Validation	Để trống Email → nhấn Tiếp tục → Toast cảnh báo			pending	
18	Book Management	Web	Register	Validation	Nhập email sai định dạng (thiếu @, thiếu domain) → Toast lỗi			pending	
19	Book Management	Web	Register	Validation	Nhập mật khẩu < 3 ký tự → Toast "Mật khẩu phải có ít nhất 3 ký tự!"			pending	
20	Book Management	Web	Register	Validation	Nhập Mật khẩu và Nhập lại mật khẩu không khớp → Toast "Mật khẩu nhập lại không khớp!"			pending	
21	Book Management	Web	Register	Function	Điền đầy đủ thông tin hợp lệ → nhấn Tiếp tục → Toast "Mã OTP đã được gửi!" → chuyển sang Step 2 (nhập OTP)			pending	
22	Book Management	Web	Register	Function	Ở Step 2 nhập mã OTP đúng → Toast "Tạo tài khoản thành công!" → chuyển hướng về trang login			pending	
23	Book Management	Web	Register	Negative	Ở Step 2 nhập mã OTP sai → Toast lỗi "Xác nhận thất bại"			pending	
24	Book Management	Web	Register	Negative	Ở Step 2 để trống OTP → nhấn Xác nhận → Toast "Vui lòng nhập OTP"			pending	
25	Book Management	Web	Register	Negative	Đăng ký email đã tồn tại → Toast lỗi hiển thị thông báo trùng			pending	
26	Book Management	Web	Register	Negative	Đăng ký username đã tồn tại → Toast lỗi			pending	
27	Book Management	Web	Register	UI	Nhấn nút "Quay lại" ở Step 2 → quay về Step 1, giữ nguyên dữ liệu đã nhập			pending	
28	Book Management	Web	Register	UI	Nhấn link "Đăng nhập" ở cuối trang → chuyển đến /login.html			pending	
```

---

## MODULE: QUÊN MẬT KHẨU (Forgot Password) – Web

```tsv
#	Dự án	Môi trường	Tên module	Chức năng	Task	Test Plan Start date	End date	Progress	Status	Note
29	Book Management	Web	Forgot Password	Validation	Để trống email → nhấn Gửi liên kết → hiển thị lỗi "Vui lòng nhập Email hợp lệ"			pending	
30	Book Management	Web	Forgot Password	Validation	Nhập email sai định dạng (thiếu @) → hiển thị lỗi validation			pending	
31	Book Management	Web	Forgot Password	Function	Nhập email hợp lệ đã đăng ký → nhấn Gửi → chuyển sang Step 2 nhập OTP			pending	
32	Book Management	Web	Forgot Password	Function	Ở Step 2 nhập OTP đúng → Toast "Xác nhận thành công!" → chuyển hướng sang trang Reset Password với token			pending	
33	Book Management	Web	Forgot Password	Negative	Ở Step 2 nhập OTP sai → Toast "OTP không chính xác"			pending	
34	Book Management	Web	Forgot Password	Negative	Ở Step 2 để trống OTP → Toast "Vui lòng nhập OTP"			pending	
35	Book Management	Web	Forgot Password	UI	Nhấn link "← Quay lại trang Đăng nhập" → chuyển về /login.html			pending	
```

---

## MODULE: ĐẶT LẠI MẬT KHẨU (Reset Password) – Web

```tsv
#	Dự án	Môi trường	Tên module	Chức năng	Task	Test Plan Start date	End date	Progress	Status	Note
36	Book Management	Web	Reset Password	Negative	Truy cập trang reset-password.html không có email/token trên URL → hiển thị cảnh báo "Liên kết không hợp lệ hoặc đã hết hạn"			pending	
37	Book Management	Web	Reset Password	Validation	Nhập mật khẩu mới < 6 ký tự → hiển thị lỗi "Mật khẩu phải có ít nhất 6 ký tự"			pending	
38	Book Management	Web	Reset Password	Validation	Mật khẩu mới và Xác nhận mật khẩu không khớp → hiển thị lỗi "Mật khẩu xác nhận không khớp"			pending	
39	Book Management	Web	Reset Password	Function	Nhập mật khẩu mới hợp lệ + xác nhận khớp + token hợp lệ → Toast "Đặt lại mật khẩu thành công!" → chuyển về login			pending	
40	Book Management	Web	Reset Password	Negative	Nhập mật khẩu với token hết hạn/sai → hiển thị lỗi "Đặt lại mật khẩu thất bại"			pending	
41	Book Management	Web	Reset Password	UI	Nhấn icon 👁️ toggle hiển thị/ẩn mật khẩu → chuyển đổi giữa type="password" và type="text"			pending	
```

---

## MODULE: QUẢN LÝ SÁCH – TRANG CHỦ (Index/Books) – Web

```tsv
#	Dự án	Môi trường	Tên module	Chức năng	Task	Test Plan Start date	End date	Progress	Status	Note
42	Book Management	Web	Trang chủ	Function	Mở trang chủ (index.html) → danh sách sách tải thành công → hiển thị section "Đang thịnh hành" và "Gợi ý cho bạn"			pending	
43	Book Management	Web	Trang chủ	Function	Nhấn tab thể loại (Công nghệ/Kinh tế/Văn học) → lọc sách theo thể loại tương ứng			pending	
44	Book Management	Web	Trang chủ	Function	Nhấn tab "Tất cả" → hiển thị lại toàn bộ sách			pending	
45	Book Management	Web	Trang chủ	Function	Nhấn vào card sách → popup chi tiết sách hiển thị đầy đủ (tiêu đề, tác giả, giá, mô tả, ảnh bìa)			pending	
46	Book Management	Web	Trang chủ	Function	Nhấn nút "Xem thêm sách" → thêm 12 sách tiếp theo vào danh sách			pending	
47	Book Management	Web	Trang chủ	Negative	Lọc theo thể loại không có sách nào → hiển thị "Hiện nay chưa có sách thuộc thể loại này" + nút "Quay lại tất cả sách"			pending	
48	Book Management	Web	Trang chủ	Permission	User thường truy cập trang chủ → KHÔNG thấy nút Thêm/Sửa/Xóa sách, chỉ thấy nút Mua và Chi tiết			pending	
49	Book Management	Web	Trang chủ	Permission	Admin đăng nhập vào trang chủ → thấy nút "Chỉnh sửa sách" trong popup chi tiết (thay nút Mua)			pending	
50	Book Management	Web	Trang chủ	Permission	Người dùng chưa đăng nhập nhấn "Mua ngay" → hiển thị modal yêu cầu đăng nhập			pending	
```
