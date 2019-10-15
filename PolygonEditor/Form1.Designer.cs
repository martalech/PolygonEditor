namespace PolygonEditor
{
    partial class PolygonEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PolygonEditor));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.MoveComponentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddVertexMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RemoveVertexMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MovePolygonMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RemovePolygonMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HalveEdgeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EqualEdgesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PerpendiculateEdgesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Board = new System.Windows.Forms.PictureBox();
            this.RemoveRelationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Board)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(25, 25);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MoveComponentMenuItem,
            this.AddVertexMenuItem,
            this.RemoveVertexMenuItem,
            this.MovePolygonMenuItem,
            this.RemovePolygonMenuItem,
            this.HalveEdgeMenuItem,
            this.EqualEdgesMenuItem,
            this.PerpendiculateEdgesMenuItem,
            this.RemoveRelationMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1182, 33);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MoveComponentMenuItem
            // 
            this.MoveComponentMenuItem.Checked = true;
            this.MoveComponentMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MoveComponentMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.MoveComponentMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("MoveComponentMenuItem.Image")));
            this.MoveComponentMenuItem.Name = "MoveComponentMenuItem";
            this.MoveComponentMenuItem.Size = new System.Drawing.Size(37, 29);
            this.MoveComponentMenuItem.Text = "toolStripMenuItem2";
            this.MoveComponentMenuItem.Click += new System.EventHandler(this.OnAddVertexMenuItemClick);
            // 
            // AddVertexMenuItem
            // 
            this.AddVertexMenuItem.AccessibleDescription = "";
            this.AddVertexMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.AddVertexMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("AddVertexMenuItem.Image")));
            this.AddVertexMenuItem.Name = "AddVertexMenuItem";
            this.AddVertexMenuItem.Size = new System.Drawing.Size(37, 29);
            this.AddVertexMenuItem.Tag = "";
            this.AddVertexMenuItem.Click += new System.EventHandler(this.OnMoveComponentMenuItemClick);
            // 
            // RemoveVertexMenuItem
            // 
            this.RemoveVertexMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RemoveVertexMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("RemoveVertexMenuItem.Image")));
            this.RemoveVertexMenuItem.Name = "RemoveVertexMenuItem";
            this.RemoveVertexMenuItem.Size = new System.Drawing.Size(37, 29);
            this.RemoveVertexMenuItem.Text = "toolStripMenuItem3";
            this.RemoveVertexMenuItem.Click += new System.EventHandler(this.OnRemoveVertexMenuItemClick);
            // 
            // MovePolygonMenuItem
            // 
            this.MovePolygonMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.MovePolygonMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("MovePolygonMenuItem.Image")));
            this.MovePolygonMenuItem.Name = "MovePolygonMenuItem";
            this.MovePolygonMenuItem.Size = new System.Drawing.Size(37, 29);
            this.MovePolygonMenuItem.Text = "toolStripMenuItem4";
            this.MovePolygonMenuItem.Click += new System.EventHandler(this.OnMovePolygonMenuItemClick);
            // 
            // RemovePolygonMenuItem
            // 
            this.RemovePolygonMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RemovePolygonMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("RemovePolygonMenuItem.Image")));
            this.RemovePolygonMenuItem.Name = "RemovePolygonMenuItem";
            this.RemovePolygonMenuItem.Size = new System.Drawing.Size(37, 29);
            this.RemovePolygonMenuItem.Text = "toolStripMenuItem1";
            this.RemovePolygonMenuItem.Click += new System.EventHandler(this.OnRemovePolygonMenuItemClick);
            // 
            // HalveEdgeMenuItem
            // 
            this.HalveEdgeMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.HalveEdgeMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("HalveEdgeMenuItem.Image")));
            this.HalveEdgeMenuItem.Name = "HalveEdgeMenuItem";
            this.HalveEdgeMenuItem.Size = new System.Drawing.Size(37, 29);
            this.HalveEdgeMenuItem.Text = "toolStripMenuItem5";
            this.HalveEdgeMenuItem.Click += new System.EventHandler(this.OnHalveEdgeMenuItemClick);
            // 
            // EqualEdgesMenuItem
            // 
            this.EqualEdgesMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.EqualEdgesMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("EqualEdgesMenuItem.Image")));
            this.EqualEdgesMenuItem.Name = "EqualEdgesMenuItem";
            this.EqualEdgesMenuItem.Size = new System.Drawing.Size(37, 29);
            this.EqualEdgesMenuItem.Text = "toolStripMenuItem6";
            this.EqualEdgesMenuItem.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.EqualEdgesMenuItem.Click += new System.EventHandler(this.OnEqualEdgesMenuItemClick);
            // 
            // PerpendiculateEdgesMenuItem
            // 
            this.PerpendiculateEdgesMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PerpendiculateEdgesMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("PerpendiculateEdgesMenuItem.Image")));
            this.PerpendiculateEdgesMenuItem.Name = "PerpendiculateEdgesMenuItem";
            this.PerpendiculateEdgesMenuItem.Size = new System.Drawing.Size(37, 29);
            this.PerpendiculateEdgesMenuItem.Text = "toolStripMenuItem7";
            this.PerpendiculateEdgesMenuItem.Click += new System.EventHandler(this.OnPerpendiculateEdgesMenuItemClick);
            // 
            // Board
            // 
            this.Board.BackColor = System.Drawing.Color.White;
            this.Board.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Board.Location = new System.Drawing.Point(0, 33);
            this.Board.Name = "Board";
            this.Board.Size = new System.Drawing.Size(1182, 620);
            this.Board.TabIndex = 1;
            this.Board.TabStop = false;
            this.Board.Paint += new System.Windows.Forms.PaintEventHandler(this.OnBoardPaint);
            this.Board.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnBoadMouseClick);
            this.Board.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnBoadMouseDown);
            this.Board.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnBoardMouseMove);
            this.Board.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnBoardMouseUp);
            // 
            // RemoveRelationMenuItem
            // 
            this.RemoveRelationMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RemoveRelationMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("RemoveRelationMenuItem.Image")));
            this.RemoveRelationMenuItem.Name = "RemoveRelationMenuItem";
            this.RemoveRelationMenuItem.Size = new System.Drawing.Size(37, 29);
            this.RemoveRelationMenuItem.Text = "toolStripMenuItem1";
            this.RemoveRelationMenuItem.Click += new System.EventHandler(this.OnRemoveRelationMenuItemClick);
            // 
            // PolygonEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1182, 653);
            this.Controls.Add(this.Board);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "PolygonEditor";
            this.Text = "Polygon Editor";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Board)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem AddVertexMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MoveComponentMenuItem;
        private System.Windows.Forms.PictureBox Board;
        private System.Windows.Forms.ToolStripMenuItem RemoveVertexMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MovePolygonMenuItem;
        private System.Windows.Forms.ToolStripMenuItem HalveEdgeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem EqualEdgesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem PerpendiculateEdgesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RemovePolygonMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RemoveRelationMenuItem;
    }
}

