
namespace SimulationPlatform
{
    partial class FormMain
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonSetFeedSignal = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button8);
            this.groupBox1.Controls.Add(this.button7);
            this.groupBox1.Controls.Add(this.button6);
            this.groupBox1.Controls.Add(this.button5);
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Controls.Add(this.buttonSetFeedSignal);
            this.groupBox1.Location = new System.Drawing.Point(424, 47);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(580, 701);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "流程控制";
            // 
            // buttonSetFeedSignal
            // 
            this.buttonSetFeedSignal.Location = new System.Drawing.Point(32, 39);
            this.buttonSetFeedSignal.Name = "buttonSetFeedSignal";
            this.buttonSetFeedSignal.Size = new System.Drawing.Size(196, 49);
            this.buttonSetFeedSignal.TabIndex = 0;
            this.buttonSetFeedSignal.Text = "设置上料信号使能";
            this.buttonSetFeedSignal.UseVisualStyleBackColor = true;
            this.buttonSetFeedSignal.Click += new System.EventHandler(this.buttonSetFeedSignal_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button10);
            this.groupBox2.Controls.Add(this.button9);
            this.groupBox2.Controls.Add(this.comboBox1);
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(386, 736);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "测试平台";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "DI ",
            "DO ",
            " PI ",
            "AI ",
            "AO ",
            "RTD_3L ",
            "RTD_4L",
            "TC "});
            this.comboBox1.Location = new System.Drawing.Point(48, 233);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 26);
            this.comboBox1.TabIndex = 3;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(202, 214);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(138, 62);
            this.button3.TabIndex = 2;
            this.button3.Text = "启动工位测试";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(48, 126);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(292, 62);
            this.button2.TabIndex = 1;
            this.button2.Text = "启动测试";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(48, 35);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(292, 62);
            this.button1.TabIndex = 0;
            this.button1.Text = "初始化";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(32, 244);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(196, 49);
            this.button4.TabIndex = 1;
            this.button4.Text = "设置机械手空闲";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(32, 322);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(418, 49);
            this.button5.TabIndex = 2;
            this.button5.Text = "设置机械手移动结束";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(254, 39);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(196, 49);
            this.button6.TabIndex = 3;
            this.button6.Text = "设置上料信号失效";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(254, 244);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(196, 49);
            this.button7.TabIndex = 4;
            this.button7.Text = "设置机械手忙碌";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(32, 430);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(418, 52);
            this.button8.TabIndex = 5;
            this.button8.Text = "设置机械手移动结束并设置测试AI工位忙碌状态";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(48, 374);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(292, 62);
            this.button9.TabIndex = 4;
            this.button9.Text = "发送结果OK";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(48, 460);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(292, 62);
            this.button10.TabIndex = 5;
            this.button10.Text = "发送结果NG";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1884, 986);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "FormMain";
            this.Text = "仿真平台";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonSetFeedSignal;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button9;
    }
}

