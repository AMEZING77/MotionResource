using DC.Resource2.MontionControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DC.Resource2
{
    public partial class MotionConfigForm : Form
    {
        private AddressCatalogDbRepository _catalog;
        private EquipmentMotionMechanismDbRepository _emmRepository;
        private BindingList<AddressRecordVM> _addressList;
        private BindingList<BindingItem> _emmList = new BindingList<BindingItem>();
        public MotionConfigForm()
        {
            InitializeComponent();
        }

        private void MotionConfigForm_Load(object sender, EventArgs e)
        {
            _catalog = new AddressCatalogDbRepository();
            _emmRepository = new EquipmentMotionMechanismDbRepository();
            _addressList = QueryAddressList();
            dgvAddressCatalog.DataSource = _addressList;
            lsbMotionMecha.DataSource = _emmList;
            BindingToEmmListbox();
            lsbMotionMecha.SelectedIndexChanged += LsbMotionMecha_SelectedIndexChanged;
        }

        private BindingList<AddressRecordVM> QueryAddressList()
        {
            var result = new BindingList<AddressRecordVM>();
            var mmList = _emmRepository.List().ToDictionary(mm => mm.Id, mm => mm.GetDescription());
            foreach (var item in _catalog.List())
            {
                var target = new AddressRecordVM
                {
                    Address = item.Address,
                    FuncCode = item.FuncCode,
                    Id = item.Id,
                    IOType = item.IOType,
                    IsEnable = item.IsEnable,
                    MechanismId = item.MechanismId,
                };
                target.MotionMechDesciption = mmList.ContainsKey(item.MechanismId) ? mmList[item.MechanismId] : "未关联";
                result.Add(target);
            }
            return result;
        }

        private void BindingToEmmListbox()
        {
            var mmList = _emmRepository.List();
            _emmList?.Clear();
            foreach (var item in mmList.Select(mm => new BindingItem { Name = mm.GetDescription(), Value = mm.Id }))
            {
                _emmList.Add(item);
            }
            LsbMotionMecha_SelectedIndexChanged(this, null);
        }

        private void LsbMotionMecha_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsbMotionMecha.SelectedValue == null)
            {
                txbIpAddr.Text = string.Empty;
                txbMechaType.Text = string.Empty;
                txbOem.Text = string.Empty;
                txbPort.Text = string.Empty;
                txbProtocol.Text = string.Empty;
                txbSeries.Text = string.Empty;
                return;
            }
            var target = _emmRepository.Get((int)(lsbMotionMecha.SelectedValue));
            if (target != null)
            {
                txbIpAddr.Text = target.IpAddress;
                txbMechaType.Text = target.MechanismType.GetDescription();
                txbOem.Text = target.Oem.GetDescription();
                txbPort.Text = target.Oem.ToString();
                txbProtocol.Text = target.Protocol.GetDescription();
                txbSeries.Text = target.Series;
            }
        }

        private void btnDelAddress_Click(object sender, EventArgs e)
        {
            if (dgvAddressCatalog.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选中一行地址数据!", "提示");
                return;
            }
            var dialogRes = MessageBox.Show("确定删除选中的行吗？", "确认", MessageBoxButtons.YesNo);
            if (dialogRes == DialogResult.Yes)
            {
                var address = dgvAddressCatalog.SelectedRows[0].DataBoundItem as AddressRecordVM;
                if (address != null)
                {
                    _catalog.Delete(address.Id);
                    _addressList.Remove(address);
                    MessageBox.Show($"删除功能码{address.FuncCode}成功!");
                }
            }
        }

        private void btnAddAddress_Click(object sender, EventArgs e)
        {
            if (_emmRepository.List().Count == 0)
            {
                MessageBox.Show("当前尚未创建运动控制机构，请先创建一个运动控制机构以便于地址条目绑定!", "提示", MessageBoxButtons.OK);
                return;
            }

            var form = new CreateUpdateAddressForm();
            form.ShowDialog();

            _addressList = QueryAddressList();
            dgvAddressCatalog.DataSource = _addressList;
        }

        private void btnEditAddress_Click(object sender, EventArgs e)
        {
            if (dgvAddressCatalog.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选中一行地址数据!", "提示");
                return;
            }
            var address = dgvAddressCatalog.SelectedRows[0].DataBoundItem as AddressRecord;
            var form = new CreateUpdateAddressForm(address);
            var dialogRes = form.ShowDialog();
            if (dialogRes == DialogResult.OK)
            {
                _addressList.Clear();
                _addressList = QueryAddressList();
                dgvAddressCatalog.DataSource = _addressList;
            }
        }

        private void btnAddEmm_Click(object sender, EventArgs e)
        {
            var addForm = new CreateUpdateEmmForm();
            var dialogRes = addForm.ShowDialog();
            if (dialogRes == DialogResult.OK)
            {
                BindingToEmmListbox();
            }
        }

        private void btnEditEmm_Click(object sender, EventArgs e)
        {
            if (lsbMotionMecha.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选中某个运动控制机构!", "提示");
                return;
            }
            var target = _emmRepository.Get((int)lsbMotionMecha.SelectedValue);
            if (target == null) { MessageBox.Show("选中的运动控制机制不存在，请重新打开UI尝试!", "提示"); }
            var editForm = new CreateUpdateEmmForm(target);
            var dialogRes = editForm.ShowDialog();
            if (dialogRes == DialogResult.OK)
            {
                BindingToEmmListbox();
            }
        }

        private void btnDelEmm_Click(object sender, EventArgs e)
        {
            if (lsbMotionMecha.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选中某个运动控制机构!", "提示");
                return;
            }
            var dialogRes = MessageBox.Show("确定删除选中的运动控制机构吗，删除运动控制机构也会同时关联的地址？", "确认", MessageBoxButtons.YesNo);
            if (dialogRes == DialogResult.Yes)
            {
                var item = lsbMotionMecha.SelectedItem as BindingItem;
                _emmRepository.Delete(item.Value);
                _emmList.Remove(item);
                _addressList = QueryAddressList();
                dgvAddressCatalog.DataSource = _addressList;
                MessageBox.Show($"删除成功!");
            }
        }
    }

    internal class AddressRecordVM : AddressRecord
    {
        [DisplayName("所属运动控制机构")]
        public string MotionMechDesciption { get; set; }
    }
}
