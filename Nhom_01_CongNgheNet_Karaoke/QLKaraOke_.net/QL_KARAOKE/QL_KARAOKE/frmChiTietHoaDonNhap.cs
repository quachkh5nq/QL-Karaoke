using QL_KARAOKE.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QL_KARAOKE
{
    public partial class frmChiTietHoaDonNhap : Form
    {
        public frmChiTietHoaDonNhap(long idHoaDon, byte danhapkho)
        {
            this.idHoaDon = idHoaDon;
            this.danhapkho = danhapkho;

            InitializeComponent();
        }
        private long idHoaDon;
        private byte danhapkho;

        private KARAOKE_DatabaseDataContext db;
        private void frmChiTietHoaDonNhap_Load(object sender, EventArgs e)
        {
            db = new KARAOKE_DatabaseDataContext();



            
            var hd = db.HoaDonNhaps.SingleOrDefault(x => x.ID == idHoaDon);
            if (hd.DaNhap == 1)
            {
                btnThem.Enabled = btnXoa.Enabled = false;
            }

           
            var rs = from h in db.MatHangs.Where(x => (x.idcha == null || x.idcha <= 0) && x.isDichVu == 0)
                     join d in db.DonViTinhs on h.DVT equals d.ID
                     select new
                     {
                         tenmathang = h.TenMatHang + " - " + d.TenDVT,
                         mahang = h.ID
                     };
            cbbMatHang.DataSource = rs;
            cbbMatHang.DisplayMember = "tenmathang";
            cbbMatHang.ValueMember = "mahang";
            cbbMatHang.SelectedIndex = -1;

            ShowData();


           
            dgvMatHang.Columns["idmathang"].Visible = false;
            //tùy chỉnh lại hiển thị trên datagridview
            dgvMatHang.Columns["mathang"].HeaderText = "Tên mặt hàng";
            dgvMatHang.Columns["dvt"].HeaderText = "ĐVT";
            dgvMatHang.Columns["sl"].HeaderText = "SL";
            dgvMatHang.Columns["dg"].HeaderText = "Đơn giá";
            dgvMatHang.Columns["thanhtien"].HeaderText = "Thành tiền";

            dgvMatHang.Columns["dvt"].Width = 100;
            dgvMatHang.Columns["sl"].Width = 100;
            dgvMatHang.Columns["thanhtien"].Width = 100;
            dgvMatHang.Columns["dg"].Width = 100;
            dgvMatHang.Columns["mathang"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            //định dạng phần nghìn
            dgvMatHang.Columns["dg"].DefaultCellStyle.Format = "N0";
            dgvMatHang.Columns["thanhtien"].DefaultCellStyle.Format = "N0";

            //căn cho đẹp
            dgvMatHang.Columns["sl"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvMatHang.Columns["dvt"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvMatHang.Columns["dg"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvMatHang.Columns["thanhtien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        private void ShowData()
        {
            
            var rs = (from c in db.ChiTietHoaDonNhaps.Where(x => x.IDHoaDon == idHoaDon)
                      join h in db.MatHangs
                      on c.IDMatHang equals h.ID
                      join d in db.DonViTinhs on h.DVT equals d.ID

                      select new
                      {
                          
                          idmathang = h.ID,
                          idcha = h.idcha,
                          mathang = h.TenMatHang,
                          dvt = d.TenDVT,
                          sl = c.SoLuong,
                          dg = c.DonGiaNhap,
                          thanhtien = c.SoLuong * c.DonGiaNhap
                      }).Where(x => x.idcha <= 0 || x.idcha == null);

            dgvMatHang.DataSource = rs;
            lblTongTien.Text = string.Format("Tổng tiền: {0:N0} VNĐ", rs.Sum(x => x.thanhtien));
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (cbbMatHang.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn mặt hàng cần nhập", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var ct = new ChiTietHoaDonNhap();
            ct.IDHoaDon = idHoaDon;
            
            var item = db.ChiTietHoaDonNhaps.FirstOrDefault(x => x.IDHoaDon == idHoaDon && x.IDMatHang == int.Parse(cbbMatHang.SelectedValue.ToString()));

            

            if (item == null)
            {
                ct.IDMatHang = int.Parse(cbbMatHang.SelectedValue.ToString());
                ct.DonGiaNhap = int.Parse(txtDonGiaNhap.Text);
                ct.SoLuong = int.Parse(txtSL.Text);
                db.ChiTietHoaDonNhaps.InsertOnSubmit(ct);
                db.SubmitChanges();
            }
            else
            {
                item.SoLuong += int.Parse(txtSL.Text);
                db.SubmitChanges();
            }

            ShowData();
            
        }

        #region
        //trong quá trình nhập hàng, nhiều lúc chúng ta sẽ nhập nhầm mặt hàng so với thực tế
        //vì vậy, chúng ta cần code thêm chức năng xóa mặt hàng khi nhập phiếu với điều kiện là phiếu chưa đc xác nhận nhập kho
        //muốn xóa được 1 mặt hàng trong bảng chitiethoadonnhap, chúng ta cần xác định được khóa chính của nó
        //khóa chính của nó gồm 2 trường: id hóa đơn nhập và id mặt hàng
        //chức năng xóa chúng ta sẽ code hoàn toàn tương tự như các module trước
        #endregion
        private DataGridViewRow r;
        private void dgvMatHang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                r = dgvMatHang.Rows[e.RowIndex];
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (r == null)
            {
                MessageBox.Show("Vui lòng chọn mặt hàng cần xóa khỏi phiếu nhập", "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (

                MessageBox.Show("Bạn có chắc muốn xóa mặt hàng: " + r.Cells["mathang"].Value.ToString() + " ?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.Yes
                )
            {

                var item = db.ChiTietHoaDonNhaps.FirstOrDefault(x => x.IDHoaDon == idHoaDon && x.IDMatHang == int.Parse(r.Cells["idmathang"].Value.ToString()));
                db.ChiTietHoaDonNhaps.DeleteOnSubmit(item);
                db.SubmitChanges();
                MessageBox.Show("Xóa mặt hàng thành công", "Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowData();
            }
        }

        private void rbtNhapKho_Click(object sender, EventArgs e)
        {
            if (rbtYeuCau.Checked)
            {
                txtTienThanhToan.Enabled = false;
                txtTienThanhToan.Text = "0";//mặc số tiền thanh toán sẽ là 0
            }
            else
            {
                txtTienThanhToan.Enabled = true;
            }
        }

        private void btnXacNhan_Click(object sender, EventArgs e)
        {

           
            if (dgvMatHang.Rows.Count == 0)
            {
                MessageBox.Show("Vui lòng nhập mặt hàng vào hóa đơn nhập trước khi tiếp tục", "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

           

            
            var hd = db.HoaDonNhaps.SingleOrDefault(x => x.ID == idHoaDon);
            hd.TienThanhToan = int.Parse(txtTienThanhToan.Text);
            hd.DaNhap = rbtYeuCau.Checked ? (byte)0 : (byte)1;
            db.SubmitChanges();
            this.Dispose();//đóng form
        }
    }
}
