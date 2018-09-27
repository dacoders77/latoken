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
			this.panel_big2 = new System.Windows.Forms.Panel();
			this.listView1 = new System.Windows.Forms.ListView();
			this.panel_big = new System.Windows.Forms.Panel();
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// panel_big2
			// 
			this.panel_big2.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.panel_big2.Location = new System.Drawing.Point(968, 18);
			this.panel_big2.Name = "panel_big2";
			this.panel_big2.Size = new System.Drawing.Size(603, 871);
			this.panel_big2.TabIndex = 27;
			// 
			// listView1
			// 
			this.listView1.Location = new System.Drawing.Point(14, 79);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(282, 810);
			this.listView1.TabIndex = 28;
			this.listView1.UseCompatibleStateImageBehavior = false;
			// 
			// panel_big
			// 
			this.panel_big.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.panel_big.Location = new System.Drawing.Point(363, 18);
			this.panel_big.Name = "panel_big";
			this.panel_big.Size = new System.Drawing.Size(521, 871);
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
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1066, 700);
			this.Controls.Add(this.panel_big2);
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.panel_big);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.Panel panel_big2;
		public System.Windows.Forms.ListView listView1;
		public System.Windows.Forms.Panel panel_big;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
	}
}

