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
    public partial class frmMatHang : Form
    {
        public frmMatHang(string nv)
        {
            this.nhanvien = nv;
            InitializeComponent();
        }

        private KARAOKE_DatabaseDataContext db;
        private string nhanvien;
        private DataGridViewRow r;
        private void frmMatHang_Load(object sender, EventArgs e)
        {

            db = new KARAOKE_DatabaseDataContext();

            cbbMatHangGoc.DataSource = db.MatHangs.Where(x => (x.idcha == null || x.idcha == -1) && x.isDichVu == 0 && x.isDelete ==0); //id cha bằng null hoặc -1 tức là không có mặt hàng nào là cha
         
            cbbMatHangGoc.DisplayMember = "TenMatHang";
            cbbMatHangGoc.ValueMember = "ID";
            cbbMatHangGoc.SelectedIndex = -1;

            ShowData();

            dgvMatHang.Columns["idcha"].Visible = false;
            dgvMatHang.Columns["tile"].Visible = false;

            //set be rong
            dgvMatHang.Columns["id"].Width = 100;
            dgvMatHang.Columns["tendvt"].Width = 100;
            dgvMatHang.Columns["dongiaban"].Width = 100;
            dgvMatHang.Columns["tenmathang"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;//auto size 

            // can chinh 
            dgvMatHang.Columns["id"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; 
            dgvMatHang.Columns["tendvt"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; 
            dgvMatHang.Columns["dongiaban"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; 
            dgvMatHang.Columns["dongiaban"].DefaultCellStyle.Format = "N0";

            //dat ten cot
            dgvMatHang.Columns["id"].HeaderText = "Mã hàng";
            dgvMatHang.Columns["tendvt"].HeaderText = "ĐVT";
            dgvMatHang.Columns["dongiaban"].HeaderText = "Đơn giá";
            dgvMatHang.Columns["tenmathang"].HeaderText = "Tên mặt hàng";


            //load len combobox
            cbbDVT.DataSource = db.DonViTinhs.Where(x=>x.isDelete==0);
            cbbDVT.DisplayMember = "TenDVT";
            cbbDVT.ValueMember = "ID";

            cbbDVT.SelectedIndex = -1;
        }

        private void ShowData()
        {
            var rs = from h in db.MatHangs.Where(x=>x.isDelete == 0)
                     join d in db.DonViTinhs.Where(x=>x.isDelete ==0) on h.DVT equals d.ID
                     select new
                     {
                         h.ID,
                         h.idcha,
                         h.Tile,
                         h.TenMatHang,
                         d.TenDVT,
                         h.DonGiaBan
                     };
            dgvMatHang.DataSource = rs;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTenMatHang.Text))
            {
                MessageBox.Show("Vui lòng nhập tên mặt hàng", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenMatHang.Select();
                return;
            }
            if (cbbDVT.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn đơn vị tính", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(txtDonGiaBan.Text))
            {
                MessageBox.Show("Vui lòng nhập đơn giá", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDonGiaBan.Select();
                return;
            }

            int idCha = -1;
            int tile = 0;
            if (cbbMatHangGoc.SelectedIndex >= 0)
            {
                idCha = int.Parse(cbbMatHangGoc.SelectedValue.ToString());
                //
                try
                {
                    tile = int.Parse(txtTiLe.Text);
                    if (tile <= 0)
                    {
                        MessageBox.Show("Tỉ lệ quy đổi phải lớn hơn 0", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtTiLe.Select();
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show("Tỉ lệ quy đổi không hợp lệ", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTiLe.Select();
                    return;
                }
            }
            var mh = new MatHang();
            mh.TenMatHang = txtTenMatHang.Text;
            mh.DVT = int.Parse(cbbDVT.SelectedValue.ToString());
            
            mh.DonGiaBan = int.Parse(txtDonGiaBan.Text);
            mh.idcha = idCha;
            mh.Tile = tile;
            mh.NgayTao = DateTime.Now;
            mh.NguoiTao = nhanvien;
            mh.isDelete = 0;
            mh.isDichVu = rbtDichVu.Checked ? (byte)1 : (byte)0;

            db.MatHangs.InsertOnSubmit(mh);
            db.SubmitChanges();//luu

            ShowData();
            MessageBox.Show("Thêm mới mặt hàng thành công", "Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);

            
            txtTenMatHang.Text = null;
            txtDonGiaBan.Text = "0";
            cbbDVT.SelectedIndex = -1;

            txtTenMatHang.Select();

        }

        private void txtDonGiaBan_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (r == null)
            {
                MessageBox.Show("Vui lòng chọn mặt hàng cần cập nhật", "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

          
           
            var mh = db.MatHangs.SingleOrDefault(x => x.ID == int.Parse(r.Cells["id"].Value.ToString()));
           


          
            if (string.IsNullOrEmpty(txtTenMatHang.Text))//kiem tra ten k dc null
            {
                MessageBox.Show("Vui lòng nhập tên mặt hàng", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenMatHang.Select();
                return;
            }
            if (cbbDVT.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn đơn vị tính", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(txtDonGiaBan.Text))
            {
                MessageBox.Show("Vui lòng nhập đơn giá", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDonGiaBan.Select();
                return;
            }

            int idCha = -1;
            int tile = 0;
            if (cbbMatHangGoc.SelectedIndex >= 0)
            {
                idCha = int.Parse(cbbMatHangGoc.SelectedValue.ToString());
                //
                try
                {
                    tile = int.Parse(txtTiLe.Text);
                    if (tile <= 0)
                    {
                        MessageBox.Show("Tỉ lệ quy đổi phải lớn hơn 0", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtTiLe.Select();
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show("Tỉ lệ quy đổi không hợp lệ", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTiLe.Select();
                    return;
                }
            }

            
            mh.TenMatHang = txtTenMatHang.Text;
            mh.DVT = int.Parse(cbbDVT.SelectedValue.ToString());
            mh.DonGiaBan = int.Parse(txtDonGiaBan.Text);
            mh.idcha = idCha;
            mh.Tile = tile;
            mh.isDichVu = rbtDichVu.Checked ? (byte)1 : (byte)0;
            mh.NgayCapNhat = DateTime.Now;
            mh.NguoiCapNhat = nhanvien;
            
            db.SubmitChanges();

            ShowData();
            MessageBox.Show("Cập nhật mặt hàng thành công", "Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);

            
            txtTenMatHang.Text = null;
            txtDonGiaBan.Text = "0";
            cbbDVT.SelectedIndex = -1;

        }

        private void dgvMatHang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                r = dgvMatHang.Rows[e.RowIndex];

                
                txtTenMatHang.Text = r.Cells["tenmathang"].Value.ToString();
                txtDonGiaBan.Text = r.Cells["dongiaban"].Value.ToString();
                cbbDVT.Text = r.Cells["tendvt"].Value.ToString();
                txtTiLe.Text = r.Cells["tile"].Value == null ? "0" : r.Cells["tile"].Value.ToString();
                if (r.Cells["idcha"].Value == null || r.Cells["idcha"].Value.ToString() == "-1")
                {
                    cbbMatHangGoc.SelectedIndex = -1;
                }
                else
                {
                    var item = db.MatHangs.SingleOrDefault(x => x.ID == int.Parse(r.Cells["idcha"].Value.ToString()));
                    cbbMatHangGoc.Text = item.TenMatHang;
                }

            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            
            if (r == null)
            {
                MessageBox.Show("Vui lòng chọn mặt hàng cần xóa", "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (
                MessageBox.Show(
                        "Bạn có muốn xóa mặt hàng: " + r.Cells["tenmathang"].Value.ToString() + "?",
                        "Xác nhận xóa mặt hàng",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    ) == DialogResult.Yes
                )
            {
                try
                {
                    var mh = db.MatHangs.SingleOrDefault(x => x.ID == int.Parse(r.Cells["id"].Value.ToString()));
                    mh.isDelete = 1;
                    db.SubmitChanges();
                    MessageBox.Show("Xóa mặt hàng thành công", "Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch
                {
                    MessageBox.Show("Xóa mặt hàng thất bại", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }

                ShowData();//update lai ds 

          
                txtTenMatHang.Text = null;
                txtDonGiaBan.Text = "0";
                cbbDVT.SelectedIndex = -1;
            }
        }

        private void txtTiLe_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void rbtMatHang_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtMatHang.Checked)
            {
                cbbMatHangGoc.Enabled = true;
                txtTiLe.Enabled = true;
            }
            else
            {
                cbbMatHangGoc.Enabled = false;
                cbbMatHangGoc.SelectedIndex = -1;
                txtTiLe.Text = "0";
                txtTiLe.Enabled = false;
            }
        }
    }
}
