namespace DC.Resource2.MontionControl
{
    partial class CreateUpdateAddressForm
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
            this.lblFuncCode = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txbAddress = new System.Windows.Forms.TextBox();
            this.cmbIoType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbMotionMecha = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chbEnable = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cmbFuncCode = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lblFuncCode
            // 
            this.lblFuncCode.AutoSize = true;
            this.lblFuncCode.Location = new System.Drawing.Point(139, 73);
            this.lblFuncCode.Name = "lblFuncCode";
            this.lblFuncCode.Size = new System.Drawing.Size(41, 12);
            this.lblFuncCode.TabIndex = 1;
            this.lblFuncCode.Text = "功能码";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(151, 114);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "地址";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(138, 164);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "IO类型";
            // 
            // txbAddress
            // 
            this.txbAddress.Location = new System.Drawing.Point(221, 111);
            this.txbAddress.Name = "txbAddress";
            this.txbAddress.Size = new System.Drawing.Size(133, 21);
            this.txbAddress.TabIndex = 6;
            // 
            // cmbIoType
            // 
            this.cmbIoType.DisplayMember = "Name";
            this.cmbIoType.FormattingEnabled = true;
            this.cmbIoType.Location = new System.Drawing.Point(221, 161);
            this.cmbIoType.Name = "cmbIoType";
            this.cmbIoType.Size = new System.Drawing.Size(133, 20);
            this.cmbIoType.TabIndex = 8;
            this.cmbIoType.ValueMember = "Value";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(114, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 9;
            this.label3.Text = "运动控制机构";
            // 
            // cmbMotionMecha
            // 
            this.cmbMotionMecha.DisplayMember = "Name";
            this.cmbMotionMecha.FormattingEnabled = true;
            this.cmbMotionMecha.Location = new System.Drawing.Point(221, 31);
            this.cmbMotionMecha.Name = "cmbMotionMecha";
            this.cmbMotionMecha.Size = new System.Drawing.Size(133, 20);
            this.cmbMotionMecha.TabIndex = 10;
            this.cmbMotionMecha.ValueMember = "Value";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(126, 205);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "是否启用";
            // 
            // chbEnable
            // 
            this.chbEnable.AutoSize = true;
            this.chbEnable.Location = new System.Drawing.Point(221, 204);
            this.chbEnable.Name = "chbEnable";
            this.chbEnable.Size = new System.Drawing.Size(48, 16);
            this.chbEnable.TabIndex = 12;
            this.chbEnable.Text = "启用";
            this.chbEnable.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(324, 270);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 13;
            this.btnOk.Text = "确定";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(206, 270);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 14;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // cmbFuncCode
            // 
            this.cmbFuncCode.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbFuncCode.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.cmbFuncCode.DisplayMember = "Name";
            this.cmbFuncCode.FormattingEnabled = true;
            this.cmbFuncCode.Location = new System.Drawing.Point(221, 73);
            this.cmbFuncCode.Name = "cmbFuncCode";
            this.cmbFuncCode.Size = new System.Drawing.Size(133, 20);
            this.cmbFuncCode.TabIndex = 15;
            this.cmbFuncCode.ValueMember = "Value";
            // 
            // CreateUpdateAddressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(428, 305);
            this.Controls.Add(this.cmbFuncCode);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.chbEnable);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbMotionMecha);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbIoType);
            this.Controls.Add(this.txbAddress);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblFuncCode);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateUpdateAddressForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.CreateUpdateAddress_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblFuncCode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txbAddress;
        private System.Windows.Forms.ComboBox cmbIoType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbMotionMecha;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chbEnable;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ComboBox cmbFuncCode;
    }
}