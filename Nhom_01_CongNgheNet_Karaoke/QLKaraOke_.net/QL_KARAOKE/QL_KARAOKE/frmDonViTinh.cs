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
    public partial class frmDonViTinh : Form
    {
        public frmDonViTinh(string nv)
        {
            this.nhanvien = nv;
            InitializeComponent();
        }

        private KARAOKE_DatabaseDataContext db;
        private string nhanvien;
        private void frmDonViTinh_Load(object sender, EventArgs e)
        {
            db = new KARAOKE_DatabaseDataContext(); 
            ShowData();
            dgvDVT.Columns["ID"].HeaderText = "Mã ĐVT";
            dgvDVT.Columns["ID"].Width = 100;
            dgvDVT.Columns["ID"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;//Căn giữa ô
            dgvDVT.Columns["TenDVT"].HeaderText = "Tên ĐVT";
            dgvDVT.Columns["TenDVT"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;//autosize abc :v
        }
        private void ShowData()
        {
            var rs = (from d in db.DonViTinhs.Where(x=>x.isDelete == 0)
                      select new
                      {
                          d.ID,
                          d.TenDVT
                      }).ToList();
            dgvDVT.DataSource = rs; 
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtDVT.Text))//neu txtDVT khong null
            {
                DonViTinh dvt = new DonViTinh();
                dvt.TenDVT = txtDVT.Text;      
                dvt.NguoiTao = nhanvien;
                dvt.NgayTao = DateTime.Now;
                dvt.isDelete = 0;
                db.DonViTinhs.InsertOnSubmit(dvt);//luu vao database
                db.SubmitChanges();
                MessageBox.Show("Thêm mới đơn vị tính thành công!", "Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowData();
                txtDVT.Text = null;
            }
            else
            {
                MessageBox.Show("Vui lòng nhập đơn vị tính", "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            txtDVT.Select();
        }

        private DataGridViewRow r;
        private void dgvDVT_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            
            r = dgvDVT.Rows[e.RowIndex];
            

            txtDVT.Text = r.Cells["TenDVT"].Value.ToString();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (r == null)
            {
                MessageBox.Show("Vui lòng chọn đơn vị tính cần cập nhật", "Chú ý!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!string.IsNullOrEmpty(txtDVT.Text))
            {
                var dvt = db.DonViTinhs.SingleOrDefault(x => x.ID == int.Parse(r.Cells["id"].Value.ToString()));// ep kieu
                
                dvt.TenDVT = txtDVT.Text;
                dvt.NgayCapNhat = DateTime.Now;
                dvt.NguoiCapNhat = nhanvien;
                db.SubmitChanges();//lưu vào csdl
                MessageBox.Show("Cập nhật đơn vị thành công !", "Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowData();
                txtDVT.Text = null;
                r = null;
            }
            else
            {
                MessageBox.Show("Vui lòng nhập tên đơn vị tính", "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if(r == null)
            {
                MessageBox.Show("Vui lòng chọn đơn vị tính cần xóa", "Chú ý!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("Bạn thực sự muốn xóa đơn vị tính " + r.Cells["TenDVT"].Value.ToString() + "?",
                "Xác nhận xóa", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var dvt = db.DonViTinhs.SingleOrDefault(x => x.ID == int.Parse(r.Cells["id"].Value.ToString()));
                dvt.isDelete = 1;
                db.SubmitChanges();
                MessageBox.Show("Xóa đơn vị tính thành công!", "Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowData();
                r = null;
            }
        }
    }
}
