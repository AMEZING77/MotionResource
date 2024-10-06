using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DC.Resource2.MontionControl
{
    public partial class CreateUpdateAddressForm : Form
    {
        private List<MotionMechanism> _mmList;
        private AddressRecord _target;
        private bool _isEdit = false;
        public CreateUpdateAddressForm()
        {
            InitializeComponent();
        }

        public CreateUpdateAddressForm(AddressRecord addressRecord)
        {
            InitializeComponent();
            _target = addressRecord;
            _isEdit = true;
        }

        private void CreateUpdateAddress_Load(object sender, EventArgs e)
        {
            this.Text = _target != null ? "编辑地址" : "新建地址";

            var emmRepo = new EquipmentMotionMechanismDbRepository();
            _mmList = emmRepo.List();

            cmbMotionMecha.DataSource = _mmList
                .Select(mm => new BindingItem { Name = mm.GetDescription(), Value = mm.Id })
                .ToList();
            if (_mmList.Count == 1) { cmbMotionMecha.SelectedIndex = 0; }
            cmbIoType.DataSource = Enum.GetValues(typeof(IOType)).OfType<IOType>()
                .Select(eit => new BindingItem { Name = eit.ToString(), Value = (int)eit })
                .ToList();

            var customSource = new AutoCompleteStringCollection();
            foreach (var fc in typeof(FuncCodes).GetFields())
            {
                customSource.Add(fc.GetValue(null) as string);
            }
            cmbFuncCode.AutoCompleteCustomSource = customSource;
            if (_target != null)
            {
                var mm = _mmList.Where(m => m.Id == _target.MechanismId).FirstOrDefault();
                if (mm != null) { cmbMotionMecha.SelectedValue = mm.Id; }
                cmbFuncCode.Text = _target.FuncCode;
                txbAddress.Text = _target.Address;
                cmbIoType.SelectedValue = (int)_target.IOType;
                chbEnable.Checked = _target.IsEnable;
            }
            else { chbEnable.Checked = true; }

            cmbMotionMecha.SelectedIndexChanged += CmbMotionMecha_SelectedIndexChanged;
        }

        private void CmbMotionMecha_SelectedIndexChanged(object sender, EventArgs e)
        {
            //对于PLC类型的控制机构，IO类型固定为通用类型
            var id = Convert.ToInt32(cmbMotionMecha.SelectedValue);
            var mm = _mmList.Where(m => m.Id == id).FirstOrDefault();
            if (mm != null && mm.MechanismType == MechanismType.Plc)
            {
                cmbIoType.SelectedValue = IOType.Common.ToString();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            AddressRecord target = ConstructFromUI();
            var repo = new AddressCatalogDbRepository();
            var validator = new AddressValidator(repo);
            var validationRes = validator.Validate(target, _isEdit);
            if (validationRes.Any())
            {
                MessageBox.Show(string.Join(";\r\n", validationRes), "提示", MessageBoxButtons.OK);
                return;
            }
            if (_target != null)
            {
                repo.Update(target);
            }
            else
            {
                repo.Add(target);
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private AddressRecord ConstructFromUI()
        {
            return new AddressRecord
            {
                Address = txbAddress.Text,
                FuncCode = cmbFuncCode.Text,
                IOType = (IOType)cmbIoType.SelectedValue,
                IsEnable = chbEnable.Checked,
                MechanismId = Convert.ToInt32(cmbMotionMecha.SelectedValue),
                Id = _target != null ? _target.Id : 0,
            };
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (!_isEdit) { return; }
            var target = ConstructFromUI();
            if (!_target.Equals(target))
            {
                var dialogRes = MessageBox.Show("目标已经被修改，确定要退出吗?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogRes == DialogResult.Yes) { this.Close(); }
            }
            else { this.Close(); }
        }
    }

    internal class BindingItem
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
