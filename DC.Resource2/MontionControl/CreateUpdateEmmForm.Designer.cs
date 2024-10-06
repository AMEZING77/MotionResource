namespace DC.Resource2.MontionControl
{
    partial class CreateUpdateEmmForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.cmbEmmType = new System.Windows.Forms.ComboBox();
            this.txbCode = new System.Windows.Forms.MaskedTextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txbIpAddr = new System.Windows.Forms.MaskedTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.nudPort = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbProtocol = new System.Windows.Forms.ComboBox();
            this.cmbSeries = new System.Windows.Forms.ComboBox();
            this.cmbOem = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(80, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "运动控制机构类别";
            // 
            // cmbEmmType
            // 
            this.cmbEmmType.DisplayMember = "Name";
            this.cmbEmmType.FormattingEnabled = true;
            this.cmbEmmType.Location = new System.Drawing.Point(200, 27);
            this.cmbEmmType.Name = "cmbEmmType";
            this.cmbEmmType.Size = new System.Drawing.Size(121, 20);
            this.cmbEmmType.TabIndex = 1;
            this.cmbEmmType.ValueMember = "Value";
            // 
            // txbCode
            // 
            this.txbCode.Location = new System.Drawing.Point(200, 172);
            this.txbCode.Name = "txbCode";
            this.txbCode.Size = new System.Drawing.Size(121, 21);
            this.txbCode.TabIndex = 3;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(443, 296);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "确认";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(338, 296);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(134, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "OEM厂商";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(128, 102);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "产品系列";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(140, 218);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "IP地址";
            // 
            // txbIpAddr
            // 
            this.txbIpAddr.Location = new System.Drawing.Point(200, 215);
            this.txbIpAddr.Name = "txbIpAddr";
            this.txbIpAddr.Size = new System.Drawing.Size(121, 21);
            this.txbIpAddr.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(146, 254);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "端口号";
            // 
            // nudPort
            // 
            this.nudPort.Location = new System.Drawing.Point(200, 252);
            this.nudPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudPort.Name = "nudPort";
            this.nudPort.Size = new System.Drawing.Size(121, 21);
            this.nudPort.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(128, 139);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 12;
            this.label6.Text = "通信协议";
            // 
            // cmbProtocol
            // 
            this.cmbProtocol.DisplayMember = "Name";
            this.cmbProtocol.FormattingEnabled = true;
            this.cmbProtocol.Location = new System.Drawing.Point(200, 136);
            this.cmbProtocol.Name = "cmbProtocol";
            this.cmbProtocol.Size = new System.Drawing.Size(121, 20);
            this.cmbProtocol.TabIndex = 13;
            this.cmbProtocol.ValueMember = "Value";
            // 
            // cmbSeries
            // 
            this.cmbSeries.DisplayMember = "Name";
            this.cmbSeries.FormattingEnabled = true;
            this.cmbSeries.Location = new System.Drawing.Point(200, 99);
            this.cmbSeries.Name = "cmbSeries";
            this.cmbSeries.Size = new System.Drawing.Size(121, 20);
            this.cmbSeries.TabIndex = 14;
            this.cmbSeries.ValueMember = "Value";
            // 
            // cmbOem
            // 
            this.cmbOem.DisplayMember = "Name";
            this.cmbOem.FormattingEnabled = true;
            this.cmbOem.Location = new System.Drawing.Point(200, 63);
            this.cmbOem.Name = "cmbOem";
            this.cmbOem.Size = new System.Drawing.Size(121, 20);
            this.cmbOem.TabIndex = 15;
            this.cmbOem.ValueMember = "Value";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(152, 175);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 12);
            this.label7.TabIndex = 16;
            this.label7.Text = "编号";
            // 
            // CreateUpdateEmmForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 331);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.cmbOem);
            this.Controls.Add(this.cmbSeries);
            this.Controls.Add(this.cmbProtocol);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.nudPort);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txbIpAddr);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txbCode);
            this.Controls.Add(this.cmbEmmType);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateUpdateEmmForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.AddUpdateMotionMechanism_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbEmmType;
        private System.Windows.Forms.MaskedTextBox txbCode;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.MaskedTextBox txbIpAddr;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nudPort;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbProtocol;
        private System.Windows.Forms.ComboBox cmbSeries;
        private System.Windows.Forms.ComboBox cmbOem;
        private System.Windows.Forms.Label label7;
    }
}