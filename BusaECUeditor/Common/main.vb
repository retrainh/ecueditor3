'
'    This file is part of ecueditor - ecueditor.com
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
Imports System.Threading
Imports System.IO
Imports System.Text
Imports System.Net.Mail
Imports System.Deployment.Application
Imports System.Net
Imports System.Web
Imports TwitterVB2




Public Class main

#Region "Declare Functions"

    Public Declare Function FT_GetModemStatus Lib "FTD2XX.DLL" (ByVal lnghandle As Integer, ByRef modstat As Integer) As Integer
    Public Declare Function FT_SetBreakOn Lib "FTD2XX.DLL" (ByVal lnghandle As Integer) As Integer
    Public Declare Function FT_SetBreakOff Lib "FTD2XX.DLL" (ByVal lnghandle As Integer) As Integer
    Public Declare Function FT_Open Lib "FTD2XX.DLL" (ByVal iDevice As Integer, ByRef lnghandle As Integer) As Integer
    Public Declare Function FT_Purge Lib "FTD2XX.DLL" (ByVal lnghandle As Integer) As Integer
    Public Declare Function FT_SetRts Lib "FTD2XX.DLL" (ByVal lnghandle As Integer) As Integer
    Public Declare Function FT_ClrRts Lib "FTD2XX.DLL" (ByVal lnghandle As Integer) As Integer

    Public Declare Function FT_SetDtr Lib "FTD2XX.DLL" (ByVal lnghandle As Integer) As Integer 'new for Interface V1.1 ***************************************
    Public Declare Function FT_ClrDtr Lib "FTD2XX.DLL" (ByVal lnghandle As Integer) As Integer 'new for Interface V1.1 ***************************************

    Public Declare Function FT_SetLatencyTimer Lib "FTD2XX.DLL" (ByVal lnghandle As Integer, ByVal uTime As Byte) As Integer
    Public Declare Function FT_SetUSBParameters Lib "FTD2XX.DLL" (ByVal lnghandle As Integer, ByVal dwInTransferSize As Integer, ByVal dwOutTransferSize As Integer) As Integer

    Public Declare Function FT_ResetDevice Lib "FTD2XX.DLL" (ByVal lnghandle As Integer, ByVal iMask As Integer) As Integer
    Public Declare Function FT_GetNumberOfDevices Lib "FTD2XX.DLL" Alias "FT_ListDevices" (ByRef lngNumberofdevices As Integer, ByVal pvarg2 As String, ByVal lngflags As Integer) As Integer
    Public Declare Function FT_ListDevices Lib "FTD2XX.DLL" (ByRef lngNumberofdevices As Integer, ByVal pvarg2 As String, ByVal lngflags As Integer) As Integer
    Public Declare Function FT_Close Lib "FTD2XX.DLL" (ByVal lnghandle As Integer) As Integer
    Public Declare Function FT_GetComPortNumber Lib "FTD2XX.DLL" (ByVal lnghandle As Integer, ByRef portnumber As Integer) As Integer
    Public Declare Function FT_SetBaudRate Lib "FTD2XX.DLL" (ByVal lngHandle As Integer, ByVal lngBaudRate As Integer) As Integer
    Public Declare Function FT_SetDataCharacteristics Lib "FTD2XX.DLL" (ByVal lnghandle As Integer, ByVal uWordLength As Byte, ByVal uStopBits As Byte, ByVal uParity As Byte) As Integer
    Public Declare Function FT_Write_Bytes Lib "FTD2XX.DLL" Alias "FT_Write" (ByVal lngHandle As Integer, ByRef lpvBuffer As Byte, ByVal lngBufferSize As Integer, ByRef lngBytesWritten As Integer) As Integer
    Public Declare Function FT_Read_Bytes Lib "FTD2XX.DLL" Alias "FT_Read" (ByVal lngHandle As Integer, ByRef lpvBuffer As Byte, ByVal lngBufferSize As Integer, ByRef lngBytesReturned As Integer) As Integer
    Public Declare Function FT_GetStatus Lib "FTD2XX.DLL" (ByVal lngHandle As Integer, ByRef lngamountInRxQueue As Integer, ByRef lngAmountInTxQueue As Integer, ByRef lngEventStatus As Integer) As Integer
    Public Declare Function FT_SetTimeouts Lib "FTD2XX.DLL" (ByVal lngHandle As Integer, ByVal rxTimeout As Integer, ByVal txTimeout As Integer) As Integer
    Public Declare Function timeBeginPeriod Lib "winmm.dll" (ByVal uPeriod As Integer) As Integer
    Public Declare Function timeEndPeriod Lib "winmm.dll" (ByVal uPeriod As Integer) As Integer

    Public Declare Function FT_Write Lib "FTD2XX.DLL" (ByVal ftHandle As Integer, ByVal lpBuffer() As Byte, ByVal nBufferSize As Integer, ByRef lpBytesWritten As Integer) As Integer

    Declare Function QueryPerformanceCounter Lib "Kernel32" (ByRef X As Long) As Short 'new HighResTimer*******************************
    Declare Function QueryPerformanceFrequency Lib "Kernel32" (ByRef X As Long) As Short    ' new HighResTimer************************

#End Region

#Region "Variables"

    Dim path As String = ""
    Dim comparepath As String = ""
    Dim fs As FileStream
    Dim tw As New TwitterAPI
    Dim tweets As Boolean
    Dim latest As DateTime
    Dim latest_id As ULong


#End Region

#Region "Form Events"

    Private Sub Main_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'generate_tweets()

        Me.Visible = False
        '
        ' COmmented out for VBonXP
        '
        'My.Application.SaveMySettingsOnExit = True

        '
        ' Win XP error handler
        ' 
        On Error GoTo skip_update


        Dim deploy As ApplicationDeployment = ApplicationDeployment.CurrentDeployment
        Dim update As UpdateCheckInfo = deploy.CheckForDetailedUpdate()
        If (deploy.CheckForUpdate()) Then
            If (Updateavail.ShowDialog() = Windows.Forms.DialogResult.OK) Then
                deploy.Update()
                Application.Restart()
            End If
        End If
skip_update:




        ' initialize global variables, just in case

        DisableButtons()
        path = My.Settings.Item("path")
        comparepath = My.Settings.Item("comparepath")
        ECUVersion = ""

        BlockPgm = True ' just initializing

        'MsgBox("Important note: This is a prerelease of Hayabusa ECUeditor v2.0 for testing new functionality.")

        If My.Settings.Item("ComPort") = "" Then MsgBox("Com port not set up. Please click 'Connect for enginedata' and set up the correct com port.")

        LoginForm.Show()
        LoginForm.Select()


    End Sub

#End Region

#Region "Control Events"

    Private Sub B_FuelMap_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles B_FuelMap.Click
        Select Case ECUVersion
            Case "gen1"
                FuelMap.Show()
                FuelMap.Select()
            Case "gen2"
                K8Fuelmap.Show()
                K8Fuelmap.Select()
            Case "bking"
                BKingFuelMap.Show()
                BKingFuelMap.Select()
            Case "gixxer"
                GixxerFuelmap.Show()
                GixxerFuelmap.Select()
            Case Else
                MsgBox("Feature not yet implemented")
        End Select
    End Sub

    Private Sub B_IgnitionMap_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles B_IgnitionMap.Click

        Select Case ECUVersion
            Case "gen1"
                IgnitionMap.Show()
                IgnitionMap.Select()
            Case "gen2"
                K8Ignitionmap.Show()
                K8Ignitionmap.Select()
            Case "gixxer"
                GixxerIgnitionmap.Show()
                GixxerIgnitionmap.Select()
            Case "bking"
                BKingIgnitionMap.Show()
                BKingIgnitionMap.Select()
            Case Else
                MsgBox("Feature not yet implemented")
        End Select

    End Sub

    Private Sub B_Limiters_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles B_Limiters.Click

        Select Case ECUVersion
            Case "gen1"
                Limiters.Show()
                Limiters.Select()
            Case "gen2"
                K8Limiters.Show()
                K8Limiters.Select()
            Case "bking"
                BKingLimiters.Show()
                BKingLimiters.Select()
            Case "gixxer"
                GixxerLimiters.Show()
                GixxerLimiters.Select()
            Case Else
                MsgBox("feature not yet supported")
        End Select
    End Sub

    Private Sub B_FlashECU_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles B_FlashECU.Click

        FlashTheECU()

    End Sub

    Private Sub B_EngineData_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles B_EngineData.Click
        Select Case ECUVersion
            Case "gen1"
                Datastream.Show()
                Datastream.Select()
            Case "gen2"
                K8Datastream.Show()
                K8Datastream.Select()
            Case "bking"
                K8Datastream.Show()
                K8Datastream.Select()
            Case "gixxer"
                K8Datastream.Show()
                K8Datastream.Select()
            Case "enginedata"
                K8Datastream.Show()
                K8Datastream.Select()
            Case Else
                MsgBox("Feature not yet implemented")
        End Select
    End Sub

    Private Sub B_AdvancedSettings_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles B_AdvancedSettings.Click

        Select Case ECUVersion
            Case "gen1"
                AdvSettings.Show()
                AdvSettings.Select()
            Case "gen2"
                K8Advsettings.Show()
                K8Advsettings.Select()
            Case "bking"
                BKingAdvSettings.Show()
                BKingAdvSettings.Select()
            Case "gixxer"
                GixxerAdvSettings.Show()
                GixxerAdvSettings.Select()
            Case Else
                MsgBox("Feature not yet implemented")
        End Select

    End Sub

    Private Sub B_Shifter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles B_Shifter.Click
        Select Case ECUVersion
            Case "gen1"
                Shifter.Show()
                Shifter.Select()
            Case "gen2"
                K8Shifter.Show()
                K8Shifter.Select()
            Case "gixxer"
                GixxerShifter.Show()
                GixxerShifter.Select()
            Case "bking"
                BKingShifter.Show()
                BKingShifter.Select()
            Case Else
                MsgBox("Feature not yet implemented")
        End Select
    End Sub

    Private Sub G1NewBaseMapUSToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
    End Sub

    Private Sub G1NewBaseMapEUToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
    End Sub

    Private Sub NewK8ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
    End Sub

    Private Sub NewStockBkingToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
    End Sub

    Private Sub NewStockBkingUSToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
    End Sub

    Private Sub OpenToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenToolStripMenuItem.Click
        ' Lets use OpenFileDialog to open a new flash image file
        Dim fdlg As OpenFileDialog = New OpenFileDialog()
        Dim fs As FileStream
        fdlg.InitialDirectory = path 'My.Application.Info.DirectoryPath
        fdlg.Title = "Open ECU .bin file"
        fdlg.Filter = "ECU definitions (*.bin)|*.bin"
        fdlg.FilterIndex = 1
        fdlg.RestoreDirectory = True
        fdlg.FileName = path

        CloseChildWindows()

        If fdlg.ShowDialog() = Windows.Forms.DialogResult.OK Then

            ' OK, so the file is found, now lets start processing it
            path = fdlg.FileName
            If path.Length > 15 Then
                L_Comparefile.Text = "..." & path.Substring(path.Length - 15)
                L_File.Text = L_Comparefile.Text
            Else
                L_Comparefile.Text = path
                L_File.Text = L_Comparefile.Text
            End If

            ' Open the stream and read it to global variable "Flash". 
            fs = File.OpenRead(path)
            Dim b(1) As Byte
            Dim i As Integer
            i = 0
            Do While fs.Read(b, 0, 1) > 0
                Flash(i) = b(0)
                FlashCopy(i) = b(0)
                i = i + 1
            Loop
            fs.Close()

            ' Check that the binary lenght matches expected ecu
            ' and initialize variables and stuff as needed 

            Select Case i
                Case (262144 * 4)
                    ECUVersion = "gen2"
                    '
                    ' Make sure the ECU id is supported type
                    '
                    i = 0
                    ECUID.Text = ""
                    Do While i < 8
                        ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
                        i = i + 1
                    Loop

                    ' check the ecu id bytes and validate that the ecu flash image is supported
                    SetECUType()

                    BlockPgm = True
                    CloseChildWindows()
                Case (262144)
                    ECUVersion = "gen1"
                    FlashToolStripMenuItem.Visible = False

                    ' Make sure the ECU id is supported type
                    i = 0
                    ECUID.Text = ""
                    Do While i < 8
                        ECUID.Text = ECUID.Text & Chr(Flash(&H3FFF0 + i))
                        i = i + 1
                    Loop

                    ' check the ecu id bytes and validate that the ecu flash image is supported
                    If Mid(ECUID.Text, 1, 6) <> "BB34BB" Then
                        ECUNotSupported.ShowDialog()
                    Else
                        Hayabusa.Visible = True
                        G1BinFileVersion(ECUID.Text)
                    End If

                Case Else
                    ECUVersion = ""
                    ECUNotSupported.ShowDialog()
            End Select

            My.Settings.Item("path") = path
            My.Settings.Item("comparepath") = comparepath
            ' enable controls, otherwise at form load an event will occur
            Limiters.C_RPM.Enabled = True

        End If

    End Sub

    Private Sub SaveToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveToolStripMenuItem.Click
        ' lets use SaveFileDialog
        Dim fs As FileStream
        Dim fdlg As SaveFileDialog = New SaveFileDialog()
        fdlg.Title = "Save ECU .bin file"
        fdlg.Filter = "All files (*.*)|*.*|All files (*.*)|*.*"
        fdlg.FilterIndex = 2
        fdlg.RestoreDirectory = True
        fdlg.FileName = path


        Select Case ECUVersion
            Case "gen1"
                ' First, lets show a warning dialogue about dangers of updating the ecu
                'If filesavenotice.ShowDialog = Windows.Forms.DialogResult.OK Then

                ' and now lets start saving the file using savefiledialog
                If fdlg.ShowDialog() = Windows.Forms.DialogResult.OK Then

                    path = fdlg.FileName
                    If Not path.Contains(".bin") Then path = path & ".bin"
                    ' if the file exists then lets make a backup copy of it just in case...
                    If File.Exists(path) = True Then
                        If File.Exists(path & ".bak") Then
                            File.Delete(path & ".bak")
                        End If
                        File.Copy(path, (path & ".bak"))
                        File.Delete(path)
                    End If
                    ' save the file
                    fs = File.Open(path, FileMode.CreateNew)
                    fs.Write(Flash, 0, 262144)
                    fs.Close()

                    If path.Length > 15 Then
                        L_File.Text = "..." & path.Substring(path.Length - 15)
                    Else
                        L_File.Text = path
                    End If

                End If
                'End If
            Case "gen2"
                ' First, lets show a warning dialogue about dangers of updating the ecu
                'If filesavenotice.ShowDialog = Windows.Forms.DialogResult.OK Then

                ' and now lets start saving the file using savefiledialog
                If fdlg.ShowDialog() = Windows.Forms.DialogResult.OK Then

                    path = fdlg.FileName
                    If Not path.Contains(".bin") Then path = path & ".bin"
                    ' if the file exists then lets make a backup copy of it just in case...
                    If File.Exists(path) = True Then
                        If File.Exists(path & ".bak") Then
                            File.Delete(path & ".bak")
                        End If
                        File.Copy(path, (path & ".bak"))
                        File.Delete(path)
                    End If
                    ' save the file
                    fs = File.Open(path, FileMode.CreateNew)
                    fs.Write(Flash, 0, (262144 * 4))
                    fs.Close()
                    If path.Length > 15 Then
                        L_File.Text = "..." & path.Substring(path.Length - 15)
                    Else
                        L_File.Text = path
                    End If

                End If
            Case "bking"
                ' First, lets show a warning dialogue about dangers of updating the ecu
                'If filesavenotice.ShowDialog = Windows.Forms.DialogResult.OK Then

                ' and now lets start saving the file using savefiledialog
                If fdlg.ShowDialog() = Windows.Forms.DialogResult.OK Then

                    path = fdlg.FileName
                    If Not path.Contains(".bin") Then path = path & ".bin"
                    ' if the file exists then lets make a backup copy of it just in case...
                    If File.Exists(path) = True Then
                        If File.Exists(path & ".bak") Then
                            File.Delete(path & ".bak")
                        End If
                        File.Copy(path, (path & ".bak"))
                        File.Delete(path)
                    End If
                    ' save the file
                    fs = File.Open(path, FileMode.CreateNew)
                    fs.Write(Flash, 0, (262144 * 4))
                    fs.Close()
                    If path.Length > 15 Then
                        L_File.Text = "..." & path.Substring(path.Length - 15)
                    Else
                        L_File.Text = path
                    End If

                End If
            Case "gixxer"
                ' First, lets show a warning dialogue about dangers of updating the ecu
                'If filesavenotice.ShowDialog = Windows.Forms.DialogResult.OK Then

                ' and now lets start saving the file using savefiledialog
                If fdlg.ShowDialog() = Windows.Forms.DialogResult.OK Then

                    path = fdlg.FileName
                    If Not path.Contains(".bin") Then path = path & ".bin"
                    ' if the file exists then lets make a backup copy of it just in case...
                    If File.Exists(path) = True Then
                        If File.Exists(path & ".bak") Then
                            File.Delete(path & ".bak")
                        End If
                        File.Copy(path, (path & ".bak"))
                        File.Delete(path)
                    End If
                    ' save the file
                    fs = File.Open(path, FileMode.CreateNew)
                    fs.Write(Flash, 0, (262144 * 4))
                    fs.Close()
                    If path.Length > 15 Then
                        L_File.Text = "..." & path.Substring(path.Length - 15)
                    Else
                        L_File.Text = path
                    End If

                End If
            Case Else
                ' First, lets show a warning dialogue about dangers of updating the ecu
                'If filesavenotice.ShowDialog = Windows.Forms.DialogResult.OK Then

                ' and now lets start saving the file using savefiledialog
                If fdlg.ShowDialog() = Windows.Forms.DialogResult.OK Then

                    path = fdlg.FileName
                    If Not path.Contains(".bin") Then path = path & ".bin"
                    ' if the file exists then lets make a backup copy of it just in case...
                    If File.Exists(path) = True Then
                        If File.Exists(path & ".bak") Then
                            File.Delete(path & ".bak")
                        End If
                        File.Copy(path, (path & ".bak"))
                        File.Delete(path)
                    End If
                    ' save the file
                    fs = File.Open(path, FileMode.CreateNew)
                    fs.Write(Flash, 0, (262144 * 4))
                    fs.Close()
                    If path.Length > 15 Then
                        L_File.Text = "..." & path.Substring(path.Length - 15)
                    Else
                        L_File.Text = path
                    End If

                End If
                MsgBox("Somehow trying to save a .bin which is not gen1, gen2 or Gixxer. please save your bin and send it to info@WoolichRacing.com")
        End Select

    End Sub

    Private Sub OpenComparemapToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenComparemapToolStripMenuItem.Click
        ' Lets use OpenFileDialog to open a a flash image file
        ' this function is used for fuelmap and ignitionmap compare function to compare maps
        ' to the previous maps
        Dim fdlg As OpenFileDialog = New OpenFileDialog()
        Dim fs As FileStream

        CloseChildWindows()

        Select Case ECUVersion
            Case "gen1"
                fdlg.InitialDirectory = comparepath 'My.Application.Info.DirectoryPath
                fdlg.Title = "Open ECU .bin file"
                fdlg.Filter = "ECU definitions (*.bin)|*.bin"
                fdlg.FilterIndex = 1
                fdlg.RestoreDirectory = True
                fdlg.FileName = comparepath

                If fdlg.ShowDialog() = Windows.Forms.DialogResult.OK Then

                    ' OK, so the file is found, now lets start processing it
                    comparepath = fdlg.FileName
                    If comparepath.Length > 15 Then
                        L_Comparefile.Text = "..." & comparepath.Substring(comparepath.Length - 15)
                    Else
                        L_Comparefile.Text = comparepath
                    End If


                    ' Open the stream and read it to global variable "Flash". 
                    fs = File.OpenRead(comparepath)
                    Dim b(1) As Byte
                    Dim i As Integer
                    i = 0
                    Do While fs.Read(b, 0, 1) > 0
                        FlashCopy(i) = b(0)
                        i = i + 1
                    Loop
                    fs.Close()

                    ' Check that the binary lenght matches expected ecu
                    If i <> 262144 Then
                        ECUNotSupported.ShowDialog()
                    End If
                End If
                '
                ' Lets write this value into the memory so that its easier for the user to know
                '
                My.Settings.Item("comparepath") = comparepath
                FuelMap.Close()
                IgnitionMap.Close()
            Case "gen2"
                fdlg.InitialDirectory = comparepath 'My.Application.Info.DirectoryPath
                fdlg.Title = "Open ECU .bin file"
                fdlg.Filter = "ECU definitions (*.bin)|*.bin"
                fdlg.FilterIndex = 1
                fdlg.RestoreDirectory = True
                fdlg.FileName = comparepath

                If fdlg.ShowDialog() = Windows.Forms.DialogResult.OK Then

                    ' OK, so the file is found, now lets start processing it
                    comparepath = fdlg.FileName
                    If comparepath.Length > 15 Then
                        L_Comparefile.Text = "..." & comparepath.Substring(comparepath.Length - 15)
                    Else
                        L_Comparefile.Text = comparepath
                    End If


                    ' Open the stream and read it to global variable "Flash". 
                    fs = File.OpenRead(comparepath)
                    Dim b(1) As Byte
                    Dim i As Integer
                    i = 0
                    Do While fs.Read(b, 0, 1) > 0
                        FlashCopy(i) = b(0)
                        i = i + 1
                    Loop
                    fs.Close()

                    ' Check that the binary lenght matches expected ecu
                    If i <> (262144 * 4) Then
                        ECUNotSupported.ShowDialog()
                    End If
                End If
                '
                ' Lets write this value into the memory so that its easier for the user to know
                '
                My.Settings.Item("comparepath") = comparepath
                FuelMap.Close()
                IgnitionMap.Close()
            Case "bking"
                fdlg.InitialDirectory = comparepath 'My.Application.Info.DirectoryPath
                fdlg.Title = "Open ECU .bin file"
                fdlg.Filter = "ECU definitions (*.bin)|*.bin"
                fdlg.FilterIndex = 1
                fdlg.RestoreDirectory = True
                fdlg.FileName = comparepath

                If fdlg.ShowDialog() = Windows.Forms.DialogResult.OK Then

                    ' OK, so the file is found, now lets start processing it
                    comparepath = fdlg.FileName
                    If comparepath.Length > 15 Then
                        L_Comparefile.Text = "..." & comparepath.Substring(comparepath.Length - 15)
                    Else
                        L_Comparefile.Text = comparepath
                    End If


                    ' Open the stream and read it to global variable "Flash". 
                    fs = File.OpenRead(comparepath)
                    Dim b(1) As Byte
                    Dim i As Integer
                    i = 0
                    Do While fs.Read(b, 0, 1) > 0
                        FlashCopy(i) = b(0)
                        i = i + 1
                    Loop
                    fs.Close()

                    ' Check that the binary lenght matches expected ecu
                    If i <> (262144 * 4) Then
                        ECUNotSupported.ShowDialog()
                    End If
                End If
                '
                ' Lets write this value into the memory so that its easier for the user to know
                '
                My.Settings.Item("comparepath") = comparepath
                FuelMap.Close()
                IgnitionMap.Close()
            Case "gixxer"
                fdlg.InitialDirectory = comparepath 'My.Application.Info.DirectoryPath
                fdlg.Title = "Open ECU .bin file"
                fdlg.Filter = "ECU definitions (*.bin)|*.bin"
                fdlg.FilterIndex = 1
                fdlg.RestoreDirectory = True
                fdlg.FileName = comparepath

                If fdlg.ShowDialog() = Windows.Forms.DialogResult.OK Then

                    ' OK, so the file is found, now lets start processing it
                    comparepath = fdlg.FileName
                    If comparepath.Length > 15 Then
                        L_Comparefile.Text = "..." & comparepath.Substring(comparepath.Length - 15)
                    Else
                        L_Comparefile.Text = comparepath
                    End If


                    ' Open the stream and read it to global variable "Flash". 
                    fs = File.OpenRead(comparepath)
                    Dim b(1) As Byte
                    Dim i As Integer
                    i = 0
                    Do While fs.Read(b, 0, 1) > 0
                        FlashCopy(i) = b(0)
                        i = i + 1
                    Loop
                    fs.Close()

                    ' Check that the binary lenght matches expected ecu
                    If i <> (262144 * 4) Then
                        ECUNotSupported.ShowDialog()
                    End If
                End If
                '
                ' Lets write this value into the memory so that its easier for the user to know
                '
                My.Settings.Item("comparepath") = comparepath
        End Select
    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click

        End

    End Sub

    Private Sub ProgramInfoToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ProgramInfoToolStripMenuItem.Click

        AboutBox.Show()
        AboutBox.Select()

    End Sub

    Private Sub VersionToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles VersionToolStripMenuItem.Click
        Dim strVersion As String
        Dim strPublish As String
        Dim strTitle As String

        strVersion = "DisplayVersion"
        strPublish = "Publisher"
        strTitle = "DisplayName"

        Dim assembly As System.Reflection.Assembly
        assembly = System.Reflection.Assembly.GetExecutingAssembly()

        If (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed) Then
            Dim currentVersion As System.Version
            currentVersion = My.Application.Deployment.CurrentVersion
            strVersion = currentVersion.ToString()
        Else
            strVersion = assembly.GetName.Version.Major & "." & assembly.GetName.Version.Minor & "." & assembly.GetName.Version.Build & "." & assembly.GetName.Version.Revision
        End If

        Dim company As System.Reflection.AssemblyCompanyAttribute
        company = assembly.GetCustomAttributes(GetType(System.Reflection.AssemblyCompanyAttribute), False)(0)
        strPublish = company.Company

        Dim title As System.Reflection.AssemblyTitleAttribute
        title = assembly.GetCustomAttributes(GetType(System.Reflection.AssemblyTitleAttribute), False)(0)
        strTitle = title.Title

        Dim description As System.Reflection.AssemblyDescriptionAttribute
        description = assembly.GetCustomAttributes(GetType(System.Reflection.AssemblyDescriptionAttribute), False)(0)

        strTitle = strTitle & " " & description.Description

        MsgBox(vbTab & vbTab & "ECUeditor Version: " & vbTab & strVersion & vbCr _
        & vbCr & vbTab & vbTab & "         Publisher: " & strPublish & vbCr & vbTab & vbTab & " Version info displayed thanks to Eric. " & vbCr, 0, strTitle)

    End Sub

    Private Sub ProgramUpdateToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        ProgramUpdate.Show()
        ProgramUpdate.Select()

    End Sub

    Private Sub InstallFTDIDriversToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles InstallFTDIDriversToolStripMenuItem.Click

        Dim filepath As String = Application.StartupPath & "\common\CDM20814_Setup.exe"

        If File.Exists(filepath) = False Then
            MsgBox("FTDI driver is missing, can not be installed")
            Return
        Else
            If MsgBox("Install/reinstall the FTDI USB COM port drivers, press OK or Cancel", MsgBoxStyle.OkCancel) = MsgBoxResult.Ok Then
                'Shell(filepath, AppWinStyle.NormalFocus, True, -1)
                System.Diagnostics.Process.Start(filepath)
                MsgBox("When drivers are installed, please reboot your computer")
            Else
                MsgBox("Drivers not installed")
            End If
        End If

    End Sub

    Private Sub SetupCOMPortToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SetupCOMPortToolStripMenuItem.Click

        K8setupcomport.Show()

    End Sub

    Private Sub VerifyChecksumToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles VerifyChecksumToolStripMenuItem.Click
        Select Case ECUVersion
            Case "gen1"
                MsgBox("Command not supported with gen1 ecu.")
            Case "gen2"
                TestCheckSum()
            Case "gixxer"
                TestCheckSum()
            Case "bking"
                TestCheckSum()
            Case Else
                MsgBox("Unknown ecu type, command not supported.")
        End Select
    End Sub

    Private Sub VerifyECUToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles VerifyECUToolStripMenuItem.Click
        Select Case ECUVersion
            Case "gen1"
                MsgBox("Command not supported with gen1 ecu.")
            Case "gen2"
                ReadECU()
            Case "bking"
                ReadECU()
            Case "gixxer"
                ReadECU()
            Case Else
                MsgBox("Unknown ecu type, command not supported.")
        End Select
    End Sub

    Private Sub FullEraseToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FullEraseToolStripMenuItem.Click
        Select Case ECUVersion
            Case "gen1"
                MsgBox("Command not supported with gen1 ecu.")
            Case "gen2"
                EraseECU()
            Case "bking"
                EraseECU()
            Case "gixxer"
                EraseECU()
            Case Else
                MsgBox("Unknown ecu type, command not supported.")
        End Select
    End Sub

    Private Sub FlashTheECUToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FlashTheECUToolStripMenuItem.Click
        Select Case ECUVersion
            Case "gen1"
                MsgBox("Command not supported with gen1 ecu.")
            Case "gen2"
                FlashTheECU()
            Case "bking"
                FlashTheECU()
            Case "gixxer"
                FlashTheECU()
            Case "flashonly"
                FlashTheECU()
            Case Else
                MsgBox("Unknown ecu type, command not supported.")
        End Select

    End Sub

    Private Sub HomepageToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
    End Sub

    Private Sub L_ProgramHomepage_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs)

        System.Diagnostics.Process.Start("http://www.ecueditor.com")

    End Sub

#End Region

#Region "Functions"

    Private Sub G1BinFileVersion(ByVal ecuid As String)
        Select Case Mid(ecuid, 1, 8)
            Case "BB34BB51"
                Hayabusa.Text = "Hayabusa EU 32920-24FGO"
                Metric = True
                ECUVersion = "gen1"
                ' enable controls, otherwise at form load an event will occur
                Limiters.C_RPM.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = True
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True
                B_EngineData.Enabled = True
                B_DataLogging.Enabled = False

            Case "BB34BB35"
                Hayabusa.Text = "Hayabusa USA 32920-24FKO"
                Metric = False
                ECUVersion = "gen1"
                ' enable controls, otherwise at form load an event will occur
                Limiters.C_RPM.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = True
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True
                B_EngineData.Enabled = True
                B_DataLogging.Enabled = False

            Case Else
                Hayabusa.Text = "Unknown model"
                Metric = True
                ECUVersion = ""
                ' enable controls, otherwise at form load an event will occur
                Limiters.C_RPM.Enabled = False
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = False
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = False
                B_IgnitionMap.Enabled = False
                B_AdvancedSettings.Enabled = False
                B_EngineData.Enabled = True
                B_DataLogging.Enabled = False

        End Select
    End Sub

    Private Sub G1ReadMap(ByVal defpath)
        'this procedure reads base map from the location given as parameter
        'this procedure is for G1 only
        'JaSa 01.July.2010

        L_File.Text = ""
        L_Comparefile.Text = ""


        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> 262144 Then
            ECUNotSupported.ShowDialog()
        End If
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&H3FFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        If Mid(ECUID.Text, 1, 6) <> "BB34BB" Then
            ECUNotSupported.ShowDialog()
        Else
            Hayabusa.Visible = True
            FlashToolStripMenuItem.Visible = False
            G1BinFileVersion(ECUID.Text)
        End If


        ' if the computername does not match to the stored computername, do not use the email address from this map
        CloseChildWindows()

        MsgBox("A new gen1 " & Hayabusa.Text & " basemap is generated", MsgBoxStyle.Information)

    End Sub


    Private Sub DisableButtons()
        '
        ' set the controls enabled or disabled at start or when file is reload. 
        ' the ecu version testing sets the correct buttons visible or enabled.
        '
        Hayabusa.Visible = False
        OpenToolStripMenuItem.Enabled = True
        SaveToolStripMenuItem.Enabled = False
        B_Limiters.Enabled = False
        B_Shifter.Enabled = False
        B_FlashECU.Enabled = False
        B_FuelMap.Enabled = False
        B_IgnitionMap.Enabled = False
        B_AdvancedSettings.Enabled = False
        B_EngineData.Enabled = False
        B_DataLogging.Enabled = False
        ReadProcessOnGoing = False
        FuelMapVisible = False
        IgnitionMapVisible = False

    End Sub

    Private Sub CloseChildWindows()
        '
        ' This sub closes all open windows that are closeable. Did not get MDI thread working properly so using just vb close instead
        '

        'gen2
        K8Advsettings.Close()
        K8boostfuel.Close()
        K8Fuelmap.Close()
        K8Datastream.Close()
        K8Ignitionmap.Close()
        K8Shifter.Close()
        K8dragtools.Close()
        K8injectorbalancemap.Close()
        K8dwellignition.Close()
        K8STPmap.Close()
        K8Limiters.Close()

        'gen1 
        FuelMap.Close()
        IgnitionMap.Close()
        AdvSettings.Close()
        Shifter.Close()
        Limiters.Close()

        'BKing
        BKingFuelMap.Close()
        BKingIgnitionMap.Close()
        BKingAdvSettings.Close()
        BKingShifter.Close()
        BKingLimiters.Close()

        'gixxer
        GixxerFuelmap.Close()
        GixxerIgnitionmap.Close()
        GixxerAdvSettings.Close()
        GixxerShifter.Close()
        GixxerLimiters.Close()
        Gixxerramair.Close()
        Gixxersteeringdampenermap.Close()
        GixxerSTPmap.Close()
        Gixxerinjectorbalancemap.Close()
        GixxerFuelGraph.Close()

        '
        ' resume on error back to default
        '

    End Sub

    Private Sub FlashTheECU()
        Select Case ECUVersion
            Case "gen1"
                RenesasFDT()
            Case "gen2"
                FlashSerial()
            Case "bking"
                FlashSerial()
            Case "gixxer"
                FlashSerial()
            Case "flashonly"
                FlashSerial()
            Case Else
                MsgBox("ECU programmer not defined for this .bin file")
        End Select

    End Sub

    Private Sub CleanUpFDTDirectory(ByVal FDTPath As String)
        Dim di As New IO.DirectoryInfo(FDTPath)
        Dim diar1 As IO.FileInfo() = di.GetFiles()
        Dim dra As IO.FileInfo
        Dim delcount As Integer
        delcount = 0
        '
        'Cleanup all BB34*.bin files which are older than -14 days
        '
        For Each dra In diar1
            If dra.CreationTime < (My.Computer.Clock.LocalTime.AddDays(-14)) Then
                If dra.FullName.Contains(".bin") And dra.FullName.Contains("BB34") Then
                    File.Delete(dra.FullName)
                    delcount = delcount + 1
                End If
            End If
        Next

    End Sub

    Private Function FName(ByVal p As String) As String
        Dim testFile As System.IO.FileInfo
        testFile = My.Computer.FileSystem.GetFileInfo(p)
        Dim folderPath As String = testFile.DirectoryName
        Dim fileName As String = testFile.Name
        Return (fileName)
    End Function

    Public Shared Function StrToByteArray(ByVal str As String) As Byte()
        Dim encoding As New System.Text.ASCIIEncoding()
        Return encoding.GetBytes(str)
    End Function 'StrToByteArray

    Private Sub RenesasFDT()

        Dim path As String
        Dim shpath As String
        Dim curpath As String
        Dim FDTpath As String
        Dim FDTfile As String
        Dim MOTpath As String
        Dim MOTfile As String
        Dim i As Integer
        Dim b() As Byte
        Dim s_record As String
        Dim r0, r1, r2, r3, r4 As Integer
        Dim l As Integer
        Dim cks As Integer
        Dim cb(4) As Byte


        ' program assumes FDT.exe in one of the following directory
        FDTpath = My.Settings.Item("FDTpath")
        If Not File.Exists(FDTpath & "FDT.EXE") Then
            ' check out for the known versions, assume latest known being used if found
            If File.Exists("C:\Program Files\Renesas\FDT3.07\" & "FDT.EXE") Then FDTpath = "C:\Program Files\Renesas\FDT3.07\"
            If File.Exists("C:\Program Files\Renesas\FDT4.00\" & "FDT.EXE") Then FDTpath = "C:\Program Files\Renesas\FDT4.00\"
            If File.Exists("C:\Program Files\Renesas\FDT4.01\" & "FDT.EXE") Then FDTpath = "C:\Program Files\Renesas\FDT4.01\"
            If File.Exists("C:\Program Files\Renesas\FDT4.02\" & "FDT.EXE") Then FDTpath = "C:\Program Files\Renesas\FDT4.02\"
            If File.Exists("C:\Program Files\Renesas\FDT4.03\" & "FDT.EXE") Then FDTpath = "C:\Program Files\Renesas\FDT4.03\"
            If File.Exists("C:\Program Files\Renesas\FDT4.04\" & "FDT.EXE") Then FDTpath = "C:\Program Files\Renesas\FDT4.04\"
            If File.Exists("C:\Program Files\Renesas\FDT4.05\" & "FDT.EXE") Then FDTpath = "C:\Program Files\Renesas\FDT4.05\"
            If File.Exists("C:\Program Files\Renesas\FDT4.06\" & "FDT.EXE") Then FDTpath = "C:\Program Files\Renesas\FDT4.06\"
            If File.Exists("C:\Program Files\Renesas\FDT4.1\" & "FDT.EXE") Then FDTpath = "C:\Program Files\Renesas\FDT4.1\"
            If File.Exists("C:\Program Files\Renesas\FDT4.10\" & "FDT.EXE") Then FDTpath = "C:\Program Files\Renesas\FDT4.10\"

            If File.Exists("C:\Program Files (x86)\Renesas\FDT3.07\" & "FDT.EXE") Then FDTpath = "C:\Program Files (x86)\Renesas\FDT3.07\"
            If File.Exists("C:\Program Files (x86)\Renesas\FDT4.00\" & "FDT.EXE") Then FDTpath = "C:\Program Files (x86)\Renesas\FDT4.00\"
            If File.Exists("C:\Program Files (x86)\Renesas\FDT4.01\" & "FDT.EXE") Then FDTpath = "C:\Program Files (x86)\Renesas\FDT4.01\"
            If File.Exists("C:\Program Files (x86)\Renesas\FDT4.02\" & "FDT.EXE") Then FDTpath = "C:\Program Files (x86)\Renesas\FDT4.02\"
            If File.Exists("C:\Program Files (x86)\Renesas\FDT4.03\" & "FDT.EXE") Then FDTpath = "C:\Program Files (x86)\Renesas\FDT4.03\"
            If File.Exists("C:\Program Files (x86)\Renesas\FDT4.04\" & "FDT.EXE") Then FDTpath = "C:\Program Files (x86)\Renesas\FDT4.04\"
            If File.Exists("C:\Program Files (x86)\Renesas\FDT4.05\" & "FDT.EXE") Then FDTpath = "C:\Program Files (x86)\Renesas\FDT4.05\"
            If File.Exists("C:\Program Files (x86)\Renesas\FDT4.06\" & "FDT.EXE") Then FDTpath = "C:\Program Files (x86)\Renesas\FDT4.06\"
            If File.Exists("C:\Program Files (x86)\Renesas\FDT4.1\" & "FDT.EXE") Then FDTpath = "C:\Program Files (x86)\Renesas\FDT4.1\"
            If File.Exists("C:\Program Files (x86)\Renesas\FDT4.10\" & "FDT.EXE") Then FDTpath = "C:\Program Files (x86)\Renesas\FDT4.10\"

            My.Settings.Item("FDTpath") = FDTpath ' store the latest know as the default_owner
        End If

        If Not File.Exists(FDTpath & "FDT.EXE") Then
            MsgBox("FDT.exe not found, please install from www.renesas.com/fdt. Only versions 3.07, 4.00, 4.01, 4.02 tested.", MsgBoxStyle.Information)
        Else
            ' FDT FOUND, flashing is possible
            ' first close the comport if its open
            Try
                If Datastream.SerialPort1.IsOpen Then
                    Datastream.SerialPort1.Close()
                    Datastream.Close()
                End If
            Catch ex As Exception
                Datastream.TextBox1.Text = ex.Message
            End Try

            '
            ' generate the bin to be flashed
            '
            FDTfile = "ecuflash.bin"
            path = FDTpath & FDTfile
            ' if the temporary file exists for any reason, delete it
            If File.Exists(path) = True Then
                File.Delete(path)
            End If
            ' create the temporary file from current memory
            fs = File.Open(path, FileMode.CreateNew)
            fs.Write(Flash, 0, 262144)
            fs.Close()
            '
            ' create also a .mot s-record file from current memory and use that instead of .bin
            '
            MOTfile = "ecuflash.mot"
            MOTpath = FDTpath & MOTfile
            ' if the .mot file exists for any reason, delete it
            If File.Exists(MOTpath) = True Then
                File.Delete(MOTpath)
            End If
            ' create the temporary file from current memory and calculate checksum (ones complement)
            fs = File.OpenWrite(MOTpath)

            For i = 0 To (262144 / 4) - 1
                ' read the flashword and set address
                r0 = i * 4
                r1 = ReadFlashByte((i * 4) + 0)
                r2 = ReadFlashByte((i * 4) + 1)
                r3 = ReadFlashByte((i * 4) + 2)
                r4 = ReadFlashByte((i * 4) + 3)
                ' calculate checksum
                cks = 9
                cks = cks + Val("&H" & Mid$(r0.ToString("x8"), 1, 2))
                If cks > 256 Then cks = cks - 256
                cks = cks + Val("&H" & Mid$(r0.ToString("x8"), 3, 2))
                If cks > 256 Then cks = cks - 256
                cks = cks + Val("&H" & Mid$(r0.ToString("x8"), 5, 2))
                If cks > 256 Then cks = cks - 256
                cks = cks + Val("&H" & Mid$(r0.ToString("x8"), 7, 2))
                If cks > 256 Then cks = cks - 256
                cks = cks + r1
                If cks > 256 Then cks = cks - 256
                cks = cks + r2
                If cks > 256 Then cks = cks - 256
                cks = cks + r3
                If cks > 256 Then cks = cks - 256
                cks = cks + r4
                If cks > 256 Then cks = cks - 256
                If cks <> 0 Then
                    cks = 255 - cks
                    If cks = &HFFFFFFFF Then cks = &HFF ' This will fix VB2008 bug
                End If

                ' generate and write the record
                s_record = "S309" & r0.ToString("x8") & r1.ToString("x2") & r2.ToString("x2") & r3.ToString("x2") & r4.ToString("x2") & cks.ToString("x2") & "cr"
                b = StrToByteArray(s_record)
                l = Len(s_record)
                ' add crlf
                b(l - 2) = 13
                b(l - 1) = 10
                ' write to file
                fs.Write(b, 0, l)
            Next


            fs.Close()


            ' start the programmer, first startup may require setting up the program
            ' just change first directory to FDT directory
            path = FDTpath & FDTfile
            curpath = Application.StartupPath
            ChDir(FDTpath)
            shpath = "FDT.exe /DISCRETESTARTUP ""SimpleInterfaceMode /r /u " & MOTfile ' use either MOTfile or FDTfile
            Shell(shpath, AppWinStyle.NormalFocus, True, -1)
            ChDir(curpath)
            ' dont delete the temporary file, in case system crashes it remains there for returnging it
            'If File.Exists(path) = True Then
            'File.Delete(path)
            'End If

            '
            ' Generate a backup copy of the flashed file just flashed
            '
            FDTfile = ECUID.Text & "-"
            FDTfile = FDTfile & My.Computer.Clock.LocalTime.Date.Day & "-"
            FDTfile = FDTfile & My.Computer.Clock.LocalTime.Date.Month & "-"
            FDTfile = FDTfile & My.Computer.Clock.LocalTime.Date.Year & "-"
            FDTfile = FDTfile & My.Computer.Clock.LocalTime.Hour & "-"
            FDTfile = FDTfile & My.Computer.Clock.LocalTime.Minute
            FDTfile = FDTfile & ".bin"
            path = FDTpath & FDTfile
            ' if the temporary file exists for any reason, delete it
            If File.Exists(path) = True Then
                File.Delete(path)
            End If
            ' create the temporary file from curren memory
            fs = File.Open(path, FileMode.CreateNew)
            fs.Write(Flash, 0, 262144)
            fs.Close()

            CleanUpFDTDirectory(FDTpath)
        End If


    End Sub

    Private Sub FlashSerial()

        Dim path As String
        Dim flashfile As String
        Dim binfile As String
        Dim binpath As String
        Dim cb(4) As Byte
        Dim FT_status As Long
        Dim lngHandle As Long
        Dim rxbyte, txbyte As Byte
        Dim rxqueue, txqueue, eventstat As Integer
        Dim i, x, y, comportnum As Integer
        Dim ACK As Integer = &H6
        Dim NAK As Integer = &H15
        Dim block As Integer
        Dim cp As Integer
        Dim buff(&HFF) As Byte
        Dim j As Integer
        Dim k As Integer
        Dim blk5 As Boolean
        Dim startaddr As Integer
        Dim modemstat As Integer
        Dim im, chksumflash, chksum As Long
        Dim blkF As Boolean
        Dim loopuntilack As Boolean
        Dim loopcount As Integer
        Dim do_once As Boolean
        Dim starttime As Date
        Dim endtime As Date
        Dim totaltime As TimeSpan

        endtime = Date.Now
        starttime = Date.Now
        totaltime = endtime.Subtract(starttime)
        K8FlashStatus.L_elapsedtime.Text = totaltime.Minutes & ":" & totaltime.Seconds

        K8FlashStatus.Show()
        K8FlashStatus.Progressbar_Flashstatus.Maximum = &HFF
        K8FlashStatus.Progressbar_Flashstatus.Value = 1


        K8FlashStatus.fmode.ForeColor = Color.DarkGray
        K8FlashStatus.Progressbar_Flashstatus.Value = 0
        K8FlashStatus.Progressbar_Flashstatus.Refresh()
        K8FlashStatus.Refresh()
        System.Windows.Forms.Application.DoEvents()

        '
        ' Lets calculate checksum for the bin
        '
        chksum = ReadFlashWord(&HFFFF8) 'old checksum
        WriteFlashWord(&HFFFF8, 0)
        For im = 0 To &HFFFFF
            If k = 0 Then
                chksumflash = chksumflash + (Flash(im) * &H100)
                k = 1
            Else
                k = 0
                chksumflash = chksumflash + Flash(im)
            End If
            If chksumflash > &HFFFF Then
                chksumflash = chksumflash - &H10000
            End If
        Next
        chksumflash = (&H5AA5 - chksumflash) And &HFFFF
        WriteFlashWord(&HFFFF8, chksumflash) 'new checksum to written to .bin

        If K8Datastream.Visible() Then
            K8Datastream.closeenginedatacomms()
        End If

        K8EngineDataViewer.Close()
        K8EngineDataLogger.Close()

        '
        ' Get the FTDI device handle based on com port number and leave that port open
        '
        B_FlashECU.Enabled = False ' can not restart while flashing active
        timeBeginPeriod(1)
        comportnum = Val(Mid$(My.Settings.Item("ComPort"), 4))
        FT_status = FT_GetNumberOfDevices(i, 0, &H80000000)
        i = i - 1
        For x = 0 To i
            FT_status = FT_Open(x, lngHandle) ' only one
            FT_status = FT_GetComPortNumber(lngHandle, y)
            If y = comportnum Then
                cp = x
                x = i
            End If
            FT_status = FT_Close(lngHandle)
        Next
        If FT_status <> 0 Then
            MsgBox("Could not open com port, please set correct port on K8 enginedata screen. Programming aborted, reset ecu and reprogram")
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            K8FlashStatus.Close()
            Return
        End If
        '
        ' Open, Reset, set timeouts and set baud rate
        '
        FT_status = FT_Open(cp, lngHandle)
        FT_status = FT_ResetDevice(lngHandle, 3)                                'set device to default status
        FT_status = FT_status + FT_Purge(lngHandle)                             'clear rx and tx buffers
        FT_status = FT_status + FT_SetBaudRate(lngHandle, 57600)                'set speed 57600
        FT_status = FT_status + FT_SetDataCharacteristics(lngHandle, 8, 1, 0)   ' 8bits ,1 stop, parity none
        FT_status = FT_status + FT_SetTimeouts(lngHandle, 50, 50)                 'rx and tx timeouts ms
        FT_status = FT_status + FT_SetLatencyTimer(lngHandle, 8)               'ms
        FT_status = FT_status + FT_SetUSBParameters(lngHandle, 4096, 4096)      'only rx is active by FTDI
        If FT_status <> 0 Then
            MsgBox("Could not set Com port parameters. Programming aborted, set correct com port for the interface using data monitoring screen")
            B_FlashECU.Enabled = True
            K8FlashStatus.Close()
            FT_status = FT_Close(lngHandle)
            Return
        End If

        '
        ' Lets test that the interface is in the programming mode
        '
        '***************************************************************************************************************************
        FT_status = FT_SetDtr(lngHandle) 'new for Interface V1.1
        System.Threading.Thread.Sleep(100)
        '****************************************************************************************************************************
        FT_status = FT_GetModemStatus(lngHandle, modemstat)
        If FT_status <> 0 Then
            MsgBox("Set the correct Com port for the interface using data monitoring screen")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            K8FlashStatus.Close()
            Return
        End If
        If Not ((modemstat = &H6000) Or (modemstat = &H6200)) Then
            MsgBox("Interface is not on or it is not in programming mode, set programming switch to programming mode and retry")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            '***************************************************************************************************************************
            FT_status = FT_ClrDtr(lngHandle) 'new for Interface V1.1
            '****************************************************************************************************************************
            FT_status = FT_Close(lngHandle)
            Return
        Else
            '
            ' Reset ecu
            '
            FT_status = FT_SetRts(lngHandle)
            System.Threading.Thread.Sleep(300)
            FT_status = FT_ClrRts(lngHandle)
            System.Threading.Thread.Sleep(300)
        End If


        i = 0
        rxqueue = 0
        '
        ' Sync baud rate with ecu 18 x 0x00, get ack as a reply
        '
        x = 18 'default is 18
        For i = 1 To x
            txbyte = &H0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(40)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            If rxqueue <> 0 Then i = x
        Next
        System.Threading.Thread.Sleep(2)
        For x = 1 To rxqueue
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        Next
        If (rxbyte <> ACK) Then
            MsgBox("Unexpected or missing ECU response during intialization. Programming aborted, reset ecu and reprogram." & Hex(rxqueue) & " " & Hex(rxbyte))
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            '***************************************************************************************************************************
            FT_status = FT_ClrDtr(lngHandle) 'new for Interface V1.1
            '****************************************************************************************************************************
            FT_status = FT_Close(lngHandle)
            Return
        End If
        '
        ' check key status and send key if necessary
        '
        rxbyte = 0
        i = 0
        rxqueue = 0
        While (rxqueue = 0) And (i < 10)
            txbyte = &H70
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(40)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            i = i + 1
        End While
        For x = 1 To rxqueue
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        Next
        If rxbyte <> &H8C Then
            txbyte = &HF5
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H84
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HC
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H53
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H55
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H45
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H46
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H49
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H4D
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H56
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H30
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            '
            ' Receive ACK if unlock code succesfull
            '
            System.Threading.Thread.Sleep(100)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            For i = 1 To rxqueue
                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
            Next
            If rxbyte <> ACK Then
                MsgBox("No ACK received after sending unlock code. Programming aborted, reset ecu and reprogram")
                K8FlashStatus.Close()
                B_FlashECU.Enabled = True
                '***************************************************************************************************************************
                FT_status = FT_ClrDtr(lngHandle) 'new for Interface V1.1
                '****************************************************************************************************************************
                FT_status = FT_Close(lngHandle)
                Return
            End If
        End If
        '
        ' Check status after unlock code
        '
        txqueue = 0
        i = 0
        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        System.Threading.Thread.Sleep(50)
        While rxqueue = 0 And i < 10
            txbyte = &H70
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(40)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            i = i + 1
        End While
        If (i >= 10) Or (rxqueue = 0) Then
            MsgBox("Error in validating the unlock code from ECU. Programming aborted, reset ecu and reprogram")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            '***************************************************************************************************************************
            FT_status = FT_ClrDtr(lngHandle) 'new for Interface V1.1
            '****************************************************************************************************************************
            FT_status = FT_Close(lngHandle)
            Return
        Else
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1) '128
            System.Threading.Thread.Sleep(50)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1) '140
            System.Threading.Thread.Sleep(50)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        End If
        If (rxbyte <> &H8C) Or (FT_status <> 0) Then
            MsgBox("Was not able to set the ecu key. Programming aborted, reset ecu and reprogram")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            '***************************************************************************************************************************
            FT_status = FT_ClrDtr(lngHandle) 'new for Interface V1.1
            '****************************************************************************************************************************
            FT_status = FT_Close(lngHandle)
            Return
        End If
        '
        ' Clear status register just in case
        '
        txbyte = &H50
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        System.Threading.Thread.Sleep(50)
        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        For i = 1 To rxqueue
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        Next
        If rxbyte <> ACK Then
            MsgBox("Status query error 1. Programming aborted, reset ecu and reprogram")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            '***************************************************************************************************************************
            FT_status = FT_ClrDtr(lngHandle) 'new for Interface V1.1
            '****************************************************************************************************************************
            FT_status = FT_Close(lngHandle)
            Return
        End If

        '
        ' New command added, to be tested
        '
        'timeBeginPeriod(0)

        '
        ' Lets verify that this really is suzuki hayabusa ecu, flashing any other ecu type may damage the ecu and the bike
        '
        System.Threading.Thread.Sleep(100)
        txbyte = &HFF
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        txbyte = &HFF
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        txbyte = &HF
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        System.Threading.Thread.Sleep(100)
        k = 0
        Dim s As String
        s = ""
        For j = 0 To &HFF
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            For i = 1 To rxqueue
                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                '
                ' Lets test that the ecu id matches close enought to Hayabusa ecu
                ' if rxbyte = &HFF then its likely that the ecu has been fully erased and can be reflashed
                '
                If (k >= &HF0) And (k <= &HF5) Then
                    If (rxbyte <> ReadFlashByte(&HFFF00 + k)) And (rxbyte <> &HFF) Then
                        If MsgBox("Not same ECU ID in memory and inside the ecu. You can stop the flashing by pressing cancel.", MsgBoxStyle.OkCancel) = MsgBoxResult.Cancel Then
                            K8FlashStatus.Close()
                            B_FlashECU.Enabled = True
                            '***************************************************************************************************************************
                            FT_status = FT_ClrDtr(lngHandle) 'new for Interface V1.1
                            '****************************************************************************************************************************
                            FT_status = FT_Close(lngHandle)
                        End If
                    End If
                End If
                k = k + 1
            Next
        Next

        If ECUVersion = "gen2" Then

            '
            ' Lets read what is the flashingmode in ecu if memory is set to fast flashmode
            ' if fastflash then...
            '
            If ReadFlashLongWord(&H51F10) = &H536C4 Then
                System.Threading.Thread.Sleep(100)
                txbyte = &HFF
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                txbyte = &H1F
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                txbyte = &H5
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                System.Threading.Thread.Sleep(100)
                k = 0
                For j = 0 To &HFF
                    FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                    For i = 1 To rxqueue
                        FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                        If k = 19 Then
                            i = ReadFlashByte(&H51F10 + 3)
                            Select Case rxbyte
                                Case &H18 ' stock setting not fastmode, reflash block 5 is memory in fast mode
                                    If ReadFlashLongWord(&H51F10) = &H536C4 Then
                                        blk5 = True
                                    Else
                                        blk5 = False
                                    End If
                                Case &HC4 ' ecu already in fast mode, no reflashing is needed
                                    blk5 = False
                                Case &HFF ' block5 is empty, may be reflashing error. reflash block 5
                                    blk5 = True
                                Case Else
                                    MsgBox("Error in reading flashingmode from ECU, programming aborted. Please reboot ecu and reflash")
                                    BlockPgm = True
                                    K8FlashStatus.Close()
                                    B_FlashECU.Enabled = True
                                    '***************************************************************************************************************************
                                    FT_status = FT_ClrDtr(lngHandle) 'new for Interface V1.1
                                    '****************************************************************************************************************************
                                    FT_status = FT_Close(lngHandle)
                            End Select
                        End If
                        k = k + 1
                    Next
                Next
            End If

        End If


        timeBeginPeriod(1)
        System.Threading.Thread.Sleep(300)

        '
        ' For a reason or another block 0 requires full erase
        '
        If BlockChanged(0) = True Then
            BlockPgm = True
        End If

        blkF = False ' this is just used for check sum testing

        '
        ' Here is an erase for the full ecu
        '
        If BlockPgm Then
            endtime = Date.Now
            totaltime = endtime.Subtract(starttime)
            K8FlashStatus.L_elapsedtime.Text = totaltime.Minutes & ":" & totaltime.Seconds
            K8FlashStatus.fmode.Text = "Performing full erase, please wait"
            K8FlashStatus.fmode.ForeColor = Color.Gray
            K8FlashStatus.Progressbar_Flashstatus.Value = 0
            K8FlashStatus.Progressbar_Flashstatus.Refresh()
            K8FlashStatus.Refresh()
            System.Windows.Forms.Application.DoEvents()
            '
            ' Send Erase full memory command
            '
            txbyte = &HA7
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HD0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            '
            ' ECU confirms a succesfull erase by sending ACK
            '
            loopcount = 0
            loopuntilack = False
            While Not loopuntilack
                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                While (rxqueue = 0) And (i < 100)
                    System.Threading.Thread.Sleep(50)
                    FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                    i = i + 1
                End While
                For i = 1 To rxqueue
                    FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                    If rxbyte = ACK Then
                        loopuntilack = True
                    End If
                Next
                If loopcount > 10 Then
                    '
                    ' Clear program lock bit and status register
                    '
                    txbyte = &H75
                    FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                    FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                    FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                    txbyte = &H50
                    FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                    FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                    FT_Read_Bytes(lngHandle, rxbyte, 1, 1)

                End If
                If loopcount > 20 Then
                    MsgBox("No ACK after full erase, Programming aborted, reset ecu and reprogram.")
                    K8FlashStatus.Close()
                    B_FlashECU.Enabled = True
                    '***************************************************************************************************************************
                    FT_status = FT_ClrDtr(lngHandle) 'new for Interface V1.1
                    '****************************************************************************************************************************
                    FT_status = FT_Close(lngHandle)
                    BlockPgm = True
                    Return
                End If
                loopcount = loopcount + 1
            End While
        End If


        '
        ' Now programmings starts
        '
        If ECUVersion = "gen2" Then

            j = ReadFlashLongWord(&H51F10)
            If ReadFlashLongWord(&H51F10) <> &H536C4 Then
                K8FlashStatus.fmode.Text = "Normal flash "
            Else
                K8FlashStatus.fmode.Text = "Fast flash "
            End If
        Else
            K8FlashStatus.fmode.Text = "Normal flash "
        End If

        K8FlashStatus.fmode.ForeColor = Color.Black
        K8FlashStatus.Progressbar_Flashstatus.Value = 0
        K8FlashStatus.Progressbar_Flashstatus.Refresh()
        K8FlashStatus.Refresh()
        System.Windows.Forms.Application.DoEvents()

        startaddr = 0
        For block = startaddr To &HF
            ' BlockChanged returns true if there has been any changes to that block
            ' BlockPgm is a global variable that forces all blocks to be written
            If BlockChanged(block) Or BlockPgm Then

                endtime = Date.Now
                totaltime = endtime.Subtract(starttime)
                K8FlashStatus.L_elapsedtime.Text = totaltime.Minutes & ":" & totaltime.Seconds
                K8FlashStatus.fmode.Text = K8FlashStatus.fmode.Text & Hex(block)
                K8FlashStatus.Progressbar_Flashstatus.Refresh()
                K8FlashStatus.Refresh()
                System.Windows.Forms.Application.DoEvents()

                If block = &HF Then blkF = True


                '
                ' Erase block
                '
                txbyte = &H20
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                txbyte = &H0
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                txbyte = block
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                txbyte = &HD0
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)

                '
                ' ECU confirms a succesfull erase by sending ACK
                '

                loopcount = 0
                loopuntilack = False

                While Not loopuntilack
                    FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                    i = 0
                    While (rxqueue = 0) And (i < 100)
                        System.Threading.Thread.Sleep(50)
                        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                        i = i + 1
                    End While
                    For i = 1 To rxqueue
                        FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                        If rxbyte = ACK Then loopuntilack = True
                    Next
                    If rxbyte = NAK Then
                        '
                        ' Lets inform the user that something is wrong
                        '
                        K8FlashStatus.fmode.ForeColor = Color.Orange
                        '
                        ' Clear status register
                        '
                        System.Threading.Thread.Sleep(200)
                        txbyte = &H50
                        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                        i = 0
                        While (rxqueue = 0) And (i < 10)
                            System.Threading.Thread.Sleep(50)
                            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                            i = i + 1
                        End While
                        For i = 1 To rxqueue
                            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                        Next
                        '
                        ' Disable lock bit
                        '
                        System.Threading.Thread.Sleep(200)
                        txbyte = &H75
                        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                        i = 0
                        While (rxqueue = 0) And (i < 10)
                            System.Threading.Thread.Sleep(50)
                            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                            i = i + 1
                        End While
                        For i = 1 To rxqueue
                            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                        Next
                        '
                        ' Erase block again
                        '
                        System.Threading.Thread.Sleep(200)
                        txbyte = &H20
                        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                        txbyte = &H0
                        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                        txbyte = block
                        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                        txbyte = &HD0
                        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                        System.Threading.Thread.Sleep(200)
                    End If
                    If loopcount > 100 Then
                        MsgBox("No ACK after erasing a block=" & Str(block) & " Programming aborted, reset ecu and reprogram")
                        K8FlashStatus.Close()
                        B_FlashECU.Enabled = True
                        '***************************************************************************************************************************
                        FT_status = FT_ClrDtr(lngHandle) 'new for Interface V1.1
                        '****************************************************************************************************************************
                        FT_status = FT_Close(lngHandle)
                        BlockPgm = True
                        Return
                    End If
                    endtime = Date.Now
                    totaltime = endtime.Subtract(starttime)
                    K8FlashStatus.L_elapsedtime.Text = totaltime.Minutes & ":" & totaltime.Seconds
                    K8FlashStatus.L_elapsedtime.Text = TimeOfDay
                    K8FlashStatus.Progressbar_Flashstatus.Value = loopcount
                    K8FlashStatus.Progressbar_Flashstatus.Refresh()
                    K8FlashStatus.Refresh()
                    System.Windows.Forms.Application.DoEvents()

                    loopcount = loopcount + 1
                End While

                rxqueue = 0
                i = 0

                '
                ' Write block using page write
                '
                K8FlashStatus.fmode.ForeColor = Color.Black

                Dim page As Integer
                For page = 0 To &HFF
                    '
                    ' Write one page at time
                    '
                    endtime = Date.Now
                    totaltime = endtime.Subtract(starttime)
                    K8FlashStatus.L_elapsedtime.Text = totaltime.Minutes & ":" & totaltime.Seconds
                    K8FlashStatus.Progressbar_Flashstatus.Value = page
                    K8FlashStatus.Progressbar_Flashstatus.Refresh()
                    K8FlashStatus.Refresh()
                    System.Windows.Forms.Application.DoEvents()
                    '
                    ' Check if the page is filled with 0xFF, no need to program
                    '
                    i = 0
                    For y = 0 To &HFF
                        buff(y) = ReadFlashByte((block * &H10000) + (page * &H100) + y)
                        If buff(y) <> &HFF Then
                            i = i + 1
                        End If
                    Next
                    If i > 0 Then ' there is something in the page that is not 0xFF


                        loopcount = 0
                        loopuntilack = False

                        While Not loopuntilack
                            '
                            ' write a page
                            '
                            txbyte = &H41
                            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                            txbyte = page
                            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                            txbyte = block
                            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                            For y = 0 To &HFF
                                txbyte = buff(y)
                                'FT_status = FT_Write_Bytes(lngHandle, txbyte, 1, txqueue)
                            Next

                            FT_status = FT_Write(lngHandle, buff, &HFF + 1, &HFF + 1)

                            '
                            ' this should be ack from page write
                            '
                            rxbyte = 0
                            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                            i = 0
                            While (rxqueue = 0) And (i < 30)
                                System.Threading.Thread.Sleep(25)
                                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                                If rxqueue > 0 Then i = 30
                                i = i + 1
                            End While
                            For i = 1 To rxqueue
                                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                                If rxbyte = ACK Then loopuntilack = True
                            Next
                            If loopcount > 5 Then
                                '
                                ' Clear program lock bit and status register
                                '
                                K8FlashStatus.fmode.ForeColor = Color.Orange
                                K8FlashStatus.Refresh()
                                System.Windows.Forms.Application.DoEvents()
                                System.Threading.Thread.Sleep(100)

                                txbyte = &H75
                                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                                System.Threading.Thread.Sleep(100)
                                rxbyte = 0
                                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                                i = 0
                                While (rxqueue = 0) And (i < 30)
                                    System.Threading.Thread.Sleep(25)
                                    FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                                    If rxqueue > 0 Then i = 30
                                    i = i + 1
                                End While
                                For i = 1 To rxqueue
                                    FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                                Next

                                txbyte = &H50
                                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                                System.Threading.Thread.Sleep(100)
                                rxbyte = 0
                                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                                i = 0
                                While (rxqueue = 0) And (i < 30)
                                    System.Threading.Thread.Sleep(25)
                                    FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                                    If rxqueue > 0 Then i = 30
                                    i = i + 1
                                End While
                                For i = 1 To rxqueue
                                    FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                                Next

                            End If
                            If loopcount > 10 Then

                                MsgBox("No ACK after writing a block=" & Str(block) & " page=" & Str(page) & ". Programming aborted, reset ecu and reprogram")
                                K8FlashStatus.Close()
                                B_FlashECU.Enabled = True
                                '***************************************************************************************************************************
                                FT_status = FT_ClrDtr(lngHandle) 'new for Interface V1.1
                                '****************************************************************************************************************************
                                FT_status = FT_Close(lngHandle)
                                BlockPgm = True
                                Return
                            End If
                            loopcount = loopcount + 1
                        End While

                    End If
                Next
            End If
        Next


        '
        ' Acquire sum value and compare checksum. This will be done every time as flashing always
        ' starts by writing a new checksum to the image in computer memory
        '
        txbyte = &HE1
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        txbyte = 0
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        txbyte = 0
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        txbyte = &HFF
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        txbyte = &HF
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        System.Threading.Thread.Sleep(200)
        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        If rxqueue <> 2 Then MsgBox("Error in reading checksum from ecu")
        FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        k = rxbyte
        FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        k = (k) + (rxbyte * &H100)


        '
        ' Flashing is finished, wait until switch is flipped back and then close com and activate enginedata if visible
        '
        do_once = True

        timeEndPeriod(1)

        If k <> &H5AA5 Then
            MsgBox("Checksum error when validating the flash, please reflash your ecu before using it.")
            K8FlashStatus.fmode.Text = "Checksum error, please reprogram"
            ResetBlocks()
            BlockPgm = True
        Else
            K8FlashStatus.fmode.Text = "Flash OK, turn switch to enginedata"
            ResetBlocks()
            BlockPgm = False
        End If


        endtime = Date.Now
        totaltime = endtime.Subtract(starttime)
        K8FlashStatus.L_elapsedtime.Text = totaltime.Minutes & ":" & totaltime.Seconds
        K8FlashStatus.Refresh()
        System.Windows.Forms.Application.DoEvents()
        '***************************************************************************************************************************
        FT_status = FT_ClrDtr(lngHandle) 'new for Interface V1.1
        System.Threading.Thread.Sleep(100)
        '****************************************************************************************************************************

        'Enable the Close button on the K8 Flash Screen
        K8FlashStatus.CloseEnabled = True

        FT_status = FT_GetModemStatus(lngHandle, modemstat)

        While ((modemstat = &H6000) Or (modemstat = &H6200))

            Application.DoEvents()

            If K8FlashStatus.ClosedStatus = True Then
                Exit While
            End If

            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            System.Threading.Thread.Sleep(200)

            modemstat = 0
            FT_status = FT_GetModemStatus(lngHandle, modemstat)
        End While

        K8FlashStatus.Close()
        B_FlashECU.Enabled = True

        'FT_status = FT_ClrDtr(lngHandle) 'new for Interface V1.1

        FT_status = FT_Close(lngHandle)
        If (FT_status = 0) Then
            If K8Datastream.Visible() Then
                K8Datastream.startenginedatacomms()
            End If
        Else
            MsgBox("Can not close com port, please save the bin and reboot your computer and reflash just in case.")
        End If

        '
        ' generate the bin that was flashed as a backup file
        '
        flashfile = "ecuflash.bin"
        binpath = Application.StartupPath
        binfile = "ecuflash.bin"
        path = binpath & flashfile
        ' if the temporary file exists for any reason, delete it
        If File.Exists(path) = True Then
            File.Delete(path)
        End If
        ' create the temporary file from current memory
        fs = File.Open(path, FileMode.CreateNew)
        fs.Write(Flash, 0, (262144 * 4))
        fs.Close()
        '
        ' Generate a backup copy of the flashed file just flashed
        '
        binfile = ECUID.Text & "-"
        binfile = binfile & My.Computer.Clock.LocalTime.Date.Day & "-"
        binfile = binfile & My.Computer.Clock.LocalTime.Date.Month & "-"
        binfile = binfile & My.Computer.Clock.LocalTime.Date.Year & "-"
        binfile = binfile & My.Computer.Clock.LocalTime.Hour & "-"
        binfile = binfile & My.Computer.Clock.LocalTime.Minute
        binfile = binfile & ".bin"
        path = binpath & binfile
        ' if the temporary file exists for any reason, delete it
        If File.Exists(path) = True Then
            File.Delete(path)
        End If
        ' create the temporary file from curren memory
        fs = File.Open(path, FileMode.CreateNew)
        fs.Write(Flash, 0, (262144 * 4))
        fs.Close()
        ' cleanup of old backup files
        CleanUpFDTDirectory(binpath)


    End Sub

    Private Sub BlockOK(ByVal b As Integer)
        Select Case b
            Case b = 0 : Block0 = False
            Case b = 1 : Block1 = False
            Case b = 2 : Block2 = False
            Case b = 3 : Block3 = False
            Case b = 4 : Block4 = False
            Case b = 5 : Block5 = False
            Case b = 6 : Block6 = False
            Case b = 7 : Block7 = False
            Case b = 8 : Block8 = False
            Case b = 9 : Block9 = False
            Case b = 10 : BlockA = False
            Case b = 11 : BlockB = False
            Case b = 12 : BlockC = False
            Case b = 13 : BlockD = False
            Case b = 14 : BlockE = False
            Case b = 15 : BlockF = False
        End Select
    End Sub

    Sub ReadECU()
        '
        ' This subroutine reads the ecu contents into memory
        '

        Dim cb(4) As Byte
        Dim FT_status As Long
        Dim lngHandle As Long
        Dim rxbyte, txbyte As Byte
        Dim rxqueue, txqueue, eventstat As Integer
        Dim i, x, y, comportnum As Integer
        Dim ACK As Integer = &H6
        Dim NAK As Integer = &H15
        Dim cp As Integer
        Dim buff(&HFF) As Byte
        Dim j As Integer
        Dim k As Integer
        Dim modemstat As Integer
        Dim midorder, highorder As Integer
        Dim imagelength, im As Long
        Dim imageidentical As Boolean
        Dim diffstr As String
        Dim chksum As Integer
        Dim chksumflash As Long
        Dim chksumdiff As Boolean = False

        '
        ' Reset ECUID to null string
        '
        ECUID.Text = ""

        '
        ' Get the FTDI device handle based on com port number and leave that port open
        '
        timeBeginPeriod(1)
        comportnum = Val(Mid$(My.Settings.Item("ComPort"), 4))
        FT_status = FT_GetNumberOfDevices(i, 0, &H80000000)
        i = i - 1
        For x = 0 To i
            FT_status = FT_Open(x, lngHandle) ' only one
            FT_status = FT_GetComPortNumber(lngHandle, y)
            If y = comportnum Then
                cp = x
                x = i
            End If
            FT_status = FT_Close(lngHandle)
        Next
        If FT_status <> 0 Then
            MsgBox("Could not open com port, please set correct port on K8 enginedata screen. Verfiy aborted")
            FT_status = FT_Close(lngHandle)
            Return
        End If
        '
        ' Open, Reset, set timeouts and set baud rate
        '
        FT_status = FT_Open(cp, lngHandle)
        FT_status = FT_ResetDevice(lngHandle, 3)                                'set device to default status
        FT_status = FT_status + FT_Purge(lngHandle)                             'clear rx and tx buffers
        FT_status = FT_status + FT_SetBaudRate(lngHandle, 57600)                'set speed 57600
        FT_status = FT_status + FT_SetDataCharacteristics(lngHandle, 8, 1, 0)   ' 8bits ,1 stop, parity none
        FT_status = FT_status + FT_SetTimeouts(lngHandle, 5, 5)               'rx and tx timeouts ms
        If FT_status <> 0 Then
            MsgBox("Could not set Com port parameters. Verify aborted, set correct com port for the interface using data monitoring screen")
            FT_status = FT_Close(lngHandle)
            Return
        End If

        '
        ' Lets test that the interface is in the programming mode
        '
        FT_status = FT_GetModemStatus(lngHandle, modemstat)
        If FT_status <> 0 Then
            MsgBox("Set the correct Com port for the interface using data monitoring screen")
            Return
        End If
        If Not ((modemstat = &H6000) Or (modemstat = &H6200)) Then
            MsgBox("Interface is not on or it is not in programming mode, set programming switch to programming mode and retry")
            FT_status = FT_Close(lngHandle)
            Return
        Else
            '
            ' Reset ecu
            '
            FT_status = FT_SetRts(lngHandle)
            System.Threading.Thread.Sleep(300)
            FT_status = FT_ClrRts(lngHandle)
            System.Threading.Thread.Sleep(300)
        End If

        i = 0
        rxqueue = 0
        '
        ' Sync baud rate with ecu 18 x 0x00, get ack as a reply
        '
        x = 18 'default is 18
        For i = 1 To x
            txbyte = &H0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(40)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            If rxqueue <> 0 Then i = x
        Next
        System.Threading.Thread.Sleep(2)
        For x = 1 To rxqueue
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        Next


        If (rxbyte <> ACK) Then
            MsgBox("Unexpected or missing ECU response during intialization. Verify aborted, reset ecu and retry." & Hex(rxqueue) & " " & Hex(rxbyte))
            FT_status = FT_Close(lngHandle)
            Return
        End If

        '
        ' check key status and send key if necessary
        '
        rxbyte = 0
        i = 0
        rxqueue = 0
        While (rxqueue = 0) And (i < 10)
            txbyte = &H70
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(40)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            i = i + 1
        End While
        For x = 1 To rxqueue
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        Next
        If rxbyte <> &H8C Then
            txbyte = &HF5
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H84
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HC
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H53
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H55
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H45
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H46
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H49
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H4D
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H56
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H30
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            '
            ' Receive ACK if unlock code succesfull
            '
            System.Threading.Thread.Sleep(100)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            For i = 1 To rxqueue
                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
            Next
            If rxbyte <> ACK Then
                MsgBox("No ACK received after sending unlock code. Verify aborted, reset ecu and reprogram")
                FT_status = FT_Close(lngHandle)
                Return
            End If
        End If
        '
        ' Check status after unlock code
        '
        txqueue = 0
        i = 0
        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        System.Threading.Thread.Sleep(50)
        While rxqueue = 0 And i < 10
            txbyte = &H70
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(40)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            i = i + 1
        End While
        If (i >= 10) Or (rxqueue = 0) Then
            MsgBox("Error in validating the unlock code from ECU. Verify aborted, reset ecu and reprogram")
            FT_status = FT_Close(lngHandle)
            Return
        Else
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1) '128
            System.Threading.Thread.Sleep(50)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1) '140
            System.Threading.Thread.Sleep(50)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        End If
        If (rxbyte <> &H8C) Or (FT_status <> 0) Then
            MsgBox("Was not able to set the ecu key. Verify aborted, reset ecu and reprogram")
            FT_status = FT_Close(lngHandle)
            Return
        End If

        timeEndPeriod(1)

        '
        ' Lets read the ecu, &HFF, midorder, highorder -> 256 bytes of data in readbuffer
        ' if the checksum of read bytes and ecu command matches the read page will be then compared
        ' against whats in the memory. Variable flash(i) contains the information read from ecu
        ' the variable flashcopy(i) is the one that the verification against is made to.
        '
        VerifyInProgress.ProgressBar_Verify.Value = 50
        VerifyInProgress.Show()
        imageidentical = True
        k = 0
        imagelength = 0
        diffstr = ""

        'FT_SetTimeouts(lngHandle, 100, 200)

        For highorder = 0 To &HF
            For midorder = 0 To &HFF

                '
                ' Acquire sum value
                '
                txbyte = &HE1
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                txbyte = midorder
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                txbyte = highorder
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                txbyte = midorder
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                txbyte = highorder
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                System.Threading.Thread.Sleep(100)
                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                k = rxbyte
                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                chksum = (k) + (rxbyte * &H100)
                '
                ' Read Page until checksum match
                '
                chksumflash = -1
                im = imagelength
                While chksumflash <> chksum
                    '
                    ' Read page
                    '
                    txbyte = &HFF
                    FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                    txbyte = midorder
                    FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                    txbyte = highorder
                    FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                    Dim s As String
                    s = ""
                    j = 0
                    k = 0
                    chksumflash = 0
                    System.Threading.Thread.Sleep(100)
                    FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                    For i = 1 To rxqueue
                        FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                        Flash(imagelength) = rxbyte
                        If k = 0 Then
                            chksumflash = chksumflash + (rxbyte * &H100)
                            k = 1
                        Else
                            k = 0
                            chksumflash = chksumflash + rxbyte
                        End If
                        If chksumflash > &HFFFF Then
                            chksumflash = chksumflash - &H10000
                        End If
                        imagelength = imagelength + 1
                    Next
                    If chksumflash <> chksum Then
                        '
                        ' Block is being reread, lets make a note of it and also return the memory counter back to previous state
                        '
                        If Len(diffstr) < 100 Then diffstr = diffstr & "R"
                        imagelength = im
                        System.Threading.Thread.Sleep(100)
                    End If
                End While
                '
                ' Now block is read and we can check if the block contents is identical in flashcopy as read from ecu
                '
                For j = 0 To &HFF
                    If Flash((midorder * &H100) + (highorder * &H10000) + j) <> FlashCopy((midorder * &H100) + (highorder * &H10000) + j) Then
                        If (imageidentical <> False) Then
                            If (highorder = &HF) And (midorder = &HFF) And ((j = &HF8) Or j = &HF9) Then
                                '
                                ' The only difference is checksum so no trouble here
                                '
                                chksumdiff = True
                            End If
                        Else
                            '
                            ' Not a checksum difference image is not identical
                            '
                            imageidentical = False
                        End If
                        If Len(diffstr) < 100 Then diffstr = diffstr & " " & Hex((midorder * &H100) + (highorder * &H10000) + j)
                    End If
                Next
                VerifyInProgress.Select()
                VerifyInProgress.L_Txt.Text = Hex(highorder) & " " & Hex(midorder) & " - " & imageidentical & " " & diffstr & " "
                VerifyInProgress.ProgressBar_Verify.Value = Int(((highorder * &HF) + Int(midorder / &HF)) / 2.56)
                VerifyInProgress.Refresh()
                System.Windows.Forms.Application.DoEvents()
            Next
        Next
        VerifyInProgress.Close()



        '
        ' All done, close comms
        '
        B_FlashECU.Enabled = True

        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        If FT_status = 0 Then
            FT_status = FT_Close(lngHandle)
        Else
            MsgBox("Can not close com port, please reboot your ecu and computer and reread just in case.")
        End If

        '
        ' Here add the filename etc stuff to reflect that new bin has been loaded
        '
        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()


        If imageidentical Then
            If chksumdiff Then
                MsgBox("Ecu verify complete, image same as comparemap with different checksum. Ecu image is in ecueditor memory.")
            Else
                MsgBox("Ecu verify complete, image same as comparemap with checksum match. Ecu image is in ecueditor memory.")
            End If
            ResetBlocks()
            BlockPgm = False

        Else
            '
            ' Check that the binary lenght matches just in case
            '
            If imagelength <> (262144 * 4) Then
                ECUNotSupported.ShowDialog()
            End If
            '
            ' Same lenght, just inform the user that he has a new bin in the memory
            '
            MsgBox("Ecu image is not same as comparemap. Ecu image copied into ecueditor memory.", MsgBoxStyle.Exclamation)
            BlockPgm = True
        End If

        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        Do While i < 8
            On Error Resume Next ' in case one of the characters is not displayable
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        SetECUType()

    End Sub

    Private Sub EraseECU()

        Dim cb(4) As Byte
        Dim FT_status As Long
        Dim lngHandle As Long
        Dim rxbyte, txbyte As Byte
        Dim rxqueue, txqueue, eventstat As Integer
        Dim i, x, y, comportnum As Integer
        Dim ACK As Integer = &H6
        Dim NAK As Integer = &H15
        Dim cp As Integer
        Dim buff(&HFF) As Byte
        Dim modemstat As Integer
        Dim loopuntilack As Boolean
        Dim loopcount As Integer
        Dim do_once As Boolean


        If K8Datastream.Visible() Then
            K8Datastream.closeenginedatacomms()
        End If
        '
        ' Get the FTDI device handle based on com port number and leave that port open
        '
        B_FlashECU.Enabled = False ' can not restart while flashing active
        timeBeginPeriod(1)
        comportnum = Val(Mid$(My.Settings.Item("ComPort"), 4))
        FT_status = FT_GetNumberOfDevices(i, 0, &H80000000)
        i = i - 1
        For x = 0 To i
            FT_status = FT_Open(x, lngHandle) ' only one
            FT_status = FT_GetComPortNumber(lngHandle, y)
            If y = comportnum Then
                cp = x
                x = i
            End If
            FT_status = FT_Close(lngHandle)
        Next
        If FT_status <> 0 Then
            MsgBox("Could not open com port, please set correct port on K8 enginedata screen. Erase aborted, reset ecu and reprogram")
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            K8FlashStatus.Close()
            Return
        End If
        '
        ' Open, Reset, set timeouts and set baud rate
        '
        FT_status = FT_Open(cp, lngHandle)
        FT_status = FT_ResetDevice(lngHandle, 3)                                'set device to default status
        FT_status = FT_status + FT_Purge(lngHandle)                             'clear rx and tx buffers
        FT_status = FT_status + FT_SetBaudRate(lngHandle, 57600)                'set speed 57600
        FT_status = FT_status + FT_SetDataCharacteristics(lngHandle, 8, 1, 0)   ' 8bits ,1 stop, parity none
        FT_status = FT_status + FT_SetTimeouts(lngHandle, 5, 5)               'rx and tx timeouts ms
        If FT_status <> 0 Then
            MsgBox("Could not set Com port parameters. Programming aborted, set correct com port for the interface using data monitoring screen")
            B_FlashECU.Enabled = True
            K8FlashStatus.Close()
            FT_status = FT_Close(lngHandle)
            Return
        End If

        '
        ' Lets test that the interface is in the programming mode
        '
        FT_status = FT_GetModemStatus(lngHandle, modemstat)
        If FT_status <> 0 Then
            MsgBox("Set the correct Com port for the interface using data monitoring screen")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            K8FlashStatus.Close()
            Return
        End If
        If Not ((modemstat = &H6000) Or (modemstat = &H6200)) Then
            MsgBox("Interface is not on or it is not in programming mode, set programming switch to programming mode and retry")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        Else
            '
            ' Reset ecu
            '
            FT_status = FT_SetRts(lngHandle)
            System.Threading.Thread.Sleep(300)
            FT_status = FT_ClrRts(lngHandle)
            System.Threading.Thread.Sleep(300)
        End If


        i = 0
        rxqueue = 0
        '
        ' Sync baud rate with ecu 18 x 0x00, get ack as a reply
        '
        x = 18 'default is 18
        For i = 1 To x
            txbyte = &H0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(40)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            If rxqueue <> 0 Then i = x
        Next
        System.Threading.Thread.Sleep(2)
        For x = 1 To rxqueue
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        Next
        If (rxbyte <> ACK) Then
            MsgBox("Unexpected or missing ECU response during intialization. Programming aborted, reset ecu and reprogram." & Hex(rxqueue) & " " & Hex(rxbyte))
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        End If
        '
        ' check key status and send key if necessary
        '
        rxbyte = 0
        i = 0
        rxqueue = 0
        While (rxqueue = 0) And (i < 10)
            txbyte = &H70
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(40)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            i = i + 1
        End While
        For x = 1 To rxqueue
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        Next
        If rxbyte <> &H8C Then
            txbyte = &HF5
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H84
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HC
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H53
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H55
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H45
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H46
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H49
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H4D
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H56
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H30
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            '
            ' Receive ACK if unlock code succesfull
            '
            System.Threading.Thread.Sleep(100)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            For i = 1 To rxqueue
                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
            Next
            If rxbyte <> ACK Then
                MsgBox("No ACK received after sending unlock code. Programming aborted, reset ecu and reprogram")
                K8FlashStatus.Close()
                B_FlashECU.Enabled = True
                FT_status = FT_Close(lngHandle)
                Return
            End If
        End If
        '
        ' Check status after unlock code
        '
        txqueue = 0
        i = 0
        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        System.Threading.Thread.Sleep(50)
        While rxqueue = 0 And i < 10
            txbyte = &H70
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(40)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            i = i + 1
        End While
        If (i >= 10) Or (rxqueue = 0) Then
            MsgBox("Error in validating the unlock code from ECU. Programming aborted, reset ecu and reprogram")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        Else
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1) '128
            System.Threading.Thread.Sleep(50)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1) '140
            System.Threading.Thread.Sleep(50)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        End If
        If (rxbyte <> &H8C) Or (FT_status <> 0) Then
            MsgBox("Was not able to set the ecu key. Programming aborted, reset ecu and reprogram")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        End If
        '
        ' Clear status register just in case
        '
        txbyte = &H50
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        System.Threading.Thread.Sleep(50)
        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        For i = 1 To rxqueue
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        Next
        If rxbyte <> ACK Then
            MsgBox("Status query error 1. Programming aborted, reset ecu and reprogram")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        End If

        '
        K8FlashStatus.fmode.Text = "Performing full erase, please wait"
        K8FlashStatus.fmode.ForeColor = Color.Gray
        K8FlashStatus.Progressbar_Flashstatus.Value = 0
        K8FlashStatus.Progressbar_Flashstatus.Refresh()
        K8FlashStatus.Refresh()
        System.Windows.Forms.Application.DoEvents()
        '
        ' Send Erase full memory command
        '
        txbyte = &HA7
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        txbyte = &HD0
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        '
        ' ECU confirms a succesfull erase by sending ACK
        '
        loopcount = 0
        loopuntilack = False
        While Not loopuntilack
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            While (rxqueue = 0) And (i < 100)
                System.Threading.Thread.Sleep(50)
                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                i = i + 1
            End While
            For i = 1 To rxqueue
                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                If rxbyte = ACK Then
                    loopuntilack = True
                End If
            Next
            If loopcount > 10 Then
                '
                ' Clear program lock bit and status register
                '
                txbyte = &H75
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                txbyte = &H50
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)

            End If
            If loopcount > 20 Then
                MsgBox("No ACK after full erase, Programming aborted, reset ecu and reprogram.")
                K8FlashStatus.Close()
                B_FlashECU.Enabled = True
                FT_status = FT_Close(lngHandle)
                BlockPgm = True
                Return
            End If
            loopcount = loopcount + 1
        End While

        '
        ' Flashing is finished, wait until switch is flipped back and then close com and activate enginedata if visible
        '
        do_once = True

        timeEndPeriod(1)
        B_FlashECU.Enabled = True
        MsgBox("Full erase done, you can now flash the ecu")
        ResetBlocks()
        BlockPgm = True


        FT_status = FT_GetModemStatus(lngHandle, modemstat)
        K8FlashStatus.Close()
        B_FlashECU.Enabled = True

        FT_status = FT_Close(lngHandle)
        If (FT_status = 0) Then
            If K8Datastream.Visible() Then
                K8Datastream.startenginedatacomms()
            End If
        Else
            MsgBox("Can not close com port")
        End If



    End Sub

    Public Sub SetECUType()

        '
        ' here we set ecutype based on ecuid and then enable respective windows
        ' fast search key  1111
        '

        '
        ' here we set ecutype based on ecuid and then enable respective windows
        ' fast search key 111111
        '

        '
        ' Zero the variables so that values need to be assigned by this function
        '
        gixxer_modelname = ""
        gixxer_injectorbalance_map_first = 0 '&H370648
        gixxer_injectorbalance_map_name = gixxer_modelname

        gixxer_shifter_ADJ = 0 '&H5D900
        gixxer_shifter_FUELCODE = 0 '&H5DA00
        gixxer_shifter_IGNCODE = 0 '&H5DE00
        gixxer_shifter_jmp_to_fuelcode = 0 '&H45A40
        gixxer_shifter_jmp_to_igncode = 0 '&H3B9C0
        gixxer_abc = 0 '&H220C4

        gixxer_STP_map_first_table = 0 '&H59DD0
        gixxer_STP_map_first_table_fuel = 0
        gixxer_STP_modelname = ""

        gixxer_fuelmap_map_first = 0 '&H5A7F0
        gixxer_fuelmap_map_bikename = gixxer_modelname
        gixxer_ignition_rpm_limiter = 0 '&H60B2C
        gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = 0 'OK

        gixxer_fuel_limiter_by_gear = 0 '&H614C1
        gixxer_fuel_limiter_by_gear_softcut = 0 '&H614C4
        gixxer_RPM_limit_type1 = 0 '&H61372
        gixxer_fuel_limiter_softcut_or_hardcut = 0 '&H614BE
        gixxer_baseline = 0 '13450

        gixxer_ignition_map_first = 0 '&H5A3D8
        gixxer_ignition_map_name = ""
        gixxer_ignition_use_clutch_map = 0 '&H60B5C

        gixxer_msmode = 0 'OK
        gixxer_coilfi = 0 'OK
        gixxer_fan = 0 'OK
        gixxer_pair = 0 'OK
        gixxer_pairloop = 0 '&H56D5C
        gixxer_excva = 0 'OK
        gixxer_excva_flag = 0 '&H60669
        gixxer_ecumode = 0 'OK
        gixxer_ics1 = 0 'OK
        gixxer_ics2 = 0 '&H6230A
        gixxer_ics3 = 0 '&H62296

        gixxer_hoxflag = 0 'OK
        gixxer_hox1 = 0 '&H614D4
        gixxer_hox2 = 0 '&H62243

        gixxer_sdmap = 0 'gixxer_sdmap = &H5D840
        gixxer_sd1 = 0 'OK
        gixxer_sd2 = 0 '&H62ACF
        gixxer_sd3 = 0 '&H62AD2

        gixxerramair_columnheader = 0 ' &H7268E
        gixxerramair_map = 0 '&H72859

        gixxer_baud_rate = 0
        gixxer_baud_rate_timer1 = 0
        gixxer_baud_rate_timer2 = 0

        gixxer_wideband1_original = 0
        gixxer_wideband2_original = 0
        gixxer_wideband3_original = 0
        gixxer_wideband4_original = 0

        gixxer_wideband1 = 0
        gixxer_wideband2 = 0

        Hayabusa.Visible = True
        FlashToolStripMenuItem.Visible = True
        Select Case Mid(ECUID.Text, 1, 8)
            Case "DJ18SE10"
                Hayabusa.Text = "Hayabusa gen2 USA 32920-15H10"
                Metric = False
                ECUVersion = "gen2"
            Case "DJ18SE11"
                Hayabusa.Text = "Hayabusa gen2 USA 32920-15H10"
                Metric = False
                ECUVersion = "gen2"
            Case "DJ18SE20"
                Hayabusa.Text = "Hayabusa gen2 Cali 32920-15H20"
                Metric = False
                ECUVersion = "gen2"
            Case "DJ18SE00"
                Hayabusa.Text = "Hayabusa gen2 EU 32920-15H00"
                Metric = True
                ECUVersion = "gen2"
            Case "DJ18SE55"
                Hayabusa.Text = "Hayabusa gen2 Generic 32920-15H00"
                Metric = True
                ECUVersion = "gen2"
            Case "DJ47SE01"
                Hayabusa.Text = "Bking EU and AU"
                Metric = True
                ECUVersion = "bking"
            Case "DJ47SE10"
                Hayabusa.Text = "Bking Canada"
                Metric = False
                ECUVersion = "bking"
            Case "DJ47SE20"
                Hayabusa.Text = "Bking USA (California)"
                Metric = False
                ECUVersion = "bking"
            Case "DJB5SE01"

                gixxer_modelname = "ecueditor.com for gixxer K9-K11 "
                ECUVersion = "gixxer"
                Hayabusa.Text = "Gixxer K9-11 32920-47H00"

                B_FuelMap.Enabled = True
                gixxer_fuelmap_map_first = &H5CFE8
                gixxer_injectorbalance_map_first = &H5CFD0

                B_IgnitionMap.Enabled = True
                gixxer_ignition_map_first = &H5CB70
                gixxer_ignition_use_clutch_map = &H7438A
                gixxer_ignition_map_name = gixxer_modelname

                B_Limiters.Enabled = True
                gixxer_fuel_limiter_by_gear = &H74F3B
                gixxer_fuel_limiter_by_gear_softcut = &H74F3C
                gixxer_RPM_limit_type1 = &H74DDE
                gixxer_fuel_limiter_softcut_or_hardcut = &H74F38
                gixxer_baseline = 13500
                gixxer_ignition_rpm_limiter = &H74344

                B_AdvancedSettings.Enabled = True
                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = &H3CE9D
                gixxer_GPS_voltage_raw = &H805120
                gixxer_set_ign_default = &H74385

                gixxer_STP_map_first_table = &H5C450
                gixxer_STP_map_first_table_fuel = &H5D168

                gixxer_excva = &H7000D
                gixxer_excva_flag = &H73ECF
                gixxer_msmode = &H73E9E
                gixxer_ecumode = &H73D2F

                gixxer_abc = 0 '&H220C4
                gixxer_coilfi = 0 '&H60BC1
                gixxer_fan = 0 '&H6296A

                gixxer_pair = &H7A382
                gixxer_pairloop = &H594F4
                gixxer_pairloop1 = &HFF
                gixxer_pairloop2 = &HF9
                gixxer_pairloop3 = &H7F

                gixxer_ics1 = 0
                gixxer_ics2 = 0
                gixxer_ics3 = 0

                gixxer_hoxflag = &H74F4E
                gixxer_hox1 = gixxer_hoxflag
                gixxer_hox2 = &H79BBB

                gixxer_sdmap = &H606CC
                gixxer_sd1 = &H7A398 '&H62AC1
                gixxer_sd2 = &H7A395 '&H62ACF
                gixxer_sd3 = &H7A398 '&H62AD2

                B_Shifter.Enabled = True
                gixxer_shifter_ADJ = &H60800
                gixxer_shifter_FUELCODE = &H60A00
                gixxer_shifter_IGNCODE = &H60F00
                gixxer_shifter_jmp_to_fuelcode = &H478E8
                gixxer_shifter_jmp_to_igncode = &H3D3A4

                gixxerramair_columnheader = &H73FDE ' &H7268E
                gixxerramair_map = &H741A9 '&H72859

                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                gixxer_baud_rate = &H13FE9
                gixxer_baud_rate_timer1 = &H13902
                gixxer_baud_rate_timer2 = &H1397E

                gixxer_wideband1_original = &H7
                gixxer_wideband2_original = &HA1FF
                gixxer_wideband3_original = &H80
                gixxer_wideband4_original = &H6B3B

                gixxer_wideband1 = &H5D354
                gixxer_wideband2 = &H5D384

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True

            Case "DJB5SE10"
                gixxer_modelname = "ecueditor.com for gixxer K9-K11 "
                ECUVersion = "gixxer"
                Hayabusa.Text = "Gixxer K9-11 32920-47H10"


                B_FuelMap.Enabled = True
                gixxer_fuelmap_map_first = &H5CFE8
                gixxer_injectorbalance_map_first = &H5CFD0

                B_IgnitionMap.Enabled = True
                gixxer_ignition_map_first = &H5CB70
                gixxer_ignition_use_clutch_map = &H7438A
                gixxer_ignition_map_name = gixxer_modelname

                B_Limiters.Enabled = True
                gixxer_fuel_limiter_by_gear = &H74F3B
                gixxer_fuel_limiter_by_gear_softcut = &H74F3C
                gixxer_RPM_limit_type1 = &H74DDE
                gixxer_fuel_limiter_softcut_or_hardcut = &H74F38
                gixxer_baseline = 13500
                gixxer_ignition_rpm_limiter = &H74344

                B_AdvancedSettings.Enabled = True
                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = &H3CE9D
                gixxer_GPS_voltage_raw = &H805120
                gixxer_set_ign_default = &H74385

                gixxer_STP_map_first_table = &H5C450
                gixxer_STP_map_first_table_fuel = &H5D168

                gixxer_excva = &H7000D
                gixxer_excva_flag = &H73ECF
                gixxer_msmode = &H73E9E
                gixxer_ecumode = &H73D2F

                gixxer_abc = 0 '&H220C4
                gixxer_coilfi = 0 '&H60BC1
                gixxer_fan = 0 '&H6296A

                gixxer_pair = &H7A382
                gixxer_pairloop = &H594F4
                gixxer_pairloop1 = &HFF
                gixxer_pairloop2 = &HF9
                gixxer_pairloop3 = &H7F

                gixxer_ics1 = 0
                gixxer_ics2 = 0
                gixxer_ics3 = 0

                gixxer_hoxflag = &H74F4E
                gixxer_hox1 = gixxer_hoxflag
                gixxer_hox2 = &H79BBB

                gixxer_sdmap = &H606CC
                gixxer_sd1 = &H7A398 '&H62AC1
                gixxer_sd2 = &H7A395 '&H62ACF
                gixxer_sd3 = &H7A398 '&H62AD2

                B_Shifter.Enabled = True
                gixxer_shifter_ADJ = &H60800
                gixxer_shifter_FUELCODE = &H60A00
                gixxer_shifter_IGNCODE = &H60F00
                gixxer_shifter_jmp_to_fuelcode = &H478E8
                gixxer_shifter_jmp_to_igncode = &H3D3A4

                gixxerramair_columnheader = &H73FDE ' &H7268E
                gixxerramair_map = &H741A9 '&H72859

                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                gixxer_baud_rate = &H13FE9
                gixxer_baud_rate_timer1 = &H13902
                gixxer_baud_rate_timer2 = &H1397E

                gixxer_wideband1_original = &H7
                gixxer_wideband2_original = &HA1FF
                gixxer_wideband3_original = &H80
                gixxer_wideband4_original = &H6B3B

                gixxer_wideband1 = &H5D354
                gixxer_wideband2 = &H5D384

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True

            Case "DJ0HSE50"
                gixxer_modelname = "ecueditor.com for gixxer K8 "
                ECUVersion = "gixxer"

                gixxer_shifter_ADJ = &H5D900
                gixxer_shifter_FUELCODE = &H5DA00
                gixxer_shifter_IGNCODE = &H5DE00
                gixxer_shifter_jmp_to_fuelcode = &H45A40
                gixxer_shifter_jmp_to_igncode = &H3B9C0
                gixxer_abc = &H220C4

                gixxer_STP_map_first_table = &H59DD0
                gixxer_STP_map_first_table_fuel = &H5A970
                gixxer_injectorbalance_map_first = &H5A7D8

                gixxer_fuelmap_map_first = &H5A7F0
                gixxer_ignition_rpm_limiter = &H60B2C

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = &H3B4C1
                gixxer_GPS_voltage_raw = &H8050F9
                gixxer_set_ign_default = &H60B57


                gixxer_fuel_limiter_by_gear = &H614C1
                gixxer_fuel_limiter_by_gear_softcut = &H614C2
                gixxer_RPM_limit_type1 = &H61372
                gixxer_fuel_limiter_softcut_or_hardcut = &H614BE
                gixxer_baseline = 13500

                gixxer_ignition_map_first = &H5A3D8
                gixxer_ignition_use_clutch_map = &H60B5C
                gixxer_ignition_map_name = gixxer_modelname

                gixxerramair_columnheader = &H60776 ' &H7268E
                gixxerramair_map = &H60941 '&H72859

                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = True
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True

                B_AdvancedSettings.Enabled = True
                gixxer_msmode = &H6063A
                gixxer_coilfi = &H60BC1
                gixxer_fan = &H62968
                gixxer_pair = &H62ABA

                gixxer_pairloop = &H56D5C
                gixxer_pairloop1 = &HFF
                gixxer_pairloop2 = &HFC
                gixxer_pairloop3 = &H10

                gixxer_excva = &H6000D
                gixxer_excva_flag = &H60669
                gixxer_ecumode = &H604CF
                gixxer_ics1 = &H622EE
                gixxer_ics2 = &H6230A
                gixxer_ics3 = &H62296
                gixxer_hoxflag = &H614D4
                gixxer_hox1 = &H614D4
                gixxer_hox2 = &H62243
                gixxer_sdmap = &H5D840
                gixxer_sd1 = &H62AC1
                gixxer_sd2 = &H62ACF
                gixxer_sd3 = &H62AD2

                gixxer_baud_rate = &H13F91
                gixxer_baud_rate_timer1 = &H138AA
                gixxer_baud_rate_timer2 = &H13926

                gixxer_wideband1_original = &H6
                gixxer_wideband2_original = &H28F3
                gixxer_wideband3_original = &H80
                gixxer_wideband4_original = &H6760

                gixxer_wideband1 = &H59C98
                gixxer_wideband2 = &H59CC4

                Select Case ReadFlashByte(&H6292B)
                    Case &H35
                        ECUVersion = "gixxer"
                        Hayabusa.Text = "Gixxer K7- 32920-21H50"
                        Metric = True

                        gixxer_wideband1_original = &H6
                        gixxer_wideband2_original = &H2947
                        gixxer_wideband3_original = &H80
                        gixxer_wideband4_original = &H676D

                        gixxer_wideband1 = &H5AB04
                        gixxer_wideband2 = &H5AB34

                    Case &H36
                        ECUVersion = "gixxer"
                        Hayabusa.Text = "Gixxer K7- 32920-21H60"
                        Metric = False

                        gixxer_wideband1_original = &H6
                        gixxer_wideband2_original = &H2947
                        gixxer_wideband3_original = &H80
                        gixxer_wideband4_original = &H676D

                        gixxer_wideband1 = &H5AB04
                        gixxer_wideband2 = &H5AB34

                    Case &H37
                        ECUVersion = "gixxer"
                        Hayabusa.Text = "Gixxer K7- 32920-21H70"
                        Metric = False
                    Case &H39
                        ECUVersion = "gixxer"
                        Hayabusa.Text = "Gixxer K7- 32920-21Hxx Generic"
                        Metric = True
                    Case Else
                        Hayabusa.Text = "unknown gixxer ecu"
                        ECUVersion = "flashonly"
                End Select
            Case "DJ0HSE70"
                gixxer_modelname = "ecueditor.com for gixxer K8 (California)"
                ECUVersion = "gixxer"

                gixxer_shifter_ADJ = &H5D900
                gixxer_shifter_FUELCODE = &H5DA00
                gixxer_shifter_IGNCODE = &H5DE00
                gixxer_shifter_jmp_to_fuelcode = &H45A40
                gixxer_shifter_jmp_to_igncode = &H3B9C0
                gixxer_abc = &H220C4

                gixxer_STP_map_first_table = &H59DD0
                gixxer_STP_map_first_table_fuel = &H5A970
                gixxer_injectorbalance_map_first = &H5A7D8

                gixxer_fuelmap_map_first = &H5A7F0
                gixxer_ignition_rpm_limiter = &H60B2C

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = &H3B4C1
                gixxer_GPS_voltage_raw = &H8050F9
                gixxer_set_ign_default = &H60B57

                gixxer_fuel_limiter_by_gear = &H614C1
                gixxer_fuel_limiter_by_gear_softcut = &H614C2
                gixxer_RPM_limit_type1 = &H61372
                gixxer_fuel_limiter_softcut_or_hardcut = &H614BE
                gixxer_baseline = 13500

                gixxer_ignition_map_first = &H5A3D8
                gixxer_ignition_use_clutch_map = &H60B5C
                gixxer_ignition_map_name = gixxer_modelname

                gixxerramair_columnheader = &H60776 ' &H7268E
                gixxerramair_map = &H60941 '&H72859

                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = True
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True

                B_AdvancedSettings.Enabled = True
                gixxer_msmode = &H6063A
                gixxer_coilfi = &H60BC1
                gixxer_fan = &H62968
                gixxer_pair = &H62ABA

                gixxer_pairloop = &H56D5C
                gixxer_pairloop1 = &HFF
                gixxer_pairloop2 = &HFC
                gixxer_pairloop3 = &H10

                gixxer_excva = &H6000D
                gixxer_excva_flag = &H60669
                gixxer_ecumode = &H604CF
                gixxer_ics1 = &H622EE
                gixxer_ics2 = &H6230A
                gixxer_ics3 = &H62296
                gixxer_hoxflag = &H614D4
                gixxer_hox1 = &H614D4
                gixxer_hox2 = &H62243
                gixxer_sdmap = &H5D840
                gixxer_sd1 = &H62AC1
                gixxer_sd2 = &H62ACF
                gixxer_sd3 = &H62AD2

                gixxer_baud_rate = &H13F91
                gixxer_baud_rate_timer1 = &H138AA
                gixxer_baud_rate_timer2 = &H13926

                gixxer_wideband1_original = &H6
                gixxer_wideband2_original = &H2947
                gixxer_wideband3_original = &H80
                gixxer_wideband4_original = &H6761

                gixxer_wideband1 = &H5AB04
                gixxer_wideband2 = &H5AB24

                Select Case ReadFlashByte(&H6292B)
                    Case &H35
                        ECUVersion = "gixxer"
                        Hayabusa.Text = "Gixxer K7- 32920-21H50"
                        Metric = True
                    Case &H36
                        ECUVersion = "gixxer"
                        Hayabusa.Text = "Gixxer K7- 32920-21H60"
                        Metric = False
                    Case &H37
                        ECUVersion = "gixxer"
                        Hayabusa.Text = "Gixxer K7- 32920-21H70"
                        Metric = False
                    Case &H39
                        ECUVersion = "gixxer"
                        Hayabusa.Text = "Gixxer K7- 32920-21Hxx Generic"
                        Metric = True
                    Case Else
                        Hayabusa.Text = "unknown gixxer ecu"
                        ECUVersion = "flashonly"
                End Select

            Case "DJ0HSE51"
                gixxer_modelname = "ecueditor.com for gixxer K8 "
                ECUVersion = "gixxer"
                Hayabusa.Text = "Gixxer K7-K8 32920-21H50"

                gixxer_shifter_ADJ = &H5D900
                gixxer_shifter_FUELCODE = &H5DA00
                gixxer_shifter_IGNCODE = &H5DE00
                gixxer_shifter_jmp_to_fuelcode = &H45A40
                gixxer_shifter_jmp_to_igncode = &H3B9C0
                gixxer_abc = &H220C4

                gixxer_STP_map_first_table = &H59DD0
                gixxer_STP_map_first_table_fuel = &H5A970
                gixxer_injectorbalance_map_first = &H5A7D8

                gixxer_fuelmap_map_first = &H5A7F0
                gixxer_ignition_rpm_limiter = &H60B2C

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = &H3B4C1
                gixxer_GPS_voltage_raw = &H8050F9
                gixxer_set_ign_default = &H60B57

                gixxer_fuel_limiter_by_gear = &H614C1
                gixxer_fuel_limiter_by_gear_softcut = &H614C2
                gixxer_RPM_limit_type1 = &H61372
                gixxer_fuel_limiter_softcut_or_hardcut = &H614BE
                gixxer_baseline = 13500

                gixxer_ignition_map_first = &H5A3D8
                gixxer_ignition_use_clutch_map = &H60B5C
                gixxer_ignition_map_name = gixxer_modelname

                gixxerramair_columnheader = &H60776 ' &H7268E
                gixxerramair_map = &H60941 '&H72859

                gixxer_ecumode = &H604CF

                B_AdvancedSettings.Enabled = True
                gixxer_msmode = &H6063A
                gixxer_coilfi = &H60BC1
                gixxer_fan = &H62968

                gixxer_pair = &H62ABA
                gixxer_pairloop = &H56D5C
                gixxer_pairloop1 = &HFF
                gixxer_pairloop2 = &HFC
                gixxer_pairloop3 = &H10

                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                gixxer_baud_rate = &H13F91
                gixxer_baud_rate_timer1 = &H138AA
                gixxer_baud_rate_timer2 = &H13926

                gixxer_wideband1_original = &H6
                gixxer_wideband2_original = &H2947
                gixxer_wideband3_original = &H80
                gixxer_wideband4_original = &H676D

                gixxer_wideband1 = &H5AB04
                gixxer_wideband2 = &H5AB34

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = True
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True


            Case "DJ0HSE01"
                gixxer_modelname = "ecueditor.com for gixxer K8 "
                Hayabusa.Text = "Gixxer K7-K8"
                ECUVersion = "gixxer"

                gixxer_shifter_ADJ = &H5D900
                gixxer_shifter_FUELCODE = &H5DA00
                gixxer_shifter_IGNCODE = &H5DE00
                gixxer_shifter_jmp_to_fuelcode = &H45A40
                gixxer_shifter_jmp_to_igncode = &H3B9C0
                gixxer_abc = &H220C4

                gixxer_STP_map_first_table = &H59DD0
                gixxer_STP_map_first_table_fuel = &H5A970
                gixxer_injectorbalance_map_first = &H5A7D8

                gixxer_fuelmap_map_first = &H5A7F0
                gixxer_ignition_rpm_limiter = &H60B2C

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = &H3B4C1
                gixxer_GPS_voltage_raw = &H8050F9
                gixxer_set_ign_default = &H60B57

                gixxer_fuel_limiter_by_gear = &H614C1
                gixxer_fuel_limiter_by_gear_softcut = &H614C2
                gixxer_RPM_limit_type1 = &H61372
                gixxer_fuel_limiter_softcut_or_hardcut = &H614BE
                gixxer_baseline = 13500

                gixxer_ignition_map_first = &H5A3D8
                gixxer_ignition_use_clutch_map = &H60B5C
                gixxer_ignition_map_name = gixxer_modelname

                gixxerramair_columnheader = &H60776 ' &H7268E
                gixxerramair_map = &H60941 '&H72859

                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                gixxer_baud_rate = &H13F91
                gixxer_baud_rate_timer1 = &H138AA
                gixxer_baud_rate_timer2 = &H13926

                gixxer_wideband1_original = &H6
                gixxer_wideband2_original = &H2947
                gixxer_wideband3_original = &H80
                gixxer_wideband4_original = &H676D

                gixxer_wideband1 = &H5AB04
                gixxer_wideband2 = &H5AB34

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = True
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True


                Select Case ReadFlashByte(&H6292B)
                    Case &H35
                        ECUVersion = "gixxer"
                        Hayabusa.Text = "Gixxer K8- 32920-21H50"
                        Metric = True
                    Case &H36
                        ECUVersion = "gixxer"
                        Hayabusa.Text = "Gixxer K8- 32920-21H60"
                        Metric = False
                    Case &H39
                        ECUVersion = "gixxer"
                        Hayabusa.Text = "Gixxer K8- 32920-21Hxx Generic"
                        Metric = True
                    Case Else
                        Hayabusa.Text = "unknown gixxer ecu"
                        ECUVersion = "flashonly"
                End Select

                B_AdvancedSettings.Enabled = True
                gixxer_msmode = &H6063A
                gixxer_coilfi = &H60BC1
                gixxer_fan = &H62968

                gixxer_pair = &H62ABA
                gixxer_pairloop = &H56D5C
                gixxer_pairloop1 = &HFF
                gixxer_pairloop2 = &HFC
                gixxer_pairloop3 = &H10

                gixxer_excva = &H6000D
                gixxer_excva_flag = &H60669
                gixxer_hoxflag = &H614D4
                gixxer_ecumode = &H604CF
                gixxer_ics1 = &H622EE
                gixxer_ics2 = &H6230A
                gixxer_ics3 = &H62296
                gixxer_hox1 = &H614D4
                gixxer_hox2 = &H62243
                gixxer_sdmap = &H5D840
                gixxer_sd1 = &H62AC1
                gixxer_sd2 = &H62ACF
                gixxer_sd3 = &H62AD2


            Case "DJ0DSE11"
                Hayabusa.Text = "Gixxer K7- 32920-21H10"
                Metric = True
                ECUVersion = "gixxer"

                gixxer_modelname = "ecueditor.com for gixxer K7 (32920-21H10) "

                gixxer_shifter_ADJ = &H5D900
                gixxer_shifter_FUELCODE = &H5DA00
                gixxer_shifter_IGNCODE = &H5DE00
                gixxer_shifter_jmp_to_fuelcode = &H453CC        'last label at the end of function set_fuel_gear_type2hard
                gixxer_shifter_jmp_to_igncode = &H3B3C0         'last label at the end of function calculate_ignition_gear_limiters

                gixxer_STP_map_first_table = &H58F80            ' STP_gear06_ms01_modeabc_STP_map_list_
                gixxer_STP_map_first_table_fuel = &H59B04

                gixxer_fuelmap_map_first = &H59984              'Fuel_IAP
                gixxer_injectorbalance_map_first = &H5996C      'injector_balancing_volume_

                gixxer_ignition_rpm_limiter = &H60B14          'ignition_rpm_limiter

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = &H3AF15 'sub set_ignition_limiters beginning the GPS_sensor_RAW address
                gixxer_GPS_voltage_raw = &H8050F7
                gixxer_set_ign_default = &H60B3B

                gixxer_fuel_limiter_by_gear = &H614A1            'fuelcut_by_gear
                gixxer_RPM_limit_type1 = &H61356                 'RPM_limit_type1
                gixxer_fuel_limiter_softcut_or_hardcut = &H6149E 'fuel_limiter_softcut_or_hardcut
                gixxer_baseline = 13500

                gixxer_ignition_map_first = &H5956C               ' ign_group_clutch_neutral
                gixxer_ignition_use_clutch_map = &H60B40          ' use_clutch_map_if_0x1

                gixxer_ecumode = &H604CB

                gixxerramair_columnheader = &H60762 ' &H7268E
                gixxerramair_map = &H6092D '&H72859

                '
                ' These do not need to be adjusted
                '
                gixxer_ignition_map_name = gixxer_modelname
                gixxer_fuel_limiter_by_gear_softcut = gixxer_fuel_limiter_by_gear + 1
                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                gixxer_baud_rate = &H13F91
                gixxer_baud_rate_timer1 = &H138AA
                gixxer_baud_rate_timer2 = &H13926

                gixxer_wideband1_original = &H6
                gixxer_wideband2_original = &H28F3
                gixxer_wideband3_original = &H80
                gixxer_wideband4_original = &H6760

                gixxer_wideband1 = &H59C98
                gixxer_wideband2 = &H59CC4

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True


            Case "DJ0DSE04"
                Hayabusa.Text = "Gixxer K7- 32920-21H00"
                Metric = True
                ECUVersion = "gixxer"

                gixxer_modelname = "ecueditor.com for gixxer K7 (32920-21H00) "

                gixxer_shifter_ADJ = &H5D900
                gixxer_shifter_FUELCODE = &H5DA00
                gixxer_shifter_IGNCODE = &H5DE00
                gixxer_shifter_jmp_to_fuelcode = &H453CC        'last label at the end of function set_fuel_gear_type2hard
                gixxer_shifter_jmp_to_igncode = &H3B3C0         'last label at the end of function calculate_ignition_gear_limiters

                gixxer_STP_map_first_table = &H58F80            ' STP_gear06_ms01_modeabc_STP_map_list_
                gixxer_STP_map_first_table_fuel = &H59B04

                gixxer_fuelmap_map_first = &H59984              'Fuel_IAP
                gixxer_injectorbalance_map_first = &H5996C      'injector_balancing_volume_

                gixxer_ignition_rpm_limiter = &H60B14          'ignition_rpm_limiter

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = &H3AF15 'sub set_ignition_limiters beginning the GPS_sensor_RAW address
                gixxer_GPS_voltage_raw = &H8050F7
                gixxer_set_ign_default = &H60B3B

                gixxer_fuel_limiter_by_gear = &H614A1            'fuelcut_by_gear
                gixxer_RPM_limit_type1 = &H61356                 'RPM_limit_type1
                gixxer_fuel_limiter_softcut_or_hardcut = &H6149E 'fuel_limiter_softcut_or_hardcut
                gixxer_baseline = 13500

                gixxer_ignition_map_first = &H5956C               ' ign_group_clutch_neutral
                gixxer_ignition_use_clutch_map = &H60B40          ' use_clutch_map_if_0x1

                gixxerramair_columnheader = &H60762 ' &H7268E
                gixxerramair_map = &H6092D '&H72859
                gixxer_ecumode = &H604CB

                '
                ' These do not need to be adjusted
                '
                gixxer_ignition_map_name = gixxer_modelname
                gixxer_fuel_limiter_by_gear_softcut = gixxer_fuel_limiter_by_gear + 1
                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                gixxer_baud_rate = &H13F91
                gixxer_baud_rate_timer1 = &H138AA
                gixxer_baud_rate_timer2 = &H13926

                gixxer_wideband1_original = &H6
                gixxer_wideband2_original = &H28F3
                gixxer_wideband3_original = &H80
                gixxer_wideband4_original = &H6760

                gixxer_wideband1 = &H59C98
                gixxer_wideband2 = &H59CC4

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True

            Case "DJ65SE00"
                Hayabusa.Text = "Gixxer 750 EU K9- 32920-38H00"
                Metric = True
                ECUVersion = "gixxer"

                gixxer_modelname = "ecueditor.com for gixxer 750 EU K9 (32920-38H00) "

                gixxer_shifter_ADJ = 0
                gixxer_shifter_FUELCODE = 0
                gixxer_shifter_IGNCODE = 0
                gixxer_shifter_jmp_to_fuelcode = 0        'last label at the end of function set_fuel_gear_type2hard
                gixxer_shifter_jmp_to_igncode = 0         'last label at the end of function calculate_ignition_gear_limiters

                gixxer_STP_map_first_table = 0            ' STP_gear06_ms01_modeabc_STP_map_list_
                gixxer_STP_map_first_table_fuel = 0

                gixxer_fuelmap_map_first = &H5AC68              'Fuel_IAP
                gixxer_injectorbalance_map_first = 0      'injector_balancing_volume_

                gixxer_ignition_map_first = &H5A7F4              ' ign_group_clutch_neutral
                gixxer_ignition_use_clutch_map = &H743A4         ' use_clutch_map_if_0x1

                gixxer_msmode = &H73E8F
                gixxer_ecumode = &H73D2B

                gixxer_ignition_rpm_limiter = &H74374          'ignition_rpm_limiter

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = &H3BD74 + 1 'sub set_ignition_limiters beginning the GPS_sensor_RAW address
                gixxer_GPS_voltage_raw = &H80510D
                gixxer_set_ign_default = &H7439F

                gixxer_fuel_limiter_by_gear = &H74BE7            'fuelcut_by_gear
                gixxer_RPM_limit_type1 = &H74A92                'RPM_limit_type1
                gixxer_fuel_limiter_softcut_or_hardcut = &H74BE4 'fuel_limiter_softcut_or_hardcut
                gixxer_baseline = 14500

                gixxerramair_columnheader = 0
                gixxerramair_map = 0

                '
                ' These do not need to be adjusted
                '
                gixxer_ignition_map_name = gixxer_modelname
                gixxer_fuel_limiter_by_gear_softcut = gixxer_fuel_limiter_by_gear + 1
                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                gixxer_hoxflag = &H74BFA
                gixxer_hox1 = &H74BFA
                gixxer_hox2 = &H794AF

                Dim aaaa As Integer = &H13FD5
                gixxer_baud_rate = aaaa
                gixxer_baud_rate_timer1 = &H138EE
                gixxer_baud_rate_timer2 = &H1396A

                gixxer_wideband1_original = &H7
                gixxer_wideband2_original = &H9BBF
                gixxer_wideband3_original = &H80
                gixxer_wideband4_original = &H6AC6

                gixxer_wideband1 = &H5AFC4
                gixxer_wideband2 = &H5AFF0

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True

            Case "DJ65SE10"
                Hayabusa.Text = "Gixxer 750 US K9- 32920-38H10"
                Metric = True
                ECUVersion = "gixxer"

                gixxer_modelname = "ecueditor.com for gixxer 750 EU K9 (32920-38H10) "

                gixxer_shifter_ADJ = 0
                gixxer_shifter_FUELCODE = 0
                gixxer_shifter_IGNCODE = 0
                gixxer_shifter_jmp_to_fuelcode = 0        'last label at the end of function set_fuel_gear_type2hard
                gixxer_shifter_jmp_to_igncode = 0         'last label at the end of function calculate_ignition_gear_limiters

                gixxer_STP_map_first_table = 0            ' STP_gear06_ms01_modeabc_STP_map_list_
                gixxer_STP_map_first_table_fuel = 0

                gixxer_fuelmap_map_first = &H5AC68              'Fuel_IAP
                gixxer_injectorbalance_map_first = 0      'injector_balancing_volume_

                gixxer_ignition_map_first = &H5A7F4              ' ign_group_clutch_neutral
                gixxer_ignition_use_clutch_map = &H743A4         ' use_clutch_map_if_0x1

                gixxer_msmode = &H73E8F
                gixxer_ecumode = &H73D2B

                gixxer_ignition_rpm_limiter = &H74374          'ignition_rpm_limiter

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = &H3BD74 + 1 'sub set_ignition_limiters beginning the GPS_sensor_RAW address
                gixxer_GPS_voltage_raw = &H80510D
                gixxer_set_ign_default = &H7439F

                gixxer_fuel_limiter_by_gear = &H74BE7            'fuelcut_by_gear
                gixxer_RPM_limit_type1 = &H74A92                'RPM_limit_type1
                gixxer_fuel_limiter_softcut_or_hardcut = &H74BE4 'fuel_limiter_softcut_or_hardcut
                gixxer_baseline = 14500

                gixxerramair_columnheader = 0
                gixxerramair_map = 0

                '
                ' These do not need to be adjusted
                '
                gixxer_ignition_map_name = gixxer_modelname
                gixxer_fuel_limiter_by_gear_softcut = gixxer_fuel_limiter_by_gear + 1
                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                gixxer_hoxflag = &H74BFA
                gixxer_hox1 = &H74BFA
                gixxer_hox2 = &H794AF

                Dim aaaa As Integer = &H13FD5
                gixxer_baud_rate = aaaa
                gixxer_baud_rate_timer1 = &H138EE
                gixxer_baud_rate_timer2 = &H1396A

                gixxer_wideband1_original = &H7
                gixxer_wideband2_original = &H9BBF
                gixxer_wideband3_original = &H80
                gixxer_wideband4_original = &H6AC6

                gixxer_wideband1 = &H5AFC4
                gixxer_wideband2 = &H5AFF0

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True
            Case "DJF4SE00"
                Hayabusa.Text = "Gixxer K11- EU 32920-38H00"
                Metric = True
                ECUVersion = "gixxer"

                gixxer_modelname = "ecueditor.com for gixxer 750 K11 EU (32920-15J00) "

                gixxer_shifter_ADJ = 0
                gixxer_shifter_FUELCODE = 0
                gixxer_shifter_IGNCODE = 0
                gixxer_shifter_jmp_to_fuelcode = 0        'last label at the end of function set_fuel_gear_type2hard
                gixxer_shifter_jmp_to_igncode = 0         'last label at the end of function calculate_ignition_gear_limiters

                gixxer_STP_map_first_table = 0            ' STP_gear06_ms01_modeabc_STP_map_list_
                gixxer_STP_map_first_table_fuel = 0

                gixxer_fuelmap_map_first = &H6026C              'Fuel_IAP
                gixxer_injectorbalance_map_first = &H60254      'injector_balancing_volume_

                gixxer_ignition_map_first = &H5FAAC ' &H5A868              ' ign_group_clutch_neutral
                gixxer_ignition_use_clutch_map = &H743A4         ' use_clutch_map_if_0x1

                gixxer_msmode = 0 '&H73E8F
                gixxer_ecumode = 0 '&H73D2B

                gixxer_ignition_rpm_limiter = 0 '&H74374          'ignition_rpm_limiter

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = 0 ' &H3BD60 + 1 'sub set_ignition_limiters beginning the GPS_sensor_RAW address
                gixxer_GPS_voltage_raw = 0 '&H80510D
                gixxer_set_ign_default = 0 '&H7439F

                gixxer_fuel_limiter_by_gear = 0 '&H74BE7            'fuelcut_by_gear
                gixxer_RPM_limit_type1 = 0 '&H74A92                'RPM_limit_type1
                gixxer_fuel_limiter_softcut_or_hardcut = 0 '&H74BE4 'fuel_limiter_softcut_or_hardcut
                gixxer_baseline = 14500

                gixxerramair_columnheader = 0
                gixxerramair_map = 0


                '
                ' These do not need to be adjusted
                '
                gixxer_ignition_map_name = gixxer_modelname
                gixxer_fuel_limiter_by_gear_softcut = gixxer_fuel_limiter_by_gear + 1
                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True

            Case "DJF4SE10"
                Hayabusa.Text = "Gixxer K11- US 32920-38H10"
                Metric = True
                ECUVersion = "gixxer"

                gixxer_modelname = "ecueditor.com for gixxer 750 K11 EU (32920-15J10) "

                gixxer_shifter_ADJ = 0
                gixxer_shifter_FUELCODE = 0
                gixxer_shifter_IGNCODE = 0
                gixxer_shifter_jmp_to_fuelcode = 0        'last label at the end of function set_fuel_gear_type2hard
                gixxer_shifter_jmp_to_igncode = 0         'last label at the end of function calculate_ignition_gear_limiters

                gixxer_STP_map_first_table = 0            ' STP_gear06_ms01_modeabc_STP_map_list_
                gixxer_STP_map_first_table_fuel = 0

                gixxer_fuelmap_map_first = &H6026C              'Fuel_IAP
                gixxer_injectorbalance_map_first = &H60254      'injector_balancing_volume_

                gixxer_ignition_map_first = &H5FAAC ' &H5A868              ' ign_group_clutch_neutral
                gixxer_ignition_use_clutch_map = &H743A4         ' use_clutch_map_if_0x1

                gixxer_msmode = 0 '&H73E8F
                gixxer_ecumode = 0 '&H73D2B

                gixxer_ignition_rpm_limiter = 0 '&H74374          'ignition_rpm_limiter

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = 0 ' &H3BD60 + 1 'sub set_ignition_limiters beginning the GPS_sensor_RAW address
                gixxer_GPS_voltage_raw = 0 '&H80510D
                gixxer_set_ign_default = 0 '&H7439F

                gixxer_fuel_limiter_by_gear = 0 '&H74BE7            'fuelcut_by_gear
                gixxer_RPM_limit_type1 = 0 '&H74A92                'RPM_limit_type1
                gixxer_fuel_limiter_softcut_or_hardcut = 0 '&H74BE4 'fuel_limiter_softcut_or_hardcut
                gixxer_baseline = 14500

                gixxerramair_columnheader = 0
                gixxerramair_map = 0


                '
                ' These do not need to be adjusted
                '
                gixxer_ignition_map_name = gixxer_modelname
                gixxer_fuel_limiter_by_gear_softcut = gixxer_fuel_limiter_by_gear + 1
                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True
            Case "DJ64SE00"
                ' Fixed IAP map address by PKa 16.10.2011
                Hayabusa.Text = "Gixxer 750 EU K8- 32920-38H00 / 37H00"
                Metric = True
                ECUVersion = "gixxer"

                gixxer_modelname = "ecueditor.com " & Hayabusa.Text

                gixxer_shifter_ADJ = 0
                gixxer_shifter_FUELCODE = 0
                gixxer_shifter_IGNCODE = 0
                gixxer_shifter_jmp_to_fuelcode = 0        'last label at the end of function set_fuel_gear_type2hard
                gixxer_shifter_jmp_to_igncode = 0         'last label at the end of function calculate_ignition_gear_limiters

                gixxer_STP_map_first_table = 0            ' STP_gear06_ms01_modeabc_STP_map_list_
                gixxer_STP_map_first_table_fuel = 0

                gixxer_fuelmap_map_first = &H5AC54              'Fuel_IAP
                gixxer_injectorbalance_map_first = 0      'injector_balancing_volume_

                gixxer_ignition_map_first = &H5A7F4              ' ign_group_clutch_neutral
                gixxer_ignition_use_clutch_map = &H743A4         ' use_clutch_map_if_0x1

                gixxer_msmode = &H73E8F
                gixxer_ecumode = &H73D2B

                gixxer_ignition_rpm_limiter = &H74374          'ignition_rpm_limiter

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = &H3BD74 + 1 'sub set_ignition_limiters beginning the GPS_sensor_RAW address
                gixxer_GPS_voltage_raw = &H80510D
                gixxer_set_ign_default = &H7439F

                gixxer_fuel_limiter_by_gear = &H74BE7            'fuelcut_by_gear
                gixxer_RPM_limit_type1 = &H74A92                'RPM_limit_type1
                gixxer_fuel_limiter_softcut_or_hardcut = &H74BE4 'fuel_limiter_softcut_or_hardcut
                gixxer_baseline = 14500

                gixxerramair_columnheader = 0
                gixxerramair_map = 0

                '
                ' These do not need to be adjusted
                '
                gixxer_ignition_map_name = gixxer_modelname
                gixxer_fuel_limiter_by_gear_softcut = gixxer_fuel_limiter_by_gear + 1
                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True

            Case "DJ64SE01"
                ' Fixed IAP map address by PKa 16.10.2011
                Hayabusa.Text = "Gixxer 750 EU K8- 32920-38H00 / 37H00"
                Metric = True
                ECUVersion = "gixxer"

                gixxer_modelname = "ecueditor.com " & Hayabusa.Text

                gixxer_shifter_ADJ = 0
                gixxer_shifter_FUELCODE = 0
                gixxer_shifter_IGNCODE = 0
                gixxer_shifter_jmp_to_fuelcode = 0        'last label at the end of function set_fuel_gear_type2hard
                gixxer_shifter_jmp_to_igncode = 0         'last label at the end of function calculate_ignition_gear_limiters

                gixxer_STP_map_first_table = 0            ' STP_gear06_ms01_modeabc_STP_map_list_
                gixxer_STP_map_first_table_fuel = 0

                gixxer_fuelmap_map_first = &H5AC54              'Fuel_IAP
                gixxer_injectorbalance_map_first = 0      'injector_balancing_volume_

                gixxer_ignition_map_first = &H5A7F4              ' ign_group_clutch_neutral
                gixxer_ignition_use_clutch_map = &H743A4         ' use_clutch_map_if_0x1

                gixxer_msmode = &H73E8F
                gixxer_ecumode = &H73D2B

                gixxer_ignition_rpm_limiter = &H74374          'ignition_rpm_limiter

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = &H3BD74 + 1 'sub set_ignition_limiters beginning the GPS_sensor_RAW address
                gixxer_GPS_voltage_raw = &H80510D
                gixxer_set_ign_default = &H7439F

                gixxer_fuel_limiter_by_gear = &H74BE7            'fuelcut_by_gear
                gixxer_RPM_limit_type1 = &H74A92                'RPM_limit_type1
                gixxer_fuel_limiter_softcut_or_hardcut = &H74BE4 'fuel_limiter_softcut_or_hardcut
                gixxer_baseline = 14500

                gixxerramair_columnheader = 0
                gixxerramair_map = 0

                '
                ' These do not need to be adjusted
                '
                gixxer_ignition_map_name = gixxer_modelname
                gixxer_fuel_limiter_by_gear_softcut = gixxer_fuel_limiter_by_gear + 1
                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True

            Case "DJ55SE00"
                Hayabusa.Text = "Gixxer K8- EU 32920-37H00"
                Metric = True
                ECUVersion = "gixxer"

                gixxer_modelname = "ecueditor.com for gixxer 600 K8 EU (32920-37H00) "

                gixxer_shifter_ADJ = 0
                gixxer_shifter_FUELCODE = 0
                gixxer_shifter_IGNCODE = 0
                gixxer_shifter_jmp_to_fuelcode = 0        'last label at the end of function set_fuel_gear_type2hard
                gixxer_shifter_jmp_to_igncode = 0         'last label at the end of function calculate_ignition_gear_limiters

                gixxer_STP_map_first_table = 0            ' STP_gear06_ms01_modeabc_STP_map_list_
                gixxer_STP_map_first_table_fuel = 0

                gixxer_fuelmap_map_first = &H5AC54             'Fuel_IAP
                gixxer_injectorbalance_map_first = &H5AC3C     'injector_balancing_volume_

                gixxer_ignition_map_first = &H5A854              ' ign_group_clutch_neutral
                gixxer_ignition_use_clutch_map = &H743A4         ' use_clutch_map_if_0x1

                gixxer_msmode = &H73E8F
                gixxer_ecumode = &H73D2B

                gixxer_ignition_rpm_limiter = &H74374          'ignition_rpm_limiter

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = &H3BD60 + 1 'sub set_ignition_limiters beginning the GPS_sensor_RAW address
                gixxer_GPS_voltage_raw = &H80510D
                gixxer_set_ign_default = &H7439F

                gixxer_fuel_limiter_by_gear = &H74BE7            'fuelcut_by_gear
                gixxer_RPM_limit_type1 = &H74A92                'RPM_limit_type1
                gixxer_fuel_limiter_softcut_or_hardcut = &H74BE4 'fuel_limiter_softcut_or_hardcut
                gixxer_baseline = 15500

                gixxerramair_columnheader = 0
                gixxerramair_map = 0

                gixxer_excva = &H7000D
                gixxer_excva_flag = &H73EC0
                gixxer_hoxflag = &H74BFA
                gixxer_hox1 = gixxer_hoxflag
                gixxer_hox2 = &H794AF

                Dim aaaa As Integer = &H13FD5
                gixxer_baud_rate = aaaa
                gixxer_baud_rate_timer1 = &H138EE
                gixxer_baud_rate_timer2 = &H1396A

                gixxer_wideband1_original = &H7
                gixxer_wideband2_original = &H9BBF
                gixxer_wideband3_original = &H80
                gixxer_wideband4_original = &H6AC6

                gixxer_wideband1 = &H5AFB0
                gixxer_wideband2 = &H5AFDC

                '
                ' These do not need to be adjusted
                '
                gixxer_ignition_map_name = gixxer_modelname
                gixxer_fuel_limiter_by_gear_softcut = gixxer_fuel_limiter_by_gear + 1
                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True

            Case "DJE4SE01"
                Hayabusa.Text = "Gixxer K11- EU 32920-14J00"
                Metric = True
                ECUVersion = "gixxer"

                gixxer_modelname = "ecueditor.com for gixxer 600 K11- EU (32920-14J00) "

                gixxer_shifter_ADJ = 0
                gixxer_shifter_FUELCODE = 0
                gixxer_shifter_IGNCODE = 0
                gixxer_shifter_jmp_to_fuelcode = 0        'last label at the end of function set_fuel_gear_type2hard
                gixxer_shifter_jmp_to_igncode = 0         'last label at the end of function calculate_ignition_gear_limiters

                gixxer_STP_map_first_table = 0            ' STP_gear06_ms01_modeabc_STP_map_list_
                gixxer_STP_map_first_table_fuel = 0

                gixxer_fuelmap_map_first = &H6026C            'Fuel_IAP
                gixxer_injectorbalance_map_first = &H60254     'injector_balancing_volume_

                gixxer_ignition_map_first = &H5FA4C              ' ign_group_clutch_neutral
                gixxer_ignition_use_clutch_map = &H743A4         ' use_clutch_map_if_0x1

                gixxer_msmode = &H73E8F
                gixxer_ecumode = &H73D2B

                gixxer_ignition_rpm_limiter = &H74374          'ignition_rpm_limiter

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = 0 '&H3BD60 + 1 'sub set_ignition_limiters beginning the GPS_sensor_RAW address
                gixxer_GPS_voltage_raw = 0 '&H80510D
                gixxer_set_ign_default = 0 '&H7439F

                gixxer_fuel_limiter_by_gear = 0 '&H74BE7            'fuelcut_by_gear
                gixxer_RPM_limit_type1 = 0 '&H74A92                'RPM_limit_type1
                gixxer_fuel_limiter_softcut_or_hardcut = 0 '&H74BE4 'fuel_limiter_softcut_or_hardcut
                gixxer_baseline = 15500

                gixxerramair_columnheader = 0
                gixxerramair_map = 0


                '
                ' These do not need to be adjusted
                '
                gixxer_ignition_map_name = gixxer_modelname
                gixxer_fuel_limiter_by_gear_softcut = gixxer_fuel_limiter_by_gear + 1
                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True

            Case "DJE4SE10"
                Hayabusa.Text = "Gixxer K11- EU 32920-14J10"
                Metric = True
                ECUVersion = "gixxer"

                gixxer_modelname = "ecueditor.com for gixxer 600 K11- US (32920-14J10) "

                gixxer_shifter_ADJ = 0
                gixxer_shifter_FUELCODE = 0
                gixxer_shifter_IGNCODE = 0
                gixxer_shifter_jmp_to_fuelcode = 0        'last label at the end of function set_fuel_gear_type2hard
                gixxer_shifter_jmp_to_igncode = 0         'last label at the end of function calculate_ignition_gear_limiters

                gixxer_STP_map_first_table = 0            ' STP_gear06_ms01_modeabc_STP_map_list_
                gixxer_STP_map_first_table_fuel = 0

                gixxer_fuelmap_map_first = &H6026C            'Fuel_IAP
                gixxer_injectorbalance_map_first = &H60254     'injector_balancing_volume_

                gixxer_ignition_map_first = &H5FA4C              ' ign_group_clutch_neutral
                gixxer_ignition_use_clutch_map = &H743A4         ' use_clutch_map_if_0x1

                gixxer_msmode = &H73E8F
                gixxer_ecumode = &H73D2B

                gixxer_ignition_rpm_limiter = &H74374          'ignition_rpm_limiter

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = 0 '&H3BD60 + 1 'sub set_ignition_limiters beginning the GPS_sensor_RAW address
                gixxer_GPS_voltage_raw = 0 '&H80510D
                gixxer_set_ign_default = 0 '&H7439F

                gixxer_fuel_limiter_by_gear = 0 '&H74BE7            'fuelcut_by_gear
                gixxer_RPM_limit_type1 = 0 '&H74A92                'RPM_limit_type1
                gixxer_fuel_limiter_softcut_or_hardcut = 0 '&H74BE4 'fuel_limiter_softcut_or_hardcut
                gixxer_baseline = 15500

                gixxerramair_columnheader = 0
                gixxerramair_map = 0


                '
                ' These do not need to be adjusted
                '
                gixxer_ignition_map_name = gixxer_modelname
                gixxer_fuel_limiter_by_gear_softcut = gixxer_fuel_limiter_by_gear + 1
                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True

            Case "DJ55SE10"
                Hayabusa.Text = "Gixxer K8 EU 32920-37H10"
                Metric = True
                ECUVersion = "gixxer"

                gixxer_modelname = "ecueditor.com for gixxer 600 K8- US (32920-37H00) "

                gixxer_shifter_ADJ = 0
                gixxer_shifter_FUELCODE = 0
                gixxer_shifter_IGNCODE = 0
                gixxer_shifter_jmp_to_fuelcode = 0        'last label at the end of function set_fuel_gear_type2hard
                gixxer_shifter_jmp_to_igncode = 0         'last label at the end of function calculate_ignition_gear_limiters

                gixxer_STP_map_first_table = 0            ' STP_gear06_ms01_modeabc_STP_map_list_
                gixxer_STP_map_first_table_fuel = 0

                gixxer_fuelmap_map_first = &H5AC54             'Fuel_IAP
                gixxer_injectorbalance_map_first = &H5AC3C     'injector_balancing_volume_

                gixxer_ignition_map_first = &H5A854              ' ign_group_clutch_neutral
                gixxer_ignition_use_clutch_map = &H743A4         ' use_clutch_map_if_0x1

                gixxer_msmode = &H73E8F
                gixxer_ecumode = &H73D2B

                gixxer_ignition_rpm_limiter = &H74374          'ignition_rpm_limiter

                gixxer_GPS_AD_sensor_address_in_ignition_shiftkill = &H3BD60 + 1 'sub set_ignition_limiters beginning the GPS_sensor_RAW address
                gixxer_GPS_voltage_raw = &H80510D
                gixxer_set_ign_default = &H7439F

                gixxer_fuel_limiter_by_gear = &H74BE7            'fuelcut_by_gear
                gixxer_RPM_limit_type1 = &H74A92                'RPM_limit_type1
                gixxer_fuel_limiter_softcut_or_hardcut = &H74BE4 'fuel_limiter_softcut_or_hardcut
                gixxer_baseline = 15500

                gixxerramair_columnheader = 0
                gixxerramair_map = 0

                gixxer_excva = &H7000D
                gixxer_excva_flag = &H73EC0
                gixxer_hoxflag = &H74BFA
                gixxer_hox1 = gixxer_hoxflag
                gixxer_hox2 = &H794AF

                Dim aaaa As Integer = &H13FD5
                gixxer_baud_rate = aaaa
                gixxer_baud_rate_timer1 = &H138EE
                gixxer_baud_rate_timer2 = &H1396A

                gixxer_wideband1_original = &H7
                gixxer_wideband2_original = &H9BBF
                gixxer_wideband3_original = &H80
                gixxer_wideband4_original = &H6AC6

                gixxer_wideband1 = &H5AFB0
                gixxer_wideband2 = &H5AFDC

                '
                ' These do not need to be adjusted
                '
                gixxer_ignition_map_name = gixxer_modelname
                gixxer_fuel_limiter_by_gear_softcut = gixxer_fuel_limiter_by_gear + 1
                gixxer_injectorbalance_map_name = gixxer_modelname
                gixxer_STP_modelname = gixxer_modelname
                gixxer_fuelmap_map_bikename = gixxer_modelname

                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True

            Case "US______"
                Hayabusa.Text = "Enginedata only mode imperial"
                Metric = False
                ECUVersion = "enginedata"
                Limiters.C_RPM.Enabled = False
                B_Limiters.Enabled = False
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = False
                B_IgnitionMap.Enabled = False
                B_AdvancedSettings.Enabled = False
                B_DataLogging.Enabled = True
                B_FlashECU.Enabled = False
                SaveToolStripMenuItem.Enabled = False
            Case "EU______"
                Hayabusa.Text = "Enginedata only mode metric"
                Metric = True
                ECUVersion = "enginedata"
                Limiters.C_RPM.Enabled = False
                B_Limiters.Enabled = False
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = False
                B_IgnitionMap.Enabled = False
                B_AdvancedSettings.Enabled = False
                B_DataLogging.Enabled = True
                B_FlashECU.Enabled = False
                SaveToolStripMenuItem.Enabled = False
            Case Else
                Limiters.C_RPM.Enabled = False
                B_Limiters.Enabled = False
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = False
                B_IgnitionMap.Enabled = False
                B_AdvancedSettings.Enabled = False
                B_DataLogging.Enabled = True
                B_FlashECU.Enabled = False
                SaveToolStripMenuItem.Enabled = True
                MsgBox("ECU read into memory, but not recognized. please save as" & ECUID.Text & ".bin and send to info@WoolichRacing.com with notes about the bike and model.")
        End Select

        '
        ' Now lets enable functions available for different versions
        '
        Select Case ECUVersion
            Case "gen1"
                B_EngineData.Enabled = True
                FuelMap.Close()
                IgnitionMap.Close()
                FlashToolStripMenuItem.Visible = False
                B_DataLogging.Enabled = False
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = True
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True


            Case "gen2"
                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = True
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True


            Case "bking"
                B_EngineData.Enabled = True
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = True
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = True
                B_Shifter.Enabled = True
                B_FuelMap.Enabled = True
                B_IgnitionMap.Enabled = True
                B_AdvancedSettings.Enabled = True

            Case "gixxer"
                '
                ' do nothing settings are set earlier
                '
            Case Else
                '
                ' everything else is flashonly
                '
                B_EngineData.Enabled = False
                FlashToolStripMenuItem.Visible = Enabled
                B_DataLogging.Enabled = False
                SaveToolStripMenuItem.Enabled = True
                B_FlashECU.Enabled = True
                B_Limiters.Enabled = False
                B_Shifter.Enabled = False
                B_FuelMap.Enabled = False
                B_IgnitionMap.Enabled = False
                B_AdvancedSettings.Enabled = False

        End Select


    End Sub

    Private Sub TestCheckSum()

        Dim cb(4) As Byte
        Dim FT_status As Long
        Dim lngHandle As Long
        Dim rxbyte, txbyte As Byte
        Dim rxqueue, txqueue, eventstat As Integer
        Dim i, x, y, comportnum As Integer
        Dim ACK As Integer = &H6
        Dim NAK As Integer = &H15
        Dim cp As Integer
        Dim buff(&HFF) As Byte
        Dim k As Integer
        Dim modemstat As Integer
        Dim im, chksumflash As Long
        Dim do_once As Boolean
        Dim block As Integer
        Dim chksumdiff As Boolean
        Dim chksumfirmware As Integer



        If K8Datastream.Visible() Then
            K8Datastream.closeenginedatacomms()
        End If
        '
        ' Get the FTDI device handle based on com port number and leave that port open
        '
        B_FlashECU.Enabled = False ' can not restart while flashing active

        timeBeginPeriod(1)
        comportnum = Val(Mid$(My.Settings.Item("ComPort"), 4))
        FT_status = FT_GetNumberOfDevices(i, 0, &H80000000)
        i = i - 1
        For x = 0 To i
            FT_status = FT_Open(x, lngHandle) ' only one
            FT_status = FT_GetComPortNumber(lngHandle, y)
            If y = comportnum Then
                cp = x
                x = i
            End If
            FT_status = FT_Close(lngHandle)
        Next
        If FT_status <> 0 Then
            MsgBox("Could not open com port, please set correct port on K8 enginedata screen. Programming aborted, reset ecu and reprogram")
            B_FlashECU.Enabled = True

            FT_status = FT_Close(lngHandle)
            Return
        End If
        '
        ' Open, Reset, set timeouts and set baud rate
        '
        FT_status = FT_Open(cp, lngHandle)
        FT_status = FT_ResetDevice(lngHandle, 3)                                'set device to default status
        FT_status = FT_status + FT_Purge(lngHandle)                             'clear rx and tx buffers
        FT_status = FT_status + FT_SetBaudRate(lngHandle, 57600)                'set speed 57600
        FT_status = FT_status + FT_SetDataCharacteristics(lngHandle, 8, 1, 0)   ' 8bits ,1 stop, parity none
        FT_status = FT_status + FT_SetTimeouts(lngHandle, 5, 5)               'rx and tx timeouts ms
        If FT_status <> 0 Then
            MsgBox("Could not set Com port parameters. Programming aborted, set correct com port for the interface using data monitoring screen")
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        End If

        '
        ' Lets test that the interface is in the programming mode
        '
        FT_status = FT_GetModemStatus(lngHandle, modemstat)
        If FT_status <> 0 Then
            MsgBox("Set the correct Com port for the interface using data monitoring screen")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            K8FlashStatus.Close()
            Return
        End If
        If Not ((modemstat = &H6000) Or (modemstat = &H6200)) Then
            MsgBox("Interface is not on or it is not in programming mode, set programming switch to programming mode and retry")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        Else
            '
            ' Reset ecu
            '
            FT_status = FT_SetRts(lngHandle)
            System.Threading.Thread.Sleep(300)
            FT_status = FT_ClrRts(lngHandle)
            System.Threading.Thread.Sleep(300)
        End If


        i = 0
        rxqueue = 0
        '
        ' Sync baud rate with ecu 18 x 0x00, get ack as a reply
        '
        x = 18 'default is 18
        For i = 1 To x
            txbyte = &H0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(40)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            If rxqueue <> 0 Then i = x
        Next
        System.Threading.Thread.Sleep(2)
        For x = 1 To rxqueue
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        Next
        If (rxbyte <> ACK) Then
            MsgBox("Unexpected or missing ECU response during intialization. Programming aborted, reset ecu and reprogram." & Hex(rxqueue) & " " & Hex(rxbyte))
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        End If
        '
        ' check key status and send key if necessary
        '
        rxbyte = 0
        i = 0
        rxqueue = 0
        While (rxqueue = 0) And (i < 10)
            txbyte = &H70
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(40)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            i = i + 1
        End While
        For x = 1 To rxqueue
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        Next
        If rxbyte <> &H8C Then
            txbyte = &HF5
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H84
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HC
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H53
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H55
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H45
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H46
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H49
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H4D
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H56
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H30
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            '
            ' Receive ACK if unlock code succesfull
            '
            System.Threading.Thread.Sleep(100)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            For i = 1 To rxqueue
                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
            Next
            If rxbyte <> ACK Then
                MsgBox("No ACK received after sending unlock code. Test checksum aborted, reset ecu and reprogram")
                K8FlashStatus.Close()
                B_FlashECU.Enabled = True
                FT_status = FT_Close(lngHandle)
                Return
            End If
        End If
        '
        ' Check status after unlock code
        '
        txqueue = 0
        i = 0
        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        System.Threading.Thread.Sleep(50)
        While rxqueue = 0 And i < 10
            txbyte = &H70
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(40)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            i = i + 1
        End While
        If (i >= 10) Or (rxqueue = 0) Then
            MsgBox("Error in validating the unlock code from ECU. Test checksum aborted, reset ecu and reprogram")
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        Else
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1) '128
            System.Threading.Thread.Sleep(50)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1) '140
            System.Threading.Thread.Sleep(50)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        End If
        If (rxbyte <> &H8C) Or (FT_status <> 0) Then
            MsgBox("Was not able to set the ecu key. Programming aborted, reset ecu and reprogram")
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        End If
        '
        ' Clear status register just in case
        '
        txbyte = &H50
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        System.Threading.Thread.Sleep(50)
        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        For i = 1 To rxqueue
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        Next
        If rxbyte <> ACK Then
            MsgBox("Status query error 1. Test checksum aborted, reset ecu and reprogram")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        End If

        chksumdiff = False
        For block = 0 To &HF

            '
            ' Test checksum per block. In case the checksum does not match, set full
            ' flash flag that forces blocks 0x0 - 0xF flashing.
            '
            '
            ' Acquire sum value and compare checksum
            '
            txbyte = &HE1
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            txbyte = 0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            txbyte = block
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            txbyte = block
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(200)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            If rxqueue <> 2 Then
                MsgBox("Error in reading checksum from ecu")
                BlockPgm = True
            End If
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
            k = rxbyte
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
            k = (k) + (rxbyte * &H100)
            chksumfirmware = k

            '
            ' Lets calculate checksum for the bin in memory
            '
            chksumflash = 0
            k = 0
            For im = (0 + (block * &H10000)) To (&HFFFF + (block * &H10000))
                If k = 0 Then
                    'first highorder
                    chksumflash = chksumflash + (Flash(im) * &H100)
                    k = 1
                Else
                    'then loworder
                    k = 0
                    chksumflash = chksumflash + Flash(im)
                End If
                If chksumflash > &HFFFF Then
                    chksumflash = chksumflash - &H10000
                End If
            Next
            'chksumflash = (&H5AA5 - chksumflash) And &HFFFF

            If chksumfirmware <> chksumflash Then
                chksumdiff = True
            Else
                BlockOK(block)
            End If
        Next

        '
        ' Checksumtest is finished, wait until switch is flipped back and then close com and activate enginedata if visible
        '
        do_once = True

        timeEndPeriod(1)
        B_FlashECU.Enabled = True
        FT_status = FT_Close(lngHandle)

        If chksumdiff Then
            MsgBox("Checksum does not match with flash, please verify or flash your ecu before making changes. Turn switch to enginedata.")
            BlockPgm = True
        Else
            MsgBox("Flash checksum match, the computer and ecu memory are likely to be identical. You should be able to use the image in computer memory for changes. Turn switch to enginedata")
            BlockPgm = False
        End If
    End Sub

#End Region


    Private Sub FlashSerial_old()

        Dim path As String
        Dim flashfile As String
        Dim binfile As String
        Dim binpath As String
        Dim cb(4) As Byte
        Dim FT_status As Long
        Dim lngHandle As Long
        Dim rxbyte, txbyte As Byte
        Dim rxqueue, txqueue, eventstat As Integer
        Dim i, x, y, comportnum As Integer
        Dim ACK As Integer = &H6
        Dim NAK As Integer = &H15
        Dim block As Integer
        Dim cp As Integer
        Dim buff(&HFF) As Byte
        Dim j As Integer
        Dim k As Integer
        Dim blk5 As Boolean
        Dim startaddr As Integer
        Dim modemstat As Integer
        Dim im, chksumflash, chksum As Long
        Dim blkF As Boolean
        Dim loopuntilack As Boolean
        Dim loopcount As Integer
        Dim do_once As Boolean
        Dim starttime As Date
        Dim endtime As Date
        Dim totaltime As TimeSpan

        endtime = Date.Now
        starttime = Date.Now
        totaltime = endtime.Subtract(starttime)
        K8FlashStatus.L_elapsedtime.Text = totaltime.Minutes & ":" & totaltime.Seconds

        K8FlashStatus.Show()
        K8FlashStatus.Progressbar_Flashstatus.Maximum = &HFF
        K8FlashStatus.Progressbar_Flashstatus.Value = 1


        K8FlashStatus.fmode.ForeColor = Color.DarkGray
        K8FlashStatus.Progressbar_Flashstatus.Value = 0
        K8FlashStatus.Progressbar_Flashstatus.Refresh()
        K8FlashStatus.Refresh()
        System.Windows.Forms.Application.DoEvents()

        '
        ' Lets calculate checksum for the bin
        '
        chksum = ReadFlashWord(&HFFFF8) 'old checksum
        WriteFlashWord(&HFFFF8, 0)
        For im = 0 To &HFFFFF
            If k = 0 Then
                chksumflash = chksumflash + (Flash(im) * &H100)
                k = 1
            Else
                k = 0
                chksumflash = chksumflash + Flash(im)
            End If
            If chksumflash > &HFFFF Then
                chksumflash = chksumflash - &H10000
            End If
        Next
        chksumflash = (&H5AA5 - chksumflash) And &HFFFF
        WriteFlashWord(&HFFFF8, chksumflash) 'new checksum to written to .bin

        If K8Datastream.Visible() Then
            K8Datastream.closeenginedatacomms()
        End If
        '
        ' Get the FTDI device handle based on com port number and leave that port open
        '
        B_FlashECU.Enabled = False ' can not restart while flashing active
        timeBeginPeriod(1)
        comportnum = Val(Mid$(My.Settings.Item("ComPort"), 4))
        FT_status = FT_GetNumberOfDevices(i, 0, &H80000000)
        i = i - 1
        For x = 0 To i
            FT_status = FT_Open(x, lngHandle) ' only one
            FT_status = FT_GetComPortNumber(lngHandle, y)
            If y = comportnum Then
                cp = x
                x = i
            End If
            FT_status = FT_Close(lngHandle)
        Next
        If FT_status <> 0 Then
            MsgBox("Could not open com port, please set correct port on K8 enginedata screen. Programming aborted, reset ecu and reprogram")
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            K8FlashStatus.Close()
            Return
        End If
        '
        ' Open, Reset, set timeouts and set baud rate
        '
        FT_status = FT_Open(cp, lngHandle)
        FT_status = FT_ResetDevice(lngHandle, 3)                                'set device to default status
        FT_status = FT_status + FT_Purge(lngHandle)                             'clear rx and tx buffers
        FT_status = FT_status + FT_SetBaudRate(lngHandle, 57600)                'set speed 57600
        FT_status = FT_status + FT_SetDataCharacteristics(lngHandle, 8, 1, 0)   ' 8bits ,1 stop, parity none
        FT_status = FT_status + FT_SetTimeouts(lngHandle, 50, 50)                 'rx and tx timeouts ms
        FT_status = FT_status + FT_SetLatencyTimer(lngHandle, 8)               'ms
        FT_status = FT_status + FT_SetUSBParameters(lngHandle, 4096, 4096)      'only rx is active by FTDI
        If FT_status <> 0 Then
            MsgBox("Could not set Com port parameters. Programming aborted, set correct com port for the interface using data monitoring screen")
            B_FlashECU.Enabled = True
            K8FlashStatus.Close()
            FT_status = FT_Close(lngHandle)
            Return
        End If

        '
        ' Lets test that the interface is in the programming mode
        '
        FT_status = FT_SetDtr(lngHandle) 'new for Interface V1.1
        FT_status = FT_GetModemStatus(lngHandle, modemstat)
        If FT_status <> 0 Then
            MsgBox("Set the correct Com port for the interface using data monitoring screen")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            K8FlashStatus.Close()
            Return
        End If
        If Not ((modemstat = &H6000) Or (modemstat = &H6200)) Then
            MsgBox("Interface is not on or it is not in programming mode, set programming switch to programming mode and retry")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        Else
            '
            ' Reset ecu
            '
            FT_status = FT_SetRts(lngHandle)
            System.Threading.Thread.Sleep(300)
            FT_status = FT_ClrRts(lngHandle)
            System.Threading.Thread.Sleep(300)
        End If


        i = 0
        rxqueue = 0
        '
        ' Sync baud rate with ecu 18 x 0x00, get ack as a reply
        '
        x = 18 'default is 18
        For i = 1 To x
            txbyte = &H0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(40)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            If rxqueue <> 0 Then i = x
        Next
        System.Threading.Thread.Sleep(2)
        For x = 1 To rxqueue
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        Next
        If (rxbyte <> ACK) Then
            MsgBox("Unexpected or missing ECU response during intialization. Programming aborted, reset ecu and reprogram." & Hex(rxqueue) & " " & Hex(rxbyte))
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        End If
        '
        ' check key status and send key if necessary
        '
        rxbyte = 0
        i = 0
        rxqueue = 0
        While (rxqueue = 0) And (i < 10)
            txbyte = &H70
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(40)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            i = i + 1
        End While
        For x = 1 To rxqueue
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        Next
        If rxbyte <> &H8C Then
            txbyte = &HF5
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H84
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HC
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H53
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H55
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H45
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H46
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H49
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H4D
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H56
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &H30
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            '
            ' Receive ACK if unlock code succesfull
            '
            System.Threading.Thread.Sleep(100)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            For i = 1 To rxqueue
                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
            Next
            If rxbyte <> ACK Then
                MsgBox("No ACK received after sending unlock code. Programming aborted, reset ecu and reprogram")
                K8FlashStatus.Close()
                B_FlashECU.Enabled = True
                FT_status = FT_Close(lngHandle)
                Return
            End If
        End If
        '
        ' Check status after unlock code
        '
        txqueue = 0
        i = 0
        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        System.Threading.Thread.Sleep(50)
        While rxqueue = 0 And i < 10
            txbyte = &H70
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(40)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            i = i + 1
        End While
        If (i >= 10) Or (rxqueue = 0) Then
            MsgBox("Error in validating the unlock code from ECU. Programming aborted, reset ecu and reprogram")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        Else
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1) '128
            System.Threading.Thread.Sleep(50)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1) '140
            System.Threading.Thread.Sleep(50)
            FT_status = FT_status + FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        End If
        If (rxbyte <> &H8C) Or (FT_status <> 0) Then
            MsgBox("Was not able to set the ecu key. Programming aborted, reset ecu and reprogram")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        End If
        '
        ' Clear status register just in case
        '
        txbyte = &H50
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        System.Threading.Thread.Sleep(50)
        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        For i = 1 To rxqueue
            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        Next
        If rxbyte <> ACK Then
            MsgBox("Status query error 1. Programming aborted, reset ecu and reprogram")
            K8FlashStatus.Close()
            B_FlashECU.Enabled = True
            FT_status = FT_Close(lngHandle)
            Return
        End If

        '
        ' New command added, to be tested
        '
        'timeBeginPeriod(0)

        '
        ' Lets verify that this really is suzuki hayabusa ecu, flashing any other ecu type may damage the ecu and the bike
        '
        System.Threading.Thread.Sleep(100)
        txbyte = &HFF
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        txbyte = &HFF
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        txbyte = &HF
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        System.Threading.Thread.Sleep(100)
        k = 0
        Dim s As String
        s = ""
        For j = 0 To &HFF
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            For i = 1 To rxqueue
                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                '
                ' Lets test that the ecu id matches close enought to Hayabusa ecu
                ' if rxbyte = &HFF then its likely that the ecu has been fully erased and can be reflashed
                '
                If (k >= &HF0) And (k <= &HF5) Then
                    If (rxbyte <> ReadFlashByte(&HFFF00 + k)) And (rxbyte <> &HFF) Then
                        If MsgBox("Not same ECU ID in memory and inside the ecu. Possibly ecu is not from a Hayabusa. You can stop the flashing by pressing cancel.", MsgBoxStyle.OkCancel) = MsgBoxResult.Cancel Then
                            K8FlashStatus.Close()
                            B_FlashECU.Enabled = True
                            FT_status = FT_Close(lngHandle)
                        End If
                    End If
                End If
                If (k >= &HF6) And (k <= &HF7) Then
                    If (rxbyte <> &H30) And (rxbyte <> &H31) And (rxbyte <> &H32) And (rxbyte <> &H35) And (rxbyte <> &HFF) Then
                        If MsgBox("Not a Hayabusa 15H00, 15H10, 15H20 or 15Hxx Generic ecu. Programming stopped to avoid damage to ecu or bike. Press cancel to stop, ok to continue", MsgBoxStyle.OkCancel) = MsgBoxResult.Cancel Then
                            K8FlashStatus.Close()
                            B_FlashECU.Enabled = True
                            FT_status = FT_Close(lngHandle)
                        End If
                    End If
                End If
                k = k + 1
            Next
        Next
        '
        ' Lets read what is the flashingmode in ecu if memory is set to fast flashmode
        ' if fastflash then...
        '
        If ReadFlashLongWord(&H51F10) = &H536C4 Then
            System.Threading.Thread.Sleep(100)
            txbyte = &HFF
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            txbyte = &H1F
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            txbyte = &H5
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            System.Threading.Thread.Sleep(100)
            k = 0
            For j = 0 To &HFF
                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                For i = 1 To rxqueue
                    FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                    If k = 19 Then
                        i = ReadFlashByte(&H51F10 + 3)
                        Select Case rxbyte
                            Case &H18 ' stock setting not fastmode, reflash block 5 is memory in fast mode
                                If ReadFlashLongWord(&H51F10) = &H536C4 Then
                                    blk5 = True
                                Else
                                    blk5 = False
                                End If
                            Case &HC4 ' ecu already in fast mode, no reflashing is needed
                                blk5 = False
                            Case &HFF ' block5 is empty, may be reflashing error. reflash block 5
                                blk5 = True
                            Case Else
                                MsgBox("Error in reading flashingmode from ECU, programming aborted. Please reboot ecu and reflash")
                                BlockPgm = True
                                K8FlashStatus.Close()
                                B_FlashECU.Enabled = True
                                FT_status = FT_Close(lngHandle)
                        End Select
                    End If
                    k = k + 1
                Next
            Next
        End If
        timeBeginPeriod(1)
        System.Threading.Thread.Sleep(300)

        '
        ' For a reason or another block 0 requires full erase
        '
        If BlockChanged(0) = True Then
            BlockPgm = True
        End If

        blkF = False ' this is just used for check sum testing

        '
        ' Here is an erase for the full ecu
        '
        If BlockPgm Then
            endtime = Date.Now
            totaltime = endtime.Subtract(starttime)
            K8FlashStatus.L_elapsedtime.Text = totaltime.Minutes & ":" & totaltime.Seconds
            K8FlashStatus.fmode.Text = "Performing full erase, please wait"
            K8FlashStatus.fmode.ForeColor = Color.Gray
            K8FlashStatus.Progressbar_Flashstatus.Value = 0
            K8FlashStatus.Progressbar_Flashstatus.Refresh()
            K8FlashStatus.Refresh()
            System.Windows.Forms.Application.DoEvents()
            '
            ' Send Erase full memory command
            '
            txbyte = &HA7
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            txbyte = &HD0
            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            '
            ' ECU confirms a succesfull erase by sending ACK
            '
            loopcount = 0
            loopuntilack = False
            While Not loopuntilack
                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                While (rxqueue = 0) And (i < 100)
                    System.Threading.Thread.Sleep(50)
                    FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                    i = i + 1
                End While
                For i = 1 To rxqueue
                    FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                    If rxbyte = ACK Then
                        loopuntilack = True
                    End If
                Next
                If loopcount > 10 Then
                    '
                    ' Clear program lock bit and status register
                    '
                    txbyte = &H75
                    FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                    FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                    FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                    txbyte = &H50
                    FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                    FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                    FT_Read_Bytes(lngHandle, rxbyte, 1, 1)

                End If
                If loopcount > 20 Then
                    MsgBox("No ACK after full erase, Programming aborted, reset ecu and reprogram.")
                    K8FlashStatus.Close()
                    B_FlashECU.Enabled = True
                    FT_status = FT_Close(lngHandle)
                    BlockPgm = True
                    Return
                End If
                loopcount = loopcount + 1
            End While

        End If


        '
        ' Now programmings starts
        '
        j = ReadFlashLongWord(&H51F10)
        If ReadFlashLongWord(&H51F10) <> &H536C4 Then
            K8FlashStatus.fmode.Text = "Normal flash "
        Else
            K8FlashStatus.fmode.Text = "Fast flash "
        End If
        K8FlashStatus.fmode.ForeColor = Color.Black
        K8FlashStatus.Progressbar_Flashstatus.Value = 0
        K8FlashStatus.Progressbar_Flashstatus.Refresh()
        K8FlashStatus.Refresh()
        System.Windows.Forms.Application.DoEvents()

        startaddr = 0
        For block = startaddr To &HF
            ' BlockChanged returns true if there has been any changes to that block
            ' BlockPgm is a global variable that forces all blocks to be written
            If BlockChanged(block) Or BlockPgm Then

                endtime = Date.Now
                totaltime = endtime.Subtract(starttime)
                K8FlashStatus.L_elapsedtime.Text = totaltime.Minutes & ":" & totaltime.Seconds
                K8FlashStatus.fmode.Text = K8FlashStatus.fmode.Text & Hex(block)
                K8FlashStatus.Progressbar_Flashstatus.Refresh()
                K8FlashStatus.Refresh()
                System.Windows.Forms.Application.DoEvents()

                If block = &HF Then blkF = True


                '
                ' Erase block
                '
                txbyte = &H20
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                txbyte = &H0
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                txbyte = block
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                txbyte = &HD0
                FT_Write_Bytes(lngHandle, txbyte, 1, 1)

                '
                ' ECU confirms a succesfull erase by sending ACK
                '

                loopcount = 0
                loopuntilack = False

                While Not loopuntilack
                    FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                    i = 0
                    While (rxqueue = 0) And (i < 100)
                        System.Threading.Thread.Sleep(50)
                        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                        i = i + 1
                    End While
                    For i = 1 To rxqueue
                        FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                        If rxbyte = ACK Then loopuntilack = True
                    Next
                    If rxbyte = NAK Then
                        '
                        ' Lets inform the user that something is wrong
                        '
                        K8FlashStatus.fmode.ForeColor = Color.Orange
                        '
                        ' Clear status register
                        '
                        System.Threading.Thread.Sleep(200)
                        txbyte = &H50
                        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                        i = 0
                        While (rxqueue = 0) And (i < 10)
                            System.Threading.Thread.Sleep(50)
                            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                            i = i + 1
                        End While
                        For i = 1 To rxqueue
                            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                        Next
                        '
                        ' Disable lock bit
                        '
                        System.Threading.Thread.Sleep(200)
                        txbyte = &H75
                        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                        i = 0
                        While (rxqueue = 0) And (i < 10)
                            System.Threading.Thread.Sleep(50)
                            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                            i = i + 1
                        End While
                        For i = 1 To rxqueue
                            FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                        Next
                        '
                        ' Erase block again
                        '
                        System.Threading.Thread.Sleep(200)
                        txbyte = &H20
                        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                        txbyte = &H0
                        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                        txbyte = block
                        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                        txbyte = &HD0
                        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                        System.Threading.Thread.Sleep(200)
                    End If
                    If loopcount > 100 Then
                        MsgBox("No ACK after erasing a block=" & Str(block) & " Programming aborted, reset ecu and reprogram")
                        K8FlashStatus.Close()
                        B_FlashECU.Enabled = True
                        FT_status = FT_Close(lngHandle)
                        BlockPgm = True
                        Return
                    End If
                    endtime = Date.Now
                    totaltime = endtime.Subtract(starttime)
                    K8FlashStatus.L_elapsedtime.Text = totaltime.Minutes & ":" & totaltime.Seconds
                    K8FlashStatus.L_elapsedtime.Text = TimeOfDay
                    K8FlashStatus.Progressbar_Flashstatus.Value = loopcount
                    K8FlashStatus.Progressbar_Flashstatus.Refresh()
                    K8FlashStatus.Refresh()
                    System.Windows.Forms.Application.DoEvents()

                    loopcount = loopcount + 1
                End While

                rxqueue = 0
                i = 0

                '
                ' Write block using page write
                '
                K8FlashStatus.fmode.ForeColor = Color.Black

                Dim page As Integer
                For page = 0 To &HFF
                    '
                    ' Write one page at time
                    '
                    endtime = Date.Now
                    totaltime = endtime.Subtract(starttime)
                    K8FlashStatus.L_elapsedtime.Text = totaltime.Minutes & ":" & totaltime.Seconds
                    K8FlashStatus.Progressbar_Flashstatus.Value = page
                    K8FlashStatus.Progressbar_Flashstatus.Refresh()
                    K8FlashStatus.Refresh()
                    System.Windows.Forms.Application.DoEvents()
                    '
                    ' Check if the page is filled with 0xFF, no need to program
                    '
                    i = 0
                    For y = 0 To &HFF
                        buff(y) = ReadFlashByte((block * &H10000) + (page * &H100) + y)
                        If buff(y) <> &HFF Then
                            i = i + 1
                        End If
                    Next
                    If i > 0 Then ' there is something in the page that is not 0xFF


                        loopcount = 0
                        loopuntilack = False

                        While Not loopuntilack
                            '
                            ' write a page
                            '
                            txbyte = &H41
                            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                            txbyte = page
                            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                            txbyte = block
                            FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                            For y = 0 To &HFF
                                txbyte = buff(y)
                                'FT_status = FT_Write_Bytes(lngHandle, txbyte, 1, txqueue)
                            Next

                            FT_status = FT_Write(lngHandle, buff, &HFF + 1, &HFF + 1)

                            '
                            ' this should be ack from page write
                            '
                            rxbyte = 0
                            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                            i = 0
                            While (rxqueue = 0) And (i < 30)
                                System.Threading.Thread.Sleep(25)
                                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                                If rxqueue > 0 Then i = 30
                                i = i + 1
                            End While
                            For i = 1 To rxqueue
                                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                                If rxbyte = ACK Then loopuntilack = True
                            Next
                            If loopcount > 5 Then
                                '
                                ' Clear program lock bit and status register
                                '
                                K8FlashStatus.fmode.ForeColor = Color.Orange
                                K8FlashStatus.Refresh()
                                System.Windows.Forms.Application.DoEvents()
                                System.Threading.Thread.Sleep(100)

                                txbyte = &H75
                                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                                System.Threading.Thread.Sleep(100)
                                rxbyte = 0
                                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                                i = 0
                                While (rxqueue = 0) And (i < 30)
                                    System.Threading.Thread.Sleep(25)
                                    FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                                    If rxqueue > 0 Then i = 30
                                    i = i + 1
                                End While
                                For i = 1 To rxqueue
                                    FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                                Next

                                txbyte = &H50
                                FT_Write_Bytes(lngHandle, txbyte, 1, 1)
                                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                                FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                                System.Threading.Thread.Sleep(100)
                                rxbyte = 0
                                FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                                i = 0
                                While (rxqueue = 0) And (i < 30)
                                    System.Threading.Thread.Sleep(25)
                                    FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
                                    If rxqueue > 0 Then i = 30
                                    i = i + 1
                                End While
                                For i = 1 To rxqueue
                                    FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
                                Next

                            End If
                            If loopcount > 10 Then

                                MsgBox("No ACK after writing a block=" & Str(block) & " page=" & Str(page) & ". Programming aborted, reset ecu and reprogram")
                                K8FlashStatus.Close()
                                B_FlashECU.Enabled = True
                                FT_status = FT_Close(lngHandle)
                                BlockPgm = True
                                Return
                            End If
                            loopcount = loopcount + 1
                        End While

                    End If
                Next
            End If
        Next


        '
        ' Acquire sum value and compare checksum. This will be done every time as flashing always
        ' starts by writing a new checksum to the image in computer memory
        '
        txbyte = &HE1
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        txbyte = 0
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        txbyte = 0
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        txbyte = &HFF
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        txbyte = &HF
        FT_Write_Bytes(lngHandle, txbyte, 1, 1)
        System.Threading.Thread.Sleep(200)
        FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
        If rxqueue <> 2 Then MsgBox("Error in reading checksum from ecu")
        FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        k = rxbyte
        FT_Read_Bytes(lngHandle, rxbyte, 1, 1)
        k = (k) + (rxbyte * &H100)


        '
        ' Flashing is finished, wait until switch is flipped back and then close com and activate enginedata if visible
        '
        do_once = True

        timeEndPeriod(1)

        If k <> &H5AA5 Then
            MsgBox("Checksum error when validating the flash, please reflash your ecu before using it.")
            K8FlashStatus.fmode.Text = "Checksum error, please reprogram"
            ResetBlocks()
            BlockPgm = True
        Else
            K8FlashStatus.fmode.Text = "Flash OK, turn switch to enginedata"
            ResetBlocks()
            BlockPgm = False
        End If


        endtime = Date.Now
        totaltime = endtime.Subtract(starttime)
        K8FlashStatus.L_elapsedtime.Text = totaltime.Minutes & ":" & totaltime.Seconds
        K8FlashStatus.Refresh()
        System.Windows.Forms.Application.DoEvents()

        FT_status = FT_GetModemStatus(lngHandle, modemstat)
        While ((modemstat = &H6000) Or (modemstat = &H6200))
            FT_status = FT_GetStatus(lngHandle, rxqueue, txqueue, eventstat)
            System.Threading.Thread.Sleep(200)
            FT_status = FT_GetModemStatus(lngHandle, modemstat)
        End While
        K8FlashStatus.Close()
        B_FlashECU.Enabled = True

        FT_status = FT_ClrDtr(lngHandle) 'new for Interface V1.1

        FT_status = FT_Close(lngHandle)
        If (FT_status = 0) Then
            If K8Datastream.Visible() Then
                K8Datastream.startenginedatacomms()
            End If
        Else
            MsgBox("Can not close com port, please save the bin and reboot your computer and reflash just in case.")
        End If

        '
        ' generate the bin that was flashed as a backup file
        '
        flashfile = "ecuflash.bin"
        binpath = Application.StartupPath
        binfile = "ecuflash.bin"
        path = binpath & flashfile
        ' if the temporary file exists for any reason, delete it
        If File.Exists(path) = True Then
            File.Delete(path)
        End If
        ' create the temporary file from current memory
        fs = File.Open(path, FileMode.CreateNew)
        fs.Write(Flash, 0, (262144 * 4))
        fs.Close()
        '
        ' Generate a backup copy of the flashed file just flashed
        '
        binfile = ECUID.Text & "-"
        binfile = binfile & My.Computer.Clock.LocalTime.Date.Day & "-"
        binfile = binfile & My.Computer.Clock.LocalTime.Date.Month & "-"
        binfile = binfile & My.Computer.Clock.LocalTime.Date.Year & "-"
        binfile = binfile & My.Computer.Clock.LocalTime.Hour & "-"
        binfile = binfile & My.Computer.Clock.LocalTime.Minute
        binfile = binfile & ".bin"
        path = binpath & binfile
        ' if the temporary file exists for any reason, delete it
        If File.Exists(path) = True Then
            File.Delete(path)
        End If
        ' create the temporary file from curren memory
        fs = File.Open(path, FileMode.CreateNew)
        fs.Write(Flash, 0, (262144 * 4))
        fs.Close()
        ' cleanup of old backup files
        CleanUpFDTDirectory(binpath)


    End Sub

    Private Sub RecoveryToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RecoveryToolStripMenuItem.Click
        ' Lets use OpenFileDialog to open a new flash image file
        Dim fdlg As OpenFileDialog = New OpenFileDialog()
        Dim fs As FileStream
        fdlg.InitialDirectory = My.Application.Info.DirectoryPath 'My.Application.Info.DirectoryPath
        fdlg.Title = "Open ECU .bin file from recovery location"
        fdlg.Filter = "ECU definitions (*.bin)|*.bin"
        fdlg.FilterIndex = 1
        fdlg.RestoreDirectory = True
        fdlg.FileName = My.Application.Info.DirectoryPath
        CloseChildWindows()

        If fdlg.ShowDialog() = Windows.Forms.DialogResult.OK Then

            ' OK, so the file is found, now lets start processing it
            path = fdlg.FileName
            If path.Length > 15 Then
                L_Comparefile.Text = "..." & path.Substring(path.Length - 15)
                L_File.Text = L_Comparefile.Text
            Else
                L_Comparefile.Text = path
                L_File.Text = L_Comparefile.Text
            End If

            ' Open the stream and read it to global variable "Flash". 
            fs = File.OpenRead(path)
            Dim b(1) As Byte
            Dim i As Integer
            i = 0
            Do While fs.Read(b, 0, 1) > 0
                Flash(i) = b(0)
                FlashCopy(i) = b(0)
                i = i + 1
            Loop
            fs.Close()

            ' Check that the binary lenght matches expected ecu
            ' and initialize variables and stuff as needed 
            '
            ' Remove v1.5 protection if exists
            '
            If Flash(&H2) = 0 Then
                Flash(&H2) = 4
                MsgBox("ECUeditor v1.5 protection detected and removed")
            End If

            Select Case i
                Case (262144 * 4)
                    ECUVersion = "gen2"
                    '
                    ' Make sure the ECU id is supported type
                    '
                    i = 0
                    ECUID.Text = ""
                    Do While i < 8
                        ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
                        i = i + 1
                    Loop

                    ' check the ecu id bytes and validate that the ecu flash image is supported
                    If (Mid(ECUID.Text, 1, 6) <> "DJ18SE") And (Mid(ECUID.Text, 1, 6) <> "DJ47SE") Then
                        ECUNotSupported.ShowDialog()
                    Else
                        SetECUType()
                    End If
                    BlockPgm = True
                    CloseChildWindows()
                Case (262144)
                    ECUVersion = "gen1"
                    FlashToolStripMenuItem.Visible = False

                    ' Make sure the ECU id is supported type
                    i = 0
                    ECUID.Text = ""
                    Do While i < 8
                        ECUID.Text = ECUID.Text & Chr(Flash(&H3FFF0 + i))
                        i = i + 1
                    Loop

                    ' check the ecu id bytes and validate that the ecu flash image is supported
                    If Mid(ECUID.Text, 1, 6) <> "BB34BB" Then
                        ECUNotSupported.ShowDialog()
                    Else
                        Hayabusa.Visible = True
                        Select Case Mid(ECUID.Text, 1, 8)
                            Case "BB34BB51"
                                Hayabusa.Text = "Hayabusa EU"
                                Metric = True
                                ECUVersion = "gen1"
                            Case "BB34BB35"
                                Hayabusa.Text = "Hayabusa USA"
                                Metric = False
                                ECUVersion = "gen1"
                            Case Else
                                Hayabusa.Text = "Unknown model"
                                Metric = True
                                ECUVersion = ""
                        End Select
                    End If

                Case Else
                    ECUVersion = ""
                    ECUNotSupported.ShowDialog()
            End Select

            My.Settings.Item("path") = path
            My.Settings.Item("comparepath") = comparepath

            MsgBox("Now remember to save the file with a different name to your default ecueditor.com files directory")

        End If

    End Sub

    Private Sub B_DataLogging_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles B_DataLogging.Click

        K8EngineDataLogger.Show()

    End Sub

    Private Sub GixxerK5K6EnginedataOnlyToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GixxerK5K6EnginedataOnlyToolStripMenuItem.Click
        CloseChildWindows()
        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ECUID.Text = "US________"
        SetECUType()

        ' enable controls, otherwise at form load an event will occur
        B_EngineData.Enabled = True

    End Sub

    Private Sub NewStockGixxerK7ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
    End Sub

    Private Sub EcueditorcomHomepageToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EcueditorcomHomepageToolStripMenuItem.Click
        'ProgramHomepage.Show()
        'ProgramHomepage.Select()
        Process.Start("http://www.ecueditor.com")
    End Sub

    Private Sub DonateForKeepingTheProgramFreeToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DonateForKeepingTheProgramFreeToolStripMenuItem.Click
        Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=ZSM36H9HWNU2C")
    End Sub

    Private Sub ViewLatestDevelopmentFromDeveloperSiteToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ViewLatestDevelopmentFromDeveloperSiteToolStripMenuItem.Click
        Process.Start("https://bitbucket.org/ecueditor/ecueditor/changesets")
    End Sub

    Private Sub BuyInterfaceFromEBayToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BuyInterfaceFromEBayToolStripMenuItem.Click
        'Process.Start("http://product-search.ebay.com/ecueditor.com") 'this seems to return only from american site
        Process.Start("http://shop.ebay.com/i.html?_nkw=ecueditor.com&_sacat=0&_sop=12&LH_PrefLoc=2&_dmd=1&_odkw=ecueditor.com&_rdc=1") 'this is worldwide search string
    End Sub

    Private Sub NewStockGixxerK7EUToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\GixxerK7EU.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        If (Mid(ECUID.Text, 1, 4) <> "DJ0H") And (Mid(ECUID.Text, 1, 4) <> "DT0H") And (Mid(ECUID.Text, 1, 4) <> "DJ21") Then
            ECUNotSupported.ShowDialog()
        Else
            SetECUType()
        End If

        MsgBox("A new Gixxer K8- basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True

    End Sub

    Private Sub NewStockGixxerK721H00ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NewStockGixxerK721H00ToolStripMenuItem.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\Gixxer21H00.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        SetECUType()

        MsgBox("A new Gixxer K7- basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True


    End Sub

    Private Sub NewStockGixxerK721H50ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NewStockGixxerK721H50ToolStripMenuItem.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\GixxerK7EU.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        If (Mid(ECUID.Text, 1, 4) <> "DJ0H") And (Mid(ECUID.Text, 1, 4) <> "DT0H") And (Mid(ECUID.Text, 1, 4) <> "DJ21") Then
            ECUNotSupported.ShowDialog()
        Else
            SetECUType()
        End If

        MsgBox("A new Gixxer K7- basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True


    End Sub

    Private Sub NewStockGixxerK721H60ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NewStockGixxerK721H60ToolStripMenuItem.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\GixxerK7.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        If (Mid(ECUID.Text, 1, 4) <> "DJ0H") And (Mid(ECUID.Text, 1, 4) <> "DT0H") And (Mid(ECUID.Text, 1, 4) <> "DJ21") Then
            ECUNotSupported.ShowDialog()
        Else
            SetECUType()
        End If

        MsgBox("A new Gixxer K7- basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True


    End Sub

    Private Sub NewStockGixxerK821H50ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NewStockGixxerK821H50ToolStripMenuItem.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\GixxerK8.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        SetECUType()

        MsgBox("A new Gixxer K8- basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True


    End Sub

    Private Sub NewStockGixxerK9K1147H10ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NewStockGixxerK9K1147H10ToolStripMenuItem.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\Gixxerk9.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        SetECUType()

        MsgBox("A new Gixxer K9 basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True


    End Sub

    Private Sub NewStockGixxerK9K1147H10ToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NewStockGixxerK9K1147H10ToolStripMenuItem1.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\Gixxerk9US.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        SetECUType()

        MsgBox("A new Gixxer K9 basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True
    End Sub

    Private Sub USModelToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles USModelToolStripMenuItem.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\BkingUS.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "bking"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        If Mid(ECUID.Text, 1, 6) <> "DJ47SE" Then
            ECUNotSupported.ShowDialog()
        Else
            SetECUType()
        End If

        MsgBox("A new Bking basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True

    End Sub

    Private Sub EUAUModelToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EUAUModelToolStripMenuItem.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\bking.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "bking"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        If Mid(ECUID.Text, 1, 6) <> "DJ47SE" Then
            ECUNotSupported.ShowDialog()
        Else
            SetECUType()
        End If

        MsgBox("A new Bking basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True

    End Sub

    Private Sub K8EUUSAU15HToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles K8EUUSAU15HToolStripMenuItem.Click
        Dim defpath As String ' this is for this subroutine only
        CloseChildWindows()

        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\k8.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gen2"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        If Mid(ECUID.Text, 1, 6) <> "DJ18SE" Then
            ECUNotSupported.ShowDialog()
        Else
            SetECUType()
        End If

        MsgBox("A new gen2 basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True

    End Sub

    Private Sub K2K724FG0ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles K2K724FG0ToolStripMenuItem.Click
        'JaSa 01.July.2010
        'G1 EU base map selected --> reading of bin will hapen on G1ReadMap procedure

        Dim defpath As String ' this is for this subroutine only

        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\G1BusaEUdefault.bin"
        G1ReadMap(defpath)

    End Sub

    Private Sub K2K724FK0ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles K2K724FK0ToolStripMenuItem.Click
        'JaSa 01.July.2010
        'G1 US base map selected --> reading of bin will hapen on G1ReadMap procedure

        Dim defpath As String ' this is for this subroutine only
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\G1BusaUSdefault.bin"
        G1ReadMap(defpath)

    End Sub

    Private Sub ToolStripMenuItem2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItem2.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\Gixxer21H10.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        SetECUType()

        MsgBox("A new Gixxer K7- basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True



    End Sub

    Private Sub ToolStripMenuItem5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItem5.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\DJ0HSE70.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        SetECUType()

        MsgBox("A new Gixxer K7- basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True



    End Sub

    Private Sub K8EUAU21H50EarlyToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles K8EUAU21H50EarlyToolStripMenuItem.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\21H50_DJ0HSE01.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        If (Mid(ECUID.Text, 1, 4) <> "DJ0H") And (Mid(ECUID.Text, 1, 4) <> "DT0H") And (Mid(ECUID.Text, 1, 4) <> "DJ21") Then
            ECUNotSupported.ShowDialog()
        Else
            SetECUType()
        End If

        MsgBox("A new Gixxer K8- basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True



    End Sub

    Private Sub K9EU38H00ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles K9EU38H00ToolStripMenuItem.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\G750K9.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        SetECUType()

        MsgBox("A new Gixxer 750K9- basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True

    End Sub

    Private Sub ReportABugToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ReportABugToolStripMenuItem.Click
        Process.Start("https://bitbucket.org/ecueditor/ecueditor/issues?status=new")
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Process.Start("http://www.facebook.com/pages/ecueditorcom/133860313347043#!/pages/ecueditorcom/133860313347043")
    End Sub

    Private Sub EcueditorcomfacebookToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EcueditorcomfacebookToolStripMenuItem.Click
        Process.Start("http://www.facebook.com/pages/ecueditorcom/133860313347043#!/pages/ecueditorcom/133860313347043")
    End Sub

    Private Sub EnginedataOnlyModeMetricToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EnginedataOnlyModeMetricToolStripMenuItem.Click
        CloseChildWindows()
        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ECUID.Text = "EU________"
        SetECUType()

        ' enable controls, otherwise at form load an event will occur
        B_EngineData.Enabled = True
    End Sub

    Private Sub K9GR38HH0ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles K9GR38HH0ToolStripMenuItem.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\G750K9G.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        SetECUType()

        MsgBox("A new Gixxer 750K8 basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True

    End Sub

    Private Sub MakeAnDIYInterfaceToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MakeAnDIYInterfaceToolStripMenuItem.Click
        Process.Start("http://macmadigan.no-ip.com/ecueditor/interface.html")
    End Sub

    Private Sub ToolStripMenuItem4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItem4.Click

    End Sub

    Private Sub K8EU37H00ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles K8EU37H00ToolStripMenuItem.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\37H00_DJ55SE00.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        SetECUType()

        MsgBox("A new Gixxer 600K8- basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True


    End Sub


    Public Sub generate_tweets()
        Dim objImage As MemoryStream
        Dim objwebClient As WebClient


        If tweets = False Then
            R_tw.Text = ""
        End If

        On Error Resume Next ' in case the pc is not connected to internet

        My.Settings.m_strConsumerKey = "fDEUnOOMBxtj5RjPuehV2g"
        My.Settings.m_strConsumerSecret = "CUtSovat53Hz9Cp7uzhP5yJTqnCcSjnV4PDROEBMk"
        My.Settings.Save()
        objwebClient = New WebClient()

        tw.AuthenticateWith(My.Settings.m_strConsumerKey, My.Settings.m_strConsumerSecret, "251085356-DYkUUNAZScLYtXXSBi5UyKgbJM7eKbfySFggeBuw", "6PiFsw8K1nDbP8zu9outK7I3SBelkE8gEHHKxSfwA")
        Dim tp As New TwitterParameters
        tp.Add(TwitterParameterNames.Count, 1)
        For Each tweet As TwitterStatus In tw.HomeTimeline(tp)
            R_tw.Text = (tweet.User.ScreenName & " - " & tweet.Text)
            objImage = New MemoryStream(objwebClient.DownloadData(tweet.User.ProfileImageUrl))
            tw_p.Image = Image.FromStream(objImage)
        Next


        'Dim tweet As TwitterStatus = tw.AccountInformation.Status()
        'R_tw.Text = tweet.User.ScreenName & " : " & tweet.Text
        'objImage = New MemoryStream(objwebClient.DownloadData(tweet.User.ProfileImageUrl))
        'tw_p.Image = Image.FromStream(objImage)
        'Dim tsp As New TwitterSearchParameters
        'tsp.Add(TwitterSearchParameterNames.SearchTerm, "ecueditor")
        'Dim ts As TwitterStatus = tw.UserTimeline.FindLast(tsp)

        'R_tw.Text = tw.AccountInformation.ScreenName & " - " & tw.AccountInformation.Status.Text
        'For Each tweet As TwitterStatus In tw.HomeTimeline()
        ' If tweet.User.ScreenName = tw.AccountInformation.ScreenName Then
        ' objImage = New MemoryStream(objwebClient.DownloadData(tweet.User.ProfileImageUrl))
        ' tw_p.Image = Image.FromStream(objImage)
        ' latest = tweet.CreatedAt()
        ' latest_id = tweet.ID
        ' End If
        'Next

        R_tw.Refresh()
        tweets = True
    End Sub

    Private Sub R_tw_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles R_tw.DoubleClick
        Process.Start("https://twitter.com/#!/ecueditor")
    End Sub


    Function GetLinkURL() As String
        Return tw.GetAuthorizationLink(My.Settings.m_strConsumerKey, My.Settings.m_strConsumerSecret)
    End Function

    Public Function isValidPIN(ByVal p_strPIN) As Boolean
        Return tw.ValidatePIN(p_strPIN)
    End Function

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        If tweets = True Then
            If Not K8FlashStatus.Visible Then
                If Not K8Datastream.Visible Then
                    If Not K8EngineDataLogger.Visible Then
                        generate_tweets()
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub ViewFAQFacebookToolStripMenuItem_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ViewFAQFacebookToolStripMenuItem.Click
        Process.Start("http://www.WoolichRacing.com/ecueditor_help")
    End Sub

    Private Sub K8US37H10ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles K8US37H10ToolStripMenuItem.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\DJ55SE10.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        SetECUType()

        MsgBox("A new Gixxer basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True

    End Sub

    Private Sub K11EUToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles K11EUToolStripMenuItem.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\DJF4SE00.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        SetECUType()

        MsgBox("A new Gixxer basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True


    End Sub

    Private Sub K11EUToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles K11EUToolStripMenuItem1.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\DJE4SE01.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        SetECUType()

        MsgBox("A new Gixxer basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True

    End Sub

    Private Sub H10ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles H10ToolStripMenuItem.Click
        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()


        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\DJ47SE10.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "bking"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        If Mid(ECUID.Text, 1, 6) <> "DJ47SE" Then
            ECUNotSupported.ShowDialog()
        Else
            SetECUType()
        End If

        MsgBox("A new Bking basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True


    End Sub

    Private Sub GetInterfaceFromWoolichRacingcomToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles GetInterfaceFromWoolichRacingcomToolStripMenuItem.Click
        Process.Start("http://www.WoolichRacing.com/Products.aspx")
    End Sub

    Private Sub WoolichRacingLogBoxConfigToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles WoolichRacingLogBoxConfigToolStripMenuItem.Click

        LogBoxCfg.Show()

    End Sub

    Private Sub ConvertwrlFileToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles ConvertwrlFileToolStripMenuItem.Click

        Cursor = Cursors.WaitCursor
        Application.DoEvents()

        Dim writer As StreamWriter
        Dim reader As BinaryReader
        Dim completed As Boolean = False

        Try

            OpenFileDialog1.FileName = ""
            OpenFileDialog1.Filter = "*.wrl|*.wrl"

            OpenFileDialog1.Multiselect = True

            If (OpenFileDialog1.ShowDialog() = DialogResult.OK) Then
                For Each filename As String In OpenFileDialog1.FileNames
                    Dim convertedFilename As String = filename.ToLower().Replace(".wrl", ".csv")
                    Dim filePointer As Integer = 0
                    Dim buffer(69) As Byte
                    writer = New StreamWriter(convertedFilename)
                    reader = New BinaryReader(File.OpenRead(filename))

                    Dim header As StringBuilder = New StringBuilder()

                    header.Append("Log Time")
                    header.Append(",")
                    header.Append("RPM")
                    header.Append(",")
                    header.Append("TPS")
                    header.Append(",")
                    header.Append("IAP")
                    header.Append(",")
                    header.Append("HO2")
                    header.Append(",")
                    header.Append("HO2 Wideband")
                    header.Append(",")
                    header.Append("IGN")
                    header.Append(",")
                    header.Append("STP")
                    header.Append(",")
                    header.Append("GEAR")
                    header.Append(",")
                    header.Append("CLUTCH")
                    header.Append(",")
                    header.Append("NT")
                    header.Append(",")
                    header.Append("BOOST")
                    header.Append(",")
                    header.Append("CLT")
                    header.Append(",")
                    header.Append("IAT")
                    header.Append(",")
                    header.Append("PAIR")
                    header.Append(",")
                    header.Append("FUEL1")
                    header.Append(",")
                    header.Append("FUEL2")
                    header.Append(",")
                    header.Append("FUEL3")
                    header.Append(",")
                    header.Append("FUEL4")

                    writer.WriteLine(header)

                    If reader.Read(buffer, filePointer, 6) = 6 Then
                        While reader.Read(buffer, filePointer, 70) = 70

                            Try

                                Dim stringBuilder As New StringBuilder()
                                stringBuilder.Append(buffer(64).ToString("00"))
                                stringBuilder.Append(":")
                                stringBuilder.Append(buffer(65).ToString("00"))
                                stringBuilder.Append(":")
                                stringBuilder.Append(buffer(66).ToString("00"))
                                stringBuilder.Append(".")
                                stringBuilder.Append((buffer(67) * 100 + buffer(68) * 10 + buffer(69)).ToString("000"))
                                stringBuilder.Append(",")

                                Dim rpm As Integer = CInt((((&HFF * buffer(24)) + buffer(25S)) / 2.55) / 10) * 10
                                stringBuilder.Append(rpm)
                                stringBuilder.Append(",")

                                stringBuilder.Append(K8EngineDataLogger.CalcTPS(buffer(26)))
                                stringBuilder.Append(",")

                                Dim iap As Integer = buffer(30)
                                iap = iap - buffer(27)
                                stringBuilder.Append(K8EngineDataLogger.CalcPressure(iap).ToString("0.0"))
                                stringBuilder.Append(",")

                                stringBuilder.Append(K8EngineDataLogger.CalcAFR(buffer(32)).ToString("0.0"))
                                stringBuilder.Append(",")

                                Dim wideband As Integer = buffer(20) * 256 + buffer(21)
                                stringBuilder.Append(K8EngineDataLogger.CalcWidebandAFR(wideband).ToString("0.0"))
                                stringBuilder.Append(",")

                                stringBuilder.Append(K8EngineDataLogger.CalcIgnDeg(buffer(49)))
                                stringBuilder.Append(",")

                                stringBuilder.Append(K8EngineDataLogger.CalcSTP(buffer(53)))
                                stringBuilder.Append(",")

                                stringBuilder.Append(buffer(33))
                                stringBuilder.Append(",")

                                stringBuilder.Append(K8EngineDataLogger.CalcClutch(buffer(59) And &H10))
                                stringBuilder.Append(",")

                                stringBuilder.Append(K8EngineDataLogger.CalcNeutral(buffer(60) And 2))
                                stringBuilder.Append(",")

                                stringBuilder.Append(K8EngineDataLogger.CalcBoost(buffer(22)))
                                stringBuilder.Append(",")

                                stringBuilder.Append(K8EngineDataLogger.CalcTemp(buffer(28)))
                                stringBuilder.Append(",")

                                stringBuilder.Append(K8EngineDataLogger.CalcTemp(buffer(29)))
                                stringBuilder.Append(",")

                                stringBuilder.Append(K8EngineDataLogger.CalcPair(buffer(58) And 1))
                                stringBuilder.Append(",")

                                stringBuilder.Append(buffer(38) * 256 + buffer(39))
                                stringBuilder.Append(",")

                                stringBuilder.Append(buffer(40) * 256 + buffer(41))
                                stringBuilder.Append(",")

                                stringBuilder.Append(buffer(42) * 256 + buffer(43))
                                stringBuilder.Append(",")

                                stringBuilder.Append(buffer(44) * 256 + buffer(45))

                                writer.WriteLine(stringBuilder.ToString())

                            Catch ex As Exception
                                Dim message As String = ex.Message
                            End Try

                        End While
                    End If

                    writer.Close()
                    reader.Close()

                Next
            End If

            completed = True

        Catch ex As Exception
            MessageBox.Show(ex.Message, ".wrl Convert", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        Cursor.Current = Cursors.Default

        If completed = True Then
            MessageBox.Show(".wrl files converted to .csv files", ".Wrl Convert", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If

    End Sub

    Private Sub K11US15J10ToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles K11US15J10ToolStripMenuItem.Click

        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()

        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\DJF4SE10.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        SetECUType()

        MsgBox("A new Gixxer basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True

    End Sub

    Private Sub K11US14J10ToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles K11US14J10ToolStripMenuItem.Click

        Dim defpath As String ' this is for this subroutine only

        CloseChildWindows()

        ' OK, so the file is found, now lets start processing it
        defpath = My.Application.Info.DirectoryPath & "\ecu.bin\DJE4SE10.bin"

        L_File.Text = ""
        L_Comparefile.Text = ""
        DisableButtons()

        ' Open the stream and read it to global variable "Flash". 
        fs = File.OpenRead(defpath)
        Dim b(1) As Byte
        Dim i As Integer
        i = 0
        Do While fs.Read(b, 0, 1) > 0
            Flash(i) = b(0)
            FlashCopy(i) = b(0)
            i = i + 1
        Loop
        fs.Close()

        ' Check that the binary lenght matches expected ecu
        If i <> (262144 * 4) Then
            ECUNotSupported.ShowDialog()
        End If

        ECUVersion = "gixxer"
        '
        ' Make sure the ECU id is supported type
        '
        i = 0
        ECUID.Text = ""
        Do While i < 8
            ECUID.Text = ECUID.Text & Chr(Flash(&HFFFF0 + i))
            i = i + 1
        Loop

        ' check the ecu id bytes and validate that the ecu flash image is supported
        SetECUType()

        MsgBox("A new Gixxer basemap is generated", MsgBoxStyle.Information)

        BlockPgm = True

    End Sub

End Class


