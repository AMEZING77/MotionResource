namespace DC.Resource2
{
    partial class MotionConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgvAddressCatalog = new System.Windows.Forms.DataGridView();
            this.btnAddAddress = new System.Windows.Forms.Button();
            this.btnEditAddress = new System.Windows.Forms.Button();
            this.btnDelAddress = new System.Windows.Forms.Button();
            this.btnAddEmm = new System.Windows.Forms.Button();
            this.btnDelEmm = new System.Windows.Forms.Button();
            this.btnEditEmm = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txbPort = new System.Windows.Forms.TextBox();
            this.txbIpAddr = new System.Windows.Forms.TextBox();
            this.txbSeries = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txbProtocol = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txbOem = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txbMechaType = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lsbMotionMecha = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAddressCatalog)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvAddressCatalog
            // 
            this.dgvAddressCatalog.AllowUserToAddRows = false;
            this.dgvAddressCatalog.AllowUserToDeleteRows = false;
            this.dgvAddressCatalog.AllowUserToOrderColumns = true;
            this.dgvAddressCatalog.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAddressCatalog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAddressCatalog.Location = new System.Drawing.Point(12, 133);
            this.dgvAddressCatalog.Name = "dgvAddressCatalog";
            this.dgvAddressCatalog.ReadOnly = true;
            this.dgvAddressCatalog.RowTemplate.Height = 23;
            this.dgvAddressCatalog.Size = new System.Drawing.Size(779, 377);
            this.dgvAddressCatalog.TabIndex = 0;
            // 
            // btnAddAddress
            // 
            this.btnAddAddress.Location = new System.Drawing.Point(828, 133);
            this.btnAddAddress.Name = "btnAddAddress";
            this.btnAddAddress.Size = new System.Drawing.Size(75, 23);
            this.btnAddAddress.TabIndex = 1;
            this.btnAddAddress.Text = "添加地址";
            this.btnAddAddress.UseVisualStyleBackColor = true;
            this.btnAddAddress.Click += new System.EventHandler(this.btnAddAddress_Click);
            // 
            // btnEditAddress
            // 
            this.btnEditAddress.Location = new System.Drawing.Point(828, 176);
            this.btnEditAddress.Name = "btnEditAddress";
            this.btnEditAddress.Size = new System.Drawing.Size(75, 23);
            this.btnEditAddress.TabIndex = 2;
            this.btnEditAddress.Text = "编辑地址";
            this.btnEditAddress.UseVisualStyleBackColor = true;
            this.btnEditAddress.Click += new System.EventHandler(this.btnEditAddress_Click);
            // 
            // btnDelAddress
            // 
            this.btnDelAddress.Location = new System.Drawing.Point(828, 224);
            this.btnDelAddress.Name = "btnDelAddress";
            this.btnDelAddress.Size = new System.Drawing.Size(75, 23);
            this.btnDelAddress.TabIndex = 3;
            this.btnDelAddress.Text = "删除地址";
            this.btnDelAddress.UseVisualStyleBackColor = true;
            this.btnDelAddress.Click += new System.EventHandler(this.btnDelAddress_Click);
            // 
            // btnAddEmm
            // 
            this.btnAddEmm.Location = new System.Drawing.Point(672, 11);
            this.btnAddEmm.Name = "btnAddEmm";
            this.btnAddEmm.Size = new System.Drawing.Size(87, 25);
            this.btnAddEmm.TabIndex = 5;
            this.btnAddEmm.Text = "新增控制机构";
            this.btnAddEmm.UseVisualStyleBackColor = true;
            this.btnAddEmm.Click += new System.EventHandler(this.btnAddEmm_Click);
            // 
            // btnDelEmm
            // 
            this.btnDelEmm.Location = new System.Drawing.Point(672, 93);
            this.btnDelEmm.Name = "btnDelEmm";
            this.btnDelEmm.Size = new System.Drawing.Size(87, 25);
            this.btnDelEmm.TabIndex = 6;
            this.btnDelEmm.Text = "删除控制机构";
            this.btnDelEmm.UseVisualStyleBackColor = true;
            this.btnDelEmm.Click += new System.EventHandler(this.btnDelEmm_Click);
            // 
            // btnEditEmm
            // 
            this.btnEditEmm.Location = new System.Drawing.Point(672, 50);
            this.btnEditEmm.Name = "btnEditEmm";
            this.btnEditEmm.Size = new System.Drawing.Size(87, 25);
            this.btnEditEmm.TabIndex = 7;
            this.btnEditEmm.Text = "编辑控制机构";
            this.btnEditEmm.UseVisualStyleBackColor = true;
            this.btnEditEmm.Click += new System.EventHandler(this.btnEditEmm_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txbPort);
            this.groupBox1.Controls.Add(this.txbIpAddr);
            this.groupBox1.Controls.Add(this.txbSeries);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txbProtocol);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txbOem);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txbMechaType);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(236, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(383, 121);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            // 
            // txbPort
            // 
            this.txbPort.Location = new System.Drawing.Point(267, 91);
            this.txbPort.Name = "txbPort";
            this.txbPort.ReadOnly = true;
            this.txbPort.Size = new System.Drawing.Size(100, 21);
            this.txbPort.TabIndex = 11;
            // 
            // txbIpAddr
            // 
            this.txbIpAddr.Location = new System.Drawing.Point(267, 50);
            this.txbIpAddr.Name = "txbIpAddr";
            this.txbIpAddr.ReadOnly = true;
            this.txbIpAddr.Size = new System.Drawing.Size(100, 21);
            this.txbIpAddr.TabIndex = 10;
            // 
            // txbSeries
            // 
            this.txbSeries.Location = new System.Drawing.Point(267, 12);
            this.txbSeries.Name = "txbSeries";
            this.txbSeries.ReadOnly = true;
            this.txbSeries.Size = new System.Drawing.Size(100, 21);
            this.txbSeries.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(220, 96);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 12);
            this.label6.TabIndex = 8;
            this.label6.Text = "端口号";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(220, 53);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 7;
            this.label5.Text = "IP地址";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(208, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "产品系列";
            // 
            // txbProtocol
            // 
            this.txbProtocol.Location = new System.Drawing.Point(89, 88);
            this.txbProtocol.Name = "txbProtocol";
            this.txbProtocol.ReadOnly = true;
            this.txbProtocol.Size = new System.Drawing.Size(100, 21);
            this.txbProtocol.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "通信协议";
            // 
            // txbOem
            // 
            this.txbOem.Location = new System.Drawing.Point(89, 50);
            this.txbOem.Name = "txbOem";
            this.txbOem.ReadOnly = true;
            this.txbOem.Size = new System.Drawing.Size(100, 21);
            this.txbOem.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "OEM厂商";
            // 
            // txbMechaType
            // 
            this.txbMechaType.Location = new System.Drawing.Point(89, 14);
            this.txbMechaType.Name = "txbMechaType";
            this.txbMechaType.ReadOnly = true;
            this.txbMechaType.Size = new System.Drawing.Size(100, 21);
            this.txbMechaType.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "控制机构类型";
            // 
            // lsbMotionMecha
            // 
            this.lsbMotionMecha.DisplayMember = "Name";
            this.lsbMotionMecha.Font = new System.Drawing.Font("SimSun", 12F);
            this.lsbMotionMecha.FormattingEnabled = true;
            this.lsbMotionMecha.ItemHeight = 16;
            this.lsbMotionMecha.Location = new System.Drawing.Point(12, 10);
            this.lsbMotionMecha.Name = "lsbMotionMecha";
            this.lsbMotionMecha.Size = new System.Drawing.Size(218, 116);
            this.lsbMotionMecha.TabIndex = 10;
            this.lsbMotionMecha.ValueMember = "Value";
            // 
            // MotionConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 523);
            this.Controls.Add(this.lsbMotionMecha);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnEditEmm);
            this.Controls.Add(this.btnDelEmm);
            this.Controls.Add(this.btnAddEmm);
            this.Controls.Add(this.btnDelAddress);
            this.Controls.Add(this.btnEditAddress);
            this.Controls.Add(this.btnAddAddress);
            this.Controls.Add(this.dgvAddressCatalog);
            this.Name = "MotionConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "运动控制配置";
            this.Load += new System.EventHandler(this.MotionConfigForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAddressCatalog)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvAddressCatalog;
        private System.Windows.Forms.Button btnAddAddress;
        private System.Windows.Forms.Button btnEditAddress;
        private System.Windows.Forms.Button btnDelAddress;
        private System.Windows.Forms.Button btnAddEmm;
        private System.Windows.Forms.Button btnDelEmm;
        private System.Windows.Forms.Button btnEditEmm;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox lsbMotionMecha;
        private System.Windows.Forms.TextBox txbPort;
        private System.Windows.Forms.TextBox txbIpAddr;
        private System.Windows.Forms.TextBox txbSeries;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txbProtocol;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txbOem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txbMechaType;
        private System.Windows.Forms.Label label1;
    }
}