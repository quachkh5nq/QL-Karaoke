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
    public partial class frmTonKho : Form
    {
        public frmTonKho()
        {
            InitializeComponent();
        }

        private KARAOKE_DatabaseDataContext db;
        private void frmTonKho_Load(object sender, EventArgs e)
        {
            db = new KARAOKE_DatabaseDataContext();
            btnThongKe.PerformClick();//goi sự kiện click btnThongKe khi form được load
            dgvTonKho.Columns["mahang"].HeaderText = "Mã Hàng";
            dgvTonKho.Columns["tenhang"].HeaderText = "Mặt Hàng";
            dgvTonKho.Columns["dvt"].HeaderText = "ĐVT";
            dgvTonKho.Columns["tonkho"].HeaderText = "Tồn Kho";

            dgvTonKho.Columns["isDichVu"].Visible = false;
            dgvTonKho.Columns["dg"].Visible = false;

            dgvTonKho.Columns["tenhang"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

        }

        private void btnThongKe_Click(object sender, EventArgs e)
        {
            if (rbtDaHet.Checked)
            {
                ThongKe(0);
                return;
            }
            if(rbtGanHet.Checked)
            {
                ThongKe(-1);
                return;
            }
            if(rbtTatCa.Checked)
            {
                ThongKe(1);
                return;
            }
        }
        private void ThongKe(int dieukien)
        {
            #region ton_kho_cha
            
            var details = from ct in db.ChiTietHoaDonNhaps
                          join hd in db.HoaDonNhaps.Where(x => x.DaNhap == 1)// lấy các hóa đơn có trạng thái đã nhập là 1
                          on ct.IDHoaDon equals hd.ID
                          select new
                          {
                              mahang = ct.IDMatHang,
                              sl = ct.SoLuong
                          };
            var nhapCha = from ct in details.GroupBy(x => x.mahang)
                          join h in db.MatHangs.Where(x => x.idcha == null || x.idcha <= 0) on ct.First().mahang equals h.ID
                          join d in db.DonViTinhs on h.DVT equals d.ID

                          select new
                          {
                              mahang = ct.First().mahang,
                              tenhang = h.TenMatHang,
                              dvt = d.TenDVT,
                              dg = h.DonGiaBan,
                              soluong = ct.Sum(x => x.sl) //lấy tổng số lượng
                          };

            var xuatCha = from p in db.ChiTietHoaDonBans.GroupBy(x => x.IDMatHang)
                          join h in db.MatHangs.Where(x => x.idcha == null || x.idcha <= 0 && x.isDichVu == 0)////lấy tổng số lượng của các mặt hàng k là con của mặt hàng khác: idCha = null hoặc idCha <=0
                          on p.First().IDMatHang equals h.ID
                          select new
                          {
                              mahang = h.ID,
                              soluong = p.Sum(x => x.SL)
                          };
            
            var xuatQuyRaCha = from ct in db.ChiTietHoaDonBans.GroupBy(x => x.IDMatHang)
                               join h in db.MatHangs.Where(x => x.idcha > 0)//lấy các mặt hàng con
                               on ct.First().IDMatHang equals h.ID
                               select new
                               {
                                   mahang = (int)h.idcha,
                                   soluong = ct.Sum(x => x.SL) % h.Tile == 0 ? ct.Sum(x => x.SL) / h.Tile : ct.Sum(x => x.SL) / h.Tile + 1
                               };


            //tính tổng toàn bộ mặt hàng cha đã bán ra dựa vào kết quả thu được 
            var tongXuatCha = from xc in xuatCha.Union(xuatQuyRaCha).GroupBy(x => x.mahang)
                              select new
                              {
                                  mahang = xc.First().mahang,
                                  soluong = xc.Sum(x => x.soluong)
                              };
            
            var tonKhoCha = from p in nhapCha
                            join q in tongXuatCha on p.mahang equals q.mahang into tmp
                            from t in tmp.DefaultIfEmpty()
                            select new
                            {
                                mahang = p.mahang,
                                tenhang = p.tenhang,
                                isDichvu = 0,
                                dvt = p.dvt,
                                dg = p.dg,
                                tonkho = (int)(p.soluong - (t == null ? 0 : t.soluong)) //nhập - xuất
                            };


            #endregion

            #region ton_kho_con
     
            var nhapCon = from ct in nhapCha
                          join h in db.MatHangs on ct.mahang equals h.idcha 
                          join d in db.DonViTinhs on h.DVT equals d.ID
                          select new
                          {
                              mahang = h.ID,
                              tenhang = h.TenMatHang,
                              dvt = d.TenDVT,
                              dg = h.DonGiaBan,
                              soluong = ct.soluong * h.Tile
                          };
  
            var xuatConQuyTuCha = from xc in xuatCha
                                  join h in db.MatHangs.Where(x => x.idcha > 0)
                                  on xc.mahang equals h.idcha
                                  select new
                                  {
                                      mahang = h.ID,
                                      soluong = xc.soluong * h.Tile
                                  };

         
            var xuatCon = from ct in db.ChiTietHoaDonBans.GroupBy(x => x.IDMatHang)
                          join h in db.MatHangs.Where(x => x.idcha > 0 && x.isDichVu == 0)
                          on ct.First().IDMatHang equals h.ID
                          select new
                          {
                              mahang = h.ID,
                              soluong = ct.Sum(x => x.SL)
                          };

            

            var tongConXuat = from ct in xuatConQuyTuCha.Union(xuatCon).GroupBy(x => x.mahang)
                              select new
                              {
                                  mahang = ct.First().mahang,
                                  slXuat = ct.Sum(x => x.soluong)
                              };

            
            var tonKhoCon = from nc in nhapCon
                            join xc in tongConXuat on nc.mahang equals xc.mahang into tmp
                            from x in tmp.DefaultIfEmpty()
                            select new
                            {
                                mahang = nc.mahang,
                                tenhang = nc.tenhang,
                                isDichvu = 0,
                                dvt = nc.dvt,
                                dg = nc.dg,
                                tonkho = (int)(nc.soluong - (x == null ? 0 : x.slXuat))
                            };


            #endregion


            
            var tonkhoHang = tonKhoCha.Concat(tonKhoCon).OrderBy(x => x.tenhang);
            if(dieukien == 0)
            {
                var result = tonkhoHang.Where(x => x.tonkho == 0);
                dgvTonKho.DataSource = result;
                return;
            }

            if(dieukien == 1)
            {
                dgvTonKho.DataSource = tonkhoHang;
                return;
            }

            if(dieukien == -1)
            {
                var ganhet = int.Parse(db.CauHinhs.SingleOrDefault(x => x.tukhoa == "ganhet").giatri);
                var result = tonkhoHang.Where(x => x.tonkho < ganhet);
                dgvTonKho.DataSource = result;
                return;
            }
        }

    }
}
