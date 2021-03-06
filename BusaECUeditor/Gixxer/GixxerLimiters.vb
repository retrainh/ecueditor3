'
'    This file is part of ecueditor - Hayabusa ECUeditor
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
'       22.1.2011 - changed all memory addresses as variables
'
'
'



Public Class GixxerLimiters
    Dim rpmconv As Long
    Dim addedrpm As Integer


    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C_gearlimiter.CheckedChanged
        ' petrik, modified for K8
        If C_gearlimiter.Checked = True Then
            '
            ' Write limiters off values
            '
            WriteFlashByte(gixxer_fuel_limiter_by_gear, &H0) 'fuel limiter by gear 
            WriteFlashByte(gixxer_fuel_limiter_by_gear_softcut, &H0) 'fuel limiter by gear softcut
            C_gearlimiter.Text = "Gear limiters removed"
        Else
            '
            ' Write default values
            '
            WriteFlashByte(gixxer_fuel_limiter_by_gear, &H80) 'fuel limiter by gear 
            WriteFlashByte(gixxer_fuel_limiter_by_gear_softcut, &H80) 'fuel limiter by gear softcut
            C_gearlimiter.Text = "Gear limiters on"

        End If

    End Sub

    Private Sub RPM_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RPM.SelectedIndexChanged
        Dim i As Integer

        ' Set various RPM limits based on RPM value selected
        i = Val(RPM.Text)
        addedrpm = i - gixxer_baseline ' we are just setting here the baseline

        'If i >= 13500 Then
        'C_gearlimiter.Checked = True
        'End If

        '
        ' Fuel limiters
        '

        ' Type 0 - set by fuel config const, no fuelcut
        ' Type 1 - softcut
        ' Type 2 - hardcut
        ' Type 3 - GPS error or clutch
        ' Type 4 - limp mode, errorcode present - do not set with ecueditor
        ' Type 5 - limp mode, errorcode present - do not set with ecueditor
        ' Type 6 - normal running, but errorcode present
        '
        ' RPM/Fuel soft type 1, the softcut hits earlier than hardcut. Abosolute limiter at hard cut limit.
        '

        WriteFlashWord(gixxer_RPM_limit_type1, Int((rpmconv / (addedrpm + (gixxer_baseline - 71)))))
        WriteFlashWord(gixxer_RPM_limit_type1 + 2, Int((rpmconv / (addedrpm + (gixxer_baseline - 47)))))
        WriteFlashWord(gixxer_RPM_limit_type1 + 4, Int((rpmconv / (addedrpm + (gixxer_baseline - 22)))))
        WriteFlashWord(gixxer_RPM_limit_type1 + 6, Int((rpmconv / (addedrpm + (gixxer_baseline)))))
        '
        ' RPM/Fuel hard type 2, modified same as softlimit hardcut
        '
        WriteFlashWord(gixxer_RPM_limit_type1 + 8, Int((rpmconv / (addedrpm + (gixxer_baseline - 22)))))
        WriteFlashWord(gixxer_RPM_limit_type1 + 10, Int((rpmconv / (addedrpm + (gixxer_baseline)))))

        '
        ' RPM/Fuel soft hard type 3 neutral
        '
        If gixxer_fuel_limiter_by_gear = &H74F3B Then
            'k9
            WriteFlashWord(gixxer_RPM_limit_type1 + 12, Int((rpmconv / (addedrpm + (gixxer_baseline - 445)))))
            WriteFlashWord(gixxer_RPM_limit_type1 + 14, Int((rpmconv / (addedrpm + (gixxer_baseline - 399)))))
            WriteFlashWord(gixxer_RPM_limit_type1 + 16, Int((rpmconv / (addedrpm + (gixxer_baseline - 1051)))))
            WriteFlashWord(gixxer_RPM_limit_type1 + 18, Int((rpmconv / (addedrpm + (gixxer_baseline - 1000)))))
        Else
            'K7 K8
            WriteFlashWord(gixxer_RPM_limit_type1 + 12, Int((rpmconv / (addedrpm + (gixxer_baseline - 646)))))
            WriteFlashWord(gixxer_RPM_limit_type1 + 14, Int((rpmconv / (addedrpm + (gixxer_baseline - 602)))))
        End If

        '
        ' RPM limiter type 6, this is the limiter when FI light is on but still running normally

        '
        ' RPM/Ignition limiters, these are set to around 150rpm higher than fuel limiters
        '
        WriteFlashWord(gixxer_ignition_rpm_limiter, Int((rpmconv / (addedrpm + (gixxer_baseline + 148))))) 'normal limiter
        WriteFlashWord(gixxer_ignition_rpm_limiter + 2, Int((rpmconv / (addedrpm + (gixxer_baseline + 198))))) 'normal limiter
        If (ReadFlashByte(gixxer_GPS_AD_sensor_address_in_ignition_shiftkill) = &H80) Then WriteFlashWord(gixxer_ignition_rpm_limiter + 4, Int((rpmconv / (addedrpm + (gixxer_baseline - 445))))) 'clutch limiter
        If (ReadFlashByte(gixxer_GPS_AD_sensor_address_in_ignition_shiftkill) = &H80) Then WriteFlashWord(gixxer_ignition_rpm_limiter + 6, Int((rpmconv / (addedrpm + (gixxer_baseline - 399))))) 'clutch limiter
        If gixxer_ignition_rpm_limiter = &H60B14 Then ' only for 21h00
            WriteFlashWord(gixxer_ignition_rpm_limiter + 8, Int((rpmconv / (addedrpm + (gixxer_baseline + 1395))))) 'On TPS limiter a bit unsure about condition triggering this one
            WriteFlashWord(gixxer_ignition_rpm_limiter + 10, Int((rpmconv / (addedrpm + (gixxer_baseline + 1500))))) 'On TPS limiter  a bit unsure about condition triggering this one
        End If


    End Sub

    Private Sub GixxerLimiters_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles Me.KeyPress
        If e.KeyChar = Chr(27) Then Me.Close()
        If e.KeyChar = "P" Or e.KeyChar = "p" Then
            printthis()
        End If

    End Sub

    Private Sub Limiters_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim i As Integer
        rpmconv = 3840000000 / &H100

        Me.Text = gixxer_modelname & " - Limiters"

        If gixxer_fuel_limiter_by_gear = 0 Then
            MsgBox("feature not yet implemented")
            Me.Close()
        Else

            '
            ' Determine if gear limiters are on or off
            '
            If ReadFlashByte(gixxer_fuel_limiter_by_gear) <> &H80 Then
                C_gearlimiter.Checked = True
                C_gearlimiter.Text = "Gear limiters removed"
            Else
                C_gearlimiter.Checked = False
                C_gearlimiter.Text = "Gear limiters on"
            End If

            '
            ' Determine if softcut is on or off
            '
            If ReadFlashByte(gixxer_fuel_limiter_softcut_or_hardcut) = &HFF Then
                Hardcut.Checked = True
                Hardcut.Text = "Fuel hardcut only"
            Else
                Hardcut.Checked = False
                Hardcut.Text = "Fuel softcut enabled"
            End If


            'populate RPM with initial value
            i = ReadFlashWord(gixxer_RPM_limit_type1 + 6) ' this is the reference RPM that is stored in the system
            i = Int(((rpmconv / (i + 0))) + 1)
            i = CInt(i / 50) * 50 'the conversions are not exact, round it up to the closest 50 to avoid confusion
            Me.RPM.Items.Add(i.ToString())
            i = 10000
            Do While i < 15900 ' this is the maximum rpm allowed, abovet this the ecu will set up flags that are not known
                Me.RPM.Items.Add(i.ToString())
                i = i + 100
            Loop
            Me.RPM.Items.Add(i.ToString())

            Me.RPM.SelectedIndex = 0
            Me.RPM.Enabled = True
        End If

    End Sub


    Private Sub Hardcut_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Hardcut.CheckedChanged
        If Hardcut.Checked = True Then
            WriteFlashByte(gixxer_fuel_limiter_softcut_or_hardcut, &HFF)
            Hardcut.Text = "Fuel hardcut only"
        Else
            WriteFlashByte(gixxer_fuel_limiter_softcut_or_hardcut, &H80)
            Hardcut.Text = "Fuel softcut enabled"
        End If

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Close()
    End Sub



End Class