'
'    This file is part of BusaECUeditor - Hayabusa ECUeditor
'
'    Hayabusa ECUeditor is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.
'
'    Hayabusa ECUeditor is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with Hayabusa ECUeditor.  If not, see <http://www.gnu.org/licenses/>.
'
'    Notice: Please note that under GPL if you use this program or parts of it
'    you are obliged to distribute your software including source code
'    under this same license for free. For more information see paragraph 5
'    of the GNU licence.
'
Imports System
Imports System.IO
Imports System.Text
Imports System.Net.Mail

Public Class BKingAdvSettings

#Region "Variables"

    Dim loading As Boolean
    Dim _stock = &H80
    Dim _300cc = &H60
    Dim _400cc = &H30
    Dim _500cc = &H20
    Dim _600cc = &H10

#End Region

#Region "Form Events"

    Private Sub BKingAdvSettings_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        loading = True

        If readflashbyte(&H74BDC) = &H80 Then
            C_HOX.Text = "Oxy sensor ON"
            C_HOX.Checked = True
        Else
            C_HOX.Text = "Oxy sensor OFF"
            C_HOX.Checked = False
        End If

        If readflashbyte(&H79A90) = &HFF Then
            C_PAIR.Text = "PAIR ON"
            C_PAIR.Checked = True
        Else
            C_PAIR.Text = "PAIR OFF"
            C_PAIR.Checked = False
        End If

        If readflashbyte(&H79A95) = &HFF Then
            C_EVAP.Text = "EVAP ON"
            C_EVAP.Checked = True
        Else
            C_EVAP.Checked = False
            C_EVAP.Text = "EVAP OFF"
        End If

        If readflashbyte(&H73EBC) = &HFF Then
            C_EXC.Checked = True
            C_EXC.Text = "EXC ON"
        Else
            C_EXC.Checked = False
            C_EXC.Text = "EXC OFF"
        End If


        loading = False

    End Sub

    Private Sub BKingAdvSettings_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles Me.KeyPress

        If e.KeyChar = Chr(27) Then
            Me.Close()
        End If

        If e.KeyChar = "P" Or e.KeyChar = "p" Then
            PrintForm1.PrinterSettings.DefaultPageSettings.Margins.Left = 10
            PrintForm1.PrinterSettings.DefaultPageSettings.Margins.Right = 10
            PrintForm1.Print()
        End If

    End Sub

#End Region

#Region "Control Events"

    Private Sub C_HOX_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C_HOX.CheckedChanged

        If Not loading Then

            If C_HOX.Checked = True Then
                C_HOX.Text = "Oxy sensor ON"
                writeflashbyte(&H74BDC, &H80)
            Else
                C_HOX.Text = "Oxy sensor OFF"
                writeflashbyte(&H74BDC, &H0)
            End If

        End If

    End Sub

    Private Sub C_PAIR_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C_PAIR.CheckedChanged

        If Not loading Then

            If C_PAIR.Checked = True Then
                C_PAIR.Text = "PAIR ON"
                writeflashbyte(&H79A90, &HFF)
            Else
                C_PAIR.Text = "PAIR OFF"
                writeflashbyte(&H79A90, &H80)
            End If

        End If

    End Sub

    Private Sub C_EVAP_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C_EVAP.CheckedChanged

        If Not loading Then

            If C_EVAP.Checked = True Then
                C_EVAP.Text = "EVAP ON"
                writeflashbyte(&H79A95, &HFF)
            Else
                C_EVAP.Text = "EVAP OFF"
                writeflashbyte(&H79A95, &H0)
            End If

        End If

    End Sub

    Private Sub C_EXC_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C_EXC.CheckedChanged

        If Not loading Then

            If C_EXC.Checked = True Then
                C_EXC.Text = "EXCV ON"
                writeflashbyte(&H73EBC, &HFF)
                writeflashbyte(&H7000D, &HFF)
                writeflashbyte(&H7000F, &H0)
            Else
                C_EXC.Text = "EXCV OFF"
                writeflashbyte(&H73EBC, &H1) 'could be 0 or 1
                writeflashbyte(&H7000D, &H0) 'if 0 shows error on busa
                writeflashbyte(&H7000F, &H80)
            End If

        End If

    End Sub

    Private Sub B_STP_Map_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles B_STP_Map.Click

        BkingSTPmap.Show()
        BkingSTPmap.Select()

    End Sub

    Private Sub B_Inj_Bal_Map_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles B_Inj_Bal_Map.Click

        Bkinginjectorbalancemap.Show()
        Bkinginjectorbalancemap.Select()

    End Sub

#End Region

End Class
