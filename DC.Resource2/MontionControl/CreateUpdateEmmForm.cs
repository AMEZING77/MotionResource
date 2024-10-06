using HslCommunication.Secs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DC.Resource2.MontionControl
{
    public partial class CreateUpdateEmmForm : Form
    {
        private MotionMechanism _target;
        private bool _isEdit = false;
        private List<BindingItem<OEM>> _oemBindingItems;
        private BindingList<BindingItem<Protocol>> _bindingProtocol = new BindingList<BindingItem<Protocol>>();
        private BindingList<BindingItem<string>> _bindingSeries = new BindingList<BindingItem<string>>();

        public CreateUpdateEmmForm()
        {
            InitializeComponent();
        }

        public CreateUpdateEmmForm(MotionMechanism target)
        {
            InitializeComponent();
            _target = target;
            _isEdit = true;
        }

        private void AddUpdateMotionMechanism_Load(object sender, EventArgs e)
        {
            this.Text = !_isEdit ? "新增运动控制机构" : "编辑运动控制机构";
            //txbIpAddr.Mask = "###.###.###.###";
            txbIpAddr.ValidatingType = typeof(System.Net.IPAddress);

            _oemBindingItems = EnumExtensions.ToBindingItems<OEM>();
            cmbEmmType.DataSource = EnumExtensions.ToBindingItems<MechanismType>();
            cmbEmmType.SelectedIndexChanged += CmbEmmType_SelectedIndexChanged;
            cmbOem.DataSource = _oemBindingItems;
            cmbProtocol.DataSource = _bindingProtocol;
            cmbSeries.DataSource = _bindingSeries;
            cmbOem.SelectedIndexChanged += CmbOem_SelectedIndexChanged;
            if (_isEdit)
            {
                cmbEmmType.SelectedValue = _target.MechanismType;
                cmbOem.SelectedValue = _target.Oem;
                Refill(_bindingProtocol, PlcControllerFactory.SupportedProtocol[_target.Oem]);
                cmbProtocol.SelectedValue = _target.Protocol;
                Refill(_bindingSeries, PlcControllerFactory.SupportedSeries[_target.Oem]);
                cmbSeries.SelectedValue = _target.Series;
                txbCode.Text = _target.Code;
                txbIpAddr.Text = _target.IpAddress;
                nudPort.Value = _target.Port;
            }
            else
            {
                cmbEmmType.SelectedValue = MechanismType.Plc;
                cmbOem.SelectedValue = OEM.PlcInovance;
                Refill(_bindingProtocol, PlcControllerFactory.SupportedProtocol[OEM.PlcInovance]);
                Refill(_bindingSeries, PlcControllerFactory.SupportedSeries[OEM.PlcInovance]);
                nudPort.Value = 502;
            }

        }

        private void CmbEmmType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var mt = (MechanismType)cmbEmmType.SelectedValue;
            cmbOem.DataSource = _oemBindingItems
                .Where(ob => mt == MechanismType.Plc ? ob.Value.IsPlcOem() : !ob.Value.IsPlcOem())
                .ToList();
        }

        private void CmbOem_SelectedIndexChanged(object sender, EventArgs e)
        {
            var oem = (OEM)cmbOem.SelectedValue;
            if (oem.IsPlcOem())
            {
                Refill(_bindingProtocol, PlcControllerFactory.SupportedProtocol[oem]);
                Refill(_bindingSeries, PlcControllerFactory.SupportedSeries[oem]);
            }
            else
            {
                _bindingProtocol.Clear();
                _bindingProtocol.Add(new BindingItem<Protocol> { Name = "未指定", Value = Protocol.Unknown });
                _bindingSeries.Clear();
                _bindingSeries.Add(new BindingItem<string> { Name = "未指定", Value = "未指定" });
            }
        }

        private void Refill<T>(BindingList<BindingItem<T>> bindingList, IEnumerable<T> items)
        {
            bindingList.Clear();
            foreach (T item in items) { bindingList.Add(new BindingItem<T> { Name = item.ToString(), Value = item }); }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            MotionMechanism target = ConstructFromUI();
            var emmValidator = new EmmValidator(new EquipmentMotionMechanismDbRepository());
            var validationRes = emmValidator.Validate(target, _isEdit);
            if (validationRes.Count > 0)
            {
                MessageBox.Show(string.Join(";\r\n", validationRes), "提示");
                return;
            }
            var respository = new EquipmentMotionMechanismDbRepository();
            if (_isEdit) { respository.Update(target); }
            else { respository.Add(target); }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private MotionMechanism ConstructFromUI()
        {
            return new MotionMechanism
            {
                Id = _isEdit ? _target.Id : 0,
                Code = txbCode.Text,
                IpAddress = txbIpAddr.Text,
                MechanismType = (MechanismType)cmbEmmType.SelectedValue,
                Oem = (OEM)cmbOem.SelectedValue,
                Port = (ushort)nudPort.Value,
                Protocol = (Protocol)cmbProtocol.SelectedValue,
                Series = (string)cmbSeries.SelectedValue,
            };
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (!_isEdit)
            {
                this.Close();
                return;
            }
            var target = ConstructFromUI();
            if (!_target.Equals(target))
            {
                var dialogRes = MessageBox.Show("目标已经被修改，确定要退出吗?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogRes == DialogResult.Yes) { this.Close(); }
            }
            else { this.Close(); }
        }
    }
}
