﻿
namespace DMTTestStation
{
    partial class FormInput
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormInput));
            this.groupBoxInput = new System.Windows.Forms.GroupBox();
            this.textBoxInput = new System.Windows.Forms.TextBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxInput.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxInput
            // 
            this.groupBoxInput.Controls.Add(this.textBoxInput);
            this.groupBoxInput.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxInput.Location = new System.Drawing.Point(11, 20);
            this.groupBoxInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxInput.Name = "groupBoxInput";
            this.groupBoxInput.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxInput.Size = new System.Drawing.Size(529, 135);
            this.groupBoxInput.TabIndex = 0;
            this.groupBoxInput.TabStop = false;
            this.groupBoxInput.Text = "序列号";
            // 
            // textBoxInput
            // 
            this.textBoxInput.Font = new System.Drawing.Font("微软雅黑", 22F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxInput.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBoxInput.Location = new System.Drawing.Point(30, 43);
            this.textBoxInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxInput.Name = "textBoxInput";
            this.textBoxInput.Size = new System.Drawing.Size(473, 56);
            this.textBoxInput.TabIndex = 0;
            this.textBoxInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxInput_KeyDown);
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(269, 181);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(125, 46);
            this.buttonOk.TabIndex = 1;
            this.buttonOk.Text = "确定";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(414, 181);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(125, 46);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // FormInput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(558, 250);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.groupBoxInput);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormInput";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "输入";
            this.Load += new System.EventHandler(this.FormInput_Load);
            this.groupBoxInput.ResumeLayout(false);
            this.groupBoxInput.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxInput;
        private System.Windows.Forms.TextBox textBoxInput;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
    }
}