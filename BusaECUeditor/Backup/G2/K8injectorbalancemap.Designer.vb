<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class K8injectorbalancemap
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(K8injectorbalancemap))
        Me.INJBALmapgrid = New System.Windows.Forms.DataGridView
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.T_TPSIAP = New System.Windows.Forms.TextBox
        Me.T_RPM = New System.Windows.Forms.TextBox
        Me.T_change = New System.Windows.Forms.TextBox
        Me.PrintForm1 = New Microsoft.VisualBasic.PowerPacks.Printing.PrintForm(Me.components)
        Me.L_geartext = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.Label3 = New System.Windows.Forms.Label
        Me.L_MS = New System.Windows.Forms.Label
        Me.L_mode = New System.Windows.Forms.Label
        Me.L_gear = New System.Windows.Forms.Label
        CType(Me.INJBALmapgrid, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'INJBALmapgrid
        '
        Me.INJBALmapgrid.AllowUserToAddRows = False
        Me.INJBALmapgrid.AllowUserToDeleteRows = False
        Me.INJBALmapgrid.AllowUserToResizeColumns = False
        Me.INJBALmapgrid.AllowUserToResizeRows = False
        Me.INJBALmapgrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.INJBALmapgrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
        Me.INJBALmapgrid.Location = New System.Drawing.Point(3, 64)
        Me.INJBALmapgrid.Margin = New System.Windows.Forms.Padding(2, 3, 2, 3)
        Me.INJBALmapgrid.Name = "INJBALmapgrid"
        Me.INJBALmapgrid.ReadOnly = True
        Me.INJBALmapgrid.RowHeadersWidth = 500
        Me.INJBALmapgrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        Me.INJBALmapgrid.ShowEditingIcon = False
        Me.INJBALmapgrid.ShowRowErrors = False
        Me.INJBALmapgrid.Size = New System.Drawing.Size(1006, 345)
        Me.INJBALmapgrid.TabIndex = 0
        '
        'Timer1
        '
        Me.Timer1.Interval = 500
        '
        'T_TPSIAP
        '
        Me.T_TPSIAP.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.T_TPSIAP.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.T_TPSIAP.Enabled = False
        Me.T_TPSIAP.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.T_TPSIAP.Location = New System.Drawing.Point(570, 2)
        Me.T_TPSIAP.Name = "T_TPSIAP"
        Me.T_TPSIAP.Size = New System.Drawing.Size(116, 19)
        Me.T_TPSIAP.TabIndex = 19
        Me.T_TPSIAP.WordWrap = False
        '
        'T_RPM
        '
        Me.T_RPM.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.T_RPM.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.T_RPM.Enabled = False
        Me.T_RPM.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.T_RPM.Location = New System.Drawing.Point(471, 2)
        Me.T_RPM.Name = "T_RPM"
        Me.T_RPM.Size = New System.Drawing.Size(93, 19)
        Me.T_RPM.TabIndex = 18
        Me.T_RPM.WordWrap = False
        '
        'T_change
        '
        Me.T_change.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.T_change.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.T_change.Enabled = False
        Me.T_change.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.T_change.Location = New System.Drawing.Point(373, 2)
        Me.T_change.Name = "T_change"
        Me.T_change.Size = New System.Drawing.Size(92, 19)
        Me.T_change.TabIndex = 20
        Me.T_change.WordWrap = False
        '
        'PrintForm1
        '
        Me.PrintForm1.DocumentName = "document"
        Me.PrintForm1.Form = Me
        Me.PrintForm1.PrintAction = System.Drawing.Printing.PrintAction.PrintToPrinter
        Me.PrintForm1.PrinterSettings = CType(resources.GetObject("PrintForm1.PrinterSettings"), System.Drawing.Printing.PrinterSettings)
        Me.PrintForm1.PrintFileName = Nothing
        '
        'L_geartext
        '
        Me.L_geartext.AutoSize = True
        Me.L_geartext.Location = New System.Drawing.Point(14, 14)
        Me.L_geartext.Name = "L_geartext"
        Me.L_geartext.Size = New System.Drawing.Size(30, 13)
        Me.L_geartext.TabIndex = 21
        Me.L_geartext.Text = "Gear"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(83, 14)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(34, 13)
        Me.Label2.TabIndex = 22
        Me.Label2.Text = "Mode"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(155, 14)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(23, 13)
        Me.Label3.TabIndex = 23
        Me.Label3.Text = "MS"
        '
        'L_MS
        '
        Me.L_MS.AutoSize = True
        Me.L_MS.Location = New System.Drawing.Point(184, 14)
        Me.L_MS.Name = "L_MS"
        Me.L_MS.Size = New System.Drawing.Size(22, 13)
        Me.L_MS.TabIndex = 24
        Me.L_MS.Text = "n/a"
        '
        'L_mode
        '
        Me.L_mode.AutoSize = True
        Me.L_mode.Location = New System.Drawing.Point(114, 14)
        Me.L_mode.Name = "L_mode"
        Me.L_mode.Size = New System.Drawing.Size(22, 13)
        Me.L_mode.TabIndex = 25
        Me.L_mode.Text = "n/a"
        '
        'L_gear
        '
        Me.L_gear.AutoSize = True
        Me.L_gear.Location = New System.Drawing.Point(41, 14)
        Me.L_gear.Name = "L_gear"
        Me.L_gear.Size = New System.Drawing.Size(22, 13)
        Me.L_gear.TabIndex = 26
        Me.L_gear.Text = "n/a"
        '
        'K8injectorbalancemap
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(5.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1014, 412)
        Me.Controls.Add(Me.L_gear)
        Me.Controls.Add(Me.L_mode)
        Me.Controls.Add(Me.L_MS)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.L_geartext)
        Me.Controls.Add(Me.T_change)
        Me.Controls.Add(Me.T_TPSIAP)
        Me.Controls.Add(Me.T_RPM)
        Me.Controls.Add(Me.INJBALmapgrid)
        Me.Font = New System.Drawing.Font("Arial", 7.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(2, 3, 2, 3)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "K8injectorbalancemap"
        Me.Text = "ECUeditor Injector Balance maps"
        CType(Me.INJBALmapgrid, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents INJBALmapgrid As System.Windows.Forms.DataGridView
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents T_TPSIAP As System.Windows.Forms.TextBox
    Friend WithEvents T_RPM As System.Windows.Forms.TextBox
    Friend WithEvents T_change As System.Windows.Forms.TextBox
    Friend WithEvents PrintForm1 As Microsoft.VisualBasic.PowerPacks.Printing.PrintForm
    Friend WithEvents L_gear As System.Windows.Forms.Label
    Friend WithEvents L_mode As System.Windows.Forms.Label
    Friend WithEvents L_MS As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents L_geartext As System.Windows.Forms.Label
End Class
