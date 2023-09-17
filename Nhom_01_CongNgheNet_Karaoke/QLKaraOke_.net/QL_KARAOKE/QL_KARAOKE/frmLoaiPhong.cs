﻿using QL_KARAOKE.DB;
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
    public partial class frmLoaiPhong : Form
    {
        public frmLoaiPhong(string nv)
        {
            this.nhanvien = nv;
            InitializeComponent();
        }
        private string nhanvien;
        private KARAOKE_DatabaseDataContext db;

        private void frmLoaiPhong_Load(object sender, EventArgs e)
        {
            db = new KARAOKE_DatabaseDataContext();
            ShowData();

            //edit cột
            dgvLoaiPhong.Columns["id"].HeaderText = "Mã Loại";
            dgvLoaiPhong.Columns["TenLoaiPhong"].HeaderText = "Tên Loại Phòng";
            dgvLoaiPhong.Columns["DonGia"].HeaderText = "Đơn Giá";

            dgvLoaiPhong.Columns["id"].Width = 100;
            dgvLoaiPhong.Columns["DonGia"].Width = 200;
            dgvLoaiPhong.Columns["TenLoaiPhong"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            

            dgvLoaiPhong.Columns["DonGia"].DefaultCellStyle.Format = "N0"; 

        }

        private void ShowData()
        {
            

            var rs = from l in db.LoaiPhongs.Where(x=>x.isDelete == 0)
                     select new
                     {
                         l.ID,
                         l.TenLoaiPhong,
                         l.DonGia
                     };
            dgvLoaiPhong.DataSource = rs.ToList();
        }

        
        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTenLoaiPhong.Text))
            {
                MessageBox.Show("Vui lòng nhập tên loại phòng", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenLoaiPhong.Select();
                return;
            }
            if (string.IsNullOrEmpty(txtDonGia.Text))
            {
                MessageBox.Show("Vui lòng nhập đơn giá", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDonGia.Select();
                return;
            }
            else
            {
                LoaiPhong l = new LoaiPhong();
                l.TenLoaiPhong = txtTenLoaiPhong.Text;
                l.DonGia = int.Parse(txtDonGia.Text);
                l.NgayTao = DateTime.Now;
                l.NguoiTao = nhanvien;
                l.isDelete = 0;

                db.LoaiPhongs.InsertOnSubmit(l);//thêm vào csdl
                db.SubmitChanges();
                MessageBox.Show("Thêm mới loại phòng thành công", "Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowData();

              
                txtTenLoaiPhong.Text = null;
                txtDonGia.Text = "0";
            }
        }

        private DataGridViewRow r;
        
        private void btnSua_Click(object sender, EventArgs e)
        {   
            
            if(r == null)
            {
                MessageBox.Show("Vui lòng chọn loại phòng cần cập nhật", "Chú ý!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if(r != null)
            {
                var l = db.LoaiPhongs.SingleOrDefault(x => x.ID == int.Parse(r.Cells["ID"].Value.ToString()));
                l.TenLoaiPhong = txtTenLoaiPhong.Text;
                l.DonGia = int.Parse(txtDonGia.Text);

                l.NgayCapNhat = DateTime.Now;
                l.NguoiCapNhat = nhanvien;

                db.SubmitChanges();
                MessageBox.Show("Cập nhật loại phòng thành công", "Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowData();
            }
           
        }


        
        private void btnXoa_Click(object sender, EventArgs e)
        {
            if(r == null)
            {
                MessageBox.Show("Vui lòn chọn loại phòng cần xóa", "Chú ý!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var l = db.LoaiPhongs.SingleOrDefault(x => x.ID == int.Parse(r.Cells["ID"].Value.ToString()));
            if(MessageBox.Show("Bạn có muốn xóa loại "+r.Cells["TenLoaiPhong"].Value.ToString()+"?",
                "Xác nhận xóa",MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes
                )
            {
                try
                {
                    var lp = db.LoaiPhongs.SingleOrDefault(x => x.ID == int.Parse(r.Cells["ID"].Value.ToString()));
                    lp.isDelete = 1;
                    MessageBox.Show("Xóa thành công","Successfully",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    db.SubmitChanges();
                    ShowData();
                    
                    txtTenLoaiPhong.Text = null;
                    txtDonGia.Text = "0";
                    r = null;
                }
                catch
                {
                    
                    MessageBox.Show("Xóa thất bại", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        
        private void dgvLoaiPhong_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                r = dgvLoaiPhong.Rows[e.RowIndex];
                txtTenLoaiPhong.Text = r.Cells["TenLoaiPhong"].Value.ToString();
                txtDonGia.Text = r.Cells["DonGia"].Value.ToString();
            }
        }

        
        private void txtDonGia_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void dgvLoaiPhong_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtDonGia_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void txtTenLoaiPhong_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
