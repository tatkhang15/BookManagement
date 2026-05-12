# Test Cases – Tester 3: Nguyễn Duy Huy Hoàng

- **Nhiệm vụ**: Kiểm thử chức năng
- **Phạm vi**: Người dùng (Users) + Giao diện Web UI/UX + Quản trị sách Admin UI
- **Môi trường**: Web
- **Tổng số test case**: 50

---

## MODULE: HỒ SƠ CÁ NHÂN (User Profile) – Web

```tsv
#	Dự án	Môi trường	Tên module	Chức năng	Task	Test Plan Start date	End date	Progress	Status	Note
1	Book Management	Web	User Profile	Function	Đăng nhập User → mở /user-profile.html → hiển thị đúng Họ tên, Email, Avatar chữ cái đầu			pending	
2	Book Management	Web	User Profile	Function	Trang profile hiển thị đúng Số dư ví, Ngày tạo tài khoản, Trạng thái, Quyền hạn			pending	
3	Book Management	Web	User Profile	Function	Sửa Họ và Tên → nhấn "Cập nhật thông tin" → Toast "Cập nhật thành công!" → tên mới hiển thị			pending	
4	Book Management	Web	User Profile	Function	Sửa Email → nhấn Cập nhật → cập nhật thành công			pending	
5	Book Management	Web	User Profile	Function	Sửa Số điện thoại hợp lệ (0352345678) → nhấn Cập nhật → thành công			pending	
6	Book Management	Web	User Profile	Validation	Nhập số điện thoại sai định dạng (123456, abc, 12345) → hiển thị lỗi "Số điện thoại không hợp lệ (Phải bắt đầu bằng 0, gồm 10 chữ số)"			pending	
7	Book Management	Web	User Profile	Validation	Nhập số điện thoại bắt đầu bằng 01 (0123456789) → hiển thị lỗi (chỉ chấp nhận 03/05/07/08/09)			pending	
8	Book Management	Web	User Profile	Validation	Nhập số điện thoại chỉ 9 chữ số (035234567) → hiển thị lỗi			pending	
9	Book Management	Web	User Profile	Validation	Nhập ký tự chữ vào ô số điện thoại → bị chặn (chỉ cho nhập số)			pending	
10	Book Management	Web	User Profile	Function	User có role "User" → badge quyền hạn hiển thị "User" với style mặc định			pending	
11	Book Management	Web	User Profile	Function	Admin đăng nhập → badge quyền hạn hiển thị "Quản trị viên (Admin)" với style đỏ			pending	
12	Book Management	Web	User Profile	Function	Nhấn tab "Sách đã mua" → chuyển sang tab hiển thị grid sách đã mua			pending	
13	Book Management	Web	User Profile	Function	Nhấn tab "Thông tin tài khoản" → quay về tab thông tin cá nhân			pending	
14	Book Management	Web	User Profile	Function	Truy cập /user-profile.html?tab=purchased → tự động mở tab "Sách đã mua"			pending	
15	Book Management	Web	User Profile	Permission	Chưa đăng nhập → truy cập /user-profile.html → bị chuyển hướng về login			pending	
16	Book Management	Web	User Profile	Function	Nhấn "Xoá vĩnh viễn" → hiện confirm dialog → nhấn OK → Toast "Đã xoá tài khoản thành công!" → chuyển về login			pending	
17	Book Management	Web	User Profile	Function	Nhấn "Xoá vĩnh viễn" → hiện confirm dialog → nhấn Cancel → không xảy ra gì			pending	
```

---

## MODULE: QUẢN LÝ NGƯỜI DÙNG – ADMIN (Admin Users) – Web

```tsv
#	Dự án	Môi trường	Tên module	Chức năng	Task	Test Plan Start date	End date	Progress	Status	Note
18	Book Management	Web	Admin Users	Function	Admin đăng nhập → mở /admin-users.html → hiển thị bảng danh sách user (ID, Tên, Email, Quyền, Số dư)			pending	
19	Book Management	Web	Admin Users	Function	Bảng hiển thị đúng số lượng user (ví dụ "5 người dùng")			pending	
20	Book Management	Web	Admin Users	Function	Nhấn "Chỉnh sửa" user → hiện prompt nhập tên mới → nhập hợp lệ → cập nhật thành công			pending	
21	Book Management	Web	Admin Users	Function	Nhấn "Chỉnh sửa" user → prompt nhập email → nhập email mới → cập nhật thành công			pending	
22	Book Management	Web	Admin Users	Function	Nhấn "Chỉnh sửa" user → prompt nhập SĐT → nhập hợp lệ → cập nhật thành công			pending	
23	Book Management	Web	Admin Users	Function	Nhấn "Nâng cấp SubAdmin" → Toast "Cập nhật quyền thành công" → role user đổi thành SubAdmin			pending	
24	Book Management	Web	Admin Users	Function	Nhấn "Hạ quyền User" → Toast "Cập nhật quyền thành công" → role user đổi thành User			pending	
25	Book Management	Web	Admin Users	Function	Sau khi đổi role user thành SubAdmin → đăng nhập bằng user đó → quyền SubAdmin hoạt động đúng			pending	
26	Book Management	Web	Admin Users	Negative	Admin tự đổi role chính mình → API trả lỗi "Không thể tự cập nhật quyền của chính mình"			pending	
27	Book Management	Web	Admin Users	Permission	User thường truy cập /admin-users.html → bị chuyển hướng về trang chủ			pending	
28	Book Management	Web	Admin Users	Permission	Chưa đăng nhập truy cập /admin-users.html → bị chuyển hướng về trang chủ			pending	
29	Book Management	Web	Admin Users	Function	Không có user nào (edge case) → hiển thị "Chưa có người dùng nào trên hệ thống"			pending	
```

---

## MODULE: GIAO DIỆN & ĐIỀU HƯỚNG WEB UI/UX – Web

```tsv
#	Dự án	Môi trường	Tên module	Chức năng	Task	Test Plan Start date	End date	Progress	Status	Note
30	Book Management	Web	Navigation	Function	Truy cập / → serve trang index.html (trang chủ) thành công			pending	
31	Book Management	Web	Navigation	Function	Nhấn link "Đăng nhập" trên sidebar/header → chuyển đến /login.html			pending	
32	Book Management	Web	Navigation	Function	Nhấn link "Đăng ký" → chuyển đến /register.html			pending	
33	Book Management	Web	Navigation	Function	Nhấn logo 📚 trên trang login → quay về trang chủ /			pending	
34	Book Management	Web	Navigation	Function	Nhấn logo 📚 trên trang register → quay về trang chủ /			pending	
35	Book Management	Web	Navigation	Function	Sidebar hiển thị đúng các menu item theo role (User vs Admin)			pending	
36	Book Management	Web	Navigation	Function	Nhấn overlay sidebar (trên mobile) → sidebar đóng lại			pending	
37	Book Management	Web	UI	Function	Toast thông báo hiển thị và tự động biến mất sau vài giây			pending	
38	Book Management	Web	UI	Function	Modal xác nhận (confirm) hiển thị đúng nội dung + có nút Xác nhận và Hủy			pending	
```

---

## MODULE: QUẢN TRỊ SÁCH TRÊN WEB – ADMIN UI – Web

```tsv
#	Dự án	Môi trường	Tên module	Chức năng	Task	Test Plan Start date	End date	Progress	Status	Note
39	Book Management	Web	Admin Books	Permission	User thường truy cập /books.html (trang quản lý sách) → bị chuyển hướng về trang chủ			pending	
40	Book Management	Web	Admin Books	Permission	User thường truy cập /add-book.html → bị chuyển hướng về trang chủ			pending	
41	Book Management	Web	Admin Books	Permission	User thường truy cập /edit-book.html → bị chuyển hướng về trang chủ			pending	
42	Book Management	Web	Admin Books	Function	Admin mở /books.html → hiển thị bảng danh sách sách (ID, Ảnh, Tiêu đề, Tác giả, Thể loại, Giá, Trạng thái)			pending	
43	Book Management	Web	Admin Books	Function	Admin nhấn "➕ Thêm Sách Mới" → chuyển đến /add-book.html			pending	
44	Book Management	Web	Admin Books	Function	Admin điền đầy đủ thông tin hợp lệ trên add-book → nhấn "Lưu Sách" → Toast "Thêm sách thành công!" → chuyển về trang chủ			pending	
45	Book Management	Web	Admin Books	Validation	Admin để trống Tiêu đề trên add-book → hiển thị lỗi "Tiêu đề là bắt buộc!"			pending	
46	Book Management	Web	Admin Books	Validation	Admin để trống Tác giả trên add-book → hiển thị lỗi "Tác giả là bắt buộc!"			pending	
47	Book Management	Web	Admin Books	Function	Admin nhấn "✏️ Sửa" trên books.html → chuyển đến /edit-book.html?id={bookId} → form tải đúng dữ liệu sách hiện tại			pending	
48	Book Management	Web	Admin Books	Function	Admin sửa tiêu đề/tác giả/giá trên edit-book → nhấn "Cập nhật Sách" → Toast "Cập nhật sách thành công!"			pending	
49	Book Management	Web	Admin Books	Function	Admin nhấn "🗑️ Xóa" → hiện modal "Xác nhận xóa" → nhấn Xóa → Toast "Xóa sách thành công!" → sách biến mất khỏi bảng			pending	
50	Book Management	Web	Admin Books	Function	Admin tìm kiếm sách bằng ô Search → kết quả lọc theo tiêu đề/tác giả/thể loại/ISBN			pending	
```
