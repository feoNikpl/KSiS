namespace ChatClient
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.ChatHistory = new System.Windows.Forms.RichTextBox();
            this.Message = new System.Windows.Forms.RichTextBox();
            this.send = new System.Windows.Forms.Button();
            this.GetHistory = new System.Windows.Forms.Button();
            this.ClientName = new System.Windows.Forms.TextBox();
            this.SerwerConnect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.MembersBox = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ChatHistory
            // 
            this.ChatHistory.Location = new System.Drawing.Point(12, 68);
            this.ChatHistory.Name = "ChatHistory";
            this.ChatHistory.Size = new System.Drawing.Size(388, 384);
            this.ChatHistory.TabIndex = 1;
            this.ChatHistory.Text = "";
            // 
            // Message
            // 
            this.Message.Location = new System.Drawing.Point(12, 491);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(388, 96);
            this.Message.TabIndex = 2;
            this.Message.Text = "";
            // 
            // send
            // 
            this.send.Enabled = false;
            this.send.Location = new System.Drawing.Point(440, 561);
            this.send.Name = "send";
            this.send.Size = new System.Drawing.Size(75, 23);
            this.send.TabIndex = 4;
            this.send.Text = "отправить";
            this.send.UseVisualStyleBackColor = true;
            this.send.Click += new System.EventHandler(this.send_Click);
            // 
            // GetHistory
            // 
            this.GetHistory.Enabled = false;
            this.GetHistory.Location = new System.Drawing.Point(440, 489);
            this.GetHistory.Name = "GetHistory";
            this.GetHistory.Size = new System.Drawing.Size(121, 23);
            this.GetHistory.TabIndex = 6;
            this.GetHistory.Text = "история диалога";
            this.GetHistory.UseVisualStyleBackColor = true;
            this.GetHistory.Click += new System.EventHandler(this.GetHistory_Click);
            // 
            // ClientName
            // 
            this.ClientName.Location = new System.Drawing.Point(12, 27);
            this.ClientName.Name = "ClientName";
            this.ClientName.Size = new System.Drawing.Size(100, 20);
            this.ClientName.TabIndex = 7;
            // 
            // SerwerConnect
            // 
            this.SerwerConnect.Location = new System.Drawing.Point(134, 27);
            this.SerwerConnect.Name = "SerwerConnect";
            this.SerwerConnect.Size = new System.Drawing.Size(151, 20);
            this.SerwerConnect.TabIndex = 8;
            this.SerwerConnect.Text = "Соедениться с сервером";
            this.SerwerConnect.UseVisualStyleBackColor = true;
            this.SerwerConnect.Click += new System.EventHandler(this.SerwerConnect_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "введите ваше имя";
            // 
            // MembersBox
            // 
            this.MembersBox.FormattingEnabled = true;
            this.MembersBox.Location = new System.Drawing.Point(440, 68);
            this.MembersBox.Name = "MembersBox";
            this.MembersBox.Size = new System.Drawing.Size(166, 381);
            this.MembersBox.TabIndex = 12;
            this.MembersBox.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(437, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 13);
            this.label2.TabIndex = 13;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(618, 640);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.MembersBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SerwerConnect);
            this.Controls.Add(this.ClientName);
            this.Controls.Add(this.GetHistory);
            this.Controls.Add(this.send);
            this.Controls.Add(this.Message);
            this.Controls.Add(this.ChatHistory);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox ChatHistory;
        private System.Windows.Forms.RichTextBox Message;
        private System.Windows.Forms.Button send;
        private System.Windows.Forms.Button GetHistory;
        private System.Windows.Forms.TextBox ClientName;
        private System.Windows.Forms.Button SerwerConnect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox MembersBox;
        private System.Windows.Forms.Label label2;
    }
}

