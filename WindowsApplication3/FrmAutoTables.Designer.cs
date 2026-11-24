namespace WindowsApplication3
{
    partial class FrmAutoTables
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.splitTables = new System.Windows.Forms.SplitContainer();
            this.lblMaterialTitle = new System.Windows.Forms.Label();
            this.dgvAutoTabMaterial = new System.Windows.Forms.DataGridView();
            this.lblWorkOrderTitle = new System.Windows.Forms.Label();
            this.dgvAutoTabWorkOrder = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitTables)).BeginInit();
            this.splitTables.Panel1.SuspendLayout();
            this.splitTables.Panel2.SuspendLayout();
            this.splitTables.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAutoTabMaterial)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAutoTabWorkOrder)).BeginInit();
            this.SuspendLayout();
            //
            // splitTables
            //
            this.splitTables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitTables.Location = new System.Drawing.Point(0, 0);
            this.splitTables.Name = "splitTables";
            this.splitTables.Orientation = System.Windows.Forms.Orientation.Vertical;
            //
            // splitTables.Panel1
            //
            this.splitTables.Panel1.Controls.Add(this.dgvAutoTabMaterial);
            this.splitTables.Panel1.Controls.Add(this.lblMaterialTitle);
            //
            // splitTables.Panel2
            //
            this.splitTables.Panel2.Controls.Add(this.dgvAutoTabWorkOrder);
            this.splitTables.Panel2.Controls.Add(this.lblWorkOrderTitle);
            this.splitTables.Size = new System.Drawing.Size(1084, 562);
            this.splitTables.SplitterDistance = 542;
            this.splitTables.SplitterWidth = 6;
            this.splitTables.TabIndex = 0;
            //
            // lblMaterialTitle
            //
            this.lblMaterialTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMaterialTitle.Font = new System.Drawing.Font("Microsoft YaHei", 11F, System.Drawing.FontStyle.Bold);
            this.lblMaterialTitle.Location = new System.Drawing.Point(0, 0);
            this.lblMaterialTitle.Name = "lblMaterialTitle";
            this.lblMaterialTitle.Size = new System.Drawing.Size(542, 32);
            this.lblMaterialTitle.TabIndex = 0;
            this.lblMaterialTitle.Text = "物料";
            this.lblMaterialTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // dgvAutoTabMaterial
            //
            this.dgvAutoTabMaterial.AllowUserToAddRows = false;
            this.dgvAutoTabMaterial.AllowUserToDeleteRows = false;
            this.dgvAutoTabMaterial.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAutoTabMaterial.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAutoTabMaterial.Location = new System.Drawing.Point(0, 32);
            this.dgvAutoTabMaterial.Name = "dgvAutoTabMaterial";
            this.dgvAutoTabMaterial.ReadOnly = true;
            this.dgvAutoTabMaterial.RowHeadersWidth = 40;
            this.dgvAutoTabMaterial.Size = new System.Drawing.Size(542, 530);
            this.dgvAutoTabMaterial.TabIndex = 1;
            //
            // lblWorkOrderTitle
            //
            this.lblWorkOrderTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblWorkOrderTitle.Font = new System.Drawing.Font("Microsoft YaHei", 11F, System.Drawing.FontStyle.Bold);
            this.lblWorkOrderTitle.Location = new System.Drawing.Point(0, 0);
            this.lblWorkOrderTitle.Name = "lblWorkOrderTitle";
            this.lblWorkOrderTitle.Size = new System.Drawing.Size(536, 32);
            this.lblWorkOrderTitle.TabIndex = 0;
            this.lblWorkOrderTitle.Text = "工单";
            this.lblWorkOrderTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // dgvAutoTabWorkOrder
            //
            this.dgvAutoTabWorkOrder.AllowUserToAddRows = false;
            this.dgvAutoTabWorkOrder.AllowUserToDeleteRows = false;
            this.dgvAutoTabWorkOrder.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAutoTabWorkOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAutoTabWorkOrder.Location = new System.Drawing.Point(0, 32);
            this.dgvAutoTabWorkOrder.Name = "dgvAutoTabWorkOrder";
            this.dgvAutoTabWorkOrder.ReadOnly = true;
            this.dgvAutoTabWorkOrder.RowHeadersWidth = 40;
            this.dgvAutoTabWorkOrder.Size = new System.Drawing.Size(536, 530);
            this.dgvAutoTabWorkOrder.TabIndex = 1;
            //
            // FrmAutoTables
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 562);
            this.Controls.Add(this.splitTables);
            this.Name = "FrmAutoTables";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation;
            this.Text = "实时数据看板 - 物料 / 工单";
            this.splitTables.Panel1.ResumeLayout(false);
            this.splitTables.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitTables)).EndInit();
            this.splitTables.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAutoTabMaterial)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAutoTabWorkOrder)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitTables;
        private System.Windows.Forms.Label lblMaterialTitle;
        private System.Windows.Forms.DataGridView dgvAutoTabMaterial;
        private System.Windows.Forms.Label lblWorkOrderTitle;
        private System.Windows.Forms.DataGridView dgvAutoTabWorkOrder;
    }
}
