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
			this.orderBookControl = new BitMEXAssistant.OrderBookControl();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.panel_big.SuspendLayout();
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
			// orderBookControl
			// 
			this.orderBookControl.ActiveOrders = null;
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
			// dataGridView1
			// 
			this.dataGridView1.AllowUserToAddRows = false;
			this.dataGridView1.AllowUserToDeleteRows = false;
			this.dataGridView1.AllowUserToOrderColumns = true;
			this.dataGridView1.AllowUserToResizeRows = false;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Location = new System.Drawing.Point(14, 21);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.ReadOnly = true;
			this.dataGridView1.RowHeadersVisible = false;
			this.dataGridView1.ShowCellToolTips = false;
			this.dataGridView1.ShowEditingIcon = false;
			this.dataGridView1.Size = new System.Drawing.Size(226, 572);
			this.dataGridView1.TabIndex = 28;
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Checked = true;
			this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBox1.Location = new System.Drawing.Point(14, 599);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(99, 17);
			this.checkBox1.TabIndex = 29;
			this.checkBox1.Text = "Sound Enabled";
			this.checkBox1.UseVisualStyleBackColor = true;
			this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(795, 636);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.panel_big);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.panel_big.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public System.Windows.Forms.Panel panel_big;
        private OrderBookControl orderBookControl;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}

