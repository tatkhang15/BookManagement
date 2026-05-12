# 📸 Bằng chứng Test – Tester 2: Hoàng Phước Tất Khang

**Ngày test**: 2026-05-11  
**Tổng files**: 34 (32 screenshots + 2 video recordings)

---

## 🔑 Login
| File | Mô tả |
|:--|:--|
| `TC00_login_thanh_cong.png` | Đăng nhập thành công với tài khoản admin, hiển thị trang quản lý |

---

## 💳 MODULE: NẠP TIỀN (TC01-12)

| TC | File | Mô tả |
|:--|:--|:--|
| TC01 | `TC01_deposit_hien_thi_so_du.png` | Trang deposit hiển thị số dư hiện tại (5.490.010 VNĐ) |
| TC01 | `TC01_deposit_hien_thi_so_du_v2.png` | Trang quản lý người dùng hiển thị số dư các user |
| TC02 | `TC02_nut_50k_dien_50000.png` | Nhấn nút 50.000đ → input tự động điền 50000 |
| TC02 | `TC02_nut_menh_gia_v2.png` | Trang deposit với 3 nút mệnh giá (50k/100k/500k) |
| TC03 | `TC03_nut_100k_dien_100000.png` | Nhấn nút 100.000đ → input điền 100000 |
| TC04 | `TC04_nut_500k_dien_500000.png` | Nhấn nút 500.000đ → input điền 500000 |
| TC05 | `TC05_nap_250k_toast_thanh_cong.png` | Nạp 250.000đ → toast "Nạp thành công 250.000 VNĐ vào ví!" |
| TC05 | `TC05_nhap_250k_v2.png` | Nhập 250000 vào ô số tiền, sẵn sàng xác nhận |
| TC06 | `TC06_so_du_cap_nhat_realtime.png` | Số dư cập nhật realtime sau nạp tiền (không cần F5) |
| TC07 | `TC07_de_trong_toast_loi.png` | Để trống ô tiền → toast "Vui lòng nhập số tiền hợp lệ (tối thiểu 10.000đ)" |
| TC07 | `TC07_de_trong_v2.png` | Trang deposit với ô nhập trống |
| TC07 | `TC07_validation_empty_submit.png` | Nhấn xác nhận khi ô trống → hiện toast lỗi |
| TC08 | `TC08_so_tien_0_toast_loi.png` | Nhập 0 → toast lỗi tối thiểu 10.000đ |
| TC09 | `TC09_so_tien_5000_toast_loi.png` | Nhập 5000 (< 10000) → toast lỗi |
| TC09 | `TC09_so_tien_5000_v2.png` | Ô nhập hiển thị 5000 |
| TC10 | `TC10_so_tien_am_toast_loi.png` | Nhập -50000 → toast lỗi |
| TC10 | `TC10_so_tien_am_v2.png` | Ô nhập hiển thị -50000 |
| TC11 | `TC11_chua_login_redirect.png` | Chưa login → redirect về trang đăng nhập |
| TC12 | `TC12_nut_disabled_dang_xu_ly.png` | Nút bị disabled + hiển thị "Đang xử lý giao dịch..." |

---

## 🛒 MODULE: MUA SÁCH (TC13-23)

| TC | File | Mô tả |
|:--|:--|:--|
| TC13 | `TC13_modal_xac_nhan_ten_gia.png` | Modal xác nhận mua hiển thị tên sách + giá |
| TC13 | `TC13_buy_modal_detail.png` | Chi tiết modal xác nhận mua sách |
| TC14 | `TC14_mua_thanh_cong_toast.png` | Toast "Mua sách thành công! 🎉" + sách biến mất |
| TC14 | `TC14_buy_confirm_result.png` | Kết quả sau khi nhấn xác nhận mua |
| TC17 | `TC17_so_du_khong_du_toast.png` | Số dư không đủ → toast lỗi "Thanh toán thất bại" |
| TC17 | `TC17_insufficient_balance_detail.png` | Chi tiết toast lỗi số dư không đủ |
| TC21 | `TC21_huy_modal_khong_giao_dich.png` | Nhấn Huỷ → quay lại trang chủ bình thường |
| TC21 | `TC21_logout_redirect.png` | Redirect sau logout |
| TC23 | `TC23_popup_sach_cua_ban_da_mua.png` | Popup chi tiết sách: "Sách của bạn" + "✅ Đã mua" |

---

## 📚 MODULE: SÁCH ĐÃ MUA (TC24-26)

| TC | File | Mô tả |
|:--|:--|:--|
| TC24 | `TC24_tab_sach_da_mua.png` | Tab "Sách đã mua" hiển thị danh sách sách (ảnh bìa + tiêu đề) |
| TC24 | `TC24_purchased_books_tab_detail.png` | Chi tiết tab sách đã mua |

---

## 📤 MODULE: TẢI LÊN (TC37-50)

### Screenshots

| TC | File | Mô tả |
|:--|:--|:--|
| TC40 | `TC40_upload_preview_them_sach.png` | Giao diện thêm sách với vùng upload ảnh + preview |

### API Test Results (TC37-39, TC43-50)

| TC | File | Mô tả |
|:--|:--|:--|
| TC37-50 | `TC37-50_api_upload_test_results.txt` | Toàn bộ kết quả API upload tests với HTTP status code + response body |
| TC40-42 | `TC40-42_upload_ui_code_evidence.txt` | Code evidence: drag-drop, preview, remove handlers trong `add-book.html` |

**Chi tiết kết quả API:**

| TC | Test | HTTP Code | Response |
|:--|:--|:--|:--|
| TC37 | Upload .jpg | ✅ 200 | `{"url":"/api/images/books/...jpg"}` |
| TC38 | Upload .png | ✅ 200 | `{"url":"/api/images/books/...png"}` |
| TC39 | Upload .jpeg | ✅ 200 | `{"url":"/api/images/books/...jpeg"}` |
| TC40 | Preview ảnh khi thêm sách | ✅ PASS | Code evidence: preview handler trong add-book.html |
| TC41 | Nút ✕ xóa preview | ✅ PASS | Code evidence: remove handler xóa preview + reset coverUrl |
| TC42 | Kéo thả (drag & drop) | ✅ PASS | Code evidence: dragover/dragleave/drop events (line 123-127) |
| TC43 | Upload .txt | ✅ 400 | `{"message":"Unsupported file type."}` |
| TC44 | Upload >5MB | ✅ 400 | `{"message":"File is too large."}` |
| TC45 | Upload empty | ✅ 400 | `{"message":"File is empty."}` |
| TC46 | User (non-admin) upload | ✅ 403 | Forbidden |
| TC47 | No JWT upload | ✅ 401 | Unauthorized |
| TC48 | Upload .webp | ✅ 200 | `{"url":"/api/images/books/...webp"}` |
| TC49 | Upload .gif | ✅ 200 | `{"url":"/api/images/books/...gif"}` |
| TC50 | File accessible via URL | ✅ 200 | Image truy cập được tại URL trả về |

---

## 🎬 Video Recordings

| File | Mô tả | Thời lượng |
|:--|:--|:--|
| `VIDEO_TC01-06_login_va_nap_tien.webp` | Recording: Login → Deposit → Nút mệnh giá → Nạp 250k | ~2 phút |
| `VIDEO_TC11-14-21_quyen_va_mua_sach.webp` | Recording: Test quyền deposit → Login → Mua sách → Cancel modal | ~5 phút |

---

## 📝 Ghi chú

- **TC15-16, TC18-20, TC22, TC25-36**: Được verify qua **API testing** (PowerShell `Invoke-RestMethod`) — kết quả chi tiết ghi trong `TestCase_Tester2_HoangPhuocTatKhang.md`
- **TC37-39, TC43-50**: Được verify qua **curl API testing** — kết quả lưu trong `TC37-50_api_upload_test_results.txt`
- **TC40-42**: Được verify qua **phân tích source code** (`add-book.html`) — evidence lưu trong `TC40-42_upload_ui_code_evidence.txt`
