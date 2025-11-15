namespace DC.Resource2.IntegrationTest
{
    partial class PLCForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnGoHome = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btMove = new System.Windows.Forms.Button();
            this.txtDist = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btnReadDI = new System.Windows.Forms.Button();
            this.btnCreateInitDB = new System.Windows.Forms.Button();
            this.btnShowAddrConfig = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(12, 10);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(140, 80);
            this.btnConnect.TabIndex = 1;
            this.btnConnect.Text = "连接PLC";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnGoHome
            // 
            this.btnGoHome.Location = new System.Drawing.Point(12, 98);
            this.btnGoHome.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnGoHome.Name = "btnGoHome";
            this.btnGoHome.Size = new System.Drawing.Size(140, 80);
            this.btnGoHome.TabIndex = 2;
            this.btnGoHome.Text = "回原点";
            this.btnGoHome.UseVisualStyleBackColor = true;
            this.btnGoHome.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(12, 186);
            this.btnStop.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(140, 80);
            this.btnStop.TabIndex = 3;
            this.btnStop.Text = "停止";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.button4_Click);
            // 
            // btMove
            // 
            this.btMove.Location = new System.Drawing.Point(12, 274);
            this.btMove.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btMove.Name = "btMove";
            this.btMove.Size = new System.Drawing.Size(140, 80);
            this.btMove.TabIndex = 4;
            this.btMove.Text = "相对运动";
            this.btMove.UseVisualStyleBackColor = true;
            this.btMove.Click += new System.EventHandler(this.button5_Click);
            // 
            // txtDist
            // 
            this.txtDist.Location = new System.Drawing.Point(314, 298);
            this.txtDist.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtDist.Name = "txtDist";
            this.txtDist.Size = new System.Drawing.Size(148, 35);
            this.txtDist.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(160, 302);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(154, 24);
            this.label1.TabIndex = 6;
            this.label1.Text = "相对运动距离";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(474, 302);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 24);
            this.label2.TabIndex = 7;
            this.label2.Text = "MM";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(678, 20);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(502, 680);
            this.richTextBox1.TabIndex = 8;
            this.richTextBox1.Text = "";
            // 
            // btnReadDI
            // 
            this.btnReadDI.Location = new System.Drawing.Point(12, 362);
            this.btnReadDI.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnReadDI.Name = "btnReadDI";
            this.btnReadDI.Size = new System.Drawing.Size(140, 80);
            this.btnReadDI.TabIndex = 9;
            this.btnReadDI.Text = "点位读取间隔测试";
            this.btnReadDI.UseVisualStyleBackColor = true;
            this.btnReadDI.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnCreateInitDB
            // 
            this.btnCreateInitDB.Location = new System.Drawing.Point(512, 40);
            this.btnCreateInitDB.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCreateInitDB.Name = "btnCreateInitDB";
            this.btnCreateInitDB.Size = new System.Drawing.Size(112, 80);
            this.btnCreateInitDB.TabIndex = 10;
            this.btnCreateInitDB.Text = "创建数据库";
            this.btnCreateInitDB.UseVisualStyleBackColor = true;
            this.btnCreateInitDB.Click += new System.EventHandler(this.button6_Click);
            // 
            // btnShowAddrConfig
            // 
            this.btnShowAddrConfig.Location = new System.Drawing.Point(12, 452);
            this.btnShowAddrConfig.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnShowAddrConfig.Name = "btnShowAddrConfig";
            this.btnShowAddrConfig.Size = new System.Drawing.Size(140, 80);
            this.btnShowAddrConfig.TabIndex = 11;
            this.btnShowAddrConfig.Text = "显示地址目录 ";
            this.btnShowAddrConfig.UseVisualStyleBackColor = true;
            this.btnShowAddrConfig.Click += new System.EventHandler(this.btnShowAddrConfig_Click);
            // 
            // PLCForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 720);
            this.Controls.Add(this.btnShowAddrConfig);
            this.Controls.Add(this.btnCreateInitDB);
            this.Controls.Add(this.btnReadDI);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtDist);
            this.Controls.Add(this.btMove);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnGoHome);
            this.Controls.Add(this.btnConnect);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "PLCForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnGoHome;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btMove;
        private System.Windows.Forms.TextBox txtDist;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btnReadDI;
        private System.Windows.Forms.Button btnCreateInitDB;
        private System.Windows.Forms.Button btnShowAddrConfig;
    }
}