namespace BitMEXAssistant
{
	partial class Form1
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
            this.panel_big = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.orderBookControl1 = new BitMEXAssistant.OrderBookControl();
            this.orderBookControl = new BitMEXAssistant.OrderBookControl();
            this.panel_big.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel_big
            // 
            this.panel_big.AutoScroll = true;
            this.panel_big.BackColor = System.Drawing.SystemColors.Window;
            this.panel_big.Controls.Add(this.orderBookControl);
            this.panel_big.Location = new System.Drawing.Point(261, 21);
            this.panel_big.Name = "panel_big";
            this.panel_big.Size = new System.Drawing.Size(521, 595);
            this.panel_big.TabIndex = 26;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(14, 50);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(90, 23);
            this.button2.TabIndex = 25;
            this.button2.Text = "disconnect";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(14, 21);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(90, 23);
            this.button1.TabIndex = 24;
            this.button1.Text = "connect";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.Controls.Add(this.orderBookControl1);
            this.panel1.Location = new System.Drawing.Point(788, 21);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(521, 595);
            this.panel1.TabIndex = 27;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(14, 104);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.ShowCellToolTips = false;
            this.dataGridView1.ShowEditingIcon = false;
            this.dataGridView1.Size = new System.Drawing.Size(241, 512);
            this.dataGridView1.TabIndex = 28;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(14, 81);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(99, 17);
            this.checkBox1.TabIndex = 29;
            this.checkBox1.Text = "Sound Enabled";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // orderBookControl1
            // 
            this.orderBookControl1.DataSet = null;
            this.orderBookControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.orderBookControl1.Font = new System.Drawing.Font("Calibri", 10F);
            this.orderBookControl1.Location = new System.Drawing.Point(0, 0);
            this.orderBookControl1.Name = "orderBookControl1";
            this.orderBookControl1.PriceEnd = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.orderBookControl1.PriceStart = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.orderBookControl1.PriceStep = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.orderBookControl1.Size = new System.Drawing.Size(504, 871);
            this.orderBookControl1.SoundEnabled = true;
            this.orderBookControl1.TabIndex = 0;
            this.orderBookControl1.Text = "orderBookControl1";
            // 
            // orderBookControl
            // 
            this.orderBookControl.DataSet = null;
            this.orderBookControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.orderBookControl.Font = new System.Drawing.Font("Calibri", 10F);
            this.orderBookControl.Location = new System.Drawing.Point(0, 0);
            this.orderBookControl.Name = "orderBookControl";
            this.orderBookControl.PriceEnd = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.orderBookControl.PriceStart = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.orderBookControl.PriceStep = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.orderBookControl.Size = new System.Drawing.Size(504, 871);
            this.orderBookControl.SoundEnabled = true;
            this.orderBookControl.TabIndex = 0;
            this.orderBookControl.Text = "orderBookControl1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1321, 636);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel_big);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel_big.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		public System.Windows.Forms.Panel panel_big;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
        private OrderBookControl orderBookControl;
		public System.Windows.Forms.Panel panel1;
		private OrderBookControl orderBookControl1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}

