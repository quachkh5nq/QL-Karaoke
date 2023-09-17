using QL_KARAOKE.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QL_KARAOKE
{
    public partial class frmNhapHang : Form
    {
        public frmNhapHang(string nhanvien)
        {
            this.nhanvien = nhanvien;
            InitializeComponent();
        }
        private KARAOKE_DatabaseDataContext db;
        private string nhanvien;
        private void frmNhapHang_Load(object sender, EventArgs e)
        {
            db = new KARAOKE_DatabaseDataContext();
            mtbNgayNhap.Text = DateTime.Now.ToString("dd/MM/yyyy");

            cbbNhanVienNhap.DataSource = db.NhanViens.Where(x=>x.isDelete ==0);
            cbbNhanVienNhap.DisplayMember = "hovaten";
            cbbNhanVienNhap.ValueMember = "username";
            cbbNhanVienNhap.SelectedIndex = -1;

            cbbNCC.DataSource = db.NhaCungCaps.Where(x=>x.isDelete==0);
            cbbNCC.DisplayMember = "ID";
            cbbNCC.ValueMember = "ID";
            cbbNCC.SelectedIndex = -1;
            ShowData();
            dgvHoaDonNhap.Columns["danhap"].Visible = false;

            dgvHoaDonNhap.Columns["id"].HeaderText = "ID Phiếu";
            dgvHoaDonNhap.Columns["NgayNhap"].HeaderText = "Ngày nhập";
            dgvHoaDonNhap.Columns["TenNCC"].HeaderText = "Nhà cung cấp";
            dgvHoaDonNhap.Columns["trangthai"].HeaderText = "Trạng thái";
            dgvHoaDonNhap.Columns["tongtien"].HeaderText = "Tổng tiền";
            dgvHoaDonNhap.Columns["dathanhtoan"].HeaderText = "Đã thanh toán";

            dgvHoaDonNhap.Columns["id"].Width = 100;
            dgvHoaDonNhap.Columns["NgayNhap"].Width = 100;
            dgvHoaDonNhap.Columns["trangthai"].Width = 100;
            dgvHoaDonNhap.Columns["tongtien"].Width = 100;
            dgvHoaDonNhap.Columns["dathanhtoan"].Width = 100;
            dgvHoaDonNhap.Columns["tenncc"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dgvHoaDonNhap.Columns["id"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvHoaDonNhap.Columns["trangthai"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvHoaDonNhap.Columns["tongtien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvHoaDonNhap.Columns["dathanhtoan"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dgvHoaDonNhap.Columns["tongtien"].DefaultCellStyle.Format = "N0";
            dgvHoaDonNhap.Columns["dathanhtoan"].DefaultCellStyle.Format = "N0";
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            DateTime ngaynhap;
            try
            {
                ngaynhap = DateTime.ParseExact(mtbNgayNhap.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            catch
            {
                MessageBox.Show("Ngày nhập không hợp lệ", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            //nhân viên nhập hàng được hiểu là người phụ trách kiểm kê hàng hóa khi nhập vào kho
            if (cbbNhanVienNhap.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn nhân viên nhập hàng", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cbbNCC.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var od = new HoaDonNhap();
                od.NhanVienNhap = cbbNhanVienNhap.SelectedValue.ToString();
                od.IDNhaCC = int.Parse(cbbNCC.SelectedValue.ToString());
                od.NgayNhap = ngaynhap;

                od.NgayTao = DateTime.Now;
                od.NguoiTao = nhanvien;

                db.HoaDonNhaps.InsertOnSubmit(od);
                db.SubmitChanges();


       
                var idHDNhap = db.HoaDonNhaps.Max(x => x.ID);
               
                new frmChiTietHoaDonNhap(idHDNhap, 0).ShowDialog();
                ShowData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Tạo hóa đơn nhập hàng thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowData()
        {
            
            var rs = from o in db.HoaDonNhaps
                     join n in db.NhaCungCaps on o.IDNhaCC equals n.ID
                     join c in db.NhanViens on o.NhanVienNhap equals c.Username

                     select new
                     {
                         id = o.ID,
                         NgayNhap = o.NgayNhap,
                         TenNCC = n.TenNCC,
                         danhap = o.DaNhap,
                         trangthai = o.DaNhap == 1 ? "Đã nhập" : "Yêu cầu",
                         tongtien = db.ChiTietHoaDonNhaps.Where(x => x.IDHoaDon == o.ID).Sum(y => y.SoLuong * y.DonGiaNhap),//tổng tiền được tính dựa vào tổng các mặt hàng: sl*dg tương ứng với hóa đơn, 
                         
                         dathanhtoan = o.TienThanhToan
                     };

            dgvHoaDonNhap.DataSource = rs;
        }


        private DataGridViewRow r;
        private void dgvHoaDonNhap_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                r = dgvHoaDonNhap.Rows[e.RowIndex];

                
          

                byte danhapkho = r.Cells["danhap"].Value == null ? (byte)0 : byte.Parse(r.Cells["danhap"].Value.ToString());
                
                new frmChiTietHoaDonNhap(long.Parse(r.Cells["id"].Value.ToString()), danhapkho).ShowDialog();
                ShowData();
            }
        }


        private void btnXoa_Click(object sender, EventArgs e)
        {
            if(r == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu nhập cần xóa", "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
           
        }
    }
}
